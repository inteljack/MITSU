using Mitsu.Core.Domain;
using Mitsu.Core.Production;
using Mitsu.Printer;
using Mitsu.Printer.Simulator;

namespace Mitsu.Core.Tests;

/// <summary>Production runs against the simulator over TCP. Triggers stand in for cartons passing the photo-eye.</summary>
public class ProductionControllerTests
{
    private static readonly ProductionOptions Fast = new()
    {
        BufferTarget = 3,
        PollInterval = TimeSpan.FromMilliseconds(30),
        AlarmCheckEveryPolls = 3,
    };

    private sealed class Rig : IAsyncDisposable
    {
        public required X1Simulator Sim { get; init; }
        public required X1Client Client { get; init; }

        public static async Task<Rig> StartAsync(X1SimulatorOptions? o = null)
        {
            var sim = new X1Simulator(o);
            sim.Start();
            var client = new X1Client(new X1ClientOptions
            {
                Host = "127.0.0.1",
                Port = sim.Port,
                RequestTimeout = TimeSpan.FromSeconds(1),
                HeartbeatInterval = TimeSpan.Zero,
            });
            await client.ConnectAsync();
            return new Rig { Sim = sim, Client = client };
        }

        public PrintTarget Target(ushort station = 1, string name = "表面") =>
            new(name, Client, 1, station, "MSG00003.xml");

        public async ValueTask DisposeAsync()
        {
            await Client.DisposeAsync();
            await Sim.DisposeAsync();
        }
    }

    private static async Task WaitUntil(Func<bool> cond, int timeoutMs = 3000)
    {
        var end = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!cond())
        {
            if (DateTime.UtcNow > end) throw new TimeoutException("Condition not reached.");
            await Task.Delay(10);
        }
    }

    [Fact]
    public async Task Full_run_prints_every_carton_once_in_order_and_completes()
    {
        await using var rig = await Rig.StartAsync();
        var ctl = new ProductionController(new[] { rig.Target() }, Fast);
        var printed = new List<CartonRecord>();
        ctl.CartonPrinted += printed.Add;

        var run = ctl.RunAsync(TestData.Job(boxesDone: 2, total: 8, serial: "0098"));
        await WaitUntil(() => ctl.State == ProductionState.Running);

        for (int i = 0; i < 6; i++)
        {
            await WaitUntil(() => rig.Sim.BufferCount(1, 1) > 0); // controller keeps the buffer topped up
            Assert.NotNull(rig.Sim.Trigger(1, 1));
        }

        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(ProductionState.Completed, result.State);
        Assert.False(rig.Sim.IsPrinting(1));

        var onPrinter = rig.Sim.PrintedItems.Select(p => p.Values[(int)PayloadField.MachineNo]).ToList();
        Assert.Equal(new[] { "210098", "210099", "210100", "210101", "210102", "210103" }, onPrinter);
        Assert.All(rig.Sim.PrintedItems, p => Assert.True(p.FromBuffer));
        Assert.Equal(new[] { 3, 4, 5, 6, 7, 8 }, printed.Select(c => c.BoxNo));
        Assert.Equal((8, "0104"), result.ResumePoint);
    }

    [Fact]
    public async Task Operator_stop_voids_unprinted_cartons_and_gives_resume_point()
    {
        await using var rig = await Rig.StartAsync();
        var ctl = new ProductionController(new[] { rig.Target() }, Fast);
        using var stop = new CancellationTokenSource();

        var run = ctl.RunAsync(TestData.Job(total: 20, serial: "0001"), stop.Token);
        await WaitUntil(() => ctl.State == ProductionState.Running);
        rig.Sim.Trigger(1, 1);
        rig.Sim.Trigger(1, 1);
        await WaitUntil(() => ctl.CurrentJob!.PrintedCount == 2);

        stop.Cancel();
        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(ProductionState.Stopped, result.State);
        Assert.False(rig.Sim.IsPrinting(1));
        Assert.Equal(0, rig.Sim.BufferCount(1, 1));
        Assert.Equal(2, result.PrintedCount);
        Assert.Empty(result.Uncertain);
        Assert.All(result.Job.Cartons.Skip(2), c => Assert.Equal(CartonState.Voided, c.State));
        Assert.Equal((2, "0003"), result.ResumePoint); // next run starts at box 3, serial 0003 — no skip, no repeat
    }

    [Fact]
    public async Task Printing_faster_than_refill_is_detected_as_underrun()
    {
        await using var rig = await Rig.StartAsync();
        var ctl = new ProductionController(new[] { rig.Target() },
            Fast with { BufferTarget = 1, PollInterval = TimeSpan.FromMilliseconds(150) });

        var run = ctl.RunAsync(TestData.Job(total: 10));
        await WaitUntil(() => ctl.State == ProductionState.Running); // baseline taken
        rig.Sim.Trigger(1, 1); // consumes the only queued carton
        rig.Sim.Trigger(1, 1); // buffer empty -> simulator prints blank

        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(ProductionState.Faulted, result.State);
        Assert.Contains("計數器", result.FaultReason);
        Assert.False(rig.Sim.IsPrinting(1));
    }

    [Fact]
    public async Task Front_back_mismatch_stops_the_line()
    {
        await using var rig = await Rig.StartAsync();
        var ctl = new ProductionController(
            new[] { rig.Target(1, "表面"), rig.Target(2, "裏面") },
            Fast with { MaxTargetSkew = 1 });

        var run = ctl.RunAsync(TestData.Job(total: 10));
        await WaitUntil(() => ctl.State == ProductionState.Running);
        rig.Sim.Trigger(1, 1);
        rig.Sim.Trigger(1, 2);
        rig.Sim.Trigger(1, 1);
        rig.Sim.Trigger(1, 1); // front is 2 ahead of back

        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(ProductionState.Faulted, result.State);
        Assert.Contains("正反面", result.FaultReason);
        Assert.Equal(1, result.PrintedCount);
        // cartons printed on the front only are flagged for the operator, not silently skipped
        Assert.Equal(2, result.Uncertain.Count);
        Assert.Equal((1, "678569137"), result.ResumePoint);
    }

    [Fact]
    public async Task Ink_alarm_before_start_faults_without_printing()
    {
        await using var rig = await Rig.StartAsync();
        rig.Sim.SetAlarm(Printer.Protocol.InkAlarm.InkEmpty, Printer.Protocol.InkAlarm.NoError);
        var ctl = new ProductionController(new[] { rig.Target() }, Fast);

        var result = await ctl.RunAsync(TestData.Job());
        Assert.Equal(ProductionState.Faulted, result.State);
        Assert.Contains("警報", result.FaultReason);
        Assert.Equal(0, result.Job.SentCount);
    }

    [Fact]
    public async Task Alarm_during_run_stops_the_line()
    {
        await using var rig = await Rig.StartAsync();
        var ctl = new ProductionController(new[] { rig.Target() }, Fast);
        var run = ctl.RunAsync(TestData.Job(total: 10));
        await WaitUntil(() => ctl.State == ProductionState.Running);
        rig.Sim.Trigger(1, 1);
        rig.Sim.SetAlarm(Printer.Protocol.InkAlarm.InkLow, Printer.Protocol.InkAlarm.NoError);

        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(ProductionState.Faulted, result.State);
        Assert.False(rig.Sim.IsPrinting(1));
        Assert.Equal(1, result.PrintedCount);
        Assert.Empty(result.Uncertain);
    }

    [Fact]
    public async Task Lost_connection_marks_in_flight_cartons_uncertain()
    {
        await using var rig = await Rig.StartAsync();
        var ctl = new ProductionController(new[] { rig.Target() }, Fast);
        var run = ctl.RunAsync(TestData.Job(total: 10));
        await WaitUntil(() => ctl.State == ProductionState.Running);
        rig.Sim.Trigger(1, 1);
        await WaitUntil(() => ctl.CurrentJob!.PrintedCount == 1);

        rig.Sim.DisconnectClients();
        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(ProductionState.Faulted, result.State);
        Assert.Equal(1, result.PrintedCount);
        Assert.Equal(3, result.Uncertain.Count); // buffer contents unknown after the drop
        Assert.True(result.Job.HasUncertain);
    }

    [Fact]
    public async Task Printer_rejection_at_start_is_reported()
    {
        // no message selected is impossible here, so force a NAK by exceeding the printer buffer depth
        await using var rig = await Rig.StartAsync(new X1SimulatorOptions { BufferCapacity = 2 });
        var ctl = new ProductionController(new[] { rig.Target() }, Fast with { BufferTarget = 3 });

        var result = await ctl.RunAsync(TestData.Job());
        Assert.Equal(ProductionState.Faulted, result.State);
        Assert.IsType<X1NakException>(result.Error);
        Assert.False(rig.Sim.IsPrinting(1));
    }

    [Fact]
    public async Task Only_one_job_at_a_time()
    {
        await using var rig = await Rig.StartAsync();
        var ctl = new ProductionController(new[] { rig.Target() }, Fast);
        using var stop = new CancellationTokenSource();
        var run = ctl.RunAsync(TestData.Job(total: 10), stop.Token);
        await WaitUntil(() => ctl.State == ProductionState.Running);

        await Assert.ThrowsAsync<InvalidOperationException>(() => ctl.RunAsync(TestData.Job()));
        stop.Cancel();
        Assert.Equal(ProductionState.Stopped, (await run).State);
    }
}
