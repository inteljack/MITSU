namespace Mitsu.App.Forms;

/// <summary>系統設定: printers, print targets, production and payload options (PropertyGrid on a copy).</summary>
public sealed class SettingsForm : Form
{
    private readonly PropertyGrid _grid = new()
    {
        Dock = DockStyle.Fill,
        PropertySort = PropertySort.Categorized,
        ToolbarVisible = false,
        HelpVisible = true,
    };

    public SettingsForm(AppSettings current)
    {
        Result = current.Clone();
        Text = "系統設定";
        Font = Ui.Base;
        Size = new Size(820, 680);
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        _grid.SelectedObject = Result;

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(6) };
        var cancel = Ui.Button("取消", (_, _) => DialogResult = DialogResult.Cancel);
        var save = Ui.Button("儲存", (_, _) => Save());
        var pwd = Ui.Button("設定工程密碼…", (_, _) => PasswordForm.Change(this, Result), 160);
        bottom.Controls.Add(cancel);
        bottom.Controls.Add(save);
        bottom.Controls.Add(pwd);
        CancelButton = cancel;

        var note = new Label
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(8, 6, 8, 0),
            Text = $"設定檔：{AppSettings.DefaultPath}\n儲存後會中斷目前連線，需重新連線才生效。",
        };

        Controls.Add(_grid);
        Controls.Add(note);
        Controls.Add(bottom);
    }

    public AppSettings Result { get; }

    private void Save()
    {
        var errors = Result.Check();
        if (errors.Count > 0)
        {
            Ui.Error(this, string.Join("\n", errors), "設定有誤");
            return;
        }
        DialogResult = DialogResult.OK;
    }
}

/// <summary>Engineer password prompt / change. Stored only as a PBKDF2 hash.</summary>
public sealed class PasswordForm : Form
{
    private readonly TextBox _pwd = new() { UseSystemPasswordChar = true, Dock = DockStyle.Fill };
    private readonly TextBox _confirm = new() { UseSystemPasswordChar = true, Dock = DockStyle.Fill };

    private PasswordForm(string title, bool withConfirm)
    {
        Text = title;
        Font = Ui.Base;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(420, withConfirm ? 170 : 120);

        var t = Ui.Grid(2, 0, 100);
        t.Controls.Add(Ui.Label(withConfirm ? "新密碼：" : "密碼："), 0, 0);
        t.Controls.Add(_pwd, 1, 0);
        if (withConfirm)
        {
            t.Controls.Add(Ui.Label("再輸入一次："), 0, 1);
            t.Controls.Add(_confirm, 1, 1);
        }
        var ok = Ui.Button("確定", (_, _) => DialogResult = DialogResult.OK, 100);
        var cancel = Ui.Button("取消", (_, _) => DialogResult = DialogResult.Cancel, 100);
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        bottom.Controls.Add(cancel);
        bottom.Controls.Add(ok);
        AcceptButton = ok;
        CancelButton = cancel;
        Controls.Add(t);
        Controls.Add(bottom);
    }

    public static bool Verify(IWin32Window owner, AppSettings s)
    {
        using var f = new PasswordForm("請輸入工程密碼", false);
        if (f.ShowDialog(owner) != DialogResult.OK) return false;
        if (s.VerifyPassword(f._pwd.Text)) return true;
        Ui.Error(owner, "密碼錯誤。");
        return false;
    }

    public static void Change(IWin32Window owner, AppSettings s)
    {
        using var f = new PasswordForm("設定工程密碼", true);
        if (f.ShowDialog(owner) != DialogResult.OK) return;
        if (f._pwd.Text.Length < 4) { Ui.Error(owner, "密碼至少 4 個字元。"); return; }
        if (f._pwd.Text != f._confirm.Text) { Ui.Error(owner, "兩次輸入的密碼不同。"); return; }
        s.SetPassword(f._pwd.Text);
        MessageBox.Show(owner, "密碼已設定，按「儲存」後生效。", "工程密碼");
    }
}
