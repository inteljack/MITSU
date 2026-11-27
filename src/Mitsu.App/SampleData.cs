using Mitsu.Core.Data;
using Mitsu.Core.Domain;

namespace Mitsu.App;

/// <summary>
/// 示範資料 until the SQL/Excel data layer exists. Rows follow the legacy sample files
/// (hejian\PrintData.xls, CartonSize.xls and the 2021 site screenshot).
/// </summary>
public static class SampleData
{
    public static IMasterDataSource Create() => new InMemoryMasterDataSource(Products, Cartons);

    private static ProductSpec P(string key, string caseNo, string model, string bore, string output,
        string phase, string volt, string hz, string code, string tokki = "") => new()
    {
        KeyCode = key,
        CaseNo = caseNo,
        Model = "MODEL",
        ModelValue = model,
        Bore = "BORE",
        BoreValue = bore,
        BoreUnit = "mm",
        Output = "OUTPUT",
        OutputValue = output,
        OutputUnit = "kW",
        PhaseValue = phase,
        PhaseUnit = "φ",
        VoltageValue = volt,
        VoltageUnit = "V",
        HzValue = hz,
        HzUnit = "Hz",
        Code = "CODE",
        CodeValue = code,
        Tokki = tokki,
        Place = "MADE BY TSURUMI TAIWAN",
    };

    private static readonly ProductSpec[] Products =
    {
        P("IA01HN-2DS51", "01156", "HSN2.4S-51", "50", "0.4", "1", "100", "50", "A01HN-2DS51"),
        P("II01IHS3E-5C", "01158", "HS3.75S-52", "80", "0.75", "1", "220/230/240", "50", "I01IHS3E-5C"),
        P("IA01RHE-DS02", "01153", "HSD2.55S", "50", "0.55", "3", "200", "60", "A01RHE-DS02", "2''NPTFLANGE USA(ETL)"),
        P("IB01KTZ-21", "01153", "KTZ21.5", "80", "1.5", "3", "400", "50", "B01KTZ-21"),
    };

    private static readonly CartonSpec[] Cartons =
    {
        new() { CaseNo = "01153", Length = 330, Width = 225, Height = 538, PrintHeight = 248, OutsideStart = 25, OutsideEnd = 530, InsideStart = 25, InsideEnd = 530 },
        new() { CaseNo = "01156", Length = 330, Width = 225, Height = 538, PrintHeight = 248, OutsideStart = 25, OutsideEnd = 530, InsideStart = 25, InsideEnd = 530 },
        new() { CaseNo = "01158", Length = 690, Width = 205, Height = 70, PrintHeight = 205, OutsideStart = 30, OutsideEnd = 650, InsideStart = 30, InsideEnd = 650 },
    };
}
