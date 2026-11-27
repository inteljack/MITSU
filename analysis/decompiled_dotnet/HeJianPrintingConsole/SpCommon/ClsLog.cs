using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SpCommon;

internal class ClsLog
{
	public class logTable
	{
		public static string logIdCaption = "ID";

		public static string logTimeCaption = "LogTime";

		public static string logMessageCaption = "LogMessage";
	}

	private static string m_strDir = Application.StartupPath + "\\Log\\";

	private static string m_strLogFileName = m_strDir + $"{DateTime.Now.Year:0000}{DateTime.Now.Month:00}{DateTime.Now.Day:00}{DateTime.Now.Hour:00}" + ".Log";

	public static string _logTableNameOnSql = "TLog";

	public static bool LogWrite(string strLog)
	{
		try
		{
			string exceptionMessage = "";
			strLog = "本地时间:" + GetCurTime() + " " + strLog;
			if (!LogWriteToSql(strLog, ref exceptionMessage))
			{
				strLog = "Exception:" + exceptionMessage + " Log:" + strLog;
				ClsFile.CreateDir(m_strDir);
				FileStream fsLog = new FileStream(m_strLogFileName, FileMode.Append);
				byte[] byteArrLog = Encoding.Default.GetBytes(GetCurTime() + "  " + strLog + "\r\n");
				fsLog.Write(byteArrLog, 0, byteArrLog.Length);
				fsLog.Close();
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool LogWrite(byte[] byteArrLog)
	{
		try
		{
			ClsFile.CreateDir(m_strDir);
			FileStream fsLog = new FileStream(m_strLogFileName, FileMode.Append);
			byte[] byteArrLogTime = Encoding.Default.GetBytes(GetCurTime() + "  ");
			byte[] byteArrLogTmp = new byte[byteArrLogTime.Length + byteArrLog.Length + 2];
			Array.Copy(byteArrLogTime, byteArrLogTmp, byteArrLogTime.Length);
			Array.Copy(byteArrLog, 0, byteArrLogTmp, byteArrLogTime.Length, byteArrLog.Length);
			byteArrLogTmp[byteArrLogTime.Length + byteArrLog.Length] = 16;
			byteArrLogTmp[byteArrLogTime.Length + byteArrLog.Length + 1] = 19;
			fsLog.Write(byteArrLogTmp, 0, byteArrLog.Length);
			fsLog.Close();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static string GetCurTime()
	{
		return DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "  " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond;
	}

	public static bool LogWriteToSql(string logStr, ref string exceptionMessage)
	{
		try
		{
			bool result = true;
			if (ClsDataBase.CheckDatabaseIsConnected())
			{
				string logTabPrefix = _logTableNameOnSql.Substring(0, 2).Trim().ToUpper();
				string sql = "INSERT INTO " + _logTableNameOnSql + "(" + logTabPrefix + logTable.logIdCaption + "," + logTabPrefix + logTable.logTimeCaption + "," + logTabPrefix + logTable.logMessageCaption + ") VALUES('" + Guid.NewGuid().ToString() + "', CONVERT(NVARCHAR(100), GETDATE(), 25),N'" + logStr + "')";
				int iRlt = ClsDataBase.SqlOperateData(sql);
				result = iRlt > 0;
			}
			else
			{
				result = false;
			}
			return result;
		}
		catch (Exception ex)
		{
			exceptionMessage = ex.Message;
			return false;
		}
	}

	public static bool InitializeLogTab()
	{
		if (!ClsDataBase.CheckExistsTable(_logTableNameOnSql))
		{
			DataTable dtLog = new DataTable(_logTableNameOnSql);
			dtLog.Columns.Add(logTable.logTimeCaption);
			dtLog.Columns.Add(logTable.logMessageCaption);
			ClsDataBase.SqlCreateTab(dtLog, _logTableNameOnSql);
		}
		else
		{
			string logTabPrefix = _logTableNameOnSql.Substring(0, 2).Trim().ToUpper();
			int needDeleteMonthCount = -6;
			string sql = "DELETE " + _logTableNameOnSql + " WHERE " + logTabPrefix + logTable.logTimeCaption + "<DATEADD(month," + needDeleteMonthCount + ",GETDATE())";
			ClsDataBase.SqlOperateData(sql);
		}
		return ClsDataBase.CheckExistsTable(_logTableNameOnSql);
	}
}
