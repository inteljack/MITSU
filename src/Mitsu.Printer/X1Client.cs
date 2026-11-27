using System.Diagnostics;
using System.Net.Sockets;
using Mitsu.Printer.Protocol;

namespace Mitsu.Printer;

public sealed class X1ClientOptions
{
    public required string Host { get; init; }
    public int Port { get; init; } = 25001;
    public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>Per-request reply timeout. On timeout the connection is closed (pairing can no longer be trusted).</summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>Send 43h when the link has been idle this long. <see cref="TimeSpan.Zero"/> disables.</summary>
    public TimeSpan HeartbeatInterval { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>Largest frame we will send. The vendor SDK's buffer is 5120 bytes.</summary>
    public int MaxFrameSize { get; init; } = 5120;

    /// <summary>
    /// Use incrementing transaction ids and reject replies whose non-zero id does not match.
    /// The vendor SDK always sends 0; whether the printer echoes the id is unverified (P0),
    /// so this is off by default.
    /// </summary>
    public bool UseTransactionIds { get; init; }
}

public enum TrafficDirection { Sent, Received }

public sealed class X1TrafficEventArgs(TrafficDirection direction, byte[] bytes) : EventArgs
{
    public TrafficDirection Direction { get; } = direction;
    public byte[] Bytes { get; } = bytes;
}

public sealed class X1FrameEventArgs(AnserFrame frame) : EventArgs
{
    public AnserFrame Frame { get; } = frame;
}

public sealed class X1DisconnectedEventArgs(string reason, Exception? error) : EventArgs
{
    public string Reason { get; } = reason;
    public Exception? Error { get; } = error;
}

/// <summary>
/// TCP client for NexGen X1 / SPH (Anser protocol on port 25001).
/// <list type="bullet">
/// <item>Strict request/reply: one request in flight, serialised by a semaphore.</item>
/// <item>Every request has a timeout. Timeout, write/read failure or a corrupt stream closes
///       the connection and fails the pending request; nothing can hang forever.</item>
/// <item>Replies are matched by command byte (plus sub-command for 20h). Anything else is
///       raised as <see cref="UnsolicitedFrame"/> and never consumed as a reply.</item>
/// <item>Stream reassembly via <see cref="FrameDecoder"/>.</item>
/// <item>No automatic reconnect: the caller (production controller) decides when to call
///       <see cref="ConnectAsync"/> again, because after a drop the job state must be re-checked.</item>
/// </list>
/// Event handlers run on the thread pool, except <see cref="Traffic"/> which runs inline on the
/// I/O path and must be fast (it is meant for frame logging).
/// </summary>
public sealed partial class X1Client : IX1Printer, IAsyncDisposable
{
    private readonly X1ClientOptions _options;
    private readonly SemaphoreSlim _requestLock = new(1, 1);
    private readonly object _state = new();

    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private CancellationTokenSource? _connCts;
    private Task? _readLoop;
    private Task? _heartbeatLoop;
    private Pending? _pending;
    private int _generation;
    private ushort _nextTid;
    private long _lastActivity;

    public X1Client(X1ClientOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public X1ClientOptions Options => _options;

    public event EventHandler<X1FrameEventArgs>? UnsolicitedFrame;
    public event EventHandler<X1DisconnectedEventArgs>? Disconnected;
    public event EventHandler<X1TrafficEventArgs>? Traffic;

    public bool IsConnected
    {
        get { lock (_state) return _stream != null; }
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        lock (_state)
        {
            if (_stream != null) throw new InvalidOperationException("Already connected.");
        }

        var tcp = new TcpClient { NoDelay = true };
        try
        {
            tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(_options.ConnectTimeout);
            try
            {
                await tcp.ConnectAsync(_options.Host, _options.Port, timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new X1ConnectionException(
                    $"Connect to {_options.Host}:{_options.Port} timed out after {_options.ConnectTimeout.TotalSeconds:0.#} s.");
            }
            catch (SocketException ex)
            {
                throw new X1ConnectionException($"Connect to {_options.Host}:{_options.Port} failed: {ex.Message}", ex);
            }
        }
        catch
        {
            tcp.Dispose();
            throw;
        }

        var stream = tcp.GetStream();
        var cts = new CancellationTokenSource();
        int gen;
        lock (_state)
        {
            if (_stream != null)
            {
                tcp.Dispose();
                throw new InvalidOperationException("Already connected.");
            }
            _tcp = tcp;
            _stream = stream;
            _connCts = cts;
            gen = ++_generation;
            Touch();
        }

        _readLoop = Task.Run(() => ReadLoopAsync(stream, gen, cts.Token));
        if (_options.HeartbeatInterval > TimeSpan.Zero)
            _heartbeatLoop = Task.Run(() => HeartbeatLoopAsync(gen, cts.Token));
    }

    public async Task DisconnectAsync()
    {
        Task? read, hb;
        int gen;
        lock (_state)
        {
            gen = _generation;
            read = _readLoop;
            hb = _heartbeatLoop;
        }
        Fault(gen, "Closed by client.", null, raiseEvent: false);
        await IgnoreErrors(read).ConfigureAwait(false);
        await IgnoreErrors(hb).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        _requestLock.Dispose();
    }

    /// <summary>Send a raw command and wait for its reply (4Fh ACK, data reply, or NAK as exception).</summary>
    public Task<AnserFrame> SendRawAsync(byte command, byte[] data, TimeSpan? timeout = null, CancellationToken ct = default)
    {
        ushort? sub = command == AnserCommand.Extended && data.Length >= 2 ? (ushort)(data[0] << 8 | data[1]) : null;
        return SendAsync(new X1Requests.Request(command, data, sub), timeout, ct);
    }

    internal async Task<AnserFrame> SendAsync(X1Requests.Request req, TimeSpan? timeout = null, CancellationToken ct = default)
    {
        // Validate size before taking the lock / touching the socket.
        int frameLen = AnserFrame.HeaderSize + req.Data.Length;
        if (frameLen > _options.MaxFrameSize)
            throw new ArgumentException($"Frame of {frameLen} bytes exceeds MaxFrameSize {_options.MaxFrameSize}.");

        await _requestLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            NetworkStream stream;
            int gen;
            Pending pending;
            byte[] bytes;
            lock (_state)
            {
                stream = _stream ?? throw new X1ConnectionException("Not connected.");
                gen = _generation;
                ushort tid = _options.UseTransactionIds ? NextTid() : (ushort)0;
                bytes = new AnserFrame(req.Command, req.Data, tid).Encode();
                pending = new Pending(req.Command, req.SubCommand, tid);
                _pending = pending;
            }

            try
            {
                Traffic?.Invoke(this, new X1TrafficEventArgs(TrafficDirection.Sent, bytes));
                await stream.WriteAsync(bytes, ct).ConfigureAwait(false);
                Touch();
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // A partial write would corrupt the stream; drop the link to be safe.
                Fault(gen, "Request cancelled during write.", null);
                throw;
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or SocketException)
            {
                Fault(gen, "Write failed.", ex);
                throw new X1ConnectionException("Write failed: " + ex.Message, ex);
            }

            var limit = timeout ?? _options.RequestTimeout;
            AnserFrame reply;
            try
            {
                reply = await pending.Reply.Task.WaitAsync(limit, ct).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                Fault(gen, $"No reply to {Describe(req)} within {limit.TotalMilliseconds:0} ms.", null);
                throw new X1TimeoutException($"No reply to {Describe(req)} within {limit.TotalMilliseconds:0} ms.");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // The reply may still arrive; we can no longer pair it reliably.
                Fault(gen, $"Request {Describe(req)} cancelled while waiting for reply.", null);
                throw;
            }
            finally
            {
                lock (_state)
                {
                    if (ReferenceEquals(_pending, pending)) _pending = null;
                }
            }

            if (reply.Command == AnserCommand.Nak)
            {
                byte? code = reply.Data.Length > 0 ? reply.Data.Span[0] : null;
                throw new X1NakException(req.Command, req.SubCommand, code);
            }
            return reply;
        }
        finally
        {
            _requestLock.Release();
        }
    }

    private async Task ReadLoopAsync(NetworkStream stream, int gen, CancellationToken token)
    {
        var decoder = new FrameDecoder();
        var buf = new byte[8192];
        try
        {
            while (!token.IsCancellationRequested)
            {
                int n = await stream.ReadAsync(buf, token).ConfigureAwait(false);
                if (n == 0)
                {
                    Fault(gen, "Printer closed the connection.", null);
                    return;
                }
                Touch();
                Traffic?.Invoke(this, new X1TrafficEventArgs(TrafficDirection.Received, buf.AsSpan(0, n).ToArray()));
                decoder.Append(buf.AsSpan(0, n));
                while (decoder.TryReadFrame() is { } frame)
                    Dispatch(frame);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (AnserProtocolException ex)
        {
            Fault(gen, "Protocol error: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
                Fault(gen, "Read failed: " + ex.Message, ex);
        }
    }

    private void Dispatch(AnserFrame frame)
    {
        Pending? p;
        lock (_state) p = _pending;

        if (p != null && p.Matches(frame) && p.Reply.TrySetResult(frame))
            return;

        var handler = UnsolicitedFrame;
        if (handler != null)
            ThreadPool.QueueUserWorkItem(_ => SafeInvoke(() => handler(this, new X1FrameEventArgs(frame))));
    }

    private async Task HeartbeatLoopAsync(int gen, CancellationToken token)
    {
        var interval = _options.HeartbeatInterval;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(100, interval.TotalMilliseconds / 4)));
        try
        {
            while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
            {
                if (Stopwatch.GetElapsedTime(Interlocked.Read(ref _lastActivity)) < interval) continue;
                lock (_state)
                {
                    if (gen != _generation || _stream == null) return;
                }
                try
                {
                    await GetProtocolVersionAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    return;
                }
                catch (X1Exception)
                {
                    // Timeout / connection errors already faulted the link and raised Disconnected.
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void Fault(int gen, string reason, Exception? error, bool raiseEvent = true)
    {
        TcpClient? tcp;
        CancellationTokenSource? cts;
        Pending? pending;
        lock (_state)
        {
            if (gen != _generation || _stream == null) return;
            _generation++;
            tcp = _tcp;
            cts = _connCts;
            pending = _pending;
            _tcp = null;
            _stream = null;
            _connCts = null;
            _pending = null;
        }

        try { cts?.Cancel(); } catch { /* ignore */ }
        try { tcp?.Dispose(); } catch { /* ignore */ }
        cts?.Dispose();
        pending?.Reply.TrySetException(new X1ConnectionException(reason, error));

        var handler = Disconnected;
        if (raiseEvent && handler != null)
            ThreadPool.QueueUserWorkItem(_ => SafeInvoke(() => handler(this, new X1DisconnectedEventArgs(reason, error))));
    }

    private void Touch() => Interlocked.Exchange(ref _lastActivity, Stopwatch.GetTimestamp());

    private ushort NextTid()
    {
        _nextTid++;
        if (_nextTid == 0) _nextTid = 1;
        return _nextTid;
    }

    private static string Describe(X1Requests.Request r) =>
        r.SubCommand is { } s ? $"{r.Command:X2}h+{s:X4}h" : $"{r.Command:X2}h";

    private static void SafeInvoke(Action a)
    {
        try { a(); }
        catch (Exception ex) { Debug.WriteLine("X1Client event handler threw: " + ex); }
    }

    private static async Task IgnoreErrors(Task? t)
    {
        if (t == null) return;
        try { await t.ConfigureAwait(false); } catch { /* ignore */ }
    }

    private sealed class Pending(byte command, ushort? subCommand, ushort tid)
    {
        public TaskCompletionSource<AnserFrame> Reply { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool Matches(AnserFrame f)
        {
            if (tid != 0 && f.TransactionId != 0 && f.TransactionId != tid) return false;
            if (f.Command is AnserCommand.Ack or AnserCommand.Nak) return true;
            if (f.Command != command) return false;
            if (subCommand is { } s && f.SubCommand != s) return false;
            return true;
        }
    }
}
