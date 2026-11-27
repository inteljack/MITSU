using Mitsu.Core.Domain;

namespace Mitsu.Core.Production;

public enum CartonState
{
    /// <summary>Values sent to the printer buffer(s), not yet confirmed printed on every side.</summary>
    Sent,
    /// <summary>Confirmed printed on every print target.</summary>
    Printed,
    /// <summary>Sent but certainly not printed (buffer cleared after a confirmed stop); number can be reused.</summary>
    Voided,
    /// <summary>
    /// Outcome unknown (link lost while in flight, or printed on only one side). Needs an operator
    /// decision — this is exactly the 跳號 / 正反面不一致 case of 2013–2015 (analysis/04 §5).
    /// </summary>
    Uncertain,
}

/// <summary>One carton of a run. Its box number and machine number are fixed when it is created.</summary>
public sealed class CartonRecord
{
    internal CartonRecord(int sequence, int boxNo, string serial, string machineNo, string[] values, int targetCount)
    {
        Sequence = sequence;
        BoxNo = boxNo;
        Serial = serial;
        MachineNo = machineNo;
        Values = values;
        PrintedOn = new DateTimeOffset?[targetCount];
    }

    /// <summary>0-based position within this run.</summary>
    public int Sequence { get; }

    /// <summary>1-based box number shown to the operator (BoxesDone + Sequence + 1).</summary>
    public int BoxNo { get; }

    public string Serial { get; }
    public string MachineNo { get; }

    /// <summary>The 20 strings sent with CFh.</summary>
    public IReadOnlyList<string> Values { get; }

    public CartonState State { get; internal set; } = CartonState.Sent;
    public DateTimeOffset? SentAt { get; internal set; }

    /// <summary>Confirmation time per print target (index = target index).</summary>
    public DateTimeOffset?[] PrintedOn { get; }

    public DateTimeOffset? PrintedAt => State == CartonState.Printed ? PrintedOn.Max() : null;

    public override string ToString() => $"#{BoxNo} {MachineNo} {State}";
}

/// <summary>
/// Bookkeeping for one run: which cartons were sent and which are confirmed per print target.
/// Pure state, no I/O and no threads — the controller drives it from a single loop, which removes
/// the counter races that caused the 2013 "buffer pages drop to 0" bug (analysis/04 #25).
/// Confirmation is FIFO per target: a target having consumed n entries means cartons 0..n-1 were
/// printed on that target.
/// </summary>
public sealed class ProductionJob
{
    private readonly List<CartonRecord> _cartons = new();
    private readonly int[] _confirmed;
    private readonly string[] _staticValues;
    private readonly MachineNumberSequence _sequence;
    private readonly CartonPayloadComposer _composer;

    public ProductionJob(JobSpec spec, int targetCount, CartonPayloadComposer? composer = null, FieldLimits? limits = null)
    {
        if (targetCount <= 0) throw new ArgumentOutOfRangeException(nameof(targetCount));
        spec.Validate(limits).ThrowIfInvalid();
        Spec = spec;
        TargetCount = targetCount;
        _confirmed = new int[targetCount];
        _composer = composer ?? new CartonPayloadComposer();
        _sequence = spec.Sequence;
        _staticValues = _composer.ComposeStatic(spec.Product, spec.MadeNo);
    }

    public JobSpec Spec { get; }
    public int TargetCount { get; }
    public int Total => Spec.RemainingBoxes;
    public int SentCount => _cartons.Count;
    public IReadOnlyList<CartonRecord> Cartons => _cartons;

    /// <summary>Cartons confirmed on every target.</summary>
    public int PrintedCount => _confirmed.Min();

    public int ConfirmedOn(int target) => _confirmed[target];

    public bool AllSent => SentCount >= Total;
    public bool IsComplete => PrintedCount >= Total;

    /// <summary>Largest difference in confirmed count between targets (front/back skew).</summary>
    public int Skew => _confirmed.Max() - _confirmed.Min();

    /// <summary>Creates the next carton (machine number assigned now). Null when all are sent.</summary>
    public CartonRecord? CreateNext(DateTimeOffset now)
    {
        if (AllSent) return null;
        int seq = _cartons.Count;
        string serial = _sequence.SerialAt(seq);
        string machineNo = _sequence.Prefix + serial;
        var values = _composer.Compose(_staticValues, Spec.Product, machineNo);
        var c = new CartonRecord(seq, Spec.BoxesDone + seq + 1, serial, machineNo, values, TargetCount) { SentAt = now };
        _cartons.Add(c);
        return c;
    }

    /// <summary>
    /// Records that <paramref name="target"/> has consumed <paramref name="consumed"/> entries in total.
    /// Returns cartons that became fully printed. Throws if the count goes backwards or beyond what was sent.
    /// </summary>
    public IReadOnlyList<CartonRecord> Confirm(int target, int consumed, DateTimeOffset now)
    {
        if (consumed < _confirmed[target])
            throw new InvalidOperationException($"Target {target}: consumed count went back from {_confirmed[target]} to {consumed}.");
        if (consumed > SentCount)
            throw new InvalidOperationException($"Target {target}: consumed {consumed} but only {SentCount} sent.");

        int before = PrintedCount;
        for (int i = _confirmed[target]; i < consumed; i++)
            _cartons[i].PrintedOn[target] = now;
        _confirmed[target] = consumed;

        int after = PrintedCount;
        var newly = new List<CartonRecord>();
        for (int i = before; i < after; i++)
        {
            _cartons[i].State = CartonState.Printed;
            newly.Add(_cartons[i]);
        }
        return newly;
    }

    /// <summary>
    /// Closes out cartons still in flight. <paramref name="buffersCleared"/>: the printer confirmed the
    /// line stopped and the buffers were cleared, so cartons printed on no target are certainly not
    /// printed (Voided); partially printed ones stay Uncertain. Without that confirmation, every
    /// in-flight carton is Uncertain.
    /// </summary>
    public IReadOnlyList<CartonRecord> CloseInFlight(bool buffersCleared)
    {
        var touched = new List<CartonRecord>();
        foreach (var c in _cartons.Where(c => c.State == CartonState.Sent))
        {
            bool printedSomewhere = c.PrintedOn.Any(t => t != null);
            c.State = buffersCleared && !printedSomewhere ? CartonState.Voided : CartonState.Uncertain;
            touched.Add(c);
        }
        return touched;
    }

    /// <summary>
    /// Values to resume with after this run (箱數 and the next 機番流水號), based on cartons confirmed on
    /// every target. If <see cref="CartonState.Uncertain"/> cartons exist the operator must decide
    /// first (one side may already carry the next number). NextSerial is empty when the sequence
    /// cannot continue (e.g. 0000 reached in descending mode).
    /// </summary>
    public (int BoxesDone, string NextSerial) ResumePoint
    {
        get
        {
            int printed = PrintedCount;
            string next;
            try { next = _sequence.SerialAt(printed); }
            catch (InvalidOperationException) { next = ""; }
            return (Spec.BoxesDone + printed, next);
        }
    }

    public bool HasUncertain => _cartons.Any(c => c.State == CartonState.Uncertain);
}
