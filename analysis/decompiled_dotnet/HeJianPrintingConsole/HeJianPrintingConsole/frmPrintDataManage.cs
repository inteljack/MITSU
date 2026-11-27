using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmPrintDataManage : frmBase
{
	public enum EnumIniDataBaseTabSheetIndex
	{
		PrintData,
		CartonSize
	}

	private Thread m_ThreadExport;

	private string m_strExcelFileName = "";

	private DataTable m_dtPrintData;

	private IContainer components;

	private Panel pnlClientFrontSideCartonSize;

	private Panel pnlClientTopDataInput;

	private TextBox textBoxKeyCodeValue;

	private Label lblKeyCode;

	private Button btnReadData;

	private Button btnExport;

	private Button btnModify;

	private Button btnDel;

	private Button btnImport;

	private Button btnAdd;

	private TextBox textBoxID;

	private Label label2;

	private SaveFileDialog saveFileDialog1;

	private GroupBox groupBox1;

	private TextBox textBoxCaseNo;

	private Label label10;

	private GroupBox gbDataSide;

	private TextBox textBoxPlace;

	private TextBox textBoxModelValueSide;

	private GroupBox gbDataFront;

	private TextBox textBoxHzUnit;

	private TextBox textBoxHzValue;

	private TextBox textBoxVoltageUnit;

	private TextBox textBoxVoltageValue;

	private TextBox textBoxPhaseUnit;

	private TextBox textBoxPhaseValue;

	private TextBox textBoxOutputUnit;

	private TextBox textBoxOutputValue;

	private Label label7;

	private TextBox textBoxOutput;

	private TextBox textBoxBoreUnit;

	private TextBox textBoxBoreValue;

	private Label label5;

	private TextBox textBoxBore;

	private TextBox textBoxCodeValue;

	private Label label8;

	private TextBox textBoxCode;

	private TextBox textBoxTokki;

	private TextBox textBoxModelValue;

	private Label label1;

	private TextBox textBoxModel;

	private TextBox textBoxInsidePrintWordEnd;

	private TextBox textBoxInsidePrintWordBegin;

	private TextBox textBoxOutSidePrintWordEnd;

	private TextBox textBoxOutSidePrintWordBegin;

	private TextBox textBoxPrintWordHeight;

	private Button btnBrowse;

	private Label lblExportTip;

	public frmPrintDataManage()
	{
		InitializeComponent();
	}

	private void btnReadData_Click(object sender, EventArgs e)
	{
		try
		{
			string strTabName = ClsXml.GetPrintDataTableName();
			string strSql = "SELECT ROW_NUMBER() OVER(ORDER BY [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "]) AS " + strTabName.Substring(0, 2).Trim().ToUpper() + "Number,*  FROM " + strTabName + "  WHERE [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "] LIKE '%" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxKeyCodeValue.Text) + "%' ORDER BY [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "]";
			frmPrintDataSelect frmPDS = new frmPrintDataSelect(strSql, textBoxKeyCodeValue.Text);
			frmPDS.Icon = base.Icon;
			string strLastMainTitle = ((frmMain)base.Parent.Parent.Parent).Text;
			((frmMain)base.Parent.Parent.Parent).Text = frmMain.m_strFrmMainTitle + "---" + frmPDS.Text;
			frmPDS.ShowDialog();
			((frmMain)base.Parent.Parent.Parent).Text = strLastMainTitle;
			ImportData(ClsShare.m_dtSelData);
			ImPortCartonSize();
			if (ClsShare.m_dtSelData != null && ClsShare.m_dtSelData.Rows.Count > 0)
			{
				Refresh();
				string strCheckFieldExceedMaxLenMsg = "";
				if (CheckFieldExceedMaxLen(ref strCheckFieldExceedMaxLenMsg))
				{
					ClsMsgBox.MessageBoxShowDefWarn(this, strCheckFieldExceedMaxLenMsg);
				}
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("資料管理窗口中讀取資料異常:" + ex.Message);
		}
	}

	private void ImportData(DataTable dtSrc)
	{
		if (dtSrc == null || dtSrc.Rows.Count <= 0)
		{
			return;
		}
		for (int iIndex = 0; iIndex < dtSrc.Columns.Count; iIndex++)
		{
			string strCaption = dtSrc.Columns[iIndex].Caption.Trim().ToUpper();
			string strValue = dtSrc.Rows[0][iIndex].ToString();
			if (strCaption == ClsPrintDataField.strID)
			{
				textBoxID.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strKeyCode)
			{
				textBoxKeyCodeValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strCaseNo)
			{
				textBoxCaseNo.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strModel)
			{
				textBoxModel.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strModelValue)
			{
				textBoxModelValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strBore)
			{
				textBoxBore.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strBoreValue)
			{
				textBoxBoreValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strBoreUnit)
			{
				textBoxBoreUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strOutput)
			{
				textBoxOutput.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strOutputValue)
			{
				textBoxOutputValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strOutputUnit)
			{
				textBoxOutputUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strPhaseValue)
			{
				textBoxPhaseValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strPhaseUnit)
			{
				textBoxPhaseUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strVoltageValue)
			{
				textBoxVoltageValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strVoltageUnit)
			{
				textBoxVoltageUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strHzValue)
			{
				textBoxHzValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strHzUnit)
			{
				textBoxHzUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strTokki)
			{
				textBoxTokki.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strCode)
			{
				textBoxCode.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strCodeValue)
			{
				textBoxCodeValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strPlace)
			{
				textBoxPlace.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strCaseNo)
			{
				textBoxCaseNo.Text = strValue;
			}
		}
	}

	private void ImPortCartonSize()
	{
		try
		{
			string strCaseNo = textBoxCaseNo.Text.Trim();
			if (strCaseNo.Length <= 0)
			{
				return;
			}
			string strTabName = ClsXml.GetCartonSizeTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strSql = "SELECT * FROM " + strTabName + "  WHERE [" + strPrefix + ClsCartonSizeFieldKey.strCaseNo + "]='" + strCaseNo + "'";
			DataTable dt = ClsDataBase.SqlReadData(strSql);
			if (dt == null || dt.Rows.Count <= 0)
			{
				return;
			}
			for (int iIndex = 0; iIndex < dt.Columns.Count; iIndex++)
			{
				string strCaption = dt.Columns[iIndex].Caption.Substring(2).Trim().ToUpper();
				string strValue = dt.Rows[0][iIndex].ToString();
				if (strCaption == ClsCartonSizeFieldKey.strPrintWordHeight)
				{
					textBoxPrintWordHeight.Text = strValue;
				}
				else if (strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordBegin)
				{
					textBoxOutSidePrintWordBegin.Text = strValue;
				}
				else if (strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordEnd)
				{
					textBoxOutSidePrintWordEnd.Text = strValue;
				}
				else if (strCaption == ClsCartonSizeFieldKey.strInsidePrintWordBegin)
				{
					textBoxInsidePrintWordBegin.Text = strValue;
				}
				else if (strCaption == ClsCartonSizeFieldKey.strInsidePrintWordEnd)
				{
					textBoxInsidePrintWordEnd.Text = strValue;
				}
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("ImPortCartonSize exception:" + ex.Message);
		}
	}

	private void textBoxKeyCodeValue_KeyUp(object sender, KeyEventArgs e)
	{
	}

	private void frmPrintDataManage_Load(object sender, EventArgs e)
	{
		textBoxKeyCodeValue_KeyUp(sender, null);
	}

	private void btnAdd_Click(object sender, EventArgs e)
	{
		try
		{
			if (textBoxKeyCodeValue.Text.Trim().Length <= 0)
			{
				MessageBox.Show("Key code 不可為空!");
				return;
			}
			string strCheckFieldExceedMaxLenMsg = "";
			if (CheckFieldExceedMaxLen(ref strCheckFieldExceedMaxLenMsg))
			{
				ClsMsgBox.MessageBoxShowDefWarn(this, strCheckFieldExceedMaxLenMsg);
				return;
			}
			string strTabName = ClsXml.GetPrintDataTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strKeyWord = strPrefix + ClsPrintDataField.strKeyCode.Trim().ToUpper();
			string strKeyValue = ClsDataBase.ResetSingleQuotationMarkForSql(textBoxKeyCodeValue.Text);
			if (ClsDataBase.SqlCheckDataExist(strTabName, strKeyWord, strKeyValue))
			{
				MessageBox.Show("數據已經存在，无法新增!");
				return;
			}
			textBoxID.Text = Guid.NewGuid().ToString();
			int iSqlEffectCount = SqlInsData();
			if (iSqlEffectCount > 0)
			{
				MessageBox.Show("新增成功");
				return;
			}
			ClsLog.LogWrite("噴印資料新增時插入失敗導致新增失敗!");
			MessageBox.Show("新增失败，請檢查數據庫連接是否正常!");
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	private bool CheckFieldExceedMaxLen(ref string strMsg)
	{
		try
		{
			ClsGetPrintDataFieldMaxLen ClsGPDFM = new ClsGetPrintDataFieldMaxLen();
			strMsg = "以下欄位資料長度超過系統設定：\n\n";
			bool bRlt = false;
			if (textBoxKeyCodeValue.Text.Trim().Length > ClsGPDFM.iLen_Keycode)
			{
				strMsg += GroupTipStr("Keycode", textBoxKeyCodeValue.Text.Trim(), ClsGPDFM.iLen_Keycode);
				bRlt = true;
			}
			if (textBoxModel.Text.Trim().Length > ClsGPDFM.iLen_Model)
			{
				strMsg += GroupTipStr("Model", textBoxModel.Text.Trim(), ClsGPDFM.iLen_Model);
				bRlt = true;
			}
			if (textBoxModelValue.Text.Trim().Length > ClsGPDFM.iLen_ModelValue)
			{
				strMsg += GroupTipStr("ModelValue", textBoxModelValue.Text.Trim(), ClsGPDFM.iLen_ModelValue);
				bRlt = true;
			}
			if (textBoxBore.Text.Trim().Length > ClsGPDFM.iLen_Bore)
			{
				strMsg += GroupTipStr("Bore", textBoxBore.Text.Trim(), ClsGPDFM.iLen_Bore);
				bRlt = true;
			}
			if (textBoxBoreValue.Text.Trim().Length > ClsGPDFM.iLen_BoreValue)
			{
				strMsg += GroupTipStr("BoreValue", textBoxBoreValue.Text.Trim(), ClsGPDFM.iLen_BoreValue);
				bRlt = true;
			}
			if (textBoxBoreUnit.Text.Trim().Length > ClsGPDFM.iLen_BoreUnit)
			{
				strMsg += GroupTipStr("BoreUnit", textBoxBoreUnit.Text.Trim(), ClsGPDFM.iLen_BoreUnit);
				bRlt = true;
			}
			if (textBoxOutput.Text.Trim().Length > ClsGPDFM.iLen_Output)
			{
				strMsg += GroupTipStr("Output", textBoxOutput.Text.Trim(), ClsGPDFM.iLen_Output);
				bRlt = true;
			}
			if (textBoxOutputValue.Text.Trim().Length > ClsGPDFM.iLen_OutputValue)
			{
				strMsg += GroupTipStr("OutputValue", textBoxOutputValue.Text.Trim(), ClsGPDFM.iLen_OutputValue);
				bRlt = true;
			}
			if (textBoxOutputUnit.Text.Trim().Length > ClsGPDFM.iLen_OutputUnit)
			{
				strMsg += GroupTipStr("OutputUnit", textBoxOutputUnit.Text.Trim(), ClsGPDFM.iLen_OutputUnit);
				bRlt = true;
			}
			if (textBoxPhaseValue.Text.Trim().Length > ClsGPDFM.iLen_PhaseValue)
			{
				strMsg += GroupTipStr("PhaseValue", textBoxPhaseValue.Text.Trim(), ClsGPDFM.iLen_PhaseValue);
				bRlt = true;
			}
			if (textBoxPhaseUnit.Text.Trim().Length > ClsGPDFM.iLen_PhaseUnit)
			{
				strMsg += GroupTipStr("PhaseUnit", textBoxPhaseUnit.Text.Trim(), ClsGPDFM.iLen_PhaseUnit);
				bRlt = true;
			}
			if (textBoxVoltageValue.Text.Trim().Length > ClsGPDFM.iLen_VoltageValue)
			{
				strMsg += GroupTipStr("VoltageValue", textBoxVoltageValue.Text.Trim(), ClsGPDFM.iLen_VoltageValue);
				bRlt = true;
			}
			if (textBoxVoltageUnit.Text.Trim().Length > ClsGPDFM.iLen_VoltageUnit)
			{
				strMsg += GroupTipStr("VoltageUnit", textBoxVoltageUnit.Text.Trim(), ClsGPDFM.iLen_VoltageUnit);
				bRlt = true;
			}
			if (textBoxHzValue.Text.Trim().Length > ClsGPDFM.iLen_HzValue)
			{
				strMsg += GroupTipStr("HzValue", textBoxHzValue.Text.Trim(), ClsGPDFM.iLen_HzValue);
				bRlt = true;
			}
			if (textBoxHzUnit.Text.Trim().Length > ClsGPDFM.iLen_HzUnit)
			{
				strMsg += GroupTipStr("HzUnit", textBoxHzUnit.Text.Trim(), ClsGPDFM.iLen_HzUnit);
				bRlt = true;
			}
			if (textBoxTokki.Text.Trim().Length > ClsGPDFM.iLen_Tokki)
			{
				strMsg += GroupTipStr("Tokki", textBoxTokki.Text.Trim(), ClsGPDFM.iLen_Tokki);
				bRlt = true;
			}
			if (textBoxCode.Text.Trim().Length > ClsGPDFM.iLen_Code)
			{
				strMsg += GroupTipStr("Code", textBoxCode.Text.Trim(), ClsGPDFM.iLen_Code);
				bRlt = true;
			}
			if (textBoxCodeValue.Text.Trim().Length > ClsGPDFM.iLen_CodeValue)
			{
				strMsg += GroupTipStr("CodeValue", textBoxCodeValue.Text.Trim(), ClsGPDFM.iLen_CodeValue);
				bRlt = true;
			}
			if (textBoxPlace.Text.Trim().Length > ClsGPDFM.iLen_Place)
			{
				strMsg += GroupTipStr("Place", textBoxPlace.Text.Trim(), ClsGPDFM.iLen_Place);
				bRlt = true;
			}
			if (textBoxCaseNo.Text.Trim().Length > ClsGPDFM.iLen_CaseNo)
			{
				strMsg += GroupTipStr("CaseNo", textBoxCaseNo.Text.Trim(), ClsGPDFM.iLen_CaseNo);
				bRlt = true;
			}
			return bRlt;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("CheckFieldExceedMaxLen exception:" + ex.Message);
			return true;
		}
	}

	private string GroupTipStr(string strKey, string strValue, int iLen)
	{
		return strKey + " [" + strValue + "]  系統設定  " + iLen + " \n";
	}

	private int SqlInsData()
	{
		string strPrintDataTabName = ClsXml.GetPrintDataTableName();
		string strSql = "INSERT " + strPrintDataTabName + " (";
		string strColumnNamePrefix = strPrintDataTabName.Substring(0, 2);
		strSql = strSql + "[" + strColumnNamePrefix + ClsPrintDataField.strID + "],[" + strColumnNamePrefix + ClsPrintDataField.strKeyCode + "],[" + strColumnNamePrefix + ClsPrintDataField.strCaseNo + "],[" + strColumnNamePrefix + ClsPrintDataField.strModel + "],[" + strColumnNamePrefix + ClsPrintDataField.strModelValue + "],[" + strColumnNamePrefix + ClsPrintDataField.strBore + "],[" + strColumnNamePrefix + ClsPrintDataField.strBoreValue + "],[" + strColumnNamePrefix + ClsPrintDataField.strBoreUnit + "],[" + strColumnNamePrefix + ClsPrintDataField.strOutput + "],[" + strColumnNamePrefix + ClsPrintDataField.strOutputValue + "],[" + strColumnNamePrefix + ClsPrintDataField.strOutputUnit + "],[" + strColumnNamePrefix + ClsPrintDataField.strPhaseValue + "],[" + strColumnNamePrefix + ClsPrintDataField.strPhaseUnit + "],[" + strColumnNamePrefix + ClsPrintDataField.strVoltageValue + "],[" + strColumnNamePrefix + ClsPrintDataField.strVoltageUnit + "],[" + strColumnNamePrefix + ClsPrintDataField.strHzValue + "],[" + strColumnNamePrefix + ClsPrintDataField.strHzUnit + "],[" + strColumnNamePrefix + ClsPrintDataField.strCode + "],[" + strColumnNamePrefix + ClsPrintDataField.strCodeValue + "],[" + strColumnNamePrefix + ClsPrintDataField.strTokki + "],[" + strColumnNamePrefix + ClsPrintDataField.strPlace + "]) ";
		strSql = strSql + " VALUES( N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxID.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxKeyCodeValue.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxCaseNo.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxModel.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxModelValue.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxBore.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxBoreValue.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxBoreUnit.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxOutput.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxOutputValue.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxOutputUnit.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxPhaseValue.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxPhaseUnit.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxVoltageValue.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxVoltageUnit.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxHzValue.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxHzUnit.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxCode.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxCodeValue.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxTokki.Text) + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxPlace.Text) + "')";
		return ClsDataBase.SqlOperateData(strSql);
	}

	private void MsgBoxTip_SqlOperate(int iSqlEffectCount, string strDest)
	{
		if (iSqlEffectCount > 0)
		{
			MessageBox.Show(strDest + "成功");
		}
		else
		{
			MessageBox.Show(strDest + "失敗");
		}
	}

	private void btnImport_Click(object sender, EventArgs e)
	{
		try
		{
			string strExcelFileName = "";
			if (!ClsShare.GetOpenDlgFileName("選擇導入文件", "All File(*.*)|*.*|Excel(*.xls)|*.xls", 2, ClsXml.GetImportFloder(), ref strExcelFileName))
			{
				return;
			}
			DataTable dtPrintData = ClsExcel.GetDataTableFromExcel(strExcelFileName);
			ClsExcel.DelSpaceColumn(ref dtPrintData);
			ClsExcel.DelSpaceRow(0, ref dtPrintData);
			if (!CheckPrintDataFormat(dtPrintData))
			{
				MessageBox.Show("導入格式錯誤");
				return;
			}
			string strTabName = ClsXml.GetPrintDataTableName();
			bool bNeedClearData = false;
			if (ClsDataBase.CheckExistsTable(strTabName))
			{
				string strSqlSelect = "SELECT " + strTabName.Substring(0, 2).Trim().ToUpper() + ClsPrintDataField.strID + " FROM " + strTabName;
				DataTable dtTmp = ClsDataBase.SqlReadData(strSqlSelect);
				if (dtTmp != null && dtTmp.Rows.Count > 0)
				{
					if (DialogResult.Yes != MessageBox.Show("數據已經存在，是否清空?", "系統提示", MessageBoxButtons.YesNo))
					{
						return;
					}
					bNeedClearData = true;
				}
			}
			if (ClsDataBase.SqlCheckDatabaseStatus())
			{
				if (ImportExcelToSql(dtPrintData, bNeedClearData, ClsXml.GetPrintDataTableName(), 0))
				{
					MessageBox.Show("導入成功");
				}
				else
				{
					MessageBox.Show("導入失敗，可能原因如下：\n\n1:數據有重复的Key code\n\n2:表格被其他程序占用\n\n3:未初始化數據庫\n\n4:數據庫连接不正常");
				}
			}
			else
			{
				MessageBox.Show("導入失敗，可能原因如下：\n\n1:數據有重复的Key code\n\n2:表格被其他程序占用\n\n3:未初始化數據庫\n\n4:數據庫连接不正常");
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	private bool CheckPrintDataFormat(DataTable dtSrc)
	{
		try
		{
			bool bRlt = true;
			for (int iColumnIndex = 0; iColumnIndex < dtSrc.Columns.Count; iColumnIndex++)
			{
				string strCaption = dtSrc.Columns[iColumnIndex].Caption.Trim().ToUpper();
				if (strCaption == ClsPrintDataField.strBore.Trim().ToUpper() || strCaption == ClsPrintDataField.strBoreUnit.Trim().ToUpper() || strCaption == ClsPrintDataField.strBoreValue.Trim().ToUpper() || strCaption == ClsPrintDataField.strCaseNo.Trim().ToUpper() || strCaption == ClsPrintDataField.strCode.Trim().ToUpper() || strCaption == ClsPrintDataField.strCodeValue.Trim().ToUpper() || strCaption == ClsPrintDataField.strHzUnit.Trim().ToUpper() || strCaption == ClsPrintDataField.strHzValue.Trim().ToUpper() || strCaption == ClsPrintDataField.strID.Trim().ToUpper() || strCaption == ClsPrintDataField.strKeyCode.Trim().ToUpper() || strCaption == ClsPrintDataField.strModel.Trim().ToUpper() || strCaption == ClsPrintDataField.strModelValue.Trim().ToUpper() || strCaption == ClsPrintDataField.strOutput.Trim().ToUpper() || strCaption == ClsPrintDataField.strOutputUnit.Trim().ToUpper() || strCaption == ClsPrintDataField.strOutputValue.Trim().ToUpper() || strCaption == ClsPrintDataField.strPhaseUnit.Trim().ToUpper() || strCaption == ClsPrintDataField.strPhaseValue.Trim().ToUpper() || strCaption == ClsPrintDataField.strPlace.Trim().ToUpper() || strCaption == ClsPrintDataField.strTokki.Trim().ToUpper() || strCaption == ClsPrintDataField.strVoltageUnit.Trim().ToUpper() || strCaption == ClsPrintDataField.strVoltageValue.Trim().ToUpper())
				{
					bRlt = true;
					continue;
				}
				bRlt = false;
				break;
			}
			return bRlt;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			return false;
		}
	}

	public static bool ImportExcelToSql(DataTable dtExcel, bool bNeedClearData, string strTabName, int iSheetIndex)
	{
		try
		{
			if (ClsDataBase.SqlImportData(dtExcel, bNeedClearData, strTabName) > 0)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("ImportExcelToSql exception:" + ex.Message);
			return false;
		}
	}

	private void frmPrintDataManage_Resize(object sender, EventArgs e)
	{
	}

	private void btnModify_Click(object sender, EventArgs e)
	{
		if (textBoxKeyCodeValue.Text.Trim().Length <= 0)
		{
			MessageBox.Show("Key code 不可為空!");
			return;
		}
		string strCheckFieldExceedMaxLenMsg = "";
		if (CheckFieldExceedMaxLen(ref strCheckFieldExceedMaxLenMsg))
		{
			ClsMsgBox.MessageBoxShowDefWarn(this, strCheckFieldExceedMaxLenMsg);
			return;
		}
		if (SqlModifyData(textBoxID.Text.Trim()) > 0)
		{
			MessageBox.Show("修改成功");
			return;
		}
		ClsLog.LogWrite("噴印資料修改失敗!");
		MessageBox.Show("修改失敗，請檢查數據庫連接是否正常!");
	}

	private int SqlModifyData(string strGuid)
	{
		if (SqlDelData(strGuid) > 0)
		{
			return SqlInsData();
		}
		return 0;
	}

	private void btnDel_Click(object sender, EventArgs e)
	{
		if (textBoxKeyCodeValue.Text.Trim().Length <= 0)
		{
			MessageBox.Show("Key code 不可為空!");
			return;
		}
		int iSqlEffectCount = SqlDelData(textBoxID.Text.Trim());
		if (iSqlEffectCount > 0)
		{
			ResetDataOnUI();
			MessageBox.Show("刪除成功");
		}
		else
		{
			ClsLog.LogWrite("噴印資料刪除失敗!");
			MessageBox.Show("刪除失敗，請檢查數據庫連接是否正常!");
		}
	}

	private void ResetDataOnUI()
	{
		for (int iControlIndex = 0; iControlIndex < pnlClientTopDataInput.Controls.Count; iControlIndex++)
		{
			if (pnlClientTopDataInput.Controls[iControlIndex] is TextBox tbTmp)
			{
				tbTmp.Text = "";
			}
		}
		for (int i = 0; i < pnlClientFrontSideCartonSize.Controls.Count; i++)
		{
			if (!(pnlClientFrontSideCartonSize.Controls[i] is GroupBox gbTmp))
			{
				continue;
			}
			for (int iCtrlIndexOnGB = 0; iCtrlIndexOnGB < gbTmp.Controls.Count; iCtrlIndexOnGB++)
			{
				if (gbTmp.Controls[iCtrlIndexOnGB] is TextBox tbTmp2)
				{
					tbTmp2.Text = "";
				}
			}
		}
	}

	private int SqlDelData(string strGuid)
	{
		string strPrintDataTabName = ClsXml.GetPrintDataTableName();
		string strSql = "DELETE " + strPrintDataTabName;
		string strColumnNamePrefix = strPrintDataTabName.Substring(0, 2);
		strSql = strSql + "  WHERE [" + strColumnNamePrefix + ClsPrintDataField.strID + "]='" + strGuid + "'";
		return ClsDataBase.SqlOperateData(strSql);
	}

	private void textBoxModelValue_TextChanged(object sender, EventArgs e)
	{
		textBoxModelValueSide.Text = textBoxModelValue.Text;
	}

	private void btnExport_Click(object sender, EventArgs e)
	{
		try
		{
			string strPrintDataTabName = ClsXml.GetPrintDataTableName();
			if (!ClsDataBase.CheckExistsTable(strPrintDataTabName))
			{
				MessageBox.Show("導出失敗，請檢查數據庫連接是否正常!");
				return;
			}
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "導出噴印數據";
			sfd.Filter = "All Files(*.*)|*.*|Excel(*.xls)|*.xls";
			sfd.FilterIndex = 2;
			sfd.InitialDirectory = ClsXml.GetExportFloder();
			if (DialogResult.OK == sfd.ShowDialog())
			{
				m_strExcelFileName = sfd.FileName;
				btnExport.Enabled = false;
				lblExportTip.Visible = true;
				string strTabName = ClsXml.GetPrintDataTableName();
				string strSql = "SELECT *  FROM " + strTabName + " ORDER BY [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "]";
				m_dtPrintData = new DataTable();
				m_dtPrintData = ClsDataBase.SqlReadData(strSql);
				m_dtPrintData.Columns.RemoveAt(0);
				for (int iColumnIndex = 0; iColumnIndex < m_dtPrintData.Columns.Count; iColumnIndex++)
				{
					string strTmp = m_dtPrintData.Columns[iColumnIndex].Caption;
					m_dtPrintData.Columns[iColumnIndex].Caption = strTmp.Substring(2);
				}
				Refresh();
				ClsExcel.ExportExcel(m_dtPrintData, m_strExcelFileName, "PrintData");
				lblExportTip.Visible = false;
				btnExport.Enabled = true;
				Refresh();
				if (File.Exists(m_strExcelFileName))
				{
					MessageBox.Show("導出成功");
				}
				else
				{
					MessageBox.Show("導出失敗");
				}
			}
		}
		catch (Exception ex)
		{
			lblExportTip.Visible = false;
			btnExport.Enabled = true;
			Refresh();
			MessageBox.Show(ex.Message);
		}
	}

	private void sfd_FileOk(object sender, EventArgs e)
	{
		SaveFileDialog sfd = (SaveFileDialog)sender;
		if (File.Exists(sfd.FileName))
		{
			switch (MessageBox.Show("文件已存在，是否覆蓋？是則覆蓋，否則追加，取消則退出", "系統提示", MessageBoxButtons.YesNoCancel))
			{
			case DialogResult.Yes:
				File.Delete(sfd.FileName);
				ClsExcel.CreateBlankExcel(sfd.FileName);
				break;
			case DialogResult.Cancel:
				sfd.FileName = "";
				((CancelEventArgs)e).Cancel = true;
				break;
			}
		}
	}

	private DataTable IniExportDataTab()
	{
		DataTable dtDest = new DataTable();
		dtDest.Columns.Add();
		dtDest.Columns[0].ColumnName = ClsPrintDataField.strKeyCode;
		dtDest.Columns[0].Caption = ClsPrintDataField.strKeyCode;
		dtDest.Columns.Add();
		dtDest.Columns[1].ColumnName = ClsPrintDataField.strCaseNo;
		dtDest.Columns[1].Caption = ClsPrintDataField.strCaseNo;
		dtDest.Columns.Add();
		dtDest.Columns[2].ColumnName = ClsPrintDataField.strModel;
		dtDest.Columns[2].Caption = ClsPrintDataField.strModel;
		dtDest.Columns.Add();
		dtDest.Columns[3].ColumnName = ClsPrintDataField.strModelValue;
		dtDest.Columns[3].Caption = ClsPrintDataField.strModelValue;
		dtDest.Columns.Add();
		dtDest.Columns[4].ColumnName = ClsPrintDataField.strBore;
		dtDest.Columns[4].Caption = ClsPrintDataField.strBore;
		dtDest.Columns.Add();
		dtDest.Columns[5].ColumnName = ClsPrintDataField.strBoreValue;
		dtDest.Columns[5].Caption = ClsPrintDataField.strBoreValue;
		dtDest.Columns.Add();
		dtDest.Columns[6].ColumnName = ClsPrintDataField.strBoreUnit;
		dtDest.Columns[6].Caption = ClsPrintDataField.strBoreUnit;
		dtDest.Columns.Add();
		dtDest.Columns[7].ColumnName = ClsPrintDataField.strOutput;
		dtDest.Columns[7].Caption = ClsPrintDataField.strOutput;
		dtDest.Columns.Add();
		dtDest.Columns[8].ColumnName = ClsPrintDataField.strOutputValue;
		dtDest.Columns[8].Caption = ClsPrintDataField.strOutputValue;
		dtDest.Columns.Add();
		dtDest.Columns[9].ColumnName = ClsPrintDataField.strOutputUnit;
		dtDest.Columns[9].Caption = ClsPrintDataField.strOutputUnit;
		dtDest.Columns.Add();
		dtDest.Columns[10].ColumnName = ClsPrintDataField.strPhaseValue;
		dtDest.Columns[10].Caption = ClsPrintDataField.strPhaseValue;
		dtDest.Columns.Add();
		dtDest.Columns[11].ColumnName = ClsPrintDataField.strPhaseUnit;
		dtDest.Columns[11].Caption = ClsPrintDataField.strPhaseUnit;
		dtDest.Columns.Add();
		dtDest.Columns[12].ColumnName = ClsPrintDataField.strVoltageValue;
		dtDest.Columns[12].ColumnName = ClsPrintDataField.strVoltageValue;
		dtDest.Columns.Add();
		dtDest.Columns[13].ColumnName = ClsPrintDataField.strVoltageUnit;
		dtDest.Columns[13].Caption = ClsPrintDataField.strVoltageUnit;
		dtDest.Columns.Add();
		dtDest.Columns[14].ColumnName = ClsPrintDataField.strHzValue;
		dtDest.Columns[14].Caption = ClsPrintDataField.strHzValue;
		dtDest.Columns.Add();
		dtDest.Columns[15].ColumnName = ClsPrintDataField.strHzUnit;
		dtDest.Columns[15].Caption = ClsPrintDataField.strHzUnit;
		dtDest.Columns.Add();
		dtDest.Columns[16].ColumnName = ClsPrintDataField.strCode;
		dtDest.Columns[16].Caption = ClsPrintDataField.strCode;
		dtDest.Columns.Add();
		dtDest.Columns[17].ColumnName = ClsPrintDataField.strCodeValue;
		dtDest.Columns[17].Caption = ClsPrintDataField.strCodeValue;
		dtDest.Columns.Add();
		dtDest.Columns[18].ColumnName = ClsPrintDataField.strTokki;
		dtDest.Columns[18].Caption = ClsPrintDataField.strTokki;
		dtDest.Columns.Add();
		dtDest.Columns[19].ColumnName = ClsPrintDataField.strPlace;
		dtDest.Columns[19].Caption = ClsPrintDataField.strPlace;
		dtDest.Rows.Add();
		dtDest.Rows[0][0] = textBoxKeyCodeValue.Text;
		dtDest.Rows[0][1] = textBoxCaseNo.Text;
		dtDest.Rows[0][2] = textBoxModel.Text;
		dtDest.Rows[0][3] = textBoxModelValue.Text;
		dtDest.Rows[0][4] = textBoxBore.Text;
		dtDest.Rows[0][5] = textBoxBoreValue.Text;
		dtDest.Rows[0][6] = textBoxBoreUnit.Text;
		dtDest.Rows[0][7] = textBoxOutput.Text;
		dtDest.Rows[0][8] = textBoxOutputValue.Text;
		dtDest.Rows[0][9] = textBoxOutputUnit.Text;
		dtDest.Rows[0][10] = textBoxPhaseValue.Text;
		dtDest.Rows[0][11] = textBoxPhaseUnit.Text;
		dtDest.Rows[0][12] = textBoxVoltageValue.Text;
		dtDest.Rows[0][13] = textBoxVoltageUnit.Text;
		dtDest.Rows[0][14] = textBoxHzValue.Text;
		dtDest.Rows[0][15] = textBoxHzUnit.Text;
		dtDest.Rows[0][16] = textBoxCode.Text;
		dtDest.Rows[0][17] = textBoxCodeValue.Text;
		dtDest.Rows[0][18] = textBoxTokki.Text;
		dtDest.Rows[0][19] = textBoxPlace.Text;
		return dtDest;
	}

	private void pnlClientFront_Paint(object sender, PaintEventArgs e)
	{
	}

	private void btnBrowse_Click(object sender, EventArgs e)
	{
		try
		{
			string strTabName = ClsXml.GetPrintDataTableName();
			string strSql = "SELECT *  FROM " + strTabName;
			frmPrintDataBrowse frmPDB = new frmPrintDataBrowse(strSql);
			frmPDB.ShowDialog();
		}
		catch
		{
		}
	}

	private void gbDataFront_Paint(object sender, PaintEventArgs e)
	{
		ClsShare.RepainGroupBoxSide(sender, e, this);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.pnlClientFrontSideCartonSize = new System.Windows.Forms.Panel();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.textBoxInsidePrintWordEnd = new System.Windows.Forms.TextBox();
		this.textBoxInsidePrintWordBegin = new System.Windows.Forms.TextBox();
		this.textBoxOutSidePrintWordEnd = new System.Windows.Forms.TextBox();
		this.textBoxOutSidePrintWordBegin = new System.Windows.Forms.TextBox();
		this.textBoxPrintWordHeight = new System.Windows.Forms.TextBox();
		this.textBoxCaseNo = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.gbDataSide = new System.Windows.Forms.GroupBox();
		this.textBoxPlace = new System.Windows.Forms.TextBox();
		this.textBoxModelValueSide = new System.Windows.Forms.TextBox();
		this.gbDataFront = new System.Windows.Forms.GroupBox();
		this.lblExportTip = new System.Windows.Forms.Label();
		this.textBoxHzUnit = new System.Windows.Forms.TextBox();
		this.textBoxHzValue = new System.Windows.Forms.TextBox();
		this.textBoxVoltageUnit = new System.Windows.Forms.TextBox();
		this.textBoxVoltageValue = new System.Windows.Forms.TextBox();
		this.textBoxPhaseUnit = new System.Windows.Forms.TextBox();
		this.textBoxPhaseValue = new System.Windows.Forms.TextBox();
		this.textBoxOutputUnit = new System.Windows.Forms.TextBox();
		this.textBoxOutputValue = new System.Windows.Forms.TextBox();
		this.label7 = new System.Windows.Forms.Label();
		this.textBoxOutput = new System.Windows.Forms.TextBox();
		this.textBoxBoreUnit = new System.Windows.Forms.TextBox();
		this.textBoxBoreValue = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.textBoxBore = new System.Windows.Forms.TextBox();
		this.textBoxCodeValue = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
		this.textBoxCode = new System.Windows.Forms.TextBox();
		this.textBoxTokki = new System.Windows.Forms.TextBox();
		this.textBoxModelValue = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.textBoxModel = new System.Windows.Forms.TextBox();
		this.pnlClientTopDataInput = new System.Windows.Forms.Panel();
		this.textBoxID = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.btnReadData = new System.Windows.Forms.Button();
		this.textBoxKeyCodeValue = new System.Windows.Forms.TextBox();
		this.lblKeyCode = new System.Windows.Forms.Label();
		this.btnAdd = new System.Windows.Forms.Button();
		this.btnImport = new System.Windows.Forms.Button();
		this.btnDel = new System.Windows.Forms.Button();
		this.btnModify = new System.Windows.Forms.Button();
		this.btnExport = new System.Windows.Forms.Button();
		this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
		this.btnBrowse = new System.Windows.Forms.Button();
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		this.pnlClientFrontSideCartonSize.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.gbDataSide.SuspendLayout();
		this.gbDataFront.SuspendLayout();
		this.pnlClientTopDataInput.SuspendLayout();
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.pnlClientFrontSideCartonSize);
		base.pnlClientData.Controls.Add(this.pnlClientTopDataInput);
		base.pnlClientData.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.pnlClientData.Size = new System.Drawing.Size(1012, 556);
		base.pnlBottomCtrl.Controls.Add(this.btnBrowse);
		base.pnlBottomCtrl.Controls.Add(this.btnExport);
		base.pnlBottomCtrl.Controls.Add(this.btnModify);
		base.pnlBottomCtrl.Controls.Add(this.btnDel);
		base.pnlBottomCtrl.Controls.Add(this.btnImport);
		base.pnlBottomCtrl.Controls.Add(this.btnAdd);
		base.pnlBottomCtrl.Location = new System.Drawing.Point(0, 556);
		base.pnlBottomCtrl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.pnlBottomCtrl.Size = new System.Drawing.Size(1012, 142);
		this.pnlClientFrontSideCartonSize.BackColor = System.Drawing.Color.Transparent;
		this.pnlClientFrontSideCartonSize.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientFrontSideCartonSize.Controls.Add(this.groupBox1);
		this.pnlClientFrontSideCartonSize.Controls.Add(this.gbDataSide);
		this.pnlClientFrontSideCartonSize.Controls.Add(this.gbDataFront);
		this.pnlClientFrontSideCartonSize.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientFrontSideCartonSize.Location = new System.Drawing.Point(0, 92);
		this.pnlClientFrontSideCartonSize.Name = "pnlClientFrontSideCartonSize";
		this.pnlClientFrontSideCartonSize.Size = new System.Drawing.Size(1008, 460);
		this.pnlClientFrontSideCartonSize.TabIndex = 45;
		this.pnlClientFrontSideCartonSize.Paint += new System.Windows.Forms.PaintEventHandler(pnlClientFront_Paint);
		this.groupBox1.Controls.Add(this.textBoxInsidePrintWordEnd);
		this.groupBox1.Controls.Add(this.textBoxInsidePrintWordBegin);
		this.groupBox1.Controls.Add(this.textBoxOutSidePrintWordEnd);
		this.groupBox1.Controls.Add(this.textBoxOutSidePrintWordBegin);
		this.groupBox1.Controls.Add(this.textBoxPrintWordHeight);
		this.groupBox1.Controls.Add(this.textBoxCaseNo);
		this.groupBox1.Controls.Add(this.label10);
		this.groupBox1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.groupBox1.Location = new System.Drawing.Point(13, 391);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(958, 62);
		this.groupBox1.TabIndex = 106;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "紙箱規格";
		this.groupBox1.Paint += new System.Windows.Forms.PaintEventHandler(gbDataFront_Paint);
		this.textBoxInsidePrintWordEnd.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxInsidePrintWordEnd.Location = new System.Drawing.Point(875, 26);
		this.textBoxInsidePrintWordEnd.Name = "textBoxInsidePrintWordEnd";
		this.textBoxInsidePrintWordEnd.ReadOnly = true;
		this.textBoxInsidePrintWordEnd.Size = new System.Drawing.Size(59, 23);
		this.textBoxInsidePrintWordEnd.TabIndex = 108;
		this.textBoxInsidePrintWordBegin.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxInsidePrintWordBegin.Location = new System.Drawing.Point(765, 26);
		this.textBoxInsidePrintWordBegin.Name = "textBoxInsidePrintWordBegin";
		this.textBoxInsidePrintWordBegin.ReadOnly = true;
		this.textBoxInsidePrintWordBegin.Size = new System.Drawing.Size(59, 23);
		this.textBoxInsidePrintWordBegin.TabIndex = 107;
		this.textBoxOutSidePrintWordEnd.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxOutSidePrintWordEnd.Location = new System.Drawing.Point(655, 26);
		this.textBoxOutSidePrintWordEnd.Name = "textBoxOutSidePrintWordEnd";
		this.textBoxOutSidePrintWordEnd.ReadOnly = true;
		this.textBoxOutSidePrintWordEnd.Size = new System.Drawing.Size(59, 23);
		this.textBoxOutSidePrintWordEnd.TabIndex = 106;
		this.textBoxOutSidePrintWordBegin.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxOutSidePrintWordBegin.Location = new System.Drawing.Point(545, 26);
		this.textBoxOutSidePrintWordBegin.Name = "textBoxOutSidePrintWordBegin";
		this.textBoxOutSidePrintWordBegin.ReadOnly = true;
		this.textBoxOutSidePrintWordBegin.Size = new System.Drawing.Size(59, 23);
		this.textBoxOutSidePrintWordBegin.TabIndex = 105;
		this.textBoxPrintWordHeight.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxPrintWordHeight.Location = new System.Drawing.Point(435, 26);
		this.textBoxPrintWordHeight.Name = "textBoxPrintWordHeight";
		this.textBoxPrintWordHeight.ReadOnly = true;
		this.textBoxPrintWordHeight.Size = new System.Drawing.Size(59, 23);
		this.textBoxPrintWordHeight.TabIndex = 104;
		this.textBoxCaseNo.BackColor = System.Drawing.Color.White;
		this.textBoxCaseNo.Location = new System.Drawing.Point(91, 26);
		this.textBoxCaseNo.Name = "textBoxCaseNo";
		this.textBoxCaseNo.Size = new System.Drawing.Size(319, 23);
		this.textBoxCaseNo.TabIndex = 64;
		this.label10.AutoSize = true;
		this.label10.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label10.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label10.Location = new System.Drawing.Point(5, 29);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(75, 14);
		this.label10.TabIndex = 63;
		this.label10.Text = "紙箱編號:";
		this.gbDataSide.Controls.Add(this.textBoxPlace);
		this.gbDataSide.Controls.Add(this.textBoxModelValueSide);
		this.gbDataSide.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.gbDataSide.Location = new System.Drawing.Point(13, 292);
		this.gbDataSide.Name = "gbDataSide";
		this.gbDataSide.Size = new System.Drawing.Size(958, 74);
		this.gbDataSide.TabIndex = 105;
		this.gbDataSide.TabStop = false;
		this.gbDataSide.Text = "側嘜";
		this.gbDataSide.Paint += new System.Windows.Forms.PaintEventHandler(gbDataFront_Paint);
		this.textBoxPlace.BackColor = System.Drawing.Color.White;
		this.textBoxPlace.Location = new System.Drawing.Point(435, 32);
		this.textBoxPlace.Name = "textBoxPlace";
		this.textBoxPlace.Size = new System.Drawing.Size(499, 23);
		this.textBoxPlace.TabIndex = 60;
		this.textBoxModelValueSide.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxModelValueSide.Location = new System.Drawing.Point(8, 32);
		this.textBoxModelValueSide.Name = "textBoxModelValueSide";
		this.textBoxModelValueSide.ReadOnly = true;
		this.textBoxModelValueSide.Size = new System.Drawing.Size(402, 23);
		this.textBoxModelValueSide.TabIndex = 59;
		this.gbDataFront.Controls.Add(this.lblExportTip);
		this.gbDataFront.Controls.Add(this.textBoxHzUnit);
		this.gbDataFront.Controls.Add(this.textBoxHzValue);
		this.gbDataFront.Controls.Add(this.textBoxVoltageUnit);
		this.gbDataFront.Controls.Add(this.textBoxVoltageValue);
		this.gbDataFront.Controls.Add(this.textBoxPhaseUnit);
		this.gbDataFront.Controls.Add(this.textBoxPhaseValue);
		this.gbDataFront.Controls.Add(this.textBoxOutputUnit);
		this.gbDataFront.Controls.Add(this.textBoxOutputValue);
		this.gbDataFront.Controls.Add(this.label7);
		this.gbDataFront.Controls.Add(this.textBoxOutput);
		this.gbDataFront.Controls.Add(this.textBoxBoreUnit);
		this.gbDataFront.Controls.Add(this.textBoxBoreValue);
		this.gbDataFront.Controls.Add(this.label5);
		this.gbDataFront.Controls.Add(this.textBoxBore);
		this.gbDataFront.Controls.Add(this.textBoxCodeValue);
		this.gbDataFront.Controls.Add(this.label8);
		this.gbDataFront.Controls.Add(this.textBoxCode);
		this.gbDataFront.Controls.Add(this.textBoxTokki);
		this.gbDataFront.Controls.Add(this.textBoxModelValue);
		this.gbDataFront.Controls.Add(this.label1);
		this.gbDataFront.Controls.Add(this.textBoxModel);
		this.gbDataFront.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.gbDataFront.Location = new System.Drawing.Point(13, 33);
		this.gbDataFront.Name = "gbDataFront";
		this.gbDataFront.Size = new System.Drawing.Size(961, 226);
		this.gbDataFront.TabIndex = 104;
		this.gbDataFront.TabStop = false;
		this.gbDataFront.Text = "正嘜";
		this.gbDataFront.Paint += new System.Windows.Forms.PaintEventHandler(gbDataFront_Paint);
		this.lblExportTip.AutoSize = true;
		this.lblExportTip.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblExportTip.Location = new System.Drawing.Point(306, 183);
		this.lblExportTip.Name = "lblExportTip";
		this.lblExportTip.Size = new System.Drawing.Size(241, 29);
		this.lblExportTip.TabIndex = 101;
		this.lblExportTip.Text = "正在導出數據...";
		this.lblExportTip.Visible = false;
		this.textBoxHzUnit.BackColor = System.Drawing.Color.White;
		this.textBoxHzUnit.Location = new System.Drawing.Point(717, 107);
		this.textBoxHzUnit.Name = "textBoxHzUnit";
		this.textBoxHzUnit.Size = new System.Drawing.Size(217, 23);
		this.textBoxHzUnit.TabIndex = 100;
		this.textBoxHzValue.BackColor = System.Drawing.Color.White;
		this.textBoxHzValue.Location = new System.Drawing.Point(435, 107);
		this.textBoxHzValue.Name = "textBoxHzValue";
		this.textBoxHzValue.Size = new System.Drawing.Size(260, 23);
		this.textBoxHzValue.TabIndex = 99;
		this.textBoxHzValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxVoltageUnit.BackColor = System.Drawing.Color.White;
		this.textBoxVoltageUnit.Location = new System.Drawing.Point(351, 107);
		this.textBoxVoltageUnit.Name = "textBoxVoltageUnit";
		this.textBoxVoltageUnit.Size = new System.Drawing.Size(59, 23);
		this.textBoxVoltageUnit.TabIndex = 98;
		this.textBoxVoltageValue.BackColor = System.Drawing.Color.White;
		this.textBoxVoltageValue.Location = new System.Drawing.Point(216, 107);
		this.textBoxVoltageValue.Name = "textBoxVoltageValue";
		this.textBoxVoltageValue.Size = new System.Drawing.Size(115, 23);
		this.textBoxVoltageValue.TabIndex = 97;
		this.textBoxVoltageValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxPhaseUnit.BackColor = System.Drawing.Color.White;
		this.textBoxPhaseUnit.Location = new System.Drawing.Point(83, 107);
		this.textBoxPhaseUnit.Name = "textBoxPhaseUnit";
		this.textBoxPhaseUnit.Size = new System.Drawing.Size(103, 23);
		this.textBoxPhaseUnit.TabIndex = 96;
		this.textBoxPhaseValue.BackColor = System.Drawing.Color.White;
		this.textBoxPhaseValue.Location = new System.Drawing.Point(8, 107);
		this.textBoxPhaseValue.Name = "textBoxPhaseValue";
		this.textBoxPhaseValue.Size = new System.Drawing.Size(53, 23);
		this.textBoxPhaseValue.TabIndex = 95;
		this.textBoxPhaseValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxOutputUnit.BackColor = System.Drawing.Color.White;
		this.textBoxOutputUnit.Location = new System.Drawing.Point(717, 69);
		this.textBoxOutputUnit.Name = "textBoxOutputUnit";
		this.textBoxOutputUnit.Size = new System.Drawing.Size(217, 23);
		this.textBoxOutputUnit.TabIndex = 94;
		this.textBoxOutputValue.BackColor = System.Drawing.Color.White;
		this.textBoxOutputValue.Location = new System.Drawing.Point(527, 69);
		this.textBoxOutputValue.Name = "textBoxOutputValue";
		this.textBoxOutputValue.Size = new System.Drawing.Size(168, 23);
		this.textBoxOutputValue.TabIndex = 93;
		this.textBoxOutputValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.label7.AutoSize = true;
		this.label7.BackColor = System.Drawing.Color.Transparent;
		this.label7.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label7.Location = new System.Drawing.Point(511, 72);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(15, 14);
		this.label7.TabIndex = 92;
		this.label7.Text = ":";
		this.textBoxOutput.BackColor = System.Drawing.Color.White;
		this.textBoxOutput.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxOutput.Location = new System.Drawing.Point(435, 68);
		this.textBoxOutput.Name = "textBoxOutput";
		this.textBoxOutput.Size = new System.Drawing.Size(59, 23);
		this.textBoxOutput.TabIndex = 91;
		this.textBoxBoreUnit.BackColor = System.Drawing.Color.White;
		this.textBoxBoreUnit.Location = new System.Drawing.Point(216, 69);
		this.textBoxBoreUnit.Name = "textBoxBoreUnit";
		this.textBoxBoreUnit.Size = new System.Drawing.Size(194, 23);
		this.textBoxBoreUnit.TabIndex = 90;
		this.textBoxBoreValue.BackColor = System.Drawing.Color.White;
		this.textBoxBoreValue.Location = new System.Drawing.Point(83, 69);
		this.textBoxBoreValue.Name = "textBoxBoreValue";
		this.textBoxBoreValue.Size = new System.Drawing.Size(103, 23);
		this.textBoxBoreValue.TabIndex = 89;
		this.textBoxBoreValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.label5.AutoSize = true;
		this.label5.BackColor = System.Drawing.Color.Transparent;
		this.label5.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label5.Location = new System.Drawing.Point(62, 72);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(15, 14);
		this.label5.TabIndex = 88;
		this.label5.Text = ":";
		this.textBoxBore.BackColor = System.Drawing.Color.White;
		this.textBoxBore.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxBore.Location = new System.Drawing.Point(8, 68);
		this.textBoxBore.Name = "textBoxBore";
		this.textBoxBore.Size = new System.Drawing.Size(53, 23);
		this.textBoxBore.TabIndex = 87;
		this.textBoxCodeValue.BackColor = System.Drawing.Color.White;
		this.textBoxCodeValue.Location = new System.Drawing.Point(83, 183);
		this.textBoxCodeValue.Name = "textBoxCodeValue";
		this.textBoxCodeValue.Size = new System.Drawing.Size(327, 23);
		this.textBoxCodeValue.TabIndex = 84;
		this.label8.AutoSize = true;
		this.label8.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label8.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label8.Location = new System.Drawing.Point(62, 186);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(15, 14);
		this.label8.TabIndex = 83;
		this.label8.Text = ":";
		this.textBoxCode.BackColor = System.Drawing.Color.White;
		this.textBoxCode.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxCode.Location = new System.Drawing.Point(8, 182);
		this.textBoxCode.Name = "textBoxCode";
		this.textBoxCode.Size = new System.Drawing.Size(53, 23);
		this.textBoxCode.TabIndex = 82;
		this.textBoxTokki.BackColor = System.Drawing.Color.White;
		this.textBoxTokki.Location = new System.Drawing.Point(435, 146);
		this.textBoxTokki.Name = "textBoxTokki";
		this.textBoxTokki.Size = new System.Drawing.Size(499, 23);
		this.textBoxTokki.TabIndex = 81;
		this.textBoxModelValue.BackColor = System.Drawing.Color.White;
		this.textBoxModelValue.Location = new System.Drawing.Point(83, 31);
		this.textBoxModelValue.Name = "textBoxModelValue";
		this.textBoxModelValue.Size = new System.Drawing.Size(851, 23);
		this.textBoxModelValue.TabIndex = 79;
		this.textBoxModelValue.TextChanged += new System.EventHandler(textBoxModelValue_TextChanged);
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label1.Location = new System.Drawing.Point(62, 34);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(15, 14);
		this.label1.TabIndex = 78;
		this.label1.Text = ":";
		this.textBoxModel.BackColor = System.Drawing.Color.White;
		this.textBoxModel.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxModel.Location = new System.Drawing.Point(8, 30);
		this.textBoxModel.Name = "textBoxModel";
		this.textBoxModel.Size = new System.Drawing.Size(53, 23);
		this.textBoxModel.TabIndex = 77;
		this.pnlClientTopDataInput.BackColor = System.Drawing.Color.Transparent;
		this.pnlClientTopDataInput.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientTopDataInput.Controls.Add(this.textBoxID);
		this.pnlClientTopDataInput.Controls.Add(this.label2);
		this.pnlClientTopDataInput.Controls.Add(this.btnReadData);
		this.pnlClientTopDataInput.Controls.Add(this.textBoxKeyCodeValue);
		this.pnlClientTopDataInput.Controls.Add(this.lblKeyCode);
		this.pnlClientTopDataInput.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlClientTopDataInput.Location = new System.Drawing.Point(0, 0);
		this.pnlClientTopDataInput.Name = "pnlClientTopDataInput";
		this.pnlClientTopDataInput.Size = new System.Drawing.Size(1008, 92);
		this.pnlClientTopDataInput.TabIndex = 44;
		this.textBoxID.BackColor = System.Drawing.Color.White;
		this.textBoxID.Location = new System.Drawing.Point(399, 35);
		this.textBoxID.Name = "textBoxID";
		this.textBoxID.Size = new System.Drawing.Size(197, 21);
		this.textBoxID.TabIndex = 14;
		this.textBoxID.Text = "ID";
		this.textBoxID.Visible = false;
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.Transparent;
		this.label2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label2.Location = new System.Drawing.Point(367, 38);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(31, 14);
		this.label2.TabIndex = 13;
		this.label2.Text = "ID:";
		this.label2.Visible = false;
		this.btnReadData.Location = new System.Drawing.Point(805, 28);
		this.btnReadData.Name = "btnReadData";
		this.btnReadData.Size = new System.Drawing.Size(166, 34);
		this.btnReadData.TabIndex = 12;
		this.btnReadData.Text = "讀取資料";
		this.btnReadData.UseVisualStyleBackColor = true;
		this.btnReadData.Click += new System.EventHandler(btnReadData_Click);
		this.textBoxKeyCodeValue.BackColor = System.Drawing.Color.White;
		this.textBoxKeyCodeValue.Location = new System.Drawing.Point(93, 35);
		this.textBoxKeyCodeValue.MaxLength = 500;
		this.textBoxKeyCodeValue.Name = "textBoxKeyCodeValue";
		this.textBoxKeyCodeValue.Size = new System.Drawing.Size(197, 21);
		this.textBoxKeyCodeValue.TabIndex = 1;
		this.textBoxKeyCodeValue.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxKeyCodeValue_KeyUp);
		this.lblKeyCode.AutoSize = true;
		this.lblKeyCode.BackColor = System.Drawing.Color.Transparent;
		this.lblKeyCode.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblKeyCode.Location = new System.Drawing.Point(8, 38);
		this.lblKeyCode.Name = "lblKeyCode";
		this.lblKeyCode.Size = new System.Drawing.Size(79, 14);
		this.lblKeyCode.TabIndex = 0;
		this.lblKeyCode.Text = "Key code:";
		this.btnAdd.Location = new System.Drawing.Point(13, 37);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(99, 36);
		this.btnAdd.TabIndex = 1;
		this.btnAdd.Text = "新增";
		this.btnAdd.UseVisualStyleBackColor = true;
		this.btnAdd.Click += new System.EventHandler(btnAdd_Click);
		this.btnImport.Location = new System.Drawing.Point(681, 37);
		this.btnImport.Name = "btnImport";
		this.btnImport.Size = new System.Drawing.Size(99, 36);
		this.btnImport.TabIndex = 2;
		this.btnImport.Text = "導入";
		this.btnImport.UseVisualStyleBackColor = true;
		this.btnImport.Click += new System.EventHandler(btnImport_Click);
		this.btnDel.Location = new System.Drawing.Point(355, 37);
		this.btnDel.Name = "btnDel";
		this.btnDel.Size = new System.Drawing.Size(99, 36);
		this.btnDel.TabIndex = 3;
		this.btnDel.Text = "刪除";
		this.btnDel.UseVisualStyleBackColor = true;
		this.btnDel.Click += new System.EventHandler(btnDel_Click);
		this.btnModify.Location = new System.Drawing.Point(184, 37);
		this.btnModify.Name = "btnModify";
		this.btnModify.Size = new System.Drawing.Size(99, 36);
		this.btnModify.TabIndex = 4;
		this.btnModify.Text = "修改";
		this.btnModify.UseVisualStyleBackColor = true;
		this.btnModify.Click += new System.EventHandler(btnModify_Click);
		this.btnExport.Location = new System.Drawing.Point(834, 37);
		this.btnExport.Name = "btnExport";
		this.btnExport.Size = new System.Drawing.Size(99, 36);
		this.btnExport.TabIndex = 5;
		this.btnExport.Text = "導出";
		this.btnExport.UseVisualStyleBackColor = true;
		this.btnExport.Click += new System.EventHandler(btnExport_Click);
		this.btnBrowse.Location = new System.Drawing.Point(667, 93);
		this.btnBrowse.Name = "btnBrowse";
		this.btnBrowse.Size = new System.Drawing.Size(99, 36);
		this.btnBrowse.TabIndex = 6;
		this.btnBrowse.Text = "瀏覽";
		this.btnBrowse.UseVisualStyleBackColor = true;
		this.btnBrowse.Visible = false;
		this.btnBrowse.Click += new System.EventHandler(btnBrowse_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(1016, 702);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.Name = "frmPrintDataManage";
		this.Text = "噴印資料管理";
		base.Load += new System.EventHandler(frmPrintDataManage_Load);
		base.Resize += new System.EventHandler(frmPrintDataManage_Resize);
		base.pnlClientData.ResumeLayout(false);
		base.pnlBottomCtrl.ResumeLayout(false);
		this.pnlClientFrontSideCartonSize.ResumeLayout(false);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.gbDataSide.ResumeLayout(false);
		this.gbDataSide.PerformLayout();
		this.gbDataFront.ResumeLayout(false);
		this.gbDataFront.PerformLayout();
		this.pnlClientTopDataInput.ResumeLayout(false);
		this.pnlClientTopDataInput.PerformLayout();
		base.ResumeLayout(false);
	}
}
