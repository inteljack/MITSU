using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SpCommon;

public class ClsShare
{
	public static DataTable m_dtSelData;

	public static Button m_btnStop;

	public static bool m_bRestartApp;

	public static bool GetOpenDlgFileName(string strTitle, string strFilter, int iFilterIndex, string strIniDirectory, ref string strFileName)
	{
		OpenFileDialog ofdBmp = new OpenFileDialog();
		ofdBmp.Title = strTitle;
		ofdBmp.Filter = strFilter;
		ofdBmp.FilterIndex = iFilterIndex;
		ofdBmp.InitialDirectory = strIniDirectory;
		if (DialogResult.OK == ofdBmp.ShowDialog())
		{
			strFileName = ofdBmp.FileName;
			return true;
		}
		strFileName = "";
		return false;
	}

	public static DialogResult GetFolderPath(ref string strSelPath)
	{
		FolderBrowserDialog fbd = new FolderBrowserDialog();
		DialogResult DlgRlt = fbd.ShowDialog();
		strSelPath = fbd.SelectedPath;
		return DlgRlt;
	}

	public static DataTable DataGridViewToDataTable(DataGridView dgv)
	{
		DataTable dt = new DataTable();
		for (int count = 0; count < dgv.Columns.Count; count++)
		{
			DataColumn dc = new DataColumn(dgv.Columns[count].Name.ToString());
			dt.Columns.Add(dc);
		}
		for (int i = 0; i < dgv.Rows.Count; i++)
		{
			DataRow dr = dt.NewRow();
			for (int countsub = 0; countsub < dgv.Columns.Count; countsub++)
			{
				dr[countsub] = dgv.Rows[i].Cells[countsub].Value.ToString();
			}
			dt.Rows.Add(dr);
		}
		return dt;
	}

	public static bool OnlyInputNum(char KeyChar, bool bHasSign = false)
	{
		bool bRlt = false;
		if (KeyChar > '9' || KeyChar < '0')
		{
			if (bHasSign)
			{
				if (KeyChar != '\b' && KeyChar != '\u007f' && KeyChar != '-')
				{
					return false;
				}
				return true;
			}
			if (KeyChar != '\b' && KeyChar != '\u007f')
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public static bool OnlyInputAscii(char KeyChar)
	{
		bool bRlt = false;
		if (KeyChar > '\u0080')
		{
			return false;
		}
		return true;
	}

	public static bool JudgeMonth(string strSrc)
	{
		try
		{
			if (strSrc.Length > 2)
			{
				return false;
			}
			int iMonth = Convert.ToInt32(strSrc);
			if (iMonth >= 1 && iMonth <= 12)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Judge month exception:" + ex.Message);
			return false;
		}
	}

	public static bool JudgeDay(string strCurYear, string strCurMonth, string strCurDay)
	{
		try
		{
			if (strCurDay.Length > 2)
			{
				return false;
			}
			int iYear = Convert.ToInt32(strCurYear);
			int iMonth = Convert.ToInt32(strCurMonth);
			int iDay = Convert.ToInt32(strCurDay);
			if (iMonth < 1 || iMonth > 12)
			{
				return false;
			}
			int iMaxDay = 30;
			switch (iMonth)
			{
			case 1:
			case 3:
			case 5:
			case 7:
			case 8:
			case 10:
			case 12:
				iMaxDay = 31;
				break;
			case 2:
				iMaxDay = ((!JudgeLeapYear(iYear)) ? 28 : 29);
				break;
			case 4:
			case 6:
			case 9:
			case 11:
				iMaxDay = 30;
				break;
			}
			if (iDay >= 1 && iDay <= iMaxDay)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Judge day exception:" + ex.Message);
			return false;
		}
	}

	public static bool JudgeLeapYear(int iYear)
	{
		try
		{
			if (iYear % 4 == 0)
			{
				if (iYear % 100 == 0)
				{
					if (iYear % 400 == 0)
					{
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Judge leap year exception:" + ex.Message);
			return false;
		}
	}

	public static MemoryStream FileStreamToMemStream(string strFileNameSrc)
	{
		FileStream fsSrc = new FileStream(strFileNameSrc, FileMode.Open, FileAccess.Read, FileShare.Read);
		MemoryStream msDest = new MemoryStream();
		msDest.SetLength(fsSrc.Length - fsSrc.Position);
		fsSrc.Read(msDest.GetBuffer(), 0, (int)(fsSrc.Length - fsSrc.Position));
		return msDest;
	}

	public static string GetCurTime()
	{
		return DateTime.Now.Year + "-" + $"{DateTime.Now.Month:D2}" + "-" + $"{DateTime.Now.Day:D2}" + " " + $"{DateTime.Now.Hour:D2}" + ":" + $"{DateTime.Now.Minute:D2}" + ":" + $"{DateTime.Now.Second:D2}" + "." + $"{DateTime.Now.Millisecond.ToString():D3}";
	}

	public static string GetCurTimeFromSQl()
	{
		try
		{
			string strSql = "SELECT CONVERT(VARCHAR(100), GETDATE(), 121) ";
			DataTable dt = ClsDataBase.SqlReadData(strSql);
			if (dt != null && dt.Rows.Count > 0)
			{
				return dt.Rows[0][0].ToString();
			}
			return "";
		}
		catch
		{
			return "";
		}
	}

	public static bool CheckStrIsNum(string strSrc)
	{
		try
		{
			bool bRlt = true;
			foreach (char c in strSrc)
			{
				if (c < '0' || c > '9')
				{
					bRlt = false;
					break;
				}
			}
			return bRlt;
		}
		catch
		{
			return false;
		}
	}

	public static bool CheckStrIsSignNum(string strSrc)
	{
		try
		{
			bool bRlt = true;
			if (strSrc.Substring(0, 1) == "-")
			{
				strSrc = strSrc.Substring(1);
			}
			string text = strSrc;
			foreach (char c in text)
			{
				if (c < '0' || c > '9')
				{
					bRlt = false;
					break;
				}
			}
			return bRlt;
		}
		catch
		{
			return false;
		}
	}

	public static bool CheckStrIsNumAndPoint(string strSrc)
	{
		try
		{
			bool bRlt = true;
			foreach (char c in strSrc)
			{
				if ((c < '0' || c > '9') && c != '.')
				{
					bRlt = false;
					break;
				}
			}
			return bRlt;
		}
		catch
		{
			return false;
		}
	}

	public static bool CheckStrIsAscii(string strSrc)
	{
		try
		{
			bool bRlt = true;
			foreach (char c in strSrc)
			{
				if (c > '\u0080')
				{
					bRlt = false;
					break;
				}
			}
			return bRlt;
		}
		catch
		{
			return false;
		}
	}

	public static void RepainGroupBoxSide(object sender, PaintEventArgs e, Form frmParent)
	{
		GroupBox GB = (GroupBox)sender;
		string strText = GB.Text;
		e.Graphics.Clear(frmParent.BackColor);
		Pen penTmp = new Pen(Color.Black, 2f);
		e.Graphics.DrawString(GB.Text, GB.Font, Brushes.Blue, 10f, 1f);
		e.Graphics.DrawLine(penTmp, 1, 7, 8, 7);
		e.Graphics.DrawLine(penTmp, e.Graphics.MeasureString(strText, GB.Font).Width + 10f, 7f, GB.Width - 2, 7f);
		e.Graphics.DrawLine(penTmp, 1, 7, 1, GB.Height - 2);
		e.Graphics.DrawLine(penTmp, 1, GB.Height - 2, GB.Width - 2, GB.Height - 2);
		e.Graphics.DrawLine(penTmp, GB.Width - 2, 7, GB.Width - 2, GB.Height - 2);
	}
}
