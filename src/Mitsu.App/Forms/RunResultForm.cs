using Mitsu.Core.Production;

namespace Mitsu.App.Forms;

/// <summary>
/// Shown when a run ends with cartons whose printing could not be confirmed (one side only, or the
/// link dropped). The operator must check them on the line — the old system silently skipped them,
/// which caused the 跳號 / 正反面不一致 complaints.
/// </summary>
public sealed class RunResultForm : Form
{
    public RunResultForm(ProductionResult r)
    {
        Text = "需人工確認的紙箱";
        Font = Ui.Base;
        Size = new Size(900, 560);
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;

        var (boxesDone, next) = r.ResumePoint;
        var head = new Label
        {
            Dock = DockStyle.Top,
            Height = 120,
            Padding = new Padding(10),
            ForeColor = Ui.Bad,
            Font = Ui.Bold,
            Text = (r.FaultReason != null ? $"停止原因：{r.FaultReason}\n" : "") +
                   $"下列 {r.Uncertain.Count} 箱無法確認是否已噴出（或只噴了其中一面），請到產線逐箱檢查。\n" +
                   $"畫面的起始值已設為「兩面都確認噴出」之後的下一箱：第 {boxesDone + 1} 箱、流水號 {next}。\n" +
                   "若檢查後發現這些箱子已正確噴出，請手動調整箱數與流水號後再繼續。",
        };

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
        };
        grid.Columns.Add("box", "箱數");
        grid.Columns.Add("machine", "機番");
        grid.Columns.Add("sent", "送出時間");
        for (int i = 0; i < r.Job.TargetCount; i++) grid.Columns.Add("t" + i, $"目標 {i + 1} 確認");
        foreach (var c in r.Uncertain)
        {
            var row = new List<object> { c.BoxNo, c.MachineNo, c.SentAt?.LocalDateTime.ToString("HH:mm:ss") ?? "" };
            row.AddRange(c.PrintedOn.Select(t => (object)(t?.LocalDateTime.ToString("HH:mm:ss") ?? "未確認")));
            grid.Rows.Add(row.ToArray());
        }

        var ok = Ui.Button("我知道了", (_, _) => Close(), 140);
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(6) };
        bottom.Controls.Add(ok);
        AcceptButton = ok;

        Controls.Add(grid);
        Controls.Add(head);
        Controls.Add(bottom);
    }
}
