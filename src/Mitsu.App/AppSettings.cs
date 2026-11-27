using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.Json;
using Mitsu.Core.Domain;
using Mitsu.Core.Production;

namespace Mitsu.App;

[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed class PrinterConfig
{
    [DisplayName("名稱"), Description("給噴印目標引用的名稱")]
    public string Name { get; set; } = "X1-1";

    [DisplayName("IP 位址")]
    public string Host { get; set; } = "192.168.1.100";

    [DisplayName("埠號"), Description("X1/SPH 固定為 25001")]
    public int Port { get; set; } = 25001;

    public override string ToString() => $"{Name} ({Host}:{Port})";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed class TargetConfig
{
    [DisplayName("名稱"), Description("例如 表面、裏面")]
    public string Name { get; set; } = "表面";

    [DisplayName("噴碼機"), Description("對應「噴碼機」清單中的名稱")]
    public string Printer { get; set; } = "X1-1";

    [DisplayName("產線 Line")]
    public ushort Line { get; set; } = 1;

    [DisplayName("工位 Station")]
    public ushort Station { get; set; } = 1;

    [DisplayName("訊息檔"), Description("X1 上的訊息（版面）檔名，ASCII，例如 MSG00001.xml")]
    public string MessageFile { get; set; } = "MSG00001.xml";

    public override string ToString() => $"{Name} → {Printer} L{Line}/S{Station} {MessageFile}";
}

/// <summary>Application settings stored as JSON in %ProgramData%\MITSU\appsettings.json.</summary>
public sealed class AppSettings
{
    // ---------------------------------------------------------------- printers
    [Category("1. 噴碼機"), DisplayName("噴碼機"), Description("連線的噴碼機")]
    public List<PrinterConfig> Printers { get; set; } = new() { new PrinterConfig() };

    [Category("1. 噴碼機"), DisplayName("噴印目標"), Description("每一面（表面／裏面）對應的噴碼機、產線、工位與訊息檔。同一箱的資料會送到每個目標。")]
    public List<TargetConfig> Targets { get; set; } = new() { new TargetConfig() };

    [Category("1. 噴碼機"), DisplayName("使用內建模擬器"), Description("不連真機，改連程式內建的模擬噴碼機（測試／教學用）")]
    public bool UseSimulator { get; set; } = true;

    [Category("1. 噴碼機"), DisplayName("模擬器自動觸發 (ms)"), Description("模擬器每隔多久模擬一個紙箱通過電眼；0 = 不自動")]
    public int SimulatorAutoTriggerMs { get; set; } = 2000;

    // ---------------------------------------------------------------- production
    [Category("2. 生產"), DisplayName("緩衝箱數"), Description("預先送進噴碼機緩衝區的箱數（取代舊版「噴印最大緩存」）。不可大於噴碼機緩衝深度。")]
    public int BufferTarget { get; set; } = 3;

    [Category("2. 生產"), DisplayName("輪詢間隔 (ms)")]
    public int PollIntervalMs { get; set; } = 200;

    [Category("2. 生產"), DisplayName("警報檢查頻率（每 N 次輪詢）")]
    public int AlarmCheckEveryPolls { get; set; } = 10;

    [Category("2. 生產"), DisplayName("計數器交叉檢查"), Description("計數器比送出的資料多（噴出空白箱）時停機")]
    public bool CounterCrossCheck { get; set; } = true;

    [Category("2. 生產"), DisplayName("正反面容許差 (箱)"), Description("各噴印目標已噴箱數相差超過此值即停機")]
    public int MaxTargetSkew { get; set; } = 3;

    // ---------------------------------------------------------------- payload
    [Category("3. 噴印內容"), DisplayName("電壓與頻率間空白數")]
    public int SpacesBetweenVoltageAndHz { get; set; } = 5;

    [Category("3. 噴印內容"), DisplayName("機番標籤")]
    public string MachineNoLabel { get; set; } = "No:";

    // ---------------------------------------------------------------- internal
    [Browsable(false)]
    public string? EngineerPasswordHash { get; set; }

    [Browsable(false)]
    public int[]? StatusWindowBounds { get; set; }

    [Browsable(false)]
    public int[]? MainWindowBounds { get; set; }

    public ProductionOptions ToProductionOptions() => new()
    {
        BufferTarget = Math.Max(1, BufferTarget),
        PollInterval = TimeSpan.FromMilliseconds(Math.Clamp(PollIntervalMs, 50, 5000)),
        AlarmCheckEveryPolls = AlarmCheckEveryPolls,
        CounterCrossCheck = CounterCrossCheck,
        MaxTargetSkew = Math.Max(0, MaxTargetSkew),
    };

    public PayloadOptions ToPayloadOptions() => new()
    {
        SpacesBetweenVoltageAndHz = Math.Max(0, SpacesBetweenVoltageAndHz),
        MachineNoLabel = MachineNoLabel ?? "",
    };

    /// <summary>Problems that make the settings unusable (shown before saving / connecting).</summary>
    public List<string> Check()
    {
        var errors = new List<string>();
        if (Printers.Count == 0) errors.Add("至少要設定一台噴碼機");
        if (Targets.Count == 0) errors.Add("至少要設定一個噴印目標");
        foreach (var d in Printers.GroupBy(p => p.Name).Where(g => g.Count() > 1))
            errors.Add($"噴碼機名稱重複：{d.Key}");
        foreach (var t in Targets)
        {
            if (Printers.All(p => p.Name != t.Printer))
                errors.Add($"噴印目標「{t.Name}」引用了不存在的噴碼機「{t.Printer}」");
            if (string.IsNullOrWhiteSpace(t.MessageFile) || t.MessageFile.Any(c => c > 0x7E || c < 0x20))
                errors.Add($"噴印目標「{t.Name}」的訊息檔名必須是 ASCII");
        }
        foreach (var d in Targets.GroupBy(t => (t.Printer, t.Line, t.Station)).Where(g => g.Count() > 1))
            errors.Add($"噴印目標重複：{d.Key.Printer} L{d.Key.Line}/S{d.Key.Station}");
        return errors;
    }

    // ---------------------------------------------------------------- persistence

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static string DefaultPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MITSU", "appsettings.json");

    public static AppSettings Load(string? path = null)
    {
        path ??= DefaultPath;
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), Json) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // fall through to defaults; the caller shows the path in the log
        }
        return new AppSettings();
    }

    public void Save(string? path = null)
    {
        path ??= DefaultPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(this, Json));
        File.Move(tmp, path, overwrite: true);
    }

    public AppSettings Clone() => JsonSerializer.Deserialize<AppSettings>(JsonSerializer.Serialize(this, Json), Json)!;

    // ---------------------------------------------------------------- password (PBKDF2, never stored in plain text)

    [Browsable(false), System.Text.Json.Serialization.JsonIgnore]
    public bool HasPassword => !string.IsNullOrEmpty(EngineerPasswordHash);

    public void SetPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        EngineerPasswordHash = $"100000:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password)
    {
        if (!HasPassword) return true;
        var parts = EngineerPasswordHash!.Split(':');
        if (parts.Length != 3 || !int.TryParse(parts[0], out int iter)) return false;
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
