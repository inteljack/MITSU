using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HeJianPrintingConsole;

public class frmTimer : Form
{
	private int m_iCount = 1;

	private IContainer components;

	private Panel pnlClient;

	private Panel pnlClientClientShow;

	private Panel pnlClientTopTitle;

	private Label lblTitle;

	private Label lblShowContent;

	private Timer timer1;

	public frmTimer(string strTitle, int iTimeCount)
	{
		InitializeComponent();
		lblTitle.Text = strTitle;
		lblShowContent.Text = iTimeCount.ToString();
		m_iCount = iTimeCount;
	}

	private void pnlClientTopTitle_Resize(object sender, EventArgs e)
	{
		lblTitle.Left = (pnlClientTopTitle.Width - lblTitle.Width) / 2;
		lblTitle.Top = (pnlClientTopTitle.Height - lblTitle.Height) / 2;
	}

	private void pnlClientClientShow_Resize(object sender, EventArgs e)
	{
		lblShowContent.Left = (pnlClientClientShow.Width - lblShowContent.Width) / 2;
		lblShowContent.Top = (pnlClientClientShow.Height - lblShowContent.Height) / 2;
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		if (m_iCount > 0)
		{
			lblShowContent.Text = (--m_iCount).ToString();
			return;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void frmTimer_Load(object sender, EventArgs e)
	{
		timer1.Enabled = true;
	}

	private void pnlClient_Paint(object sender, PaintEventArgs e)
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
		this.components = new System.ComponentModel.Container();
		this.pnlClient = new System.Windows.Forms.Panel();
		this.pnlClientTopTitle = new System.Windows.Forms.Panel();
		this.pnlClientClientShow = new System.Windows.Forms.Panel();
		this.lblTitle = new System.Windows.Forms.Label();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.lblShowContent = new System.Windows.Forms.Label();
		this.pnlClient.SuspendLayout();
		this.pnlClientTopTitle.SuspendLayout();
		this.pnlClientClientShow.SuspendLayout();
		base.SuspendLayout();
		this.pnlClient.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pnlClient.Controls.Add(this.pnlClientClientShow);
		this.pnlClient.Controls.Add(this.pnlClientTopTitle);
		this.pnlClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClient.Location = new System.Drawing.Point(0, 0);
		this.pnlClient.Name = "pnlClient";
		this.pnlClient.Size = new System.Drawing.Size(695, 400);
		this.pnlClient.TabIndex = 0;
		this.pnlClient.Paint += new System.Windows.Forms.PaintEventHandler(pnlClient_Paint);
		this.pnlClientTopTitle.Controls.Add(this.lblTitle);
		this.pnlClientTopTitle.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlClientTopTitle.Location = new System.Drawing.Point(0, 0);
		this.pnlClientTopTitle.Name = "pnlClientTopTitle";
		this.pnlClientTopTitle.Size = new System.Drawing.Size(693, 57);
		this.pnlClientTopTitle.TabIndex = 0;
		this.pnlClientTopTitle.Resize += new System.EventHandler(pnlClientTopTitle_Resize);
		this.pnlClientClientShow.Controls.Add(this.lblShowContent);
		this.pnlClientClientShow.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientClientShow.Location = new System.Drawing.Point(0, 57);
		this.pnlClientClientShow.Name = "pnlClientClientShow";
		this.pnlClientClientShow.Size = new System.Drawing.Size(693, 341);
		this.pnlClientClientShow.TabIndex = 1;
		this.pnlClientClientShow.Resize += new System.EventHandler(pnlClientClientShow_Resize);
		this.lblTitle.AutoSize = true;
		this.lblTitle.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblTitle.Location = new System.Drawing.Point(279, 13);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(93, 29);
		this.lblTitle.TabIndex = 0;
		this.lblTitle.Text = "Title";
		this.timer1.Interval = 1000;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		this.lblShowContent.AutoSize = true;
		this.lblShowContent.Font = new System.Drawing.Font("宋体", 42f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblShowContent.Location = new System.Drawing.Point(298, 112);
		this.lblShowContent.Name = "lblShowContent";
		this.lblShowContent.Size = new System.Drawing.Size(53, 56);
		this.lblShowContent.TabIndex = 0;
		this.lblShowContent.Text = "1";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.ClientSize = new System.Drawing.Size(695, 400);
		base.Controls.Add(this.pnlClient);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmTimer";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "系統提示";
		base.Load += new System.EventHandler(frmTimer_Load);
		this.pnlClient.ResumeLayout(false);
		this.pnlClientTopTitle.ResumeLayout(false);
		this.pnlClientTopTitle.PerformLayout();
		this.pnlClientClientShow.ResumeLayout(false);
		this.pnlClientClientShow.PerformLayout();
		base.ResumeLayout(false);
	}
}
