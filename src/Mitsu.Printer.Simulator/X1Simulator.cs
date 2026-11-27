using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mitsu.Printer;
using Mitsu.Printer.Protocol;

namespace Mitsu.Printer.Simulator;

public enum EmptyBufferBehavior
{
    /// <summary>Trigger prints with all variables empty (and is recorded as a blank print).</summary>
    PrintBlank,
    /// <summary>Trigger is ignored: nothing printed, counters unchanged.</summary>
    SkipPrint,
}

/// <summary>
/// Behaviour knobs. Defaults encode our current best guess of the real printer; every
/// option marked "P0" is something to adjust once verified on hardware.
/// </summary>
public sealed class X1SimulatorOptions
{
    /// <summary>TCP port; 0 picks a free port (see <see cref="X1Simulator.Port"/>).</summary>
    public int Port { get; init; }
    public IPAddress Address { get; init; } = IPAddress.Loopback;

    /// <summary>P0: depth of the CFh FIFO per line/station.</summary>
    public int BufferCapacity { get; init; } = 10;

    /// <summary>P0: NAK code returned when CFh is sent to a full buffer.</summary>
    public byte BufferFullErrorCode { get; init; } = X1ErrorCodes.ParameterOutOfRange;

    /// <summary>P0: whether CFh is rejected (11h) when no message is selected on the station.</summary>
    public bool RequireMessageForDynamicVars { get; init; } = true;

    /// <summary>P0: what a trigger does when the buffer is empty.</summary>
    public EmptyBufferBehavior OnEmptyBuffer { get; init; } = EmptyBufferBehavior.PrintBlank;

    /// <summary>P0: whether replies echo the request transaction id (otherwise 0).</summary>
    public bool EchoTransactionId { get; init; } = true;

    /// <summary>45h block size: 68 (fw ≥ V1.0.24) or 56.</summary>
    public int StatusBlockSize { get; init; } = 68;

    public string PrinterName { get; init; } = "X1-SIMULATOR";
    public ProtocolVersion Version { get; init; } = new(1, 0, 24);

    /// <summary>When set, every printing line/station with a message is triggered at this interval.</summary>
    public TimeSpan? AutoTriggerInterval { get; init; }
}

public sealed record PrintedItem(
    ushort Line,
    ushort Station,
    string? Message,
    IReadOnlyList<string> Values,
    bool FromBuffer,
    long ItemCounter,
    DateTime Time);

/// <summary>
/// In-process fake X1/SPH speaking the Anser protocol on TCP. Used for development without a
/// printer and for automated tests (including fault injection: dropped replies, delays, split
/// TCP segments, unsolicited frames, alarms, forced disconnects).
/// A "trigger" stands in for the photo-eye: it prints one item from the CFh buffer.
/// </summary>
public sealed class X1Simulator : IAsyncDisposable
{
    private readonly X1SimulatorOptions _o;
    private readonly object _gate = new();
    private readonly Dictionary<ushort, LineState> _lines = new();
    private readonly Dictionary<string, string[]> _tables = new();
    private readonly List<PrintedItem> _printed = new();
    private readonly List<ClientConn> _clients = new();
    private readonly CancellationTokenSource _cts = new();
    private TcpListener? _listener;
    private Task? _acceptLoop;
    private Task? _autoLoop;
    private AlarmStatus _alarm = new(InkAlarm.NoError, InkAlarm.NoError);
    private int _dropResponses;
    private TimeSpan _responseDelay;
    private bool _splitResponses;
    private DateTime _clock = DateTime.Now;

    public X1Simulator(X1SimulatorOptions? options = null)
    {
        _o = options ?? new X1SimulatorOptions();
    }

    public int Port { get; private set; }
    public X1SimulatorOptions Options => _o;

    /// <summary>Raised after each simulated print (on the triggering thread).</summary>
    public event Action<PrintedItem>? Printed;

    /// <summary>Raised for every received request (for console logging).</summary>
    public event Action<AnserFrame>? RequestReceived;

    public void Start()
    {
        _listener = new TcpListener(_o.Address, _o.Port);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _acceptLoop = Task.Run(AcceptLoopAsync);
        if (_o.AutoTriggerInterval is { } iv)
            _autoLoop = Task.Run(() => AutoTriggerLoopAsync(iv));
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        try { _listener?.Stop(); } catch { /* ignore */ }
        DisconnectClients();
        foreach (var t in new[] { _acceptLoop, _autoLoop })
            if (t != null)
                try { await t.ConfigureAwait(false); } catch { /* ignore */ }
        _cts.Dispose();
    }

    // ---------------------------------------------------------------- test / operator controls

    public void SetAlarm(InkAlarm head0, InkAlarm head1)
    {
        lock (_gate) _alarm = new AlarmStatus(head0, head1);
    }

    /// <summary>Swallow the next n replies (the request is still executed).</summary>
    public void DropNextResponses(int n) => Interlocked.Exchange(ref _dropResponses, n);

    public TimeSpan ResponseDelay
    {
        get { lock (_gate) return _responseDelay; }
        set { lock (_gate) _responseDelay = value; }
    }

    /// <summary>Send replies one byte at a time to exercise TCP reassembly.</summary>
    public bool SplitResponses
    {
        get { lock (_gate) return _splitResponses; }
        set { lock (_gate) _splitResponses = value; }
    }

    public void DisconnectClients()
    {
        List<ClientConn> cs;
        lock (_gate) { cs = _clients.ToList(); _clients.Clear(); }
        foreach (var c in cs) c.Close();
    }

    public async Task SendUnsolicitedAsync(byte command, byte[] data)
    {
        List<ClientConn> cs;
        lock (_gate) cs = _clients.ToList();
        var bytes = new AnserFrame(command, data).Encode();
        foreach (var c in cs) await c.WriteAsync(bytes, false, _cts.Token).ConfigureAwait(false);
    }

    public bool IsPrinting(ushort line)
    {
        lock (_gate) return _lines.TryGetValue(line, out var l) && l.Printing;
    }

    public string? SelectedMessage(ushort line, ushort station)
    {
        lock (_gate) return Station(line, station).Message;
    }

    public int BufferCount(ushort line, ushort station)
    {
        lock (_gate) return Station(line, station).Buffer.Count;
    }

    public long ItemCounter(ushort line)
    {
        lock (_gate) return Line(line).ItemCounter;
    }

    public IReadOnlyList<PrintedItem> PrintedItems
    {
        get { lock (_gate) return _printed.ToList(); }
    }

    public IReadOnlyDictionary<string, string[]> Tables
    {
        get { lock (_gate) return new Dictionary<string, string[]>(_tables); }
    }

    /// <summary>Simulate the photo-eye: print one item on line/station. Returns null when nothing printed.</summary>
    public PrintedItem? Trigger(ushort line, ushort station = 1)
    {
        PrintedItem item;
        lock (_gate)
        {
            var l = Line(line);
            var s = Station(line, station);
            if (!l.Printing || s.Message == null) return null;

            string[] values;
            bool fromBuffer;
            if (s.Buffer.Count > 0)
            {
                values = s.Buffer.Dequeue();
                fromBuffer = true;
            }
            else if (_o.OnEmptyBuffer == EmptyBufferBehavior.PrintBlank)
            {
                values = new string[X1Requests.DynamicVarCount];
                Array.Fill(values, "");
                fromBuffer = false;
            }
            else
            {
                return null;
            }

            l.ItemCounter++;
            l.PrintCount++;
            l.TotalCount++;
            item = new PrintedItem(line, station, s.Message, values, fromBuffer, l.ItemCounter, DateTime.Now);
            _printed.Add(item);
        }
        Printed?.Invoke(item);
        return item;
    }

    // ---------------------------------------------------------------- networking

    private async Task AcceptLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            TcpClient tcp;
            try { tcp = await _listener!.AcceptTcpClientAsync(_cts.Token).ConfigureAwait(false); }
            catch { return; }
            tcp.NoDelay = true;
            var conn = new ClientConn(tcp);
            lock (_gate) _clients.Add(conn);
            _ = Task.Run(() => ServeAsync(conn));
        }
    }

    private async Task ServeAsync(ClientConn conn)
    {
        var decoder = new FrameDecoder();
        var buf = new byte[8192];
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                int n = await conn.Stream.ReadAsync(buf, _cts.Token).ConfigureAwait(false);
                if (n == 0) break;
                decoder.Append(buf.AsSpan(0, n));
                while (decoder.TryReadFrame() is { } req)
                {
                    RequestReceived?.Invoke(req);
                    var reply = Handle(req);
                    if (Interlocked.Decrement(ref _dropResponses) >= 0) continue;
                    Interlocked.Exchange(ref _dropResponses, 0);

                    TimeSpan delay;
                    bool split;
                    lock (_gate) { delay = _responseDelay; split = _splitResponses; }
                    if (delay > TimeSpan.Zero) await Task.Delay(delay, _cts.Token).ConfigureAwait(false);

                    ushort tid = _o.EchoTransactionId ? req.TransactionId : (ushort)0;
                    var bytes = new AnserFrame(reply.Command, reply.Data, tid).Encode();
                    await conn.WriteAsync(bytes, split, _cts.Token).ConfigureAwait(false);
                }
            }
        }
        catch
        {
            // client gone / protocol error -> drop connection
        }
        finally
        {
            lock (_gate) _clients.Remove(conn);
            conn.Close();
        }
    }

    private async Task AutoTriggerLoopAsync(TimeSpan interval)
    {
        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(_cts.Token).ConfigureAwait(false))
            {
                List<(ushort, ushort)> targets;
                lock (_gate)
                    targets = _lines.Where(l => l.Value.Printing)
                        .SelectMany(l => l.Value.Stations.Where(s => s.Value.Message != null).Select(s => (l.Key, s.Key)))
                        .ToList();
                foreach (var (line, station) in targets) Trigger(line, station);
            }
        }
        catch (OperationCanceledException) { }
    }

    // ---------------------------------------------------------------- command handling

    private readonly record struct Reply(byte Command, byte[] Data);

    private static Reply Ack() => new(AnserCommand.Ack, Array.Empty<byte>());
    private static Reply Nak(byte code) => new(AnserCommand.Nak, new[] { code });

    private Reply Handle(AnserFrame f)
    {
        var d = f.Data.Span;
        try
        {
            lock (_gate)
            {
                switch (f.Command)
                {
                    case AnserCommand.GetProtocolVersion:
                        return new(f.Command, new[] { _o.Version.Major, _o.Version.Minor, _o.Version.Patch });
                    case AnserCommand.GetAlarmStatus:
                        return new(f.Command, new[] { (byte)_alarm.Head0, (byte)_alarm.Head1 });
                    case AnserCommand.GetPrintingStatus:
                        return new(f.Command, BuildPrintingStatus());
                    case AnserCommand.GetPrinterName:
                        return new(f.Command, Encoding.ASCII.GetBytes(_o.PrinterName));
                    case AnserCommand.GetSystemClock:
                        return new(f.Command, new PayloadWriter().U16((ushort)_clock.Year).U8((byte)_clock.Month)
                            .U8((byte)_clock.Day).U8((byte)_clock.Hour).U8((byte)_clock.Minute).U8((byte)_clock.Second).ToArray());
                    case AnserCommand.SetSystemClock:
                        _clock = X1Responses.ParseSystemClock(d);
                        return Ack();
                    case AnserCommand.GetCounter:
                        return HandleGetCounter(d);
                    case AnserCommand.Extended:
                        return HandleExtended(f);
                    case AnserCommand.SetDynamicVarBuffer:
                        return HandleSetDynamicVarBuffer(d);
                    case AnserCommand.SetDynamicVarTable:
                        return HandleSetDynamicVarTable(d);
                    case AnserCommand.ClearDynamicVarBuffer:
                    {
                        var (line, station) = LineStation(d);
                        Station(line, station).Buffer.Clear();
                        return Ack();
                    }
                    case AnserCommand.ReadDynamicVarBufferStatus:
                    {
                        var (line, station) = LineStation(d);
                        return new(f.Command, new PayloadWriter().U16((ushort)Station(line, station).Buffer.Count).ToArray());
                    }
                    default:
                        return Nak(X1ErrorCodes.InvalidOperation);
                }
            }
        }
        catch (Exception ex) when (ex is ArgumentException or IndexOutOfRangeException or AnserProtocolException)
        {
            return Nak(X1ErrorCodes.InvalidContent);
        }
    }

    private Reply HandleExtended(AnserFrame f)
    {
        var d = f.Data.Span;
        ushort sub = BinaryPrimitives.ReadUInt16BigEndian(d);
        var p = d[2..];
        switch (sub)
        {
            case AnserSubCommand.SetProductionLineOperation:
            {
                ushort line = BinaryPrimitives.ReadUInt16BigEndian(p);
                bool start = p[2] == 0;
                var l = Line(line);
                if (!start)
                {
                    l.Printing = false;
                    return Ack();
                }
                if (!_alarm.IsOk) return Nak(X1ErrorCodes.InkNotReady);
                if (!l.Stations.Values.Any(s => s.Message != null)) return Nak(X1ErrorCodes.SelectMessageBeforePrint);
                l.Printing = true;
                l.PrintCount = 0;
                return Ack();
            }
            case AnserSubCommand.SetPrintMessage:
            {
                var (line, station) = LineStation(p);
                if (p.Length < 4 + X1Requests.MessageFileNameWidth) return Nak(X1ErrorCodes.InvalidContent);
                string name = Encoding.ASCII.GetString(p.Slice(4, X1Requests.MessageFileNameWidth)).TrimEnd('\0');
                if (Line(line).Printing) return Nak(X1ErrorCodes.IncorrectLineStatus);
                if (name.Length == 0) return Nak(X1ErrorCodes.StationFileNotFound);
                Station(line, station).Message = name;
                return Ack();
            }
            case AnserSubCommand.SetPrintMessageWhilePrint:
            {
                var (line, station) = LineStation(p);
                int len = BinaryPrimitives.ReadUInt16BigEndian(p[4..]);
                string name = Encoding.UTF8.GetString(p.Slice(6, len));
                if (name.Length == 0) return Nak(X1ErrorCodes.StationFileNotFound);
                Station(line, station).Message = name;
                return Ack();
            }
            default:
                return Nak(X1ErrorCodes.InvalidOperation);
        }
    }

    private Reply HandleSetDynamicVarBuffer(ReadOnlySpan<byte> d)
    {
        var (line, station) = LineStation(d);
        var values = ReadVars(d[4..]);
        var s = Station(line, station);
        if (_o.RequireMessageForDynamicVars && s.Message == null) return Nak(X1ErrorCodes.SelectMessageBeforePrint);
        if (s.Buffer.Count >= _o.BufferCapacity) return Nak(_o.BufferFullErrorCode);
        s.Buffer.Enqueue(values);
        return Ack();
    }

    private Reply HandleSetDynamicVarTable(ReadOnlySpan<byte> d)
    {
        int fnLen = d[0];
        string name = Encoding.UTF8.GetString(d.Slice(1, fnLen));
        _tables[name] = ReadVars(d[(1 + fnLen)..]);
        return Ack();
    }

    private Reply HandleGetCounter(ReadOnlySpan<byte> d)
    {
        ushort msgNo = BinaryPrimitives.ReadUInt16BigEndian(d);
        byte type = d[2];
        long value = 0;
        foreach (var l in _lines.Values)
            if (l.Stations.Values.Any(s => MessageNumber(s.Message) == msgNo))
                value = l.ItemCounter;
        if (type is < 1 or > 3) return Nak(X1ErrorCodes.ParameterOutOfRange);
        return new(AnserCommand.GetCounter, new PayloadWriter()
            .U16(msgNo).U8(type).U32((uint)value).U32(999_999_999).U32(0).U8(9).U32(1).U8(0).ToArray());
    }

    private byte[] BuildPrintingStatus()
    {
        var printing = _lines.Where(l => l.Value.Printing).OrderBy(l => l.Key).ToList();
        var w = new PayloadWriter().U8((byte)printing.Count);
        foreach (var (id, l) in printing)
        {
            ushort station = l.Stations.Where(s => s.Value.Message != null).Select(s => s.Key).DefaultIfEmpty((ushort)1).First();
            int printables = l.Stations.Values.Sum(s => s.Buffer.Count);
            w.U16(id).U16(station)
             .U32((uint)l.ItemCounter).U32(0).U32(0)          // item / box / lot (low)
             .U32(l.PrintCount).U32(l.TotalCount)
             .U32((uint)printables)
             .U32(0).U32(0).U32(0).U32(0).U32(0).U32(0).U32(0); // ppm, speed, rsv, time, rsv, total printable, idle
            if (_o.StatusBlockSize == 68)
                w.U32((uint)(l.ItemCounter >> 32)).U32(0).U32(0);
        }
        return w.ToArray();
    }

    private static string[] ReadVars(ReadOnlySpan<byte> d)
    {
        var values = new string[X1Requests.DynamicVarCount];
        int off = 0;
        for (int i = 0; i < values.Length; i++)
        {
            int len = d[off];
            values[i] = Encoding.UTF8.GetString(d.Slice(off + 1, len));
            off += 1 + len;
        }
        if (off != d.Length) throw new ArgumentException("Trailing bytes after 20 strings.");
        return values;
    }

    private static (ushort, ushort) LineStation(ReadOnlySpan<byte> d) =>
        (BinaryPrimitives.ReadUInt16BigEndian(d), BinaryPrimitives.ReadUInt16BigEndian(d[2..]));

    private static int? MessageNumber(string? file)
    {
        if (file == null) return null;
        var digits = new string(file.Where(char.IsAsciiDigit).ToArray());
        return int.TryParse(digits, out var n) ? n : null;
    }

    private LineState Line(ushort id)
    {
        if (!_lines.TryGetValue(id, out var l)) _lines[id] = l = new LineState();
        return l;
    }

    private StationState Station(ushort line, ushort station)
    {
        var l = Line(line);
        if (!l.Stations.TryGetValue(station, out var s)) l.Stations[station] = s = new StationState();
        return s;
    }

    private sealed class LineState
    {
        public bool Printing;
        public long ItemCounter;
        public uint PrintCount;
        public uint TotalCount;
        public readonly Dictionary<ushort, StationState> Stations = new();
    }

    private sealed class StationState
    {
        public string? Message;
        public readonly Queue<string[]> Buffer = new();
    }

    private sealed class ClientConn(TcpClient tcp)
    {
        private readonly SemaphoreSlim _write = new(1, 1);
        public NetworkStream Stream { get; } = tcp.GetStream();

        public async Task WriteAsync(byte[] bytes, bool split, CancellationToken ct)
        {
            await _write.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (!split)
                {
                    await Stream.WriteAsync(bytes, ct).ConfigureAwait(false);
                    return;
                }
                for (int i = 0; i < bytes.Length; i++)
                {
                    await Stream.WriteAsync(bytes.AsMemory(i, 1), ct).ConfigureAwait(false);
                    await Stream.FlushAsync(ct).ConfigureAwait(false);
                    if (i % 4 == 3) await Task.Delay(1, ct).ConfigureAwait(false);
                }
            }
            finally
            {
                _write.Release();
            }
        }

        public void Close()
        {
            try { tcp.Client.Shutdown(SocketShutdown.Both); } catch { /* ignore */ }
            tcp.Dispose();
        }
    }
}
