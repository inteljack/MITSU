using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HeJianPrintingConsole;

namespace SpCommon;

internal class ClsRegHotKey
{
	private const int WM_HOTKEY = 786;

	private const int WM_CREATE = 1;

	private const int WM_DESTROY = 2;

	public const int MOD_NONE = 0;

	public const int MOD_ALT = 1;

	public const int MOD_CONTROL = 2;

	public const int MOD_SHIFT = 4;

	public const int MOD_WINDOWS = 8;

	public const int VK_SPACE = 32;

	public const int VK_DELETE = 46;

	public static int iHotKeyID = 32;

	[DllImport("user32.dll")]
	private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

	[DllImport("user32.dll")]
	private static extern int UnregisterHotKey(IntPtr hwnd, int id);

	public static void RegKey(IntPtr hwnd, int hotKey_id, int fsModifiers, int vk)
	{
		if (RegisterHotKey(hwnd, hotKey_id, fsModifiers, vk) == 0 || 1 == 0)
		{
			MessageBox.Show("注册热键失败！");
		}
	}

	public static void UnRegKey(IntPtr hwnd, int hotKey_id)
	{
		UnregisterHotKey(hwnd, hotKey_id);
	}

	public static void WndPrcRegHotKey(ref Message m, IntPtr Handle)
	{
		int msg = m.Msg;
		if (msg != 786)
		{
			return;
		}
		int num = m.WParam.ToInt32();
		if (num == 32)
		{
			if (ClsShare.m_btnStop.Enabled)
			{
				MessageBox.Show("噴印期間無法進入超級設定");
				return;
			}
			frmSuperSet frmSS = new frmSuperSet();
			frmSS.ShowDialog();
		}
	}
}
