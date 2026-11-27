using System.Text;
using Mitsu.Printer;
using Mitsu.Printer.Protocol;
using Mitsu.Printer.Simulator;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
    PrintUsage();
    return 0;
}

try
{
    return args[0] switch
    {
        "sim" => await RunSimulatorAsync(args[1..]),
        "probe" => await RunProbeAsync(args[1..]),
        _ => Fail($"Unknown mode '{args[0]}'."),
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine("Error: " + ex.Message);
    return 1;
}

static int Fail(string msg)
{
    Console.Error.WriteLine(msg);
    PrintUsage();
    return 2;
}

static void PrintUsage()
{
    Console.WriteLine("""
        x1tool — NexGen X1/SPH communication tool

        x1tool sim   [--port 25001] [--capacity 10] [--auto-ms 0] [--empty blank|skip] [--any-ip]
            Run the printer simulator. Console commands:
              t [line] [station]       simulate photo-eye trigger (print one item)
              alarm <h0> <h1>          set alarm codes (0 = ok, 2 = ink empty ...)
              drop <n>                 drop next n replies
              delay <ms>               reply delay
              split on|off             send replies byte by byte
              kick                     disconnect clients
              status                   show state
              q                        quit

        x1tool probe <host> [--port 25001] [--log frames.log] [--timeout-ms 3000] [--tid]
            Connect to a printer (or simulator) and run commands interactively.
            Every frame is logged as hex to the log file (default x1-yyyyMMdd-HHmmss.log).
              ver | alarm | status | name | clock
              counter <msgNo> <1=item|2=box|3=lot>
              select <line> <station> <file.xml>       20h+214h (stopped)
              selp <line> <station> <file.xml>         20h+24Ch (while printing)
              start <line> | stop <line>               20h+203h
              cf <line> <station> v1|v2|...            CFh push values (separator |)
              ca <table.ini> v1|v2|...                 CAh
              cc <line> <station>                      CCh buffer count
              cb <line> <station>                      CBh clear buffer
              raw <cmdHex> [dataHex]                   e.g. raw 20 0091
              fifo <line> <station> <max>              P0 test: clear, push until NAK, report depth
              watch <line> <station> <seconds>         poll CCh + 45h every 200 ms and print changes
              q
        """);
}

// ============================================================================ simulator

static async Task<int> RunSimulatorAsync(string[] a)
{
    var opt = Options.Parse(a);
    var options = new X1SimulatorOptions
    {
        Port = opt.Int("port", 25001),
        BufferCapacity = opt.Int("capacity", 10),
        AutoTriggerInterval = opt.Int("auto-ms", 0) is > 0 and var ms ? TimeSpan.FromMilliseconds(ms) : null,
        OnEmptyBuffer = opt.Str("empty", "blank") == "skip" ? EmptyBufferBehavior.SkipPrint : EmptyBufferBehavior.PrintBlank,
        Address = opt.Flag("any-ip") ? System.Net.IPAddress.Any : System.Net.IPAddress.Loopback,
    };
    await using var sim = new X1Simulator(options);
    sim.RequestReceived += f => Console.WriteLine($"  <- {Describe(f)}");
    sim.Printed += p => Console.WriteLine(
        $"  ** PRINT L{p.Line}/S{p.Station} #{p.ItemCounter} {(p.FromBuffer ? "" : "(BLANK) ")}{string.Join("|", p.Values.Where(v => v.Length > 0))}");
    sim.Start();
    Console.WriteLine($"Simulator listening on {options.Address}:{sim.Port}  (capacity {options.BufferCapacity}). 'q' to quit.");

    while (Console.ReadLine() is { } line)
    {
        var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (p.Length == 0) continue;
        try
        {
            switch (p[0])
            {
                case "q": return 0;
                case "t":
                    var item = sim.Trigger(U16(p, 1, 1), U16(p, 2, 1));
                    if (item == null) Console.WriteLine("  (nothing printed: line not printing or no message selected)");
                    break;
                case "alarm": sim.SetAlarm((InkAlarm)byte.Parse(p[1]), (InkAlarm)byte.Parse(p[2])); break;
                case "drop": sim.DropNextResponses(int.Parse(p[1])); break;
                case "delay": sim.ResponseDelay = TimeSpan.FromMilliseconds(int.Parse(p[1])); break;
                case "split": sim.SplitResponses = p[1] == "on"; break;
                case "kick": sim.DisconnectClients(); break;
                case "status":
                    for (ushort l = 1; l <= 2; l++)
                        Console.WriteLine($"  line {l}: printing={sim.IsPrinting(l)} counter={sim.ItemCounter(l)} " +
                                          $"msg={sim.SelectedMessage(l, 1) ?? "-"} buffer(S1)={sim.BufferCount(l, 1)}");
                    break;
                default: Console.WriteLine("  ?"); break;
            }
        }
        catch (Exception ex) { Console.WriteLine("  error: " + ex.Message); }
    }

    // stdin closed (e.g. started as a background service): keep serving until Ctrl+C.
    using var stop = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.Cancel(); };
    try { await Task.Delay(Timeout.Infinite, stop.Token); } catch (OperationCanceledException) { }
    return 0;
}

// ============================================================================ probe

static async Task<int> RunProbeAsync(string[] a)
{
    if (a.Length == 0 || a[0].StartsWith("--")) return Fail("probe needs a host.");
    var opt = Options.Parse(a[1..]);
    string logPath = opt.Str("log", $"x1-{DateTime.Now:yyyyMMdd-HHmmss}.log");
    await using var log = new StreamWriter(logPath, append: true, Encoding.UTF8) { AutoFlush = true };
    var logLock = new object();
    void Log(string s) { lock (logLock) log.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {s}"); }

    await using var c = new X1Client(new X1ClientOptions
    {
        Host = a[0],
        Port = opt.Int("port", 25001),
        RequestTimeout = TimeSpan.FromMilliseconds(opt.Int("timeout-ms", 3000)),
        HeartbeatInterval = TimeSpan.FromSeconds(10),
        UseTransactionIds = opt.Flag("tid"),
    });
    c.Traffic += (_, e) => Log($"{(e.Direction == TrafficDirection.Sent ? ">>" : "<<")} {Convert.ToHexString(e.Bytes)}");
    c.UnsolicitedFrame += (_, e) => { Log("UNSOLICITED " + Describe(e.Frame)); Console.WriteLine($"  [unsolicited] {Describe(e.Frame)}"); };
    c.Disconnected += (_, e) => { Log("DISCONNECTED " + e.Reason); Console.WriteLine($"  [disconnected] {e.Reason}"); };

    Console.WriteLine($"Connecting to {a[0]}:{c.Options.Port} ... (log: {Path.GetFullPath(logPath)})");
    await c.ConnectAsync();
    Console.WriteLine("Connected. 'help' for commands.");

    while (true)
    {
        Console.Write("x1> ");
        var line = Console.ReadLine();
        if (line == null) break;
        line = line.Trim().TrimStart('﻿'); // piped input from PowerShell may start with a BOM
        if (line.Length == 0) continue;
        Log("CMD " + line);
        var p = line.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
        try
        {
            if (!c.IsConnected && p[0] != "q" && p[0] != "connect")
            {
                Console.WriteLine("  not connected — type 'connect'");
                continue;
            }
            string result = p[0] switch
            {
                "q" or "quit" => "",
                "help" => Help(),
                "connect" => await Do(async () => { await c.ConnectAsync(); return "connected"; }),
                "ver" => (await c.GetProtocolVersionAsync()).ToString(),
                "alarm" => (await c.GetAlarmStatusAsync()).ToString(),
                "name" => await c.GetPrinterNameAsync(),
                "clock" => (await c.GetSystemClockAsync()).ToString("yyyy-MM-dd HH:mm:ss"),
                "status" => FormatStatus(await c.GetPrintingStatusAsync()),
                "counter" => (await c.GetCounterAsync(U16(p, 1, 1), (CounterType)byte.Parse(p[2]))).ToString(),
                "select" => await Ok(c.SelectMessageAsync(U16(p, 1, 1), U16(p, 2, 1), p[3])),
                "selp" => await Ok(c.SelectMessageWhilePrintingAsync(U16(p, 1, 1), U16(p, 2, 1), p[3])),
                "start" => await Ok(c.StartLineAsync(U16(p, 1, 1))),
                "stop" => await Ok(c.StopLineAsync(U16(p, 1, 1))),
                "cf" => await Ok(c.PushDynamicVarsAsync(U16(p, 1, 1), U16(p, 2, 1), Values(p, 3))),
                "ca" => await Ok(c.SetDynamicVarTableAsync(p[1], Values(line.Split(' ', 3), 2))),
                "cc" => (await c.GetDynamicVarBufferCountAsync(U16(p, 1, 1), U16(p, 2, 1))).ToString(),
                "cb" => await Ok(c.ClearDynamicVarBufferAsync(U16(p, 1, 1), U16(p, 2, 1))),
                "raw" => Describe(await c.SendRawAsync(Convert.ToByte(p[1], 16), p.Length > 2 ? Convert.FromHexString(string.Concat(p[2..]).Replace(" ", "")) : Array.Empty<byte>())),
                "fifo" => await FifoTest(c, U16(p, 1, 1), U16(p, 2, 1), int.Parse(p[3])),
                "watch" => await Watch(c, U16(p, 1, 1), U16(p, 2, 1), int.Parse(p[3])),
                _ => "unknown command ('help')",
            };
            if (p[0] is "q" or "quit") break;
            Console.WriteLine("  " + result.Replace("\n", "\n  "));
            Log("RESULT " + result.Replace("\n", " / "));
        }
        catch (X1NakException ex)
        {
            Console.WriteLine($"  NAK: {ex.Message}");
            Log("NAK " + ex.Message);
        }
        catch (Exception ex) when (ex is X1Exception or ArgumentException or FormatException or IndexOutOfRangeException or AnserProtocolException)
        {
            Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
            Log("ERROR " + ex.Message);
        }
    }
    return 0;

    static string Help() { PrintUsage(); return ""; }
}

static async Task<string> FifoTest(X1Client c, ushort line, ushort station, int max)
{
    var sb = new StringBuilder();
    await c.ClearDynamicVarBufferAsync(line, station);
    sb.AppendLine($"cleared; CCh = {await c.GetDynamicVarBufferCountAsync(line, station)}");
    for (int i = 1; i <= max; i++)
    {
        try
        {
            await c.PushDynamicVarsAsync(line, station, new[] { $"FIFO-{i:000}" });
        }
        catch (X1NakException ex)
        {
            sb.AppendLine($"push #{i} rejected: {ex.Message}");
            sb.Append($"=> accepted {i - 1} entries before rejection");
            return sb.ToString();
        }
        sb.AppendLine($"push #{i} ok; CCh = {await c.GetDynamicVarBufferCountAsync(line, station)}");
    }
    sb.Append($"=> all {max} pushes accepted (no limit reached). Remember to 'cb' afterwards.");
    return sb.ToString();
}

static async Task<string> Watch(X1Client c, ushort line, ushort station, int seconds)
{
    var end = DateTime.Now.AddSeconds(seconds);
    string last = "";
    int changes = 0;
    while (DateTime.Now < end)
    {
        short count = await c.GetDynamicVarBufferCountAsync(line, station);
        var st = await c.GetPrintingStatusAsync();
        var ls = st.Lines.FirstOrDefault(x => x.Line == line);
        string now = $"buffer={count} printing={(ls != null)} item={ls?.ItemCounter} printCount={ls?.PrintCount}";
        if (now != last)
        {
            Console.WriteLine($"  {DateTime.Now:HH:mm:ss.fff} {now}");
            last = now;
            changes++;
        }
        await Task.Delay(200);
    }
    return $"watch finished ({changes} distinct states)";
}

static async Task<string> Ok(Task t) { await t; return "OK"; }
static async Task<string> Do(Func<Task<string>> f) => await f();

static string[] Values(string[] p, int idx) => p.Length > idx ? p[idx].Split('|') : Array.Empty<string>();

static ushort U16(string[] p, int idx, ushort def) => p.Length > idx ? ushort.Parse(p[idx]) : def;

static string FormatStatus(PrintingStatus s)
{
    if (s.Lines.Count == 0) return "no line printing";
    return $"block={s.BlockSize}\n" + string.Join("\n", s.Lines.Select(l =>
        $"line {l.Line} station {l.Station}: item={l.ItemCounter} box={l.BoxCounter} lot={l.LotCounter} " +
        $"printCount={l.PrintCount} total={l.PrintTotalCount} printables={l.Printables} ppm={l.PrintsPerMinute} speed={l.LineSpeed}"));
}

static string Describe(AnserFrame f) =>
    $"{f.Command:X2}h{(f.SubCommand is { } s ? $"+{s:X4}h" : "")} tid={f.TransactionId} st={f.Status} data[{f.Data.Length}]={Convert.ToHexString(f.Data.Span[..Math.Min(f.Data.Length, 48)])}{(f.Data.Length > 48 ? "…" : "")}";

sealed class Options
{
    private readonly Dictionary<string, string?> _v = new();

    public static Options Parse(string[] a)
    {
        var o = new Options();
        for (int i = 0; i < a.Length; i++)
        {
            if (!a[i].StartsWith("--")) throw new ArgumentException($"Unexpected argument '{a[i]}'.");
            string key = a[i][2..];
            string? val = i + 1 < a.Length && !a[i + 1].StartsWith("--") ? a[++i] : null;
            o._v[key] = val;
        }
        return o;
    }

    public int Int(string k, int def) => _v.TryGetValue(k, out var v) && v != null ? int.Parse(v) : def;
    public string Str(string k, string def) => _v.TryGetValue(k, out var v) && v != null ? v : def;
    public bool Flag(string k) => _v.ContainsKey(k);
}
