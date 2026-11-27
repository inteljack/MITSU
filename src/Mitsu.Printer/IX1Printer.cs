using Mitsu.Printer.Protocol;

namespace Mitsu.Printer;

/// <summary>
/// The subset of printer operations the production controller needs. Implemented by
/// <see cref="X1Client"/>; tests can substitute a fake.
/// </summary>
public interface IX1Printer
{
    bool IsConnected { get; }
    Task<AlarmStatus> GetAlarmStatusAsync(CancellationToken ct = default);
    Task<PrintingStatus> GetPrintingStatusAsync(CancellationToken ct = default);
    Task StartLineAsync(ushort line, CancellationToken ct = default);
    Task StopLineAsync(ushort line, CancellationToken ct = default);
    Task SelectMessageAsync(ushort line, ushort station, string messageFile, CancellationToken ct = default);
    Task PushDynamicVarsAsync(ushort line, ushort station, IReadOnlyList<string> values, CancellationToken ct = default);
    Task ClearDynamicVarBufferAsync(ushort line, ushort station, CancellationToken ct = default);
    Task<short> GetDynamicVarBufferCountAsync(ushort line, ushort station, CancellationToken ct = default);
}
