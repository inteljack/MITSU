using System.Numerics;

namespace Mitsu.Core.Domain;

public enum SerialDirection
{
    /// <summary>按順序噴印: serial +1 per carton.</summary>
    Ascending,
    /// <summary>按逆序噴印: serial −1 per carton.</summary>
    Descending,
}

/// <summary>
/// 機番 = prefix + serial. The serial is kept as a digit string with a fixed width so leading
/// zeros survive (the legacy C++Builder build stored it as a double and lost them, see
/// analysis/03 §7.1). Examples: "B-" + "10643301", "21" + "678569136".
/// </summary>
public sealed class MachineNumberSequence
{
    public MachineNumberSequence(string prefix, string startSerial, SerialDirection direction)
    {
        Prefix = prefix ?? "";
        StartSerial = startSerial ?? "";
        Direction = direction;
        Width = StartSerial.Length;
        _start = IsDigits(StartSerial) ? BigInteger.Parse(StartSerial) : BigInteger.Zero;
    }

    private readonly BigInteger _start;

    public string Prefix { get; }
    public string StartSerial { get; }
    public SerialDirection Direction { get; }

    /// <summary>Number of serial digits; every generated serial keeps this width.</summary>
    public int Width { get; }

    /// <summary>
    /// Validates that <paramref name="count"/> cartons can be numbered: digits only, and the
    /// sequence must neither go below zero (descending) nor need more digits than the start
    /// value (ascending, e.g. 999 → 1000 would change the barcode length).
    /// This replaces the legacy "總箱數 ≥ 流水號" rule that 20160530 removed.
    /// </summary>
    public ValidationResult Validate(long count)
    {
        var r = new ValidationResult();
        if (Prefix.Length == 0)
            r.Add("機番前綴", "機番前綴不可為空");
        if (StartSerial.Length == 0)
            r.Add("機番流水號", "機番流水號不可為空");
        else if (!IsDigits(StartSerial))
            r.Add("機番流水號", "機番流水號只能是數字");
        if (!r.IsValid || count <= 0) return r;

        var last = ValueAt(count - 1);
        if (last < 0)
            r.Add("機番流水號", $"逆序噴印 {count} 箱會使流水號小於 0（起始 {StartSerial}）");
        else if (last.ToString().Length > Width)
            r.Add("機番流水號", $"順序噴印 {count} 箱會超出 {Width} 位數（起始 {StartSerial}）");
        return r;
    }

    /// <summary>Serial (digits only, zero-padded) of the carton at 0-based offset from the start.</summary>
    public string SerialAt(long offset)
    {
        var v = ValueAt(offset);
        if (v < 0) throw new InvalidOperationException($"Serial below zero at offset {offset}.");
        var s = v.ToString();
        if (s.Length > Width) throw new InvalidOperationException($"Serial {s} exceeds {Width} digits.");
        return s.PadLeft(Width, '0');
    }

    /// <summary>Full machine number (prefix + serial) at 0-based offset.</summary>
    public string MachineNoAt(long offset) => Prefix + SerialAt(offset);

    /// <summary>A sequence that continues after <paramref name="consumed"/> cartons (for resuming a job).</summary>
    public MachineNumberSequence Advance(long consumed) => new(Prefix, SerialAt(consumed), Direction);

    private BigInteger ValueAt(long offset) =>
        Direction == SerialDirection.Ascending ? _start + offset : _start - offset;

    private static bool IsDigits(string s) => s.Length > 0 && s.All(char.IsAsciiDigit);
}
