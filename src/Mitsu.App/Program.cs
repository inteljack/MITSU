using Mitsu.App.Forms;

namespace Mitsu.App;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length >= 2 && args[0] == "--screenshot")
            return ScreenshotTool.Run(args[1]);
        if (args.Length >= 2 && args[0] == "--demo-run")
            return ScreenshotTool.DemoRun(args[1], args.Length >= 3 && args[2] == "stop");

        using var mutex = new Mutex(true, @"Global\Anser.MitsuConsole", out bool first);
        if (!first)
        {
            MessageBox.Show("鶴見噴印控制台已經在執行中。", "鶴見噴印控制台", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return 1;
        }

        Application.ThreadException += (_, e) =>
            MessageBox.Show("發生未預期的錯誤：\n" + e.Exception.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);

        var settings = AppSettings.Load();
        Application.Run(new MainForm(settings, SampleData.Create()));
        return 0;
    }
}

/// <summary>
/// Development aid: renders the main windows to PNG files without user interaction
/// (MitsuConsole.exe --screenshot &lt;folder&gt;).
/// </summary>
internal static class ScreenshotTool
{
    public static int Run(string folder)
    {
        Directory.CreateDirectory(folder);
        var settings = new AppSettings();
        var data = SampleData.Create();

        var main = new MainForm(settings, data) { StartPosition = FormStartPosition.Manual, Location = new Point(-3000, -3000) };
        main.Show();
        var product = data.GetProductAsync("II01IHS3E-5C").Result!;
        main.LoadProductAsync(product).Wait();
        main.SetInputs("A678569", 0, 136, "21", "678569136");
        Application.DoEvents();
        Save(main, Path.Combine(folder, "main.png"));

        using (var preview = new Forms.PayloadPreviewForm(new Core.Domain.JobSpec
               {
                   Product = product,
                   Carton = data.GetCartonAsync(product.CaseNo).Result!,
                   MadeNo = "A678569",
                   BoxesDone = 0,
                   TotalBoxes = 136,
                   MachineNoPrefix = "21",
                   MachineNoSerial = "678569136",
               }, settings.ToPayloadOptions(), settings.Targets))
        {
            ShowOffscreen(preview);
            Save(preview, Path.Combine(folder, "preview.png"));
        }

        using (var status = new StatusForm(settings, () => false))
        {
            ShowOffscreen(status);
            status.SetState(Core.Production.ProductionState.Running, "噴印中，可以放入紙箱");
            status.SetCount(12, 136);
            status.SetMachineNo("21678569147");
            Application.DoEvents();
            Save(status, Path.Combine(folder, "status.png"));
        }

        using (var set = new SettingsForm(settings))
        {
            ShowOffscreen(set);
            Save(set, Path.Combine(folder, "settings.png"));
        }

        using (var sel = new DataSelectForm(data, ""))
        {
            ShowOffscreen(sel);
            Application.DoEvents();
            Thread.Sleep(100);
            Application.DoEvents();
            Save(sel, Path.Combine(folder, "select.png"));
        }

        main.Dispose();
        return 0;
    }

    /// <summary>
    /// Drives the real main window against the built-in simulator (auto trigger) without user
    /// interaction: read data → connect → print 10 boxes → (optionally stop half-way) → result.
    /// Writes screenshots and the event log to <paramref name="folder"/>. Exit code 0 = expected outcome.
    /// </summary>
    public static int DemoRun(string folder, bool stopHalfway)
    {
        Directory.CreateDirectory(folder);
        var settings = new AppSettings
        {
            UseSimulator = true,
            SimulatorAutoTriggerMs = 150,
            PollIntervalMs = 50,
            AlarmCheckEveryPolls = 5,
        };
        var data = SampleData.Create();
        using var main = new MainForm(settings, data) { Unattended = true, StartPosition = FormStartPosition.Manual, Location = new Point(-3000, -3000) };
        main.Show();
        main.LoadProductAsync(data.GetProductAsync("II01IHS3E-5C").Result!).Wait();
        main.SetInputs("A678569", 0, 10, "21", "0136");
        Application.DoEvents();

        var print = main.PrintAsync();
        bool midShot = false, stopped = false;
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (!print.IsCompleted && DateTime.UtcNow < deadline)
        {
            Application.DoEvents();
            Thread.Sleep(10);
            int printed = main.LogLines.Count(l => l.Contains("已噴出"));
            if (!midShot && printed >= 4)
            {
                Save(main, Path.Combine(folder, "run-mid.png"));
                midShot = true;
            }
            if (stopHalfway && !stopped && printed >= 5)
            {
                main.StopPrinting();
                stopped = true;
            }
        }
        for (int i = 0; i < 20; i++) { Application.DoEvents(); Thread.Sleep(10); }
        Save(main, Path.Combine(folder, stopHalfway ? "run-stopped.png" : "run-end.png"));
        File.WriteAllLines(Path.Combine(folder, stopHalfway ? "run-stopped.log" : "run-end.log"), main.LogLines);

        if (!print.IsCompleted) return 2;
        var log = string.Join("\n", main.LogLines);
        bool ok = stopHalfway ? log.Contains("已停止噴印") : log.Contains("噴印完成");
        main.Unattended = true;
        main.Hide();
        return ok ? 0 : 1;
    }

    private static void ShowOffscreen(Form f)
    {
        f.StartPosition = FormStartPosition.Manual;
        f.Location = new Point(-3000, -3000);
        f.Show();
        Application.DoEvents();
    }

    private static void Save(Form f, string path)
    {
        using var bmp = new Bitmap(f.Width, f.Height);
        f.DrawToBitmap(bmp, new Rectangle(0, 0, f.Width, f.Height));
        bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }
}
