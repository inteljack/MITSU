using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmPrintDataSelect : frmBase
{
	private string m_strSql = "";

	private DataTable m_dtSaveData;

	private int m_iPageCount;

	private int m_iOnePageCount = 10;

	private int m_iCutPage = 1;

	private bool m_bDirectClose = true;

	private int m_iCurPageShowNormalLen = 10;

	private IContainer components;

	private Panel pnlClientClientData;

	private DataGridView dataGridViewPrintData;

	private Panel pnlClientTopFind;

	private TextBox textBoxCondition;

	private Label lblKeyCode;

	private Button btnFind;

	private Button btnNextPage;

	private Button btnPreviewPage;

	private GroupBox gbPage;

	private Label lblPage;

	private ComboBox comboBoxMaxLenCtrl;

	private CheckBox checkBoxExceedMaxLen;

	public frmPrintDataSelect(string strSql, string strFindCondition)
	{
		InitializeComponent();
		m_strSql = strSql;
		textBoxCondition.Text = strFindCondition;
	}

	private void frmPrintDataSelect_Load(object sender, EventArgs e)
	{
		comboBoxMaxLenCtrl.SelectedIndex = 0;
		IniDataEx(m_strSql);
	}

	private void IniDataEx(string strSql)
	{
		string strTabName = ClsXml.GetPrintDataTableName();
		if (ClsDataBase.CheckExistsTable(strTabName))
		{
			IniData(ClsDataBase.SqlReadData(strSql));
		}
		else
		{
			IniData(null);
		}
	}

	private void IniData(DataTable dt)
	{
		try
		{
			dataGridViewPrintData.DataSource = new DataTable();
			if (dt == null)
			{
				string strCurPage = "0";
				strCurPage = strCurPage.PadLeft(m_iCurPageShowNormalLen);
				lblPage.Text = strCurPage + "/0";
			}
			m_dtSaveData = new DataTable();
			m_dtSaveData = dt.Copy();
			m_iPageCount = (dt.Rows.Count + (m_iOnePageCount - 1)) / m_iOnePageCount;
			if (m_iPageCount >= 1)
			{
				btnPreviewPage.Enabled = false;
				if (m_iPageCount > 1)
				{
					btnNextPage.Enabled = true;
				}
				else
				{
					btnNextPage.Enabled = false;
				}
				ShowCurPage(dt, 1);
			}
			else
			{
				btnPreviewPage.Enabled = false;
				btnNextPage.Enabled = false;
				string strCurPage2 = "0";
				strCurPage2 = strCurPage2.PadLeft(m_iCurPageShowNormalLen);
				lblPage.Text = strCurPage2 + "/0";
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("噴印資料選擇窗口中初始化數據異常:" + ex.Message);
		}
	}

	private void ShowCurPage(DataTable dt, int iCurPage)
	{
		DataTable dtTmp = new DataTable();
		int iShowRowCout = ((iCurPage * m_iOnePageCount > dt.Rows.Count) ? (dt.Rows.Count - (iCurPage - 1) * m_iOnePageCount) : m_iOnePageCount);
		string strCurPage = iCurPage.ToString();
		strCurPage = strCurPage.PadLeft(m_iCurPageShowNormalLen);
		lblPage.Text = strCurPage + "/" + m_iPageCount;
		for (int iColumnIndex = 0; iColumnIndex < dt.Columns.Count; iColumnIndex++)
		{
			dtTmp.Columns.Add();
			dtTmp.Columns[iColumnIndex].ColumnName = dt.Columns[iColumnIndex].ColumnName.Substring(2);
		}
		for (int iRowIndex = 0; iRowIndex < iShowRowCout; iRowIndex++)
		{
			dtTmp.Rows.Add();
			for (int i = 0; i < dt.Columns.Count; i++)
			{
				dtTmp.Rows[iRowIndex][i] = dt.Rows[(iCurPage - 1) * m_iOnePageCount + iRowIndex][i];
			}
		}
		dataGridViewPrintData.DataSource = dtTmp;
		ShowPrintDataColumn();
		CheckFieldExceedMaxLen(dataGridViewPrintData);
	}

	private void CheckFieldExceedMaxLen(DataGridView dgvSrc)
	{
		try
		{
			if (dgvSrc == null || dgvSrc.Rows.Count <= 0)
			{
				return;
			}
			ClsGetPrintDataFieldMaxLen ClsGPDFM = new ClsGetPrintDataFieldMaxLen();
			for (int iColumnIndex = 0; iColumnIndex < dgvSrc.Columns.Count; iColumnIndex++)
			{
				string strCaption = dgvSrc.Columns[iColumnIndex].Name.Trim().ToUpper();
				if (strCaption == ClsPrintDataField.strKeyCode)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_Keycode);
				}
				else if (strCaption == ClsPrintDataField.strModel)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_Model);
				}
				else if (strCaption == ClsPrintDataField.strModelValue)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_ModelValue);
				}
				else if (strCaption == ClsPrintDataField.strBore)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_Bore);
				}
				else if (strCaption == ClsPrintDataField.strBoreValue)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_BoreValue);
				}
				else if (strCaption == ClsPrintDataField.strBoreUnit)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_BoreUnit);
				}
				else if (strCaption == ClsPrintDataField.strOutput)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_Output);
				}
				else if (strCaption == ClsPrintDataField.strOutputValue)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_OutputValue);
				}
				else if (strCaption == ClsPrintDataField.strOutputUnit)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_OutputUnit);
				}
				else if (strCaption == ClsPrintDataField.strPhaseValue)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_PhaseValue);
				}
				else if (strCaption == ClsPrintDataField.strPhaseUnit)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_PhaseUnit);
				}
				else if (strCaption == ClsPrintDataField.strVoltageValue)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_VoltageValue);
				}
				else if (strCaption == ClsPrintDataField.strVoltageUnit)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_VoltageUnit);
				}
				else if (strCaption == ClsPrintDataField.strHzValue)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_HzValue);
				}
				else if (strCaption == ClsPrintDataField.strHzUnit)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_HzUnit);
				}
				else if (strCaption == ClsPrintDataField.strTokki)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_Tokki);
				}
				else if (strCaption == ClsPrintDataField.strCode)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_Code);
				}
				else if (strCaption == ClsPrintDataField.strCodeValue)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_CodeValue);
				}
				else if (strCaption == ClsPrintDataField.strPlace)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_Place);
				}
				else if (strCaption == ClsPrintDataField.strCaseNo)
				{
					CheckRowCellExceedMaxLen(dgvSrc, iColumnIndex, ClsGPDFM.iLen_CaseNo);
				}
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("CheckFieldExceedMaxLen exception:" + ex.Message);
		}
	}

	private void CheckRowCellExceedMaxLen(DataGridView dgvSrc, int iColumnIndex, int iLen)
	{
		DataGridViewCellStyle dgvCellStyle = new DataGridViewCellStyle();
		dgvCellStyle.ForeColor = Color.Red;
		for (int iRowIndex = 0; iRowIndex < dgvSrc.Rows.Count; iRowIndex++)
		{
			string strValue = dgvSrc.Rows[iRowIndex].Cells[iColumnIndex].Value.ToString().Trim();
			if (CheckCellExceedMaxLen(strValue, iLen))
			{
				dgvSrc.Rows[iRowIndex].Cells[iColumnIndex].Style = dgvCellStyle;
			}
		}
	}

	private bool CheckCellExceedMaxLen(string strSrc, int iLen)
	{
		try
		{
			if (strSrc.Length > iLen)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("CheckCellExceedMaxLen [strSrc=" + strSrc + "  iLen=" + iLen + " exception:" + ex.Message);
			return false;
		}
	}

	private void ShowPrintDataColumn()
	{
		for (int iColumnIndex = 0; iColumnIndex < dataGridViewPrintData.Columns.Count; iColumnIndex++)
		{
			dataGridViewPrintData.Columns[iColumnIndex].Visible = false;
			if (dataGridViewPrintData.Columns[iColumnIndex].HeaderText.Trim().ToUpper() != ClsPrintDataField.strID)
			{
				dataGridViewPrintData.Columns[iColumnIndex].Visible = true;
			}
		}
	}

	private void dataGridViewPrintData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
	{
		try
		{
			ClsShare.m_dtSelData = new DataTable();
			for (int iIndex = 0; iIndex < dataGridViewPrintData.Columns.Count; iIndex++)
			{
				ClsShare.m_dtSelData.Columns.Add(dataGridViewPrintData.Columns[iIndex].HeaderText);
				if (ClsShare.m_dtSelData.Rows.Count <= 0)
				{
					ClsShare.m_dtSelData.Rows.Add(dataGridViewPrintData.Rows[e.RowIndex]);
				}
				ClsShare.m_dtSelData.Rows[0][iIndex] = dataGridViewPrintData.Rows[e.RowIndex].Cells[iIndex].Value;
			}
			m_bDirectClose = false;
			Close();
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite(ex.Message);
		}
	}

	private void btnNextPage_Click(object sender, EventArgs e)
	{
		if (m_iCutPage < m_iPageCount)
		{
			m_iCutPage++;
			ShowCurPage(m_dtSaveData, m_iCutPage);
			btnPreviewPage.Enabled = true;
		}
		if (m_iCutPage == m_iPageCount)
		{
			btnNextPage.Enabled = false;
		}
	}

	private void btnPreviewPage_Click(object sender, EventArgs e)
	{
		if (m_iCutPage > 1)
		{
			m_iCutPage--;
			ShowCurPage(m_dtSaveData, m_iCutPage);
			btnNextPage.Enabled = true;
		}
		if (m_iCutPage <= 1)
		{
			btnPreviewPage.Enabled = false;
		}
	}

	private void btnFind_Click(object sender, EventArgs e)
	{
		try
		{
			if (checkBoxExceedMaxLen.Checked)
			{
				FindExceedMaxLenPrintData();
			}
			else
			{
				FindAllPrintData();
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("噴印資料選擇窗口中的搜索異常:" + ex.Message);
		}
	}

	private void FindAllPrintData()
	{
		string strTabName = ClsXml.GetPrintDataTableName();
		string strSql = "SELECT ROW_NUMBER() OVER(ORDER BY GETDATE()) AS " + strTabName.Substring(0, 2).Trim().ToUpper() + "Number,*  FROM " + strTabName + "  WHERE [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "] LIKE '%" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxCondition.Text) + "%'ORDER BY [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "]";
		IniDataEx(strSql);
	}

	private void FindLessMaxLenPrintData()
	{
		ClsGetPrintDataFieldMaxLen ClsGPDFM = new ClsGetPrintDataFieldMaxLen();
		string strTabName = ClsXml.GetPrintDataTableName();
		string strPrefix = strTabName.Trim().ToUpper().Substring(0, 2);
		string strSql = "SELECT ROW_NUMBER() OVER(ORDER BY GETDATE()) AS " + strTabName.Substring(0, 2).Trim().ToUpper() + "Number,*  FROM " + strTabName + "  WHERE [" + strPrefix + ClsPrintDataField.strKeyCode + "] LIKE '%" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxCondition.Text) + "%' AND  (  LEN([" + strPrefix + ClsPrintDataField.strBore + "]) <=" + ClsGPDFM.iLen_Bore + " AND LEN([" + strPrefix + ClsPrintDataField.strBoreUnit + "]) <=" + ClsGPDFM.iLen_BoreUnit + " AND LEN([" + strPrefix + ClsPrintDataField.strBoreValue + "]) <=" + ClsGPDFM.iLen_BoreValue + " AND LEN([" + strPrefix + ClsPrintDataField.strCaseNo + "]) <=" + ClsGPDFM.iLen_CaseNo + " AND LEN([" + strPrefix + ClsPrintDataField.strCode + "]) <=" + ClsGPDFM.iLen_Code + " AND LEN([" + strPrefix + ClsPrintDataField.strCodeValue + "]) <=" + ClsGPDFM.iLen_CodeValue + " AND LEN([" + strPrefix + ClsPrintDataField.strHzUnit + "]) <=" + ClsGPDFM.iLen_HzUnit + " AND LEN([" + strPrefix + ClsPrintDataField.strHzValue + "]) <=" + ClsGPDFM.iLen_HzValue + " AND LEN([" + strPrefix + ClsPrintDataField.strKeyCode + "]) <=" + ClsGPDFM.iLen_Keycode + " AND LEN([" + strPrefix + ClsPrintDataField.strModel + "]) <=" + ClsGPDFM.iLen_Model + " AND LEN([" + strPrefix + ClsPrintDataField.strModelValue + "]) <=" + ClsGPDFM.iLen_ModelValue + " AND LEN([" + strPrefix + ClsPrintDataField.strOutput + "]) <=" + ClsGPDFM.iLen_Output + " AND LEN([" + strPrefix + ClsPrintDataField.strOutputUnit + "]) <=" + ClsGPDFM.iLen_OutputUnit + " AND LEN([" + strPrefix + ClsPrintDataField.strOutputValue + "]) <=" + ClsGPDFM.iLen_OutputValue + " AND LEN([" + strPrefix + ClsPrintDataField.strPhaseUnit + "]) <=" + ClsGPDFM.iLen_PhaseUnit + " AND LEN([" + strPrefix + ClsPrintDataField.strPhaseValue + "]) <=" + ClsGPDFM.iLen_PhaseValue + " AND LEN([" + strPrefix + ClsPrintDataField.strPlace + "]) <=" + ClsGPDFM.iLen_Place + " AND LEN([" + strPrefix + ClsPrintDataField.strTokki + "]) <=" + ClsGPDFM.iLen_Tokki + " AND LEN([" + strPrefix + ClsPrintDataField.strVoltageUnit + "]) <=" + ClsGPDFM.iLen_VoltageUnit + " AND LEN([" + strPrefix + ClsPrintDataField.strVoltageValue + "]) <=" + ClsGPDFM.iLen_VoltageValue + " ) ORDER BY [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "]";
		IniDataEx(strSql);
	}

	private void FindExceedMaxLenPrintData()
	{
		ClsGetPrintDataFieldMaxLen ClsGPDFM = new ClsGetPrintDataFieldMaxLen();
		string strTabName = ClsXml.GetPrintDataTableName();
		string strPrefix = strTabName.Trim().ToUpper().Substring(0, 2);
		string strSql = "SELECT ROW_NUMBER() OVER(ORDER BY GETDATE()) AS " + strTabName.Substring(0, 2).Trim().ToUpper() + "Number,*  FROM " + strTabName + "  WHERE [" + strPrefix + ClsPrintDataField.strKeyCode + "] LIKE '%" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxCondition.Text) + "%' AND  (  LEN([" + strPrefix + ClsPrintDataField.strBore + "]) >" + ClsGPDFM.iLen_Bore + " OR LEN([" + strPrefix + ClsPrintDataField.strBoreUnit + "]) >" + ClsGPDFM.iLen_BoreUnit + " OR LEN([" + strPrefix + ClsPrintDataField.strBoreValue + "]) >" + ClsGPDFM.iLen_BoreValue + " OR LEN([" + strPrefix + ClsPrintDataField.strCaseNo + "]) >" + ClsGPDFM.iLen_CaseNo + " OR LEN([" + strPrefix + ClsPrintDataField.strCode + "]) >" + ClsGPDFM.iLen_Code + " OR LEN([" + strPrefix + ClsPrintDataField.strCodeValue + "]) >" + ClsGPDFM.iLen_CodeValue + " OR LEN([" + strPrefix + ClsPrintDataField.strHzUnit + "]) >" + ClsGPDFM.iLen_HzUnit + " OR LEN([" + strPrefix + ClsPrintDataField.strHzValue + "]) >" + ClsGPDFM.iLen_HzValue + " OR LEN([" + strPrefix + ClsPrintDataField.strKeyCode + "]) >" + ClsGPDFM.iLen_Keycode + " OR LEN([" + strPrefix + ClsPrintDataField.strModel + "]) >" + ClsGPDFM.iLen_Model + " OR LEN([" + strPrefix + ClsPrintDataField.strModelValue + "]) >" + ClsGPDFM.iLen_ModelValue + " OR LEN([" + strPrefix + ClsPrintDataField.strOutput + "]) >" + ClsGPDFM.iLen_Output + " OR LEN([" + strPrefix + ClsPrintDataField.strOutputUnit + "]) >" + ClsGPDFM.iLen_OutputUnit + " OR LEN([" + strPrefix + ClsPrintDataField.strOutputValue + "]) >" + ClsGPDFM.iLen_OutputValue + " OR LEN([" + strPrefix + ClsPrintDataField.strPhaseUnit + "]) >" + ClsGPDFM.iLen_PhaseUnit + " OR LEN([" + strPrefix + ClsPrintDataField.strPhaseValue + "]) >" + ClsGPDFM.iLen_PhaseValue + " OR LEN([" + strPrefix + ClsPrintDataField.strPlace + "]) >" + ClsGPDFM.iLen_Place + " OR LEN([" + strPrefix + ClsPrintDataField.strTokki + "]) >" + ClsGPDFM.iLen_Tokki + " OR LEN([" + strPrefix + ClsPrintDataField.strVoltageUnit + "]) >" + ClsGPDFM.iLen_VoltageUnit + " OR LEN([" + strPrefix + ClsPrintDataField.strVoltageValue + "]) >" + ClsGPDFM.iLen_VoltageValue + " ) ORDER BY [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "]";
		IniDataEx(strSql);
	}

	private void frmPrintDataSelect_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (m_bDirectClose)
		{
			ClsShare.m_dtSelData = null;
		}
	}

	private void pnlAllClient_Paint(object sender, PaintEventArgs e)
	{
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
		this.pnlClientTopFind = new System.Windows.Forms.Panel();
		this.checkBoxExceedMaxLen = new System.Windows.Forms.CheckBox();
		this.comboBoxMaxLenCtrl = new System.Windows.Forms.ComboBox();
		this.btnFind = new System.Windows.Forms.Button();
		this.textBoxCondition = new System.Windows.Forms.TextBox();
		this.lblKeyCode = new System.Windows.Forms.Label();
		this.pnlClientClientData = new System.Windows.Forms.Panel();
		this.dataGridViewPrintData = new System.Windows.Forms.DataGridView();
		this.btnPreviewPage = new System.Windows.Forms.Button();
		this.btnNextPage = new System.Windows.Forms.Button();
		this.gbPage = new System.Windows.Forms.GroupBox();
		this.lblPage = new System.Windows.Forms.Label();
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		this.pnlClientTopFind.SuspendLayout();
		this.pnlClientClientData.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewPrintData).BeginInit();
		this.gbPage.SuspendLayout();
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.pnlClientClientData);
		base.pnlClientData.Controls.Add(this.pnlClientTopFind);
		base.pnlClientData.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.pnlClientData.Size = new System.Drawing.Size(915, 341);
		base.pnlBottomCtrl.Controls.Add(this.gbPage);
		base.pnlBottomCtrl.Controls.Add(this.btnNextPage);
		base.pnlBottomCtrl.Controls.Add(this.btnPreviewPage);
		base.pnlBottomCtrl.Location = new System.Drawing.Point(0, 341);
		base.pnlBottomCtrl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.pnlClientTopFind.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientTopFind.Controls.Add(this.checkBoxExceedMaxLen);
		this.pnlClientTopFind.Controls.Add(this.comboBoxMaxLenCtrl);
		this.pnlClientTopFind.Controls.Add(this.btnFind);
		this.pnlClientTopFind.Controls.Add(this.textBoxCondition);
		this.pnlClientTopFind.Controls.Add(this.lblKeyCode);
		this.pnlClientTopFind.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlClientTopFind.Location = new System.Drawing.Point(0, 0);
		this.pnlClientTopFind.Name = "pnlClientTopFind";
		this.pnlClientTopFind.Size = new System.Drawing.Size(911, 74);
		this.pnlClientTopFind.TabIndex = 0;
		this.checkBoxExceedMaxLen.AutoSize = true;
		this.checkBoxExceedMaxLen.Location = new System.Drawing.Point(329, 24);
		this.checkBoxExceedMaxLen.Name = "checkBoxExceedMaxLen";
		this.checkBoxExceedMaxLen.Size = new System.Drawing.Size(240, 16);
		this.checkBoxExceedMaxLen.TabIndex = 7;
		this.checkBoxExceedMaxLen.Text = "只顯示實際資料長度超出預設長度的記錄";
		this.checkBoxExceedMaxLen.UseVisualStyleBackColor = true;
		this.comboBoxMaxLenCtrl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxMaxLenCtrl.FormattingEnabled = true;
		this.comboBoxMaxLenCtrl.Items.AddRange(new object[3] { "全部", "標準長度以內", "標準長度以外" });
		this.comboBoxMaxLenCtrl.Location = new System.Drawing.Point(726, 22);
		this.comboBoxMaxLenCtrl.Name = "comboBoxMaxLenCtrl";
		this.comboBoxMaxLenCtrl.Size = new System.Drawing.Size(121, 20);
		this.comboBoxMaxLenCtrl.TabIndex = 6;
		this.comboBoxMaxLenCtrl.Visible = false;
		this.btnFind.Location = new System.Drawing.Point(592, 16);
		this.btnFind.Name = "btnFind";
		this.btnFind.Size = new System.Drawing.Size(106, 33);
		this.btnFind.TabIndex = 4;
		this.btnFind.Text = "搜尋";
		this.btnFind.UseVisualStyleBackColor = true;
		this.btnFind.Click += new System.EventHandler(btnFind_Click);
		this.textBoxCondition.Location = new System.Drawing.Point(90, 22);
		this.textBoxCondition.Name = "textBoxCondition";
		this.textBoxCondition.Size = new System.Drawing.Size(197, 21);
		this.textBoxCondition.TabIndex = 3;
		this.lblKeyCode.AutoSize = true;
		this.lblKeyCode.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblKeyCode.Location = new System.Drawing.Point(5, 25);
		this.lblKeyCode.Name = "lblKeyCode";
		this.lblKeyCode.Size = new System.Drawing.Size(75, 14);
		this.lblKeyCode.TabIndex = 2;
		this.lblKeyCode.Text = "搜尋條件:";
		this.pnlClientClientData.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientClientData.Controls.Add(this.dataGridViewPrintData);
		this.pnlClientClientData.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientClientData.Location = new System.Drawing.Point(0, 74);
		this.pnlClientClientData.Name = "pnlClientClientData";
		this.pnlClientClientData.Size = new System.Drawing.Size(911, 263);
		this.pnlClientClientData.TabIndex = 1;
		this.dataGridViewPrintData.AllowUserToAddRows = false;
		this.dataGridViewPrintData.AllowUserToDeleteRows = false;
		this.dataGridViewPrintData.BackgroundColor = System.Drawing.SystemColors.Window;
		this.dataGridViewPrintData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewPrintData.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGridViewPrintData.Location = new System.Drawing.Point(0, 0);
		this.dataGridViewPrintData.MultiSelect = false;
		this.dataGridViewPrintData.Name = "dataGridViewPrintData";
		this.dataGridViewPrintData.ReadOnly = true;
		this.dataGridViewPrintData.RowTemplate.Height = 23;
		this.dataGridViewPrintData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGridViewPrintData.Size = new System.Drawing.Size(907, 259);
		this.dataGridViewPrintData.TabIndex = 9;
		this.dataGridViewPrintData.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridViewPrintData_CellDoubleClick);
		this.btnPreviewPage.Enabled = false;
		this.btnPreviewPage.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.btnPreviewPage.ForeColor = System.Drawing.Color.Black;
		this.btnPreviewPage.Location = new System.Drawing.Point(141, 23);
		this.btnPreviewPage.Name = "btnPreviewPage";
		this.btnPreviewPage.Size = new System.Drawing.Size(148, 66);
		this.btnPreviewPage.TabIndex = 9;
		this.btnPreviewPage.Text = "上一頁";
		this.btnPreviewPage.UseVisualStyleBackColor = true;
		this.btnPreviewPage.Click += new System.EventHandler(btnPreviewPage_Click);
		this.btnNextPage.Enabled = false;
		this.btnNextPage.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.btnNextPage.ForeColor = System.Drawing.Color.Black;
		this.btnNextPage.Location = new System.Drawing.Point(647, 23);
		this.btnNextPage.Name = "btnNextPage";
		this.btnNextPage.Size = new System.Drawing.Size(148, 66);
		this.btnNextPage.TabIndex = 10;
		this.btnNextPage.Text = "下一頁";
		this.btnNextPage.UseVisualStyleBackColor = true;
		this.btnNextPage.Click += new System.EventHandler(btnNextPage_Click);
		this.gbPage.Controls.Add(this.lblPage);
		this.gbPage.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.gbPage.Location = new System.Drawing.Point(321, 18);
		this.gbPage.Name = "gbPage";
		this.gbPage.Size = new System.Drawing.Size(295, 66);
		this.gbPage.TabIndex = 14;
		this.gbPage.TabStop = false;
		this.lblPage.AutoSize = true;
		this.lblPage.Font = new System.Drawing.Font("宋体", 18f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPage.ForeColor = System.Drawing.Color.Black;
		this.lblPage.Location = new System.Drawing.Point(6, 25);
		this.lblPage.Name = "lblPage";
		this.lblPage.Size = new System.Drawing.Size(23, 24);
		this.lblPage.TabIndex = 15;
		this.lblPage.Text = "/";
		this.lblPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(919, 460);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmPrintDataSelect";
		base.ShowInTaskbar = false;
		this.Text = "噴印資料選擇";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmPrintDataSelect_FormClosing);
		base.Load += new System.EventHandler(frmPrintDataSelect_Load);
		base.pnlClientData.ResumeLayout(false);
		base.pnlBottomCtrl.ResumeLayout(false);
		this.pnlClientTopFind.ResumeLayout(false);
		this.pnlClientTopFind.PerformLayout();
		this.pnlClientClientData.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dataGridViewPrintData).EndInit();
		this.gbPage.ResumeLayout(false);
		this.gbPage.PerformLayout();
		base.ResumeLayout(false);
	}
}
