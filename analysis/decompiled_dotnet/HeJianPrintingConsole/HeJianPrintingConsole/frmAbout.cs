using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace HeJianPrintingConsole;

public class frmAbout : Form
{
	private IContainer components;

	private Button btnOK;

	private Label lblImportPath;

	private Label label3;

	private Label lblProductName;

	private Label lblSoftwareVersion;

	private Label label2;

	private Label lblCopyright;

	private PictureBox picBoxAppIcon;

	public frmAbout()
	{
		InitializeComponent();
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void frmAbout_Load(object sender, EventArgs e)
	{
		FileVersionInfo FVI = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
		lblProductName.Text = FVI.ProductName;
		lblSoftwareVersion.Text = FVI.ProductVersion;
		lblCopyright.Text = FVI.LegalCopyright;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeJianPrintingConsole.frmAbout));
		this.btnOK = new System.Windows.Forms.Button();
		this.lblImportPath = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.lblProductName = new System.Windows.Forms.Label();
		this.lblSoftwareVersion = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.lblCopyright = new System.Windows.Forms.Label();
		this.picBoxAppIcon = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.picBoxAppIcon).BeginInit();
		base.SuspendLayout();
		this.btnOK.Location = new System.Drawing.Point(156, 216);
		this.btnOK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(147, 49);
		this.btnOK.TabIndex = 0;
		this.btnOK.Text = "確定";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.lblImportPath.AutoSize = true;
		this.lblImportPath.BackColor = System.Drawing.Color.Transparent;
		this.lblImportPath.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblImportPath.Location = new System.Drawing.Point(66, 104);
		this.lblImportPath.Name = "lblImportPath";
		this.lblImportPath.Size = new System.Drawing.Size(109, 19);
		this.lblImportPath.TabIndex = 26;
		this.lblImportPath.Text = "軟體版本：";
		this.label3.AutoSize = true;
		this.label3.BackColor = System.Drawing.Color.Transparent;
		this.label3.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label3.Location = new System.Drawing.Point(66, 40);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(109, 19);
		this.label3.TabIndex = 29;
		this.label3.Text = "產品名稱：";
		this.lblProductName.AutoSize = true;
		this.lblProductName.BackColor = System.Drawing.Color.Transparent;
		this.lblProductName.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblProductName.Location = new System.Drawing.Point(197, 40);
		this.lblProductName.Name = "lblProductName";
		this.lblProductName.Size = new System.Drawing.Size(123, 19);
		this.lblProductName.TabIndex = 32;
		this.lblProductName.Text = "具體產品名稱";
		this.lblSoftwareVersion.AutoSize = true;
		this.lblSoftwareVersion.BackColor = System.Drawing.Color.Transparent;
		this.lblSoftwareVersion.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSoftwareVersion.Location = new System.Drawing.Point(197, 104);
		this.lblSoftwareVersion.Name = "lblSoftwareVersion";
		this.lblSoftwareVersion.Size = new System.Drawing.Size(123, 19);
		this.lblSoftwareVersion.TabIndex = 30;
		this.lblSoftwareVersion.Text = "具體軟體版本";
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.Transparent;
		this.label2.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label2.Location = new System.Drawing.Point(66, 168);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(109, 19);
		this.label2.TabIndex = 33;
		this.label2.Text = "版權聲明：";
		this.lblCopyright.AutoSize = true;
		this.lblCopyright.BackColor = System.Drawing.Color.Transparent;
		this.lblCopyright.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblCopyright.Location = new System.Drawing.Point(197, 168);
		this.lblCopyright.Name = "lblCopyright";
		this.lblCopyright.Size = new System.Drawing.Size(123, 19);
		this.lblCopyright.TabIndex = 34;
		this.lblCopyright.Text = "具體版權聲明";
		this.picBoxAppIcon.Image = (System.Drawing.Image)resources.GetObject("picBoxAppIcon.Image");
		this.picBoxAppIcon.Location = new System.Drawing.Point(14, 36);
		this.picBoxAppIcon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.picBoxAppIcon.Name = "picBoxAppIcon";
		this.picBoxAppIcon.Size = new System.Drawing.Size(41, 42);
		this.picBoxAppIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.picBoxAppIcon.TabIndex = 35;
		this.picBoxAppIcon.TabStop = false;
		this.picBoxAppIcon.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 14f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.ClientSize = new System.Drawing.Size(485, 294);
		base.Controls.Add(this.picBoxAppIcon);
		base.Controls.Add(this.lblCopyright);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.lblProductName);
		base.Controls.Add(this.lblSoftwareVersion);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.lblImportPath);
		base.Controls.Add(this.btnOK);
		this.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmAbout";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "關於";
		base.Load += new System.EventHandler(frmAbout_Load);
		((System.ComponentModel.ISupportInitialize)this.picBoxAppIcon).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
