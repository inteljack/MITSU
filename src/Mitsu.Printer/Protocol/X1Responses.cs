using System.Buffers.Binary;
using System.Text;

namespace Mitsu.Printer.Protocol;

public readonly record struct ProtocolVersion(byte Major, byte Minor, byte Patch)
{
    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}

/// <summary>7Fh: alarm code per print head (0 = no error).</summary>
public readonly record struct AlarmStatus(InkAlarm Head0, InkAlarm Head1)
{
    public bool IsOk => Head0 == InkAlarm.NoError && Head1 == InkAlarm.NoError;
}

public enum InkAlarm : byte
{
    NoError = 0,
    InkLow = 1,
    InkEmpty = 2,
    InkTypeNotSupported = 3,
    InkColorNotSupported = 4,
    EdcNotMatch = 5,
    NoCartridge = 6,
    GateNotClosed = 7,
    DiscError = 8,
    Error = 9,
}

/// <summary>One block of the 45h reply (one printing production line).</summary>
public sealed record LinePrintingStatus(
    ushort Line,
    ushort Station,
    long ItemCounter,
    long BoxCounter,
    long LotCounter,
    uint PrintCount,
    uint PrintTotalCount,
    int Printables,
    uint PrintsPerMinute,
    uint LineSpeed,
    uint AccumulatedPrintingTime,
    uint TotalPrintable,
    uint CartridgeIdleTime);

/// <summary>45h reply. Only lines that are currently printing are reported.</summary>
public sealed record PrintingStatus(IReadOnlyList<LinePrintingStatus> Lines, int BlockSize);

public sealed record CounterInfo(ushort MessageNo, CounterType Type, int Value, int Max, int Min, byte Digits, int Step, byte Format);

public static class X1Responses
{
    public static ProtocolVersion ParseProtocolVersion(ReadOnlySpan<byte> d)
    {
        Require(d, 3, "43h");
        return new ProtocolVersion(d[0], d[1], d[2]);
    }

    public static AlarmStatus ParseAlarmStatus(ReadOnlySpan<byte> d)
    {
        Require(d, 2, "7Fh");
        return new AlarmStatus((InkAlarm)d[0], (InkAlarm)d[1]);
    }

    /// <summary>
    /// 45h. Block size is derived from the reply length: 56 bytes (before fw V1.0.24)
    /// or 68 bytes (with high parts of the counters). The vendor SDK hard-coded offsets
    /// that overlap; we compute them instead (01_X1_protocol.md §5.1).
    /// </summary>
    public static PrintingStatus ParsePrintingStatus(ReadOnlySpan<byte> d)
    {
        Require(d, 1, "45h");
        int n = d[0];
        if (n == 0) return new PrintingStatus(Array.Empty<LinePrintingStatus>(), 0);
        int body = d.Length - 1;
        if (body % n != 0)
            throw new AnserProtocolException($"45h: {body} bytes is not a multiple of {n} lines.");
        int bs = body / n;
        if (bs != 56 && bs != 68)
            throw new AnserProtocolException($"45h: unexpected block size {bs} (expected 56 or 68).");

        var list = new List<LinePrintingStatus>(n);
        for (int i = 0; i < n; i++)
        {
            var b = d.Slice(1 + i * bs, bs);

            list.Add(new LinePrintingStatus(
                Line: BinaryPrimitives.ReadUInt16BigEndian(b),
                Station: BinaryPrimitives.ReadUInt16BigEndian(b[2..]),
                ItemCounter: Wide(b, 4, 56),
                BoxCounter: Wide(b, 8, 60),
                LotCounter: Wide(b, 12, 64),
                PrintCount: BinaryPrimitives.ReadUInt32BigEndian(b[16..]),
                PrintTotalCount: BinaryPrimitives.ReadUInt32BigEndian(b[20..]),
                Printables: BinaryPrimitives.ReadInt32BigEndian(b[24..]),
                PrintsPerMinute: BinaryPrimitives.ReadUInt32BigEndian(b[28..]),
                LineSpeed: BinaryPrimitives.ReadUInt32BigEndian(b[32..]),
                AccumulatedPrintingTime: BinaryPrimitives.ReadUInt32BigEndian(b[40..]),
                TotalPrintable: BinaryPrimitives.ReadUInt32BigEndian(b[48..]),
                CartridgeIdleTime: BinaryPrimitives.ReadUInt32BigEndian(b[52..])));
        }
        return new PrintingStatus(list, bs);
    }

    /// <summary>68-byte blocks carry a high 32-bit part for the counters; 56-byte blocks are int32 only.</summary>
    private static long Wide(ReadOnlySpan<byte> b, int lowOff, int highOff) =>
        b.Length >= 68
            ? ((long)BinaryPrimitives.ReadUInt32BigEndian(b[highOff..]) << 32) | BinaryPrimitives.ReadUInt32BigEndian(b[lowOff..])
            : BinaryPrimitives.ReadInt32BigEndian(b[lowOff..]);

    public static CounterInfo ParseCounter(ReadOnlySpan<byte> d)
    {
        Require(d, 21, "3Ah");
        return new CounterInfo(
            BinaryPrimitives.ReadUInt16BigEndian(d),
            (CounterType)d[2],
            BinaryPrimitives.ReadInt32BigEndian(d[3..]),
            BinaryPrimitives.ReadInt32BigEndian(d[7..]),
            BinaryPrimitives.ReadInt32BigEndian(d[11..]),
            d[15],
            BinaryPrimitives.ReadInt32BigEndian(d[16..]),
            d[20]);
    }

    /// <summary>CCh: one int16, believed to be the number of queued entries (unverified).</summary>
    public static short ParseBufferStatus(ReadOnlySpan<byte> d)
    {
        Require(d, 2, "CCh");
        return BinaryPrimitives.ReadInt16BigEndian(d);
    }

    public static DateTime ParseSystemClock(ReadOnlySpan<byte> d)
    {
        Require(d, 7, "34h");
        return new DateTime(BinaryPrimitives.ReadUInt16BigEndian(d), d[2], d[3], d[4], d[5], d[6]);
    }

    public static string ParseAscii(ReadOnlySpan<byte> d) => Encoding.ASCII.GetString(d).TrimEnd('\0');

    private static void Require(ReadOnlySpan<byte> d, int len, string what)
    {
        if (d.Length < len)
            throw new AnserProtocolException($"{what} reply has {d.Length} bytes; expected at least {len}.");
    }
}
