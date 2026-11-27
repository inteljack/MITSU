using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmCartonSizeManage : frmBase
{
	private IContainer components;

	public ImageList imgListMsgLargeIcon;

	public ImageList imageListMsgSmallIcon;

	private Button btnImport;

	private Button btnDelaySet;

	private Panel pnlClientDataRight;

	private Panel pnlCientDataClient;

	private Button btnExport;

	private Button btnModify;

	private Panel pnlClientDataClientBottom;

	private DataGridView dataGridViewCartonSize;

	private Panel pnlCientDataClientClient;

	private GroupBox gbDataFront;

	private TextBox textBoxCaseNo;

	private Label label10;

	private TextBox textBoxCartonSizeHeight;

	private Label label3;

	private TextBox textBoxCartonSizeWidth;

	private Label label2;

	private TextBox textBoxCartonSizeLength;

	private Label label1;

	private TextBox textBoxInsidePrintWordEnd;

	private TextBox textBoxInsidePrintWordBegin;

	private TextBox textBoxOutSidePrintWordEnd;

	private TextBox textBoxOutSidePrintWordBegin;

	private TextBox textBoxPrintWordHeight;

	private Label label4;

	private Label label8;

	private Label label7;

	private Label label6;

	private Label label5;

	private Button btnDel;

	private Button btnAdd;

	private PictureBox pictureBoxCartonSize;

	private DataGridView dataGridViewPrintHeadStatus;

	private DataGridViewTextBoxColumn ColumnPrintHeadN0;

	private DataGridViewTextBoxColumn PrintHeadDelay;

	private Label label17;

	private Label label16;

	private Label label15;

	private Label label14;

	private Label label13;

	private Label label12;

	private Label label11;

	private Label label9;

	public frmCartonSizeManage()
	{
		InitializeComponent();
	}

	private void frmBoxStdManage_Load(object sender, EventArgs e)
	{
		LoadCartonSizeList(bShowFirstRow: true);
		dataGridViewCartonSize_CellClick(this, null);
	}

	private void ShowPrintHeadDelayList(DataTable dt)
	{
		for (int iRowIndex = 0; iRowIndex < dt.Rows.Count; iRowIndex++)
		{
			dataGridViewPrintHeadStatus.Rows.Add();
			for (int iColumnIndex = 0; iColumnIndex < dt.Columns.Count; iColumnIndex++)
			{
				dataGridViewPrintHeadStatus.Rows[iRowIndex].Cells[iColumnIndex].Value = dt.Rows[iRowIndex][iColumnIndex].ToString();
			}
		}
	}

	private void LoadCartonSizeList(bool bShowFirstRow, string strShowCaseNo = "")
	{
		try
		{
			GetCartonSizeList();
			int iSelectRowIndex = 0;
			if (!bShowFirstRow)
			{
				iSelectRowIndex = GetDataGridViewRowIndex(dataGridViewCartonSize, 0, strShowCaseNo);
			}
			dataGridViewCartonSize.Rows[iSelectRowIndex].Selected = true;
			ShowCartonsizeOnUI(dataGridViewCartonSize.Rows[iSelectRowIndex].Cells[0]);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("LoadCartonSizeList exception:" + ex.Message);
		}
	}

	private void GetCartonSizeList()
	{
		try
		{
			string strTabName = ClsXml.GetCartonSizeTableName();
			DataTable dtTmp = null;
			if (ClsDataBase.CheckExistsTable(strTabName))
			{
				string strSql = "SELECT * FROM " + strTabName + "  ORDER BY [" + strTabName.Substring(0, 2).Trim().ToUpper() + ClsCartonSizeFieldKey.strCaseNo + "]";
				dtTmp = ClsDataBase.SqlReadData(strSql);
			}
			ResetCartonsizeTabHead(ref dtTmp);
			dataGridViewCartonSize.DataSource = dtTmp;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("IniCartonSize exception:" + ex.Message);
		}
	}

	private void ResetCartonsizeTabHead(ref DataTable dtSrc)
	{
		if (dtSrc == null)
		{
			dtSrc = new DataTable();
			string prefix = "TC";
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strID);
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strCaseNo);
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strCartonSizeL);
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strCartonSizeW);
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strCartonSizeH);
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strPrintWordHeight);
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strOutsidePrintWordBegin);
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strOutsidePrintWordEnd);
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strInsidePrintWordBegin);
			dtSrc.Columns.Add(prefix + ClsCartonSizeFieldKey.strInsidePrintWordEnd);
		}
		dtSrc.Columns.RemoveAt(0);
		for (int iColumnIndex = 0; iColumnIndex < dtSrc.Columns.Count; iColumnIndex++)
		{
			string strCaption = dtSrc.Columns[iColumnIndex].Caption.Substring(2);
			if (strCaption.Trim().ToUpper() == ClsCartonSizeFieldKey.strCaseNo)
			{
				strCaption = "①" + strCaption;
			}
			else if (strCaption.Trim().ToUpper() == ClsCartonSizeFieldKey.strPrintWordHeight)
			{
				strCaption = "②" + strCaption;
			}
			else if (strCaption.Trim().ToUpper() == ClsCartonSizeFieldKey.strOutsidePrintWordBegin)
			{
				strCaption = "③" + strCaption;
			}
			else if (strCaption.Trim().ToUpper() == ClsCartonSizeFieldKey.strOutsidePrintWordEnd)
			{
				strCaption = "④" + strCaption;
			}
			else if (strCaption.Trim().ToUpper() == ClsCartonSizeFieldKey.strInsidePrintWordBegin)
			{
				strCaption = "⑤" + strCaption;
			}
			else if (strCaption.Trim().ToUpper() == ClsCartonSizeFieldKey.strInsidePrintWordEnd)
			{
				strCaption = "⑥" + strCaption;
			}
			dtSrc.Columns[iColumnIndex].Caption = strCaption;
			dtSrc.Columns[iColumnIndex].ColumnName = strCaption;
		}
	}

	private void frmBoxStdManage_FormClosing(object sender, FormClosingEventArgs e)
	{
	}

	private void btnImport_Click(object sender, EventArgs e)
	{
		try
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "導入紙箱規格";
			ofd.Filter = "All Files(*.*)|*.*|Excel(*.xls)|*.xls";
			ofd.FilterIndex = 2;
			ofd.InitialDirectory = ClsXml.GetImportFloder();
			if (DialogResult.OK != ofd.ShowDialog())
			{
				return;
			}
			DataTable dt = ClsExcel.GetDataTableFromExcel(ofd.FileName);
			ClsExcel.DelSpaceColumn(ref dt);
			ClsExcel.DelSpaceRow(0, ref dt);
			if (!CheckCartonSizeFormat(dt))
			{
				MessageBox.Show("導入格式錯誤");
				return;
			}
			string strTabName = ClsXml.GetCartonSizeTableName();
			if (ClsDataBase.CheckExistsTable(strTabName))
			{
				MessageBox.Show("成功導入 " + ClsDataBase.SqlImportData(dt, strTabName, strTabName.Substring(0, 2).Trim().ToUpper() + ClsCartonSizeFieldKey.strCaseNo, bIsCover: true) + " 個紙箱規格");
				LoadCartonSizeList(bShowFirstRow: true);
			}
			else
			{
				MessageBox.Show("導入失敗，可能原因如下：\n\n1:數據有重复的CASE-No\n\n2:表格被其他程序占用\n\n3:未初始化數據庫\n\n4:數據庫连接不正常");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("btnImport_Click:" + ex.Message);
			MessageBox.Show(ex.Message);
		}
	}

	private bool CheckCartonSizeFormat(DataTable dtSrc)
	{
		try
		{
			bool bRlt = true;
			for (int iColumnIndex = 0; iColumnIndex < dtSrc.Columns.Count; iColumnIndex++)
			{
				string strCaption = dtSrc.Columns[iColumnIndex].Caption.Trim().ToUpper();
				if (strCaption == ClsCartonSizeFieldKey.strCartonSizeH.Trim().ToUpper() || strCaption == ClsCartonSizeFieldKey.strCartonSizeL.Trim().ToUpper() || strCaption == ClsCartonSizeFieldKey.strCartonSizeW.Trim().ToUpper() || strCaption == ClsCartonSizeFieldKey.strCaseNo.Trim().ToUpper() || strCaption == ClsCartonSizeFieldKey.strID.Trim().ToUpper() || strCaption == ClsCartonSizeFieldKey.strInsidePrintWordBegin.Trim().ToUpper() || strCaption == ClsCartonSizeFieldKey.strInsidePrintWordEnd.Trim().ToUpper() || strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordBegin.Trim().ToUpper() || strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordEnd.Trim().ToUpper() || strCaption == ClsCartonSizeFieldKey.strPrintWordHeight.Trim().ToUpper())
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

	private void btnDelaySet_Click(object sender, EventArgs e)
	{
		int[] iArrStatus = new int[1];
		ClsAdt.imGetPrinterStatus(iArrStatus);
		if (iArrStatus[0] == 1)
		{
			MessageBox.Show("噴印中無法設定延遲");
			return;
		}
		DataTable dtPrintHeadData = ClsShare.DataGridViewToDataTable(dataGridViewPrintHeadStatus);
		bool bSetResult = false;
		int iSetPrintHeadNo = 0;
		ClsAdt.SetPrintHeadDelay(dtPrintHeadData, ref bSetResult, ref iSetPrintHeadNo);
		if (bSetResult)
		{
			MessageBox.Show("設定成功");
		}
		else
		{
			MessageBox.Show(iSetPrintHeadNo + "號噴頭設定失敗");
		}
	}

	private void SetPrintHeadDelay()
	{
		_ = (DataTable)dataGridViewPrintHeadStatus.DataSource;
	}

	private void dataGridViewPrintHeadStatus_CellValueChanged(object sender, DataGridViewCellEventArgs e)
	{
		if (dataGridViewPrintHeadStatus.Rows.Count <= 0 || dataGridViewPrintHeadStatus.Rows[e.RowIndex].Cells[ClsAdt.ClsPrintHeadItems.strColumnPhDelayName].Value == null)
		{
			return;
		}
		string strInputVal = dataGridViewPrintHeadStatus.Rows[e.RowIndex].Cells[ClsAdt.ClsPrintHeadItems.strColumnPhDelayName].Value.ToString();
		int iDecimalPointCount = 0;
		string text = strInputVal;
		foreach (char cTmp in text)
		{
			if ((cTmp >= '0' && cTmp <= '9') || cTmp == '\r' || cTmp == '\b' || cTmp == '.')
			{
				if (cTmp == '.')
				{
					iDecimalPointCount++;
					if (iDecimalPointCount > 1)
					{
						dataGridViewPrintHeadStatus.Rows[e.RowIndex].Cells[ClsAdt.ClsPrintHeadItems.strColumnPhDelayName].Value = "0.0";
						MessageBox.Show("數字格式錯誤");
						return;
					}
				}
				continue;
			}
			dataGridViewPrintHeadStatus.Rows[e.RowIndex].Cells[ClsAdt.ClsPrintHeadItems.strColumnPhDelayName].Value = "0.0";
			return;
		}
		if (Convert.ToDouble(strInputVal) > 2500.0)
		{
			dataGridViewPrintHeadStatus.Rows[e.RowIndex].Cells[ClsAdt.ClsPrintHeadItems.strColumnPhDelayName].Value = strInputVal.Substring(0, 3);
		}
	}

	private void btnExport_Click(object sender, EventArgs e)
	{
		try
		{
			string strTabName = ClsXml.GetCartonSizeTableName();
			if (!ClsDataBase.CheckExistsTable(strTabName))
			{
				MessageBox.Show("導出失敗，請檢查數據庫連接是否正常!");
				return;
			}
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "導出纸箱规格";
			sfd.Filter = "All Files(*.*)|*.*|Excel(*.xls)|*.xls";
			sfd.FilterIndex = 2;
			sfd.InitialDirectory = ClsXml.GetExportFloder();
			if (DialogResult.OK == sfd.ShowDialog())
			{
				btnExport.Enabled = false;
				string strExcelFileName = sfd.FileName;
				strTabName = ClsXml.GetCartonSizeTableName();
				string strSql = "SELECT * FROM " + strTabName + "  ORDER BY [" + strTabName.Substring(0, 2).Trim().ToUpper() + ClsCartonSizeFieldKey.strCaseNo + "]";
				DataTable dt = new DataTable();
				dt = ClsDataBase.SqlReadData(strSql);
				dt.Columns.RemoveAt(0);
				for (int iColumnIndex = 0; iColumnIndex < dt.Columns.Count; iColumnIndex++)
				{
					string strTmp = dt.Columns[iColumnIndex].Caption;
					dt.Columns[iColumnIndex].Caption = strTmp.Substring(2);
				}
				ClsExcel.ExportExcel(dt, strExcelFileName, "Carton-Size");
				btnExport.Enabled = true;
				if (File.Exists(strExcelFileName))
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
			btnExport.Enabled = true;
			MessageBox.Show(ex.Message);
		}
	}

	private DataGridView DataGridViewCopy(DataGridView dgvSrc, int iColumnStartIndex = 0, int iHeaderTextStartIndex = 0)
	{
		try
		{
			DataGridView dgvDest = new DataGridView();
			dgvDest.AllowUserToAddRows = false;
			for (int iColumnIndex = iColumnStartIndex; iColumnIndex < dgvSrc.Columns.Count; iColumnIndex++)
			{
				dgvDest.Columns.Add(dgvSrc.Columns[iColumnIndex].Name, dgvSrc.Columns[iColumnIndex].HeaderText.Substring(iHeaderTextStartIndex).Trim().ToUpper());
			}
			for (int iRowIndex = 0; iRowIndex < dgvSrc.Rows.Count; iRowIndex++)
			{
				dgvDest.Rows.Add();
				for (int i = iColumnStartIndex; i < dgvSrc.Columns.Count; i++)
				{
					dgvDest.Rows[iRowIndex].Cells[i - iColumnStartIndex].Value = dgvSrc.Rows[iRowIndex].Cells[i].Value;
				}
			}
			return dgvDest;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("DataGridView exception:" + ex.Message);
			return null;
		}
	}

	private void frmBoxStdManage_Resize(object sender, EventArgs e)
	{
		btnDelaySet.Left = pnlCientDataClient.Width + (pnlClientDataRight.Width - btnDelaySet.Width) / 2;
	}

	private void btnModify_Click(object sender, EventArgs e)
	{
		try
		{
			string strCaseNo = textBoxCaseNo.Text;
			if (strCaseNo.Length <= 0)
			{
				MessageBox.Show("紙箱編號不可為空!");
			}
			else if (dataGridViewCartonSize == null || dataGridViewCartonSize.Rows.Count <= 0)
			{
				MessageBox.Show("紙箱編號不存在！");
			}
			else if (!CheckCaseNoExistOnDGV(dataGridViewCartonSize, strCaseNo, 0))
			{
				MessageBox.Show("紙箱編號不存在！");
			}
			else if (ModifyCartonSize())
			{
				LoadCartonSizeList(bShowFirstRow: false, strCaseNo);
				MessageBox.Show("修改成功");
			}
			else
			{
				ClsLog.LogWrite("紙箱規格修改失敗!");
				MessageBox.Show("修改失敗，請檢查數據庫連接是否正常!");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("ModifyCartonSize exception:" + ex.Message);
			MessageBox.Show(ex.Message);
		}
	}

	private bool ModifyCartonSize()
	{
		try
		{
			bool bRlt = true;
			string strTabName = ClsXml.GetCartonSizeTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strCaseNo = ClsDataBase.ResetSingleQuotationMarkForSql(textBoxCaseNo.Text);
			if (SqlDelData(strPrefix + ClsCartonSizeFieldKey.strCaseNo, strCaseNo, strTabName) > 0)
			{
				if (SqlInsData(strTabName) <= 0)
				{
					bRlt = false;
				}
			}
			else
			{
				bRlt = false;
				ClsLog.LogWrite("ModifyCartonSize delete " + strCaseNo + " fail");
			}
			return bRlt;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("ModifyCartonSize exception:" + ex.Message);
			return false;
		}
	}

	private int SqlDelData(string strKey, string strValue, string strTabName)
	{
		string strSql = "DELETE " + strTabName;
		strSql = strSql + "  WHERE [" + strKey + "]='" + strValue + "'";
		return ClsDataBase.SqlOperateData(strSql);
	}

	private int SqlInsData(string strTabName)
	{
		try
		{
			string strSql = "INSERT " + strTabName + " (";
			string strColumnNamePrefix = strTabName.Substring(0, 2);
			strSql = strSql + "[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strID + "],[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strCaseNo + "],[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strCartonSizeL + "],[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strCartonSizeW + "],[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strCartonSizeH + "],[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strPrintWordHeight + "],[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strOutsidePrintWordBegin + "],[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strOutsidePrintWordEnd + "],[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strInsidePrintWordBegin + "],[" + strColumnNamePrefix + ClsCartonSizeFieldKey.strInsidePrintWordEnd + "]) ";
			strSql = strSql + " VALUES( N'" + Guid.NewGuid().ToString() + "',N'" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxCaseNo.Text) + "',N'" + textBoxCartonSizeLength.Text + "',N'" + textBoxCartonSizeWidth.Text + "',N'" + textBoxCartonSizeHeight.Text + "',N'" + textBoxPrintWordHeight.Text + "',N'" + textBoxOutSidePrintWordBegin.Text + "',N'" + textBoxOutSidePrintWordEnd.Text + "',N'" + textBoxInsidePrintWordBegin.Text + "',N'" + textBoxInsidePrintWordEnd.Text + "')";
			return ClsDataBase.SqlOperateData(strSql);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Insert data when modify carton size.Exception:" + ex.Message);
			return 0;
		}
	}

	private void dataGridViewCartonSize_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		try
		{
			ShowCartonsizeOnUI(dataGridViewCartonSize.CurrentCell);
		}
		catch
		{
		}
	}

	private void ShowCartonsizeOnUI(DataGridViewCell dgvCurCell)
	{
		try
		{
			DataGridViewRow dgvRow = dgvCurCell.OwningRow;
			_ = dgvCurCell.OwningColumn;
			for (int iColumnIndex = 0; iColumnIndex < dataGridViewCartonSize.Columns.Count; iColumnIndex++)
			{
				string strCaption = dataGridViewCartonSize.Columns[iColumnIndex].HeaderText.Trim().ToUpper();
				string strValue = dgvRow.Cells[iColumnIndex].Value.ToString();
				if (strCaption.Contains(ClsCartonSizeFieldKey.strCaseNo))
				{
					textBoxCaseNo.Text = strValue;
				}
				else if (strCaption.Contains(ClsCartonSizeFieldKey.strCartonSizeL))
				{
					textBoxCartonSizeLength.Text = strValue;
				}
				else if (strCaption.Contains(ClsCartonSizeFieldKey.strCartonSizeW))
				{
					textBoxCartonSizeWidth.Text = strValue;
				}
				else if (strCaption.Contains(ClsCartonSizeFieldKey.strCartonSizeH))
				{
					textBoxCartonSizeHeight.Text = strValue;
				}
				else if (strCaption.Contains(ClsCartonSizeFieldKey.strPrintWordHeight))
				{
					textBoxPrintWordHeight.Text = strValue;
				}
				else if (strCaption.Contains(ClsCartonSizeFieldKey.strOutsidePrintWordBegin))
				{
					textBoxOutSidePrintWordBegin.Text = strValue;
				}
				else if (strCaption.Contains(ClsCartonSizeFieldKey.strOutsidePrintWordEnd))
				{
					textBoxOutSidePrintWordEnd.Text = strValue;
				}
				else if (strCaption.Contains(ClsCartonSizeFieldKey.strInsidePrintWordBegin))
				{
					textBoxInsidePrintWordBegin.Text = strValue;
				}
				else if (strCaption.Contains(ClsCartonSizeFieldKey.strInsidePrintWordEnd))
				{
					textBoxInsidePrintWordEnd.Text = strValue;
				}
			}
		}
		catch
		{
		}
	}

	private void pnlCientDataClient_Resize(object sender, EventArgs e)
	{
		pnlClientDataClientBottom.Height = pnlClientDataClientBottom.Parent.Height / 3;
	}

	private void dataGridViewCartonSize_SelectionChanged(object sender, EventArgs e)
	{
		dataGridViewCartonSize_CellClick(sender, null);
	}

	private void btnDel_Click(object sender, EventArgs e)
	{
		try
		{
			string strCaseNo = textBoxCaseNo.Text;
			if (strCaseNo.Length <= 0)
			{
				MessageBox.Show("紙箱編號不可為空!");
				return;
			}
			if (dataGridViewCartonSize == null || dataGridViewCartonSize.Rows.Count <= 0)
			{
				MessageBox.Show("紙箱編號不存在！");
				return;
			}
			if (!CheckCaseNoExistOnDGV(dataGridViewCartonSize, strCaseNo, 0))
			{
				MessageBox.Show("紙箱編號不存在！");
				return;
			}
			string strTabName = ClsXml.GetCartonSizeTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			if (SqlDelData(strPrefix + ClsCartonSizeFieldKey.strCaseNo, ClsDataBase.ResetSingleQuotationMarkForSql(strCaseNo), strTabName) > 0)
			{
				GetCartonSizeList();
				if (dataGridViewCartonSize != null && dataGridViewCartonSize.Rows.Count > 0)
				{
					dataGridViewCartonSize.Rows[0].Selected = false;
				}
				ResetDataOnUI();
				MessageBox.Show("删除成功");
			}
			else
			{
				ClsLog.LogWrite("紙箱規格刪除失敗!");
				MessageBox.Show("删除失敗，請檢查數據庫連接是否正常!");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("DelCartonSize fail" + ex.Message);
			MessageBox.Show(ex.Message);
		}
	}

	private bool CheckCaseNoExistOnDGV(DataGridView dgvSrc, string strCaseNo, int iCaseNoIndex)
	{
		bool bRlt = false;
		for (int iRowIndex = 0; iRowIndex < dgvSrc.Rows.Count; iRowIndex++)
		{
			if (strCaseNo.Trim().ToUpper() == dgvSrc.Rows[iRowIndex].Cells[iCaseNoIndex].Value.ToString().Trim().ToUpper())
			{
				bRlt = true;
				break;
			}
		}
		return bRlt;
	}

	private void ResetDataOnUI()
	{
		for (int iControlIndex = 0; iControlIndex < pnlCientDataClientClient.Controls.Count; iControlIndex++)
		{
			if (!(pnlCientDataClientClient.Controls[iControlIndex] is GroupBox gbTmp))
			{
				continue;
			}
			for (int iCtrlIndexOnGB = 0; iCtrlIndexOnGB < gbTmp.Controls.Count; iCtrlIndexOnGB++)
			{
				if (gbTmp.Controls[iCtrlIndexOnGB] is TextBox tbTmp)
				{
					tbTmp.Text = "";
				}
			}
		}
	}

	private void btnAdd_Click(object sender, EventArgs e)
	{
		try
		{
			string strCaseNo = textBoxCaseNo.Text.Trim();
			if (strCaseNo.Length <= 0)
			{
				MessageBox.Show("紙箱編號不可為空!");
				return;
			}
			string strTabName = ClsXml.GetCartonSizeTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strKeyWord = strPrefix + ClsCartonSizeFieldKey.strCaseNo.Trim().ToUpper();
			string strKeyValue = ClsDataBase.ResetSingleQuotationMarkForSql(textBoxCaseNo.Text);
			if (ClsDataBase.SqlCheckDataExist(strTabName, strKeyWord, strKeyValue))
			{
				MessageBox.Show("數據已經存在，无法新增!");
			}
			else if (SqlInsData(strTabName) > 0)
			{
				GetCartonSizeList();
				int iSelectRowIndex = GetDataGridViewRowIndex(dataGridViewCartonSize, 0, strCaseNo);
				dataGridViewCartonSize.Rows[iSelectRowIndex].Selected = true;
				ShowCartonsizeOnUI(dataGridViewCartonSize.Rows[iSelectRowIndex].Cells[0]);
				MessageBox.Show("新增成功");
			}
			else
			{
				ClsLog.LogWrite("紙箱規格新增時插入失敗導致新增失敗!");
				MessageBox.Show("新增失败，請檢查數據庫連接是否正常!");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Add carton size exception:" + ex.Message);
		}
	}

	private bool CheckCartonSize()
	{
		try
		{
			if (textBoxCartonSizeLength.Text.Trim().Length <= 0)
			{
				MessageBox.Show("紙箱的長度不可為空!");
				return false;
			}
			if (textBoxCartonSizeWidth.Text.Trim().Length <= 0)
			{
				MessageBox.Show("紙箱的寬度不可為空!");
				return false;
			}
			if (textBoxCartonSizeHeight.Text.Trim().Length <= 0)
			{
				MessageBox.Show("紙箱的高度不可為空!");
				return false;
			}
			if (textBoxPrintWordHeight.Text.Trim().Length <= 0)
			{
				MessageBox.Show("印字高度不可為空!");
				return false;
			}
			if (textBoxOutSidePrintWordBegin.Text.Trim().Length <= 0)
			{
				MessageBox.Show("表面印字開始不可為空!");
				return false;
			}
			if (textBoxOutSidePrintWordEnd.Text.Trim().Length <= 0)
			{
				MessageBox.Show("表面印字終了不可為空!");
				return false;
			}
			if (textBoxInsidePrintWordBegin.Text.Trim().Length <= 0)
			{
				MessageBox.Show("裏面印字開始不可為空!");
				return false;
			}
			if (textBoxInsidePrintWordEnd.Text.Trim().Length <= 0)
			{
				MessageBox.Show("裏面印字終了不可為空!");
				return false;
			}
			double dCartonSizeL = Convert.ToDouble(textBoxCartonSizeLength.Text.Trim());
			double dCartonSizeW = Convert.ToDouble(textBoxCartonSizeWidth.Text.Trim());
			double dOutsidePrintWordEndPos = Convert.ToDouble(textBoxOutSidePrintWordEnd.Text.Trim());
			double dInsidePrintWordEndPos = Convert.ToDouble(textBoxInsidePrintWordEnd.Text.Trim());
			if (dCartonSizeL + dCartonSizeW < dOutsidePrintWordEndPos)
			{
				MessageBox.Show("表面印字終了位置超過範圍，請調整紙箱的長度、寬度或者表面印字終了位置!");
				return false;
			}
			if (dCartonSizeL + dCartonSizeW < dInsidePrintWordEndPos)
			{
				MessageBox.Show("裏面印字終了位置超過範圍，請調整紙箱的長度、寬度或者裏面印字終了位置!");
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			return false;
		}
	}

	private int GetDataGridViewRowIndex(DataGridView dgvSrc, int iKeyIndex, string strKey)
	{
		try
		{
			int iRlt = 0;
			for (int iRowIndex = 0; iRowIndex < dgvSrc.Rows.Count; iRowIndex++)
			{
				string strCaption = dgvSrc.Rows[iRowIndex].Cells[iKeyIndex].Value.ToString().Trim().ToUpper();
				if (strCaption == strKey.Trim().ToUpper())
				{
					iRlt = iRowIndex;
					break;
				}
			}
			return iRlt;
		}
		catch
		{
			return 0;
		}
	}

	private void textBoxCartonSizeLength_KeyPress(object sender, KeyPressEventArgs e)
	{
		try
		{
			if (e.KeyChar != '.' && !ClsShare.OnlyInputNum(e.KeyChar))
			{
				e.Handled = true;
			}
		}
		catch
		{
		}
	}

	private void pnlCientDataClientClient_Resize(object sender, EventArgs e)
	{
		pictureBoxCartonSize.Left = gbDataFront.Left;
		pictureBoxCartonSize.Top = gbDataFront.Top + gbDataFront.Height + 15;
		pictureBoxCartonSize.Width = gbDataFront.Width;
		pictureBoxCartonSize.Height = pnlCientDataClientClient.Height - gbDataFront.Height - 40;
	}

	private void textBoxCartonSizeLength_KeyUp(object sender, KeyEventArgs e)
	{
		try
		{
			string strTmp = ((TextBox)sender).Text;
			if (strTmp.Length <= 0)
			{
				return;
			}
			if (strTmp.Substring(0, 1).Contains('.'))
			{
				if (strTmp.Length > 1)
				{
					((TextBox)sender).Text = strTmp.Substring(1, strTmp.Length - 1);
				}
				else
				{
					((TextBox)sender).Text = "";
				}
			}
			else
			{
				if (!strTmp.Contains('.'))
				{
					return;
				}
				string[] strArrTmp = strTmp.Split('.');
				strTmp = "";
				for (int iIndex = 0; iIndex < strArrTmp.Count(); iIndex++)
				{
					strTmp += strArrTmp[iIndex];
					if (iIndex == 0)
					{
						strTmp += ".";
					}
				}
				((TextBox)sender).Text = strTmp;
				e.Handled = true;
			}
		}
		catch
		{
		}
	}

	private void textBoxInsidePrintWordEnd_TextChanged(object sender, EventArgs e)
	{
		if (!ClsShare.CheckStrIsNumAndPoint(((TextBox)sender).Text.Trim()))
		{
			((TextBox)sender).Text = "0";
			MessageBox.Show("輸入必須為數字或者小數點");
		}
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
		this.components = new System.ComponentModel.Container();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeJianPrintingConsole.frmCartonSizeManage));
		this.imgListMsgLargeIcon = new System.Windows.Forms.ImageList(this.components);
		this.imageListMsgSmallIcon = new System.Windows.Forms.ImageList(this.components);
		this.btnImport = new System.Windows.Forms.Button();
		this.btnDelaySet = new System.Windows.Forms.Button();
		this.pnlClientDataRight = new System.Windows.Forms.Panel();
		this.dataGridViewPrintHeadStatus = new System.Windows.Forms.DataGridView();
		this.ColumnPrintHeadN0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.PrintHeadDelay = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.pnlCientDataClient = new System.Windows.Forms.Panel();
		this.pnlCientDataClientClient = new System.Windows.Forms.Panel();
		this.pictureBoxCartonSize = new System.Windows.Forms.PictureBox();
		this.gbDataFront = new System.Windows.Forms.GroupBox();
		this.label17 = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.label14 = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.textBoxCartonSizeHeight = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.textBoxCartonSizeWidth = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.textBoxCartonSizeLength = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.textBoxInsidePrintWordEnd = new System.Windows.Forms.TextBox();
		this.textBoxInsidePrintWordBegin = new System.Windows.Forms.TextBox();
		this.textBoxOutSidePrintWordEnd = new System.Windows.Forms.TextBox();
		this.textBoxOutSidePrintWordBegin = new System.Windows.Forms.TextBox();
		this.textBoxPrintWordHeight = new System.Windows.Forms.TextBox();
		this.textBoxCaseNo = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.pnlClientDataClientBottom = new System.Windows.Forms.Panel();
		this.dataGridViewCartonSize = new System.Windows.Forms.DataGridView();
		this.btnExport = new System.Windows.Forms.Button();
		this.btnModify = new System.Windows.Forms.Button();
		this.btnDel = new System.Windows.Forms.Button();
		this.btnAdd = new System.Windows.Forms.Button();
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		this.pnlClientDataRight.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewPrintHeadStatus).BeginInit();
		this.pnlCientDataClient.SuspendLayout();
		this.pnlCientDataClientClient.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBoxCartonSize).BeginInit();
		this.gbDataFront.SuspendLayout();
		this.pnlClientDataClientBottom.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewCartonSize).BeginInit();
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.pnlCientDataClient);
		base.pnlClientData.Controls.Add(this.pnlClientDataRight);
		base.pnlClientData.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.pnlClientData.Size = new System.Drawing.Size(1024, 513);
		base.pnlBottomCtrl.Controls.Add(this.btnDel);
		base.pnlBottomCtrl.Controls.Add(this.btnAdd);
		base.pnlBottomCtrl.Controls.Add(this.btnModify);
		base.pnlBottomCtrl.Controls.Add(this.btnExport);
		base.pnlBottomCtrl.Controls.Add(this.btnDelaySet);
		base.pnlBottomCtrl.Controls.Add(this.btnImport);
		base.pnlBottomCtrl.Location = new System.Drawing.Point(0, 513);
		base.pnlBottomCtrl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.pnlBottomCtrl.Size = new System.Drawing.Size(1024, 103);
		this.imgListMsgLargeIcon.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imgListMsgLargeIcon.ImageStream");
		this.imgListMsgLargeIcon.TransparentColor = System.Drawing.Color.Transparent;
		this.imgListMsgLargeIcon.Images.SetKeyName(0, "MsgIconLarge.png");
		this.imageListMsgSmallIcon.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListMsgSmallIcon.ImageStream");
		this.imageListMsgSmallIcon.TransparentColor = System.Drawing.Color.Transparent;
		this.imageListMsgSmallIcon.Images.SetKeyName(0, "MsgIconSmall.png");
		this.btnImport.Location = new System.Drawing.Point(566, 24);
		this.btnImport.Name = "btnImport";
		this.btnImport.Size = new System.Drawing.Size(99, 36);
		this.btnImport.TabIndex = 6;
		this.btnImport.Text = "導入";
		this.btnImport.UseVisualStyleBackColor = true;
		this.btnImport.Click += new System.EventHandler(btnImport_Click);
		this.btnDelaySet.Location = new System.Drawing.Point(1033, 37);
		this.btnDelaySet.Name = "btnDelaySet";
		this.btnDelaySet.Size = new System.Drawing.Size(99, 36);
		this.btnDelaySet.TabIndex = 8;
		this.btnDelaySet.Text = "應用延遲設定";
		this.btnDelaySet.UseVisualStyleBackColor = true;
		this.btnDelaySet.Visible = false;
		this.btnDelaySet.Click += new System.EventHandler(btnDelaySet_Click);
		this.pnlClientDataRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientDataRight.Controls.Add(this.dataGridViewPrintHeadStatus);
		this.pnlClientDataRight.Dock = System.Windows.Forms.DockStyle.Right;
		this.pnlClientDataRight.Location = new System.Drawing.Point(965, 0);
		this.pnlClientDataRight.Name = "pnlClientDataRight";
		this.pnlClientDataRight.Size = new System.Drawing.Size(55, 509);
		this.pnlClientDataRight.TabIndex = 23;
		this.pnlClientDataRight.Visible = false;
		this.dataGridViewPrintHeadStatus.AllowUserToAddRows = false;
		this.dataGridViewPrintHeadStatus.BackgroundColor = System.Drawing.Color.WhiteSmoke;
		this.dataGridViewPrintHeadStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewPrintHeadStatus.Columns.AddRange(this.ColumnPrintHeadN0, this.PrintHeadDelay);
		this.dataGridViewPrintHeadStatus.Dock = System.Windows.Forms.DockStyle.Right;
		this.dataGridViewPrintHeadStatus.GridColor = System.Drawing.Color.WhiteSmoke;
		this.dataGridViewPrintHeadStatus.Location = new System.Drawing.Point(-293, 0);
		this.dataGridViewPrintHeadStatus.Name = "dataGridViewPrintHeadStatus";
		this.dataGridViewPrintHeadStatus.RowTemplate.Height = 23;
		this.dataGridViewPrintHeadStatus.Size = new System.Drawing.Size(344, 505);
		this.dataGridViewPrintHeadStatus.TabIndex = 26;
		this.dataGridViewPrintHeadStatus.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridViewPrintHeadStatus_CellValueChanged);
		dataGridViewCellStyle1.Format = "N2";
		dataGridViewCellStyle1.NullValue = null;
		this.ColumnPrintHeadN0.DefaultCellStyle = dataGridViewCellStyle1;
		this.ColumnPrintHeadN0.HeaderText = "PrintHead #";
		this.ColumnPrintHeadN0.Name = "ColumnPrintHeadN0";
		this.ColumnPrintHeadN0.ReadOnly = true;
		this.ColumnPrintHeadN0.Width = 150;
		dataGridViewCellStyle2.Format = "N2";
		dataGridViewCellStyle2.NullValue = null;
		this.PrintHeadDelay.DefaultCellStyle = dataGridViewCellStyle2;
		this.PrintHeadDelay.HeaderText = "Delay (Unit: mm)";
		this.PrintHeadDelay.Name = "PrintHeadDelay";
		this.PrintHeadDelay.Width = 150;
		this.pnlCientDataClient.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlCientDataClient.Controls.Add(this.pnlCientDataClientClient);
		this.pnlCientDataClient.Controls.Add(this.pnlClientDataClientBottom);
		this.pnlCientDataClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlCientDataClient.Location = new System.Drawing.Point(0, 0);
		this.pnlCientDataClient.Name = "pnlCientDataClient";
		this.pnlCientDataClient.Size = new System.Drawing.Size(965, 509);
		this.pnlCientDataClient.TabIndex = 24;
		this.pnlCientDataClient.Resize += new System.EventHandler(pnlCientDataClient_Resize);
		this.pnlCientDataClientClient.Controls.Add(this.pictureBoxCartonSize);
		this.pnlCientDataClientClient.Controls.Add(this.gbDataFront);
		this.pnlCientDataClientClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlCientDataClientClient.Location = new System.Drawing.Point(0, 0);
		this.pnlCientDataClientClient.Name = "pnlCientDataClientClient";
		this.pnlCientDataClientClient.Size = new System.Drawing.Size(961, 405);
		this.pnlCientDataClientClient.TabIndex = 1;
		this.pnlCientDataClientClient.Resize += new System.EventHandler(pnlCientDataClientClient_Resize);
		this.pictureBoxCartonSize.Image = (System.Drawing.Image)resources.GetObject("pictureBoxCartonSize.Image");
		this.pictureBoxCartonSize.Location = new System.Drawing.Point(20, 137);
		this.pictureBoxCartonSize.Name = "pictureBoxCartonSize";
		this.pictureBoxCartonSize.Size = new System.Drawing.Size(557, 210);
		this.pictureBoxCartonSize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pictureBoxCartonSize.TabIndex = 103;
		this.pictureBoxCartonSize.TabStop = false;
		this.gbDataFront.Controls.Add(this.label17);
		this.gbDataFront.Controls.Add(this.label16);
		this.gbDataFront.Controls.Add(this.label15);
		this.gbDataFront.Controls.Add(this.label14);
		this.gbDataFront.Controls.Add(this.label13);
		this.gbDataFront.Controls.Add(this.label12);
		this.gbDataFront.Controls.Add(this.label11);
		this.gbDataFront.Controls.Add(this.label9);
		this.gbDataFront.Controls.Add(this.label8);
		this.gbDataFront.Controls.Add(this.label7);
		this.gbDataFront.Controls.Add(this.label6);
		this.gbDataFront.Controls.Add(this.label5);
		this.gbDataFront.Controls.Add(this.label4);
		this.gbDataFront.Controls.Add(this.textBoxCartonSizeHeight);
		this.gbDataFront.Controls.Add(this.label3);
		this.gbDataFront.Controls.Add(this.textBoxCartonSizeWidth);
		this.gbDataFront.Controls.Add(this.label2);
		this.gbDataFront.Controls.Add(this.textBoxCartonSizeLength);
		this.gbDataFront.Controls.Add(this.label1);
		this.gbDataFront.Controls.Add(this.textBoxInsidePrintWordEnd);
		this.gbDataFront.Controls.Add(this.textBoxInsidePrintWordBegin);
		this.gbDataFront.Controls.Add(this.textBoxOutSidePrintWordEnd);
		this.gbDataFront.Controls.Add(this.textBoxOutSidePrintWordBegin);
		this.gbDataFront.Controls.Add(this.textBoxPrintWordHeight);
		this.gbDataFront.Controls.Add(this.textBoxCaseNo);
		this.gbDataFront.Controls.Add(this.label10);
		this.gbDataFront.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.gbDataFront.Location = new System.Drawing.Point(22, 5);
		this.gbDataFront.Name = "gbDataFront";
		this.gbDataFront.Size = new System.Drawing.Size(935, 125);
		this.gbDataFront.TabIndex = 102;
		this.gbDataFront.TabStop = false;
		this.gbDataFront.Text = "紙箱規格";
		this.label17.AutoSize = true;
		this.label17.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label17.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label17.Location = new System.Drawing.Point(885, 96);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(31, 19);
		this.label17.TabIndex = 127;
		this.label17.Text = "mm";
		this.label16.AutoSize = true;
		this.label16.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label16.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label16.Location = new System.Drawing.Point(647, 96);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(31, 19);
		this.label16.TabIndex = 126;
		this.label16.Text = "mm";
		this.label15.AutoSize = true;
		this.label15.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label15.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label15.Location = new System.Drawing.Point(416, 95);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(31, 19);
		this.label15.TabIndex = 125;
		this.label15.Text = "mm";
		this.label14.AutoSize = true;
		this.label14.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label14.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label14.Location = new System.Drawing.Point(191, 95);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(31, 19);
		this.label14.TabIndex = 124;
		this.label14.Text = "mm";
		this.label13.AutoSize = true;
		this.label13.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label13.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label13.Location = new System.Drawing.Point(885, 61);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(31, 19);
		this.label13.TabIndex = 123;
		this.label13.Text = "mm";
		this.label12.AutoSize = true;
		this.label12.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label12.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label12.Location = new System.Drawing.Point(648, 61);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(31, 19);
		this.label12.TabIndex = 122;
		this.label12.Text = "mm";
		this.label11.AutoSize = true;
		this.label11.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label11.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label11.Location = new System.Drawing.Point(417, 60);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(31, 19);
		this.label11.TabIndex = 121;
		this.label11.Text = "mm";
		this.label9.AutoSize = true;
		this.label9.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label9.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label9.Location = new System.Drawing.Point(192, 60);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(31, 19);
		this.label9.TabIndex = 120;
		this.label9.Text = "mm";
		this.label8.AutoSize = true;
		this.label8.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label8.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label8.Location = new System.Drawing.Point(696, 98);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(120, 14);
		this.label8.TabIndex = 119;
		this.label8.Text = "⑥裏面印字終了:";
		this.label7.AutoSize = true;
		this.label7.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label7.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label7.Location = new System.Drawing.Point(459, 98);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(120, 14);
		this.label7.TabIndex = 118;
		this.label7.Text = "⑤裏面印字開始:";
		this.label6.AutoSize = true;
		this.label6.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label6.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label6.Location = new System.Drawing.Point(232, 97);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(120, 14);
		this.label6.TabIndex = 117;
		this.label6.Text = "④表面印字終了:";
		this.label5.AutoSize = true;
		this.label5.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label5.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label5.Location = new System.Drawing.Point(9, 97);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(120, 14);
		this.label5.TabIndex = 116;
		this.label5.Text = "③表面印字開始:";
		this.label4.AutoSize = true;
		this.label4.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label4.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label4.Location = new System.Drawing.Point(698, 63);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(90, 14);
		this.label4.TabIndex = 115;
		this.label4.Text = "②印字高度:";
		this.textBoxCartonSizeHeight.BackColor = System.Drawing.Color.White;
		this.textBoxCartonSizeHeight.Location = new System.Drawing.Point(583, 59);
		this.textBoxCartonSizeHeight.Name = "textBoxCartonSizeHeight";
		this.textBoxCartonSizeHeight.Size = new System.Drawing.Size(59, 23);
		this.textBoxCartonSizeHeight.TabIndex = 114;
		this.textBoxCartonSizeHeight.TextChanged += new System.EventHandler(textBoxInsidePrintWordEnd_TextChanged);
		this.textBoxCartonSizeHeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxCartonSizeLength_KeyPress);
		this.textBoxCartonSizeHeight.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxCartonSizeLength_KeyUp);
		this.label3.AutoSize = true;
		this.label3.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label3.Location = new System.Drawing.Point(461, 63);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(99, 14);
		this.label3.TabIndex = 113;
		this.label3.Text = "紙箱高度(H):";
		this.textBoxCartonSizeWidth.BackColor = System.Drawing.Color.White;
		this.textBoxCartonSizeWidth.Location = new System.Drawing.Point(354, 58);
		this.textBoxCartonSizeWidth.Name = "textBoxCartonSizeWidth";
		this.textBoxCartonSizeWidth.Size = new System.Drawing.Size(59, 23);
		this.textBoxCartonSizeWidth.TabIndex = 112;
		this.textBoxCartonSizeWidth.TextChanged += new System.EventHandler(textBoxInsidePrintWordEnd_TextChanged);
		this.textBoxCartonSizeWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxCartonSizeLength_KeyPress);
		this.textBoxCartonSizeWidth.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxCartonSizeLength_KeyUp);
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label2.Location = new System.Drawing.Point(234, 62);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(99, 14);
		this.label2.TabIndex = 111;
		this.label2.Text = "紙箱寬度(W):";
		this.textBoxCartonSizeLength.BackColor = System.Drawing.Color.White;
		this.textBoxCartonSizeLength.Location = new System.Drawing.Point(130, 58);
		this.textBoxCartonSizeLength.Name = "textBoxCartonSizeLength";
		this.textBoxCartonSizeLength.Size = new System.Drawing.Size(59, 23);
		this.textBoxCartonSizeLength.TabIndex = 110;
		this.textBoxCartonSizeLength.TextChanged += new System.EventHandler(textBoxInsidePrintWordEnd_TextChanged);
		this.textBoxCartonSizeLength.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxCartonSizeLength_KeyPress);
		this.textBoxCartonSizeLength.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxCartonSizeLength_KeyUp);
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label1.Location = new System.Drawing.Point(10, 62);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(99, 14);
		this.label1.TabIndex = 109;
		this.label1.Text = "紙箱長度(L):";
		this.textBoxInsidePrintWordEnd.BackColor = System.Drawing.Color.White;
		this.textBoxInsidePrintWordEnd.Location = new System.Drawing.Point(821, 94);
		this.textBoxInsidePrintWordEnd.Name = "textBoxInsidePrintWordEnd";
		this.textBoxInsidePrintWordEnd.Size = new System.Drawing.Size(59, 23);
		this.textBoxInsidePrintWordEnd.TabIndex = 108;
		this.textBoxInsidePrintWordEnd.TextChanged += new System.EventHandler(textBoxInsidePrintWordEnd_TextChanged);
		this.textBoxInsidePrintWordEnd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxCartonSizeLength_KeyPress);
		this.textBoxInsidePrintWordEnd.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxCartonSizeLength_KeyUp);
		this.textBoxInsidePrintWordBegin.BackColor = System.Drawing.Color.White;
		this.textBoxInsidePrintWordBegin.Location = new System.Drawing.Point(582, 94);
		this.textBoxInsidePrintWordBegin.Name = "textBoxInsidePrintWordBegin";
		this.textBoxInsidePrintWordBegin.Size = new System.Drawing.Size(59, 23);
		this.textBoxInsidePrintWordBegin.TabIndex = 107;
		this.textBoxInsidePrintWordBegin.TextChanged += new System.EventHandler(textBoxInsidePrintWordEnd_TextChanged);
		this.textBoxInsidePrintWordBegin.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxCartonSizeLength_KeyPress);
		this.textBoxInsidePrintWordBegin.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxCartonSizeLength_KeyUp);
		this.textBoxOutSidePrintWordEnd.BackColor = System.Drawing.Color.White;
		this.textBoxOutSidePrintWordEnd.Location = new System.Drawing.Point(354, 93);
		this.textBoxOutSidePrintWordEnd.Name = "textBoxOutSidePrintWordEnd";
		this.textBoxOutSidePrintWordEnd.Size = new System.Drawing.Size(59, 23);
		this.textBoxOutSidePrintWordEnd.TabIndex = 106;
		this.textBoxOutSidePrintWordEnd.TextChanged += new System.EventHandler(textBoxInsidePrintWordEnd_TextChanged);
		this.textBoxOutSidePrintWordEnd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxCartonSizeLength_KeyPress);
		this.textBoxOutSidePrintWordEnd.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxCartonSizeLength_KeyUp);
		this.textBoxOutSidePrintWordBegin.BackColor = System.Drawing.Color.White;
		this.textBoxOutSidePrintWordBegin.Location = new System.Drawing.Point(129, 93);
		this.textBoxOutSidePrintWordBegin.Name = "textBoxOutSidePrintWordBegin";
		this.textBoxOutSidePrintWordBegin.Size = new System.Drawing.Size(59, 23);
		this.textBoxOutSidePrintWordBegin.TabIndex = 105;
		this.textBoxOutSidePrintWordBegin.TextChanged += new System.EventHandler(textBoxInsidePrintWordEnd_TextChanged);
		this.textBoxOutSidePrintWordBegin.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxCartonSizeLength_KeyPress);
		this.textBoxOutSidePrintWordBegin.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxCartonSizeLength_KeyUp);
		this.textBoxPrintWordHeight.BackColor = System.Drawing.Color.White;
		this.textBoxPrintWordHeight.Location = new System.Drawing.Point(822, 59);
		this.textBoxPrintWordHeight.Name = "textBoxPrintWordHeight";
		this.textBoxPrintWordHeight.Size = new System.Drawing.Size(59, 23);
		this.textBoxPrintWordHeight.TabIndex = 104;
		this.textBoxPrintWordHeight.TextChanged += new System.EventHandler(textBoxInsidePrintWordEnd_TextChanged);
		this.textBoxPrintWordHeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxCartonSizeLength_KeyPress);
		this.textBoxPrintWordHeight.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxCartonSizeLength_KeyUp);
		this.textBoxCaseNo.BackColor = System.Drawing.Color.White;
		this.textBoxCaseNo.Location = new System.Drawing.Point(130, 27);
		this.textBoxCaseNo.Name = "textBoxCaseNo";
		this.textBoxCaseNo.Size = new System.Drawing.Size(799, 23);
		this.textBoxCaseNo.TabIndex = 66;
		this.label10.AutoSize = true;
		this.label10.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label10.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label10.Location = new System.Drawing.Point(9, 31);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(90, 14);
		this.label10.TabIndex = 65;
		this.label10.Text = "①紙箱編號:";
		this.pnlClientDataClientBottom.Controls.Add(this.dataGridViewCartonSize);
		this.pnlClientDataClientBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.pnlClientDataClientBottom.Location = new System.Drawing.Point(0, 405);
		this.pnlClientDataClientBottom.Name = "pnlClientDataClientBottom";
		this.pnlClientDataClientBottom.Size = new System.Drawing.Size(961, 100);
		this.pnlClientDataClientBottom.TabIndex = 0;
		this.dataGridViewCartonSize.AllowUserToAddRows = false;
		this.dataGridViewCartonSize.AllowUserToDeleteRows = false;
		this.dataGridViewCartonSize.BackgroundColor = System.Drawing.Color.WhiteSmoke;
		this.dataGridViewCartonSize.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewCartonSize.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGridViewCartonSize.Location = new System.Drawing.Point(0, 0);
		this.dataGridViewCartonSize.MultiSelect = false;
		this.dataGridViewCartonSize.Name = "dataGridViewCartonSize";
		this.dataGridViewCartonSize.ReadOnly = true;
		this.dataGridViewCartonSize.RowTemplate.Height = 23;
		this.dataGridViewCartonSize.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGridViewCartonSize.Size = new System.Drawing.Size(961, 100);
		this.dataGridViewCartonSize.TabIndex = 11;
		this.dataGridViewCartonSize.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridViewCartonSize_CellClick);
		this.dataGridViewCartonSize.SelectionChanged += new System.EventHandler(dataGridViewCartonSize_SelectionChanged);
		this.btnExport.Location = new System.Drawing.Point(706, 24);
		this.btnExport.Name = "btnExport";
		this.btnExport.Size = new System.Drawing.Size(99, 36);
		this.btnExport.TabIndex = 14;
		this.btnExport.Text = "導出";
		this.btnExport.UseVisualStyleBackColor = true;
		this.btnExport.Click += new System.EventHandler(btnExport_Click);
		this.btnModify.Location = new System.Drawing.Point(180, 24);
		this.btnModify.Name = "btnModify";
		this.btnModify.Size = new System.Drawing.Size(99, 36);
		this.btnModify.TabIndex = 15;
		this.btnModify.Text = "修改";
		this.btnModify.UseVisualStyleBackColor = true;
		this.btnModify.Click += new System.EventHandler(btnModify_Click);
		this.btnDel.Location = new System.Drawing.Point(338, 24);
		this.btnDel.Name = "btnDel";
		this.btnDel.Size = new System.Drawing.Size(99, 36);
		this.btnDel.TabIndex = 18;
		this.btnDel.Text = "刪除";
		this.btnDel.UseVisualStyleBackColor = true;
		this.btnDel.Click += new System.EventHandler(btnDel_Click);
		this.btnAdd.Location = new System.Drawing.Point(22, 24);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(99, 36);
		this.btnAdd.TabIndex = 17;
		this.btnAdd.Text = "新增";
		this.btnAdd.UseVisualStyleBackColor = true;
		this.btnAdd.Click += new System.EventHandler(btnAdd_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(1028, 620);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.Name = "frmCartonSizeManage";
		this.Text = "紙箱規格管理";
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmBoxStdManage_FormClosing);
		base.Load += new System.EventHandler(frmBoxStdManage_Load);
		base.Resize += new System.EventHandler(frmBoxStdManage_Resize);
		base.pnlClientData.ResumeLayout(false);
		base.pnlBottomCtrl.ResumeLayout(false);
		this.pnlClientDataRight.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dataGridViewPrintHeadStatus).EndInit();
		this.pnlCientDataClient.ResumeLayout(false);
		this.pnlCientDataClientClient.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.pictureBoxCartonSize).EndInit();
		this.gbDataFront.ResumeLayout(false);
		this.gbDataFront.PerformLayout();
		this.pnlClientDataClientBottom.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dataGridViewCartonSize).EndInit();
		base.ResumeLayout(false);
	}
}
