using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using HeJianPrintingConsole;

namespace SpCommon;

internal class ClsDataBase
{
	public static SqlConnection SqlOpen()
	{
		try
		{
			string strSqlConnectionString = get_sqlconnectionstring();
			if (strSqlConnectionString.Trim().Length <= 0)
			{
				return null;
			}
			SqlConnection ConnSql = new SqlConnection(strSqlConnectionString);
			ConnSql.Open();
			return ConnSql;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlOpen Exception:" + ex.Message);
			return null;
		}
	}

	public static void SqlClose(SqlConnection ConnSql)
	{
		try
		{
			ConnSql?.Close();
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("exSqlClose Exception:" + ex.Message);
		}
	}

	private static string get_sqlconnectionstring()
	{
		try
		{
			string sqlconnectionstring = "";
			if (sqlconnectionstring == "")
			{
				string strServer = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strServerName, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				string strDataBaseName = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strDatabaseName, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				string strUID = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strDatabaseUserID, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				string strPWD = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strDatabasePwd, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				if (strServer.Length <= 0 || strDataBaseName.Length <= 0)
				{
					return "";
				}
				return string.Format("server={0};uid={2};password={3};database={1}", strServer, strDataBaseName, strUID, strPWD);
			}
			return sqlconnectionstring;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("get_sqlconnectionstring Exception:" + ex.Message);
			return null;
		}
	}

	public static DataTable SqlReadData(string strSql)
	{
		try
		{
			SqlDataAdapter sda = new SqlDataAdapter(strSql, get_sqlconnectionstring());
			DataSet ds = new DataSet();
			sda.Fill(ds);
			if (ds.Tables.Count == 0)
			{
				return null;
			}
			return ds.Tables[0];
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlReadData exception:" + ex.Message);
			return null;
		}
	}

	public static int SqlOperateData(string strSql)
	{
		try
		{
			SqlConnection SqlCon = SqlOpen();
			if (SqlCon == null)
			{
				ClsLog.LogWrite("SqlOperateData exception:SqlCon is null");
				return 0;
			}
			SqlCommand SqlCmd = new SqlCommand();
			SqlCmd.Connection = SqlCon;
			SqlCmd.CommandText = strSql;
			int iRlt = SqlCmd.ExecuteNonQuery();
			SqlClose(SqlCon);
			return iRlt;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlOperateData exception" + ex.Message);
			return 0;
		}
	}

	public static bool SqlCreateTab(DataTable dtSrc, string strTabName, string strPrimaryKeyName = "")
	{
		try
		{
			if (CheckExistsTable(strTabName))
			{
				return true;
			}
			string strSql = "CREATE TABLE " + strTabName + " ( [" + strTabName.Substring(0, 2) + "ID] nvarchar(50) ,";
			for (int iColumnIndex = 0; iColumnIndex < dtSrc.Columns.Count; iColumnIndex++)
			{
				int iVarcharLen = 1024;
				string strConstraintCondition = "";
				if (strPrimaryKeyName.Length > 0 && dtSrc.Columns[iColumnIndex].Caption.Trim().ToUpper() == strPrimaryKeyName.Trim().ToUpper())
				{
					iVarcharLen = 512;
					strConstraintCondition = " PRIMARY KEY ";
				}
				strSql = strSql + " [" + strTabName.Substring(0, 2).Trim().ToUpper() + dtSrc.Columns[iColumnIndex].Caption + "]  nvarchar(" + iVarcharLen + ") " + strConstraintCondition + " ";
				if (iColumnIndex != dtSrc.Columns.Count - 1)
				{
					strSql += ",";
				}
			}
			strSql += ")";
			SqlConnection ConnSql = SqlOpen();
			SqlCommand CmdSql = new SqlCommand();
			CmdSql.Connection = ConnSql;
			CmdSql.CommandText = strSql;
			try
			{
				CmdSql.ExecuteNonQuery();
				SqlClose(ConnSql);
				return CheckExistsTable(strTabName);
			}
			catch (Exception ex)
			{
				ClsLog.LogWrite("SqlCreateTab exception.TableName:" + strTabName + ex.Message);
				return false;
			}
		}
		catch (Exception ex2)
		{
			ClsLog.LogWrite("SqlCreateTab Exception:" + ex2.Message);
			return false;
		}
	}

	public static bool CheckExistsTable(string tablename)
	{
		try
		{
			string tableNameStr = "select count(1) from sysobjects where name = '" + tablename + "'";
			using SqlConnection con = new SqlConnection(get_sqlconnectionstring());
			con.Open();
			SqlCommand cmd = new SqlCommand(tableNameStr, con);
			if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
			{
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("CheckExistsTable exception:" + ex.Message);
			return false;
		}
	}

	public static int SqlImportData(DataTable dt, bool bNeedClearData, string strTabName)
	{
		SqlConnection ConnSql = SqlOpen();
		SqlTransaction sqlT = ConnSql.BeginTransaction();
		SqlCommand CmdSql = new SqlCommand();
		CmdSql.Connection = ConnSql;
		CmdSql.Transaction = sqlT;
		try
		{
			if (bNeedClearData)
			{
				string strSql = "DELETE " + strTabName;
				CmdSql.CommandText = strSql;
				CmdSql.ExecuteNonQuery();
			}
			int iCount = 0;
			string strSqlKey = "INSERT " + strTabName + "([" + strTabName.Substring(0, 2).Trim().ToUpper() + "ID],";
			for (int iColumnIndex = 0; iColumnIndex < dt.Columns.Count; iColumnIndex++)
			{
				strSqlKey = strSqlKey + "[" + strTabName.Substring(0, 2).Trim().ToUpper() + dt.Columns[iColumnIndex].Caption + "]";
				if (iColumnIndex < dt.Columns.Count - 1)
				{
					strSqlKey += ", ";
				}
			}
			strSqlKey += ")  VALUES(";
			for (int iRowIndex = 0; iRowIndex < dt.Rows.Count; iRowIndex++)
			{
				string strSql2 = strSqlKey + "'" + Guid.NewGuid().ToString() + "',";
				for (int i = 0; i < dt.Columns.Count; i++)
				{
					string strValue = dt.Rows[iRowIndex][i].ToString();
					strSql2 = strSql2 + "N'" + strValue + "'";
					if (i < dt.Columns.Count - 1)
					{
						strSql2 += ", ";
					}
				}
				strSql2 += ") ";
				CmdSql.CommandText = strSql2;
				int iRetCount = CmdSql.ExecuteNonQuery();
				iCount += iRetCount;
			}
			sqlT.Commit();
			SqlClose(ConnSql);
			return iCount;
		}
		catch (Exception ex)
		{
			sqlT.Rollback();
			SqlClose(ConnSql);
			ClsLog.LogWrite("SqlImportData1 Exception:" + ex.Message);
			return 0;
		}
	}

	public static int SqlImportData(DataTable dt, string strTabName, string strCheckKeyWord, bool bIsCover)
	{
		try
		{
			int iCount = 0;
			SqlConnection ConnSql = SqlOpen();
			string strSqlKey = InitializInsHead(strTabName, dt);
			strSqlKey += ")  VALUES(";
			for (int iRowIndex = 0; iRowIndex < dt.Rows.Count; iRowIndex++)
			{
				string strSql = strSqlKey + "'" + Guid.NewGuid().ToString() + "',";
				bool bNeedCover = true;
				for (int iColumnIndex = 0; iColumnIndex < dt.Columns.Count; iColumnIndex++)
				{
					strSql = strSql + "N'" + dt.Rows[iRowIndex][iColumnIndex].ToString() + "'";
					if ((strTabName.Substring(0, 2).Trim().ToUpper() + dt.Columns[iColumnIndex].Caption).Trim().ToUpper() == strCheckKeyWord)
					{
						string strCheckValue = dt.Rows[iRowIndex][iColumnIndex].ToString();
						if (SqlCheckDataExist(strTabName, strCheckKeyWord, strCheckValue))
						{
							if (DialogResult.Yes == MessageBox.Show(strCheckValue + " 已經存在，是否覆蓋?", "系統提示", MessageBoxButtons.YesNo))
							{
								string strSqlDel = "DELETE  " + strTabName + "  WHERE [" + strCheckKeyWord + "]='" + strCheckValue + "'";
								SqlOperateData(strSqlDel);
							}
							else
							{
								bNeedCover = false;
							}
						}
					}
					if (iColumnIndex < dt.Columns.Count - 1)
					{
						strSql += ", ";
					}
				}
				if (bNeedCover)
				{
					strSql += ") ";
					SqlCommand CmdSql = new SqlCommand();
					CmdSql.Connection = ConnSql;
					CmdSql.CommandText = strSql;
					int iRetCount = CmdSql.ExecuteNonQuery();
					iCount += iRetCount;
				}
			}
			SqlClose(ConnSql);
			return iCount;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlImportData2 Exception:" + ex.Message);
			MessageBox.Show(ex.Message);
			return 0;
		}
	}

	private static string InitializInsHead(string strTabName, DataTable dt)
	{
		try
		{
			string strSqlKey = "INSERT " + strTabName + "([" + strTabName.Substring(0, 2).Trim().ToUpper() + "ID],";
			for (int iColumnIndex = 0; iColumnIndex < dt.Columns.Count; iColumnIndex++)
			{
				strSqlKey = strSqlKey + "[" + strTabName.Substring(0, 2).Trim().ToUpper() + dt.Columns[iColumnIndex].Caption + "]";
				if (iColumnIndex < dt.Columns.Count - 1)
				{
					strSqlKey += ", ";
				}
			}
			return strSqlKey;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("InitializInsHead Exception:" + ex.Message);
			return null;
		}
	}

	public static bool SqlCheckDataExist(string strTabName, string strCheckKeyWord, string strCheckValue)
	{
		try
		{
			string strSql = "SELECT COUNT(1) FROM " + strTabName + "  WHERE [" + strCheckKeyWord + "]='" + strCheckValue + "'";
			DataTable dt = SqlReadData(strSql);
			if (Convert.ToInt32(dt.Rows[0][0]) > 0)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlCheckDataExist exception:" + ex.Message);
			return false;
		}
	}

	public static bool SqlDelTab(string strTabName, string strConditionKeyWord, string strConditionKeyValue)
	{
		try
		{
			string strDelSql = "DELETE " + strTabName + "  WHERE [" + strConditionKeyWord + "]='" + strConditionKeyValue + "'";
			DataTable dt = SqlReadData(strDelSql);
			if (dt.Rows.Count > 0)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlCheckDataExist exception:" + ex.Message);
			return false;
		}
	}

	public static bool SqlCreateDatabase(string strDataBaseName)
	{
		try
		{
			if (SqlCheckDatabaseExist(strDataBaseName) > 0)
			{
				return true;
			}
			string strSql = "USE MASTER CREATE DATABASE " + strDataBaseName;
			string strSqlConnectionString = "Server=" + ClsXml.GetServerName() + ";Database=MASTER;uid=" + ClsXml.GetDataBaseUserID() + ";pwd=" + ClsXml.GetDataBasePwd();
			SqlConnection SqlCon = new SqlConnection(strSqlConnectionString);
			SqlCon.Open();
			SqlCommand SqlCmd = new SqlCommand();
			SqlCmd.Connection = SqlCon;
			SqlCmd.CommandText = strSql;
			SqlCmd.ExecuteNonQuery();
			SqlClose(SqlCon);
			if (SqlCheckDatabaseExist(strDataBaseName) > 0)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("CreateDatabase exception:" + ex.Message);
			return false;
		}
	}

	public static bool SqlDelDatabase(string strDataBaseName)
	{
		try
		{
			if (SqlCheckDatabaseExist(strDataBaseName) <= 0)
			{
				return true;
			}
			string strSql = "USE MASTER ALTER DATABASE " + strDataBaseName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE DROP DATABASE " + strDataBaseName;
			string strSqlConnectionString = "Server=" + ClsXml.GetServerName() + ";Database=MASTER;uid=" + ClsXml.GetDataBaseUserID() + ";pwd=" + ClsXml.GetDataBasePwd();
			SqlConnection SqlCon = new SqlConnection(strSqlConnectionString);
			SqlCon.Open();
			SqlCommand SqlCmd = new SqlCommand();
			SqlCmd.Connection = SqlCon;
			SqlCmd.CommandText = strSql;
			SqlCmd.ExecuteNonQuery();
			SqlClose(SqlCon);
			if (SqlCheckDatabaseExist(strDataBaseName) <= 0)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Delete Database exception:" + ex.Message);
			return false;
		}
	}

	public static int SqlCheckDatabaseExist(string strDataBaseName)
	{
		try
		{
			string strSql = "USE MASTER SELECT COUNT(*) FROM MASTER..SYSDATABASES WHERE NAME ='" + strDataBaseName + "'";
			DataTable dt = SqlReadData(strSql);
			return Convert.ToInt32(dt.Rows[0][0]);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlCheckDatabaseExist Exception:" + ex.Message);
			return 0;
		}
	}

	public static bool SqlCreatePrintDataTab()
	{
		string strIniFileSrcPath = Application.StartupPath + "\\" + ClsXml.GetInitializDataBaseTabsFileSrc();
		string strTabName = ClsXml.GetPrintDataTableName();
		DataTable dt = ClsExcel.GetDataTableFromExcel(strIniFileSrcPath);
		return SqlCreateTab(dt, strTabName, ClsPrintDataField.strKeyCode);
	}

	public static bool SqlCreateCartonSizeTab()
	{
		string strIniFileSrcPath = Application.StartupPath + "\\" + ClsXml.GetInitializDataBaseTabsFileSrc();
		string strTabName = ClsXml.GetCartonSizeTableName();
		DataTable dt = ClsExcel.GetDataTableFromExcel(strIniFileSrcPath, 1);
		return SqlCreateTab(dt, strTabName);
	}

	public static bool SqlCreateProductRecordMasterTab()
	{
		try
		{
			string strTabName = ClsXml.GetProductRecordMasterTableName();
			if (CheckExistsTable(strTabName))
			{
				return true;
			}
			int iVarcharLen = 1024;
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strSql = "CREATE TABLE " + strTabName + " ( [" + strPrefix + ClsProductRecordMasterField.strID + "] NVARCHAR(50) PRIMARY KEY,[" + strPrefix + ClsProductRecordMasterField.strBatchNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordMasterField.strKeyCode + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordMasterField.strMadeNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordMasterField.strCaseNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordMasterField.strBeginPrintTime + "] DATETIME,[" + strPrefix + ClsProductRecordMasterField.strEndPrintTime + "] DATETIME,[" + strPrefix + ClsProductRecordMasterField.strBeginBoxNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordMasterField.strEndBoxNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordMasterField.strBeginMachineNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordMasterField.strEndMachineNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordMasterField.strPrintStopStatus + "] NVARCHAR(" + iVarcharLen + "))";
			SqlOperateData(strSql);
			return CheckExistsTable(strTabName);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlCreateProductRecordMasterTab" + ex.Message);
			return false;
		}
	}

	public static bool SqlCreateProductRecordDetailTab()
	{
		try
		{
			string strTabName = ClsXml.GetProductRecordDetailTableName();
			if (CheckExistsTable(strTabName))
			{
				return true;
			}
			int iVarcharLen = 1024;
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strSql = "CREATE TABLE " + strTabName + " ( [" + strPrefix + ClsProductRecordDetailField.strID + "] NVARCHAR(50) PRIMARY KEY,[" + strPrefix + ClsProductRecordDetailField.strPageNo + "] int ,[" + strPrefix + ClsProductRecordDetailField.strKeyCode + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordDetailField.strMadeNo + "]  NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordDetailField.strCaseNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordDetailField.strCurrentPrintBoxNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordDetailField.strCurrentPrintMachineNo + "] NVARCHAR(" + iVarcharLen + "),[" + strPrefix + ClsProductRecordDetailField.strPageBeginTime + "] DATETIME,[" + strPrefix + ClsProductRecordDetailField.strPageEndTime + "] DATETIME,[" + strPrefix + ClsProductRecordDetailField.strBatchNo + "] NVARCHAR(" + iVarcharLen + "))";
			SqlOperateData(strSql);
			return CheckExistsTable(strTabName);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlCreateProductRecordTab" + ex.Message);
			return false;
		}
	}

	public static bool SqlCheckDatabaseStatus()
	{
		try
		{
			string strSql = "SELECT 1";
			DataTable dt = SqlReadData(strSql);
			if (dt != null && dt.Rows.Count > 0)
			{
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static string ResetSingleQuotationMarkForSql(string strSrc)
	{
		string strRlt = "";
		return strSrc.Replace("'", "''");
	}

	public static bool CheckDatabaseIsConnected(bool bUseDefaultConnectionString = true, string strConnectionString = "")
	{
		if (bUseDefaultConnectionString)
		{
			strConnectionString = get_sqlconnectionstring();
		}
		SqlConnection connection = new SqlConnection(strConnectionString);
		try
		{
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}
			return true;
		}
		catch
		{
			return false;
		}
	}
}
