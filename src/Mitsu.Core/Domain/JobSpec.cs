namespace Mitsu.Core.Domain;

/// <summary>
/// Maximum character lengths per PrintData column (legacy Config.xml PrintDataLen_* defaults,
/// analysis/02 §5.1). Over-long values are rejected when a job starts.
/// </summary>
public sealed record FieldLimits
{
    public int KeyCode { get; init; } = 50;
    public int CaseNo { get; init; } = 12;
    public int Model { get; init; } = 5;
    public int ModelValue { get; init; } = 21;
    public int Bore { get; init; } = 4;
    public int BoreValue { get; init; } = 5;
    public int BoreUnit { get; init; } = 4;
    public int Output { get; init; } = 6;
    public int OutputValue { get; init; } = 4;
    public int OutputUnit { get; init; } = 2;
    public int PhaseValue { get; init; } = 1;
    public int PhaseUnit { get; init; } = 2;
    public int VoltageValue { get; init; } = 11;
    public int VoltageUnit { get; init; } = 1;
    public int HzValue { get; init; } = 2;
    public int HzUnit { get; init; } = 2;
    public int Code { get; init; } = 4;
    public int CodeValue { get; init; } = 12;
    public int Tokki { get; init; } = 25;
    public int Place { get; init; } = 25;
    public int MadeNo { get; init; } = 15;
    public int MachineNoPrefix { get; init; } = 12;
    public int MachineNoSerial { get; init; } = 14;
}

/// <summary>
/// Operator input for one print run (噴印控制台 基本設置) plus the selected PrintData/CartonSize rows.
/// Box numbering follows the legacy screen: <see cref="BoxesDone"/> is the number already printed
/// (0-based start), the run prints boxes BoxesDone+1 … TotalBoxes.
/// </summary>
public sealed record JobSpec
{
    public required ProductSpec Product { get; init; }
    public required CartonSpec Carton { get; init; }

    /// <summary>製番 NO.</summary>
    public required string MadeNo { get; init; }

    /// <summary>箱數（已噴）.</summary>
    public int BoxesDone { get; init; }

    /// <summary>總箱數.</summary>
    public int TotalBoxes { get; init; }

    public required string MachineNoPrefix { get; init; }

    /// <summary>機番流水號 of the next carton to print (digits; leading zeros kept).</summary>
    public required string MachineNoSerial { get; init; }

    public SerialDirection Direction { get; init; } = SerialDirection.Ascending;

    public int RemainingBoxes => TotalBoxes - BoxesDone;

    public MachineNumberSequence Sequence => new(MachineNoPrefix, MachineNoSerial, Direction);

    public ValidationResult Validate(FieldLimits? limits = null)
    {
        limits ??= new FieldLimits();
        var r = new ValidationResult();
        var p = Product;

        if (string.IsNullOrWhiteSpace(p.KeyCode)) r.Add("Key code", "Key code 不可為空");
        if (string.IsNullOrWhiteSpace(MadeNo)) r.Add("製番", "製番不可為空");
        if (BoxesDone < 0) r.Add("箱數", "箱數不可小於 0");
        if (TotalBoxes <= 0) r.Add("總箱數", "總箱數必須大於 0");
        else if (BoxesDone >= TotalBoxes) r.Add("箱數", "箱數必須小於總箱數");
        if (!string.Equals(p.CaseNo, Carton.CaseNo, StringComparison.OrdinalIgnoreCase))
            r.Add("紙箱編號", $"噴印資料的紙箱編號 {p.CaseNo} 與紙箱規格 {Carton.CaseNo} 不符");

        Len(r, "Key code", p.KeyCode, limits.KeyCode);
        Len(r, "CASE-No", p.CaseNo, limits.CaseNo);
        Len(r, "MODEL", p.Model, limits.Model);
        Len(r, "MODEL_value", p.ModelValue, limits.ModelValue);
        Len(r, "BORE", p.Bore, limits.Bore);
        Len(r, "BORE_value", p.BoreValue, limits.BoreValue);
        Len(r, "BORE_unit", p.BoreUnit, limits.BoreUnit);
        Len(r, "OUTPUT", p.Output, limits.Output);
        Len(r, "OUTPUT_value", p.OutputValue, limits.OutputValue);
        Len(r, "OUTPUT_unit", p.OutputUnit, limits.OutputUnit);
        Len(r, "PHASE_value", p.PhaseValue, limits.PhaseValue);
        Len(r, "PHASE_unit", p.PhaseUnit, limits.PhaseUnit);
        Len(r, "VOLTAGE_value", p.VoltageValue, limits.VoltageValue);
        Len(r, "VOLTAGE_unit", p.VoltageUnit, limits.VoltageUnit);
        Len(r, "Hz_value", p.HzValue, limits.HzValue);
        Len(r, "Hz_unit", p.HzUnit, limits.HzUnit);
        Len(r, "CODE", p.Code, limits.Code);
        Len(r, "CODE_value", p.CodeValue, limits.CodeValue);
        Len(r, "TOKKI", p.Tokki, limits.Tokki);
        Len(r, "PLACE", p.Place, limits.Place);
        Len(r, "製番", MadeNo, limits.MadeNo);
        Len(r, "機番前綴", MachineNoPrefix, limits.MachineNoPrefix);
        Len(r, "機番流水號", MachineNoSerial, limits.MachineNoSerial);

        r.Merge(CartonLayout.Validate(Carton));
        if (r.Errors.All(e => e.Field is not ("箱數" or "總箱數")))
            r.Merge(Sequence.Validate(RemainingBoxes));
        return r;
    }

    private static void Len(ValidationResult r, string field, string? value, int max)
    {
        if (value != null && value.Length > max)
            r.Add(field, $"長度 {value.Length} 超過上限 {max}");
    }
}

/// <summary>Carton geometry checks (legacy import rules + lessons from analysis/04 §5).</summary>
public static class CartonLayout
{
    public static ValidationResult Validate(CartonSpec c)
    {
        var r = new ValidationResult();
        if (string.IsNullOrWhiteSpace(c.CaseNo)) r.Add("CASE-No", "紙箱編號不可為空");
        if (c.Length <= 0 || c.Width <= 0) r.Add("紙箱尺寸", "紙箱長、寬必須大於 0");
        Side(r, "表面", c.OutsideStart, c.OutsideEnd, c.Length + c.Width);
        Side(r, "裏面", c.InsideStart, c.InsideEnd, c.Length + c.Width);
        return r;
    }

    private static void Side(ValidationResult r, string name, decimal start, decimal end, decimal limit)
    {
        if (start < 0) r.Add($"{name}印字開始", "不可為負值");
        if (end > limit) r.Add($"{name}印字終了", $"{end} 大於紙箱 L+W（{limit}）");
        // Some legacy cartons (01160, 01163, 01172, 01173) have start > end; meaning unconfirmed
        // with the customer (analysis/04 §6.3), so it is reported rather than silently accepted.
        if (start > end) r.Add($"{name}印字開始", $"開始 {start} 大於終了 {end}（待與客戶確認此紙箱的定義）");
    }
}
