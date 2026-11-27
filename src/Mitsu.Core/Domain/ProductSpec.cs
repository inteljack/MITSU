namespace Mitsu.Core.Domain;

/// <summary>
/// One row of PrintData (噴印資料), keyed by Key code. Column names follow the customer's Excel
/// (analysis/04 §4.1). MODEL / BORE / OUTPUT / CODE hold the *label text* printed in front of the value.
/// </summary>
public sealed record ProductSpec
{
    public required string KeyCode { get; init; }
    public required string CaseNo { get; init; }
    public string Model { get; init; } = "";
    public string ModelValue { get; init; } = "";
    public string Bore { get; init; } = "";
    public string BoreValue { get; init; } = "";
    public string BoreUnit { get; init; } = "";
    public string Output { get; init; } = "";
    public string OutputValue { get; init; } = "";
    public string OutputUnit { get; init; } = "";
    public string PhaseValue { get; init; } = "";
    public string PhaseUnit { get; init; } = "";
    public string VoltageValue { get; init; } = "";
    public string VoltageUnit { get; init; } = "";
    public string HzValue { get; init; } = "";
    public string HzUnit { get; init; } = "";
    public string Code { get; init; } = "";
    public string CodeValue { get; init; } = "";
    public string Tokki { get; init; } = "";
    public string Place { get; init; } = "";
}

/// <summary>One row of CartonSize (紙箱規格). All lengths in mm.</summary>
public sealed record CartonSpec
{
    public required string CaseNo { get; init; }
    public decimal Length { get; init; }
    public decimal Width { get; init; }
    public decimal Height { get; init; }

    /// <summary>印字高度.</summary>
    public decimal PrintHeight { get; init; }

    /// <summary>表面印字開始 / 終了 (front side, measured along L then W).</summary>
    public decimal OutsideStart { get; init; }
    public decimal OutsideEnd { get; init; }

    /// <summary>裏面印字開始 / 終了 (back side, printed in the opposite direction).</summary>
    public decimal InsideStart { get; init; }
    public decimal InsideEnd { get; init; }
}
