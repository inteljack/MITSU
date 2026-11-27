using System;
using System.Threading;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

internal static class Program
{
	private static Mutex mutex;

	[STAThread]
	private static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		mutex = new Mutex(initiallyOwned: true, "f78436b9-98b5-43b8-aa11-bdee7c6f4ff9", out var bRunOne);
		if (ClsXml.GetRestartAppFlag())
		{
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strRestartAppFlag, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, "FALSE");
			Application.Run(new frmMain());
		}
		else if (bRunOne)
		{
			mutex.ReleaseMutex();
			Application.Run(new frmMain());
		}
		else
		{
			MessageBox.Show("程序已經運行！", "系統提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			Application.Exit();
		}
	}
}
