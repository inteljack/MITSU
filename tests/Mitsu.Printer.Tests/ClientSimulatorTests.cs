using Mitsu.Printer.Protocol;
using Mitsu.Printer.Simulator;

namespace Mitsu.Printer.Tests;

/// <summary>End-to-end: X1Client against the in-process simulator over real TCP.</summary>
public class ClientSimulatorTests
{
    private static async Task<(X1Simulator sim, X1Client client)> StartAsync(
        X1SimulatorOptions? simOptions = null, TimeSpan? requestTimeout = null, bool tids = false)
    {
        var sim = new X1Simulator(simOptions);
        sim.Start();
        var client = new X1Client(new X1ClientOptions
        {
            Host = "127.0.0.1",
            Port = sim.Port,
            RequestTimeout = requestTimeout ?? TimeSpan.FromSeconds(2),
            HeartbeatInterval = TimeSpan.Zero,
            UseTransactionIds = tids,
        });
        await client.ConnectAsync();
        return (sim, client);
    }

    [Fact]
    public async Task Basic_queries()
    {
        var (sim, c) = await StartAsync();
        await using var _ = sim;
        await using var __ = c;

        Assert.Equal(new ProtocolVersion(1, 0, 24), await c.GetProtocolVersionAsync());
        Assert.True((await c.GetAlarmStatusAsync()).IsOk);
        Assert.Equal("X1-SIMULATOR", await c.GetPrinterNameAsync());
        Assert.Empty((await c.GetPrintingStatusAsync()).Lines);
    }

    [Fact]
    public async Task Production_cycle_fifo_and_counters()
    {
        var (sim, c) = await StartAsync(new X1SimulatorOptions { BufferCapacity = 3 });
        await using var _ = sim;
        await using var __ = c;

        await c.StopLineAsync(1);
        await c.SelectMessageAsync(1, 1, "MSG00003.xml");
        await c.ClearDynamicVarBufferAsync(1, 1);
        await c.PushDynamicVarsAsync(1, 1, new[] { "MODEL-A", "21678569136" });
        await c.PushDynamicVarsAsync(1, 1, new[] { "MODEL-A", "21678569137" });
        Assert.Equal(2, await c.GetDynamicVarBufferCountAsync(1, 1));

        await c.StartLineAsync(1);
        Assert.True(sim.IsPrinting(1));

        var p1 = sim.Trigger(1, 1);
        Assert.NotNull(p1);
        Assert.Equal("21678569136", p1!.Values[1]);
        Assert.Equal(1, await c.GetDynamicVarBufferCountAsync(1, 1));

        var status = await c.GetPrintingStatusAsync();
        Assert.Single(status.Lines);
        Assert.Equal(1, status.Lines[0].ItemCounter);
        Assert.Equal(68, status.BlockSize);

        var counter = await c.GetCounterAsync(3, CounterType.Item);
        Assert.Equal(1, counter.Value);

        Assert.Equal("21678569137", sim.Trigger(1, 1)!.Values[1]);
        var blank = sim.Trigger(1, 1)!; // buffer empty -> blank print (default behaviour)
        Assert.False(blank.FromBuffer);

        await c.StopLineAsync(1);
        Assert.False(sim.IsPrinting(1));
        Assert.Empty((await c.GetPrintingStatusAsync()).Lines);
    }

    [Fact]
    public async Task Nak_is_an_exception_and_client_stays_usable()
    {
        var (sim, c) = await StartAsync(new X1SimulatorOptions { BufferCapacity = 1 });
        await using var _ = sim;
        await using var __ = c;

        // start without selected message -> 11h
        var ex = await Assert.ThrowsAsync<X1NakException>(() => c.StartLineAsync(1));
        Assert.Equal(X1ErrorCodes.SelectMessageBeforePrint, ex.ErrorCode);
        Assert.Equal((ushort)0x0203, ex.SubCommand);

        // CF without message -> 11h
        await Assert.ThrowsAsync<X1NakException>(() => c.PushDynamicVarsAsync(1, 1, new[] { "x" }));

        await c.SelectMessageAsync(1, 1, "MSG00001.xml");
        await c.PushDynamicVarsAsync(1, 1, new[] { "a" });
        var full = await Assert.ThrowsAsync<X1NakException>(() => c.PushDynamicVarsAsync(1, 1, new[] { "b" }));
        Assert.Equal(X1ErrorCodes.ParameterOutOfRange, full.ErrorCode);

        sim.SetAlarm(InkAlarm.InkEmpty, InkAlarm.NoError);
        var ink = await Assert.ThrowsAsync<X1NakException>(() => c.StartLineAsync(1));
        Assert.Equal(X1ErrorCodes.InkNotReady, ink.ErrorCode);
        Assert.Equal(InkAlarm.InkEmpty, (await c.GetAlarmStatusAsync()).Head0);

        Assert.True(c.IsConnected);
    }

    [Fact]
    public async Task Invalid_arguments_fail_before_sending_and_do_not_block_queue()
    {
        var (sim, c) = await StartAsync();
        await using var _ = sim;
        await using var __ = c;

        await Assert.ThrowsAsync<ArgumentException>(() => c.PushDynamicVarsAsync(1, 1, Enumerable.Repeat("x", 21).ToList()));
        await Assert.ThrowsAsync<ArgumentException>(() => c.SelectMessageAsync(1, 1, new string('A', 300)));
        Assert.Equal(new ProtocolVersion(1, 0, 24), await c.GetProtocolVersionAsync());
    }

    [Fact]
    public async Task Lost_reply_times_out_and_disconnects_instead_of_hanging()
    {
        var (sim, c) = await StartAsync(requestTimeout: TimeSpan.FromMilliseconds(300));
        await using var _ = sim;
        await using var __ = c;

        var disconnected = new TaskCompletionSource<string>();
        c.Disconnected += (_, e) => disconnected.TrySetResult(e.Reason);

        sim.DropNextResponses(1);
        await Assert.ThrowsAsync<X1TimeoutException>(() => c.GetProtocolVersionAsync());
        Assert.False(c.IsConnected);
        Assert.Contains("No reply", await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(2)));

        await Assert.ThrowsAsync<X1ConnectionException>(() => c.GetProtocolVersionAsync());

        // caller reconnects explicitly
        await c.ConnectAsync();
        Assert.Equal(new ProtocolVersion(1, 0, 24), await c.GetProtocolVersionAsync());
    }

    [Fact]
    public async Task Byte_by_byte_replies_are_reassembled()
    {
        var (sim, c) = await StartAsync();
        await using var _ = sim;
        await using var __ = c;

        sim.SplitResponses = true;
        await c.SelectMessageAsync(1, 1, "MSG00003.xml");
        await c.PushDynamicVarsAsync(1, 1, new[] { "x" });
        Assert.Equal(1, await c.GetDynamicVarBufferCountAsync(1, 1));
        Assert.Equal(new ProtocolVersion(1, 0, 24), await c.GetProtocolVersionAsync());
    }

    [Fact]
    public async Task Unsolicited_frame_does_not_break_pairing()
    {
        var (sim, c) = await StartAsync();
        await using var _ = sim;
        await using var __ = c;

        var got = new TaskCompletionSource<AnserFrame>();
        c.UnsolicitedFrame += (_, e) => got.TrySetResult(e.Frame);

        sim.ResponseDelay = TimeSpan.FromMilliseconds(200);
        var pending = c.GetProtocolVersionAsync();
        await Task.Delay(50);
        await sim.SendUnsolicitedAsync(AnserCommand.PrintCompletedReport, new byte[] { 0, 1 });

        Assert.Equal(new ProtocolVersion(1, 0, 24), await pending);
        Assert.Equal(AnserCommand.PrintCompletedReport, (await got.Task.WaitAsync(TimeSpan.FromSeconds(2))).Command);
    }

    [Fact]
    public async Task Concurrent_callers_are_serialised()
    {
        var (sim, c) = await StartAsync(new X1SimulatorOptions { BufferCapacity = 100 });
        await using var _ = sim;
        await using var __ = c;

        await c.SelectMessageAsync(1, 1, "MSG00003.xml");
        var tasks = Enumerable.Range(0, 50).Select(i => i % 2 == 0
            ? c.PushDynamicVarsAsync(1, 1, new[] { i.ToString() })
            : c.GetProtocolVersionAsync());
        await Task.WhenAll(tasks);
        Assert.Equal(25, await c.GetDynamicVarBufferCountAsync(1, 1));
    }

    [Fact]
    public async Task Server_disconnect_fails_pending_request_and_raises_event()
    {
        var (sim, c) = await StartAsync(requestTimeout: TimeSpan.FromSeconds(5));
        await using var _ = sim;
        await using var __ = c;

        var disconnected = new TaskCompletionSource<bool>();
        c.Disconnected += (_, _) => disconnected.TrySetResult(true);

        sim.ResponseDelay = TimeSpan.FromSeconds(2);
        var pending = c.GetProtocolVersionAsync();
        await Task.Delay(100);
        sim.DisconnectClients();

        await Assert.ThrowsAsync<X1ConnectionException>(() => pending);
        Assert.True(await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(2)));
        Assert.False(c.IsConnected);
    }

    [Fact]
    public async Task Transaction_ids_when_enabled()
    {
        var (sim, c) = await StartAsync(tids: true);
        await using var _ = sim;
        await using var __ = c;

        var sent = new List<byte[]>();
        c.Traffic += (_, e) => { if (e.Direction == TrafficDirection.Sent) lock (sent) sent.Add(e.Bytes); };
        await c.GetProtocolVersionAsync();
        await c.GetProtocolVersionAsync();
        lock (sent)
        {
            Assert.Equal(1, sent[0][1]);
            Assert.Equal(2, sent[1][1]);
        }
    }

    [Fact]
    public async Task Heartbeat_detects_dead_printer()
    {
        var sim = new X1Simulator();
        sim.Start();
        await using var _ = sim;
        await using var c = new X1Client(new X1ClientOptions
        {
            Host = "127.0.0.1",
            Port = sim.Port,
            RequestTimeout = TimeSpan.FromMilliseconds(300),
            HeartbeatInterval = TimeSpan.FromMilliseconds(300),
        });
        var disconnected = new TaskCompletionSource<bool>();
        c.Disconnected += (_, _) => disconnected.TrySetResult(true);
        await c.ConnectAsync();

        sim.DropNextResponses(int.MaxValue);
        Assert.True(await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task Connect_to_closed_port_fails_cleanly()
    {
        await using var c = new X1Client(new X1ClientOptions
        {
            Host = "127.0.0.1",
            Port = 1, // nothing listening
            ConnectTimeout = TimeSpan.FromSeconds(2),
        });
        await Assert.ThrowsAsync<X1ConnectionException>(() => c.ConnectAsync());
        Assert.False(c.IsConnected);
    }
}
