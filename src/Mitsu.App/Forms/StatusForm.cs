using Mitsu.Core.Production;

namespace Mitsu.App.Forms;

/// <summary>
/// 大字箱數視窗: 「已噴箱數 / 總箱數」 in 72pt, meant for the second monitor. Cannot be closed by
/// the operator while printing (legacy requirement); position is remembered.
/// </summary>
public sealed class StatusForm : Form
{
    private readonly AppSettings _settings;
    private readonly Func<bool> _isRunning;
    private readonly Label _count = new()
    {
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("DFKai-SB", 72f, FontStyle.Bold),
        ForeColor = Color.White,
    };
    private readonly Label _state = new()
    {
        Dock = DockStyle.Top,
        Height = 70,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Microsoft JhengHei UI", 24f, FontStyle.Bold),
        ForeColor = Color.White,
    };
    private readonly Label _machine = new()
    {
        Dock = DockStyle.Bottom,
        Height = 60,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Microsoft JhengHei UI", 22f, FontStyle.Bold),
        ForeColor = Color.White,
    };

    public StatusForm(AppSettings settings, Func<bool> isRunning)
    {
        _settings = settings;
        _isRunning = isRunning;
        Text = "噴印箱數";
        BackColor = Ui.Idle;
        Size = new Size(900, 420);
        MinimizeBox = false;
        MaximizeBox = true;
        TopMost = true;
        ShowInTaskbar = false;
        Controls.Add(_count);
        Controls.Add(_machine);
        Controls.Add(_state);
        Place();
    }

    public void SetCount(int done, int total) => _count.Text = total > 0 ? $"{done} / {total}" : "– / –";

    public void SetMachineNo(string machineNo) => _machine.Text = "機番 " + machineNo;

    public void SetState(ProductionState s, string text)
    {
        _state.Text = text;
        BackColor = s switch
        {
            ProductionState.Running or ProductionState.Completed => Ui.Ok,
            ProductionState.Starting or ProductionState.Stopping => Ui.Busy,
            ProductionState.Faulted => Ui.Bad,
            ProductionState.Stopped => Ui.Warn,
            _ => Ui.Idle,
        };
    }

    public void SetFault(string reason)
    {
        BackColor = Ui.Bad;
        _state.Text = "異常停止";
        _machine.Text = reason;
    }

    private void Place()
    {
        StartPosition = FormStartPosition.Manual;
        if (_settings.StatusWindowBounds is { Length: 4 } b)
        {
            var r = new Rectangle(b[0], b[1], b[2], b[3]);
            if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(r)))
            {
                Bounds = r;
                return;
            }
        }
        // default: centre of the second monitor if there is one, else top-right of the primary
        var screen = Screen.AllScreens.FirstOrDefault(s => !s.Primary) ?? Screen.PrimaryScreen!;
        var wa = screen.WorkingArea;
        Location = screen.Primary
            ? new Point(wa.Right - Width - 20, wa.Top + 20)
            : new Point(wa.Left + (wa.Width - Width) / 2, wa.Top + (wa.Height - Height) / 2);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && _isRunning())
        {
            e.Cancel = true; // operators may not close it while printing
            return;
        }
        if (WindowState == FormWindowState.Normal)
            _settings.StatusWindowBounds = new[] { Left, Top, Width, Height };
        base.OnFormClosing(e);
    }
}
