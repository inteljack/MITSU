namespace Mitsu.Printer.Protocol;

/// <summary>Anser command bytes (the CMD byte inside the 0x64 PDU).</summary>
public static class AnserCommand
{
    public const byte Extended = 0x20;                 // + 2-byte sub-command
    public const byte PrintCompletedReport = 0x30;      // printer-initiated? (unverified)
    public const byte Nak = 0x31;                       // data[0] = error code
    public const byte GetMessageNo = 0x32;
    public const byte GetIpAddress = 0x33;
    public const byte GetSystemClock = 0x34;
    public const byte SetSystemClock = 0x35;
    public const byte GetCounter = 0x3A;
    public const byte GetPrinterInformation = 0x42;
    public const byte GetProtocolVersion = 0x43;
    public const byte GetPrintingStatus = 0x45;
    public const byte Ack = 0x4F;
    public const byte GetAlarmStatus = 0x7F;
    public const byte GetPrinterStatus = 0xA7;
    public const byte SetDynamicVarTable = 0xCA;        // file name + 20 strings
    public const byte ClearDynamicVarBuffer = 0xCB;     // line + station
    public const byte ReadDynamicVarBufferStatus = 0xCC; // line + station -> int16
    public const byte SetDynamicVarBuffer = 0xCF;       // line + station + 20 strings
    public const byte GetPrinterName = 0xD3;
}

/// <summary>Sub-commands of <see cref="AnserCommand.Extended"/> (0x20).</summary>
public static class AnserSubCommand
{
    public const ushort SetProductionLineOperation = 0x0203; // start/stop (0 = start, 1 = stop)
    public const ushort SetPrintMessage = 0x0214;            // select message while stopped
    public const ushort SetCounterWhilePrint = 0x024A;
    public const ushort SetPrintMessageWhilePrint = 0x024C;
}
