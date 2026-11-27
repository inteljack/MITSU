using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmSysSet : frmBase
{
	private Thread m_ThreadIniDataBase;

	private IContainer components;

	private Button btnLeave;

	private Button btnSet;

	private TabControl tabCtrlSysSet;

	private TabPage tabPagePathSet;

	private TabPage tabPagePwdSet;

	private TextBox textBoxSurePwd;

	private Label label3;

	private TextBox textBoxNewPwd;

	private Label label2;

	private TextBox textBoxOldPwd;

	private Label label1;

	private Button btnExportPath;

	private TextBox textBoxExportPath;

	private Label lblExportPath;

	private Button btnImportPath;

	private TextBox textBoxImportPath;

	private Label lblImportPath;

	private TabPage tabPageDataBaseSet;

	private TextBox textBoxDataBasePwd;

	private Label label13;

	private TextBox textBoxDataBaseUserID;

	private Label label12;

	private TextBox textBoxDataBaseName;

	private Label label11;

	private TextBox textBoxServerName;

	private Label label10;

	private Label lblIniDB;

	private TabPage tabPageFormSet;

	private TextBox textBoxMainFormHeight;

	private Label label17;

	private TextBox textBoxMainFormWidth;

	private Label label16;

	private TextBox textBoxMainFormY;

	private Label label15;

	private TextBox textBoxMainFormX;

	private Label label14;

	private TabPage tabPageGroupMsgSet;

	private Button btnSourceMsg1Path;

	private TextBox textBoxSourceMsg1Path;

	private Label lblSrcMsg1;

	private Button btnSourceMsg2Path;

	private TextBox textBoxSourceMsg2Path;

	private Label lblSrcMsg2;

	private Label label18;

	private TextBox textBoxMaxSinglePrintHeadHeight;

	private Label label19;

	private TextBox textBoxOneMsgMaxPrintHeadCountOnGroupMsg;

	private Label label20;

	private Button btnGetMainFormParam;

	private Button btnSaveImportExportPath;

	private Button btnModifyPWD;

	private Button btnSetDataBase;

	private Button btnSaveFormPos;

	private Button btnFrontSideMsgPath;

	private TabPage tabPagePrintDataFieldMaxLen;

	private NumericUpDown numericUpDownPlace;

	private NumericUpDown numericUpDownHzUnit;

	private NumericUpDown numericUpDownPhaseUnit;

	private NumericUpDown numericUpDownOutput;

	private NumericUpDown numericUpDownModelValue;

	private NumericUpDown numericUpDownTokki;

	private NumericUpDown numericUpDownHzValue;

	private NumericUpDown numericUpDownPhaseValue;

	private NumericUpDown numericUpDownBoreUnit;

	private NumericUpDown numericUpDownModel;

	private NumericUpDown numericUpDownCodeValue;

	private NumericUpDown numericUpDownVoltageUnit;

	private NumericUpDown numericUpDownOutputUnit;

	private NumericUpDown numericUpDownBoreValue;

	private NumericUpDown numericUpDownCaseNo;

	private NumericUpDown numericUpDownCode;

	private NumericUpDown numericUpDownVoltageValue;

	private NumericUpDown numericUpDownOutputValue;

	private NumericUpDown numericUpDownBore;

	private NumericUpDown numericUpDownKeycode;

	private Label label29;

	private Label label30;

	private Label label31;

	private Label label32;

	private Label label33;

	private Label label34;

	private Label label35;

	private Label label36;

	private Label label37;

	private Label label38;

	private Label label24;

	private Label label25;

	private Label label26;

	private Label label27;

	private Label label28;

	private Label label23;

	private Label label22;

	private Label label21;

	private Label label5;

	private Label label4;

	private Button btnSavePrintDataFieldMaxLen;

	private TabPage tabPageSysBackupResume;

	private Button btnBackupResumeOK;

	private RadioButton radioBtnSysResume;

	private RadioButton radioBtnSysBackup;

	private Label label6;

	public frmSysSet()
	{
		InitializeComponent();
	}

	private void btnLeave_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void btnSelModelPath_Click(object sender, EventArgs e)
	{
		string strGetFolderPath = "";
		DialogResult DlgRlt = ClsShare.GetFolderPath(ref strGetFolderPath);
		if (DlgRlt == DialogResult.OK)
		{
			if ((Button)sender == btnExportPath)
			{
				textBoxExportPath.Text = strGetFolderPath;
			}
			else if ((Button)sender == btnImportPath)
			{
				textBoxImportPath.Text = strGetFolderPath;
			}
		}
	}

	private void textBoxNewPwd_TextChanged(object sender, EventArgs e)
	{
	}

	private bool CheckPwd(string strSrcPwd, string strDestPwd)
	{
		if (strSrcPwd == strDestPwd)
		{
			return true;
		}
		return false;
	}

	private void textBoxSurePwd_TextChanged(object sender, EventArgs e)
	{
	}

	private void textBoxSurePwd_Leave(object sender, EventArgs e)
	{
	}

	private void btnSet_Click(object sender, EventArgs e)
	{
		if (tabCtrlSysSet.Controls["tabPagePathSet"].Visible)
		{
			SetPath();
		}
		else if (tabCtrlSysSet.Controls["tabPagePwdSet"].Visible)
		{
			SetSysPwd();
		}
		else if (tabCtrlSysSet.Controls["tabPageDataBaseSet"].Visible)
		{
			SetDataBase();
		}
		else if (tabCtrlSysSet.Controls["tabPageFormSet"].Visible)
		{
			SetForm();
		}
		else if (tabCtrlSysSet.Controls["tabPageGroupMsgSet"].Visible)
		{
			SetGroupMsgParam();
		}
		else if (tabCtrlSysSet.Controls["tabPageSysBackupResume"].Visible)
		{
			SetSysBackupResume();
		}
	}

	private void SetPath()
	{
		try
		{
			string strImportFolder = textBoxImportPath.Text;
			if (strImportFolder.Length <= 0)
			{
				btnImportPath.Focus();
				MessageBox.Show("導入路徑不可為空");
				return;
			}
			string strExportFolder = textBoxExportPath.Text;
			if (strExportFolder.Length <= 0)
			{
				btnExportPath.Focus();
				MessageBox.Show("導出路徑不可為空");
				return;
			}
			bool bRltSetImportFolder = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strImportFolder, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strImportFolder);
			bool bRltSetExportFolder = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strExportFolder, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strExportFolder);
			if (bRltSetImportFolder && bRltSetExportFolder)
			{
				MessageBox.Show("保存成功");
			}
			else
			{
				MessageBox.Show("保存失敗");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SetPath exception:" + ex.Message);
			MessageBox.Show("保存失敗");
		}
	}

	private void SetSysPwd()
	{
		try
		{
			if (!CheckPwd(textBoxOldPwd.Text, ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSystemPwd, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue)))
			{
				MessageBox.Show("舊密碼錯誤");
				textBoxOldPwd.Focus();
			}
			else if (!CheckPwd(textBoxNewPwd.Text, textBoxSurePwd.Text))
			{
				MessageBox.Show("確認密碼與新密碼不一致");
			}
			else if (ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSystemPwd, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, textBoxNewPwd.Text))
			{
				MessageBox.Show("修改成功");
			}
			else
			{
				MessageBox.Show("修改失敗");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SetSysPwd exception:" + ex.Message);
			MessageBox.Show("修改失敗");
		}
	}

	private void SetDataBase()
	{
		try
		{
			lblIniDB.Visible = true;
			string sqlConnectionString = string.Format("server={0};uid={2};password={3};database={1}", textBoxServerName.Text, textBoxDataBaseName.Text, textBoxDataBaseUserID.Text, textBoxDataBasePwd.Text);
			if (!ClsDataBase.CheckDatabaseIsConnected(bUseDefaultConnectionString: false, sqlConnectionString))
			{
				MessageBox.Show("數據庫無法連接，請確認參數是否正確!");
				lblIniDB.Visible = false;
				Refresh();
				return;
			}
			bool bRlt1 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strServerName, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, textBoxServerName.Text);
			bool bRlt2 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strDatabaseName, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, textBoxDataBaseName.Text);
			bool bRlt3 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strDatabaseUserID, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, textBoxDataBaseUserID.Text);
			bool bRlt4 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strDatabasePwd, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, textBoxDataBasePwd.Text);
			if (bRlt1 && bRlt2 && bRlt3 && bRlt4)
			{
				string strDataBaseName = ClsXml.GetDataBaseName();
				bool bIniDataBase = false;
				if (ClsDataBase.SqlCheckDatabaseExist(strDataBaseName) > 0)
				{
					if (DialogResult.Yes != MessageBox.Show("數據庫已經在存在，是否清空後重新初始化?", "系統提示", MessageBoxButtons.YesNo))
					{
						lblIniDB.Visible = false;
						return;
					}
					if (!ClsDataBase.SqlDelDatabase(strDataBaseName))
					{
						MessageBox.Show("清空數據庫失敗，請確認該數據庫是否被占用?");
						lblIniDB.Visible = false;
						return;
					}
					bIniDataBase = true;
				}
				if (bIniDataBase || DialogResult.Yes == MessageBox.Show("是否初始化數據庫?", "系統提示", MessageBoxButtons.YesNo))
				{
					StartIniDatabaseThread();
				}
				else
				{
					lblIniDB.Visible = false;
				}
			}
			else
			{
				MessageBox.Show("初始化數據庫失敗");
				ClsLog.LogWrite("保存數據庫信息至配置文件失敗");
				lblIniDB.Visible = false;
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SetDataBase exception:" + ex.Message);
			MessageBox.Show("初始化數據庫失敗");
			lblIniDB.Visible = false;
		}
	}

	private void SetForm()
	{
		try
		{
			string strMainFormX = textBoxMainFormX.Text;
			string strMainFormY = textBoxMainFormY.Text;
			string strMainFormWidth = textBoxMainFormWidth.Text;
			string strMainFormHeight = textBoxMainFormHeight.Text;
			if (Convert.ToInt32(strMainFormWidth) <= 0)
			{
				MessageBox.Show("寬度必須為正數");
				return;
			}
			if (Convert.ToInt32(strMainFormHeight) <= 0)
			{
				MessageBox.Show("高度必須為正數");
				return;
			}
			bool bRltSetMainFormX = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strMainFormX, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strMainFormX);
			bool bRltSetMainFormY = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strMainFormY, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strMainFormY);
			bool bRltSetMainFormWidth = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strMainFormWidth, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strMainFormWidth);
			bool bRltSetMainFormHeight = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strMainFormHeight, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strMainFormHeight);
			if (bRltSetMainFormX && bRltSetMainFormY && bRltSetMainFormWidth && bRltSetMainFormHeight)
			{
				MessageBox.Show("保存成功");
			}
			else
			{
				MessageBox.Show("保存失敗");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SetForm exception:" + ex.Message);
			MessageBox.Show("保存失敗");
		}
	}

	private void SetGroupMsgParam()
	{
		try
		{
			string strSourceMsg1Path = textBoxSourceMsg1Path.Text;
			string strSourceMsg2Path = textBoxSourceMsg2Path.Text;
			string strOneMsgMaxPrintHeadCount = textBoxOneMsgMaxPrintHeadCountOnGroupMsg.Text;
			string strSinglePrintHeadHeight = textBoxMaxSinglePrintHeadHeight.Text;
			if (strSourceMsg1Path.Length <= 0 || strSourceMsg2Path.Length <= 0)
			{
				MessageBox.Show("路徑不可為空");
				return;
			}
			bool bRlt1 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSourceMsg1Path, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strSourceMsg1Path);
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSourceMsg2Path, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strSourceMsg2Path);
			bool bRlt3 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strOneMsgMaxPrintHeadCountOnGroupMsg, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strOneMsgMaxPrintHeadCount);
			bool bRlt4 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSinglePrintHeadHeight, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strSinglePrintHeadHeight);
			if (bRlt1 && bRlt1 && bRlt3 && bRlt4)
			{
				MessageBox.Show("保存成功");
			}
			else
			{
				MessageBox.Show("保存失敗");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SetGroupMsgParam exception:" + ex.Message);
			MessageBox.Show("保存失敗");
		}
	}

	private void SetSysBackupResume()
	{
		string strFolderPath = "";
		if (radioBtnSysBackup.Checked)
		{
			try
			{
				ClsXml.GetExportFloder();
				if (ClsFile.GetFolderPath(ref strFolderPath) == DialogResult.OK)
				{
					File.Copy(Application.StartupPath + "\\Config.xml", strFolderPath + "\\Config.xml", overwrite: true);
					ClsFile.CopyDirectory(Application.StartupPath + "\\Images", strFolderPath);
					ClsFile.CopyDirectory(Application.StartupPath + "\\Msg", strFolderPath);
					if (File.Exists(strFolderPath + "\\Config.xml") && Directory.Exists(strFolderPath + "\\Images") && Directory.Exists(strFolderPath + "\\Msg"))
					{
						MessageBox.Show("備份成功");
					}
					else
					{
						MessageBox.Show("備份失敗");
					}
				}
				return;
			}
			catch (Exception ex)
			{
				ClsLog.LogWrite("SetSysBackup exception:" + ex.Message);
				MessageBox.Show("備份失敗," + ex.Message);
				return;
			}
		}
		if (!radioBtnSysResume.Checked)
		{
			return;
		}
		try
		{
			ClsXml.GetImportFloder();
			if (ClsFile.GetFolderPath(ref strFolderPath) != DialogResult.OK || DialogResult.No == MessageBox.Show("該操作會清除當前所有數據，並還原為備份檔，確定要還原嗎!", "系統提示", MessageBoxButtons.YesNo))
			{
				return;
			}
			string strBackupBeforeResumePath = Application.StartupPath + "\\BackupTemp";
			string strApplicationPath = Application.StartupPath;
			if (Directory.Exists(strFolderPath))
			{
				try
				{
					if (!Directory.Exists(strBackupBeforeResumePath))
					{
						Directory.CreateDirectory(strBackupBeforeResumePath);
					}
					File.Copy(strApplicationPath + "\\config.xml", strBackupBeforeResumePath + "\\config.xml", overwrite: true);
					ClsFile.CopyDirectory(strApplicationPath + "\\Images", strBackupBeforeResumePath);
					ClsFile.CopyDirectory(strApplicationPath + "\\Msg", strBackupBeforeResumePath);
					ClsFile.ClearWholeFolder(strApplicationPath + "\\Images", bClearRootFolder: true);
					ClsFile.ClearWholeFolder(strApplicationPath + "\\Msg", bClearRootFolder: true);
					File.Copy(strFolderPath + "\\config.xml", strApplicationPath + "\\config.xml", overwrite: true);
					ClsFile.CopyDirectory(strFolderPath + "\\Images", strApplicationPath);
					ClsFile.CopyDirectory(strFolderPath + "\\Msg", strApplicationPath);
					LoadSysSet();
					ClsFile.ClearWholeFolder(strBackupBeforeResumePath, bClearRootFolder: true);
				}
				catch
				{
					ClsFile.CopyDirectory(strBackupBeforeResumePath, strApplicationPath);
					LoadSysSet();
					ClsFile.ClearWholeFolder(strBackupBeforeResumePath, bClearRootFolder: true);
					MessageBox.Show("還原失敗，請確認備份檔是否正確!");
					return;
				}
				int iTimeCount = 5;
				string strTimerTitle = "還原成功，程序將在" + iTimeCount + "秒後重啟!";
				frmTimer frmT = new frmTimer(strTimerTitle, iTimeCount);
				frmT.FormBorderStyle = FormBorderStyle.None;
				frmT.ShowDialog();
				ClsShare.m_bRestartApp = true;
				ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strRestartAppFlag, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, "TRUE");
				Process.Start(Assembly.GetExecutingAssembly().Location);
				Application.Restart();
			}
			else
			{
				MessageBox.Show("還原失敗");
			}
		}
		catch (Exception ex2)
		{
			ClsLog.LogWrite("SetSysResume exception:" + ex2.Message);
			MessageBox.Show("還原失敗," + ex2.Message);
		}
	}

	private void StartIniDatabaseThread()
	{
		m_ThreadIniDataBase = new Thread(IniDatabaseThread);
		m_ThreadIniDataBase.IsBackground = true;
		m_ThreadIniDataBase.Start();
	}

	private void IniDatabaseThread()
	{
		int iMaxTimes = 5;
		bool bRlt = false;
		Invoke((EventHandler)delegate
		{
			while (iMaxTimes-- > 0)
			{
				if (IniDatabase())
				{
					bRlt = true;
					break;
				}
				Thread.Sleep(2000);
			}
			lblIniDB.Visible = false;
			if (bRlt)
			{
				MessageBox.Show("初始化數據庫成功");
			}
			else
			{
				MessageBox.Show("初始化數據庫失敗");
			}
		});
	}

	private bool IniDatabase()
	{
		try
		{
			if (ClsDataBase.SqlCreateDatabase(ClsXml.GetDataBaseName()))
			{
				if (SqlIniTab())
				{
					return true;
				}
				ClsLog.LogWrite("SqlIniTab fail");
				return false;
			}
			ClsLog.LogWrite("SqlCreateDatabase fail");
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("IniDatabase exception:" + ex.Message);
			return false;
		}
	}

	private bool SqlIniTab()
	{
		try
		{
			bool bRlt1 = ClsDataBase.SqlCreatePrintDataTab();
			bool bRlt2 = ClsDataBase.SqlCreateCartonSizeTab();
			bool bRlt3 = ClsDataBase.SqlCreateProductRecordMasterTab();
			bool bRlt4 = ClsDataBase.SqlCreateProductRecordDetailTab();
			bool bRlt5 = ClsLog.InitializeLogTab();
			if (bRlt1 && bRlt2 && bRlt3 && bRlt4 && bRlt5)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SqlIniTab exception:" + ex.Message);
			return false;
		}
	}

	private string SelMusicFile()
	{
		string strTitle = "選擇音效檔";
		string strFilter = "所有文件(*.*)|*.*|Wave(*.wav)|*.wav";
		int iFilterIndex = 2;
		string strFileName = "";
		ClsShare.GetOpenDlgFileName(strTitle, strFilter, iFilterIndex, Application.StartupPath, ref strFileName);
		return strFileName;
	}

	private void frmSysSet_Load(object sender, EventArgs e)
	{
		LoadSysSet();
		AlterControlPos();
		tabCtrlSysSet.Controls.Remove(tabCtrlSysSet.Controls["tabPageMusicSet"]);
	}

	private void LoadSysSet()
	{
		LoadPathSet();
		LoadDataBaseSet();
		LoadFormSet();
		LoadGroupMsgParamSet();
		LoadPrintDataFieldMaxLenSet();
	}

	private void LoadPathSet()
	{
		textBoxImportPath.Text = ClsXml.GetImportFloder();
		textBoxExportPath.Text = ClsXml.GetExportFloder();
	}

	private void LoadDataBaseSet()
	{
		textBoxServerName.Text = ClsXml.GetServerName();
		textBoxDataBaseName.Text = ClsXml.GetDataBaseName();
		textBoxDataBaseUserID.Text = ClsXml.GetDataBaseUserID();
		textBoxDataBasePwd.Text = ClsXml.GetDataBasePwd();
	}

	private void LoadFormSet()
	{
		textBoxMainFormX.Text = ClsXml.GetMainFormX().ToString();
		textBoxMainFormY.Text = ClsXml.GetMainFormY().ToString();
		textBoxMainFormWidth.Text = ClsXml.GetMainFormWidth().ToString();
		textBoxMainFormHeight.Text = ClsXml.GetMainFormHeight().ToString();
	}

	private void LoadGroupMsgParamSet()
	{
		textBoxSourceMsg1Path.Text = ClsXml.GetSourceMsg1Path().ToString();
		textBoxSourceMsg2Path.Text = ClsXml.GetSourceMsg2Path().ToString();
		textBoxOneMsgMaxPrintHeadCountOnGroupMsg.Text = ClsXml.GetOneMsgMaxPrintHeadCountOnGroupMsg().ToString();
		textBoxMaxSinglePrintHeadHeight.Text = ClsXml.GetSinglePrintHeadHeight().ToString();
	}

	private void LoadPrintDataFieldMaxLenSet()
	{
		ClsGetPrintDataFieldMaxLen ClsGPDFM = new ClsGetPrintDataFieldMaxLen();
		numericUpDownBore.Value = ClsGPDFM.iLen_Bore;
		numericUpDownBoreUnit.Value = ClsGPDFM.iLen_BoreUnit;
		numericUpDownBoreValue.Value = ClsGPDFM.iLen_BoreValue;
		numericUpDownCaseNo.Value = ClsGPDFM.iLen_CaseNo;
		numericUpDownCode.Value = ClsGPDFM.iLen_Code;
		numericUpDownCodeValue.Value = ClsGPDFM.iLen_CodeValue;
		numericUpDownHzUnit.Value = ClsGPDFM.iLen_HzUnit;
		numericUpDownHzValue.Value = ClsGPDFM.iLen_HzValue;
		numericUpDownKeycode.Value = ClsGPDFM.iLen_Keycode;
		numericUpDownModel.Value = ClsGPDFM.iLen_Model;
		numericUpDownModelValue.Value = ClsGPDFM.iLen_ModelValue;
		numericUpDownOutput.Value = ClsGPDFM.iLen_Output;
		numericUpDownOutputUnit.Value = ClsGPDFM.iLen_OutputUnit;
		numericUpDownOutputValue.Value = ClsGPDFM.iLen_OutputValue;
		numericUpDownPhaseUnit.Value = ClsGPDFM.iLen_PhaseUnit;
		numericUpDownPhaseValue.Value = ClsGPDFM.iLen_PhaseValue;
		numericUpDownPlace.Value = ClsGPDFM.iLen_Place;
		numericUpDownTokki.Value = ClsGPDFM.iLen_Tokki;
		numericUpDownVoltageUnit.Value = ClsGPDFM.iLen_VoltageUnit;
		numericUpDownVoltageValue.Value = ClsGPDFM.iLen_VoltageValue;
	}

	private void AlterControlPos()
	{
		int iLeft = btnSaveImportExportPath.Left;
		int iTop = btnSaveImportExportPath.Top;
		AlterBtnPos(btnSaveImportExportPath, iLeft, iTop);
		AlterBtnPos(btnModifyPWD, iLeft, iTop);
		AlterBtnPos(btnSetDataBase, iLeft, iTop);
		AlterBtnPos(btnSaveFormPos, iLeft, iTop);
		AlterBtnPos(btnFrontSideMsgPath, iLeft, iTop);
		AlterBtnPos(btnBackupResumeOK, iLeft, iTop);
		AlterBtnPos(btnSavePrintDataFieldMaxLen, iLeft, iTop);
	}

	private void AlterBtnPos(object sender, int iLeft, int iTop)
	{
		Button btnTmp = (Button)sender;
		btnTmp.Left = iLeft;
		btnTmp.Top = iTop;
	}

	private void textBoxOldPwd_TextChanged(object sender, EventArgs e)
	{
	}

	private void textBoxMainFormX_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!ClsShare.OnlyInputNum(e.KeyChar))
		{
			e.Handled = true;
		}
	}

	private void textBoxMainFormX_KeyUp(object sender, KeyEventArgs e)
	{
		if (((TextBox)sender).Text.Length == 0)
		{
			((TextBox)sender).Text = "0";
		}
	}

	private void textBoxMainFormWidth_KeyUp(object sender, KeyEventArgs e)
	{
		if (((TextBox)sender).Text.Length == 0)
		{
			((TextBox)sender).Text = "1";
		}
	}

	private void btnMsg1Path_Click(object sender, EventArgs e)
	{
		string strGetFileName = "";
		if (ClsFile.GetOpenDlgFileName("Select Msg", "All Files(*.*)|*.*|Msg(*.ms)|*.ms", 2, Application.StartupPath, ref strGetFileName))
		{
			if ((Button)sender == btnSourceMsg1Path)
			{
				textBoxSourceMsg1Path.Text = strGetFileName;
			}
			else if ((Button)sender == btnSourceMsg2Path)
			{
				textBoxSourceMsg2Path.Text = strGetFileName;
			}
		}
	}

	private void tabCtrlSysSet_Resize(object sender, EventArgs e)
	{
	}

	private void btnGetMainFormPos_Click(object sender, EventArgs e)
	{
		int iLeft = ((base.Parent.Parent.Parent.Left > 0) ? base.Parent.Parent.Parent.Left : 0);
		int iTop = ((base.Parent.Parent.Parent.Top > 0) ? base.Parent.Parent.Parent.Top : 0);
		textBoxMainFormX.Text = iLeft.ToString();
		textBoxMainFormY.Text = iTop.ToString();
		textBoxMainFormWidth.Text = base.Parent.Parent.Parent.Width.ToString();
		textBoxMainFormHeight.Text = base.Parent.Parent.Parent.Height.ToString();
	}

	private void textBoxMainFormX_TextChanged(object sender, EventArgs e)
	{
		if (!ClsShare.CheckStrIsSignNum(((TextBox)sender).Text.Trim()))
		{
			MessageBox.Show("輸入必須為整數!");
			((TextBox)sender).Text = "0";
		}
	}

	private void radioBtnSysBackup_CheckedChanged(object sender, EventArgs e)
	{
	}

	private void btnSaveImportExportPath_Click(object sender, EventArgs e)
	{
		SetPath();
	}

	private void btnModifyPWD_Click(object sender, EventArgs e)
	{
		SetSysPwd();
	}

	private void btnSaveFormPos_Click(object sender, EventArgs e)
	{
		SetForm();
	}

	private void btnSetDataBase_Click(object sender, EventArgs e)
	{
		SetDataBase();
	}

	private void btnFrontSideMsgPath_Click(object sender, EventArgs e)
	{
		SetGroupMsgParam();
	}

	private void btnBackupResumeOK_Click(object sender, EventArgs e)
	{
		SetSysBackupResume();
	}

	private void btnSavePrintDataFieldMaxLen_Click(object sender, EventArgs e)
	{
		SetPrintDataFieldMaxlen();
	}

	private void SetPrintDataFieldMaxlen()
	{
		try
		{
			bool bRlt1 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Bore, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownBore.Value.ToString());
			bool bRlt2 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_BoreUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownBoreUnit.Value.ToString());
			bool bRlt3 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_BoreValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownBoreValue.Value.ToString());
			bool bRlt4 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_CaseNo, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownCaseNo.Value.ToString());
			bool bRlt5 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Code, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownCode.Value.ToString());
			bool bRlt6 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_CodeValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownCodeValue.Value.ToString());
			bool bRlt7 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_HzUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownHzUnit.Value.ToString());
			bool bRlt8 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_HzValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownHzValue.Value.ToString());
			bool bRlt9 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_KeyCode, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownKeycode.Value.ToString());
			bool bRlt10 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Model, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownModel.Value.ToString());
			bool bRlt11 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_ModelValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownModelValue.Value.ToString());
			bool bRlt12 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Output, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownOutput.Value.ToString());
			bool bRlt13 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_OutputUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownOutputUnit.Value.ToString());
			bool bRlt14 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_OutputValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownOutputValue.Value.ToString());
			bool bRlt15 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_PhaseUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownPhaseUnit.Value.ToString());
			bool bRlt16 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_PhaseValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownPhaseValue.Value.ToString());
			bool bRlt17 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Place, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownPlace.Value.ToString());
			bool bRlt18 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Tokki, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownTokki.Value.ToString());
			bool bRlt19 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_VoltageUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownVoltageUnit.Value.ToString());
			bool bRlt20 = ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_VoltageValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, numericUpDownVoltageValue.Value.ToString());
			if (bRlt1 && bRlt2 && bRlt3 && bRlt4 && bRlt5 && bRlt6 && bRlt7 && bRlt8 && bRlt9 && bRlt10 && bRlt11 && bRlt12 && bRlt13 && bRlt14 && bRlt15 && bRlt16 && bRlt17 && bRlt18 && bRlt19 && bRlt20)
			{
				MessageBox.Show("保存成功");
			}
			else
			{
				MessageBox.Show("保存失敗");
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SetPrintDataFieldMaxlen exception:" + ex.Message);
			MessageBox.Show("保存失敗");
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
		this.btnLeave = new System.Windows.Forms.Button();
		this.btnSet = new System.Windows.Forms.Button();
		this.tabCtrlSysSet = new System.Windows.Forms.TabControl();
		this.tabPagePathSet = new System.Windows.Forms.TabPage();
		this.btnSaveImportExportPath = new System.Windows.Forms.Button();
		this.btnImportPath = new System.Windows.Forms.Button();
		this.textBoxImportPath = new System.Windows.Forms.TextBox();
		this.lblImportPath = new System.Windows.Forms.Label();
		this.btnExportPath = new System.Windows.Forms.Button();
		this.textBoxExportPath = new System.Windows.Forms.TextBox();
		this.lblExportPath = new System.Windows.Forms.Label();
		this.tabPagePwdSet = new System.Windows.Forms.TabPage();
		this.btnModifyPWD = new System.Windows.Forms.Button();
		this.textBoxSurePwd = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.textBoxNewPwd = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.textBoxOldPwd = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.tabPageDataBaseSet = new System.Windows.Forms.TabPage();
		this.btnSetDataBase = new System.Windows.Forms.Button();
		this.lblIniDB = new System.Windows.Forms.Label();
		this.textBoxDataBasePwd = new System.Windows.Forms.TextBox();
		this.label13 = new System.Windows.Forms.Label();
		this.textBoxDataBaseUserID = new System.Windows.Forms.TextBox();
		this.label12 = new System.Windows.Forms.Label();
		this.textBoxDataBaseName = new System.Windows.Forms.TextBox();
		this.label11 = new System.Windows.Forms.Label();
		this.textBoxServerName = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.tabPageFormSet = new System.Windows.Forms.TabPage();
		this.btnSaveFormPos = new System.Windows.Forms.Button();
		this.btnGetMainFormParam = new System.Windows.Forms.Button();
		this.textBoxMainFormHeight = new System.Windows.Forms.TextBox();
		this.label17 = new System.Windows.Forms.Label();
		this.textBoxMainFormWidth = new System.Windows.Forms.TextBox();
		this.label16 = new System.Windows.Forms.Label();
		this.textBoxMainFormY = new System.Windows.Forms.TextBox();
		this.label15 = new System.Windows.Forms.Label();
		this.textBoxMainFormX = new System.Windows.Forms.TextBox();
		this.label14 = new System.Windows.Forms.Label();
		this.tabPageGroupMsgSet = new System.Windows.Forms.TabPage();
		this.btnFrontSideMsgPath = new System.Windows.Forms.Button();
		this.label18 = new System.Windows.Forms.Label();
		this.textBoxMaxSinglePrintHeadHeight = new System.Windows.Forms.TextBox();
		this.label19 = new System.Windows.Forms.Label();
		this.textBoxOneMsgMaxPrintHeadCountOnGroupMsg = new System.Windows.Forms.TextBox();
		this.label20 = new System.Windows.Forms.Label();
		this.btnSourceMsg1Path = new System.Windows.Forms.Button();
		this.textBoxSourceMsg1Path = new System.Windows.Forms.TextBox();
		this.lblSrcMsg1 = new System.Windows.Forms.Label();
		this.btnSourceMsg2Path = new System.Windows.Forms.Button();
		this.textBoxSourceMsg2Path = new System.Windows.Forms.TextBox();
		this.lblSrcMsg2 = new System.Windows.Forms.Label();
		this.tabPagePrintDataFieldMaxLen = new System.Windows.Forms.TabPage();
		this.label6 = new System.Windows.Forms.Label();
		this.numericUpDownPlace = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownHzUnit = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownPhaseUnit = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownOutput = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownModelValue = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownTokki = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownHzValue = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownPhaseValue = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownBoreUnit = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownModel = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownCodeValue = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownVoltageUnit = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownOutputUnit = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownBoreValue = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownCaseNo = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownCode = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownVoltageValue = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownOutputValue = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownBore = new System.Windows.Forms.NumericUpDown();
		this.numericUpDownKeycode = new System.Windows.Forms.NumericUpDown();
		this.label29 = new System.Windows.Forms.Label();
		this.label30 = new System.Windows.Forms.Label();
		this.label31 = new System.Windows.Forms.Label();
		this.label32 = new System.Windows.Forms.Label();
		this.label33 = new System.Windows.Forms.Label();
		this.label34 = new System.Windows.Forms.Label();
		this.label35 = new System.Windows.Forms.Label();
		this.label36 = new System.Windows.Forms.Label();
		this.label37 = new System.Windows.Forms.Label();
		this.label38 = new System.Windows.Forms.Label();
		this.label24 = new System.Windows.Forms.Label();
		this.label25 = new System.Windows.Forms.Label();
		this.label26 = new System.Windows.Forms.Label();
		this.label27 = new System.Windows.Forms.Label();
		this.label28 = new System.Windows.Forms.Label();
		this.label23 = new System.Windows.Forms.Label();
		this.label22 = new System.Windows.Forms.Label();
		this.label21 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.btnSavePrintDataFieldMaxLen = new System.Windows.Forms.Button();
		this.tabPageSysBackupResume = new System.Windows.Forms.TabPage();
		this.btnBackupResumeOK = new System.Windows.Forms.Button();
		this.radioBtnSysResume = new System.Windows.Forms.RadioButton();
		this.radioBtnSysBackup = new System.Windows.Forms.RadioButton();
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		this.tabCtrlSysSet.SuspendLayout();
		this.tabPagePathSet.SuspendLayout();
		this.tabPagePwdSet.SuspendLayout();
		this.tabPageDataBaseSet.SuspendLayout();
		this.tabPageFormSet.SuspendLayout();
		this.tabPageGroupMsgSet.SuspendLayout();
		this.tabPagePrintDataFieldMaxLen.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownPlace).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownHzUnit).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownPhaseUnit).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownOutput).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownModelValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownTokki).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownHzValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownPhaseValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownBoreUnit).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownModel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownCodeValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownVoltageUnit).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownOutputUnit).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownBoreValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownCaseNo).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownCode).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownVoltageValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownOutputValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownBore).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownKeycode).BeginInit();
		this.tabPageSysBackupResume.SuspendLayout();
		base.SuspendLayout();
		base.pnlClientData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		base.pnlClientData.Controls.Add(this.tabCtrlSysSet);
		base.pnlClientData.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.pnlClientData.Size = new System.Drawing.Size(936, 454);
		base.pnlBottomCtrl.Controls.Add(this.btnLeave);
		base.pnlBottomCtrl.Controls.Add(this.btnSet);
		base.pnlBottomCtrl.Location = new System.Drawing.Point(0, 454);
		base.pnlBottomCtrl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.pnlBottomCtrl.Size = new System.Drawing.Size(936, 115);
		base.pnlBottomCtrl.Visible = false;
		this.btnLeave.Location = new System.Drawing.Point(288, 45);
		this.btnLeave.Name = "btnLeave";
		this.btnLeave.Size = new System.Drawing.Size(106, 33);
		this.btnLeave.TabIndex = 12;
		this.btnLeave.Text = "離開";
		this.btnLeave.UseVisualStyleBackColor = true;
		this.btnLeave.Visible = false;
		this.btnLeave.Click += new System.EventHandler(btnLeave_Click);
		this.btnSet.Location = new System.Drawing.Point(93, 45);
		this.btnSet.Name = "btnSet";
		this.btnSet.Size = new System.Drawing.Size(106, 33);
		this.btnSet.TabIndex = 11;
		this.btnSet.Text = "設置";
		this.btnSet.UseVisualStyleBackColor = true;
		this.btnSet.Click += new System.EventHandler(btnSet_Click);
		this.tabCtrlSysSet.Controls.Add(this.tabPagePathSet);
		this.tabCtrlSysSet.Controls.Add(this.tabPagePwdSet);
		this.tabCtrlSysSet.Controls.Add(this.tabPageDataBaseSet);
		this.tabCtrlSysSet.Controls.Add(this.tabPageFormSet);
		this.tabCtrlSysSet.Controls.Add(this.tabPageGroupMsgSet);
		this.tabCtrlSysSet.Controls.Add(this.tabPagePrintDataFieldMaxLen);
		this.tabCtrlSysSet.Controls.Add(this.tabPageSysBackupResume);
		this.tabCtrlSysSet.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabCtrlSysSet.Location = new System.Drawing.Point(0, 0);
		this.tabCtrlSysSet.Name = "tabCtrlSysSet";
		this.tabCtrlSysSet.SelectedIndex = 0;
		this.tabCtrlSysSet.Size = new System.Drawing.Size(934, 452);
		this.tabCtrlSysSet.TabIndex = 0;
		this.tabCtrlSysSet.Resize += new System.EventHandler(tabCtrlSysSet_Resize);
		this.tabPagePathSet.Controls.Add(this.btnSaveImportExportPath);
		this.tabPagePathSet.Controls.Add(this.btnImportPath);
		this.tabPagePathSet.Controls.Add(this.textBoxImportPath);
		this.tabPagePathSet.Controls.Add(this.lblImportPath);
		this.tabPagePathSet.Controls.Add(this.btnExportPath);
		this.tabPagePathSet.Controls.Add(this.textBoxExportPath);
		this.tabPagePathSet.Controls.Add(this.lblExportPath);
		this.tabPagePathSet.Location = new System.Drawing.Point(4, 22);
		this.tabPagePathSet.Name = "tabPagePathSet";
		this.tabPagePathSet.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
		this.tabPagePathSet.Size = new System.Drawing.Size(926, 426);
		this.tabPagePathSet.TabIndex = 0;
		this.tabPagePathSet.Text = "導入/導出路徑";
		this.tabPagePathSet.UseVisualStyleBackColor = true;
		this.btnSaveImportExportPath.Location = new System.Drawing.Point(90, 346);
		this.btnSaveImportExportPath.Name = "btnSaveImportExportPath";
		this.btnSaveImportExportPath.Size = new System.Drawing.Size(106, 33);
		this.btnSaveImportExportPath.TabIndex = 28;
		this.btnSaveImportExportPath.Text = "保存";
		this.btnSaveImportExportPath.UseVisualStyleBackColor = true;
		this.btnSaveImportExportPath.Click += new System.EventHandler(btnSaveImportExportPath_Click);
		this.btnImportPath.Location = new System.Drawing.Point(623, 34);
		this.btnImportPath.Name = "btnImportPath";
		this.btnImportPath.Size = new System.Drawing.Size(50, 23);
		this.btnImportPath.TabIndex = 27;
		this.btnImportPath.Text = "...";
		this.btnImportPath.UseVisualStyleBackColor = true;
		this.btnImportPath.Click += new System.EventHandler(btnSelModelPath_Click);
		this.textBoxImportPath.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxImportPath.Location = new System.Drawing.Point(94, 35);
		this.textBoxImportPath.Name = "textBoxImportPath";
		this.textBoxImportPath.ReadOnly = true;
		this.textBoxImportPath.Size = new System.Drawing.Size(528, 21);
		this.textBoxImportPath.TabIndex = 26;
		this.textBoxImportPath.Text = "導入路徑";
		this.lblImportPath.AutoSize = true;
		this.lblImportPath.BackColor = System.Drawing.Color.Transparent;
		this.lblImportPath.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblImportPath.Location = new System.Drawing.Point(6, 38);
		this.lblImportPath.Name = "lblImportPath";
		this.lblImportPath.Size = new System.Drawing.Size(75, 14);
		this.lblImportPath.TabIndex = 25;
		this.lblImportPath.Text = "導入路徑:";
		this.btnExportPath.Location = new System.Drawing.Point(623, 105);
		this.btnExportPath.Name = "btnExportPath";
		this.btnExportPath.Size = new System.Drawing.Size(50, 23);
		this.btnExportPath.TabIndex = 24;
		this.btnExportPath.Text = "...";
		this.btnExportPath.UseVisualStyleBackColor = true;
		this.btnExportPath.Click += new System.EventHandler(btnSelModelPath_Click);
		this.textBoxExportPath.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxExportPath.Location = new System.Drawing.Point(94, 106);
		this.textBoxExportPath.Name = "textBoxExportPath";
		this.textBoxExportPath.ReadOnly = true;
		this.textBoxExportPath.Size = new System.Drawing.Size(528, 21);
		this.textBoxExportPath.TabIndex = 23;
		this.textBoxExportPath.Text = "導出路徑";
		this.lblExportPath.AutoSize = true;
		this.lblExportPath.BackColor = System.Drawing.Color.Transparent;
		this.lblExportPath.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblExportPath.Location = new System.Drawing.Point(6, 109);
		this.lblExportPath.Name = "lblExportPath";
		this.lblExportPath.Size = new System.Drawing.Size(75, 14);
		this.lblExportPath.TabIndex = 22;
		this.lblExportPath.Text = "導出路徑:";
		this.tabPagePwdSet.Controls.Add(this.btnModifyPWD);
		this.tabPagePwdSet.Controls.Add(this.textBoxSurePwd);
		this.tabPagePwdSet.Controls.Add(this.label3);
		this.tabPagePwdSet.Controls.Add(this.textBoxNewPwd);
		this.tabPagePwdSet.Controls.Add(this.label2);
		this.tabPagePwdSet.Controls.Add(this.textBoxOldPwd);
		this.tabPagePwdSet.Controls.Add(this.label1);
		this.tabPagePwdSet.Location = new System.Drawing.Point(4, 22);
		this.tabPagePwdSet.Name = "tabPagePwdSet";
		this.tabPagePwdSet.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
		this.tabPagePwdSet.Size = new System.Drawing.Size(927, 428);
		this.tabPagePwdSet.TabIndex = 2;
		this.tabPagePwdSet.Text = "密碼管理";
		this.tabPagePwdSet.UseVisualStyleBackColor = true;
		this.btnModifyPWD.Location = new System.Drawing.Point(90, 342);
		this.btnModifyPWD.Name = "btnModifyPWD";
		this.btnModifyPWD.Size = new System.Drawing.Size(106, 33);
		this.btnModifyPWD.TabIndex = 29;
		this.btnModifyPWD.Text = "修改";
		this.btnModifyPWD.UseVisualStyleBackColor = true;
		this.btnModifyPWD.Click += new System.EventHandler(btnModifyPWD_Click);
		this.textBoxSurePwd.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxSurePwd.Location = new System.Drawing.Point(105, 130);
		this.textBoxSurePwd.Name = "textBoxSurePwd";
		this.textBoxSurePwd.PasswordChar = '*';
		this.textBoxSurePwd.Size = new System.Drawing.Size(528, 21);
		this.textBoxSurePwd.TabIndex = 26;
		this.textBoxSurePwd.TextChanged += new System.EventHandler(textBoxOldPwd_TextChanged);
		this.textBoxSurePwd.Leave += new System.EventHandler(textBoxSurePwd_Leave);
		this.label3.AutoSize = true;
		this.label3.BackColor = System.Drawing.Color.Transparent;
		this.label3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label3.Location = new System.Drawing.Point(17, 133);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(75, 14);
		this.label3.TabIndex = 25;
		this.label3.Text = "確認密碼:";
		this.textBoxNewPwd.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxNewPwd.Location = new System.Drawing.Point(105, 75);
		this.textBoxNewPwd.Name = "textBoxNewPwd";
		this.textBoxNewPwd.PasswordChar = '*';
		this.textBoxNewPwd.Size = new System.Drawing.Size(528, 21);
		this.textBoxNewPwd.TabIndex = 24;
		this.textBoxNewPwd.TextChanged += new System.EventHandler(textBoxOldPwd_TextChanged);
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.Transparent;
		this.label2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label2.Location = new System.Drawing.Point(17, 78);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(60, 14);
		this.label2.TabIndex = 23;
		this.label2.Text = "新密碼:";
		this.textBoxOldPwd.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxOldPwd.Location = new System.Drawing.Point(105, 26);
		this.textBoxOldPwd.Name = "textBoxOldPwd";
		this.textBoxOldPwd.PasswordChar = '*';
		this.textBoxOldPwd.Size = new System.Drawing.Size(528, 21);
		this.textBoxOldPwd.TabIndex = 22;
		this.textBoxOldPwd.TextChanged += new System.EventHandler(textBoxOldPwd_TextChanged);
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.Transparent;
		this.label1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label1.Location = new System.Drawing.Point(17, 29);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(60, 14);
		this.label1.TabIndex = 21;
		this.label1.Text = "舊密碼:";
		this.tabPageDataBaseSet.Controls.Add(this.btnSetDataBase);
		this.tabPageDataBaseSet.Controls.Add(this.lblIniDB);
		this.tabPageDataBaseSet.Controls.Add(this.textBoxDataBasePwd);
		this.tabPageDataBaseSet.Controls.Add(this.label13);
		this.tabPageDataBaseSet.Controls.Add(this.textBoxDataBaseUserID);
		this.tabPageDataBaseSet.Controls.Add(this.label12);
		this.tabPageDataBaseSet.Controls.Add(this.textBoxDataBaseName);
		this.tabPageDataBaseSet.Controls.Add(this.label11);
		this.tabPageDataBaseSet.Controls.Add(this.textBoxServerName);
		this.tabPageDataBaseSet.Controls.Add(this.label10);
		this.tabPageDataBaseSet.Location = new System.Drawing.Point(4, 22);
		this.tabPageDataBaseSet.Name = "tabPageDataBaseSet";
		this.tabPageDataBaseSet.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
		this.tabPageDataBaseSet.Size = new System.Drawing.Size(927, 428);
		this.tabPageDataBaseSet.TabIndex = 3;
		this.tabPageDataBaseSet.Text = "伺服器";
		this.tabPageDataBaseSet.UseVisualStyleBackColor = true;
		this.btnSetDataBase.Location = new System.Drawing.Point(90, 340);
		this.btnSetDataBase.Name = "btnSetDataBase";
		this.btnSetDataBase.Size = new System.Drawing.Size(106, 33);
		this.btnSetDataBase.TabIndex = 36;
		this.btnSetDataBase.Text = "設置";
		this.btnSetDataBase.UseVisualStyleBackColor = true;
		this.btnSetDataBase.Click += new System.EventHandler(btnSetDataBase_Click);
		this.lblIniDB.AutoSize = true;
		this.lblIniDB.Font = new System.Drawing.Font("宋体", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblIniDB.Location = new System.Drawing.Point(326, 221);
		this.lblIniDB.Name = "lblIniDB";
		this.lblIniDB.Size = new System.Drawing.Size(112, 21);
		this.lblIniDB.TabIndex = 35;
		this.lblIniDB.Text = "設置中...";
		this.lblIniDB.Visible = false;
		this.textBoxDataBasePwd.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxDataBasePwd.Location = new System.Drawing.Point(116, 158);
		this.textBoxDataBasePwd.Name = "textBoxDataBasePwd";
		this.textBoxDataBasePwd.PasswordChar = '*';
		this.textBoxDataBasePwd.Size = new System.Drawing.Size(528, 21);
		this.textBoxDataBasePwd.TabIndex = 34;
		this.textBoxDataBasePwd.Text = "密碼";
		this.label13.AutoSize = true;
		this.label13.BackColor = System.Drawing.Color.Transparent;
		this.label13.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label13.Location = new System.Drawing.Point(28, 161);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(45, 14);
		this.label13.TabIndex = 33;
		this.label13.Text = "密碼:";
		this.textBoxDataBaseUserID.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxDataBaseUserID.Location = new System.Drawing.Point(116, 113);
		this.textBoxDataBaseUserID.Name = "textBoxDataBaseUserID";
		this.textBoxDataBaseUserID.Size = new System.Drawing.Size(528, 21);
		this.textBoxDataBaseUserID.TabIndex = 32;
		this.textBoxDataBaseUserID.Text = "用戶名";
		this.label12.AutoSize = true;
		this.label12.BackColor = System.Drawing.Color.Transparent;
		this.label12.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label12.Location = new System.Drawing.Point(28, 116);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(67, 14);
		this.label12.TabIndex = 31;
		this.label12.Text = "用戶名：";
		this.textBoxDataBaseName.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxDataBaseName.Location = new System.Drawing.Point(116, 68);
		this.textBoxDataBaseName.Name = "textBoxDataBaseName";
		this.textBoxDataBaseName.Size = new System.Drawing.Size(528, 21);
		this.textBoxDataBaseName.TabIndex = 30;
		this.textBoxDataBaseName.Text = "數據庫";
		this.label11.AutoSize = true;
		this.label11.BackColor = System.Drawing.Color.Transparent;
		this.label11.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label11.Location = new System.Drawing.Point(28, 71);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(60, 14);
		this.label11.TabIndex = 29;
		this.label11.Text = "资料庫:";
		this.textBoxServerName.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxServerName.Location = new System.Drawing.Point(116, 23);
		this.textBoxServerName.Name = "textBoxServerName";
		this.textBoxServerName.Size = new System.Drawing.Size(528, 21);
		this.textBoxServerName.TabIndex = 28;
		this.textBoxServerName.Text = "服務器";
		this.label10.AutoSize = true;
		this.label10.BackColor = System.Drawing.Color.Transparent;
		this.label10.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label10.Location = new System.Drawing.Point(28, 26);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(60, 14);
		this.label10.TabIndex = 27;
		this.label10.Text = "服務器:";
		this.tabPageFormSet.Controls.Add(this.btnSaveFormPos);
		this.tabPageFormSet.Controls.Add(this.btnGetMainFormParam);
		this.tabPageFormSet.Controls.Add(this.textBoxMainFormHeight);
		this.tabPageFormSet.Controls.Add(this.label17);
		this.tabPageFormSet.Controls.Add(this.textBoxMainFormWidth);
		this.tabPageFormSet.Controls.Add(this.label16);
		this.tabPageFormSet.Controls.Add(this.textBoxMainFormY);
		this.tabPageFormSet.Controls.Add(this.label15);
		this.tabPageFormSet.Controls.Add(this.textBoxMainFormX);
		this.tabPageFormSet.Controls.Add(this.label14);
		this.tabPageFormSet.Location = new System.Drawing.Point(4, 22);
		this.tabPageFormSet.Name = "tabPageFormSet";
		this.tabPageFormSet.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
		this.tabPageFormSet.Size = new System.Drawing.Size(927, 428);
		this.tabPageFormSet.TabIndex = 4;
		this.tabPageFormSet.Text = "窗體位置與大小";
		this.tabPageFormSet.UseVisualStyleBackColor = true;
		this.btnSaveFormPos.Location = new System.Drawing.Point(90, 336);
		this.btnSaveFormPos.Name = "btnSaveFormPos";
		this.btnSaveFormPos.Size = new System.Drawing.Size(106, 33);
		this.btnSaveFormPos.TabIndex = 37;
		this.btnSaveFormPos.Text = "保存";
		this.btnSaveFormPos.UseVisualStyleBackColor = true;
		this.btnSaveFormPos.Click += new System.EventHandler(btnSaveFormPos_Click);
		this.btnGetMainFormParam.Location = new System.Drawing.Point(208, 87);
		this.btnGetMainFormParam.Name = "btnGetMainFormParam";
		this.btnGetMainFormParam.Size = new System.Drawing.Size(152, 39);
		this.btnGetMainFormParam.TabIndex = 14;
		this.btnGetMainFormParam.Text = "自動填入窗體參數";
		this.btnGetMainFormParam.UseVisualStyleBackColor = true;
		this.btnGetMainFormParam.Click += new System.EventHandler(btnGetMainFormPos_Click);
		this.textBoxMainFormHeight.BackColor = System.Drawing.Color.White;
		this.textBoxMainFormHeight.Location = new System.Drawing.Point(90, 166);
		this.textBoxMainFormHeight.MaxLength = 6;
		this.textBoxMainFormHeight.Name = "textBoxMainFormHeight";
		this.textBoxMainFormHeight.Size = new System.Drawing.Size(58, 21);
		this.textBoxMainFormHeight.TabIndex = 13;
		this.textBoxMainFormHeight.Text = "1";
		this.textBoxMainFormHeight.TextChanged += new System.EventHandler(textBoxMainFormX_TextChanged);
		this.textBoxMainFormHeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxMainFormX_KeyPress);
		this.textBoxMainFormHeight.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxMainFormWidth_KeyUp);
		this.label17.AutoSize = true;
		this.label17.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label17.Location = new System.Drawing.Point(39, 169);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(30, 14);
		this.label17.TabIndex = 12;
		this.label17.Text = "高:";
		this.textBoxMainFormWidth.BackColor = System.Drawing.Color.White;
		this.textBoxMainFormWidth.Location = new System.Drawing.Point(90, 119);
		this.textBoxMainFormWidth.MaxLength = 6;
		this.textBoxMainFormWidth.Name = "textBoxMainFormWidth";
		this.textBoxMainFormWidth.Size = new System.Drawing.Size(58, 21);
		this.textBoxMainFormWidth.TabIndex = 11;
		this.textBoxMainFormWidth.Text = "1";
		this.textBoxMainFormWidth.TextChanged += new System.EventHandler(textBoxMainFormX_TextChanged);
		this.textBoxMainFormWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxMainFormX_KeyPress);
		this.textBoxMainFormWidth.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxMainFormWidth_KeyUp);
		this.label16.AutoSize = true;
		this.label16.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label16.Location = new System.Drawing.Point(39, 122);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(30, 14);
		this.label16.TabIndex = 10;
		this.label16.Text = "寬:";
		this.textBoxMainFormY.BackColor = System.Drawing.Color.White;
		this.textBoxMainFormY.Location = new System.Drawing.Point(90, 69);
		this.textBoxMainFormY.MaxLength = 6;
		this.textBoxMainFormY.Name = "textBoxMainFormY";
		this.textBoxMainFormY.Size = new System.Drawing.Size(58, 21);
		this.textBoxMainFormY.TabIndex = 9;
		this.textBoxMainFormY.Text = "1";
		this.textBoxMainFormY.TextChanged += new System.EventHandler(textBoxMainFormX_TextChanged);
		this.textBoxMainFormY.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxMainFormX_KeyPress);
		this.textBoxMainFormY.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxMainFormX_KeyUp);
		this.label15.AutoSize = true;
		this.label15.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label15.Location = new System.Drawing.Point(39, 72);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(23, 14);
		this.label15.TabIndex = 8;
		this.label15.Text = "Y:";
		this.textBoxMainFormX.BackColor = System.Drawing.Color.White;
		this.textBoxMainFormX.Location = new System.Drawing.Point(90, 25);
		this.textBoxMainFormX.MaxLength = 6;
		this.textBoxMainFormX.Name = "textBoxMainFormX";
		this.textBoxMainFormX.Size = new System.Drawing.Size(58, 21);
		this.textBoxMainFormX.TabIndex = 7;
		this.textBoxMainFormX.Text = "1";
		this.textBoxMainFormX.TextChanged += new System.EventHandler(textBoxMainFormX_TextChanged);
		this.textBoxMainFormX.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxMainFormX_KeyPress);
		this.textBoxMainFormX.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxMainFormX_KeyUp);
		this.label14.AutoSize = true;
		this.label14.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label14.Location = new System.Drawing.Point(39, 28);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(23, 14);
		this.label14.TabIndex = 6;
		this.label14.Text = "X:";
		this.tabPageGroupMsgSet.Controls.Add(this.btnFrontSideMsgPath);
		this.tabPageGroupMsgSet.Controls.Add(this.label18);
		this.tabPageGroupMsgSet.Controls.Add(this.textBoxMaxSinglePrintHeadHeight);
		this.tabPageGroupMsgSet.Controls.Add(this.label19);
		this.tabPageGroupMsgSet.Controls.Add(this.textBoxOneMsgMaxPrintHeadCountOnGroupMsg);
		this.tabPageGroupMsgSet.Controls.Add(this.label20);
		this.tabPageGroupMsgSet.Controls.Add(this.btnSourceMsg1Path);
		this.tabPageGroupMsgSet.Controls.Add(this.textBoxSourceMsg1Path);
		this.tabPageGroupMsgSet.Controls.Add(this.lblSrcMsg1);
		this.tabPageGroupMsgSet.Controls.Add(this.btnSourceMsg2Path);
		this.tabPageGroupMsgSet.Controls.Add(this.textBoxSourceMsg2Path);
		this.tabPageGroupMsgSet.Controls.Add(this.lblSrcMsg2);
		this.tabPageGroupMsgSet.Location = new System.Drawing.Point(4, 22);
		this.tabPageGroupMsgSet.Name = "tabPageGroupMsgSet";
		this.tabPageGroupMsgSet.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
		this.tabPageGroupMsgSet.Size = new System.Drawing.Size(927, 428);
		this.tabPageGroupMsgSet.TabIndex = 5;
		this.tabPageGroupMsgSet.Text = "正嘜/側嘜模板路徑";
		this.tabPageGroupMsgSet.UseVisualStyleBackColor = true;
		this.btnFrontSideMsgPath.Location = new System.Drawing.Point(90, 339);
		this.btnFrontSideMsgPath.Name = "btnFrontSideMsgPath";
		this.btnFrontSideMsgPath.Size = new System.Drawing.Size(106, 33);
		this.btnFrontSideMsgPath.TabIndex = 82;
		this.btnFrontSideMsgPath.Text = "保存";
		this.btnFrontSideMsgPath.UseVisualStyleBackColor = true;
		this.btnFrontSideMsgPath.Click += new System.EventHandler(btnFrontSideMsgPath_Click);
		this.label18.AutoSize = true;
		this.label18.BackColor = System.Drawing.Color.Transparent;
		this.label18.Font = new System.Drawing.Font("宋体", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label18.Location = new System.Drawing.Point(298, 176);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(42, 16);
		this.label18.TabIndex = 81;
		this.label18.Text = "像素";
		this.label18.Visible = false;
		this.textBoxMaxSinglePrintHeadHeight.Location = new System.Drawing.Point(215, 175);
		this.textBoxMaxSinglePrintHeadHeight.MaxLength = 8;
		this.textBoxMaxSinglePrintHeadHeight.Name = "textBoxMaxSinglePrintHeadHeight";
		this.textBoxMaxSinglePrintHeadHeight.Size = new System.Drawing.Size(77, 21);
		this.textBoxMaxSinglePrintHeadHeight.TabIndex = 80;
		this.textBoxMaxSinglePrintHeadHeight.Text = "150";
		this.textBoxMaxSinglePrintHeadHeight.Visible = false;
		this.label19.AutoSize = true;
		this.label19.BackColor = System.Drawing.Color.Transparent;
		this.label19.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label19.Location = new System.Drawing.Point(12, 178);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(150, 14);
		this.label19.TabIndex = 79;
		this.label19.Text = "單個噴頭的最大高度:";
		this.label19.Visible = false;
		this.textBoxOneMsgMaxPrintHeadCountOnGroupMsg.Location = new System.Drawing.Point(215, 139);
		this.textBoxOneMsgMaxPrintHeadCountOnGroupMsg.MaxLength = 8;
		this.textBoxOneMsgMaxPrintHeadCountOnGroupMsg.Name = "textBoxOneMsgMaxPrintHeadCountOnGroupMsg";
		this.textBoxOneMsgMaxPrintHeadCountOnGroupMsg.Size = new System.Drawing.Size(77, 21);
		this.textBoxOneMsgMaxPrintHeadCountOnGroupMsg.TabIndex = 78;
		this.textBoxOneMsgMaxPrintHeadCountOnGroupMsg.Text = "8";
		this.textBoxOneMsgMaxPrintHeadCountOnGroupMsg.Visible = false;
		this.label20.AutoSize = true;
		this.label20.BackColor = System.Drawing.Color.Transparent;
		this.label20.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label20.Location = new System.Drawing.Point(12, 143);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(202, 14);
		this.label20.TabIndex = 77;
		this.label20.Text = "單面模板所需的最大噴頭數：";
		this.label20.Visible = false;
		this.btnSourceMsg1Path.Location = new System.Drawing.Point(715, 32);
		this.btnSourceMsg1Path.Name = "btnSourceMsg1Path";
		this.btnSourceMsg1Path.Size = new System.Drawing.Size(48, 23);
		this.btnSourceMsg1Path.TabIndex = 65;
		this.btnSourceMsg1Path.Text = "...";
		this.btnSourceMsg1Path.UseVisualStyleBackColor = true;
		this.btnSourceMsg1Path.Click += new System.EventHandler(btnMsg1Path_Click);
		this.textBoxSourceMsg1Path.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxSourceMsg1Path.Location = new System.Drawing.Point(90, 33);
		this.textBoxSourceMsg1Path.Name = "textBoxSourceMsg1Path";
		this.textBoxSourceMsg1Path.ReadOnly = true;
		this.textBoxSourceMsg1Path.Size = new System.Drawing.Size(625, 21);
		this.textBoxSourceMsg1Path.TabIndex = 64;
		this.lblSrcMsg1.AutoSize = true;
		this.lblSrcMsg1.BackColor = System.Drawing.Color.Transparent;
		this.lblSrcMsg1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSrcMsg1.Location = new System.Drawing.Point(12, 36);
		this.lblSrcMsg1.Name = "lblSrcMsg1";
		this.lblSrcMsg1.Size = new System.Drawing.Size(75, 14);
		this.lblSrcMsg1.TabIndex = 63;
		this.lblSrcMsg1.Text = "正嘜模板:";
		this.btnSourceMsg2Path.Location = new System.Drawing.Point(715, 79);
		this.btnSourceMsg2Path.Name = "btnSourceMsg2Path";
		this.btnSourceMsg2Path.Size = new System.Drawing.Size(48, 23);
		this.btnSourceMsg2Path.TabIndex = 62;
		this.btnSourceMsg2Path.Text = "...";
		this.btnSourceMsg2Path.UseVisualStyleBackColor = true;
		this.btnSourceMsg2Path.Click += new System.EventHandler(btnMsg1Path_Click);
		this.textBoxSourceMsg2Path.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxSourceMsg2Path.Location = new System.Drawing.Point(90, 80);
		this.textBoxSourceMsg2Path.Name = "textBoxSourceMsg2Path";
		this.textBoxSourceMsg2Path.ReadOnly = true;
		this.textBoxSourceMsg2Path.Size = new System.Drawing.Size(625, 21);
		this.textBoxSourceMsg2Path.TabIndex = 61;
		this.lblSrcMsg2.AutoSize = true;
		this.lblSrcMsg2.BackColor = System.Drawing.Color.Transparent;
		this.lblSrcMsg2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSrcMsg2.Location = new System.Drawing.Point(12, 83);
		this.lblSrcMsg2.Name = "lblSrcMsg2";
		this.lblSrcMsg2.Size = new System.Drawing.Size(75, 14);
		this.lblSrcMsg2.TabIndex = 60;
		this.lblSrcMsg2.Text = "側嘜模板:";
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label6);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownPlace);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownHzUnit);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownPhaseUnit);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownOutput);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownModelValue);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownTokki);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownHzValue);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownPhaseValue);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownBoreUnit);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownModel);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownCodeValue);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownVoltageUnit);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownOutputUnit);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownBoreValue);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownCaseNo);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownCode);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownVoltageValue);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownOutputValue);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownBore);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.numericUpDownKeycode);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label29);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label30);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label31);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label32);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label33);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label34);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label35);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label36);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label37);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label38);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label24);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label25);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label26);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label27);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label28);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label23);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label22);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label21);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label5);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.label4);
		this.tabPagePrintDataFieldMaxLen.Controls.Add(this.btnSavePrintDataFieldMaxLen);
		this.tabPagePrintDataFieldMaxLen.Location = new System.Drawing.Point(4, 22);
		this.tabPagePrintDataFieldMaxLen.Name = "tabPagePrintDataFieldMaxLen";
		this.tabPagePrintDataFieldMaxLen.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
		this.tabPagePrintDataFieldMaxLen.Size = new System.Drawing.Size(927, 428);
		this.tabPagePrintDataFieldMaxLen.TabIndex = 7;
		this.tabPagePrintDataFieldMaxLen.Text = "噴印資料欄位最大長度";
		this.tabPagePrintDataFieldMaxLen.UseVisualStyleBackColor = true;
		this.label6.AutoSize = true;
		this.label6.BackColor = System.Drawing.Color.Transparent;
		this.label6.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label6.Location = new System.Drawing.Point(27, 21);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(75, 14);
		this.label6.TabIndex = 90;
		this.label6.Text = "單位:字符";
		this.numericUpDownPlace.Location = new System.Drawing.Point(803, 268);
		this.numericUpDownPlace.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownPlace.Name = "numericUpDownPlace";
		this.numericUpDownPlace.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownPlace.TabIndex = 89;
		this.numericUpDownPlace.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownHzUnit.Location = new System.Drawing.Point(803, 215);
		this.numericUpDownHzUnit.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownHzUnit.Name = "numericUpDownHzUnit";
		this.numericUpDownHzUnit.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownHzUnit.TabIndex = 88;
		this.numericUpDownHzUnit.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownPhaseUnit.Location = new System.Drawing.Point(803, 162);
		this.numericUpDownPhaseUnit.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownPhaseUnit.Name = "numericUpDownPhaseUnit";
		this.numericUpDownPhaseUnit.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownPhaseUnit.TabIndex = 87;
		this.numericUpDownPhaseUnit.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownOutput.Location = new System.Drawing.Point(802, 109);
		this.numericUpDownOutput.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownOutput.Name = "numericUpDownOutput";
		this.numericUpDownOutput.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownOutput.TabIndex = 86;
		this.numericUpDownOutput.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownModelValue.Location = new System.Drawing.Point(802, 56);
		this.numericUpDownModelValue.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownModelValue.Name = "numericUpDownModelValue";
		this.numericUpDownModelValue.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownModelValue.TabIndex = 85;
		this.numericUpDownModelValue.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownTokki.Location = new System.Drawing.Point(585, 268);
		this.numericUpDownTokki.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownTokki.Name = "numericUpDownTokki";
		this.numericUpDownTokki.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownTokki.TabIndex = 84;
		this.numericUpDownTokki.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownHzValue.Location = new System.Drawing.Point(585, 215);
		this.numericUpDownHzValue.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownHzValue.Name = "numericUpDownHzValue";
		this.numericUpDownHzValue.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownHzValue.TabIndex = 83;
		this.numericUpDownHzValue.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownPhaseValue.Location = new System.Drawing.Point(585, 162);
		this.numericUpDownPhaseValue.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownPhaseValue.Name = "numericUpDownPhaseValue";
		this.numericUpDownPhaseValue.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownPhaseValue.TabIndex = 82;
		this.numericUpDownPhaseValue.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownBoreUnit.Location = new System.Drawing.Point(584, 109);
		this.numericUpDownBoreUnit.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownBoreUnit.Name = "numericUpDownBoreUnit";
		this.numericUpDownBoreUnit.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownBoreUnit.TabIndex = 81;
		this.numericUpDownBoreUnit.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownModel.Location = new System.Drawing.Point(584, 56);
		this.numericUpDownModel.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownModel.Name = "numericUpDownModel";
		this.numericUpDownModel.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownModel.TabIndex = 80;
		this.numericUpDownModel.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownCodeValue.Location = new System.Drawing.Point(367, 268);
		this.numericUpDownCodeValue.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownCodeValue.Name = "numericUpDownCodeValue";
		this.numericUpDownCodeValue.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownCodeValue.TabIndex = 79;
		this.numericUpDownCodeValue.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownVoltageUnit.Location = new System.Drawing.Point(367, 215);
		this.numericUpDownVoltageUnit.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownVoltageUnit.Name = "numericUpDownVoltageUnit";
		this.numericUpDownVoltageUnit.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownVoltageUnit.TabIndex = 78;
		this.numericUpDownVoltageUnit.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownOutputUnit.Location = new System.Drawing.Point(367, 162);
		this.numericUpDownOutputUnit.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownOutputUnit.Name = "numericUpDownOutputUnit";
		this.numericUpDownOutputUnit.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownOutputUnit.TabIndex = 77;
		this.numericUpDownOutputUnit.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownBoreValue.Location = new System.Drawing.Point(366, 109);
		this.numericUpDownBoreValue.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownBoreValue.Name = "numericUpDownBoreValue";
		this.numericUpDownBoreValue.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownBoreValue.TabIndex = 76;
		this.numericUpDownBoreValue.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownCaseNo.Location = new System.Drawing.Point(366, 56);
		this.numericUpDownCaseNo.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownCaseNo.Name = "numericUpDownCaseNo";
		this.numericUpDownCaseNo.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownCaseNo.TabIndex = 75;
		this.numericUpDownCaseNo.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownCode.Location = new System.Drawing.Point(152, 268);
		this.numericUpDownCode.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownCode.Name = "numericUpDownCode";
		this.numericUpDownCode.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownCode.TabIndex = 74;
		this.numericUpDownCode.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownVoltageValue.Location = new System.Drawing.Point(152, 215);
		this.numericUpDownVoltageValue.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownVoltageValue.Name = "numericUpDownVoltageValue";
		this.numericUpDownVoltageValue.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownVoltageValue.TabIndex = 73;
		this.numericUpDownVoltageValue.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownOutputValue.Location = new System.Drawing.Point(152, 162);
		this.numericUpDownOutputValue.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownOutputValue.Name = "numericUpDownOutputValue";
		this.numericUpDownOutputValue.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownOutputValue.TabIndex = 72;
		this.numericUpDownOutputValue.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownBore.Location = new System.Drawing.Point(151, 109);
		this.numericUpDownBore.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownBore.Name = "numericUpDownBore";
		this.numericUpDownBore.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownBore.TabIndex = 71;
		this.numericUpDownBore.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownKeycode.Location = new System.Drawing.Point(151, 56);
		this.numericUpDownKeycode.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownKeycode.Name = "numericUpDownKeycode";
		this.numericUpDownKeycode.Size = new System.Drawing.Size(71, 21);
		this.numericUpDownKeycode.TabIndex = 70;
		this.numericUpDownKeycode.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.label29.AutoSize = true;
		this.label29.BackColor = System.Drawing.Color.Transparent;
		this.label29.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label29.Location = new System.Drawing.Point(693, 271);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(55, 14);
		this.label29.TabIndex = 68;
		this.label29.Text = "PLACE:";
		this.label30.AutoSize = true;
		this.label30.BackColor = System.Drawing.Color.Transparent;
		this.label30.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label30.Location = new System.Drawing.Point(477, 271);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(55, 14);
		this.label30.TabIndex = 66;
		this.label30.Text = "TOKKI:";
		this.label31.AutoSize = true;
		this.label31.BackColor = System.Drawing.Color.Transparent;
		this.label31.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label31.Location = new System.Drawing.Point(250, 271);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(95, 14);
		this.label31.TabIndex = 64;
		this.label31.Text = "CODE_value:";
		this.label32.AutoSize = true;
		this.label32.BackColor = System.Drawing.Color.Transparent;
		this.label32.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label32.Location = new System.Drawing.Point(27, 271);
		this.label32.Name = "label32";
		this.label32.Size = new System.Drawing.Size(47, 14);
		this.label32.TabIndex = 62;
		this.label32.Text = "CODE:";
		this.label33.AutoSize = true;
		this.label33.BackColor = System.Drawing.Color.Transparent;
		this.label33.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label33.Location = new System.Drawing.Point(693, 218);
		this.label33.Name = "label33";
		this.label33.Size = new System.Drawing.Size(71, 14);
		this.label33.TabIndex = 60;
		this.label33.Text = "Hz_unit:";
		this.label34.AutoSize = true;
		this.label34.BackColor = System.Drawing.Color.Transparent;
		this.label34.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label34.Location = new System.Drawing.Point(477, 218);
		this.label34.Name = "label34";
		this.label34.Size = new System.Drawing.Size(79, 14);
		this.label34.TabIndex = 58;
		this.label34.Text = "Hz_value:";
		this.label35.AutoSize = true;
		this.label35.BackColor = System.Drawing.Color.Transparent;
		this.label35.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label35.Location = new System.Drawing.Point(250, 218);
		this.label35.Name = "label35";
		this.label35.Size = new System.Drawing.Size(111, 14);
		this.label35.TabIndex = 56;
		this.label35.Text = "VOLTAGE_unit:";
		this.label36.AutoSize = true;
		this.label36.BackColor = System.Drawing.Color.Transparent;
		this.label36.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label36.Location = new System.Drawing.Point(27, 218);
		this.label36.Name = "label36";
		this.label36.Size = new System.Drawing.Size(119, 14);
		this.label36.TabIndex = 54;
		this.label36.Text = "VOLTAGE_value:";
		this.label37.AutoSize = true;
		this.label37.BackColor = System.Drawing.Color.Transparent;
		this.label37.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label37.Location = new System.Drawing.Point(693, 165);
		this.label37.Name = "label37";
		this.label37.Size = new System.Drawing.Size(95, 14);
		this.label37.TabIndex = 52;
		this.label37.Text = "PHASE_unit:";
		this.label38.AutoSize = true;
		this.label38.BackColor = System.Drawing.Color.Transparent;
		this.label38.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label38.Location = new System.Drawing.Point(477, 165);
		this.label38.Name = "label38";
		this.label38.Size = new System.Drawing.Size(103, 14);
		this.label38.TabIndex = 50;
		this.label38.Text = "PHASE_value:";
		this.label24.AutoSize = true;
		this.label24.BackColor = System.Drawing.Color.Transparent;
		this.label24.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label24.Location = new System.Drawing.Point(250, 165);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(103, 14);
		this.label24.TabIndex = 48;
		this.label24.Text = "OUTPUT_unit:";
		this.label25.AutoSize = true;
		this.label25.BackColor = System.Drawing.Color.Transparent;
		this.label25.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label25.Location = new System.Drawing.Point(27, 165);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(111, 14);
		this.label25.TabIndex = 46;
		this.label25.Text = "OUTPUT_value:";
		this.label26.AutoSize = true;
		this.label26.BackColor = System.Drawing.Color.Transparent;
		this.label26.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label26.Location = new System.Drawing.Point(693, 112);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(63, 14);
		this.label26.TabIndex = 44;
		this.label26.Text = "OUTPUT:";
		this.label27.AutoSize = true;
		this.label27.BackColor = System.Drawing.Color.Transparent;
		this.label27.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label27.Location = new System.Drawing.Point(477, 112);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(87, 14);
		this.label27.TabIndex = 42;
		this.label27.Text = "BORE_unit:";
		this.label28.AutoSize = true;
		this.label28.BackColor = System.Drawing.Color.Transparent;
		this.label28.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label28.Location = new System.Drawing.Point(250, 112);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(95, 14);
		this.label28.TabIndex = 40;
		this.label28.Text = "BORE_value:";
		this.label23.AutoSize = true;
		this.label23.BackColor = System.Drawing.Color.Transparent;
		this.label23.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label23.Location = new System.Drawing.Point(27, 112);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(47, 14);
		this.label23.TabIndex = 38;
		this.label23.Text = "BORE:";
		this.label22.AutoSize = true;
		this.label22.BackColor = System.Drawing.Color.Transparent;
		this.label22.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label22.Location = new System.Drawing.Point(693, 59);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(103, 14);
		this.label22.TabIndex = 36;
		this.label22.Text = "MODEL_value:";
		this.label21.AutoSize = true;
		this.label21.BackColor = System.Drawing.Color.Transparent;
		this.label21.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label21.Location = new System.Drawing.Point(477, 59);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(55, 14);
		this.label21.TabIndex = 34;
		this.label21.Text = "MODEL:";
		this.label5.AutoSize = true;
		this.label5.BackColor = System.Drawing.Color.Transparent;
		this.label5.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label5.Location = new System.Drawing.Point(250, 59);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(71, 14);
		this.label5.TabIndex = 32;
		this.label5.Text = "CASE-No:";
		this.label4.AutoSize = true;
		this.label4.BackColor = System.Drawing.Color.Transparent;
		this.label4.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label4.Location = new System.Drawing.Point(27, 59);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(79, 14);
		this.label4.TabIndex = 30;
		this.label4.Text = "Key code:";
		this.btnSavePrintDataFieldMaxLen.Location = new System.Drawing.Point(90, 346);
		this.btnSavePrintDataFieldMaxLen.Name = "btnSavePrintDataFieldMaxLen";
		this.btnSavePrintDataFieldMaxLen.Size = new System.Drawing.Size(106, 33);
		this.btnSavePrintDataFieldMaxLen.TabIndex = 29;
		this.btnSavePrintDataFieldMaxLen.Text = "保存";
		this.btnSavePrintDataFieldMaxLen.UseVisualStyleBackColor = true;
		this.btnSavePrintDataFieldMaxLen.Click += new System.EventHandler(btnSavePrintDataFieldMaxLen_Click);
		this.tabPageSysBackupResume.Controls.Add(this.btnBackupResumeOK);
		this.tabPageSysBackupResume.Controls.Add(this.radioBtnSysResume);
		this.tabPageSysBackupResume.Controls.Add(this.radioBtnSysBackup);
		this.tabPageSysBackupResume.Location = new System.Drawing.Point(4, 22);
		this.tabPageSysBackupResume.Name = "tabPageSysBackupResume";
		this.tabPageSysBackupResume.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
		this.tabPageSysBackupResume.Size = new System.Drawing.Size(927, 428);
		this.tabPageSysBackupResume.TabIndex = 8;
		this.tabPageSysBackupResume.Text = "系統備份與還原";
		this.tabPageSysBackupResume.UseVisualStyleBackColor = true;
		this.btnBackupResumeOK.Location = new System.Drawing.Point(90, 341);
		this.btnBackupResumeOK.Name = "btnBackupResumeOK";
		this.btnBackupResumeOK.Size = new System.Drawing.Size(106, 33);
		this.btnBackupResumeOK.TabIndex = 12;
		this.btnBackupResumeOK.Text = "確定";
		this.btnBackupResumeOK.UseVisualStyleBackColor = true;
		this.btnBackupResumeOK.Click += new System.EventHandler(btnBackupResumeOK_Click);
		this.radioBtnSysResume.AutoSize = true;
		this.radioBtnSysResume.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.radioBtnSysResume.Location = new System.Drawing.Point(90, 89);
		this.radioBtnSysResume.Name = "radioBtnSysResume";
		this.radioBtnSysResume.Size = new System.Drawing.Size(64, 14);
		this.radioBtnSysResume.TabIndex = 3;
		this.radioBtnSysResume.Text = "系統還原";
		this.radioBtnSysResume.UseVisualStyleBackColor = true;
		this.radioBtnSysBackup.AutoSize = true;
		this.radioBtnSysBackup.Checked = true;
		this.radioBtnSysBackup.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.radioBtnSysBackup.Location = new System.Drawing.Point(90, 37);
		this.radioBtnSysBackup.Name = "radioBtnSysBackup";
		this.radioBtnSysBackup.Size = new System.Drawing.Size(64, 14);
		this.radioBtnSysBackup.TabIndex = 2;
		this.radioBtnSysBackup.TabStop = true;
		this.radioBtnSysBackup.Text = "系統備份";
		this.radioBtnSysBackup.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(940, 573);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmSysSet";
		this.Text = "系統設定";
		base.Load += new System.EventHandler(frmSysSet_Load);
		base.pnlClientData.ResumeLayout(false);
		base.pnlBottomCtrl.ResumeLayout(false);
		this.tabCtrlSysSet.ResumeLayout(false);
		this.tabPagePathSet.ResumeLayout(false);
		this.tabPagePathSet.PerformLayout();
		this.tabPagePwdSet.ResumeLayout(false);
		this.tabPagePwdSet.PerformLayout();
		this.tabPageDataBaseSet.ResumeLayout(false);
		this.tabPageDataBaseSet.PerformLayout();
		this.tabPageFormSet.ResumeLayout(false);
		this.tabPageFormSet.PerformLayout();
		this.tabPageGroupMsgSet.ResumeLayout(false);
		this.tabPageGroupMsgSet.PerformLayout();
		this.tabPagePrintDataFieldMaxLen.ResumeLayout(false);
		this.tabPagePrintDataFieldMaxLen.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownPlace).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownHzUnit).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownPhaseUnit).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownOutput).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownModelValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownTokki).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownHzValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownPhaseValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownBoreUnit).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownModel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownCodeValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownVoltageUnit).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownOutputUnit).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownBoreValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownCaseNo).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownCode).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownVoltageValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownOutputValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownBore).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownKeycode).EndInit();
		this.tabPageSysBackupResume.ResumeLayout(false);
		this.tabPageSysBackupResume.PerformLayout();
		base.ResumeLayout(false);
	}
}
