namespace Mitsu.App;

/// <summary>Small layout helpers so the forms can be built in code without designer files.</summary>
internal static class Ui
{
    public static readonly Font Base = new("Microsoft JhengHei UI", 11f);
    public static readonly Font Bold = new("Microsoft JhengHei UI", 11f, FontStyle.Bold);
    public static readonly Font Large = new("Microsoft JhengHei UI", 16f, FontStyle.Bold);

    public static readonly Color Ok = Color.FromArgb(46, 125, 50);
    public static readonly Color Busy = Color.FromArgb(21, 101, 192);
    public static readonly Color Warn = Color.FromArgb(239, 108, 0);
    public static readonly Color Bad = Color.FromArgb(198, 40, 40);
    public static readonly Color Idle = Color.FromArgb(97, 97, 97);

    /// <summary>Auto-sized label so auto-sized table columns fit the whole text.</summary>
    public static Label Label(string text, ContentAlignment align = ContentAlignment.MiddleRight) => new()
    {
        Text = text,
        AutoSize = true,
        Anchor = align switch
        {
            ContentAlignment.MiddleLeft => AnchorStyles.Left,
            ContentAlignment.MiddleCenter => AnchorStyles.None,
            _ => AnchorStyles.Right,
        },
        TextAlign = align,
        Margin = new Padding(3, 6, 3, 6),
    };

    public static TextBox ReadOnlyBox() => new()
    {
        ReadOnly = true,
        Dock = DockStyle.Fill,
        BackColor = SystemColors.Control,
        TabStop = false,
        Margin = new Padding(3, 4, 3, 4),
    };

    public static TextBox Box(int maxLength = 0) => new()
    {
        Dock = DockStyle.Fill,
        MaxLength = maxLength,
        Margin = new Padding(3, 4, 3, 4),
    };

    public static Button Button(string text, EventHandler onClick, int width = 120)
    {
        var b = new Button { Text = text, Width = width, Height = 40, Margin = new Padding(6) };
        b.Click += onClick;
        return b;
    }

    public static TableLayoutPanel Grid(int columns, params float[] percents)
    {
        var t = new TableLayoutPanel
        {
            ColumnCount = columns,
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(4),
        };
        for (int i = 0; i < columns; i++)
            t.ColumnStyles.Add(i < percents.Length && percents[i] > 0
                ? new ColumnStyle(SizeType.Percent, percents[i])
                : new ColumnStyle(SizeType.AutoSize));
        return t;
    }

    public static GroupBox Group(string text, Control content)
    {
        var g = new GroupBox
        {
            Text = text,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 6),
            Font = Bold,
        };
        content.Font = Base;
        g.Controls.Add(content);
        return g;
    }

    /// <summary>Runs <paramref name="a"/> on the UI thread of <paramref name="c"/> (no-op if disposed).</summary>
    public static void Post(Control c, Action a)
    {
        if (c.IsDisposed || !c.IsHandleCreated) return;
        if (c.InvokeRequired) c.BeginInvoke(a);
        else a();
    }

    /// <summary>
    /// Runs <paramref name="a"/> on the UI thread and waits, so production events are shown in the
    /// order they happened (before the run's final result is handled).
    /// </summary>
    public static void Send(Control c, Action a)
    {
        if (c.IsDisposed || !c.IsHandleCreated) return;
        if (c.InvokeRequired)
        {
            try { c.Invoke(a); }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { } // handle destroyed while closing
        }
        else a();
    }

    public static void Error(IWin32Window owner, string message, string title = "錯誤") =>
        MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static bool Confirm(IWin32Window owner, string message, string title = "確認") =>
        MessageBox.Show(owner, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public static void AsciiOnly(TextBox box) =>
        box.KeyPress += (_, e) => { if (!char.IsControl(e.KeyChar) && (e.KeyChar > 0x7E || e.KeyChar < 0x20)) e.Handled = true; };

    public static void DigitsOnly(TextBox box) =>
        box.KeyPress += (_, e) => { if (!char.IsControl(e.KeyChar) && !char.IsAsciiDigit(e.KeyChar)) e.Handled = true; };
}
