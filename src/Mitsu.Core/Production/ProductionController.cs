using Mitsu.Core.Domain;
using Mitsu.Printer;

namespace Mitsu.Core.Production;

/// <summary>
/// Where one side of the carton is printed: a printer connection plus production line / print
/// station and the X1 message (layout) to select. A job prints the same values on every target
/// (e.g. 表面 and 裏面); a carton counts as printed only when every target confirmed it.
/// </summary>
public sealed record PrintTarget(string Name, IX1Printer Printer, ushort Line, ushort Station, string MessageFile);

public sealed record ProductionOptions
{
    /// <summary>Entries kept queued in each printer buffer (replaces PrintMaxMem). Must not exceed the CFh buffer depth.</summary>
    public int BufferTarget { get; init; } = 3;

    public TimeSpan PollInterval { get; init; } = TimeSpan.FromMilliseconds(200);

    /// <summary>Check 7Fh alarms every N polls.</summary>
    public int AlarmCheckEveryPolls { get; init; } = 10;

    /// <summary>
    /// Compare the 45h item counter with buffer consumption. A counter running ahead of consumption
    /// means items were printed without data (buffer underrun) → stop. Only applied to lines that
    /// carry a single target.
    /// </summary>
    public bool CounterCrossCheck { get; init; } = true;

    /// <summary>Consecutive polls the counter must run ahead before it is treated as an underrun (absorbs read timing).</summary>
    public int UnderrunConfirmPolls { get; init; } = 2;

    /// <summary>Max difference in printed cartons between targets before stopping (front/back mismatch).</summary>
    public int MaxTargetSkew { get; init; } = 3;
}

public enum ProductionState { Idle, Starting, Running, Stopping, Completed, Stopped, Faulted }

public sealed record ProductionResult(
    ProductionState State,
    ProductionJob Job,
    string? FaultReason,
    Exception? Error)
{
    public int PrintedCount => Job.PrintedCount;
    public (int BoxesDone, string NextSerial) ResumePoint => Job.ResumePoint;
    public IReadOnlyList<CartonRecord> Uncertain => Job.Cartons.Where(c => c.State == CartonState.Uncertain).ToList();
}

public sealed class ProductionFaultException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>
/// Runs one print job against one or more print targets. Everything happens in one async loop
/// (no shared counters between threads):
/// <code>
/// start : alarms ok → stop lines → select message → clear buffer → prefill N cartons → start lines
/// loop  : every PollInterval read CCh per target → consumed = sent − queued → confirm FIFO
///         cross-check 45h item counter (underrun) · front/back skew · alarms → refill to N
/// end   : all confirmed → stop lines → Completed
/// stop  : (token) stop lines → final count → clear buffers → unprinted = Voided
/// fault : best-effort stop; in-flight cartons Voided only if the stop+clear was confirmed, else Uncertain
/// </code>
/// Box/serial advance only from printer-confirmed consumption, never from what was sent.
/// The meaning of CCh (entries still queued) is unverified on hardware (P0); everything that
/// depends on it is isolated in <see cref="PollTargetsAsync"/>.
/// </summary>
public sealed class ProductionController
{
    private readonly IReadOnlyList<PrintTarget> _targets;
    private readonly ProductionOptions _o;
    private readonly CartonPayloadComposer _composer;
    private readonly FieldLimits? _limits;
    private readonly TimeProvider _time;

    private ProductionState _state = ProductionState.Idle;
    private int _running;

    public ProductionController(
        IReadOnlyList<PrintTarget> targets,
        ProductionOptions? options = null,
        CartonPayloadComposer? composer = null,
        FieldLimits? limits = null,
        TimeProvider? time = null)
    {
        if (targets == null || targets.Count == 0) throw new ArgumentException("At least one print target is required.");
        if (targets.Select(t => (t.Printer, t.Line, t.Station)).Distinct().Count() != targets.Count)
            throw new ArgumentException("Duplicate print target (same printer/line/station).");
        _targets = targets;
        _o = options ?? new ProductionOptions();
        if (_o.BufferTarget < 1) throw new ArgumentException("BufferTarget must be ≥ 1.");
        _composer = composer ?? new CartonPayloadComposer();
        _limits = limits;
        _time = time ?? TimeProvider.System;
    }

    public ProductionState State => _state;
    public ProductionJob? CurrentJob { get; private set; }

    public event Action<ProductionState>? StateChanged;
    public event Action<CartonRecord>? CartonSent;
    public event Action<CartonRecord>? CartonPrinted;

    /// <summary>Raised for operator-visible warnings that do not stop production.</summary>
    public event Action<string>? Warning;

    /// <summary>
    /// Runs the job until it completes, <paramref name="stopToken"/> is cancelled (operator pressed
    /// 停止) or a fault occurs. Never throws for printer/production faults — see the result.
    /// Throws <see cref="DomainValidationException"/> for invalid input before touching the printer.
    /// </summary>
    public async Task<ProductionResult> RunAsync(JobSpec spec, CancellationToken stopToken = default)
    {
        if (Interlocked.Exchange(ref _running, 1) == 1) throw new InvalidOperationException("A job is already running.");
        try
        {
            var job = new ProductionJob(spec, _targets.Count, _composer, _limits);
            CurrentJob = job;
            var run = new Run(job, _targets.Count);
            try
            {
                SetState(ProductionState.Starting);
                await StartAsync(run).ConfigureAwait(false);
                SetState(ProductionState.Running);
                await LoopAsync(run, stopToken).ConfigureAwait(false);

                if (job.IsComplete)
                {
                    await StopLinesAsync().ConfigureAwait(false);
                    SetState(ProductionState.Completed);
                    return new ProductionResult(ProductionState.Completed, job, null, null);
                }

                // operator stop
                SetState(ProductionState.Stopping);
                bool cleared = await StopAndSettleAsync(run).ConfigureAwait(false);
                job.CloseInFlight(cleared);
                SetState(ProductionState.Stopped);
                return new ProductionResult(ProductionState.Stopped, job, null, null);
            }
            catch (Exception ex) when (ex is ProductionFaultException or X1Exception or InvalidOperationException
                                           or Printer.Protocol.AnserProtocolException)
            {
                string reason = ex is X1NakException nak ? $"噴碼機拒絕指令：{nak.Message}" : ex.Message;
                SetState(ProductionState.Stopping);
                bool cleared = await TryStopAndSettleAsync(run).ConfigureAwait(false);
                job.CloseInFlight(cleared);
                SetState(ProductionState.Faulted);
                return new ProductionResult(ProductionState.Faulted, job, reason, ex);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
    }

    // ------------------------------------------------------------------ phases

    private async Task StartAsync(Run run)
    {
        foreach (var p in Printers())
        {
            var alarm = await p.GetAlarmStatusAsync().ConfigureAwait(false);
            if (!alarm.IsOk) throw new ProductionFaultException($"噴碼機警報：噴頭0={alarm.Head0}，噴頭1={alarm.Head1}");
        }

        await StopLinesAsync().ConfigureAwait(false);
        foreach (var t in _targets)
        {
            await t.Printer.SelectMessageAsync(t.Line, t.Station, t.MessageFile).ConfigureAwait(false);
            await t.Printer.ClearDynamicVarBufferAsync(t.Line, t.Station).ConfigureAwait(false);
        }

        await RefillAsync(run).ConfigureAwait(false);

        foreach (var (printer, line) in Lines())
            await printer.StartLineAsync(line).ConfigureAwait(false);

        // Fix the counter baseline immediately, before the first carton can reach the photo-eye.
        // Taking it later would silently absorb any blank prints that happened in between.
        if (_o.CounterCrossCheck)
        {
            foreach (var (t, i) in SingleTargetLines())
            {
                var line = await ReadLineStatusAsync(t).ConfigureAwait(false);
                run.CounterBaseline[i] = line.ItemCounter;
            }
        }
    }

    private async Task LoopAsync(Run run, CancellationToken stopToken)
    {
        int poll = 0;
        while (!run.Job.IsComplete)
        {
            try
            {
                await Task.Delay(_o.PollInterval, _time, stopToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return; // operator stop
            }

            await PollTargetsAsync(run).ConfigureAwait(false);
            if (run.Job.IsComplete) return;

            if (run.Job.Skew > _o.MaxTargetSkew)
                throw new ProductionFaultException(
                    $"正反面噴印數不一致：{string.Join("、", _targets.Select((t, i) => $"{t.Name}={run.Job.ConfirmedOn(i)}"))}");

            if (_o.CounterCrossCheck)
                await CheckCountersAsync(run).ConfigureAwait(false);

            if (_o.AlarmCheckEveryPolls > 0 && ++poll % _o.AlarmCheckEveryPolls == 0)
                await CheckAlarmsAsync().ConfigureAwait(false);

            await RefillAsync(run).ConfigureAwait(false);
        }
    }

    /// <summary>Reads each target's queued count and converts it into FIFO confirmations.</summary>
    private async Task PollTargetsAsync(Run run)
    {
        var now = _time.GetUtcNow();
        for (int i = 0; i < _targets.Count; i++)
        {
            var t = _targets[i];
            int queued = await t.Printer.GetDynamicVarBufferCountAsync(t.Line, t.Station).ConfigureAwait(false);
            int sent = run.Job.SentCount;
            int consumed = sent - queued;
            if (queued < 0 || consumed < run.Job.ConfirmedOn(i) || consumed > sent)
                throw new ProductionFaultException(
                    $"{t.Name} 緩衝筆數異常：已送 {sent}、緩衝 {queued}、已確認 {run.Job.ConfirmedOn(i)}");
            foreach (var c in run.Job.Confirm(i, consumed, now))
                CartonPrinted?.Invoke(c);
        }
    }

    private async Task CheckCountersAsync(Run run)
    {
        foreach (var (t, i) in SingleTargetLines())
        {
            var line = await ReadLineStatusAsync(t).ConfigureAwait(false);
            int consumed = run.Job.ConfirmedOn(i);
            run.CounterBaseline[i] ??= line.ItemCounter - consumed;
            long printedByCounter = line.ItemCounter - run.CounterBaseline[i]!.Value;

            if (printedByCounter > consumed)
            {
                if (++run.UnderrunStrikes[i] >= _o.UnderrunConfirmPolls)
                    throw new ProductionFaultException(
                        $"{t.Name}：計數器顯示已噴 {printedByCounter} 箱，但只有 {consumed} 箱有送資料（可能噴出空白或舊資料）");
            }
            else
            {
                run.UnderrunStrikes[i] = 0;
                if (printedByCounter < consumed - 1)
                    Warning?.Invoke($"{t.Name}：計數器 {printedByCounter} 落後於緩衝消耗 {consumed}");
            }
        }
    }

    private async Task CheckAlarmsAsync()
    {
        foreach (var p in Printers())
        {
            var a = await p.GetAlarmStatusAsync().ConfigureAwait(false);
            if (!a.IsOk) throw new ProductionFaultException($"噴碼機警報：噴頭0={a.Head0}，噴頭1={a.Head1}");
        }
    }

    /// <summary>Tops every buffer up to BufferTarget queued entries.</summary>
    private async Task RefillAsync(Run run)
    {
        var job = run.Job;
        while (!job.AllSent)
        {
            int minConfirmed = Enumerable.Range(0, _targets.Count).Min(job.ConfirmedOn);
            int maxQueued = job.SentCount - minConfirmed;
            if (maxQueued >= _o.BufferTarget) return;

            var carton = job.CreateNext(_time.GetUtcNow())!;
            foreach (var t in _targets)
                await t.Printer.PushDynamicVarsAsync(t.Line, t.Station, carton.Values).ConfigureAwait(false);
            CartonSent?.Invoke(carton);
        }
    }

    /// <summary>Stops lines, takes a final count, clears buffers. Returns true when all of it succeeded.</summary>
    private async Task<bool> StopAndSettleAsync(Run run)
    {
        await StopLinesAsync().ConfigureAwait(false);
        await PollTargetsAsync(run).ConfigureAwait(false);
        foreach (var t in _targets)
            await t.Printer.ClearDynamicVarBufferAsync(t.Line, t.Station).ConfigureAwait(false);
        return true;
    }

    private async Task<bool> TryStopAndSettleAsync(Run run)
    {
        // Stop every line we can reach, even if some fail.
        foreach (var (printer, line) in Lines())
        {
            try { if (printer.IsConnected) await printer.StopLineAsync(line).ConfigureAwait(false); }
            catch (Exception ex) when (ex is X1Exception or Printer.Protocol.AnserProtocolException) { }
        }
        try
        {
            if (_targets.Any(t => !t.Printer.IsConnected)) return false;
            return await StopAndSettleAsync(run).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is X1Exception or ProductionFaultException or InvalidOperationException
                                       or Printer.Protocol.AnserProtocolException)
        {
            return false;
        }
    }

    private async Task StopLinesAsync()
    {
        foreach (var (printer, line) in Lines())
            await printer.StopLineAsync(line).ConfigureAwait(false);
    }

    /// <summary>Targets that are alone on their (printer, line): the 45h counter is per line, so only these can be cross-checked.</summary>
    private IEnumerable<(PrintTarget, int)> SingleTargetLines() =>
        _targets.Select((t, i) => (t, i))
            .GroupBy(x => (x.t.Printer, x.t.Line))
            .Where(g => g.Count() == 1)
            .Select(g => g.Single());

    private static async Task<Printer.Protocol.LinePrintingStatus> ReadLineStatusAsync(PrintTarget t)
    {
        var status = await t.Printer.GetPrintingStatusAsync().ConfigureAwait(false);
        return status.Lines.FirstOrDefault(l => l.Line == t.Line)
               ?? throw new ProductionFaultException($"{t.Name}：產線 {t.Line} 不在噴印狀態");
    }

    private IEnumerable<IX1Printer> Printers() => _targets.Select(t => t.Printer).Distinct();

    private IEnumerable<(IX1Printer, ushort)> Lines() => _targets.Select(t => (t.Printer, t.Line)).Distinct();

    private void SetState(ProductionState s)
    {
        _state = s;
        StateChanged?.Invoke(s);
    }

    private sealed class Run(ProductionJob job, int targets)
    {
        public ProductionJob Job { get; } = job;
        public long?[] CounterBaseline { get; } = new long?[targets];
        public int[] UnderrunStrikes { get; } = new int[targets];
    }
}
