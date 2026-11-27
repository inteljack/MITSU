using Mitsu.Printer.Protocol;

namespace Mitsu.Printer;

/// <summary>Typed commands needed for production. See analysis/01_X1_protocol.md §3–§5.</summary>
public sealed partial class X1Client
{
    /// <summary>43h. Also used as heartbeat.</summary>
    public async Task<ProtocolVersion> GetProtocolVersionAsync(CancellationToken ct = default)
    {
        var r = await SendAsync(X1Requests.Simple(AnserCommand.GetProtocolVersion), null, ct).ConfigureAwait(false);
        return X1Responses.ParseProtocolVersion(ExpectData(r, AnserCommand.GetProtocolVersion));
    }

    /// <summary>7Fh. Alarm code of each print head.</summary>
    public async Task<AlarmStatus> GetAlarmStatusAsync(CancellationToken ct = default)
    {
        var r = await SendAsync(X1Requests.Simple(AnserCommand.GetAlarmStatus), null, ct).ConfigureAwait(false);
        return X1Responses.ParseAlarmStatus(ExpectData(r, AnserCommand.GetAlarmStatus));
    }

    /// <summary>45h. Counters of lines that are currently printing (stopped lines are not reported).</summary>
    public async Task<PrintingStatus> GetPrintingStatusAsync(CancellationToken ct = default)
    {
        var r = await SendAsync(X1Requests.Simple(AnserCommand.GetPrintingStatus), null, ct).ConfigureAwait(false);
        return X1Responses.ParsePrintingStatus(ExpectData(r, AnserCommand.GetPrintingStatus));
    }

    /// <summary>3Ah. Counter of a message number.</summary>
    public async Task<CounterInfo> GetCounterAsync(ushort messageNo, CounterType type, CancellationToken ct = default)
    {
        var r = await SendAsync(X1Requests.GetCounter(messageNo, type), null, ct).ConfigureAwait(false);
        return X1Responses.ParseCounter(ExpectData(r, AnserCommand.GetCounter));
    }

    public async Task<string> GetPrinterNameAsync(CancellationToken ct = default)
    {
        var r = await SendAsync(X1Requests.Simple(AnserCommand.GetPrinterName), null, ct).ConfigureAwait(false);
        return X1Responses.ParseAscii(ExpectData(r, AnserCommand.GetPrinterName));
    }

    public async Task<DateTime> GetSystemClockAsync(CancellationToken ct = default)
    {
        var r = await SendAsync(X1Requests.Simple(AnserCommand.GetSystemClock), null, ct).ConfigureAwait(false);
        return X1Responses.ParseSystemClock(ExpectData(r, AnserCommand.GetSystemClock));
    }

    public Task SetSystemClockAsync(DateTime time, CancellationToken ct = default) =>
        ExpectAck(X1Requests.SetSystemClock(time), ct);

    /// <summary>20h+203h operation 0.</summary>
    public Task StartLineAsync(ushort line, CancellationToken ct = default) =>
        ExpectAck(X1Requests.SetLineOperation(line, start: true), ct);

    /// <summary>20h+203h operation 1.</summary>
    public Task StopLineAsync(ushort line, CancellationToken ct = default) =>
        ExpectAck(X1Requests.SetLineOperation(line, start: false), ct);

    /// <summary>20h+214h. Select the message (job) of a line/station while stopped. File name ASCII, e.g. "MSG00003.xml".</summary>
    public Task SelectMessageAsync(ushort line, ushort station, string messageFile, CancellationToken ct = default) =>
        ExpectAck(X1Requests.SetPrintMessage(line, station, messageFile), ct);

    /// <summary>20h+24Ch. Switch message while printing.</summary>
    public Task SelectMessageWhilePrintingAsync(ushort line, ushort station, string messageFile, CancellationToken ct = default) =>
        ExpectAck(X1Requests.SetPrintMessageWhilePrint(line, station, messageFile), ct);

    /// <summary>CFh. Push up to 20 values (String1..String20) for one item into the line/station buffer.</summary>
    public Task PushDynamicVarsAsync(ushort line, ushort station, IReadOnlyList<string> values, CancellationToken ct = default) =>
        ExpectAck(X1Requests.SetDynamicVarBuffer(line, station, values), ct);

    /// <summary>CAh. Overwrite the 20 strings of a variable table file on the printer.</summary>
    public Task SetDynamicVarTableAsync(string tableFile, IReadOnlyList<string> values, CancellationToken ct = default) =>
        ExpectAck(X1Requests.SetDynamicVarTable(tableFile, values), ct);

    /// <summary>CBh. Clear the dynamic variable buffer of a line/station.</summary>
    public Task ClearDynamicVarBufferAsync(ushort line, ushort station, CancellationToken ct = default) =>
        ExpectAck(X1Requests.ClearDynamicVarBuffer(line, station), ct);

    /// <summary>CCh. Believed to be the number of entries waiting in the buffer (verify on hardware).</summary>
    public async Task<short> GetDynamicVarBufferCountAsync(ushort line, ushort station, CancellationToken ct = default)
    {
        var r = await SendAsync(X1Requests.ReadDynamicVarBufferStatus(line, station), null, ct).ConfigureAwait(false);
        return X1Responses.ParseBufferStatus(ExpectData(r, AnserCommand.ReadDynamicVarBufferStatus));
    }

    private async Task ExpectAck(X1Requests.Request req, CancellationToken ct)
    {
        var r = await SendAsync(req, null, ct).ConfigureAwait(false);
        if (r.Command == AnserCommand.Ack) return;
        // 20h commands may also answer with a 20h frame echoing the sub-command; accept that.
        if (r.Command == req.Command) return;
        throw new AnserProtocolException($"Expected ACK for {req.Command:X2}h, got {r.Command:X2}h.");
    }

    private static ReadOnlySpan<byte> ExpectData(AnserFrame r, byte command)
    {
        if (r.Command != command)
            throw new AnserProtocolException($"Expected {command:X2}h data reply, got {r.Command:X2}h.");
        return r.Data.Span;
    }
}
