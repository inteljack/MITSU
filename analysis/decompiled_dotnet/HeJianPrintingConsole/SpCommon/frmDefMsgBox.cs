using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SpCommon;

public class frmDefMsgBox : Form
{
	public enum EnumPlayTipMusicIndex
	{
		iNormal,
		iWarm
	}

	private string m_StrTitle = "";

	private string m_StrContent = "";

	private int m_iMsgBtnIndex;

	private int m_iPlayTipMusicIndex;

	public static float m_fSingleCharWidthOnLabel;

	public static int m_iBtnWidth;

	public static int m_iBtnTop;

	public static int m_iSpaceBetween2Btn;

	private IContainer components;

	private Panel pnlClient;

	private Panel pnlClientClientContent;

	private Panel pnlClientBottomDockButton;

	private Label lblContent;

	public frmDefMsgBox(string strTitle, string strContent)
	{
		m_StrTitle = strTitle;
		m_StrContent = strContent;
		m_iMsgBtnIndex = 0;
		PlayMusic(m_iPlayTipMusicIndex);
		InitializeComponent();
	}

	public frmDefMsgBox(string strTitle, string strContent, MessageBoxButtons MsgBoxBtn)
	{
		m_StrTitle = strTitle;
		m_StrContent = strContent;
		m_iMsgBtnIndex = (int)MsgBoxBtn;
		PlayMusic(m_iPlayTipMusicIndex);
		InitializeComponent();
	}

	public frmDefMsgBox(string strTitle, string strContent, MessageBoxButtons MsgBoxBtn, int iPlayTipMusicIndex)
	{
		m_StrTitle = strTitle;
		m_StrContent = strContent;
		m_iPlayTipMusicIndex = iPlayTipMusicIndex;
		m_iMsgBtnIndex = (int)MsgBoxBtn;
		PlayMusic(m_iPlayTipMusicIndex);
		InitializeComponent();
	}

	public static DialogResult DefMsgBoxShow(string strTitle, string strContent, MessageBoxButtons MsgBoxBtn, int iPlayTipMusicIndex)
	{
		Button btnExample = new Button();
		m_iBtnWidth = btnExample.Width;
		m_iSpaceBetween2Btn = (m_iBtnWidth + 3) / 4;
		btnExample = null;
		frmDefMsgBox frmDMB = new frmDefMsgBox(strTitle, strContent, MsgBoxBtn, iPlayTipMusicIndex);
		int iBtnCount = GetBtnCount(MsgBoxBtn);
		int iBtnTotalWidth = iBtnCount * m_iBtnWidth + (iBtnCount + 1) * m_iSpaceBetween2Btn;
		int iContentTotalWidth = (int)(m_fSingleCharWidthOnLabel * (float)strContent.Length);
		int iTotalWidth = ((iBtnTotalWidth > iContentTotalWidth) ? iBtnTotalWidth : iContentTotalWidth);
		frmDMB.Width = iTotalWidth;
		return frmDMB.ShowDialog();
	}

	public static DialogResult DefMsgBoxShow(string strTitle, string strContent, MessageBoxButtons MsgBoxBtn)
	{
		Button btnExample = new Button();
		m_iBtnWidth = btnExample.Width;
		m_iSpaceBetween2Btn = (m_iBtnWidth + 3) / 4;
		btnExample = null;
		frmDefMsgBox frmDMB = new frmDefMsgBox(strTitle, strContent, MsgBoxBtn);
		int iBtnCount = GetBtnCount(MsgBoxBtn);
		int iBtnTotalWidth = iBtnCount * m_iBtnWidth + (iBtnCount + 1) * m_iSpaceBetween2Btn;
		int iContentTotalWidth = (int)(m_fSingleCharWidthOnLabel * (float)strContent.Length);
		int iTotalWidth = ((iBtnTotalWidth > iContentTotalWidth) ? iBtnTotalWidth : iContentTotalWidth);
		frmDMB.Width = iTotalWidth;
		return frmDMB.ShowDialog();
	}

	public static DialogResult DefMsgBoxShow(string strTitle, string strContent)
	{
		Button btnExample = new Button();
		m_iBtnWidth = btnExample.Width;
		m_iSpaceBetween2Btn = (m_iBtnWidth + 3) / 4;
		btnExample = null;
		frmDefMsgBox frmDMB = new frmDefMsgBox(strTitle, strContent);
		int iBtnCount = 1;
		int iBtnTotalWidth = iBtnCount * m_iBtnWidth + (iBtnCount + 1) * m_iSpaceBetween2Btn;
		int iContentTotalWidth = (int)(m_fSingleCharWidthOnLabel * (float)strContent.Length);
		int iTotalWidth = ((iBtnTotalWidth > iContentTotalWidth) ? iBtnTotalWidth : iContentTotalWidth);
		frmDMB.Width = iTotalWidth;
		return frmDMB.ShowDialog();
	}

	public static DialogResult DefMsgBoxShow(string strContent)
	{
		Button btnExample = new Button();
		m_iBtnWidth = btnExample.Width;
		m_iSpaceBetween2Btn = (m_iBtnWidth + 3) / 4;
		btnExample = null;
		frmDefMsgBox frmDMB = new frmDefMsgBox("   ", strContent);
		int iBtnCount = 1;
		int iBtnTotalWidth = iBtnCount * m_iBtnWidth + (iBtnCount + 1) * m_iSpaceBetween2Btn;
		int iContentTotalWidth = (int)(m_fSingleCharWidthOnLabel * (float)strContent.Length);
		int iTotalWidth = ((iBtnTotalWidth > iContentTotalWidth) ? iBtnTotalWidth : iContentTotalWidth);
		frmDMB.Width = iTotalWidth;
		return frmDMB.ShowDialog();
	}

	private static int GetBtnCount(MessageBoxButtons MsgBoxBtn)
	{
		int iBtnCount = 1;
		switch (MsgBoxBtn)
		{
		case MessageBoxButtons.OK:
			iBtnCount = 1;
			break;
		case MessageBoxButtons.OKCancel:
			iBtnCount = 2;
			break;
		case MessageBoxButtons.AbortRetryIgnore:
			iBtnCount = 3;
			break;
		case MessageBoxButtons.RetryCancel:
			iBtnCount = 2;
			break;
		case MessageBoxButtons.YesNo:
			iBtnCount = 2;
			break;
		case MessageBoxButtons.YesNoCancel:
			iBtnCount = 3;
			break;
		}
		return iBtnCount;
	}

	private void frmDefMsgBox_Load(object sender, EventArgs e)
	{
		base.ControlBox = false;
		Text = m_StrTitle;
		IniDefParam();
	}

	private void frmDefMsgBox_Shown(object sender, EventArgs e)
	{
	}

	private void IniDefParam()
	{
		ShowButton(m_iMsgBtnIndex);
		ResizeLblMsgInfo(m_StrContent);
	}

	private void PlayMusic(int iPlayTipMusicIndex)
	{
		switch (iPlayTipMusicIndex)
		{
		case 0:
			ClsMusic.PlayMusic_Normal();
			break;
		case 1:
			ClsMusic.PlayMusic_Warm();
			break;
		}
	}

	private void ResizeLblMsgInfo(string strContent)
	{
		lblContent.Text = strContent;
		lblContent.Left = (pnlClientClientContent.Width - lblContent.Width) / 2;
		lblContent.Top = (pnlClientClientContent.Height - lblContent.Height) / 2;
	}

	private void ShowButton(int iMsgBtnIndex)
	{
		Button btnExample = new Button();
		m_iBtnTop = (pnlClientBottomDockButton.Height - btnExample.Height) / 2;
		btnExample = null;
		switch (iMsgBtnIndex)
		{
		case 0:
		{
			int iBtnOKLeft = (pnlClientBottomDockButton.Width - m_iBtnWidth + 1) / 2;
			CreateBtnOK(iBtnOKLeft, m_iBtnTop);
			break;
		}
		case 1:
			CreateBtnOKCancel();
			break;
		case 2:
			CreateBtnAbortRetryIgnore();
			break;
		case 3:
			CreateBtnYesNoCancel();
			break;
		case 4:
			CreateBtnYesNo();
			break;
		case 5:
			CreateBtnRetryCancel();
			break;
		}
	}

	private void CreateBtnOK(int iLeft, int iTop)
	{
		Button btnOK = new Button();
		btnOK.Text = ClsButtonText.strBtnOK;
		btnOK.Click += btnOK_Click;
		btnOK.Parent = pnlClientBottomDockButton;
		btnOK.Left = iLeft;
		btnOK.Top = iTop;
	}

	private void CreateBtnCancel(int iLeft, int iTop)
	{
		Button btnCancel = new Button();
		btnCancel.Text = ClsButtonText.strBtnCancel;
		btnCancel.Click += btnCancel_Click;
		btnCancel.Parent = pnlClientBottomDockButton;
		btnCancel.Left = iLeft;
		btnCancel.Top = iTop;
	}

	private void CreateBtnAbort(int iLeft, int iTop)
	{
		Button btnAbort = new Button();
		btnAbort.Text = ClsButtonText.strBtnAbort;
		btnAbort.Click += btnAbort_Click;
		btnAbort.Parent = pnlClientBottomDockButton;
		btnAbort.Left = iLeft;
		btnAbort.Top = iTop;
	}

	private void CreateBtnRetry(int iLeft, int iTop)
	{
		Button btnRetry = new Button();
		btnRetry.Text = ClsButtonText.strBtnRetry;
		btnRetry.Click += btnRetry_Click;
		btnRetry.Parent = pnlClientBottomDockButton;
		btnRetry.Left = iLeft;
		btnRetry.Top = iTop;
	}

	private void CreateBtnIgnore(int iLeft, int iTop)
	{
		Button btnIgnore = new Button();
		btnIgnore.Text = ClsButtonText.strBtnIgnore;
		btnIgnore.Click += btnIgnore_Click;
		btnIgnore.Parent = pnlClientBottomDockButton;
		btnIgnore.Left = iLeft;
		btnIgnore.Top = iTop;
	}

	private void CreateBtnYes(int iLeft, int iTop)
	{
		Button btnYes = new Button();
		btnYes.Text = ClsButtonText.strBtnYes;
		btnYes.Click += btnYes_Click;
		btnYes.Parent = pnlClientBottomDockButton;
		btnYes.Left = iLeft;
		btnYes.Top = iTop;
	}

	private void CreateBtnNo(int iLeft, int iTop)
	{
		Button btnNo = new Button();
		btnNo.Text = ClsButtonText.strBtnNo;
		btnNo.Click += btnNo_Click;
		btnNo.Parent = pnlClientBottomDockButton;
		btnNo.Left = iLeft;
		btnNo.Top = iTop;
	}

	private void CreateBtnOKCancel()
	{
		int iBtnOKLeft = (pnlClientBottomDockButton.Width - (m_iBtnWidth * 2 + m_iSpaceBetween2Btn) + 1) / 2;
		CreateBtnOK(iBtnOKLeft, m_iBtnTop);
		int iBtnCancelLeft = iBtnOKLeft + m_iBtnWidth + m_iSpaceBetween2Btn;
		CreateBtnCancel(iBtnCancelLeft, m_iBtnTop);
	}

	private void CreateBtnAbortRetryIgnore()
	{
		int iBtnAbortLeft = (pnlClientBottomDockButton.Width - (m_iBtnWidth * 3 + m_iSpaceBetween2Btn * 2)) / 2;
		CreateBtnOK(iBtnAbortLeft, m_iBtnTop);
		int iBtnRetryLeft = iBtnAbortLeft + m_iBtnWidth + m_iSpaceBetween2Btn;
		CreateBtnRetry(iBtnRetryLeft, m_iBtnTop);
		int iBtnIgnoreLeft = iBtnRetryLeft + m_iBtnWidth + m_iSpaceBetween2Btn;
		CreateBtnIgnore(iBtnIgnoreLeft, m_iBtnTop);
	}

	private void CreateBtnYesNoCancel()
	{
		int iBtnYesLeft = (pnlClientBottomDockButton.Width - (m_iBtnWidth * 3 + m_iSpaceBetween2Btn * 2)) / 2;
		CreateBtnYes(iBtnYesLeft, m_iBtnTop);
		int iBtnNoLeft = iBtnYesLeft + m_iBtnWidth + m_iSpaceBetween2Btn;
		CreateBtnNo(iBtnNoLeft, m_iBtnTop);
		int iBtnCancelLeft = iBtnNoLeft + m_iBtnWidth + m_iSpaceBetween2Btn;
		CreateBtnCancel(iBtnCancelLeft, m_iBtnTop);
	}

	private void CreateBtnYesNo()
	{
		int iBtnYesLeft = (pnlClientBottomDockButton.Width - (m_iBtnWidth * 2 + m_iSpaceBetween2Btn) + 1) / 2;
		CreateBtnYes(iBtnYesLeft, m_iBtnTop);
		int iBtnNoLeft = iBtnYesLeft + m_iBtnWidth + m_iSpaceBetween2Btn;
		CreateBtnNo(iBtnNoLeft, m_iBtnTop);
	}

	private void CreateBtnRetryCancel()
	{
		int iBtnRetryLeft = (pnlClientBottomDockButton.Width - (m_iBtnWidth * 2 + m_iSpaceBetween2Btn) + 1) / 2;
		CreateBtnOK(iBtnRetryLeft, m_iBtnTop);
		int iBtnCancelLeft = iBtnRetryLeft + m_iBtnWidth + m_iSpaceBetween2Btn;
		CreateBtnCancel(iBtnCancelLeft, m_iBtnTop);
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void btnAbort_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Abort;
		Close();
	}

	private void btnRetry_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Retry;
		Close();
	}

	private void btnIgnore_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Ignore;
		Close();
	}

	private void btnYes_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Yes;
		Close();
	}

	private void btnNo_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.No;
		Close();
	}

	private void frmDefMsgBox_FormClosing(object sender, FormClosingEventArgs e)
	{
		ClsMusic.StopMusic();
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
		this.pnlClient = new System.Windows.Forms.Panel();
		this.pnlClientClientContent = new System.Windows.Forms.Panel();
		this.lblContent = new System.Windows.Forms.Label();
		this.pnlClientBottomDockButton = new System.Windows.Forms.Panel();
		this.pnlClient.SuspendLayout();
		this.pnlClientClientContent.SuspendLayout();
		base.SuspendLayout();
		this.pnlClient.Controls.Add(this.pnlClientClientContent);
		this.pnlClient.Controls.Add(this.pnlClientBottomDockButton);
		this.pnlClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClient.Location = new System.Drawing.Point(0, 0);
		this.pnlClient.Name = "pnlClient";
		this.pnlClient.Size = new System.Drawing.Size(179, 62);
		this.pnlClient.TabIndex = 0;
		this.pnlClientClientContent.Controls.Add(this.lblContent);
		this.pnlClientClientContent.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientClientContent.Location = new System.Drawing.Point(0, 0);
		this.pnlClientClientContent.Name = "pnlClientClientContent";
		this.pnlClientClientContent.Size = new System.Drawing.Size(179, 29);
		this.pnlClientClientContent.TabIndex = 3;
		this.lblContent.AutoSize = true;
		this.lblContent.BackColor = System.Drawing.Color.Transparent;
		this.lblContent.Location = new System.Drawing.Point(54, 8);
		this.lblContent.Name = "lblContent";
		this.lblContent.Size = new System.Drawing.Size(65, 12);
		this.lblContent.TabIndex = 0;
		this.lblContent.Text = "lblContent";
		this.pnlClientBottomDockButton.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.pnlClientBottomDockButton.Location = new System.Drawing.Point(0, 29);
		this.pnlClientBottomDockButton.Name = "pnlClientBottomDockButton";
		this.pnlClientBottomDockButton.Size = new System.Drawing.Size(179, 33);
		this.pnlClientBottomDockButton.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.ClientSize = new System.Drawing.Size(179, 62);
		base.Controls.Add(this.pnlClient);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Name = "frmDefMsgBox";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmDefMsgBox_FormClosing);
		base.Load += new System.EventHandler(frmDefMsgBox_Load);
		base.Shown += new System.EventHandler(frmDefMsgBox_Shown);
		this.pnlClient.ResumeLayout(false);
		this.pnlClientClientContent.ResumeLayout(false);
		this.pnlClientClientContent.PerformLayout();
		base.ResumeLayout(false);
	}
}
