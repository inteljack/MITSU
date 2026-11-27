namespace Mitsu.Printer;

/// <summary>Base class for printer communication failures.</summary>
public class X1Exception : Exception
{
    public X1Exception(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>Not connected, or the connection dropped while a request was in flight.</summary>
public sealed class X1ConnectionException : X1Exception
{
    public X1ConnectionException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>No reply within the request timeout. The client closes the connection when this happens.</summary>
public sealed class X1TimeoutException : X1Exception
{
    public X1TimeoutException(string message) : base(message) { }
}

/// <summary>The printer answered 31h (NAK) with an error code.</summary>
public sealed class X1NakException : X1Exception
{
    public X1NakException(byte command, ushort? subCommand, byte? errorCode)
        : base(Format(command, subCommand, errorCode))
    {
        Command = command;
        SubCommand = subCommand;
        ErrorCode = errorCode;
    }

    public byte Command { get; }
    public ushort? SubCommand { get; }

    /// <summary>Null when the NAK carried no data byte.</summary>
    public byte? ErrorCode { get; }

    public string Description => X1ErrorCodes.Describe(ErrorCode);

    private static string Format(byte cmd, ushort? sub, byte? code) =>
        $"Printer rejected {cmd:X2}h{(sub is { } s ? $"+{s:X4}h" : "")}: " +
        $"{(code is { } c ? $"0x{c:X2} " : "")}{X1ErrorCodes.Describe(code)}";
}

public static class X1ErrorCodes
{
    public const byte DataLengthBeyond = 0x00;
    public const byte InvalidMessageNo = 0x01;
    public const byte InvalidProductionLine = 0x02;
    public const byte InvalidPrintStation = 0x03;
    public const byte InvalidContent = 0x04;
    public const byte InkNotReady = 0x05;
    public const byte PrintheadInUse = 0x06;
    public const byte IncorrectLineStatus = 0x07;
    public const byte StationFileNotFound = 0x08;
    public const byte ParameterOutOfRange = 0x09;
    public const byte InvalidOperation = 0x10;
    public const byte SelectMessageBeforePrint = 0x11;

    // The SDK table jumps 09h -> 10h. The vendor doc may have meant decimal 10..14
    // (0Ah..0Eh). Both are mapped until verified on real hardware (P0 item).
    private static readonly Dictionary<byte, string> Text = new()
    {
        [0x00] = "Data length beyond",
        [0x01] = "Invalid message no.",
        [0x02] = "Invalid production line index",
        [0x03] = "Invalid print station index",
        [0x04] = "Invalid content",
        [0x05] = "Ink not ready",
        [0x06] = "Printhead already in use by another production line",
        [0x07] = "Incorrect production line status",
        [0x08] = "PrintStation filename not found",
        [0x09] = "Parameter out of range",
        [0x10] = "Invalid operation",
        [0x11] = "Select message before print",
        [0x12] = "Fail to create the file",
        [0x13] = "No file found",
        [0x14] = "File size incorrect",
        [0x0A] = "Invalid operation (if codes are decimal)",
        [0x0B] = "Select message before print (if codes are decimal)",
        [0x0C] = "Fail to create the file (if codes are decimal)",
        [0x0D] = "No file found (if codes are decimal)",
        [0x0E] = "File size incorrect (if codes are decimal)",
    };

    public static string Describe(byte? code) =>
        code is null ? "no error code" : Text.TryGetValue(code.Value, out var t) ? t : "unknown error code";
}
