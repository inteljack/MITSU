using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmLogin : Form
{
	private IContainer components;

	private TextBox textBoxPwd;

	private Label lblKeyCode;

	private Button btnOK;

	private Button btnCancel;

	public frmLogin()
	{
		InitializeComponent();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		if (textBoxPwd.Text == ClsXml.GetSysPwd())
		{
			base.DialogResult = DialogResult.Yes;
		}
		else
		{
			base.DialogResult = DialogResult.No;
		}
		Close();
	}

	private void textBoxPwd_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			btnOK_Click(this, EventArgs.Empty);
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
		this.textBoxPwd = new System.Windows.Forms.TextBox();
		this.lblKeyCode = new System.Windows.Forms.Label();
		this.btnOK = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.textBoxPwd.Location = new System.Drawing.Point(56, 51);
		this.textBoxPwd.MaxLength = 50;
		this.textBoxPwd.Name = "textBoxPwd";
		this.textBoxPwd.PasswordChar = '*';
		this.textBoxPwd.Size = new System.Drawing.Size(218, 21);
		this.textBoxPwd.TabIndex = 3;
		this.textBoxPwd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPwd_KeyPress);
		this.lblKeyCode.AutoSize = true;
		this.lblKeyCode.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblKeyCode.Location = new System.Drawing.Point(12, 54);
		this.lblKeyCode.Name = "lblKeyCode";
		this.lblKeyCode.Size = new System.Drawing.Size(45, 14);
		this.lblKeyCode.TabIndex = 2;
		this.lblKeyCode.Text = "密碼:";
		this.btnOK.Location = new System.Drawing.Point(26, 108);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(81, 32);
		this.btnOK.TabIndex = 4;
		this.btnOK.Text = "確定";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.btnCancel.Location = new System.Drawing.Point(193, 108);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(81, 32);
		this.btnCancel.TabIndex = 5;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.ClientSize = new System.Drawing.Size(295, 163);
		base.ControlBox = false;
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.textBoxPwd);
		base.Controls.Add(this.lblKeyCode);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmLogin";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "登錄";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
