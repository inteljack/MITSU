using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmSuperSet : frmBase
{
	private IContainer components;

	private TabControl tabCtrlSuperSet;

	private TabPage tabPageDefaultSet;

	private TabPage tabPagePHDelaySet;

	private CheckBox checkBoxResumeDefaultPWD;

	private Button btnOK;

	private Label label3;

	private TextBox textBoxPH4Delay2;

	private Label label4;

	private TextBox textBoxPH3Delay2;

	private Label label5;

	private TextBox textBoxPH2Delay2;

	private Label label6;

	private TextBox textBoxPH1Delay2;

	private Label label2;

	private Label label1;

	private Label label45;

	private TextBox textBoxPH4Delay1;

	private Label label46;

	private Label label37;

	private TextBox textBoxPH3Delay1;

	private Label label38;

	private Label label29;

	private TextBox textBoxPH2Delay1;

	private Label label30;

	private Label label16;

	private TextBox textBoxPH1Delay1;

	private Label label15;

	private TabPage tabPageResolutionSet;

	private TextBox textBoxWidthResolution;

	private Label label11;

	private TabPage tabPagePrintMaxMem;

	private TextBox textBoxPrintMaxMem;

	private Label label7;

	private TabPage tabPageDataBuffer;

	private Label label8;

	private Label label9;

	private TextBox textBoxDataBufferTime;

	public frmSuperSet()
	{
		InitializeComponent();
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		if (tabCtrlSuperSet.Controls["tabPageDefaultSet"].Visible)
		{
			SetDefault();
		}
		else if (tabCtrlSuperSet.Controls["tabPagePHDelaySet"].Visible)
		{
			SetFixedDelay();
		}
		else if (tabCtrlSuperSet.Controls["tabPageResolutionSet"].Visible)
		{
			SetResolution();
		}
		else if (tabCtrlSuperSet.Controls["tabPagePrintMaxMem"].Visible)
		{
			SetPrintMaxMem();
		}
		else if (tabCtrlSuperSet.Controls["tabPageDataBuffer"].Visible)
		{
			SetDataBuffer();
		}
	}

	private void SetDataBuffer()
	{
		try
		{
			string dataBufferTime = textBoxDataBufferTime.Text;
			if (dataBufferTime.Length <= 0)
			{
				MessageBox.Show("數據緩衝時間不可為空");
			}
			else if (Convert.ToInt32(dataBufferTime) < 0)
			{
				MessageBox.Show("最大緩存必須大於等於零");
			}
			else if (ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strDataBufferTime_s, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, dataBufferTime))
			{
				MessageBox.Show("設置成功");
			}
			else
			{
				MessageBox.Show("設置失敗");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SetDataBuffer exception:" + ex.Message);
			MessageBox.Show(ex.Message);
		}
	}

	private void SetDefault()
	{
		if (DialogResult.Yes == MessageBox.Show("確定要恢復默認密碼嗎?", "系統提示", MessageBoxButtons.YesNo))
		{
			if (ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSystemPwd, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, "<REDACTED-default-password>"))
			{
				MessageBox.Show("恢復默認密碼成功");
			}
			else
			{
				MessageBox.Show("恢復默認密碼失敗");
			}
		}
	}

	private void SetFixedDelay()
	{
		string strPH1Delay1 = textBoxPH1Delay1.Text;
		string strPH1Delay2 = textBoxPH1Delay2.Text;
		string strPH2Delay1 = textBoxPH2Delay1.Text;
		string strPH2Delay2 = textBoxPH2Delay2.Text;
		string strPH3Delay1 = textBoxPH3Delay1.Text;
		string strPH3Delay2 = textBoxPH3Delay2.Text;
		string strPH4Delay1 = textBoxPH4Delay1.Text;
		string strPH4Delay2 = textBoxPH4Delay2.Text;
		if (strPH1Delay1.Length <= 0 || strPH1Delay2.Length <= 0 || strPH2Delay1.Length <= 0 || strPH2Delay2.Length <= 0 || strPH3Delay1.Length <= 0 || strPH3Delay2.Length <= 0 || strPH4Delay1.Length <= 0 || strPH4Delay2.Length <= 0)
		{
			MessageBox.Show("延遲不可為空!");
			return;
		}
		bool bRlt11 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintHead1FixedDelay1, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strPH1Delay1);
		bool bRlt12 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintHead1FixedDelay2, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strPH1Delay2);
		bool bRlt21 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintHead2FixedDelay1, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strPH2Delay1);
		bool bRlt22 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintHead2FixedDelay2, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strPH2Delay2);
		bool bRlt31 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintHead3FixedDelay1, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strPH3Delay1);
		bool bRlt32 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintHead3FixedDelay2, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strPH3Delay2);
		bool bRlt41 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintHead4FixedDelay1, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strPH4Delay1);
		bool bRlt42 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintHead4FixedDelay2, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strPH4Delay2);
		if (bRlt11 && bRlt12 && bRlt21 && bRlt22 && bRlt31 && bRlt32 && bRlt41 && bRlt42)
		{
			MessageBox.Show("設置成功");
		}
		else
		{
			MessageBox.Show("設置失敗");
		}
	}

	private void SetResolution()
	{
		try
		{
			string strWidthResolution = textBoxWidthResolution.Text;
			if (strWidthResolution.Length <= 0)
			{
				MessageBox.Show("分辨率不可為空");
			}
			else if (Convert.ToInt32(strWidthResolution) == 0)
			{
				MessageBox.Show("分辨率必須大於零");
			}
			else if (Convert.ToInt32(strWidthResolution) > 450)
			{
				MessageBox.Show("分辨率必須在450以內");
			}
			else if (ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strWidthDPI, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strWidthResolution))
			{
				MessageBox.Show("設置成功");
			}
			else
			{
				MessageBox.Show("設置失敗");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SetResolution exception:" + ex.Message);
			MessageBox.Show(ex.Message);
		}
	}

	private void SetPrintMaxMem()
	{
		try
		{
			string strPrintMaxMem = textBoxPrintMaxMem.Text;
			if (strPrintMaxMem.Length <= 0)
			{
				MessageBox.Show("最大緩存不可為空");
			}
			else if (Convert.ToInt32(strPrintMaxMem) == 0)
			{
				MessageBox.Show("最大緩存必須大於零");
			}
			else if (ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintMaxMem, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strPrintMaxMem))
			{
				MessageBox.Show("設置成功");
			}
			else
			{
				MessageBox.Show("設置失敗");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SetPrintMaxMem exception:" + ex.Message);
			MessageBox.Show(ex.Message);
		}
	}

	private void frmSuperSet_Load(object sender, EventArgs e)
	{
		LoadDelaySet();
		LoadResolution();
		LoadPrintMaxMem();
		LoadDataBuffer();
	}

	private void LoadDataBuffer()
	{
		textBoxDataBufferTime.Text = ClsXml.GetDataBufferTime().ToString();
	}

	private void LoadDelaySet()
	{
		textBoxPH1Delay1.Text = ClsXml.GetPH1Delay1().ToString();
		textBoxPH1Delay2.Text = ClsXml.GetPH1Delay2().ToString();
		textBoxPH2Delay1.Text = ClsXml.GetPH2Delay1().ToString();
		textBoxPH2Delay2.Text = ClsXml.GetPH2Delay2().ToString();
		textBoxPH3Delay1.Text = ClsXml.GetPH3Delay1().ToString();
		textBoxPH3Delay2.Text = ClsXml.GetPH3Delay2().ToString();
		textBoxPH4Delay1.Text = ClsXml.GetPH4Delay1().ToString();
		textBoxPH4Delay2.Text = ClsXml.GetPH4Delay2().ToString();
	}

	private void LoadResolution()
	{
		textBoxWidthResolution.Text = ClsXml.GetWidthDPI().ToString();
	}

	private void LoadPrintMaxMem()
	{
		textBoxPrintMaxMem.Text = ClsXml.GetPrintMaxMem().ToString();
	}

	private void textBoxPH1Delay1_KeyPress(object sender, KeyPressEventArgs e)
	{
		try
		{
			if (e.KeyChar != '.' && !ClsShare.OnlyInputNum(e.KeyChar))
			{
				e.Handled = true;
			}
		}
		catch
		{
		}
	}

	private void textBoxPH1Delay1_KeyUp(object sender, KeyEventArgs e)
	{
		try
		{
			string strTmp = ((TextBox)sender).Text;
			if (strTmp.Length <= 0)
			{
				return;
			}
			if (strTmp.Substring(0, 1).Contains('.'))
			{
				if (strTmp.Length > 1)
				{
					((TextBox)sender).Text = strTmp.Substring(1, strTmp.Length - 1);
				}
				else
				{
					((TextBox)sender).Text = "";
				}
			}
			else
			{
				if (!strTmp.Contains('.'))
				{
					return;
				}
				string[] strArrTmp = strTmp.Split('.');
				strTmp = "";
				for (int iIndex = 0; iIndex < strArrTmp.Count(); iIndex++)
				{
					strTmp += strArrTmp[iIndex];
					if (iIndex == 0)
					{
						strTmp += ".";
					}
				}
				((TextBox)sender).Text = strTmp;
				e.Handled = true;
			}
		}
		catch
		{
		}
	}

	private void textBoxWidthResolution_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!ClsShare.OnlyInputNum(e.KeyChar))
		{
			e.Handled = true;
		}
	}

	private void textBoxPH1Delay1_TextChanged(object sender, EventArgs e)
	{
		if (!ClsShare.CheckStrIsNumAndPoint(((TextBox)sender).Text.Trim()))
		{
			((TextBox)sender).Text = "0";
			MessageBox.Show("輸入必須為數字或者小數點");
		}
	}

	private void textBoxWidthResolution_TextChanged(object sender, EventArgs e)
	{
		if (!ClsShare.CheckStrIsNum(((TextBox)sender).Text.Trim()))
		{
			((TextBox)sender).Text = "0";
			MessageBox.Show("輸入必須為數字");
		}
	}

	private void textBoxPrintMaxMem_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!ClsShare.OnlyInputNum(e.KeyChar))
		{
			e.Handled = true;
		}
	}

	private void textBoxPrintMaxMem_TextChanged(object sender, EventArgs e)
	{
		if (!ClsShare.CheckStrIsNum(((TextBox)sender).Text.Trim()))
		{
			((TextBox)sender).Text = "0";
			MessageBox.Show("輸入必須為數字");
		}
	}

	private void btnResumeDefaultSet_Click(object sender, EventArgs e)
	{
	}

	private void textBoxDataBufferTime_TextChanged(object sender, EventArgs e)
	{
		if (!ClsShare.CheckStrIsNum(((TextBox)sender).Text.Trim()))
		{
			((TextBox)sender).Text = "0";
			MessageBox.Show("輸入必須為數字");
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeJianPrintingConsole.frmSuperSet));
		this.tabCtrlSuperSet = new System.Windows.Forms.TabControl();
		this.tabPageDefaultSet = new System.Windows.Forms.TabPage();
		this.checkBoxResumeDefaultPWD = new System.Windows.Forms.CheckBox();
		this.tabPagePHDelaySet = new System.Windows.Forms.TabPage();
		this.label3 = new System.Windows.Forms.Label();
		this.textBoxPH4Delay2 = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.textBoxPH3Delay2 = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.textBoxPH2Delay2 = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.textBoxPH1Delay2 = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.label45 = new System.Windows.Forms.Label();
		this.textBoxPH4Delay1 = new System.Windows.Forms.TextBox();
		this.label46 = new System.Windows.Forms.Label();
		this.label37 = new System.Windows.Forms.Label();
		this.textBoxPH3Delay1 = new System.Windows.Forms.TextBox();
		this.label38 = new System.Windows.Forms.Label();
		this.label29 = new System.Windows.Forms.Label();
		this.textBoxPH2Delay1 = new System.Windows.Forms.TextBox();
		this.label30 = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.textBoxPH1Delay1 = new System.Windows.Forms.TextBox();
		this.label15 = new System.Windows.Forms.Label();
		this.tabPageResolutionSet = new System.Windows.Forms.TabPage();
		this.textBoxWidthResolution = new System.Windows.Forms.TextBox();
		this.label11 = new System.Windows.Forms.Label();
		this.tabPagePrintMaxMem = new System.Windows.Forms.TabPage();
		this.textBoxPrintMaxMem = new System.Windows.Forms.TextBox();
		this.label7 = new System.Windows.Forms.Label();
		this.tabPageDataBuffer = new System.Windows.Forms.TabPage();
		this.label9 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.btnOK = new System.Windows.Forms.Button();
		this.textBoxDataBufferTime = new System.Windows.Forms.TextBox();
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		this.tabCtrlSuperSet.SuspendLayout();
		this.tabPageDefaultSet.SuspendLayout();
		this.tabPagePHDelaySet.SuspendLayout();
		this.tabPageResolutionSet.SuspendLayout();
		this.tabPagePrintMaxMem.SuspendLayout();
		this.tabPageDataBuffer.SuspendLayout();
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.tabCtrlSuperSet);
		base.pnlClientData.Margin = new System.Windows.Forms.Padding(4);
		base.pnlClientData.Size = new System.Drawing.Size(923, 417);
		base.pnlBottomCtrl.Controls.Add(this.btnOK);
		base.pnlBottomCtrl.Location = new System.Drawing.Point(0, 417);
		base.pnlBottomCtrl.Margin = new System.Windows.Forms.Padding(4);
		base.pnlBottomCtrl.Size = new System.Drawing.Size(923, 115);
		this.tabCtrlSuperSet.Controls.Add(this.tabPageDefaultSet);
		this.tabCtrlSuperSet.Controls.Add(this.tabPagePHDelaySet);
		this.tabCtrlSuperSet.Controls.Add(this.tabPageResolutionSet);
		this.tabCtrlSuperSet.Controls.Add(this.tabPagePrintMaxMem);
		this.tabCtrlSuperSet.Controls.Add(this.tabPageDataBuffer);
		this.tabCtrlSuperSet.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabCtrlSuperSet.Location = new System.Drawing.Point(0, 0);
		this.tabCtrlSuperSet.Name = "tabCtrlSuperSet";
		this.tabCtrlSuperSet.SelectedIndex = 0;
		this.tabCtrlSuperSet.Size = new System.Drawing.Size(919, 413);
		this.tabCtrlSuperSet.TabIndex = 0;
		this.tabPageDefaultSet.Controls.Add(this.checkBoxResumeDefaultPWD);
		this.tabPageDefaultSet.Location = new System.Drawing.Point(4, 22);
		this.tabPageDefaultSet.Name = "tabPageDefaultSet";
		this.tabPageDefaultSet.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageDefaultSet.Size = new System.Drawing.Size(911, 387);
		this.tabPageDefaultSet.TabIndex = 0;
		this.tabPageDefaultSet.Text = "恢復默認設置";
		this.tabPageDefaultSet.UseVisualStyleBackColor = true;
		this.checkBoxResumeDefaultPWD.AutoSize = true;
		this.checkBoxResumeDefaultPWD.Checked = true;
		this.checkBoxResumeDefaultPWD.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBoxResumeDefaultPWD.Location = new System.Drawing.Point(55, 35);
		this.checkBoxResumeDefaultPWD.Name = "checkBoxResumeDefaultPWD";
		this.checkBoxResumeDefaultPWD.Size = new System.Drawing.Size(96, 16);
		this.checkBoxResumeDefaultPWD.TabIndex = 0;
		this.checkBoxResumeDefaultPWD.Text = "恢復默認密碼";
		this.checkBoxResumeDefaultPWD.UseVisualStyleBackColor = true;
		this.tabPagePHDelaySet.Controls.Add(this.label3);
		this.tabPagePHDelaySet.Controls.Add(this.textBoxPH4Delay2);
		this.tabPagePHDelaySet.Controls.Add(this.label4);
		this.tabPagePHDelaySet.Controls.Add(this.textBoxPH3Delay2);
		this.tabPagePHDelaySet.Controls.Add(this.label5);
		this.tabPagePHDelaySet.Controls.Add(this.textBoxPH2Delay2);
		this.tabPagePHDelaySet.Controls.Add(this.label6);
		this.tabPagePHDelaySet.Controls.Add(this.textBoxPH1Delay2);
		this.tabPagePHDelaySet.Controls.Add(this.label2);
		this.tabPagePHDelaySet.Controls.Add(this.label1);
		this.tabPagePHDelaySet.Controls.Add(this.label45);
		this.tabPagePHDelaySet.Controls.Add(this.textBoxPH4Delay1);
		this.tabPagePHDelaySet.Controls.Add(this.label46);
		this.tabPagePHDelaySet.Controls.Add(this.label37);
		this.tabPagePHDelaySet.Controls.Add(this.textBoxPH3Delay1);
		this.tabPagePHDelaySet.Controls.Add(this.label38);
		this.tabPagePHDelaySet.Controls.Add(this.label29);
		this.tabPagePHDelaySet.Controls.Add(this.textBoxPH2Delay1);
		this.tabPagePHDelaySet.Controls.Add(this.label30);
		this.tabPagePHDelaySet.Controls.Add(this.label16);
		this.tabPagePHDelaySet.Controls.Add(this.textBoxPH1Delay1);
		this.tabPagePHDelaySet.Controls.Add(this.label15);
		this.tabPagePHDelaySet.Location = new System.Drawing.Point(4, 22);
		this.tabPagePHDelaySet.Name = "tabPagePHDelaySet";
		this.tabPagePHDelaySet.Padding = new System.Windows.Forms.Padding(3);
		this.tabPagePHDelaySet.Size = new System.Drawing.Size(903, 300);
		this.tabPagePHDelaySet.TabIndex = 1;
		this.tabPagePHDelaySet.Text = "延遲設置";
		this.tabPagePHDelaySet.UseVisualStyleBackColor = true;
		this.label3.AutoSize = true;
		this.label3.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label3.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label3.Location = new System.Drawing.Point(304, 179);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(31, 19);
		this.label3.TabIndex = 171;
		this.label3.Text = "mm";
		this.textBoxPH4Delay2.BackColor = System.Drawing.Color.White;
		this.textBoxPH4Delay2.Location = new System.Drawing.Point(241, 177);
		this.textBoxPH4Delay2.Name = "textBoxPH4Delay2";
		this.textBoxPH4Delay2.Size = new System.Drawing.Size(59, 21);
		this.textBoxPH4Delay2.TabIndex = 170;
		this.textBoxPH4Delay2.TextChanged += new System.EventHandler(textBoxPH1Delay1_TextChanged);
		this.textBoxPH4Delay2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPH1Delay1_KeyPress);
		this.textBoxPH4Delay2.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxPH1Delay1_KeyUp);
		this.label4.AutoSize = true;
		this.label4.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label4.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label4.Location = new System.Drawing.Point(304, 142);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(31, 19);
		this.label4.TabIndex = 169;
		this.label4.Text = "mm";
		this.textBoxPH3Delay2.BackColor = System.Drawing.Color.White;
		this.textBoxPH3Delay2.Location = new System.Drawing.Point(241, 140);
		this.textBoxPH3Delay2.Name = "textBoxPH3Delay2";
		this.textBoxPH3Delay2.Size = new System.Drawing.Size(59, 21);
		this.textBoxPH3Delay2.TabIndex = 168;
		this.textBoxPH3Delay2.TextChanged += new System.EventHandler(textBoxPH1Delay1_TextChanged);
		this.textBoxPH3Delay2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPH1Delay1_KeyPress);
		this.textBoxPH3Delay2.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxPH1Delay1_KeyUp);
		this.label5.AutoSize = true;
		this.label5.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label5.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label5.Location = new System.Drawing.Point(304, 105);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(31, 19);
		this.label5.TabIndex = 167;
		this.label5.Text = "mm";
		this.textBoxPH2Delay2.BackColor = System.Drawing.Color.White;
		this.textBoxPH2Delay2.Location = new System.Drawing.Point(241, 103);
		this.textBoxPH2Delay2.Name = "textBoxPH2Delay2";
		this.textBoxPH2Delay2.Size = new System.Drawing.Size(59, 21);
		this.textBoxPH2Delay2.TabIndex = 166;
		this.textBoxPH2Delay2.TextChanged += new System.EventHandler(textBoxPH1Delay1_TextChanged);
		this.textBoxPH2Delay2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPH1Delay1_KeyPress);
		this.textBoxPH2Delay2.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxPH1Delay1_KeyUp);
		this.label6.AutoSize = true;
		this.label6.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label6.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label6.Location = new System.Drawing.Point(304, 68);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(31, 19);
		this.label6.TabIndex = 165;
		this.label6.Text = "mm";
		this.textBoxPH1Delay2.BackColor = System.Drawing.Color.White;
		this.textBoxPH1Delay2.Location = new System.Drawing.Point(241, 66);
		this.textBoxPH1Delay2.Name = "textBoxPH1Delay2";
		this.textBoxPH1Delay2.Size = new System.Drawing.Size(59, 21);
		this.textBoxPH1Delay2.TabIndex = 164;
		this.textBoxPH1Delay2.TextChanged += new System.EventHandler(textBoxPH1Delay1_TextChanged);
		this.textBoxPH1Delay2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPH1Delay1_KeyPress);
		this.textBoxPH1Delay2.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxPH1Delay1_KeyUp);
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label2.Location = new System.Drawing.Point(248, 40);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(45, 14);
		this.label2.TabIndex = 163;
		this.label2.Text = "延遲2";
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label1.Location = new System.Drawing.Point(117, 40);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(45, 14);
		this.label1.TabIndex = 162;
		this.label1.Text = "延遲1";
		this.label45.AutoSize = true;
		this.label45.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label45.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label45.Location = new System.Drawing.Point(173, 182);
		this.label45.Name = "label45";
		this.label45.Size = new System.Drawing.Size(31, 19);
		this.label45.TabIndex = 161;
		this.label45.Text = "mm";
		this.textBoxPH4Delay1.BackColor = System.Drawing.Color.White;
		this.textBoxPH4Delay1.Location = new System.Drawing.Point(110, 180);
		this.textBoxPH4Delay1.Name = "textBoxPH4Delay1";
		this.textBoxPH4Delay1.Size = new System.Drawing.Size(59, 21);
		this.textBoxPH4Delay1.TabIndex = 160;
		this.textBoxPH4Delay1.TextChanged += new System.EventHandler(textBoxPH1Delay1_TextChanged);
		this.textBoxPH4Delay1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPH1Delay1_KeyPress);
		this.textBoxPH4Delay1.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxPH1Delay1_KeyUp);
		this.label46.AutoSize = true;
		this.label46.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label46.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label46.Location = new System.Drawing.Point(51, 184);
		this.label46.Name = "label46";
		this.label46.Size = new System.Drawing.Size(53, 14);
		this.label46.TabIndex = 159;
		this.label46.Text = "噴頭4:";
		this.label37.AutoSize = true;
		this.label37.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label37.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label37.Location = new System.Drawing.Point(173, 145);
		this.label37.Name = "label37";
		this.label37.Size = new System.Drawing.Size(31, 19);
		this.label37.TabIndex = 158;
		this.label37.Text = "mm";
		this.textBoxPH3Delay1.BackColor = System.Drawing.Color.White;
		this.textBoxPH3Delay1.Location = new System.Drawing.Point(110, 143);
		this.textBoxPH3Delay1.Name = "textBoxPH3Delay1";
		this.textBoxPH3Delay1.Size = new System.Drawing.Size(59, 21);
		this.textBoxPH3Delay1.TabIndex = 157;
		this.textBoxPH3Delay1.TextChanged += new System.EventHandler(textBoxPH1Delay1_TextChanged);
		this.textBoxPH3Delay1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPH1Delay1_KeyPress);
		this.textBoxPH3Delay1.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxPH1Delay1_KeyUp);
		this.label38.AutoSize = true;
		this.label38.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label38.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label38.Location = new System.Drawing.Point(51, 147);
		this.label38.Name = "label38";
		this.label38.Size = new System.Drawing.Size(53, 14);
		this.label38.TabIndex = 156;
		this.label38.Text = "噴頭3:";
		this.label29.AutoSize = true;
		this.label29.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label29.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label29.Location = new System.Drawing.Point(173, 108);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(31, 19);
		this.label29.TabIndex = 155;
		this.label29.Text = "mm";
		this.textBoxPH2Delay1.BackColor = System.Drawing.Color.White;
		this.textBoxPH2Delay1.Location = new System.Drawing.Point(110, 106);
		this.textBoxPH2Delay1.Name = "textBoxPH2Delay1";
		this.textBoxPH2Delay1.Size = new System.Drawing.Size(59, 21);
		this.textBoxPH2Delay1.TabIndex = 154;
		this.textBoxPH2Delay1.TextChanged += new System.EventHandler(textBoxPH1Delay1_TextChanged);
		this.textBoxPH2Delay1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPH1Delay1_KeyPress);
		this.textBoxPH2Delay1.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxPH1Delay1_KeyUp);
		this.label30.AutoSize = true;
		this.label30.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label30.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label30.Location = new System.Drawing.Point(51, 110);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(53, 14);
		this.label30.TabIndex = 153;
		this.label30.Text = "噴頭2:";
		this.label16.AutoSize = true;
		this.label16.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label16.Font = new System.Drawing.Font("宋体", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label16.Location = new System.Drawing.Point(173, 71);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(31, 19);
		this.label16.TabIndex = 152;
		this.label16.Text = "mm";
		this.textBoxPH1Delay1.BackColor = System.Drawing.Color.White;
		this.textBoxPH1Delay1.Location = new System.Drawing.Point(110, 69);
		this.textBoxPH1Delay1.Name = "textBoxPH1Delay1";
		this.textBoxPH1Delay1.Size = new System.Drawing.Size(59, 21);
		this.textBoxPH1Delay1.TabIndex = 151;
		this.textBoxPH1Delay1.TextChanged += new System.EventHandler(textBoxPH1Delay1_TextChanged);
		this.textBoxPH1Delay1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPH1Delay1_KeyPress);
		this.textBoxPH1Delay1.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxPH1Delay1_KeyUp);
		this.label15.AutoSize = true;
		this.label15.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label15.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label15.Location = new System.Drawing.Point(51, 73);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(53, 14);
		this.label15.TabIndex = 150;
		this.label15.Text = "噴頭1:";
		this.tabPageResolutionSet.Controls.Add(this.textBoxWidthResolution);
		this.tabPageResolutionSet.Controls.Add(this.label11);
		this.tabPageResolutionSet.Location = new System.Drawing.Point(4, 22);
		this.tabPageResolutionSet.Name = "tabPageResolutionSet";
		this.tabPageResolutionSet.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageResolutionSet.Size = new System.Drawing.Size(903, 300);
		this.tabPageResolutionSet.TabIndex = 2;
		this.tabPageResolutionSet.Text = "分辨率設置";
		this.tabPageResolutionSet.UseVisualStyleBackColor = true;
		this.textBoxWidthResolution.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxWidthResolution.Location = new System.Drawing.Point(107, 25);
		this.textBoxWidthResolution.Name = "textBoxWidthResolution";
		this.textBoxWidthResolution.Size = new System.Drawing.Size(147, 21);
		this.textBoxWidthResolution.TabIndex = 32;
		this.textBoxWidthResolution.TextChanged += new System.EventHandler(textBoxWidthResolution_TextChanged);
		this.textBoxWidthResolution.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxWidthResolution_KeyPress);
		this.label11.AutoSize = true;
		this.label11.BackColor = System.Drawing.Color.Transparent;
		this.label11.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label11.Location = new System.Drawing.Point(11, 28);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(90, 14);
		this.label11.TabIndex = 31;
		this.label11.Text = "橫向分辨率:";
		this.tabPagePrintMaxMem.Controls.Add(this.textBoxPrintMaxMem);
		this.tabPagePrintMaxMem.Controls.Add(this.label7);
		this.tabPagePrintMaxMem.Location = new System.Drawing.Point(4, 22);
		this.tabPagePrintMaxMem.Name = "tabPagePrintMaxMem";
		this.tabPagePrintMaxMem.Padding = new System.Windows.Forms.Padding(3);
		this.tabPagePrintMaxMem.Size = new System.Drawing.Size(911, 387);
		this.tabPagePrintMaxMem.TabIndex = 3;
		this.tabPagePrintMaxMem.Text = "噴印最大緩存";
		this.tabPagePrintMaxMem.UseVisualStyleBackColor = true;
		this.textBoxPrintMaxMem.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxPrintMaxMem.Location = new System.Drawing.Point(120, 26);
		this.textBoxPrintMaxMem.Name = "textBoxPrintMaxMem";
		this.textBoxPrintMaxMem.Size = new System.Drawing.Size(147, 21);
		this.textBoxPrintMaxMem.TabIndex = 34;
		this.textBoxPrintMaxMem.Text = "500";
		this.textBoxPrintMaxMem.TextChanged += new System.EventHandler(textBoxPrintMaxMem_TextChanged);
		this.textBoxPrintMaxMem.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxPrintMaxMem_KeyPress);
		this.label7.AutoSize = true;
		this.label7.BackColor = System.Drawing.Color.Transparent;
		this.label7.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label7.Location = new System.Drawing.Point(24, 29);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(82, 14);
		this.label7.TabIndex = 33;
		this.label7.Text = "最大緩存：";
		this.tabPageDataBuffer.Controls.Add(this.textBoxDataBufferTime);
		this.tabPageDataBuffer.Controls.Add(this.label9);
		this.tabPageDataBuffer.Controls.Add(this.label8);
		this.tabPageDataBuffer.Location = new System.Drawing.Point(4, 22);
		this.tabPageDataBuffer.Name = "tabPageDataBuffer";
		this.tabPageDataBuffer.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageDataBuffer.Size = new System.Drawing.Size(911, 387);
		this.tabPageDataBuffer.TabIndex = 4;
		this.tabPageDataBuffer.Text = "數據緩衝";
		this.tabPageDataBuffer.UseVisualStyleBackColor = true;
		this.label9.AutoSize = true;
		this.label9.BackColor = System.Drawing.Color.Transparent;
		this.label9.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label9.Location = new System.Drawing.Point(289, 34);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(22, 14);
		this.label9.TabIndex = 37;
		this.label9.Text = "秒";
		this.label8.AutoSize = true;
		this.label8.BackColor = System.Drawing.Color.Transparent;
		this.label8.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label8.Location = new System.Drawing.Point(27, 33);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(112, 14);
		this.label8.TabIndex = 35;
		this.label8.Text = "數據緩衝時間：";
		this.btnOK.Location = new System.Drawing.Point(103, 39);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(106, 33);
		this.btnOK.TabIndex = 12;
		this.btnOK.Text = "設置";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.textBoxDataBufferTime.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxDataBufferTime.Location = new System.Drawing.Point(136, 31);
		this.textBoxDataBufferTime.Name = "textBoxDataBufferTime";
		this.textBoxDataBufferTime.Size = new System.Drawing.Size(147, 21);
		this.textBoxDataBufferTime.TabIndex = 38;
		this.textBoxDataBufferTime.Text = "60";
		this.textBoxDataBufferTime.TextChanged += new System.EventHandler(textBoxDataBufferTime_TextChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(927, 536);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "frmSuperSet";
		this.Text = "超級設定";
		base.Load += new System.EventHandler(frmSuperSet_Load);
		base.pnlClientData.ResumeLayout(false);
		base.pnlBottomCtrl.ResumeLayout(false);
		this.tabCtrlSuperSet.ResumeLayout(false);
		this.tabPageDefaultSet.ResumeLayout(false);
		this.tabPageDefaultSet.PerformLayout();
		this.tabPagePHDelaySet.ResumeLayout(false);
		this.tabPagePHDelaySet.PerformLayout();
		this.tabPageResolutionSet.ResumeLayout(false);
		this.tabPageResolutionSet.PerformLayout();
		this.tabPagePrintMaxMem.ResumeLayout(false);
		this.tabPagePrintMaxMem.PerformLayout();
		this.tabPageDataBuffer.ResumeLayout(false);
		this.tabPageDataBuffer.PerformLayout();
		base.ResumeLayout(false);
	}
}
