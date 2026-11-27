using System.Buffers.Binary;

namespace Mitsu.Printer.Protocol;

/// <summary>
/// Reassembles frames from a TCP byte stream using the Length field, so split
/// and concatenated TCP segments are handled (the vendor SDK assumed one
/// socket read == one frame, see 01_X1_protocol.md §6.4).
/// Not thread-safe; owned by a single reader.
/// </summary>
public sealed class FrameDecoder
{
    private readonly int _maxFrameSize;
    private byte[] _buf = new byte[8192];
    private int _count;

    public FrameDecoder(int maxFrameSize = 65535 + 6)
    {
        _maxFrameSize = maxFrameSize;
    }

    public int BufferedBytes => _count;

    public void Append(ReadOnlySpan<byte> data)
    {
        if (_count + data.Length > _buf.Length)
        {
            var nb = new byte[Math.Max(_buf.Length * 2, _count + data.Length)];
            Buffer.BlockCopy(_buf, 0, nb, 0, _count);
            _buf = nb;
        }
        data.CopyTo(_buf.AsSpan(_count));
        _count += data.Length;
    }

    /// <summary>Returns the next complete frame, or null if more bytes are needed.</summary>
    /// <exception cref="AnserProtocolException">Stream is corrupt; the connection should be reset.</exception>
    public AnserFrame? TryReadFrame()
    {
        if (_count < 6) return null;
        var s = _buf.AsSpan(0, _count);

        ushort protocolId = BinaryPrimitives.ReadUInt16BigEndian(s[2..]);
        int length = BinaryPrimitives.ReadUInt16BigEndian(s[4..]);
        if (protocolId != 0)
            throw new AnserProtocolException($"Unexpected protocol id {protocolId:X4}.");
        if (length < 6)
            throw new AnserProtocolException($"Length field {length} is shorter than the minimum 6.");
        int total = 6 + length;
        if (total > _maxFrameSize)
            throw new AnserProtocolException($"Frame of {total} bytes exceeds limit {_maxFrameSize}.");
        if (_count < total) return null;

        var f = s[..total];
        ushort tid = BinaryPrimitives.ReadUInt16BigEndian(f);
        byte unit = f[6];
        byte fc = f[7];
        int pduLen = BinaryPrimitives.ReadUInt16BigEndian(f[8..]);
        if (fc != AnserFrame.FunctionCode)
            throw new AnserProtocolException($"Unexpected function code {fc:X2}h.");
        if (pduLen != length - 4)
            throw new AnserProtocolException($"PDU length {pduLen} inconsistent with Length {length}.");

        byte st = f[10];
        byte cmd = f[11];
        byte[] data = f[12..].ToArray();

        // shift remaining bytes to the front
        int rest = _count - total;
        if (rest > 0) Buffer.BlockCopy(_buf, total, _buf, 0, rest);
        _count = rest;

        return new AnserFrame(cmd, data, tid, unit, st);
    }

    public void Reset() => _count = 0;
}

public sealed class AnserProtocolException : Exception
{
    public AnserProtocolException(string message) : base(message) { }
}
