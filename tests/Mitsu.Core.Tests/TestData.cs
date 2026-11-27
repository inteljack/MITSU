using Mitsu.Core.Domain;

namespace Mitsu.Core.Tests;

/// <summary>Sample rows taken from the legacy hejian\PrintData.xls / CartonSize.xls (analysis/03 §3.2).</summary>
internal static class TestData
{
    public static ProductSpec Product => new()
    {
        KeyCode = "IA01HN-2DS51",
        CaseNo = "01156",
        Model = "MODEL",
        ModelValue = "HSN2.4S-51",
        Bore = "BORE",
        BoreValue = "50",
        BoreUnit = "mm",
        Output = "OUTPUT",
        OutputValue = "0.4",
        OutputUnit = "kW",
        PhaseValue = "1",
        PhaseUnit = "φ",
        VoltageValue = "100",
        VoltageUnit = "V",
        HzValue = "50",
        HzUnit = "Hz",
        Code = "CODE",
        CodeValue = "A01HN-2DS51",
        Tokki = "",
        Place = "MADE BY TSURUMI TAIWAN",
    };

    public static CartonSpec Carton => new()
    {
        CaseNo = "01156",
        Length = 330,
        Width = 225,
        Height = 538,
        PrintHeight = 248,
        OutsideStart = 25,
        OutsideEnd = 530,
        InsideStart = 25,
        InsideEnd = 530,
    };

    public static JobSpec Job(int boxesDone = 0, int total = 5, string prefix = "21", string serial = "678569136",
        SerialDirection dir = SerialDirection.Ascending) => new()
    {
        Product = Product,
        Carton = Carton,
        MadeNo = "6785691",
        BoxesDone = boxesDone,
        TotalBoxes = total,
        MachineNoPrefix = prefix,
        MachineNoSerial = serial,
        Direction = dir,
    };
}
