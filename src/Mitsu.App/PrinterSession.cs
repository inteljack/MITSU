using Mitsu.Core.Production;
using Mitsu.Printer;
using Mitsu.Printer.Simulator;

namespace Mitsu.App;

/// <summary>
/// Owns the printer connections (one <see cref="X1Client"/> per configured printer) and, in
/// simulator mode, an in-process <see cref="X1Simulator"/> that every printer entry points to.
/// </summary>
public sealed class PrinterSession : IAsyncDisposable
{
    private readonly Dictionary<string, X1Client> _clients = new();
    private X1Simulator? _simulator;

    public PrinterSession(AppSettings settings)
    {
        Settings = settings;
    }

    public AppSettings Settings { get; }
    public X1Simulator? Simulator => _simulator;

    public bool AllConnected => _clients.Count > 0 && _clients.Values.All(c => c.IsConnected);

    public event Action<string>? Log;
    public event Action? ConnectionChanged;

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        var errors = Settings.Check();
        if (errors.Count > 0) throw new InvalidOperationException("系統設定有誤：\n" + string.Join("\n", errors));

        if (Settings.UseSimulator && _simulator == null)
        {
            _simulator = new X1Simulator(new X1SimulatorOptions
            {
                BufferCapacity = Math.Max(10, Settings.BufferTarget),
                AutoTriggerInterval = Settings.SimulatorAutoTriggerMs > 0
                    ? TimeSpan.FromMilliseconds(Settings.SimulatorAutoTriggerMs)
                    : null,
            });
            _simulator.Start();
            Log?.Invoke($"內建模擬器已啟動（127.0.0.1:{_simulator.Port}，自動觸發 {Settings.SimulatorAutoTriggerMs} ms）");
        }

        foreach (var p in Settings.Printers)
        {
            if (!_clients.TryGetValue(p.Name, out var client))
            {
                client = new X1Client(new X1ClientOptions
                {
                    Host = Settings.UseSimulator ? "127.0.0.1" : p.Host,
                    Port = Settings.UseSimulator ? _simulator!.Port : p.Port,
                });
                string name = p.Name;
                client.Disconnected += (_, e) =>
                {
                    Log?.Invoke($"{name} 斷線：{e.Reason}");
                    ConnectionChanged?.Invoke();
                };
                _clients[p.Name] = client;
            }
            if (client.IsConnected) continue;

            await client.ConnectAsync(ct);
            var ver = await client.GetProtocolVersionAsync(ct);
            var name2 = await client.GetPrinterNameAsync(ct);
            Log?.Invoke($"{p.Name} 已連線：{name2}，協定版本 {ver}");
        }
        ConnectionChanged?.Invoke();
    }

    public async Task DisconnectAsync()
    {
        foreach (var c in _clients.Values) await c.DisposeAsync();
        _clients.Clear();
        if (_simulator != null)
        {
            await _simulator.DisposeAsync();
            _simulator = null;
        }
        ConnectionChanged?.Invoke();
    }

    public IReadOnlyList<PrintTarget> BuildTargets() =>
        Settings.Targets.Select(t => new PrintTarget(t.Name, _clients[t.Printer], t.Line, t.Station, t.MessageFile)).ToList();

    public ValueTask DisposeAsync() => new(DisconnectAsync());
}
