namespace Mitsu.Core.Domain;

/// <summary>
/// Index of each value in the per-carton payload. The order is the legacy ADT field order
/// (hejian 2.1.x <c>SetDataToPrintPro</c>, analysis/03 §5.3), so the X1 message objects can be
/// bound to String(index + 1) exactly like the old .ms templates were bound to ADT fields.
/// </summary>
public enum PayloadField
{
    ModelLabel = 0,          // "MODEL"
    ModelValue = 1,          // front-side model (large)
    BoreLabel = 2,           // "BORE"
    BoreValue = 3,           // value + unit, e.g. "50mm"
    OutputLabel = 4,         // "OUTPUT"
    OutputValue = 5,         // value + unit, e.g. "0.4kW"
    PhaseValue = 6,
    PhaseUnit = 7,
    VoltageHz = 8,           // "220V" + N spaces + "60Hz"
    MadeNo = 9,              // 製番 (Code128 barcode + text)
    Tokki = 10,              // 特記
    CodeAndMachineNo = 11,   // CODE_value + "   " + 機番 (Code128 barcode)
    CodeLabel = 12,          // "CODE:"
    CodeValue = 13,
    MachineNoLabel = 14,     // "No:"
    MachineNo = 15,          // 機番 = prefix + serial
    ModelValueSide = 16,     // side mark large model
    Place = 17,              // e.g. "MADE BY TSURUMI TAIWAN"
}

public sealed record PayloadOptions
{
    /// <summary>Spaces between voltage and Hz (hejian.xml SpaceBtVoltageHzValue).</summary>
    public int SpacesBetweenVoltageAndHz { get; init; } = 5;

    /// <summary>Separator between CODE_value and 機番 in the second barcode.</summary>
    public string CodeMachineSeparator { get; init; } = "   ";

    /// <summary>Label shown before the machine number (legacy lblMacineNo).</summary>
    public string MachineNoLabel { get; init; } = "No:";

    /// <summary>Suffix appended to the CODE label (Excel holds "CODE", the carton shows "CODE:").</summary>
    public string CodeLabelSuffix { get; init; } = ":";

    /// <summary>Strings sent per carton; the X1 CFh command carries exactly 20.</summary>
    public const int SlotCount = 20;
}

/// <summary>Builds the per-carton dynamic values (String1..String20 of CFh).</summary>
public sealed class CartonPayloadComposer(PayloadOptions? options = null)
{
    public PayloadOptions Options { get; } = options ?? new PayloadOptions();

    public const int FieldCount = 18;

    /// <summary>Values that are the same for every carton of a job (everything except the machine number).</summary>
    public string[] ComposeStatic(ProductSpec p, string madeNo)
    {
        var v = new string[PayloadOptions.SlotCount];
        Array.Fill(v, "");
        v[(int)PayloadField.ModelLabel] = p.Model;
        v[(int)PayloadField.ModelValue] = p.ModelValue;
        v[(int)PayloadField.BoreLabel] = p.Bore;
        v[(int)PayloadField.BoreValue] = ValueWithUnit(p.BoreValue, p.BoreUnit);
        v[(int)PayloadField.OutputLabel] = p.Output;
        v[(int)PayloadField.OutputValue] = ValueWithUnit(p.OutputValue, p.OutputUnit);
        v[(int)PayloadField.PhaseValue] = p.PhaseValue;
        v[(int)PayloadField.PhaseUnit] = p.PhaseValue.Length == 0 ? "" : p.PhaseUnit;
        v[(int)PayloadField.VoltageHz] = VoltageHz(p);
        v[(int)PayloadField.MadeNo] = madeNo;
        v[(int)PayloadField.Tokki] = p.Tokki;
        v[(int)PayloadField.CodeLabel] = p.Code.Length == 0 ? "" : p.Code + Options.CodeLabelSuffix;
        v[(int)PayloadField.CodeValue] = p.CodeValue;
        v[(int)PayloadField.MachineNoLabel] = Options.MachineNoLabel;
        v[(int)PayloadField.ModelValueSide] = p.ModelValue;
        v[(int)PayloadField.Place] = p.Place;
        return v;
    }

    /// <summary>Full payload for one carton.</summary>
    public string[] Compose(string[] staticValues, ProductSpec p, string machineNo)
    {
        var v = (string[])staticValues.Clone();
        v[(int)PayloadField.MachineNo] = machineNo;
        v[(int)PayloadField.CodeAndMachineNo] = p.CodeValue + Options.CodeMachineSeparator + machineNo;
        return v;
    }

    private string VoltageHz(ProductSpec p)
    {
        string volt = ValueWithUnit(p.VoltageValue, p.VoltageUnit);
        string hz = ValueWithUnit(p.HzValue, p.HzUnit);
        if (volt.Length == 0) return hz;
        if (hz.Length == 0) return volt;
        return volt + new string(' ', Options.SpacesBetweenVoltageAndHz) + hz;
    }

    /// <summary>Empty value → nothing printed (a unit alone would look like garbage).</summary>
    private static string ValueWithUnit(string value, string unit) => value.Length == 0 ? "" : value + unit;
}
