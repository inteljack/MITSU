using Mitsu.Core.Data;
using Mitsu.Core.Domain;

namespace Mitsu.App.Forms;

/// <summary>噴印資料選擇: fuzzy search on Key code, double-click to pick.</summary>
public sealed class DataSelectForm : Form
{
    private readonly IMasterDataSource _data;
    private readonly TextBox _txt = Ui.Box(50);
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
        RowHeadersVisible = false,
        BackgroundColor = SystemColors.Window,
    };
    private readonly Label _lblCount = Ui.Label("", ContentAlignment.MiddleLeft);
    private List<ProductSpec> _rows = new();

    private const int MaxRows = 500;

    public DataSelectForm(IMasterDataSource data, string initial)
    {
        _data = data;
        Text = "噴印資料選擇";
        Font = Ui.Base;
        Size = new Size(1000, 600);
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;

        var top = Ui.Grid(4, 0, 60, 0, 40);
        top.Dock = DockStyle.Top;
        top.Controls.Add(Ui.Label("Key code："), 0, 0);
        top.Controls.Add(_txt, 1, 0);
        top.Controls.Add(Ui.Button("搜尋", (_, _) => _ = SearchAsync(), 90), 2, 0);
        top.Controls.Add(_lblCount, 3, 0);
        _txt.Text = initial;
        _txt.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _ = SearchAsync(); } };

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(6) };
        var cancel = Ui.Button("取消", (_, _) => { DialogResult = DialogResult.Cancel; });
        var ok = Ui.Button("確定", (_, _) => Pick());
        bottom.Controls.Add(cancel);
        bottom.Controls.Add(ok);
        AcceptButton = ok;
        CancelButton = cancel;

        foreach (var (name, header) in new[]
                 {
                     ("KeyCode", "Key code"), ("CaseNo", "CASE-No"), ("ModelValue", "MODEL_value"), ("Bore", "BORE"),
                     ("Output", "OUTPUT"), ("Phase", "PHASE"), ("Voltage", "VOLTAGE"), ("Hz", "Hz"), ("CodeValue", "CODE_value"), ("Tokki", "TOKKI"),
                 })
            _grid.Columns.Add(name, header);
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Pick(); };

        Controls.Add(_grid);
        Controls.Add(top);
        Controls.Add(bottom);
        Shown += (_, _) => _ = SearchAsync();
    }

    public ProductSpec? Selected { get; private set; }

    private async Task SearchAsync()
    {
        _rows = (await _data.SearchProductsAsync(_txt.Text.Trim(), MaxRows)).ToList();
        _grid.Rows.Clear();
        var limits = new FieldLimits();
        foreach (var p in _rows)
        {
            int i = _grid.Rows.Add(p.KeyCode, p.CaseNo, p.ModelValue, p.BoreValue + p.BoreUnit, p.OutputValue + p.OutputUnit,
                p.PhaseValue + p.PhaseUnit, p.VoltageValue + p.VoltageUnit, p.HzValue + p.HzUnit, p.CodeValue, p.Tokki);
            // over-length values in red (legacy behaviour)
            void Mark(string col, string v, int max) { if (v.Length > max) _grid.Rows[i].Cells[col].Style.ForeColor = Color.Red; }
            Mark("KeyCode", p.KeyCode, limits.KeyCode);
            Mark("ModelValue", p.ModelValue, limits.ModelValue);
            Mark("CodeValue", p.CodeValue, limits.CodeValue);
            Mark("Tokki", p.Tokki, limits.Tokki);
        }
        _lblCount.Text = _rows.Count >= MaxRows ? $"顯示前 {MaxRows} 筆，請縮小條件" : $"共 {_rows.Count} 筆";
    }

    private void Pick()
    {
        if (_grid.CurrentRow == null || _grid.CurrentRow.Index < 0 || _grid.CurrentRow.Index >= _rows.Count) return;
        Selected = _rows[_grid.CurrentRow.Index];
        DialogResult = DialogResult.OK;
    }
}
