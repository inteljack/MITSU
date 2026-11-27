using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmMain : Form
{
	private frmPrintCtrl m_frmPC;

	public static string m_strFrmMainTitle = "";

	private IContainer components;

	private Panel pnlClient;

	private Panel pnlClientClient;

	private Panel pnlClientTop;

	private MenuStrip menuStripMain;

	private ToolStripMenuItem ToolStripMenuItemPrintCtrl;

	private ToolStripMenuItem ToolStripMenuItemPrintDataManage;

	private ToolStripMenuItem ToolStripMenuItemBoxStdManage;

	private ToolStripMenuItem ToolStripMenuItemProductRecord;

	private ToolStripMenuItem ToolStripMenuItemSysSet;

	private ToolStripMenuItem ToolStripMenuItemGroupMsg;

	private ToolStripMenuItem ToolStripMenuItemAbout;

	public frmMain()
	{
		InitializeComponent();
		IniMainFormParam();
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		ClsRegHotKey.WndPrcRegHotKey(ref m, base.Handle);
	}

	private void ToolStripMenuItemPrintCtrl_Click(object sender, EventArgs e)
	{
		if (m_frmPC == null)
		{
			m_frmPC = new frmPrintCtrl();
		}
		ShowFormOnObj(pnlClientClient, m_frmPC);
	}

	private void ShowFormOnObj(object objParent, Form frm, bool bNeedLoginCheck = false)
	{
		if (ClsShare.m_btnStop != null && ClsShare.m_btnStop.Enabled)
		{
			return;
		}
		int iCurrentControlsIndex = 0;
		for (int iControlsIndex = 0; iControlsIndex < ((Panel)objParent).Controls.Count; iControlsIndex++)
		{
			if (((Panel)objParent).Controls[iControlsIndex].Name != m_frmPC.Name && ((Panel)objParent).Controls[iControlsIndex].Visible)
			{
				iCurrentControlsIndex = iControlsIndex;
				break;
			}
		}
		for (int i = 0; i < ((Panel)objParent).Controls.Count; i++)
		{
			if (((Panel)objParent).Controls[i].Name != m_frmPC.Name)
			{
				if (!(((Panel)objParent).Controls[i].Name != frm.Name))
				{
					((Panel)objParent).Controls[i].Visible = true;
					return;
				}
				((Panel)objParent).Controls[i].Visible = false;
			}
			else
			{
				((Panel)objParent).Controls[i].Visible = false;
			}
		}
		if (bNeedLoginCheck)
		{
			switch (LoginCheck(frm))
			{
			case DialogResult.No:
				MessageBox.Show("密碼錯誤");
				((Panel)objParent).Controls[iCurrentControlsIndex].Visible = true;
				return;
			case DialogResult.Cancel:
				((Panel)objParent).Controls[iCurrentControlsIndex].Visible = true;
				return;
			}
		}
		int iNeedKeepFrmCount = 0;
		int iControlsTmpIndex = 0;
		while (((Panel)objParent).Controls.Count > iNeedKeepFrmCount)
		{
			if (((Panel)objParent).Controls[iControlsTmpIndex].Name != m_frmPC.Name)
			{
				if (!(((Panel)objParent).Controls[iControlsTmpIndex].Name != frm.Name))
				{
					((Panel)objParent).Controls[iControlsTmpIndex].Visible = true;
					iControlsTmpIndex++;
					return;
				}
				((Panel)objParent).Controls[iControlsTmpIndex].Visible = false;
				((Panel)objParent).Controls.RemoveAt(iControlsTmpIndex);
			}
			else
			{
				((Panel)objParent).Controls[iControlsTmpIndex].Visible = false;
				iNeedKeepFrmCount = 1;
			}
		}
		frm.TopLevel = false;
		frm.Dock = DockStyle.None;
		frm.FormBorderStyle = FormBorderStyle.None;
		frm.WindowState = FormWindowState.Maximized;
		frm.AutoScaleMode = AutoScaleMode.Dpi;
		frm.Icon = base.Icon;
		((Panel)objParent).Controls.Add(frm);
		try
		{
			frm.Show();
			Text = m_strFrmMainTitle + "---" + frm.Text;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("ShowFrmPrintCtrlShow" + ex.Message);
			frm.Show();
			Text = m_strFrmMainTitle + "---" + frm.Text;
		}
	}

	private void ToolStripMenuItemPrintDataManage_Click(object sender, EventArgs e)
	{
		frmPrintDataManage frmPDM = new frmPrintDataManage();
		frmPDM.WindowState = FormWindowState.Maximized;
		ShowFormOnObj(pnlClientClient, frmPDM, bNeedLoginCheck: true);
	}

	private void ToolStripMenuItemBoxStdManage_Click(object sender, EventArgs e)
	{
		frmCartonSizeManage frmBSM = new frmCartonSizeManage();
		frmBSM.WindowState = FormWindowState.Maximized;
		ShowFormOnObj(pnlClientClient, frmBSM, bNeedLoginCheck: true);
	}

	private void ToolStripMenuItemProductRecord_Click(object sender, EventArgs e)
	{
		frmProductRecord frmPR = new frmProductRecord();
		ShowFormOnObj(pnlClientClient, frmPR);
	}

	private void ToolStripMenuItemSysSet_Click(object sender, EventArgs e)
	{
		frmSysSet frmSS = new frmSysSet();
		ShowFormOnObj(pnlClientClient, frmSS, bNeedLoginCheck: true);
	}

	private DialogResult LoginCheck(Form frm)
	{
		frmLogin frmL = new frmLogin();
		return frmL.ShowDialog();
	}

	private void frmMain_Load(object sender, EventArgs e)
	{
		ClsShare.m_bRestartApp = false;
		m_strFrmMainTitle = Text;
		ToolStripMenuItemPrintCtrl_Click(sender, e);
		ClsRegHotKey.RegKey(base.Handle, ClsRegHotKey.iHotKeyID, 7, 46);
	}

	private void IniMainFormParam()
	{
		base.Left = ClsXml.GetMainFormX();
		base.Top = ClsXml.GetMainFormY();
		base.Width = ClsXml.GetMainFormWidth();
		base.Height = ClsXml.GetMainFormHeight();
	}

	private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
	{
		try
		{
			if (!ClsShare.m_bRestartApp)
			{
				string strText = "是否關閉程式?";
				string strCaption = "系統提示";
				DialogResult dlgRlt = MessageBox.Show(strText, strCaption, MessageBoxButtons.YesNo);
				if (dlgRlt == DialogResult.Yes)
				{
					ClsRegHotKey.UnRegKey(base.Handle, ClsRegHotKey.iHotKeyID);
					if (m_frmPC != null)
					{
						m_frmPC.Close();
					}
					Environment.Exit(0);
				}
				else
				{
					e.Cancel = true;
				}
			}
			else
			{
				ClsRegHotKey.UnRegKey(base.Handle, ClsRegHotKey.iHotKeyID);
				if (m_frmPC != null)
				{
					m_frmPC.Close();
				}
				Environment.Exit(0);
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Application exit exception:" + ex.Message);
			ClsRegHotKey.UnRegKey(base.Handle, ClsRegHotKey.iHotKeyID);
			Environment.Exit(0);
		}
	}

	private void ToolStripMenuItemGroupMsg_Click(object sender, EventArgs e)
	{
		frmGroupMsg frmGM = new frmGroupMsg();
		ShowFormOnObj(pnlClientClient, frmGM);
	}

	private void frmMain_Shown(object sender, EventArgs e)
	{
	}

	private void frmMain_Resize(object sender, EventArgs e)
	{
	}

	private void pnlClientClient_Resize(object sender, EventArgs e)
	{
		List<Form> ListFrm = new List<Form>();
		for (int iControlsIndex = 0; iControlsIndex < pnlClientClient.Controls.Count; iControlsIndex++)
		{
			ListFrm.Add((Form)pnlClientClient.Controls[iControlsIndex]);
		}
		pnlClientClient.Controls.Clear();
		foreach (Form frm in ListFrm)
		{
			frm.TopLevel = false;
			frm.Dock = DockStyle.None;
			frm.FormBorderStyle = FormBorderStyle.None;
			frm.WindowState = FormWindowState.Maximized;
			frm.AutoScaleMode = AutoScaleMode.Dpi;
			frm.Size = pnlClientClient.Size;
			pnlClientClient.Controls.Add(frm);
			frm.Show();
		}
	}

	private void frmMain_Move(object sender, EventArgs e)
	{
	}

	private void ToolStripMenuItemAbout_Click(object sender, EventArgs e)
	{
		frmAbout frmA = new frmAbout();
		frmA.Icon = base.Icon;
		string strLastMainTitle = Text;
		Text = m_strFrmMainTitle + "---" + frmA.Text;
		frmA.ShowDialog();
		Text = strLastMainTitle;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeJianPrintingConsole.frmMain));
		this.pnlClient = new System.Windows.Forms.Panel();
		this.pnlClientClient = new System.Windows.Forms.Panel();
		this.pnlClientTop = new System.Windows.Forms.Panel();
		this.menuStripMain = new System.Windows.Forms.MenuStrip();
		this.ToolStripMenuItemPrintCtrl = new System.Windows.Forms.ToolStripMenuItem();
		this.ToolStripMenuItemPrintDataManage = new System.Windows.Forms.ToolStripMenuItem();
		this.ToolStripMenuItemBoxStdManage = new System.Windows.Forms.ToolStripMenuItem();
		this.ToolStripMenuItemProductRecord = new System.Windows.Forms.ToolStripMenuItem();
		this.ToolStripMenuItemGroupMsg = new System.Windows.Forms.ToolStripMenuItem();
		this.ToolStripMenuItemSysSet = new System.Windows.Forms.ToolStripMenuItem();
		this.ToolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
		this.pnlClient.SuspendLayout();
		this.pnlClientTop.SuspendLayout();
		this.menuStripMain.SuspendLayout();
		base.SuspendLayout();
		this.pnlClient.Controls.Add(this.pnlClientClient);
		this.pnlClient.Controls.Add(this.pnlClientTop);
		this.pnlClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClient.Location = new System.Drawing.Point(0, 0);
		this.pnlClient.Margin = new System.Windows.Forms.Padding(4);
		this.pnlClient.Name = "pnlClient";
		this.pnlClient.Size = new System.Drawing.Size(1016, 734);
		this.pnlClient.TabIndex = 0;
		this.pnlClientClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientClient.Location = new System.Drawing.Point(0, 31);
		this.pnlClientClient.Margin = new System.Windows.Forms.Padding(4);
		this.pnlClientClient.Name = "pnlClientClient";
		this.pnlClientClient.Size = new System.Drawing.Size(1016, 703);
		this.pnlClientClient.TabIndex = 3;
		this.pnlClientClient.Resize += new System.EventHandler(pnlClientClient_Resize);
		this.pnlClientTop.Controls.Add(this.menuStripMain);
		this.pnlClientTop.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlClientTop.Location = new System.Drawing.Point(0, 0);
		this.pnlClientTop.Margin = new System.Windows.Forms.Padding(4);
		this.pnlClientTop.Name = "pnlClientTop";
		this.pnlClientTop.Size = new System.Drawing.Size(1016, 31);
		this.pnlClientTop.TabIndex = 2;
		this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.ToolStripMenuItemPrintCtrl, this.ToolStripMenuItemPrintDataManage, this.ToolStripMenuItemBoxStdManage, this.ToolStripMenuItemProductRecord, this.ToolStripMenuItemGroupMsg, this.ToolStripMenuItemSysSet, this.ToolStripMenuItemAbout });
		this.menuStripMain.Location = new System.Drawing.Point(0, 0);
		this.menuStripMain.Name = "menuStripMain";
		this.menuStripMain.Padding = new System.Windows.Forms.Padding(8, 3, 0, 3);
		this.menuStripMain.Size = new System.Drawing.Size(1016, 24);
		this.menuStripMain.TabIndex = 1;
		this.menuStripMain.Text = "menuStrip1";
		this.ToolStripMenuItemPrintCtrl.Name = "ToolStripMenuItemPrintCtrl";
		this.ToolStripMenuItemPrintCtrl.Size = new System.Drawing.Size(65, 18);
		this.ToolStripMenuItemPrintCtrl.Text = "噴印控制";
		this.ToolStripMenuItemPrintCtrl.Click += new System.EventHandler(ToolStripMenuItemPrintCtrl_Click);
		this.ToolStripMenuItemPrintDataManage.Name = "ToolStripMenuItemPrintDataManage";
		this.ToolStripMenuItemPrintDataManage.Size = new System.Drawing.Size(89, 18);
		this.ToolStripMenuItemPrintDataManage.Text = "噴印資料管理";
		this.ToolStripMenuItemPrintDataManage.Click += new System.EventHandler(ToolStripMenuItemPrintDataManage_Click);
		this.ToolStripMenuItemBoxStdManage.Name = "ToolStripMenuItemBoxStdManage";
		this.ToolStripMenuItemBoxStdManage.Size = new System.Drawing.Size(89, 18);
		this.ToolStripMenuItemBoxStdManage.Text = "紙箱規格管理";
		this.ToolStripMenuItemBoxStdManage.Click += new System.EventHandler(ToolStripMenuItemBoxStdManage_Click);
		this.ToolStripMenuItemProductRecord.Name = "ToolStripMenuItemProductRecord";
		this.ToolStripMenuItemProductRecord.Size = new System.Drawing.Size(65, 18);
		this.ToolStripMenuItemProductRecord.Text = "生產記錄";
		this.ToolStripMenuItemProductRecord.Click += new System.EventHandler(ToolStripMenuItemProductRecord_Click);
		this.ToolStripMenuItemGroupMsg.Name = "ToolStripMenuItemGroupMsg";
		this.ToolStripMenuItemGroupMsg.Size = new System.Drawing.Size(65, 18);
		this.ToolStripMenuItemGroupMsg.Text = "合成模板";
		this.ToolStripMenuItemGroupMsg.Visible = false;
		this.ToolStripMenuItemGroupMsg.Click += new System.EventHandler(ToolStripMenuItemGroupMsg_Click);
		this.ToolStripMenuItemSysSet.Name = "ToolStripMenuItemSysSet";
		this.ToolStripMenuItemSysSet.Size = new System.Drawing.Size(65, 18);
		this.ToolStripMenuItemSysSet.Text = "系統設定";
		this.ToolStripMenuItemSysSet.Click += new System.EventHandler(ToolStripMenuItemSysSet_Click);
		this.ToolStripMenuItemAbout.Name = "ToolStripMenuItemAbout";
		this.ToolStripMenuItemAbout.Size = new System.Drawing.Size(41, 18);
		this.ToolStripMenuItemAbout.Text = "關於";
		this.ToolStripMenuItemAbout.Click += new System.EventHandler(ToolStripMenuItemAbout_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.ClientSize = new System.Drawing.Size(1016, 734);
		base.Controls.Add(this.pnlClient);
		this.Font = new System.Drawing.Font("宋体", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "frmMain";
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "鶴見噴印控制臺軟體";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmMain_FormClosing);
		base.Load += new System.EventHandler(frmMain_Load);
		base.Shown += new System.EventHandler(frmMain_Shown);
		base.Move += new System.EventHandler(frmMain_Move);
		base.Resize += new System.EventHandler(frmMain_Resize);
		this.pnlClient.ResumeLayout(false);
		this.pnlClientTop.ResumeLayout(false);
		this.pnlClientTop.PerformLayout();
		this.menuStripMain.ResumeLayout(false);
		this.menuStripMain.PerformLayout();
		base.ResumeLayout(false);
	}
}
