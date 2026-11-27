using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmPrintDataBrowse : frmBase
{
	private string m_strSql = "";

	private Thread m_ThreadExport;

	private string m_strExcelFileName = "";

	private IContainer components;

	private DataGridView dataGridViewPrintData;

	private Button btnExport;

	private Label lblExportTip;

	public frmPrintDataBrowse(string strSql)
	{
		InitializeComponent();
		m_strSql = strSql;
	}

	private void btnExport_Click(object sender, EventArgs e)
	{
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
			Refresh();
			StartExportThread();
		}
	}

	private void StartExportThread()
	{
		m_ThreadExport = new Thread(ExportExcelThread);
		m_ThreadExport.IsBackground = true;
		m_ThreadExport.Start();
	}

	private void ExportExcelThread()
	{
		Invoke((EventHandler)delegate
		{
			ClsExcel.ExportExcel(dataGridViewPrintData, m_strExcelFileName, "噴印數據");
			lblExportTip.Visible = false;
			btnExport.Enabled = true;
			if (File.Exists(m_strExcelFileName))
			{
				MessageBox.Show("導出成功");
			}
			else
			{
				MessageBox.Show("導出失敗");
			}
		});
	}

	private void frmPrintDataBrowse_Load(object sender, EventArgs e)
	{
		dataGridViewPrintData.DataSource = ClsDataBase.SqlReadData(m_strSql);
		dataGridViewPrintData.Columns.RemoveAt(0);
		for (int iColumnIndex = 0; iColumnIndex < dataGridViewPrintData.Columns.Count; iColumnIndex++)
		{
			dataGridViewPrintData.Columns[iColumnIndex].HeaderText = dataGridViewPrintData.Columns[iColumnIndex].HeaderText.Substring(2);
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
		this.dataGridViewPrintData = new System.Windows.Forms.DataGridView();
		this.btnExport = new System.Windows.Forms.Button();
		this.lblExportTip = new System.Windows.Forms.Label();
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewPrintData).BeginInit();
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.lblExportTip);
		base.pnlClientData.Controls.Add(this.dataGridViewPrintData);
		base.pnlBottomCtrl.Controls.Add(this.btnExport);
		this.dataGridViewPrintData.AllowUserToAddRows = false;
		this.dataGridViewPrintData.BackgroundColor = System.Drawing.SystemColors.Window;
		this.dataGridViewPrintData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewPrintData.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGridViewPrintData.Location = new System.Drawing.Point(0, 0);
		this.dataGridViewPrintData.MultiSelect = false;
		this.dataGridViewPrintData.Name = "dataGridViewPrintData";
		this.dataGridViewPrintData.ReadOnly = true;
		this.dataGridViewPrintData.RowTemplate.Height = 23;
		this.dataGridViewPrintData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGridViewPrintData.Size = new System.Drawing.Size(911, 326);
		this.dataGridViewPrintData.TabIndex = 10;
		this.btnExport.Location = new System.Drawing.Point(68, 38);
		this.btnExport.Name = "btnExport";
		this.btnExport.Size = new System.Drawing.Size(106, 33);
		this.btnExport.TabIndex = 14;
		this.btnExport.Text = "導出";
		this.btnExport.UseVisualStyleBackColor = true;
		this.btnExport.Click += new System.EventHandler(btnExport_Click);
		this.lblExportTip.AutoSize = true;
		this.lblExportTip.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblExportTip.Location = new System.Drawing.Point(370, 150);
		this.lblExportTip.Name = "lblExportTip";
		this.lblExportTip.Size = new System.Drawing.Size(241, 29);
		this.lblExportTip.TabIndex = 36;
		this.lblExportTip.Text = "正在導出數據...";
		this.lblExportTip.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(919, 449);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmPrintDataBrowse";
		this.Text = "噴印數據瀏覽";
		base.Load += new System.EventHandler(frmPrintDataBrowse_Load);
		base.pnlClientData.ResumeLayout(false);
		base.pnlClientData.PerformLayout();
		base.pnlBottomCtrl.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dataGridViewPrintData).EndInit();
		base.ResumeLayout(false);
	}
}
