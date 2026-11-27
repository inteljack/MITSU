using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmProductRecord : frmBase
{
	private DataTable m_dtProRec;

	private IContainer components;

	private Panel pnlClientClientTopFind;

	private Panel pnlClientClientClientRecord;

	private Label label2;

	private GroupBox gbFindEnd;

	private GroupBox gbFindBegin;

	private Label label8;

	private Button btnFind;

	private DataGridView dataGridViewProductRecord;

	private Button btnLeave;

	private Button btnExport;

	private DateTimePicker dateTimePickerBegin;

	private DateTimePicker dateTimePickerEnd;

	public frmProductRecord()
	{
		InitializeComponent();
	}

	private void frmProductRecord_Load(object sender, EventArgs e)
	{
	}

	private void IniFindDate()
	{
	}

	private void textBoxFindYearBegin_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!ClsShare.OnlyInputNum(e.KeyChar))
		{
			e.Handled = true;
		}
	}

	private void textBoxFindMonthBegin_KeyUp(object sender, KeyEventArgs e)
	{
	}

	private void textBoxFindDayBegin_KeyUp(object sender, KeyEventArgs e)
	{
	}

	private void btnFind_Click(object sender, EventArgs e)
	{
		try
		{
			m_dtProRec = new DataTable();
			m_dtProRec = GetProductRecord();
			dataGridViewProductRecord.DataSource = m_dtProRec;
			ResetdataGridViewProductRecordColumnWidth(dataGridViewProductRecord);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("生產記錄窗口中的搜索異常:" + ex.Message);
		}
	}

	private void ResetdataGridViewProductRecordColumnWidth(DataGridView dgvSrc)
	{
		try
		{
			if (dgvSrc == null || dgvSrc.Columns.Count <= 0)
			{
				return;
			}
			for (int iColumnIndex = 0; iColumnIndex < dgvSrc.Columns.Count; iColumnIndex++)
			{
				if (iColumnIndex == 1 || iColumnIndex == 9 || iColumnIndex == 10)
				{
					dgvSrc.Columns[iColumnIndex].Width = 180;
				}
				else
				{
					dgvSrc.Columns[iColumnIndex].Width = 100;
				}
			}
		}
		catch
		{
		}
	}

	private void ResetDataGridViewColumnWidth(DataGridView dgvSrc)
	{
		try
		{
			if (dgvSrc != null && dgvSrc.Columns.Count > 0)
			{
				for (int iColumnIndex = 0; iColumnIndex < dgvSrc.Columns.Count; iColumnIndex++)
				{
					dgvSrc.Columns[iColumnIndex].Width = 130;
				}
			}
		}
		catch
		{
		}
	}

	private DataTable GetProductRecord()
	{
		DataTable dtDest = new DataTable();
		string strTabMasterName = ClsXml.GetProductRecordMasterTableName();
		string strTabDetailName = ClsXml.GetProductRecordDetailTableName();
		string strFieldPrefixMaster = strTabMasterName.Substring(0, 2);
		string strFieldPrefixDetail = strTabDetailName.Substring(0, 2);
		string strMasterBatchNo = strFieldPrefixMaster + ClsProductRecordMasterField.strBatchNo;
		string strDetailBatchNo = strFieldPrefixDetail + ClsProductRecordDetailField.strBatchNo;
		string strDetailPageEndTime = strFieldPrefixDetail + ClsProductRecordDetailField.strPageEndTime;
		string strRealPrintBoxCount = "RealPrintBoxCount";
		string strJoinTab = "(  SELECT " + strDetailBatchNo + " ,  COUNT (" + strDetailPageEndTime + ") AS  [" + strRealPrintBoxCount.Trim() + "]  FROM " + strTabDetailName + " WHERE  [" + strDetailPageEndTime + "]  IS NOT NULL  GROUP BY " + strDetailBatchNo + " ) D  LEFT JOIN " + strTabMasterName + "  M    ON  M." + strMasterBatchNo + "= D." + strDetailBatchNo + " WHERE  M.[" + strFieldPrefixMaster + ClsProductRecordField_Show.strKeyCode + "]  IS NOT NULL  ";
		string strBeginPrintTime = "CAST('" + dateTimePickerBegin.Value.Year + "-" + dateTimePickerBegin.Value.Month + "-" + dateTimePickerBegin.Value.Day + "' AS DATETIME)";
		string strEndPrintTime = "DATEADD(DAY,1,CAST('" + dateTimePickerEnd.Value.Year + "-" + dateTimePickerEnd.Value.Month + "-" + dateTimePickerEnd.Value.Day + "' AS DATETIME)) ";
		string strSelConditon = "M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strBeginPrintTime + "]>=" + strBeginPrintTime + " AND M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strEndPrintTime + "]<" + strEndPrintTime;
		string strSql = "SELECT  ROW_NUMBER()  OVER(ORDER BY M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strBeginPrintTime + "] ) AS [" + ClsProductRecordField_Show.strOrderNo + "],M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strKeyCode + "] AS [" + ClsProductRecordField_Show.strKeyCode + "],M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strMadeNo + "]  AS [" + ClsProductRecordField_Show.strMadeNo + "],M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strCaseNo + "] AS [" + ClsProductRecordField_Show.strCaseNo + "],CAST(M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strBeginBoxNo + "] AS INT)+1  AS [" + ClsProductRecordField_Show.strBeginBoxNo + "],CAST(M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strBeginBoxNo + "] AS INT)+ CAST(D.[" + strRealPrintBoxCount.Trim() + "] AS INT)  AS [" + ClsProductRecordField_Show.strEndBoxNo + "],D.[" + strRealPrintBoxCount.Trim() + "] AS [" + ClsProductRecordField_Show.strRealPrintBoxCount + "],M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strBeginMachineNo + "]  AS [" + ClsProductRecordField_Show.strBeginMachineNo + "],M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strEndMachineNo + "]  AS [" + ClsProductRecordField_Show.strEndMachineNo + "],M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strBeginPrintTime + "]  AS [" + ClsProductRecordField_Show.strBeginPrintTime + "],M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strEndPrintTime + "]  AS [" + ClsProductRecordField_Show.strEndPrintTime + "]  FROM " + strJoinTab + " AND " + strSelConditon + " ORDER BY M.[" + strFieldPrefixMaster + ClsProductRecordMasterField.strBeginPrintTime + "] ASC, D.[" + strRealPrintBoxCount + "] ASC ";
		return ClsDataBase.SqlReadData(strSql);
	}

	private void ResetProductRecordListHeadText()
	{
		for (int iColumnIndex = 0; iColumnIndex < dataGridViewProductRecord.Columns.Count; iColumnIndex++)
		{
			string strColumnHeadText = dataGridViewProductRecord.Columns[iColumnIndex].HeaderText;
			string strColumnHeadTextTmp = strColumnHeadText.Substring(2);
			if (strColumnHeadTextTmp == ClsProductRecordDetailField.strID)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].Visible = false;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordDetailField.strPageNo)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].Visible = false;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordDetailField.strKeyCode)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordDetailField_Show.strKeyCode;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordDetailField.strMadeNo)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordDetailField_Show.strMadeNo;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordDetailField.strCaseNo)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordDetailField_Show.strCaseNo;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordDetailField.strCurrentPrintBoxNo)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordDetailField_Show.strCurrentPrintBoxNo;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordMasterField.strBeginBoxNo)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordMasterField_Show.strBeginBoxNo;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordMasterField.strEndBoxNo)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordMasterField_Show.strEndBoxNo;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordMasterField.strBeginMachineNo)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordMasterField_Show.strBeginMachineNo;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordMasterField.strEndMachineNo)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordMasterField_Show.strEndMachineNo;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordDetailField.strPageBeginTime)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordDetailField_Show.strPageBeginTime;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordDetailField.strPageEndTime)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordDetailField_Show.strPageEndTime;
			}
			else if (strColumnHeadTextTmp == ClsProductRecordDetailField.strBatchNo)
			{
				dataGridViewProductRecord.Columns[iColumnIndex].HeaderText = ClsProductRecordDetailField_Show.strBatchNo;
			}
		}
	}

	private void btnLeave_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void btnExport_Click(object sender, EventArgs e)
	{
		try
		{
			string strTabMasterName = ClsXml.GetProductRecordMasterTableName();
			string strTabDetailName = ClsXml.GetProductRecordDetailTableName();
			if (!ClsDataBase.CheckExistsTable(strTabMasterName) || !ClsDataBase.CheckExistsTable(strTabDetailName))
			{
				MessageBox.Show("導出失敗，請檢查數據庫連接是否正常!");
				return;
			}
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "導出生產記錄";
			sfd.Filter = "All Files(*.*)|*.*|Excel(*.xls)|*.xls";
			sfd.FilterIndex = 2;
			sfd.InitialDirectory = ClsXml.GetExportFloder();
			if (DialogResult.OK != sfd.ShowDialog())
			{
				return;
			}
			string strExcelFileName = sfd.FileName;
			if (m_dtProRec == null)
			{
				MessageBox.Show("無法導出記錄，請確認已經搜索了需要導出的記錄！");
				return;
			}
			if (m_dtProRec.Rows.Count > 65535)
			{
				MessageBox.Show("無法導出記錄，請確認記錄行數在最大導出行數65535之內！");
				return;
			}
			ClsExcel.ExportExcel(m_dtProRec, strExcelFileName, "ProductRecord");
			if (File.Exists(strExcelFileName))
			{
				MessageBox.Show("導出成功");
			}
			else
			{
				MessageBox.Show("導出失敗");
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	private string GetCurTime()
	{
		return DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "  " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond;
	}

	private void button1_Click(object sender, EventArgs e)
	{
		string s = dateTimePickerBegin.Value.Year + "-" + dateTimePickerBegin.Value.Month + "-" + dateTimePickerBegin.Value.Day;
		MessageBox.Show(s);
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
		this.pnlClientClientTopFind = new System.Windows.Forms.Panel();
		this.btnFind = new System.Windows.Forms.Button();
		this.label8 = new System.Windows.Forms.Label();
		this.gbFindEnd = new System.Windows.Forms.GroupBox();
		this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
		this.gbFindBegin = new System.Windows.Forms.GroupBox();
		this.dateTimePickerBegin = new System.Windows.Forms.DateTimePicker();
		this.label2 = new System.Windows.Forms.Label();
		this.pnlClientClientClientRecord = new System.Windows.Forms.Panel();
		this.dataGridViewProductRecord = new System.Windows.Forms.DataGridView();
		this.btnLeave = new System.Windows.Forms.Button();
		this.btnExport = new System.Windows.Forms.Button();
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		this.pnlClientClientTopFind.SuspendLayout();
		this.gbFindEnd.SuspendLayout();
		this.gbFindBegin.SuspendLayout();
		this.pnlClientClientClientRecord.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewProductRecord).BeginInit();
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.pnlClientClientClientRecord);
		base.pnlClientData.Controls.Add(this.pnlClientClientTopFind);
		base.pnlBottomCtrl.Controls.Add(this.btnLeave);
		base.pnlBottomCtrl.Controls.Add(this.btnExport);
		this.pnlClientClientTopFind.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientClientTopFind.Controls.Add(this.btnFind);
		this.pnlClientClientTopFind.Controls.Add(this.label8);
		this.pnlClientClientTopFind.Controls.Add(this.gbFindEnd);
		this.pnlClientClientTopFind.Controls.Add(this.gbFindBegin);
		this.pnlClientClientTopFind.Controls.Add(this.label2);
		this.pnlClientClientTopFind.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlClientClientTopFind.Location = new System.Drawing.Point(0, 0);
		this.pnlClientClientTopFind.Name = "pnlClientClientTopFind";
		this.pnlClientClientTopFind.Size = new System.Drawing.Size(911, 93);
		this.pnlClientClientTopFind.TabIndex = 0;
		this.btnFind.Font = new System.Drawing.Font("宋体", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnFind.Location = new System.Drawing.Point(704, 28);
		this.btnFind.Name = "btnFind";
		this.btnFind.Size = new System.Drawing.Size(106, 38);
		this.btnFind.TabIndex = 13;
		this.btnFind.Text = "搜尋";
		this.btnFind.UseVisualStyleBackColor = true;
		this.btnFind.Click += new System.EventHandler(btnFind_Click);
		this.label8.AutoSize = true;
		this.label8.Font = new System.Drawing.Font("宋体", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label8.Location = new System.Drawing.Point(373, 37);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(25, 16);
		this.label8.TabIndex = 11;
		this.label8.Text = "到";
		this.gbFindEnd.Controls.Add(this.dateTimePickerEnd);
		this.gbFindEnd.Location = new System.Drawing.Point(419, 22);
		this.gbFindEnd.Name = "gbFindEnd";
		this.gbFindEnd.Size = new System.Drawing.Size(246, 44);
		this.gbFindEnd.TabIndex = 12;
		this.gbFindEnd.TabStop = false;
		this.dateTimePickerEnd.Location = new System.Drawing.Point(18, 15);
		this.dateTimePickerEnd.Name = "dateTimePickerEnd";
		this.dateTimePickerEnd.Size = new System.Drawing.Size(206, 21);
		this.dateTimePickerEnd.TabIndex = 16;
		this.gbFindBegin.Controls.Add(this.dateTimePickerBegin);
		this.gbFindBegin.Location = new System.Drawing.Point(103, 22);
		this.gbFindBegin.Name = "gbFindBegin";
		this.gbFindBegin.Size = new System.Drawing.Size(246, 44);
		this.gbFindBegin.TabIndex = 11;
		this.gbFindBegin.TabStop = false;
		this.dateTimePickerBegin.Location = new System.Drawing.Point(21, 15);
		this.dateTimePickerBegin.Name = "dateTimePickerBegin";
		this.dateTimePickerBegin.Size = new System.Drawing.Size(206, 21);
		this.dateTimePickerBegin.TabIndex = 15;
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("宋体", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label2.Location = new System.Drawing.Point(13, 37);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(85, 16);
		this.label2.TabIndex = 4;
		this.label2.Text = "生產日期:";
		this.pnlClientClientClientRecord.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientClientClientRecord.Controls.Add(this.dataGridViewProductRecord);
		this.pnlClientClientClientRecord.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientClientClientRecord.Location = new System.Drawing.Point(0, 93);
		this.pnlClientClientClientRecord.Name = "pnlClientClientClientRecord";
		this.pnlClientClientClientRecord.Size = new System.Drawing.Size(911, 233);
		this.pnlClientClientClientRecord.TabIndex = 1;
		this.dataGridViewProductRecord.AllowUserToAddRows = false;
		this.dataGridViewProductRecord.AllowUserToDeleteRows = false;
		this.dataGridViewProductRecord.BackgroundColor = System.Drawing.SystemColors.Window;
		this.dataGridViewProductRecord.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewProductRecord.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGridViewProductRecord.Location = new System.Drawing.Point(0, 0);
		this.dataGridViewProductRecord.MultiSelect = false;
		this.dataGridViewProductRecord.Name = "dataGridViewProductRecord";
		this.dataGridViewProductRecord.ReadOnly = true;
		this.dataGridViewProductRecord.RowTemplate.Height = 23;
		this.dataGridViewProductRecord.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGridViewProductRecord.Size = new System.Drawing.Size(907, 229);
		this.dataGridViewProductRecord.TabIndex = 10;
		this.btnLeave.Location = new System.Drawing.Point(262, 25);
		this.btnLeave.Name = "btnLeave";
		this.btnLeave.Size = new System.Drawing.Size(106, 33);
		this.btnLeave.TabIndex = 14;
		this.btnLeave.Text = "離開";
		this.btnLeave.UseVisualStyleBackColor = true;
		this.btnLeave.Visible = false;
		this.btnLeave.Click += new System.EventHandler(btnLeave_Click);
		this.btnExport.Location = new System.Drawing.Point(90, 25);
		this.btnExport.Name = "btnExport";
		this.btnExport.Size = new System.Drawing.Size(106, 33);
		this.btnExport.TabIndex = 13;
		this.btnExport.Text = "導出";
		this.btnExport.UseVisualStyleBackColor = true;
		this.btnExport.Click += new System.EventHandler(btnExport_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(919, 449);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.Name = "frmProductRecord";
		this.Text = "生產記錄";
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		base.Load += new System.EventHandler(frmProductRecord_Load);
		base.pnlClientData.ResumeLayout(false);
		base.pnlBottomCtrl.ResumeLayout(false);
		this.pnlClientClientTopFind.ResumeLayout(false);
		this.pnlClientClientTopFind.PerformLayout();
		this.gbFindEnd.ResumeLayout(false);
		this.gbFindBegin.ResumeLayout(false);
		this.pnlClientClientClientRecord.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dataGridViewProductRecord).EndInit();
		base.ResumeLayout(false);
	}
}
