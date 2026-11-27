using System.Buffers.Binary;
using System.Text;

namespace Mitsu.Printer.Protocol;

/// <summary>Small big-endian payload builder.</summary>
internal sealed class PayloadWriter
{
    private readonly List<byte> _b = new();

    public PayloadWriter U8(byte v) { _b.Add(v); return this; }

    public PayloadWriter U16(ushort v)
    {
        Span<byte> t = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(t, v);
        _b.Add(t[0]); _b.Add(t[1]);
        return this;
    }

    public PayloadWriter U32(uint v)
    {
        Span<byte> t = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(t, v);
        foreach (var x in t) _b.Add(x);
        return this;
    }

    public PayloadWriter U64(ulong v)
    {
        Span<byte> t = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(t, v);
        foreach (var x in t) _b.Add(x);
        return this;
    }

    public PayloadWriter Bytes(ReadOnlySpan<byte> v) { foreach (var x in v) _b.Add(x); return this; }

    /// <summary>1-byte length prefix + UTF-8 bytes (max 255 bytes).</summary>
    public PayloadWriter Utf8Len8(string s, string what)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        if (bytes.Length > 255)
            throw new ArgumentException($"{what} is {bytes.Length} UTF-8 bytes; max is 255.");
        U8((byte)bytes.Length);
        return Bytes(bytes);
    }

    /// <summary>ASCII, zero-padded to a fixed width.</summary>
    public PayloadWriter AsciiFixed(string s, int width, string what)
    {
        foreach (char c in s)
            if (c > 0x7E || c < 0x20)
                throw new ArgumentException($"{what} must be printable ASCII: '{s}'.");
        if (s.Length > width)
            throw new ArgumentException($"{what} is {s.Length} chars; max is {width}.");
        var bytes = Encoding.ASCII.GetBytes(s);
        Bytes(bytes);
        for (int i = bytes.Length; i < width; i++) _b.Add(0);
        return this;
    }

    public byte[] ToArray() => _b.ToArray();
}
