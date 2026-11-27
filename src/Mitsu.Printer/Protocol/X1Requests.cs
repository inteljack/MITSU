namespace Mitsu.Printer.Protocol;

/// <summary>
/// Pure builders for request payloads (CMD + data). No I/O, so they are unit-testable
/// against the hex examples in analysis/01_X1_protocol.md. All validation happens here,
/// before anything is queued or written to the socket.
/// </summary>
public static class X1Requests
{
    /// <summary>Number of strings CAh / CFh carry; the printer requires exactly 20.</summary>
    public const int DynamicVarCount = 20;
    public const int MessageFileNameWidth = 256;

    public readonly record struct Request(byte Command, byte[] Data, ushort? SubCommand = null);

    public static Request Simple(byte command) => new(command, Array.Empty<byte>());

    /// <summary>20h+203h. The printer's flag is inverted: 0 = start, 1 = stop.</summary>
    public static Request SetLineOperation(ushort line, bool start) =>
        Extended(AnserSubCommand.SetProductionLineOperation,
            new PayloadWriter().U16(line).U8(start ? (byte)0 : (byte)1));

    /// <summary>20h+214h select message (printer stopped). File name is ASCII, zero-padded to 256 bytes.</summary>
    public static Request SetPrintMessage(ushort line, ushort station, string messageFile) =>
        Extended(AnserSubCommand.SetPrintMessage,
            new PayloadWriter().U16(line).U16(station)
                .AsciiFixed(messageFile, MessageFileNameWidth, "Message file name"));

    /// <summary>20h+24Ch change message while printing. UTF-8 with a u16 length prefix (SDK limits to 255).</summary>
    public static Request SetPrintMessageWhilePrint(ushort line, ushort station, string messageFile)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(messageFile);
        if (bytes.Length > 255) throw new ArgumentException("Message file name exceeds 255 UTF-8 bytes.");
        return Extended(AnserSubCommand.SetPrintMessageWhilePrint,
            new PayloadWriter().U16(line).U16(station).U16((ushort)bytes.Length).Bytes(bytes));
    }

    /// <summary>CFh push 20 strings into the line/station dynamic-variable buffer.</summary>
    public static Request SetDynamicVarBuffer(ushort line, ushort station, IReadOnlyList<string> values)
    {
        var w = new PayloadWriter().U16(line).U16(station);
        WriteVars(w, values);
        return new Request(AnserCommand.SetDynamicVarBuffer, w.ToArray());
    }

    /// <summary>CAh overwrite the 20 strings of a variable table identified by file name.</summary>
    public static Request SetDynamicVarTable(string tableFile, IReadOnlyList<string> values)
    {
        var w = new PayloadWriter().Utf8Len8(tableFile, "Table file name");
        WriteVars(w, values);
        return new Request(AnserCommand.SetDynamicVarTable, w.ToArray());
    }

    public static Request ClearDynamicVarBuffer(ushort line, ushort station) =>
        new(AnserCommand.ClearDynamicVarBuffer, new PayloadWriter().U16(line).U16(station).ToArray());

    public static Request ReadDynamicVarBufferStatus(ushort line, ushort station) =>
        new(AnserCommand.ReadDynamicVarBufferStatus, new PayloadWriter().U16(line).U16(station).ToArray());

    /// <summary>3Ah read a counter of a message (1 = item, 2 = box, 3 = lot).</summary>
    public static Request GetCounter(ushort messageNo, CounterType type) =>
        new(AnserCommand.GetCounter, new PayloadWriter().U16(messageNo).U8((byte)type).ToArray());

    public static Request SetSystemClock(DateTime t) =>
        new(AnserCommand.SetSystemClock, new PayloadWriter()
            .U16((ushort)t.Year).U8((byte)t.Month).U8((byte)t.Day)
            .U8((byte)t.Hour).U8((byte)t.Minute).U8((byte)t.Second).ToArray());

    private static void WriteVars(PayloadWriter w, IReadOnlyList<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count > DynamicVarCount)
            throw new ArgumentException($"At most {DynamicVarCount} dynamic variables; got {values.Count}.");
        for (int i = 0; i < DynamicVarCount; i++)
        {
            // Missing trailing values are sent as empty strings. Whether the printer treats
            // an empty string as "clear" or "unchanged" is unverified (P0 item).
            string v = i < values.Count ? values[i] ?? "" : "";
            w.Utf8Len8(v, $"String{i + 1}");
        }
    }

    private static Request Extended(ushort sub, PayloadWriter rest)
    {
        var data = new PayloadWriter().U16(sub).Bytes(rest.ToArray()).ToArray();
        return new Request(AnserCommand.Extended, data, sub);
    }
}

public enum CounterType : byte
{
    Item = 1,
    Box = 2,
    Lot = 3,
}
