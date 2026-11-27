using System.Windows.Forms;

namespace HeJianPrintingConsole;

public class ClsMsgBox
{
	public static void MessageBoxShowDefWarn(IWin32Window owner, string strText)
	{
		MessageBox.Show(owner, strText, "系統提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}
