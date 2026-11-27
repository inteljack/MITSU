using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmShowPrintStatus : Form
{
	private IContainer components;

	private Panel panelSpliter;

	private Panel panelTop;

	private Panel panelBottom;

	private Panel panelBottomClient;

	private TextBox textBoxShowPrintStatusText;

	private Panel panelBottomLeft;

	private TextBox textBoxShowPrintStatusLed;

	private TextBox textBoxAllPrinBoxtCountValue;

	private TextBox textBoxHasPrintBoxCountValue;

	private Label label3;

	private Label label2;

	private Label label1;

	private Label label4;

	public frmShowPrintStatus(frmPrintCtrl frmPC)
	{
		InitializeComponent();
		IniFormParam();
		frmPC.ShowPrintStatusEvent += ShowPrintStatusEventDef;
	}

	private void IniFormParam()
	{
		base.Left = ClsXml.GetShowPrintStatusFormX();
		base.Top = ClsXml.GetShowPrintStatusFormY();
		base.Width = ClsXml.GetShowPrintStatusFormWidth();
		base.Height = ClsXml.GetShowPrintStatusFormHeight();
	}

	private void ShowPrintStatusEventDef(object sender, ClsShowPrintStatusEventArgs e)
	{
		try
		{
			if (base.Visible != e.bFormVisible)
			{
				base.Visible = e.bFormVisible;
				if (base.Visible)
				{
					Activate();
				}
			}
			textBoxHasPrintBoxCountValue.Text = e.strHasPrintBoxCount;
			textBoxAllPrinBoxtCountValue.Text = e.strAllPrintBoxCount;
			textBoxShowPrintStatusLed.BackColor = e.printStatusLedColor;
			textBoxShowPrintStatusText.Text = e.printStatusText;
		}
		catch
		{
		}
	}

	private void frmShowPrintStatus_Load(object sender, EventArgs e)
	{
		panelSpliter.Focus();
	}

	private void frmShowPrintStatus_FormClosing(object sender, FormClosingEventArgs e)
	{
		SaveFormParam();
	}

	private void SaveFormParam()
	{
		try
		{
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strShowPrintStatusFormX, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, base.Left.ToString());
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strShowPrintStatusFormY, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, base.Top.ToString());
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strShowPrintStatusFormWidth, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, base.Width.ToString());
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strShowPrintStatusFormHeight, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, base.Height.ToString());
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SaveShowPrintStatusFormParam exception:" + ex.Message);
		}
	}

	private void textBoxAllPrinBoxtCountValue_MouseDown(object sender, MouseEventArgs e)
	{
		panelSpliter.Focus();
	}

	private void frmShowPrintStatus_Paint(object sender, PaintEventArgs e)
	{
		panelSpliter.Focus();
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
		this.panelSpliter = new System.Windows.Forms.Panel();
		this.panelTop = new System.Windows.Forms.Panel();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.textBoxAllPrinBoxtCountValue = new System.Windows.Forms.TextBox();
		this.textBoxHasPrintBoxCountValue = new System.Windows.Forms.TextBox();
		this.panelBottom = new System.Windows.Forms.Panel();
		this.panelBottomClient = new System.Windows.Forms.Panel();
		this.textBoxShowPrintStatusText = new System.Windows.Forms.TextBox();
		this.panelBottomLeft = new System.Windows.Forms.Panel();
		this.textBoxShowPrintStatusLed = new System.Windows.Forms.TextBox();
		this.panelTop.SuspendLayout();
		this.panelBottom.SuspendLayout();
		this.panelBottomClient.SuspendLayout();
		this.panelBottomLeft.SuspendLayout();
		base.SuspendLayout();
		this.panelSpliter.BackColor = System.Drawing.Color.DodgerBlue;
		this.panelSpliter.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelSpliter.Location = new System.Drawing.Point(0, 0);
		this.panelSpliter.Name = "panelSpliter";
		this.panelSpliter.Size = new System.Drawing.Size(1033, 545);
		this.panelSpliter.TabIndex = 12;
		this.panelTop.Controls.Add(this.label3);
		this.panelTop.Controls.Add(this.label4);
		this.panelTop.Controls.Add(this.label2);
		this.panelTop.Controls.Add(this.label1);
		this.panelTop.Controls.Add(this.textBoxAllPrinBoxtCountValue);
		this.panelTop.Controls.Add(this.textBoxHasPrintBoxCountValue);
		this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTop.Location = new System.Drawing.Point(0, 0);
		this.panelTop.Name = "panelTop";
		this.panelTop.Size = new System.Drawing.Size(1033, 253);
		this.panelTop.TabIndex = 13;
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 72f, System.Drawing.FontStyle.Bold);
		this.label3.Location = new System.Drawing.Point(292, 134);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(170, 108);
		this.label3.TabIndex = 23;
		this.label3.Text = "數:";
		this.label4.AutoSize = true;
		this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 72f, System.Drawing.FontStyle.Bold);
		this.label4.Location = new System.Drawing.Point(11, 9);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(461, 108);
		this.label4.TabIndex = 22;
		this.label4.Text = "噴印箱數:";
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 72f, System.Drawing.FontStyle.Bold);
		this.label2.Location = new System.Drawing.Point(143, 134);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(142, 108);
		this.label2.TabIndex = 22;
		this.label2.Text = "箱";
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 72f, System.Drawing.FontStyle.Bold);
		this.label1.Location = new System.Drawing.Point(8, 134);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(142, 108);
		this.label1.TabIndex = 21;
		this.label1.Text = "總";
		this.textBoxAllPrinBoxtCountValue.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxAllPrinBoxtCountValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBoxAllPrinBoxtCountValue.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.textBoxAllPrinBoxtCountValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 72f, System.Drawing.FontStyle.Bold);
		this.textBoxAllPrinBoxtCountValue.ForeColor = System.Drawing.Color.Black;
		this.textBoxAllPrinBoxtCountValue.Location = new System.Drawing.Point(476, 135);
		this.textBoxAllPrinBoxtCountValue.Name = "textBoxAllPrinBoxtCountValue";
		this.textBoxAllPrinBoxtCountValue.ReadOnly = true;
		this.textBoxAllPrinBoxtCountValue.Size = new System.Drawing.Size(528, 109);
		this.textBoxAllPrinBoxtCountValue.TabIndex = 20;
		this.textBoxAllPrinBoxtCountValue.Text = "0";
		this.textBoxAllPrinBoxtCountValue.MouseDown += new System.Windows.Forms.MouseEventHandler(textBoxAllPrinBoxtCountValue_MouseDown);
		this.textBoxHasPrintBoxCountValue.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxHasPrintBoxCountValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBoxHasPrintBoxCountValue.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.textBoxHasPrintBoxCountValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 72f, System.Drawing.FontStyle.Bold);
		this.textBoxHasPrintBoxCountValue.ForeColor = System.Drawing.Color.Black;
		this.textBoxHasPrintBoxCountValue.Location = new System.Drawing.Point(476, 9);
		this.textBoxHasPrintBoxCountValue.Name = "textBoxHasPrintBoxCountValue";
		this.textBoxHasPrintBoxCountValue.ReadOnly = true;
		this.textBoxHasPrintBoxCountValue.Size = new System.Drawing.Size(528, 109);
		this.textBoxHasPrintBoxCountValue.TabIndex = 19;
		this.textBoxHasPrintBoxCountValue.Text = "0";
		this.textBoxHasPrintBoxCountValue.MouseDown += new System.Windows.Forms.MouseEventHandler(textBoxAllPrinBoxtCountValue_MouseDown);
		this.panelBottom.Controls.Add(this.panelBottomClient);
		this.panelBottom.Controls.Add(this.panelBottomLeft);
		this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panelBottom.Location = new System.Drawing.Point(0, 275);
		this.panelBottom.Name = "panelBottom";
		this.panelBottom.Size = new System.Drawing.Size(1033, 270);
		this.panelBottom.TabIndex = 14;
		this.panelBottomClient.Controls.Add(this.textBoxShowPrintStatusText);
		this.panelBottomClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelBottomClient.Location = new System.Drawing.Point(200, 0);
		this.panelBottomClient.Name = "panelBottomClient";
		this.panelBottomClient.Size = new System.Drawing.Size(833, 270);
		this.panelBottomClient.TabIndex = 14;
		this.textBoxShowPrintStatusText.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxShowPrintStatusText.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBoxShowPrintStatusText.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.textBoxShowPrintStatusText.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textBoxShowPrintStatusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 72f, System.Drawing.FontStyle.Bold);
		this.textBoxShowPrintStatusText.ForeColor = System.Drawing.Color.Black;
		this.textBoxShowPrintStatusText.Location = new System.Drawing.Point(0, 0);
		this.textBoxShowPrintStatusText.Multiline = true;
		this.textBoxShowPrintStatusText.Name = "textBoxShowPrintStatusText";
		this.textBoxShowPrintStatusText.ReadOnly = true;
		this.textBoxShowPrintStatusText.Size = new System.Drawing.Size(833, 270);
		this.textBoxShowPrintStatusText.TabIndex = 13;
		this.textBoxShowPrintStatusText.Text = "獲取PrintingPro狀態...";
		this.textBoxShowPrintStatusText.MouseDown += new System.Windows.Forms.MouseEventHandler(textBoxAllPrinBoxtCountValue_MouseDown);
		this.panelBottomLeft.Controls.Add(this.textBoxShowPrintStatusLed);
		this.panelBottomLeft.Dock = System.Windows.Forms.DockStyle.Left;
		this.panelBottomLeft.Location = new System.Drawing.Point(0, 0);
		this.panelBottomLeft.Name = "panelBottomLeft";
		this.panelBottomLeft.Size = new System.Drawing.Size(200, 270);
		this.panelBottomLeft.TabIndex = 13;
		this.textBoxShowPrintStatusLed.BackColor = System.Drawing.Color.Red;
		this.textBoxShowPrintStatusLed.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBoxShowPrintStatusLed.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.textBoxShowPrintStatusLed.Font = new System.Drawing.Font("Microsoft Sans Serif", 72f, System.Drawing.FontStyle.Bold);
		this.textBoxShowPrintStatusLed.ForeColor = System.Drawing.Color.Black;
		this.textBoxShowPrintStatusLed.Location = new System.Drawing.Point(10, 13);
		this.textBoxShowPrintStatusLed.Multiline = true;
		this.textBoxShowPrintStatusLed.Name = "textBoxShowPrintStatusLed";
		this.textBoxShowPrintStatusLed.ReadOnly = true;
		this.textBoxShowPrintStatusLed.Size = new System.Drawing.Size(181, 244);
		this.textBoxShowPrintStatusLed.TabIndex = 14;
		this.textBoxShowPrintStatusLed.MouseDown += new System.Windows.Forms.MouseEventHandler(textBoxAllPrinBoxtCountValue_MouseDown);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.ClientSize = new System.Drawing.Size(1033, 545);
		base.ControlBox = false;
		base.Controls.Add(this.panelBottom);
		base.Controls.Add(this.panelTop);
		base.Controls.Add(this.panelSpliter);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmShowPrintStatus";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "噴印狀態";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmShowPrintStatus_FormClosing);
		base.Load += new System.EventHandler(frmShowPrintStatus_Load);
		base.Paint += new System.Windows.Forms.PaintEventHandler(frmShowPrintStatus_Paint);
		this.panelTop.ResumeLayout(false);
		this.panelTop.PerformLayout();
		this.panelBottom.ResumeLayout(false);
		this.panelBottomClient.ResumeLayout(false);
		this.panelBottomClient.PerformLayout();
		this.panelBottomLeft.ResumeLayout(false);
		this.panelBottomLeft.PerformLayout();
		base.ResumeLayout(false);
	}
}
