using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HeJianPrintingConsole;

public class frmBase : Form
{
	private IContainer components;

	private Panel pnlAllClient;

	public Panel pnlClientData;

	public Panel pnlBottomCtrl;

	public frmBase()
	{
		InitializeComponent();
	}

	private void frmBase_Load(object sender, EventArgs e)
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
		this.pnlAllClient = new System.Windows.Forms.Panel();
		this.pnlClientData = new System.Windows.Forms.Panel();
		this.pnlBottomCtrl = new System.Windows.Forms.Panel();
		this.pnlAllClient.SuspendLayout();
		base.SuspendLayout();
		this.pnlAllClient.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlAllClient.Controls.Add(this.pnlClientData);
		this.pnlAllClient.Controls.Add(this.pnlBottomCtrl);
		this.pnlAllClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlAllClient.Location = new System.Drawing.Point(0, 0);
		this.pnlAllClient.Name = "pnlAllClient";
		this.pnlAllClient.Size = new System.Drawing.Size(919, 449);
		this.pnlAllClient.TabIndex = 0;
		this.pnlClientData.BackColor = System.Drawing.Color.Transparent;
		this.pnlClientData.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientData.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientData.Location = new System.Drawing.Point(0, 0);
		this.pnlClientData.Name = "pnlClientData";
		this.pnlClientData.Size = new System.Drawing.Size(915, 330);
		this.pnlClientData.TabIndex = 4;
		this.pnlBottomCtrl.BackColor = System.Drawing.Color.Transparent;
		this.pnlBottomCtrl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlBottomCtrl.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.pnlBottomCtrl.Location = new System.Drawing.Point(0, 330);
		this.pnlBottomCtrl.Name = "pnlBottomCtrl";
		this.pnlBottomCtrl.Size = new System.Drawing.Size(915, 115);
		this.pnlBottomCtrl.TabIndex = 3;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.ClientSize = new System.Drawing.Size(919, 449);
		base.Controls.Add(this.pnlAllClient);
		base.Name = "frmBase";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "frmBase";
		base.Load += new System.EventHandler(frmBase_Load);
		this.pnlAllClient.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
