using Mitsu.Printer.Protocol;

namespace Mitsu.Printer.Tests;

/// <summary>Byte-exact checks against the hex examples in analysis/01_X1_protocol.md.</summary>
public class ProtocolTests
{
    private static string Hex(X1Requests.Request r) => Convert.ToHexString(new AnserFrame(r.Command, r.Data).Encode());

    private static string Clean(string s) => s.Replace(" ", "");

    [Fact]
    public void Simple_query_frame()
    {
        Assert.Equal(Clean("00 00 00 00 00 06 00 64 00 02 00 43"), Hex(X1Requests.Simple(AnserCommand.GetProtocolVersion)));
    }

    [Fact]
    public void Start_and_stop_line_use_inverted_flag()
    {
        Assert.Equal(Clean("00 00 00 00 00 0B 00 64 00 07 00 20 02 03 00 01 00"), Hex(X1Requests.SetLineOperation(1, start: true)));
        Assert.Equal(Clean("00 00 00 00 00 0B 00 64 00 07 00 20 02 03 00 01 01"), Hex(X1Requests.SetLineOperation(1, start: false)));
    }

    [Fact]
    public void Select_message_is_ascii_padded_to_256()
    {
        var bytes = new AnserFrame(X1Requests.SetPrintMessage(1, 1, "MSG00003.xml").Command,
            X1Requests.SetPrintMessage(1, 1, "MSG00003.xml").Data).Encode();
        Assert.Equal(274, bytes.Length);
        Assert.StartsWith(Clean("00 00 00 00 01 0C 00 64 01 08 00 20 02 14 00 01 00 01 4D 53 47 30 30 30 30 33 2E 78 6D 6C 00"),
            Convert.ToHexString(bytes));
        Assert.All(bytes[30..], b => Assert.Equal(0, b));
    }

    [Fact]
    public void Select_message_rejects_non_ascii()
    {
        Assert.Throws<ArgumentException>(() => X1Requests.SetPrintMessage(1, 1, "訊息.xml"));
    }

    [Fact]
    public void Message_while_printing_is_utf8_with_u16_length()
    {
        Assert.Equal(Clean("00 00 00 00 00 1A 00 64 00 16 00 20 02 4C 00 01 00 01 00 0C 4D 53 47 30 30 30 30 33 2E 78 6D 6C"),
            Hex(X1Requests.SetPrintMessageWhilePrint(1, 1, "MSG00003.xml")));
    }

    [Fact]
    public void CF_frame_matches_documented_example()
    {
        var r = X1Requests.SetDynamicVarBuffer(1, 1, new[] { "AB12", "佳格" });
        Assert.Equal(
            Clean("00 00 00 00 00 28 00 64 00 24 00 CF 00 01 00 01 04 41 42 31 32 06 E4 BD B3 E6 A0 BC") + new string('0', 18 * 2),
            Hex(r));
    }

    [Fact]
    public void CA_frame_matches_documented_example()
    {
        var r = X1Requests.SetDynamicVarTable("test.ini", new[] { "AB12", "佳格" });
        Assert.Equal(
            Clean("00 00 00 00 00 2D 00 64 00 29 00 CA 08 74 65 73 74 2E 69 6E 69 04 41 42 31 32 06 E4 BD B3 E6 A0 BC") + new string('0', 18 * 2),
            Hex(r));
    }

    [Fact]
    public void CB_CC_frames()
    {
        Assert.Equal(Clean("00 00 00 00 00 0A 00 64 00 06 00 CB 00 01 00 01"), Hex(X1Requests.ClearDynamicVarBuffer(1, 1)));
        Assert.Equal(Clean("00 00 00 00 00 0A 00 64 00 06 00 CC 00 01 00 01"), Hex(X1Requests.ReadDynamicVarBufferStatus(1, 1)));
    }

    [Fact]
    public void Counter_and_clock_frames()
    {
        Assert.Equal(Clean("00 00 00 00 00 09 00 64 00 05 00 3A 00 01 01"), Hex(X1Requests.GetCounter(1, CounterType.Item)));
        Assert.Equal(Clean("00 00 00 00 00 0D 00 64 00 09 00 35 07 EA 0B 1B 0D 2D 1E"),
            Hex(X1Requests.SetSystemClock(new DateTime(2026, 11, 27, 13, 45, 30))));
    }

    [Fact]
    public void Dynamic_vars_validation()
    {
        Assert.Throws<ArgumentException>(() => X1Requests.SetDynamicVarBuffer(1, 1, Enumerable.Repeat("x", 21).ToList()));
        Assert.Throws<ArgumentException>(() => X1Requests.SetDynamicVarBuffer(1, 1, new[] { new string('中', 86) })); // 258 bytes
        X1Requests.SetDynamicVarBuffer(1, 1, new[] { new string('中', 85) }); // 255 bytes OK
    }

    [Fact]
    public void Decoder_handles_split_and_concatenated_frames()
    {
        var a = new AnserFrame(0x43, new byte[] { 1, 0, 24 }).Encode();
        var b = new AnserFrame(0x4F, Array.Empty<byte>()).Encode();
        var stream = a.Concat(b).ToArray();

        var dec = new FrameDecoder();
        var got = new List<AnserFrame>();
        foreach (var x in stream)
        {
            dec.Append(new[] { x });
            while (dec.TryReadFrame() is { } f) got.Add(f);
        }
        Assert.Equal(2, got.Count);
        Assert.Equal(0x43, got[0].Command);
        Assert.Equal(new byte[] { 1, 0, 24 }, got[0].Data.ToArray());
        Assert.Equal(0x4F, got[1].Command);

        dec.Append(stream);
        Assert.Equal(0x43, dec.TryReadFrame()!.Command);
        Assert.Equal(0x4F, dec.TryReadFrame()!.Command);
        Assert.Null(dec.TryReadFrame());
        Assert.Equal(0, dec.BufferedBytes);
    }

    [Fact]
    public void Decoder_rejects_garbage()
    {
        var dec = new FrameDecoder();
        dec.Append(new byte[] { 0, 0, 0x12, 0x34, 0, 6, 0, 0x64 });
        Assert.Throws<AnserProtocolException>(() => dec.TryReadFrame());

        dec = new FrameDecoder();
        var bad = new AnserFrame(0x43, Array.Empty<byte>()).Encode();
        bad[7] = 0x03; // wrong function code
        dec.Append(bad);
        Assert.Throws<AnserProtocolException>(() => dec.TryReadFrame());
    }

    [Theory]
    [InlineData(56)]
    [InlineData(68)]
    public void Printing_status_parses_both_block_sizes(int blockSize)
    {
        var d = new byte[1 + 2 * blockSize];
        d[0] = 2;
        // line 1 / station 1, item counter 7 ; line 2 / station 3, item counter 9
        d[2] = 1; d[4] = 1; d[8] = 7;
        int o = 1 + blockSize;
        d[o + 1] = 2; d[o + 3] = 3; d[o + 7] = 9;
        if (blockSize == 68) d[o + 56 + 3] = 1; // high part -> 2^32 + 9

        var s = X1Responses.ParsePrintingStatus(d);
        Assert.Equal(blockSize, s.BlockSize);
        Assert.Equal(2, s.Lines.Count);
        Assert.Equal((ushort)1, s.Lines[0].Line);
        Assert.Equal(7, s.Lines[0].ItemCounter);
        Assert.Equal((ushort)2, s.Lines[1].Line);
        Assert.Equal((ushort)3, s.Lines[1].Station);
        Assert.Equal(blockSize == 68 ? (1L << 32) + 9 : 9, s.Lines[1].ItemCounter);
    }

    [Fact]
    public void Printing_status_rejects_unknown_block_size()
    {
        var d = new byte[1 + 60];
        d[0] = 1;
        Assert.Throws<AnserProtocolException>(() => X1Responses.ParsePrintingStatus(d));
    }
}
