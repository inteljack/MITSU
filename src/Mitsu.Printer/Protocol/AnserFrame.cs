using System.Buffers.Binary;

namespace Mitsu.Printer.Protocol;

/// <summary>
/// One Anser frame (ADU) as used by NexGen X1 / SPH on TCP port 25001.
/// <code>
/// off len field
///  0   2  Transaction ID (BE)
///  2   2  Protocol ID = 0
///  4   2  Length (BE) = 6 + N   (bytes after this field)
///  6   1  Unit ID (SDK: PrinterID, always 0)
///  7   1  Function code, always 0x64
///  8   2  PDU length (BE) = 2 + N
/// 10   1  ST (0 when sending; meaning of non-zero in replies unknown)
/// 11   1  CMD (Anser command byte: 0x20, 0xCF, ...)
/// 12   N  data
/// </code>
/// It looks like Modbus TCP but is not: the PDU can be several KB, beyond the
/// Modbus 253-byte limit, so standard Modbus libraries cannot be used.
/// See analysis/01_X1_protocol.md §1.2.
/// </summary>
public sealed class AnserFrame
{
    public const int HeaderSize = 12;
    public const byte FunctionCode = 0x64;

    public AnserFrame(byte command, ReadOnlyMemory<byte> data, ushort transactionId = 0, byte unitId = 0, byte status = 0)
    {
        Command = command;
        Data = data;
        TransactionId = transactionId;
        UnitId = unitId;
        Status = status;
    }

    public ushort TransactionId { get; }
    public byte UnitId { get; }
    public byte Status { get; }
    public byte Command { get; }
    public ReadOnlyMemory<byte> Data { get; }

    public int EncodedLength => HeaderSize + Data.Length;

    public byte[] Encode()
    {
        int n = Data.Length;
        if (n > ushort.MaxValue - 6)
            throw new ArgumentException($"Frame data too long ({n} bytes).");

        var buf = new byte[HeaderSize + n];
        var s = buf.AsSpan();
        BinaryPrimitives.WriteUInt16BigEndian(s[0..], TransactionId);
        BinaryPrimitives.WriteUInt16BigEndian(s[2..], 0);
        BinaryPrimitives.WriteUInt16BigEndian(s[4..], (ushort)(6 + n));
        s[6] = UnitId;
        s[7] = FunctionCode;
        BinaryPrimitives.WriteUInt16BigEndian(s[8..], (ushort)(2 + n));
        s[10] = Status;
        s[11] = Command;
        Data.Span.CopyTo(s[12..]);
        return buf;
    }

    /// <summary>Sub-command (first two data bytes, BE) for 0x20 extended commands, or null.</summary>
    public ushort? SubCommand =>
        Command == AnserCommand.Extended && Data.Length >= 2
            ? BinaryPrimitives.ReadUInt16BigEndian(Data.Span)
            : null;

    public override string ToString() =>
        $"CMD={Command:X2}h TID={TransactionId} len={Data.Length}";

    public static string ToHex(ReadOnlySpan<byte> bytes) => Convert.ToHexString(bytes);
}
