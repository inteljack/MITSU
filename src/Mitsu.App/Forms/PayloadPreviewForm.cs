using Mitsu.Core.Domain;
using Mitsu.Core.Production;

namespace Mitsu.App.Forms;

/// <summary>
/// 預覽: shows exactly what will be sent per carton (String1–20 of CFh) for the first cartons of the
/// run, and which X1 message each target uses. Replaces the legacy bitmap preview until the X1
/// message layouts are designed.
/// </summary>
public sealed class PayloadPreviewForm : Form
{
    public PayloadPreviewForm(JobSpec job, PayloadOptions options, IReadOnlyList<TargetConfig> targets)
    {
        Text = "預覽：每箱送出的內容";
        Font = Ui.Base;
        Size = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;

        var pj = new ProductionJob(job, 1, new CartonPayloadComposer(options));
        var cartons = new List<CartonRecord>();
        for (int i = 0; i < Math.Min(3, job.RemainingBoxes); i++) cartons.Add(pj.CreateNext(DateTimeOffset.Now)!);

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            Font = new Font("Consolas", 11f),
        };
        grid.Columns.Add("slot", "String");
        grid.Columns.Add("field", "欄位");
        foreach (var c in cartons) grid.Columns.Add("c" + c.Sequence, $"第 {c.BoxNo} 箱");
        grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        grid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

        for (int i = 0; i < PayloadOptions.SlotCount; i++)
        {
            string name = Enum.IsDefined(typeof(PayloadField), i) ? ((PayloadField)i).ToString() : "（未使用）";
            var row = new List<object> { $"String{i + 1}", name };
            row.AddRange(cartons.Select(c => (object)ShowSpaces(c.Values[i])));
            int r = grid.Rows.Add(row.ToArray());
            if (i is (int)PayloadField.MachineNo or (int)PayloadField.CodeAndMachineNo)
                grid.Rows[r].DefaultCellStyle.BackColor = Color.LightYellow;
        }

        var info = new Label
        {
            Dock = DockStyle.Top,
            Height = 100,
            Padding = new Padding(8),
            Text = "空白以「·」顯示。黃色列每箱不同（機番）。\n" +
                   "噴印目標：" + string.Join("；", targets.Select(t => t.ToString())) + "\n" +
                   "X1 訊息中的物件依「String 編號」綁定動態變數。",
        };
        var close = Ui.Button("關閉", (_, _) => Close());
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(6) };
        bottom.Controls.Add(close);
        CancelButton = close;

        Controls.Add(grid);
        Controls.Add(info);
        Controls.Add(bottom);
    }

    private static string ShowSpaces(string s) => s.Replace(' ', '·');
}
