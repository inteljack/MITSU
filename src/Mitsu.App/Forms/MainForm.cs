using System.Media;
using Mitsu.Core.Data;
using Mitsu.Core.Domain;
using Mitsu.Core.Production;
using Mitsu.Printer;

namespace Mitsu.App.Forms;

/// <summary>
/// 噴印控制台. Layout follows the legacy screen operators know (基本設置 / 正嘜 / 側嘜 / 紙箱規格 /
/// 噴印操作), with a status panel and event log on the right.
/// </summary>
public sealed class MainForm : Form
{
    private readonly IMasterDataSource _data;
    private AppSettings _settings;
    private PrinterSession _session;

    private ProductSpec? _product;
    private CartonSpec? _carton;
    private CancellationTokenSource? _stopCts;
    private ProductionController? _controller;
    private StatusForm? _statusForm;
    private bool _running;

    // 基本設置
    private readonly TextBox _txtKeyCode = Ui.ReadOnlyBox();
    private readonly TextBox _txtMadeNo = Ui.Box(15);
    private readonly TextBox _txtBoxesDone = Ui.Box(6);
    private readonly TextBox _txtTotal = Ui.Box(6);
    private readonly TextBox _txtPrefix = Ui.Box(12);
    private readonly TextBox _txtSerial = Ui.Box(14);
    private readonly ComboBox _cboDirection = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private Button _btnRead = null!;

    // 正嘜 / 側嘜 / 紙箱
    private readonly Dictionary<string, TextBox> _show = new();

    // status panel
    private readonly Label _lblState = new() { Dock = DockStyle.Top, Height = 56, TextAlign = ContentAlignment.MiddleCenter, Font = Ui.Large, ForeColor = Color.White };
    private readonly Label _lblCount = new() { Dock = DockStyle.Top, Height = 70, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Microsoft JhengHei UI", 28f, FontStyle.Bold) };
    private readonly Label _lblMachine = new() { Dock = DockStyle.Top, Height = 34, TextAlign = ContentAlignment.MiddleCenter, Font = Ui.Bold };
    private readonly ListBox _log = new() { Dock = DockStyle.Fill, Font = new Font("Microsoft JhengHei UI", 10f), HorizontalScrollbar = true, IntegralHeight = false };
    private readonly ToolStripStatusLabel _stConn = new() { Text = "● 未連線", ForeColor = Ui.Bad };
    private readonly ToolStripStatusLabel _stState = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel _stSent = new();
    private Button _btnConnect = null!, _btnPrint = null!, _btnStop = null!, _btnPreview = null!;
    private MenuStrip _menu = null!;

    public MainForm(AppSettings settings, IMasterDataSource data)
    {
        _settings = settings;
        _data = data;
        _session = NewSession(settings);

        Text = "鶴見噴印控制台";
        Font = Ui.Base;
        MinimumSize = new Size(1100, 760);
        Size = new Size(1360, 900);
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
        SetState(ProductionState.Idle, "請先讀取資料並連線");
        UpdateCount(0, 0);
        UpdateButtons();
        RestoreWindowBounds(settings.MainWindowBounds);
    }

    // ================================================================ layout

    private void BuildLayout()
    {
        _menu = new MenuStrip { Font = Ui.Base };
        _menu.Items.Add(new ToolStripMenuItem("噴印控制台(&T)") { Enabled = false });
        _menu.Items.Add(Pending("噴印資料管理(&U)"));
        _menu.Items.Add(Pending("紙箱規格管理(&V)"));
        _menu.Items.Add(Pending("生產記錄(&W)"));
        var sys = new ToolStripMenuItem("系統設定(&X)");
        sys.DropDownItems.Add("系統設定…", null, (_, _) => OpenSettings());
        sys.DropDownItems.Add("大字箱數視窗", null, (_, _) => ShowStatusForm());
        _menu.Items.Add(sys);
        _menu.Items.Add(new ToolStripMenuItem("關於(&Z)", null, (_, _) => ShowAbout()));
        MainMenuStrip = _menu;

        var left = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
        // GroupBoxes dock Top: add in reverse order
        left.Controls.Add(Ui.Group("噴印操作", BuildOperations()));
        left.Controls.Add(Ui.Group("紙箱規格", BuildCarton()));
        left.Controls.Add(Ui.Group("側嘜", BuildSide()));
        left.Controls.Add(Ui.Group("正嘜", BuildFront()));
        left.Controls.Add(Ui.Group("基本設置", BuildBasic()));

        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        var logGroup = new GroupBox { Text = "事件記錄", Dock = DockStyle.Fill, Font = Ui.Bold, Padding = new Padding(6) };
        logGroup.Controls.Add(_log);
        right.Controls.Add(logGroup);
        right.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
        right.Controls.Add(_lblMachine);
        right.Controls.Add(_lblCount);
        right.Controls.Add(_lblState);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel2,
            SplitterWidth = 6,
        };
        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(right);

        var status = new StatusStrip { Font = Ui.Base };
        status.Items.AddRange(new ToolStripItem[] { _stConn, _stState, _stSent });

        Controls.Add(split);
        Controls.Add(status);
        Controls.Add(_menu);

        Load += (_, _) =>
        {
            split.SplitterDistance = Math.Max(600, ClientSize.Width - 420);
            Log($"設定檔：{AppSettings.DefaultPath}");
            Log(_settings.UseSimulator ? "目前為「內建模擬器」模式（系統設定可切換為真機）" : "目前為真機模式");
            Log("資料來源：示範資料（資料層完成後改為 SQL Server）");
        };
    }

    private static ToolStripMenuItem Pending(string text) =>
        new(text) { Enabled = false, ToolTipText = "資料層完成後提供" };

    private Control BuildBasic()
    {
        var t = Ui.Grid(8, 0, 30, 0, 12, 0, 12, 0, 20);
        _btnRead = Ui.Button("讀取資料", (_, _) => _ = ReadDataAsync());
        _txtKeyCode.ReadOnly = false;
        _txtKeyCode.BackColor = SystemColors.Window;
        _txtKeyCode.MaxLength = 50;
        _txtKeyCode.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) _ = ReadDataAsync(); };

        t.Controls.Add(Ui.Label("Key Code："), 0, 0);
        t.Controls.Add(_txtKeyCode, 1, 0);
        t.SetColumnSpan(_txtKeyCode, 5);
        t.Controls.Add(_btnRead, 7, 0);

        t.Controls.Add(Ui.Label("製番 NO："), 0, 1);
        t.Controls.Add(_txtMadeNo, 1, 1);
        t.Controls.Add(Ui.Label("箱數："), 2, 1);
        t.Controls.Add(_txtBoxesDone, 3, 1);
        t.Controls.Add(Ui.Label("/"), 4, 1);
        t.Controls.Add(_txtTotal, 5, 1);

        t.Controls.Add(Ui.Label("機番："), 0, 2);
        t.Controls.Add(_txtPrefix, 1, 2);
        t.Controls.Add(Ui.Label("流水號："), 2, 2);
        t.Controls.Add(_txtSerial, 3, 2);
        t.SetColumnSpan(_txtSerial, 3);
        _cboDirection.Items.AddRange(new object[] { "按順序噴印", "按逆序噴印" });
        _cboDirection.SelectedIndex = 0;
        t.Controls.Add(_cboDirection, 7, 2);

        Ui.AsciiOnly(_txtMadeNo);
        Ui.AsciiOnly(_txtPrefix);
        Ui.DigitsOnly(_txtBoxesDone);
        Ui.DigitsOnly(_txtTotal);
        Ui.DigitsOnly(_txtSerial);
        _txtBoxesDone.Text = "0";
        _txtMadeNo.TextChanged += (_, _) => SetField("MadeNoResult", _txtMadeNo.Text);
        _txtPrefix.TextChanged += (_, _) => RefreshMachineNo();
        _txtSerial.TextChanged += (_, _) => RefreshMachineNo();
        _txtBoxesDone.TextChanged += (_, _) => RefreshCountFromInputs();
        _txtTotal.TextChanged += (_, _) => RefreshCountFromInputs();
        return t;
    }

    private Control BuildFront()
    {
        var t = Ui.Grid(8, 12, 0, 20, 18, 12, 0, 20, 18);
        void Row(int r, params (string key, int col, int span)[] cells)
        {
            foreach (var (key, col, span) in cells)
            {
                Control c = key.StartsWith(':') ? Ui.Label(key[1..], ContentAlignment.MiddleCenter) : ShowBox(key);
                t.Controls.Add(c, col, r);
                if (span > 1) t.SetColumnSpan(c, span);
            }
        }
        Row(0, ("Model", 0, 1), (":：", 1, 1), ("ModelValue", 2, 6));
        Row(1, ("Bore", 0, 1), (":：", 1, 1), ("BoreValue", 2, 1), ("BoreUnit", 3, 1), ("Output", 4, 1), (":：", 5, 1), ("OutputValue", 6, 1), ("OutputUnit", 7, 1));
        Row(2, ("PhaseValue", 0, 1), ("PhaseUnit", 2, 1), ("VoltageValue", 3, 1), ("VoltageUnit", 4, 1), ("HzValue", 6, 1), ("HzUnit", 7, 1));
        Row(3, ("MadeNoResult", 0, 4), ("Tokki", 4, 4));
        Row(4, ("Code", 0, 1), (":：", 1, 1), ("CodeValue", 2, 2), (":NO:", 5, 1), ("MachineNoResult", 6, 2));
        return t;
    }

    private Control BuildSide()
    {
        var t = Ui.Grid(2, 50, 50);
        t.Controls.Add(ShowBox("ModelValueSide"), 0, 0);
        t.Controls.Add(ShowBox("Place"), 1, 0);
        return t;
    }

    private Control BuildCarton()
    {
        var t = Ui.Grid(10, 0, 14, 0, 10, 0, 10, 0, 10, 0, 10);
        (string key, string label)[] top =
        {
            ("CaseNo", "紙箱編號："), ("Length", "L："), ("Width", "W："), ("Height", "H："), ("PrintHeight", "印字高度："),
        };
        for (int i = 0; i < top.Length; i++)
        {
            t.Controls.Add(Ui.Label(top[i].label), i * 2, 0);
            t.Controls.Add(ShowBox(top[i].key), i * 2 + 1, 0);
        }
        (string key, string label)[] bottom =
        {
            ("OutsideStart", "表面開始："), ("OutsideEnd", "表面終了："), ("InsideStart", "裏面開始："), ("InsideEnd", "裏面終了："),
        };
        for (int i = 0; i < bottom.Length; i++)
        {
            t.Controls.Add(Ui.Label(bottom[i].label), i * 2, 1);
            t.Controls.Add(ShowBox(bottom[i].key), i * 2 + 1, 1);
        }
        t.Controls.Add(Ui.Label("（單位 mm）", ContentAlignment.MiddleLeft), 8, 1);
        t.SetColumnSpan(t.GetControlFromPosition(8, 1)!, 2);
        return t;
    }

    private Control BuildOperations()
    {
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(4) };
        _btnConnect = Ui.Button("連線", (_, _) => _ = ToggleConnectionAsync());
        _btnPreview = Ui.Button("預覽", (_, _) => Preview());
        _btnPrint = Ui.Button("噴印", (_, _) => _ = PrintAsync());
        _btnStop = Ui.Button("停止", (_, _) => StopPrinting());
        _btnPrint.Font = Ui.Bold;
        _btnStop.Font = Ui.Bold;
        flow.Controls.AddRange(new Control[] { _btnConnect, _btnPreview, _btnPrint, _btnStop });
        return flow;
    }

    private TextBox ShowBox(string key)
    {
        var b = Ui.ReadOnlyBox();
        _show[key] = b;
        return b;
    }

    private void SetField(string key, string? value)
    {
        if (_show.TryGetValue(key, out var b)) b.Text = value ?? "";
    }

    // ================================================================ data

    private async Task ReadDataAsync()
    {
        if (_running) return;
        using var dlg = new DataSelectForm(_data, _txtKeyCode.Text.Trim());
        if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Selected == null) return;
        await LoadProductAsync(dlg.Selected);
    }

    internal async Task LoadProductAsync(ProductSpec p)
    {
        var carton = await _data.GetCartonAsync(p.CaseNo);
        _product = p;
        _carton = carton;
        _txtKeyCode.Text = p.KeyCode;

        SetField("Model", p.Model);
        SetField("ModelValue", p.ModelValue);
        SetField("Bore", p.Bore);
        SetField("BoreValue", p.BoreValue);
        SetField("BoreUnit", p.BoreUnit);
        SetField("Output", p.Output);
        SetField("OutputValue", p.OutputValue);
        SetField("OutputUnit", p.OutputUnit);
        SetField("PhaseValue", p.PhaseValue);
        SetField("PhaseUnit", p.PhaseUnit);
        SetField("VoltageValue", p.VoltageValue);
        SetField("VoltageUnit", p.VoltageUnit);
        SetField("HzValue", p.HzValue);
        SetField("HzUnit", p.HzUnit);
        SetField("Tokki", p.Tokki);
        SetField("Code", p.Code);
        SetField("CodeValue", p.CodeValue);
        SetField("ModelValueSide", p.ModelValue);
        SetField("Place", p.Place);

        SetField("CaseNo", p.CaseNo);
        SetField("Length", carton?.Length.ToString());
        SetField("Width", carton?.Width.ToString());
        SetField("Height", carton?.Height.ToString());
        SetField("PrintHeight", carton?.PrintHeight.ToString());
        SetField("OutsideStart", carton?.OutsideStart.ToString());
        SetField("OutsideEnd", carton?.OutsideEnd.ToString());
        SetField("InsideStart", carton?.InsideStart.ToString());
        SetField("InsideEnd", carton?.InsideEnd.ToString());

        Log($"已讀取 {p.KeyCode}（紙箱 {p.CaseNo}）");
        if (carton == null)
        {
            Log($"⚠ 紙箱規格中找不到 {p.CaseNo}");
            Alert($"紙箱規格中找不到紙箱編號 {p.CaseNo}，無法噴印。");
        }
        UpdateButtons();
    }

    internal void SetInputs(string madeNo, int boxesDone, int total, string prefix, string serial)
    {
        _txtMadeNo.Text = madeNo;
        _txtBoxesDone.Text = boxesDone.ToString();
        _txtTotal.Text = total.ToString();
        _txtPrefix.Text = prefix;
        _txtSerial.Text = serial;
    }

    private void RefreshMachineNo() => SetField("MachineNoResult", _txtPrefix.Text + _txtSerial.Text);

    private void RefreshCountFromInputs()
    {
        if (_running) return;
        int.TryParse(_txtBoxesDone.Text, out int done);
        int.TryParse(_txtTotal.Text, out int total);
        UpdateCount(done, total);
    }

    /// <summary>Builds the job from the screen; null (with message) when input is incomplete.</summary>
    private JobSpec? BuildJob()
    {
        if (_product == null) { Alert("請先按「讀取資料」選擇噴印資料。"); return null; }
        if (_carton == null) { Alert($"紙箱規格中找不到紙箱編號 {_product.CaseNo}。"); return null; }
        int.TryParse(_txtBoxesDone.Text, out int done);
        int.TryParse(_txtTotal.Text, out int total);
        var job = new JobSpec
        {
            Product = _product,
            Carton = _carton,
            MadeNo = _txtMadeNo.Text.Trim(),
            BoxesDone = done,
            TotalBoxes = total,
            MachineNoPrefix = _txtPrefix.Text.Trim(),
            MachineNoSerial = _txtSerial.Text.Trim(),
            Direction = _cboDirection.SelectedIndex == 1 ? SerialDirection.Descending : SerialDirection.Ascending,
        };
        var v = job.Validate();
        if (!v.IsValid)
        {
            Alert("無法噴印，請修正以下項目：\n\n" + string.Join("\n", v.Errors.Select(e => $"• {e.Field}：{e.Message}")), "資料檢查");
            return null;
        }
        return job;
    }

    private void Preview()
    {
        var job = BuildJob();
        if (job == null) return;
        using var dlg = new PayloadPreviewForm(job, _settings.ToPayloadOptions(), _settings.Targets);
        dlg.ShowDialog(this);
    }

    // ================================================================ printer

    private PrinterSession NewSession(AppSettings s)
    {
        var session = new PrinterSession(s);
        session.Log += msg => Ui.Post(this, () => Log(msg));
        session.ConnectionChanged += () => Ui.Post(this, UpdateButtons);
        return session;
    }

    private async Task ToggleConnectionAsync()
    {
        if (_running) return;
        _btnConnect.Enabled = false;
        try
        {
            if (_session.AllConnected)
            {
                await _session.DisconnectAsync();
                Log("已中斷連線");
            }
            else
            {
                await ConnectAsync();
            }
        }
        finally
        {
            UpdateButtons();
        }
    }

    private async Task<bool> ConnectAsync()
    {
        try
        {
            Log("連線中…");
            await _session.ConnectAsync();
            return true;
        }
        catch (Exception ex) when (ex is X1Exception or InvalidOperationException or System.Net.Sockets.SocketException)
        {
            Log("連線失敗：" + ex.Message);
            Alert("連線失敗：\n" + ex.Message);
            return false;
        }
        finally
        {
            UpdateButtons();
        }
    }

    internal async Task PrintAsync()
    {
        if (_running) return;
        var job = BuildJob();
        if (job == null) return;
        if (!_session.AllConnected && !await ConnectAsync()) return;

        _controller = new ProductionController(_session.BuildTargets(), _settings.ToProductionOptions(),
            new CartonPayloadComposer(_settings.ToPayloadOptions()));
        _controller.StateChanged += s => Ui.Send(this, () => OnStateChanged(s));
        _controller.CartonSent += c => Ui.Send(this, () => _stSent.Text = $"總發送箱數 {c.Sequence + 1}");
        _controller.CartonPrinted += c => Ui.Send(this, () => OnCartonPrinted(c, job));
        _controller.Warning += w => Ui.Send(this, () => Log("⚠ " + w));

        _stopCts = new CancellationTokenSource();
        _running = true;
        UpdateButtons();
        ShowStatusForm();
        UpdateCount(job.BoxesDone, job.TotalBoxes);
        Log($"開始噴印：{job.Product.KeyCode} 製番 {job.MadeNo}，第 {job.BoxesDone + 1}–{job.TotalBoxes} 箱，起始機番 {job.MachineNoPrefix}{job.MachineNoSerial}");

        ProductionResult result;
        try
        {
            result = await _controller.RunAsync(job, _stopCts.Token);
        }
        catch (DomainValidationException ex)
        {
            _running = false;
            UpdateButtons();
            Alert(ex.Message);
            return;
        }
        finally
        {
            _stopCts.Dispose();
            _stopCts = null;
        }

        _running = false;
        HandleResult(result, job);
        UpdateButtons();
    }

    internal void StopPrinting()
    {
        if (_stopCts == null) return;
        _btnStop.Enabled = false;
        _stopCts.Cancel();
    }

    private void OnStateChanged(ProductionState s)
    {
        string text = s switch
        {
            ProductionState.Starting => "準備噴印…（選訊息、預送資料）",
            ProductionState.Running => "噴印中，可以放入紙箱",
            ProductionState.Stopping => "正在停止，請稍候…",
            ProductionState.Completed => "噴印完成",
            ProductionState.Stopped => "已停止噴印",
            ProductionState.Faulted => "異常停止",
            _ => "待機",
        };
        SetState(s, text);
        Log(text);
    }

    private void OnCartonPrinted(CartonRecord c, JobSpec job)
    {
        UpdateCount(c.BoxNo, job.TotalBoxes);
        _lblMachine.Text = $"已噴機番：{c.MachineNo}";
        _statusForm?.SetMachineNo(c.MachineNo);
        Log($"第 {c.BoxNo} 箱已噴出　機番 {c.MachineNo}");
    }

    private void HandleResult(ProductionResult r, JobSpec job)
    {
        var (boxesDone, nextSerial) = r.ResumePoint;
        Log($"本次確認噴出 {r.PrintedCount} 箱；下次由第 {boxesDone + 1} 箱、流水號 {(nextSerial.Length > 0 ? nextSerial : "（已用完）")} 開始");

        if (r.State == ProductionState.Faulted)
        {
            SystemSounds.Hand.Play();
            Log("✖ 異常：" + r.FaultReason);
            _statusForm?.SetFault(r.FaultReason ?? "");
        }

        // carry the resume point back into the inputs so the next run neither skips nor repeats a number
        _txtBoxesDone.Text = boxesDone.ToString();
        if (nextSerial.Length > 0) _txtSerial.Text = nextSerial;

        if (r.Uncertain.Count > 0)
        {
            Log($"⚠ 有 {r.Uncertain.Count} 箱無法確認是否已噴出，需人工確認");
            if (!Unattended)
            {
                using var dlg = new RunResultForm(r);
                dlg.ShowDialog(this);
            }
        }
        else if (r.State == ProductionState.Faulted)
        {
            Alert($"噴印異常停止：\n{r.FaultReason}\n\n已確認噴出 {r.PrintedCount} 箱，起始值已更新為下一箱。", "異常停止");
        }

        if (r.State == ProductionState.Completed)
        {
            CloseStatusForm();
            if (Ask("噴印完成，是否清空箱數與機番？"))
            {
                _txtBoxesDone.Text = "0";
                _txtTotal.Text = "";
                _txtSerial.Text = "";
            }
        }
        else if (r.State == ProductionState.Stopped)
        {
            CloseStatusForm();
        }
    }

    // ================================================================ status window

    private void ShowStatusForm()
    {
        if (Unattended) return; // automation must not pop windows on the desktop
        if (_statusForm == null || _statusForm.IsDisposed)
        {
            _statusForm = new StatusForm(_settings, () => _running);
            _statusForm.FormClosed += (_, _) => _statusForm = null;
        }
        _statusForm.Show();
        RefreshCountFromInputs();
    }

    private void CloseStatusForm()
    {
        if (_statusForm != null && !_statusForm.IsDisposed) _statusForm.Close();
    }

    // ================================================================ settings / about

    private void OpenSettings()
    {
        if (_running) { Alert("噴印中不可修改系統設定，請先停止。"); return; }
        if (_settings.HasPassword && !PasswordForm.Verify(this, _settings)) return;

        using var dlg = new SettingsForm(_settings);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        _ = ApplySettingsAsync(dlg.Result);
    }

    private async Task ApplySettingsAsync(AppSettings s)
    {
        try
        {
            s.Save();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Alert("無法儲存設定：\n" + ex.Message);
            return;
        }
        await _session.DisconnectAsync();
        _settings = s;
        _session = NewSession(s);
        Log("系統設定已更新，請重新連線");
        UpdateButtons();
    }

    private void ShowAbout()
    {
        var v = typeof(MainForm).Assembly.GetName().Version;
        MessageBox.Show(this,
            $"鶴見噴印控制台\n版本 {v?.Major}.{v?.Minor}.{v?.Build}\n\n噴碼機：NexGen X1 / SPH（Modbus TCP 擴充指令）\nCopyright Anser",
            "關於", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ================================================================ helpers

    private void SetState(ProductionState s, string text)
    {
        _lblState.Text = text;
        _lblState.BackColor = s switch
        {
            ProductionState.Running => Ui.Ok,
            ProductionState.Starting or ProductionState.Stopping => Ui.Busy,
            ProductionState.Completed => Ui.Ok,
            ProductionState.Faulted => Ui.Bad,
            ProductionState.Stopped => Ui.Warn,
            _ => Ui.Idle,
        };
        _stState.Text = text;
        _statusForm?.SetState(s, text);
    }

    private void UpdateCount(int done, int total)
    {
        _lblCount.Text = total > 0 ? $"{done} / {total} 箱" : "– / – 箱";
        _statusForm?.SetCount(done, total);
    }

    private void UpdateButtons()
    {
        bool connected = _session.AllConnected;
        _stConn.Text = connected ? (_settings.UseSimulator ? "● 已連線（模擬器）" : "● 已連線") : "● 未連線";
        _stConn.ForeColor = connected ? Ui.Ok : Ui.Bad;
        _btnConnect.Text = connected ? "中斷連線" : "連線";
        _btnConnect.Enabled = !_running;
        _btnPrint.Enabled = !_running && _product != null && _carton != null;
        _btnPreview.Enabled = !_running && _product != null;
        _btnStop.Enabled = _running;
        _btnRead.Enabled = !_running;
        foreach (var c in new Control[] { _txtKeyCode, _txtMadeNo, _txtBoxesDone, _txtTotal, _txtPrefix, _txtSerial, _cboDirection })
            c.Enabled = !_running;
        foreach (ToolStripItem item in _menu.Items) if (item.Text != "關於(&Z)" && item.Text != "噴印控制台(&T)" && item.ToolTipText == null) item.Enabled = !_running;
    }

    /// <summary>Demo/automation mode: dialogs are logged instead of shown.</summary>
    internal bool Unattended { get; set; }

    internal bool IsRunning => _running;

    internal IEnumerable<string> LogLines => _log.Items.Cast<string>();

    private void Alert(string message, string title = "錯誤")
    {
        if (Unattended) Log($"[{title}] {message.Replace('\n', ' ')}");
        else Ui.Error(this, message, title);
    }

    private bool Ask(string message)
    {
        if (Unattended) { Log($"[詢問] {message} → 否"); return false; }
        return Ui.Confirm(this, message);
    }

    private void Log(string message)
    {
        _log.Items.Add($"{DateTime.Now:HH:mm:ss}  {message}");
        if (_log.Items.Count > 2000) _log.Items.RemoveAt(0);
        _log.TopIndex = Math.Max(0, _log.Items.Count - 1);
    }

    private void RestoreWindowBounds(int[]? b)
    {
        if (b is not { Length: 4 }) return;
        var r = new Rectangle(b[0], b[1], b[2], b[3]);
        if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(r)))
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = r;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_running)
        {
            Alert("噴印中，請先按「停止」再關閉程式。");
            e.Cancel = true;
            return;
        }
        if (e.CloseReason == CloseReason.UserClosing && !Ui.Confirm(this, "確定要關閉鶴見噴印控制台？"))
        {
            e.Cancel = true;
            return;
        }
        if (WindowState == FormWindowState.Normal)
        {
            _settings.MainWindowBounds = new[] { Left, Top, Width, Height };
            try { _settings.Save(); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
        base.OnFormClosing(e);
    }

    protected override async void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        await _session.DisposeAsync();
    }
}
