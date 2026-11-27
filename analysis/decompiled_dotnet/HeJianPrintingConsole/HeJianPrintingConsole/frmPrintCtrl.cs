using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmPrintCtrl : frmBase
{
	public enum EnumPrintCtrlStatus
	{
		StopPrint,
		Printing,
		PausePrint
	}

	private delegate void ThreadWork(bool bCtrlUIEnabled);

	public delegate void deleteShowPrintStatus(object sender, ClsShowPrintStatusEventArgs e);

	private enum EnumPrintHeadDelayListColumnIndex
	{
		PrintHeadIndex,
		PrintHeadDelayValueIndex
	}

	public enum EnumGetAdtMsgResult
	{
		NotConfigMsgOnCaseNo,
		MsgNotExist,
		NotAdtMsg,
		AdtMsgOK,
		ConfigError
	}

	public class ClsPrintCtrlStatus
	{
		public static int iStopPrint = 0;

		public static int iPrinting = 1;

		public static int iPausePrint = 2;
	}

	private Thread m_threadListenPrintStatus;

	private Thread m_threadGetPrintedPageSn;

	private Thread m_ThreadSendData;

	private bool m_bSendDataThreadRun;

	private bool m_bGetPageSnThreadRun;

	public bool m_bPause;

	private string m_strProRecDetailTabName = "";

	private string m_strMachineNoBegin = "";

	private string m_strAdtMsgFile = "";

	private int m_iSendPageSn = 1;

	private int m_iBoxNoBegin;

	private int m_iBoxNoEnd = 1000;

	private int m_iCurPrintBoxNo;

	private int m_iLastPrintBoxNo = -1;

	private string m_strBatchNo = Guid.NewGuid().ToString();

	private string m_strBeginPrintTime = ClsShare.GetCurTimeFromSQl();

	private string m_strLastMachineNoValue = "";

	private string m_strBeginMachineNoValue1 = "";

	private string m_strBeginMachineNoValue2 = "";

	private string m_strEndMachineNo = "";

	private string m_strBoxNoEndValue = "";

	private ClsMsg.clsMsgValue_Text m_ClsMsgValueText_ModelValueFront;

	private ClsMsg.clsMsgValue_Text m_ClsMsgValueText_ModelValueSide;

	private ClsMsg.clsMsgValue_Text m_ClsMsgValueText_Place;

	private MemoryStream m_msModelValueFrontBmp;

	private MemoryStream m_msModelValueSideBmp;

	private MemoryStream m_msPlaceBmp;

	private int m_iCurPrintMem;

	private int m_iPrintMaxMem = 1;

	private string m_strSpaceBetweenVoltageUnitAndHzValue = "";

	private frmShowPrintStatus m_frmSPS;

	private bool m_bSendingData;

	private int m_sendOnePageMaxTime_ms = 10000;

	private int m_listenInterval_ms = 3000;

	private int m_setDelayMaxTimes = 5;

	private static Mutex m_adtMutex = null;

	private int m_adtMutexTimeOut_ms = -1;

	private List<uint> m_updateFailSnList;

	private int m_waitDataTime_s = 60;

	private bool m_releaseMutex = true;

	private ClsShowPrintStatusEventArgs m_ClsShowPrintStatusEventArgs = new ClsShowPrintStatusEventArgs
	{
		bFormVisible = true,
		strHasPrintBoxCount = "",
		strAllPrintBoxCount = "",
		printStatusLedColor = Color.Red,
		printStatusText = ClsAdt.ClsPrintStatus.strGetStatusFail
	};

	private bool m_bReversePrintOrder = true;

	private int[] iArrStatus = new int[1];

	private int m_iCurPrinterStatus = 1;

	private string m_strStatus = "獵取PrintingPro狀態..";

	private Color m_ColorShow = Color.Red;

	private static bool m_hasGetPrinterStatus = false;

	private int g_iLastPrintStatus = -1;

	private Color g_ColorLastColor = Color.Red;

	public static uint m_uiReturnSN = 0u;

	public static bool m_hasGetPageSn = false;

	private int m_iBoxNoBeginCurrent;

	private string m_strEndMachineNoValue2 = "";

	public static int m_iMustCloseThread = -1;

	private int m_iAppendCount = 1;

	private bool m_bSendDataNormal;

	public string m_strTextBoxModel;

	public string m_strTextBoxModelValue;

	public string m_strTextBoxBore;

	public string m_strTextBoxBoreValue;

	public string m_strTextBoxBoreUnit;

	public string m_strTextBoxOutput;

	public string m_strTextBoxOutputValue;

	public string m_strTextBoxOutputUnit;

	public string m_strTextBoxPhaseValue;

	public string m_strTextBoxPhaseUnit;

	public string m_strTextBoxVoltageValue;

	public string m_strTextBoxVoltageUnit;

	public string m_strTextBoxHzValue;

	public string m_strTextBoxHzUnit;

	public string m_strTextBoxMadeNoValue;

	public string m_strTextBoxTokki;

	public string m_strTextBoxCode;

	public string m_strTextBoxCodeValue;

	public string m_strLblMacineNo;

	public string m_strTextBoxMachineNoValue1;

	public string m_strTextBoxMachineNoValue2;

	public string m_strTextBoxPlace;

	private int m_iPrintStopStatus = 1;

	private bool m_bSendDataResult;

	public string m_strHasWritePrintDataEndBatchNo = "";

	private IContainer components;

	private Panel pnlClientFrontSideCartonSize;

	private TextBox textBoxPlace;

	private TextBox textBoxModelValueSide;

	private Panel pnlClientTopDataInput;

	private Button btnReadData;

	private Label label4;

	private TextBox textBoxBoxNoEndValue;

	private TextBox textBoxBoxNoBeginValue;

	private Label label3;

	private TextBox textBoxMadeNoValue;

	private Label label2;

	private TextBox textBoxKeyCodeValue;

	private Label lblKeyCode;

	private Panel pnlBottomBottomStatus;

	private StatusStrip statusStripPrintCtrl;

	private ToolStripStatusLabel toolStripStatusLabelPrintStatusIcon;

	private ToolStripStatusLabel toolStripStatusLabelPrintStatusText;

	private Panel pnlBottomClientCtrl;

	private TextBox textBoxHzUnit;

	private TextBox textBoxHzValue;

	private TextBox textBoxVoltageUnit;

	private TextBox textBoxVoltageValue;

	private TextBox textBoxPhaseUnit;

	private TextBox textBoxPhaseValue;

	private TextBox textBoxOutputUnit;

	private TextBox textBoxOutputValue;

	private Label label7;

	private TextBox textBoxOutput;

	private TextBox textBoxBoreUnit;

	private TextBox textBoxBoreValue;

	private Label label5;

	private TextBox textBoxBore;

	private TextBox textBoxMachineNoValueResult;

	private Label lblMacineNo;

	private TextBox textBoxCodeValue;

	private Label label8;

	private TextBox textBoxCode;

	private TextBox textBoxTokki;

	private TextBox textBoxMadeNoValueResult;

	private TextBox textBoxModelValue;

	private Label label1;

	private TextBox textBoxModel;

	private GroupBox gbDataFront;

	private GroupBox gbDataSide;

	private GroupBox groupBoxCaseNo;

	private TextBox textBoxCaseNo;

	private Label label10;

	private TextBox textBoxInsidePrintWordEnd;

	private TextBox textBoxInsidePrintWordBegin;

	private TextBox textBoxOutSidePrintWordEnd;

	private TextBox textBoxOutSidePrintWordBegin;

	private TextBox textBoxPrintWordHeight;

	private Label lblReadyPrintTip;

	private TextBox textBoxMachineNoValue2Show;

	private TextBox textBoxMachineNoValue1Show;

	private Label label9;

	private Button btnPrint;

	private Button btnStop;

	private Button btnPause;

	private Label lblStopPrintTip;

	private Button btnPreview;

	private Label lblReadyPreview;

	private CheckBox checkBoxIsPreviewRealResult;

	private Label label6;

	private ComboBox comboBoxPrintOrder;

	private Label labelWaitData;

	private System.Windows.Forms.Timer timerWaitData;

	public event deleteShowPrintStatus ShowPrintStatusEvent;

	public frmPrintCtrl()
	{
		InitializeComponent();
	}

	private void frmMain_Load(object sender, EventArgs e)
	{
		try
		{
			ClsLog.InitializeLogTab();
			IniValueOnUI(sender);
			m_adtMutex = new Mutex(initiallyOwned: true);
			ClsAdt.imOpenPrinter();
			m_adtMutex.ReleaseMutex();
			m_strProRecDetailTabName = ClsXml.GetProductRecordDetailTableName();
			StartListenPrintStatusThread();
			StartGetPrintedPageSnThread();
			ClsShare.m_btnStop = btnStop;
			ShowShowPrintStatusForm();
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("噴印控制臺窗體Load異常：" + ex.Message);
		}
	}

	private void ShowShowPrintStatusForm()
	{
		if (m_frmSPS == null)
		{
			m_frmSPS = new frmShowPrintStatus(this);
			m_frmSPS.Icon = base.Icon;
		}
	}

	private void StartListenPrintStatusThread()
	{
		m_threadListenPrintStatus = new Thread(ListenPrinterStatus);
		m_threadListenPrintStatus.IsBackground = true;
		m_threadListenPrintStatus.Start();
	}

	private void ListenPrinterStatus()
	{
		while (true)
		{
			try
			{
				while (true)
				{
					m_adtMutex.WaitOne(m_adtMutexTimeOut_ms);
					m_iCurPrinterStatus = 1;
					m_adtMutex.ReleaseMutex();
					m_strStatus = "獵取PrintingPro狀態...";
					GetPrintStatus_Invoke(bGetPrintStatus: true);
					while (!m_hasGetPrinterStatus)
					{
						Thread.Sleep(2000);
					}
					m_ColorShow = ShowPrinterStatus(m_strStatus);
					ListenPrintStatusThreadOperateUI(bOperateObj: true);
					Thread.Sleep(m_listenInterval_ms);
				}
			}
			catch (Exception ex)
			{
				ClsLog.LogWrite("SendAdt_ListenPrintStatus" + ex.Message);
				Thread.Sleep(m_listenInterval_ms);
			}
		}
	}

	private void GetPrintStatus_Invoke(bool bGetPrintStatus)
	{
		if (base.InvokeRequired)
		{
			m_hasGetPrinterStatus = false;
			ThreadWork fc = GetPrintStatus_Invoke;
			Invoke(fc, true);
		}
		else
		{
			m_adtMutex.WaitOne(m_adtMutexTimeOut_ms);
			GetPrintStatus(ref m_iCurPrinterStatus, ref m_strStatus);
			m_adtMutex.ReleaseMutex();
			m_hasGetPrinterStatus = true;
		}
	}

	private void GetPrintStatus(ref int iRltPrinterLed, ref string strStatus)
	{
		try
		{
			switch (ClsAdt.imGetPrinterStatus(iArrStatus))
			{
			case -1:
				strStatus = ClsAdt.ClsPrintStatus.strNotOpenPrinter;
				iRltPrinterLed = 1;
				ClsAdt.imOpenPrinter();
				break;
			case 1:
				switch (iArrStatus[0])
				{
				case 0:
					strStatus = ClsAdt.ClsPrintStatus.strPSOffLine;
					iRltPrinterLed = 1;
					break;
				case 1:
					strStatus = ClsAdt.ClsPrintStatus.strPrinting;
					iRltPrinterLed = 0;
					break;
				case 2:
					strStatus = ClsAdt.ClsPrintStatus.strPrintStop;
					iRltPrinterLed = 0;
					break;
				case 3:
					strStatus = ClsAdt.ClsPrintStatus.strAlarm;
					iRltPrinterLed = 2;
					GetPrinterAlarm(ref strStatus);
					break;
				}
				break;
			default:
				strStatus = ClsAdt.ClsPrintStatus.strGetStatusFail;
				iRltPrinterLed = 1;
				break;
			}
		}
		catch (Exception ex)
		{
			iRltPrinterLed = 1;
			strStatus = ex.Message;
		}
	}

	private void GetPrinterAlarm(ref string strStatus)
	{
		try
		{
			int[] iArrAlarm = new int[1];
			switch (ClsAdt.imGetPrinterAlarm(iArrAlarm))
			{
			case 1:
				switch (iArrAlarm[0])
				{
				case 0:
					strStatus = ClsAdt.ClsPrintStatus.strSystemNormal;
					break;
				case 3:
					strStatus = ClsAdt.ClsPrintStatus.strPHWarning;
					break;
				case 1:
					strStatus = ClsAdt.ClsPrintStatus.strPHError;
					break;
				case 4:
					strStatus = ClsAdt.ClsPrintStatus.strSystemWarning;
					break;
				case 2:
					strStatus = ClsAdt.ClsPrintStatus.strSystemError;
					break;
				}
				break;
			case 0:
				strStatus = ClsAdt.ClsPrintStatus.strGetAlarmDetailFail;
				break;
			case -1:
				strStatus = ClsAdt.ClsPrintStatus.strNotOpenPrinter;
				break;
			}
		}
		catch (Exception ex)
		{
			strStatus = ex.Message;
		}
	}

	private Color ShowPrinterStatus(string strStatus)
	{
		try
		{
			if (g_iLastPrintStatus == m_iCurPrinterStatus)
			{
				return g_ColorLastColor;
			}
			Color clrTmp = default(Color);
			switch (m_iCurPrinterStatus)
			{
			case 0:
				clrTmp = Color.Green;
				break;
			case 1:
			case 2:
				clrTmp = Color.Red;
				break;
			}
			g_iLastPrintStatus = m_iCurPrinterStatus;
			g_ColorLastColor = clrTmp;
			return clrTmp;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite(" ShowPrinterStatus exception:" + ex.Message);
			return Color.Red;
		}
	}

	private void ListenPrintStatusThreadOperateUI(bool bOperateObj)
	{
		if (base.InvokeRequired)
		{
			ThreadWork fc = ListenPrintStatusThreadOperateUI;
			Invoke(fc, true);
			return;
		}
		toolStripStatusLabelPrintStatusText.Text = m_strStatus;
		toolStripStatusLabelPrintStatusIcon.BackColor = m_ColorShow;
		if (!m_bSendDataThreadRun && m_ThreadSendData != null && !m_ThreadSendData.IsAlive)
		{
			m_adtMutex.WaitOne(m_adtMutexTimeOut_ms);
			ClsAdt.imSendCancel();
			ClsAdt.imPrinterStop();
			m_adtMutex.ReleaseMutex();
		}
		if (m_strStatus != ClsAdt.ClsPrintStatus.strPrinting)
		{
			btnPrint.Enabled = true;
			btnStop.Enabled = false;
			btnPause.Enabled = false;
			textBoxKeyCodeValue.ReadOnly = false;
			textBoxMadeNoValue.ReadOnly = false;
			textBoxBoxNoBeginValue.ReadOnly = false;
			textBoxBoxNoEndValue.ReadOnly = false;
			textBoxMachineNoValue1Show.ReadOnly = false;
			textBoxMachineNoValue2Show.ReadOnly = false;
			comboBoxPrintOrder.Enabled = true;
			btnReadData.Enabled = true;
			lblReadyPrintTip.Visible = false;
			lblStopPrintTip.Visible = false;
			labelWaitData.Visible = false;
			timerWaitData.Enabled = false;
			if (m_bSendDataThreadRun)
			{
				m_iPrintStopStatus = -1;
				BeginInvoke((EventHandler)delegate
				{
					WritePrintDataEndToSQl(m_iPrintStopStatus);
				});
			}
			m_bGetPageSnThreadRun = false;
			pnlClientTopDataInput.Refresh();
			pnlBottomClientCtrl.Refresh();
			this.ShowPrintStatusEvent(this, m_ClsShowPrintStatusEventArgs);
		}
		else
		{
			this.ShowPrintStatusEvent(this, m_ClsShowPrintStatusEventArgs);
		}
		m_ClsShowPrintStatusEventArgs.printStatusText = m_strStatus;
		m_ClsShowPrintStatusEventArgs.printStatusLedColor = m_ColorShow;
		statusStripPrintCtrl.Refresh();
	}

	private void StartGetPrintedPageSnThread()
	{
		if (m_threadGetPrintedPageSn == null)
		{
			m_adtMutex.WaitOne(m_adtMutexTimeOut_ms);
			ClsAdt.imClearReturnQueue();
			m_adtMutex.ReleaseMutex();
			m_threadGetPrintedPageSn = new Thread(GetPrintedPageSN);
			m_threadGetPrintedPageSn.IsBackground = true;
			m_threadGetPrintedPageSn.Start();
		}
	}

	public void GetPrintedPageSN()
	{
		while (true)
		{
			Thread.Sleep(100);
			try
			{
				if (m_bGetPageSnThreadRun)
				{
					uint uiReturnSN = 0u;
					m_adtMutex.WaitOne(m_adtMutexTimeOut_ms);
					int iRltGPP = ClsAdt.imGetPrinttedPagesn(ref uiReturnSN);
					m_adtMutex.ReleaseMutex();
					if (iRltGPP == 1 && uiReturnSN != 0)
					{
						ClsLog.LogWrite("回馈的SN=" + uiReturnSN + ",当前m_iCurPrintMem=" + m_iCurPrintMem);
						Interlocked.Decrement(ref m_iCurPrintMem);
						ClsLog.LogWrite("回馈线程递减当前喷印内存m_iCurPrintMem=" + m_iCurPrintMem);
						updateBackToSql(uiReturnSN);
						GetPageSnThreadOperateUI(bOperateObj: true);
					}
					else if (m_updateFailSnList.Count > 0 && WritePrintDateEnd(m_updateFailSnList[0]))
					{
						m_updateFailSnList.RemoveAt(0);
					}
				}
			}
			catch (Exception ex)
			{
				ClsLog.LogWrite("GetPrintPageSn exception" + ex.Message);
			}
		}
	}

	private void updateBackToSql(uint uiReturnSN)
	{
		if (!WritePrintDateEnd(uiReturnSN))
		{
			m_updateFailSnList.Add(uiReturnSN);
		}
		int iCurBatchHasPrintCount = GetCurBatchHasPrintCount(m_strBatchNo);
		if (iCurBatchHasPrintCount > 0)
		{
			ClsLog.LogWrite("If GetPageSnThreadOperateUI iCurBatchHasPrintCount:" + iCurBatchHasPrintCount);
			m_iBoxNoBeginCurrent = m_iBoxNoBegin + iCurBatchHasPrintCount;
			long lEndMachineNoValue2 = Convert.ToInt64(m_strBeginMachineNoValue2) + iCurBatchHasPrintCount - 1;
			if (m_bReversePrintOrder)
			{
				lEndMachineNoValue2 = Convert.ToInt64(m_strBeginMachineNoValue2) - iCurBatchHasPrintCount + 1;
			}
			m_strEndMachineNo = string.Concat(str1: m_strEndMachineNoValue2 = lEndMachineNoValue2.ToString().PadLeft(m_strBeginMachineNoValue2.Length, '0'), str0: m_strBeginMachineNoValue1);
			m_strHasWritePrintDataEndBatchNo = "";
			m_iPrintStopStatus = 1;
			WritePrintDataEndToSQl(m_iPrintStopStatus);
		}
		else
		{
			ClsLog.LogWrite("Else GetPageSnThreadOperateUI iCurBatchHasPrintCount:" + iCurBatchHasPrintCount);
		}
	}

	private bool WritePrintDateEnd(uint uiReturnSN)
	{
		try
		{
			string strPrefix = m_strProRecDetailTabName.Substring(0, 2).Trim().ToUpper();
			string strSql = "UPDATE " + m_strProRecDetailTabName + " SET [" + strPrefix + ClsProductRecordDetailField.strPageEndTime + "] = GetDate() WHERE  [" + strPrefix + ClsProductRecordDetailField.strPageNo + "] = " + uiReturnSN + "  AND  [" + strPrefix + ClsProductRecordDetailField.strBatchNo + "] = '" + m_strBatchNo + "'";
			int iRlt = ClsDataBase.SqlOperateData(strSql);
			if (iRlt <= 0)
			{
				ClsLog.LogWrite("WritePrintDateEnd error.PageSn:" + uiReturnSN + "  iRlt=" + iRlt);
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("WritePrintDateEnd exception:" + ex.Message);
			return false;
		}
	}

	private IAsyncResult GetPageSnThreadOperateUI(bool bOperateObj)
	{
		return BeginInvoke((EventHandler)delegate
		{
			textBoxBoxNoBeginValue.Text = m_iBoxNoBeginCurrent.ToString();
			textBoxMachineNoValue2Show.Text = m_strEndMachineNoValue2;
			string text = m_iBoxNoBeginCurrent.ToString();
			string strBoxNoEndValue = m_strBoxNoEndValue;
			if (text.Length > 0 && strBoxNoEndValue.Length > 0)
			{
				if (Convert.ToInt32(text) == Convert.ToInt32(strBoxNoEndValue))
				{
					m_bSendDataThreadRun = false;
					if (DialogResult.Yes == MessageBox.Show("噴印完成，是否清空箱數與機番?", "系統提示", MessageBoxButtons.YesNo))
					{
						textBoxBoxNoBeginValue.Text = "";
						textBoxBoxNoEndValue.Text = "";
						textBoxMachineNoValue1Show.Text = "";
						textBoxMachineNoValue2Show.Text = "";
					}
				}
				textBoxBoxNoBeginValue.Refresh();
				textBoxBoxNoEndValue.Refresh();
				textBoxMachineNoValue1Show.Refresh();
				textBoxMachineNoValue2Show.Refresh();
			}
		});
	}

	private int GetCurBatchHasPrintCount(string strBatchNo)
	{
		try
		{
			string strTabName = ClsXml.GetProductRecordDetailTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strSql = "SELECT COUNT([" + strPrefix + ClsProductRecordDetailField.strPageEndTime + "])  FROM " + strTabName + " WHERE [" + strPrefix + ClsProductRecordDetailField.strBatchNo + "]='" + strBatchNo.Trim() + "'  AND  [" + strPrefix + ClsProductRecordDetailField.strPageEndTime + "]  IS NOT NULL";
			DataTable dt = ClsDataBase.SqlReadData(strSql);
			if (dt != null && dt.Rows.Count > 0)
			{
				string strTmp = dt.Rows[0][0].ToString();
				return Convert.ToInt32(strTmp);
			}
			return 0;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("GetCurBatchHasPrintCount exception:" + ex.Message);
			return 0;
		}
	}

	private void IniValueOnUI(object sender)
	{
		textBoxMadeNoValueResult.Text = textBoxMadeNoValue.Text;
		textBoxModelValueSide.Text = textBoxModelValue.Text;
		textBoxKeyCodeValue_KeyUp(sender, null);
		comboBoxPrintOrder.SelectedIndex = 0;
	}

	private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
	{
		ClsAdt.imSendCancel();
		ClsAdt.imClearReturnQueue();
		ClsAdt.imClearPrintQueue();
		ClsAdt.imPrinterStop();
		ClsAdt.imClosePrinter();
		m_threadGetPrintedPageSn = null;
		if (m_frmSPS != null)
		{
			m_frmSPS.Close();
		}
		Environment.Exit(0);
	}

	private void textBoxKeyCodeValue_KeyUp(object sender, KeyEventArgs e)
	{
	}

	private void btnReadData_Click(object sender, EventArgs e)
	{
		string strTabName = ClsXml.GetPrintDataTableName();
		string strSql = "SELECT ROW_NUMBER() OVER(ORDER BY [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "]) AS " + strTabName.Substring(0, 2).Trim().ToUpper() + "Number,*  FROM " + strTabName + "  WHERE [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "] LIKE '%" + ClsDataBase.ResetSingleQuotationMarkForSql(textBoxKeyCodeValue.Text) + "%' ORDER BY [" + strTabName.Trim().ToUpper().Substring(0, 2) + ClsPrintDataField.strKeyCode + "]";
		frmPrintDataSelect frmPDS = new frmPrintDataSelect(strSql, textBoxKeyCodeValue.Text);
		frmPDS.Icon = base.Icon;
		string strLastMainTitle = ((frmMain)base.Parent.Parent.Parent).Text;
		((frmMain)base.Parent.Parent.Parent).Text = frmMain.m_strFrmMainTitle + "---" + frmPDS.Text;
		frmPDS.ShowDialog();
		((frmMain)base.Parent.Parent.Parent).Text = strLastMainTitle;
		ImportData(ClsShare.m_dtSelData);
		ImportCartonSize();
		if (ClsShare.m_dtSelData != null && ClsShare.m_dtSelData.Rows.Count > 0)
		{
			string strCheckFieldExceedMaxLenMsg = "";
			if (CheckFieldExceedMaxLen(ref strCheckFieldExceedMaxLenMsg))
			{
				ClsMsgBox.MessageBoxShowDefWarn(this, strCheckFieldExceedMaxLenMsg);
			}
		}
	}

	private void ImportData(DataTable dtSrc)
	{
		if (dtSrc == null || dtSrc.Rows.Count <= 0)
		{
			return;
		}
		for (int iIndex = 0; iIndex < dtSrc.Columns.Count; iIndex++)
		{
			string strCaption = dtSrc.Columns[iIndex].Caption.Trim().ToUpper();
			string strValue = dtSrc.Rows[0][iIndex].ToString();
			if (strCaption == ClsPrintDataField.strKeyCode)
			{
				textBoxKeyCodeValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strCaseNo)
			{
				textBoxCaseNo.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strModel)
			{
				textBoxModel.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strModelValue)
			{
				textBoxModelValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strBore)
			{
				textBoxBore.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strBoreValue)
			{
				textBoxBoreValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strBoreUnit)
			{
				textBoxBoreUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strOutput)
			{
				textBoxOutput.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strOutputValue)
			{
				textBoxOutputValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strOutputUnit)
			{
				textBoxOutputUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strPhaseValue)
			{
				textBoxPhaseValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strPhaseUnit)
			{
				textBoxPhaseUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strVoltageValue)
			{
				textBoxVoltageValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strVoltageUnit)
			{
				textBoxVoltageUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strHzValue)
			{
				textBoxHzValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strHzUnit)
			{
				textBoxHzUnit.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strTokki)
			{
				textBoxTokki.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strCode)
			{
				textBoxCode.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strCodeValue)
			{
				textBoxCodeValue.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strPlace)
			{
				textBoxPlace.Text = strValue;
			}
			else if (strCaption == ClsPrintDataField.strCaseNo)
			{
				textBoxCaseNo.Text = strValue;
			}
		}
	}

	private void ImportCartonSize()
	{
		try
		{
			string strCaseNo = textBoxCaseNo.Text.Trim();
			if (strCaseNo.Length <= 0)
			{
				return;
			}
			string strTabName = ClsXml.GetCartonSizeTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strSql = "SELECT * FROM " + strTabName + "  WHERE [" + strPrefix + ClsCartonSizeFieldKey.strCaseNo + "]='" + strCaseNo + "'";
			DataTable dt = ClsDataBase.SqlReadData(strSql);
			if (dt == null || dt.Rows.Count <= 0)
			{
				return;
			}
			string cartonsizeLog = "ImportCartonSize 紙箱規格:";
			for (int iIndex = 0; iIndex < dt.Columns.Count; iIndex++)
			{
				string strCaption = dt.Columns[iIndex].Caption.Substring(2).Trim().ToUpper();
				string strValue = dt.Rows[0][iIndex].ToString();
				string text = cartonsizeLog;
				cartonsizeLog = text + strCaption + "=" + strValue + ";  ";
				if (strCaption == ClsCartonSizeFieldKey.strPrintWordHeight)
				{
					textBoxPrintWordHeight.Text = strValue;
				}
				else if (strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordBegin)
				{
					textBoxOutSidePrintWordBegin.Text = strValue;
				}
				else if (strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordEnd)
				{
					textBoxOutSidePrintWordEnd.Text = strValue;
				}
				else if (strCaption == ClsCartonSizeFieldKey.strInsidePrintWordBegin)
				{
					textBoxInsidePrintWordBegin.Text = strValue;
				}
				else if (strCaption == ClsCartonSizeFieldKey.strInsidePrintWordEnd)
				{
					textBoxInsidePrintWordEnd.Text = strValue;
				}
			}
			ClsLog.LogWrite(cartonsizeLog);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("ImPortCartonSize exception:" + ex.Message);
		}
	}

	private bool CheckFieldExceedMaxLen(ref string strMsg)
	{
		try
		{
			ClsGetPrintDataFieldMaxLen ClsGPDFM = new ClsGetPrintDataFieldMaxLen();
			strMsg = "以下欄位資料長度超過系統設定：\n\n";
			bool bRlt = false;
			if (textBoxKeyCodeValue.Text.Trim().Length > ClsGPDFM.iLen_Keycode)
			{
				strMsg += GroupTipStr("Keycode", textBoxKeyCodeValue.Text.Trim(), ClsGPDFM.iLen_Keycode);
				bRlt = true;
			}
			if (textBoxModel.Text.Trim().Length > ClsGPDFM.iLen_Model)
			{
				strMsg += GroupTipStr("Model", textBoxModel.Text.Trim(), ClsGPDFM.iLen_Model);
				bRlt = true;
			}
			if (textBoxModelValue.Text.Trim().Length > ClsGPDFM.iLen_ModelValue)
			{
				strMsg += GroupTipStr("ModelValue", textBoxModelValue.Text.Trim(), ClsGPDFM.iLen_ModelValue);
				bRlt = true;
			}
			if (textBoxBore.Text.Trim().Length > ClsGPDFM.iLen_Bore)
			{
				strMsg += GroupTipStr("Bore", textBoxBore.Text.Trim(), ClsGPDFM.iLen_Bore);
				bRlt = true;
			}
			if (textBoxBoreValue.Text.Trim().Length > ClsGPDFM.iLen_BoreValue)
			{
				strMsg += GroupTipStr("BoreValue", textBoxBoreValue.Text.Trim(), ClsGPDFM.iLen_BoreValue);
				bRlt = true;
			}
			if (textBoxBoreUnit.Text.Trim().Length > ClsGPDFM.iLen_BoreUnit)
			{
				strMsg += GroupTipStr("BoreUnit", textBoxBoreUnit.Text.Trim(), ClsGPDFM.iLen_BoreUnit);
				bRlt = true;
			}
			if (textBoxOutput.Text.Trim().Length > ClsGPDFM.iLen_Output)
			{
				strMsg += GroupTipStr("Output", textBoxOutput.Text.Trim(), ClsGPDFM.iLen_Output);
				bRlt = true;
			}
			if (textBoxOutputValue.Text.Trim().Length > ClsGPDFM.iLen_OutputValue)
			{
				strMsg += GroupTipStr("OutputValue", textBoxOutputValue.Text.Trim(), ClsGPDFM.iLen_OutputValue);
				bRlt = true;
			}
			if (textBoxOutputUnit.Text.Trim().Length > ClsGPDFM.iLen_OutputUnit)
			{
				strMsg += GroupTipStr("OutputUnit", textBoxOutputUnit.Text.Trim(), ClsGPDFM.iLen_OutputUnit);
				bRlt = true;
			}
			if (textBoxPhaseValue.Text.Trim().Length > ClsGPDFM.iLen_PhaseValue)
			{
				strMsg += GroupTipStr("PhaseValue", textBoxPhaseValue.Text.Trim(), ClsGPDFM.iLen_PhaseValue);
				bRlt = true;
			}
			if (textBoxPhaseUnit.Text.Trim().Length > ClsGPDFM.iLen_PhaseUnit)
			{
				strMsg += GroupTipStr("PhaseUnit", textBoxPhaseUnit.Text.Trim(), ClsGPDFM.iLen_PhaseUnit);
				bRlt = true;
			}
			if (textBoxVoltageValue.Text.Trim().Length > ClsGPDFM.iLen_VoltageValue)
			{
				strMsg += GroupTipStr("VoltageValue", textBoxVoltageValue.Text.Trim(), ClsGPDFM.iLen_VoltageValue);
				bRlt = true;
			}
			if (textBoxVoltageUnit.Text.Trim().Length > ClsGPDFM.iLen_VoltageUnit)
			{
				strMsg += GroupTipStr("VoltageUnit", textBoxVoltageUnit.Text.Trim(), ClsGPDFM.iLen_VoltageUnit);
				bRlt = true;
			}
			if (textBoxHzValue.Text.Trim().Length > ClsGPDFM.iLen_HzValue)
			{
				strMsg += GroupTipStr("HzValue", textBoxHzValue.Text.Trim(), ClsGPDFM.iLen_HzValue);
				bRlt = true;
			}
			if (textBoxHzUnit.Text.Trim().Length > ClsGPDFM.iLen_HzUnit)
			{
				strMsg += GroupTipStr("HzUnit", textBoxHzUnit.Text.Trim(), ClsGPDFM.iLen_HzUnit);
				bRlt = true;
			}
			if (textBoxTokki.Text.Trim().Length > ClsGPDFM.iLen_Tokki)
			{
				strMsg += GroupTipStr("Tokki", textBoxTokki.Text.Trim(), ClsGPDFM.iLen_Tokki);
				bRlt = true;
			}
			if (textBoxCode.Text.Trim().Length > ClsGPDFM.iLen_Code)
			{
				strMsg += GroupTipStr("Code", textBoxCode.Text.Trim(), ClsGPDFM.iLen_Code);
				bRlt = true;
			}
			if (textBoxCodeValue.Text.Trim().Length > ClsGPDFM.iLen_CodeValue)
			{
				strMsg += GroupTipStr("CodeValue", textBoxCodeValue.Text.Trim(), ClsGPDFM.iLen_CodeValue);
				bRlt = true;
			}
			if (textBoxPlace.Text.Trim().Length > ClsGPDFM.iLen_Place)
			{
				strMsg += GroupTipStr("Place", textBoxPlace.Text.Trim(), ClsGPDFM.iLen_Place);
				bRlt = true;
			}
			if (textBoxCaseNo.Text.Trim().Length > ClsGPDFM.iLen_CaseNo)
			{
				strMsg += GroupTipStr("CaseNo", textBoxCaseNo.Text.Trim(), ClsGPDFM.iLen_CaseNo);
				bRlt = true;
			}
			return bRlt;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("CheckFieldExceedMaxLen exception:" + ex.Message);
			return true;
		}
	}

	private string GroupTipStr(string strKey, string strValue, int iLen)
	{
		return strKey + " [" + strValue + "]  系統設定  " + iLen + " \n";
	}

	private bool PrintHeadDelaySet(DataTable dtPrintHeadData)
	{
		try
		{
			if (!CheckPrintHeadDelayHasChanged(dtPrintHeadData))
			{
				return true;
			}
			int[] iArrStatus = new int[1];
			ClsAdt.imGetPrinterStatus(iArrStatus);
			if (iArrStatus[0] == 1)
			{
				ClsLog.LogWrite("噴印中無法設定延遲");
				return false;
			}
			bool bSetResult = false;
			int iSetPrintHeadNo = 0;
			int setDelayTimes = 1;
			ClsAdt.SetPrintHeadDelay(dtPrintHeadData, ref bSetResult, ref iSetPrintHeadNo);
			string printHeadDelayLog = "噴頭個數" + dtPrintHeadData.Rows.Count + "  ";
			for (int iRowIndex = 0; iRowIndex < dtPrintHeadData.Rows.Count; iRowIndex++)
			{
				float fPhDelay = (float)Convert.ToDouble(dtPrintHeadData.Rows[iRowIndex][ClsAdt.ClsPrintHeadItems.strColumnPhDelayName].ToString());
				string text = printHeadDelayLog;
				printHeadDelayLog = text + "  " + iRowIndex + "號噴頭延遲:" + ((double)fPhDelay + 0.01) * 100.0;
			}
			ClsLog.LogWrite(printHeadDelayLog);
			while (!bSetResult && setDelayTimes <= m_setDelayMaxTimes)
			{
				if (!bSetResult)
				{
					setDelayTimes++;
					ClsAdt.SetPrintHeadDelay(dtPrintHeadData, ref bSetResult, ref iSetPrintHeadNo);
				}
			}
			ClsLog.LogWrite("延遲設定次數:" + setDelayTimes);
			if (bSetResult)
			{
				ClsLog.LogWrite("延遲設定成功");
				return true;
			}
			ClsLog.LogWrite(iSetPrintHeadNo + "號噴頭設定失敗");
			MessageBox.Show("延遲設定失敗，請重新啟動程序後，再試一次!");
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("PrintHeadDelaySet exception:" + ex.Message);
			return false;
		}
	}

	private bool CheckPrintHeadDelayHasChanged(DataTable dtNeedSetPHDelayList)
	{
		try
		{
			bool bRlt = false;
			DataTable dtPHList = ClsAdt.GetPrintHeadDelayList();
			for (int iRowIndex = 0; iRowIndex < dtPHList.Rows.Count; iRowIndex++)
			{
				if (Convert.ToDouble(dtPHList.Rows[iRowIndex][1].ToString()) != Convert.ToDouble(dtNeedSetPHDelayList.Rows[iRowIndex][1].ToString()))
				{
					bRlt = true;
					break;
				}
			}
			return bRlt;
		}
		catch
		{
			return true;
		}
	}

	private DataTable GetPrintHeadDelaySet(double dMsgWidthOutside, double dMsgWidthInside)
	{
		try
		{
			DataTable dtPHDelayListTmp = new DataTable();
			IniPrintHeadDelayList(ref dtPHDelayListTmp, 4);
			string strOutSidePrintWordBegin = textBoxOutSidePrintWordBegin.Text;
			if (strOutSidePrintWordBegin.Length <= 0)
			{
				strOutSidePrintWordBegin = "0";
			}
			string strInSidePrintWordBegin = textBoxInsidePrintWordBegin.Text;
			if (strInSidePrintWordBegin.Length <= 0)
			{
				strInSidePrintWordBegin = "0";
			}
			int iWidthDpi = Convert.ToInt32(ClsXml.GetWidthDPI());
			double dDelay34DelayOffset = 0.0;
			if (dMsgWidthInside <= dMsgWidthOutside)
			{
				dDelay34DelayOffset = (dMsgWidthOutside - dMsgWidthInside) / (double)iWidthDpi * 25.4;
			}
			double dPH12Delay3 = Convert.ToDouble(strOutSidePrintWordBegin);
			double dPH34Delay3 = Convert.ToDouble(strInSidePrintWordBegin);
			double ph1Delay1 = ClsXml.GetPH1Delay1();
			double ph1Delay2 = ClsXml.GetPH1Delay2();
			double ph2Delay1 = ClsXml.GetPH2Delay1();
			double ph2Delay2 = ClsXml.GetPH2Delay2();
			double ph3Delay1 = ClsXml.GetPH3Delay1();
			double ph3Delay2 = ClsXml.GetPH3Delay2();
			double ph4Delay1 = ClsXml.GetPH4Delay1();
			double ph4Delay2 = ClsXml.GetPH4Delay2();
			dtPHDelayListTmp.Rows[0][1] = ph1Delay1 + ph1Delay2 + dPH12Delay3;
			dtPHDelayListTmp.Rows[1][1] = ph2Delay1 + ph2Delay2 + dPH12Delay3;
			dtPHDelayListTmp.Rows[2][1] = ph3Delay1 + ph3Delay2 + dPH34Delay3 - dDelay34DelayOffset;
			dtPHDelayListTmp.Rows[3][1] = ph4Delay1 + ph4Delay2 + dPH34Delay3 - dDelay34DelayOffset;
			DataTable dt = new DataTable();
			DataTable dtPHList = ClsAdt.GetPrintHeadDelayList();
			int iPrintHeadCountOnPrintPro = dtPHList.Rows.Count;
			IniPrintHeadDelayList(ref dt, iPrintHeadCountOnPrintPro);
			for (int iRowIndex = 0; iRowIndex < iPrintHeadCountOnPrintPro; iRowIndex++)
			{
				string strPHValue = dtPHDelayListTmp.Rows[iRowIndex][1].ToString();
				dt.Rows[iRowIndex][1] = strPHValue;
			}
			double delay1 = Convert.ToDouble(dtPHDelayListTmp.Rows[0][1].ToString());
			double delay2 = Convert.ToDouble(dtPHDelayListTmp.Rows[1][1].ToString());
			double delay3 = Convert.ToDouble(dtPHDelayListTmp.Rows[2][1].ToString());
			double delay4 = Convert.ToDouble(dtPHDelayListTmp.Rows[3][1].ToString());
			if (delay1 < 0.0 || delay2 < 0.0 || delay3 < 0.0 || delay4 < 0.0)
			{
				string delayLog = "延遲有負值 ; dMsgWidthOutside=" + dMsgWidthOutside + "; dMsgWidthInside=" + dMsgWidthInside + "; iWidthDpi=" + iWidthDpi + "; dPH12Delay3=" + dPH12Delay3 + "; dPH34Delay3=" + dPH34Delay3 + "; ph1Delay1=" + ph1Delay1 + "; ph1Delay2=" + ph1Delay2 + "; ph2Delay1=" + ph2Delay1 + "; ph2Delay2=" + ph2Delay2 + "; ph3Delay1=" + ph3Delay1 + "; ph3Delay2=" + ph3Delay2 + "; ph4Delay1=" + ph4Delay1 + "; ph4Delay2=" + ph4Delay2 + "; dDelay34DelayOffset=" + dDelay34DelayOffset + ";";
				delayLog = delayLog + "delay1=" + delay1 + ";  delay2=" + delay2 + ";  delay3=" + delay3 + ";  delay4=" + delay4 + ";";
				ClsLog.LogWrite(delayLog);
				return null;
			}
			return dt;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("GetPrintHeadDelayList exception:" + ex.Message);
			return null;
		}
	}

	private bool IniPrintHeadDelayList(ref DataTable dtSrc, int iPrintHeadCount)
	{
		try
		{
			dtSrc.Columns.Add();
			dtSrc.Columns[0].Caption = ClsAdt.ClsPrintHeadItems.strColumnPhNoCaption;
			dtSrc.Columns[0].ColumnName = ClsAdt.ClsPrintHeadItems.strColumnPhNoName;
			dtSrc.Columns.Add();
			dtSrc.Columns[1].Caption = ClsAdt.ClsPrintHeadItems.strColumnPhDelayCaption;
			dtSrc.Columns[1].ColumnName = ClsAdt.ClsPrintHeadItems.strColumnPhDelayName;
			for (int iRowIndex = 0; iRowIndex < iPrintHeadCount; iRowIndex++)
			{
				dtSrc.Rows.Add();
				dtSrc.Rows[iRowIndex][0] = "噴頭" + (iRowIndex + 1) + "延遲";
			}
			return true;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("IniPrintHeadDelayList exception:" + ex.Message);
			return false;
		}
	}

	private void textBoxMadeNoValue_TextChanged(object sender, EventArgs e)
	{
		if (!ClsShare.CheckStrIsAscii(((TextBox)sender).Text.Trim()))
		{
			MessageBox.Show("輸入必須為字母、數字或者符號");
			((TextBox)sender).Text = "";
		}
		else
		{
			textBoxMadeNoValueResult.Text = textBoxMadeNoValue.Text;
		}
	}

	private void textBoxModelValue_TextChanged(object sender, EventArgs e)
	{
		textBoxModelValueSide.Text = textBoxModelValue.Text;
	}

	private void textBoxMachineNoValue2_TextChanged(object sender, EventArgs e)
	{
	}

	private void ToolStripMenuItemProductRecord_Click(object sender, EventArgs e)
	{
		frmProductRecord frmPR = new frmProductRecord();
		frmPR.ShowDialog();
	}

	private void WriteToProRecMasterTab(string strBatchNo, string strKeyCode, string strMadeNo, string strCaseNo, string strBeginPrintTime, string strBeginBoxNo, string strBeginMachineNo)
	{
		try
		{
			string strTabName = ClsXml.GetProductRecordMasterTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strGuid = Guid.NewGuid().ToString();
			string strSqlKey = "INSERT " + strTabName + "([" + strPrefix + ClsProductRecordMasterField.strID + "],[" + strPrefix + ClsProductRecordMasterField.strBatchNo + "],[" + strPrefix + ClsProductRecordMasterField.strKeyCode + "],[" + strPrefix + ClsProductRecordMasterField.strMadeNo + "],[" + strPrefix + ClsProductRecordMasterField.strCaseNo + "],[" + strPrefix + ClsProductRecordMasterField.strBeginPrintTime + "],[" + strPrefix + ClsProductRecordMasterField.strBeginBoxNo + "],[" + strPrefix + ClsProductRecordMasterField.strBeginMachineNo + "])";
			string strSqlValue = "  VALUES('" + strGuid + "','" + m_strBatchNo + "','" + strKeyCode + "','" + strMadeNo + "','" + strCaseNo + "',CONVERT(VARCHAR(100), GETDATE(), 121),'" + strBeginBoxNo + "','" + strBeginMachineNo + "')";
			string strSql = strSqlKey + "  " + strSqlValue;
			if (ClsDataBase.SqlOperateData(strSql) <= 0)
			{
				ClsLog.LogWrite("Write product record master error!Guid=" + strGuid);
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("WriteToProRecMasterTab exception:" + ex.Message);
		}
	}

	private EnumGetAdtMsgResult GetAdtMsgFile(ref string strAdtMsgFile, ref double dMsgWidthOutside, ref double dMsgWidthInside)
	{
		try
		{
			string strSourceMsg1Path = ClsXml.GetSourceMsg1Path();
			if (!File.Exists(strSourceMsg1Path))
			{
				MessageBox.Show("正嘜模板不存在，請確認配置是否正確!");
				return EnumGetAdtMsgResult.ConfigError;
			}
			string strSourceMsg2Path = ClsXml.GetSourceMsg2Path();
			if (!File.Exists(strSourceMsg2Path))
			{
				MessageBox.Show("側嘜模板不存在，請確認配置是否正確!");
				return EnumGetAdtMsgResult.ConfigError;
			}
			string strCaseNo = textBoxCaseNo.Text;
			string strTabName = ClsXml.GetCartonSizeTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strSql = "SELECT * FROM " + strTabName + "  WHERE  [" + strPrefix + ClsCartonSizeFieldKey.strCaseNo + "]='" + strCaseNo + "'";
			DataTable dt = ClsDataBase.SqlReadData(strSql);
			double dOutSidePrintWordBegin = 0.0;
			double dOutSidePrintWordEnd = 0.0;
			double dInSidePrintWordBegin = 0.0;
			double dInSidePrintWordEnd = 0.0;
			if (dt == null || dt.Rows.Count <= 0)
			{
				MessageBox.Show("紙箱規格不存在");
				return EnumGetAdtMsgResult.ConfigError;
			}
			string cartonsizeLog = "GetAdtMsgFile 紙箱規格:";
			for (int iIndex = 0; iIndex < dt.Columns.Count; iIndex++)
			{
				string strCaption = dt.Columns[iIndex].Caption.Substring(2).Trim().ToUpper();
				string strValue = dt.Rows[0][iIndex].ToString();
				string text = cartonsizeLog;
				cartonsizeLog = text + strCaption + "=" + strValue + ";  ";
				if (strCaption == ClsCartonSizeFieldKey.strCartonSizeL)
				{
					if (strValue.Length <= 0)
					{
						MessageBox.Show(ClsCartonSizeFieldKey.strCartonSizeL + "為空，請重新設置!");
						return EnumGetAdtMsgResult.ConfigError;
					}
					Convert.ToDouble(strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strCartonSizeW)
				{
					if (strValue.Length <= 0)
					{
						MessageBox.Show(ClsCartonSizeFieldKey.strCartonSizeW + "為空，請重新設置!");
						return EnumGetAdtMsgResult.ConfigError;
					}
					Convert.ToDouble(strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strCartonSizeH)
				{
					if (strValue.Length <= 0)
					{
						MessageBox.Show(ClsCartonSizeFieldKey.strCartonSizeH + "為空，請重新設置!");
						return EnumGetAdtMsgResult.ConfigError;
					}
					Convert.ToDouble(strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strPrintWordHeight)
				{
					if (strValue.Length <= 0)
					{
						MessageBox.Show(ClsCartonSizeFieldKey.strPrintWordHeight + "為空，請重新設置!");
						return EnumGetAdtMsgResult.ConfigError;
					}
					Convert.ToDouble(strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordBegin)
				{
					if (strValue.Length <= 0)
					{
						MessageBox.Show(ClsCartonSizeFieldKey.strOutsidePrintWordBegin + "為空，請重新設置!");
						return EnumGetAdtMsgResult.ConfigError;
					}
					dOutSidePrintWordBegin = Convert.ToDouble(strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordEnd)
				{
					if (strValue.Length <= 0)
					{
						MessageBox.Show(ClsCartonSizeFieldKey.strOutsidePrintWordEnd + "為空，請重新設置!");
						return EnumGetAdtMsgResult.ConfigError;
					}
					dOutSidePrintWordEnd = Convert.ToDouble(strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strInsidePrintWordBegin)
				{
					if (strValue.Length <= 0)
					{
						MessageBox.Show(ClsCartonSizeFieldKey.strInsidePrintWordBegin + "為空，請重新設置!");
						return EnumGetAdtMsgResult.ConfigError;
					}
					dInSidePrintWordBegin = Convert.ToDouble(strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strInsidePrintWordEnd)
				{
					if (strValue.Length <= 0)
					{
						MessageBox.Show(ClsCartonSizeFieldKey.strInsidePrintWordEnd + "為空，請重新設置!");
						return EnumGetAdtMsgResult.ConfigError;
					}
					dInSidePrintWordEnd = Convert.ToDouble(strValue);
				}
			}
			ClsLog.LogWrite(cartonsizeLog);
			strAdtMsgFile = Application.StartupPath + "\\Msg\\AdtTemp.ms";
			StringBuilder StrBuiMsg1WidthTemp = new StringBuilder(255);
			ClsIni.GetPrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgCommonKey.strWidth, "0", StrBuiMsg1WidthTemp, 255, strSourceMsg1Path);
			StringBuilder StrBuiMsg2WidthTemp = new StringBuilder(255);
			ClsIni.GetPrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgCommonKey.strWidth, "0", StrBuiMsg2WidthTemp, 255, strSourceMsg2Path);
			double dMsg1Width = Convert.ToDouble(StrBuiMsg1WidthTemp.ToString());
			ClsLog.LogWrite("Msg1 width:" + dMsg1Width);
			if (dMsg1Width <= 0.0)
			{
				MessageBox.Show("正嘜模板寬度必須為正，請核對!");
				return EnumGetAdtMsgResult.ConfigError;
			}
			double dMsg2Width = Convert.ToDouble(StrBuiMsg2WidthTemp.ToString());
			ClsLog.LogWrite("Msg2 width:" + dMsg2Width);
			if (dMsg2Width <= 0.0)
			{
				MessageBox.Show("側嘜模板寬度必須為正，請核對!");
				return EnumGetAdtMsgResult.ConfigError;
			}
			double dDPI = ClsXml.GetWidthDPI();
			ClsLog.LogWrite("WidthDPI:" + dDPI);
			double dMillimeterPerInch = 25.4;
			double dWidthSpaceOutSide = (dOutSidePrintWordEnd - dOutSidePrintWordBegin) / dMillimeterPerInch * dDPI - dMsg1Width - dMsg2Width;
			double dWidthSpaceInSide = (dInSidePrintWordEnd - dInSidePrintWordBegin) / dMillimeterPerInch * dDPI - dMsg1Width - dMsg2Width;
			ClsLog.LogWrite("表面留白寬度:dWidthSpaceOutSide=" + dWidthSpaceOutSide + ";裏面留白寬度:dWidthSpaceInSide=" + dWidthSpaceInSide);
			GroupMsg_Whole(dWidthSpaceOutSide, 0.0, dWidthSpaceInSide, 0.0, strAdtMsgFile, ref dMsgWidthOutside, ref dMsgWidthInside);
			if (!File.Exists(strAdtMsgFile))
			{
				MessageBox.Show("模板文件不存在");
				return EnumGetAdtMsgResult.MsgNotExist;
			}
			if (ClsAdt.imCheckIsAdtMsg(strAdtMsgFile) != 1)
			{
				MessageBox.Show("不是ADT模板");
				return EnumGetAdtMsgResult.NotAdtMsg;
			}
			return EnumGetAdtMsgResult.AdtMsgOK;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("GetAdtMsgFile exception:" + ex.Message);
			MessageBox.Show("獲取ADT模板異常");
			return EnumGetAdtMsgResult.ConfigError;
		}
	}

	private int SendAdtMsg(string strAdtMsgPath)
	{
		if (!File.Exists(strAdtMsgPath))
		{
			return -3;
		}
		FileStream fs = File.Open(strAdtMsgPath, FileMode.Open);
		uint uiFileSize = (uint)fs.Length;
		byte[] ByteArrAdtMsgData = new byte[uiFileSize];
		fs.Read(ByteArrAdtMsgData, 0, (int)uiFileSize);
		int iRltAW = ClsAdt.imAppendMessage(ByteArrAdtMsgData, uiFileSize + 1, 0u, 0u);
		fs.Close();
		fs = null;
		return iRltAW;
	}

	private void ShowSendAdtMsgStatus(int iRltSAM)
	{
		switch (iRltSAM)
		{
		}
	}

	private bool AppendPageField2()
	{
		try
		{
			if (!AppendPageFieldSingle((m_strTextBoxModel.Trim().Length > 0 && m_strTextBoxModelValue.Length > 0) ? (m_strTextBoxModel + ":") : m_strTextBoxModel))
			{
				return false;
			}
			if (!AppendPageField_Pic2(m_msModelValueFrontBmp))
			{
				return false;
			}
			if (!AppendPageFieldSingle((m_strTextBoxBore.Trim().Length > 0 && m_strTextBoxBoreValue.Length > 0) ? (m_strTextBoxBore.Trim() + ":") : m_strTextBoxBore))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxBoreValue + m_strTextBoxBoreUnit))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxBoreUnit))
			{
				return false;
			}
			if (!AppendPageFieldSingle((m_strTextBoxOutput.Length > 0 && m_strTextBoxOutputValue.Length > 0 && m_strTextBoxOutputUnit.Length > 0) ? (m_strTextBoxOutput + ":") : m_strTextBoxOutput))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxOutputValue + m_strTextBoxOutputUnit))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxOutputUnit))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxPhaseValue))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxPhaseUnit))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxVoltageValue + m_strTextBoxVoltageUnit + m_strSpaceBetweenVoltageUnitAndHzValue + m_strTextBoxHzValue + m_strTextBoxHzUnit))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxVoltageUnit))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxHzValue + m_strTextBoxHzUnit))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxHzUnit))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxMadeNoValue))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxTokki))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxCodeValue + "   " + m_strTextBoxMachineNoValue1 + m_strTextBoxMachineNoValue2))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxCode + ":"))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxCodeValue))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strLblMacineNo))
			{
				return false;
			}
			if (!AppendPageFieldSingle(m_strTextBoxMachineNoValue1 + m_strTextBoxMachineNoValue2))
			{
				return false;
			}
			m_strTextBoxMachineNoValue2 = GetNextMachineNoValue2(m_strTextBoxMachineNoValue2);
			if (!AppendPageField_Pic2(m_msModelValueSideBmp))
			{
				return false;
			}
			if (!AppendPageField_Pic2(m_msPlaceBmp))
			{
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			return false;
		}
	}

	public string GetNextMachineNoValue2(string currentMachineNoValue2)
	{
		long lMachineNoValue2 = Convert.ToInt64(currentMachineNoValue2);
		string strNextMachieNoValue2 = ((!m_bReversePrintOrder) ? (lMachineNoValue2 + 1) : (lMachineNoValue2 - 1)).ToString();
		return strNextMachieNoValue2.PadLeft(currentMachineNoValue2.Length, '0');
	}

	private bool AppendPageFieldSingle(string strAdtFieldContent)
	{
		int iLen = 0;
		foreach (char c in strAdtFieldContent)
		{
			iLen = ((c >= '\u0080') ? (iLen + 2) : (iLen + 1));
		}
		int iRltAPF_Single = ClsAdt.imAppendPageField(1, strAdtFieldContent, iLen + 1);
		if (iRltAPF_Single != 1)
		{
			ClsLog.LogWrite("AppendPageField restult:" + iRltAPF_Single);
			return false;
		}
		return true;
	}

	private bool AppendPageField_Pic(string strContent, ClsMsg.clsMsgValue_Text ClsMsgValueText)
	{
		Bitmap bmp = ClsDrawText.DrawText4(ClsMsgValueText.strFontName, ClsMsgValueText.iFontSize, (FontStyle)ClsMsgValueText.iStyleType, strContent);
		MemoryStream msBmp = new MemoryStream();
		bmp.Save(msBmp, ImageFormat.Bmp);
		msBmp.Position = 0L;
		int iSize = (int)msBmp.Length;
		byte[] byteArrBmpData = new byte[iSize];
		msBmp.Read(byteArrBmpData, 0, iSize);
		msBmp.Close();
		int iRltAPF = ClsAdt.imAppendPageField(2, byteArrBmpData, iSize);
		bool bRlt = true;
		if (iRltAPF != 1)
		{
			bRlt = false;
		}
		return bRlt;
	}

	private bool AppendPageField_Pic2(MemoryStream msBmp)
	{
		msBmp.Position = 0L;
		int iSize = (int)msBmp.Length;
		byte[] byteArrBmpData = new byte[iSize];
		msBmp.Read(byteArrBmpData, 0, iSize);
		int iRltAPF = ClsAdt.imAppendPageField(2, byteArrBmpData, iSize);
		bool bRlt = true;
		if (iRltAPF != 1)
		{
			bRlt = false;
		}
		return bRlt;
	}

	private void WriteProductRecordDetail2Sql(int iPageSn)
	{
		try
		{
			string strPrefix = m_strProRecDetailTabName.Substring(0, 2).Trim().ToUpper();
			string strGuid = Guid.NewGuid().ToString();
			string strSqlKey = "INSERT " + m_strProRecDetailTabName + "([" + strPrefix + ClsProductRecordDetailField.strID + "],[" + strPrefix + ClsProductRecordDetailField.strPageNo + "],[" + strPrefix + ClsProductRecordDetailField.strKeyCode + "],[" + strPrefix + ClsProductRecordDetailField.strCaseNo + "],[" + strPrefix + ClsProductRecordDetailField.strMadeNo + "],[" + strPrefix + ClsProductRecordDetailField.strCurrentPrintBoxNo + "],[" + strPrefix + ClsProductRecordDetailField.strCurrentPrintMachineNo + "],[" + strPrefix + ClsProductRecordDetailField.strPageBeginTime + "],[" + strPrefix + ClsProductRecordDetailField.strBatchNo + "])";
			string strSqlValue = "  VALUES('" + strGuid + "'," + iPageSn + ",'" + textBoxKeyCodeValue.Text + "','" + textBoxCaseNo.Text + "','" + textBoxMadeNoValue.Text + "','" + m_iCurPrintBoxNo + "','" + m_strTextBoxMachineNoValue1 + m_strTextBoxMachineNoValue2 + "',GetDate(),'" + m_strBatchNo + "')";
			string strSql = strSqlKey + "  " + strSqlValue;
			if (ClsDataBase.SqlOperateData(strSql) <= 0)
			{
				ClsLog.LogWrite("Write product record error!Guid=" + strGuid + "  iPageSn=" + iPageSn);
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Write product record  exception:" + ex.Message);
		}
	}

	private void textBoxBoxCountBeginValue_TextChanged(object sender, EventArgs e)
	{
		try
		{
			if (btnPrint.Enabled && !ClsShare.CheckStrIsNum(((TextBox)sender).Text.Trim()))
			{
				((TextBox)sender).Text = "";
				MessageBox.Show("輸入必須為數字");
				return;
			}
			m_iPrintStopStatus = 1;
			m_strHasWritePrintDataEndBatchNo = "";
			m_ClsShowPrintStatusEventArgs.strHasPrintBoxCount = ((TextBox)sender).Text;
			this.ShowPrintStatusEvent(this, m_ClsShowPrintStatusEventArgs);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("textBoxBoxCountBeginValue_TextChanged exception:" + ex.Message);
		}
	}

	private void textBoxBoxCountEndValue_TextChanged(object sender, EventArgs e)
	{
		try
		{
			if (!ClsShare.CheckStrIsNum(((TextBox)sender).Text.Trim()))
			{
				((TextBox)sender).Text = "";
				MessageBox.Show("輸入必須為數字");
				return;
			}
			if (((TextBox)sender).Text.Length > 0)
			{
				m_iBoxNoEnd = Convert.ToInt32(((TextBox)sender).Text);
			}
			m_ClsShowPrintStatusEventArgs.strAllPrintBoxCount = ((TextBox)sender).Text;
			this.ShowPrintStatusEvent(this, m_ClsShowPrintStatusEventArgs);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("textBoxBoxCountEndValue_TextChanged exception:" + ex.Message);
		}
	}

	private void textBoxBoxNoBeginValue_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!ClsShare.OnlyInputNum(e.KeyChar))
		{
			e.Handled = true;
		}
	}

	private void pnlClientFront_Paint(object sender, PaintEventArgs e)
	{
	}

	private void textBoxMachineNoValue1Show_TextChanged(object sender, EventArgs e)
	{
		try
		{
			if ((TextBox)sender == textBoxMachineNoValue1Show)
			{
				if (!ClsShare.CheckStrIsAscii(((TextBox)sender).Text.Trim()))
				{
					MessageBox.Show("輸入必須為字母、數字或者符號");
					((TextBox)sender).Text = "";
					return;
				}
			}
			else if ((TextBox)sender == textBoxMachineNoValue2Show && !ClsShare.CheckStrIsNum(textBoxMachineNoValue2Show.Text.Trim()))
			{
				textBoxMachineNoValue2Show.Text = "";
				MessageBox.Show("輸入必須為數字");
				return;
			}
			textBoxMachineNoValueResult.Text = textBoxMachineNoValue1Show.Text + textBoxMachineNoValue2Show.Text;
			m_iPrintStopStatus = 1;
			m_strHasWritePrintDataEndBatchNo = "";
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("textBoxMachineNoValue2Show_TextChanged exception:" + ex.Message);
		}
	}

	private void textBoxMachineNoValue2Show_KeyUp(object sender, KeyEventArgs e)
	{
	}

	private void textBoxMachineNoValue2Show_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!ClsShare.OnlyInputNum(e.KeyChar))
		{
			e.Handled = true;
		}
	}

	private bool GroupMsg_Whole(double dWidthSpaceOutSide, double dHeightSpaceOutSide, double dWidthSpaceInSide, double dHeightSpaceInSide, string strAdtMsgFile, ref double dMsgWidthOutside, ref double dMsgWidthInside)
	{
		try
		{
			string strOutSideMsgFile = Application.StartupPath + "\\Msg\\Temp_OutSide.ms";
			string strInSideMsgFile = Application.StartupPath + "\\Msg\\Temp_InSide.ms";
			int iOneMsgMaxPrintHeadCount = Convert.ToInt32(ClsXml.GetOneMsgMaxPrintHeadCountOnGroupMsg());
			int iSinglePrintHeadHeight = Convert.ToInt32(ClsXml.GetSinglePrintHeadHeight());
			int iSingleMsgMaxHeight = iOneMsgMaxPrintHeadCount * iSinglePrintHeadHeight;
			ClsLog.LogWrite("表面模板合成開始");
			bool bRltOutside = GroupMsg_AB(ClsXml.GetSourceMsg1Path(), ClsXml.GetSourceMsg2Path(), dWidthSpaceOutSide, 0.0, bWidthOffset: true, bHeightOffset: false, strOutSideMsgFile, iSingleMsgMaxHeight);
			ClsLog.LogWrite(bRltOutside ? "表面模板合成成功!" : "表面模板合成失敗!");
			ClsLog.LogWrite("裏面模板合成開始");
			bool bRltInside = GroupMsg_AB(ClsXml.GetSourceMsg1Path(), ClsXml.GetSourceMsg2Path(), dWidthSpaceInSide, 0.0, bWidthOffset: true, bHeightOffset: false, strInSideMsgFile, iSingleMsgMaxHeight);
			ClsLog.LogWrite(bRltInside ? "裏面模板合成成功!" : "裏面模板合成失敗!");
			StringBuilder StrBuiMsgWidthOutsideTemp = new StringBuilder(255);
			ClsIni.GetPrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgKey_DataHead.strDataWidth, "0", StrBuiMsgWidthOutsideTemp, 255, strOutSideMsgFile);
			dMsgWidthOutside = Convert.ToDouble(StrBuiMsgWidthOutsideTemp.ToString());
			StringBuilder StrBuiMsgWidthInsideTemp = new StringBuilder(255);
			ClsIni.GetPrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgKey_DataHead.strDataWidth, "0", StrBuiMsgWidthInsideTemp, 255, strInSideMsgFile);
			dMsgWidthInside = Convert.ToDouble(StrBuiMsgWidthInsideTemp.ToString());
			ClsLog.LogWrite("整體模板合成開始");
			bool bRltWhole = GroupMsg_AB(strOutSideMsgFile, strInSideMsgFile, 0.0, 0.0, bWidthOffset: false, bHeightOffset: true, strAdtMsgFile, iSingleMsgMaxHeight * 2);
			ClsLog.LogWrite(bRltWhole ? "整體模板合成成功!" : "整體模板合成失敗!");
			StringBuilder StrBuiWidthTemp = new StringBuilder(255);
			ClsIni.GetPrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgKey_DataHead.strDataWidth, "0", StrBuiWidthTemp, 255, strAdtMsgFile);
			int iWidthTmp = Convert.ToInt32(Convert.ToDouble(StrBuiWidthTemp.ToString()));
			ClsIni.WritePrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgKey_DataHead.strDataWidth, iWidthTmp.ToString(), strAdtMsgFile);
			ClsIni.WritePrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgKey_DataHead.strName, "Adt Msg", strAdtMsgFile);
			if (bRltOutside && bRltInside && bRltWhole)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("GroupMsg_Whole exception:" + ex.Message);
			return false;
		}
	}

	private bool GroupMsg_AB(string strSrcMsg1Path, string strSrcMsg2Path, double dWidthSpace, double dHeightSpace, bool bWidthOffset, bool bHeightOffset, string strDestMsgPath, int iMsgMaxHeight)
	{
		try
		{
			MemoryStream msDestMsgInfo = new MemoryStream();
			MemoryStream msDestMsgObj = new MemoryStream();
			MemoryStream msDestMsgDataSrc = new MemoryStream();
			List<string> ListStrMsgPath = new List<string>();
			ListStrMsgPath.Add(strSrcMsg1Path);
			ListStrMsgPath.Add(strSrcMsg2Path);
			if (SeparateMsgList(ListStrMsgPath, msDestMsgInfo, msDestMsgObj, msDestMsgDataSrc, bSameMsgSrc: false))
			{
				List<double> ListDWidthSpace = new List<double>();
				List<double> ListDHeightSpace = new List<double>();
				ListDWidthSpace.Add(dWidthSpace);
				ListDHeightSpace.Add(dHeightSpace);
				ClsFile.DelExistFile(strDestMsgPath);
				if (GroupMsg(strDestMsgPath, msDestMsgInfo, msDestMsgObj, msDestMsgDataSrc, ListDWidthSpace, ListDHeightSpace, bWidthOffset, bHeightOffset))
				{
					ResetDataHeight(strDestMsgPath, iMsgMaxHeight);
					return true;
				}
				return false;
			}
			ClsLog.LogWrite("合成模板AB失敗!原因:分離模板失敗導致合成模板失敗！");
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("合成模板AB失敗!原因:GroupMsgAB exception:" + ex.Message);
			return false;
		}
	}

	private bool CheckDataHeight(string strMsgSrc, int iMsgMaxHeight)
	{
		StringBuilder StrBuiTemp = new StringBuilder(255);
		ClsIni.GetPrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgCommonKey.strHeight, "", StrBuiTemp, 255, strMsgSrc);
		try
		{
			int iHeightTmp = Convert.ToInt32(StrBuiTemp.ToString());
			if (iHeightTmp <= iMsgMaxHeight)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("CheckDataHeight exception" + ex.Message);
			return false;
		}
	}

	private void ResetDataHeight(string strMsgSrc, int iMsgMaxHeight)
	{
		try
		{
			ClsIni.WritePrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgCommonKey.strHeight, iMsgMaxHeight.ToString(), strMsgSrc);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("ResetDataHeight exception+ " + ex.Message);
		}
	}

	private bool SeparateMsgList(List<string> ListStrSrc, MemoryStream msDestMsgInfo, MemoryStream msDestMsgObj, MemoryStream msDestMsgDataSrc, bool bSameMsgSrc)
	{
		try
		{
			bool bRlt = true;
			foreach (string strSrc in ListStrSrc)
			{
				if (!SeparateMsg(strSrc, msDestMsgInfo, msDestMsgObj, msDestMsgDataSrc, bSameMsgSrc))
				{
					bRlt = false;
					break;
				}
			}
			return bRlt;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SeparateMsgList exception:" + ex.Message);
			return false;
		}
	}

	private bool SeparateMsg(string strSrc, MemoryStream msDestMsgInfo, MemoryStream msDestMsgObj, MemoryStream msDestMsgDataSrc, bool bSameMsgSrc)
	{
		try
		{
			FileStream fsMsg = File.OpenRead(strSrc);
			StreamReader srMsg = new StreamReader(fsMsg, Encoding.Default);
			string strLine = srMsg.ReadLine();
			ClsMsg.EnumMsgAppendPageField2Type EnumMsgAppendPageField2Type = ClsMsg.EnumMsgAppendPageField2Type.NotMsgAppendPageField2;
			while (strLine != null)
			{
				EnumMsgAppendPageField2Type = CheckAppendPageField2(strLine);
				switch (EnumMsgAppendPageField2Type)
				{
				case ClsMsg.EnumMsgAppendPageField2Type.MsgInfo:
					WriteMsg(ref EnumMsgAppendPageField2Type, srMsg, ref strLine, ref msDestMsgInfo);
					break;
				case ClsMsg.EnumMsgAppendPageField2Type.MsgObj:
					WriteMsg(ref EnumMsgAppendPageField2Type, srMsg, ref strLine, ref msDestMsgObj);
					break;
				case ClsMsg.EnumMsgAppendPageField2Type.MsgDataSrc:
					msDestMsgDataSrc.Position = (bSameMsgSrc ? 0 : msDestMsgDataSrc.Position);
					WriteMsg(ref EnumMsgAppendPageField2Type, srMsg, ref strLine, ref msDestMsgDataSrc);
					break;
				}
			}
			srMsg.Close();
			fsMsg.Close();
			return true;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SeparateMsg exception" + ex.Message);
			return false;
		}
	}

	private ClsMsg.EnumMsgAppendPageField2Type CheckAppendPageField2(string strSrc)
	{
		try
		{
			ClsMsg.EnumMsgAppendPageField2Type EnumMsgAppendPageField2Type = ClsMsg.EnumMsgAppendPageField2Type.NotMsgAppendPageField2;
			if (strSrc.Length > 0 && strSrc.Substring(0, 1).Trim().ToUpper() == "[" && strSrc.Substring(strSrc.Length - 1, 1) == "]")
			{
				if (strSrc.Substring(1, ClsMsg.clsMsgSection.strMsgInfo.Length) == ClsMsg.clsMsgSection.strMsgInfo)
				{
					EnumMsgAppendPageField2Type = ClsMsg.EnumMsgAppendPageField2Type.MsgInfo;
				}
				else if (strSrc.Substring(1, ClsMsg.clsMsgSection.strObj.Length).Trim().ToUpper() == ClsMsg.clsMsgSection.strObj.Trim().ToUpper())
				{
					EnumMsgAppendPageField2Type = ClsMsg.EnumMsgAppendPageField2Type.MsgObj;
				}
				else if (strSrc.Substring(1, ClsMsg.clsMsgSection.strDataSrc.Length).Trim().ToUpper() == ClsMsg.clsMsgSection.strDataSrc.Trim().ToUpper())
				{
					EnumMsgAppendPageField2Type = ClsMsg.EnumMsgAppendPageField2Type.MsgDataSrc;
				}
			}
			return EnumMsgAppendPageField2Type;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("CheckAppendPageField2 exception:" + strSrc + "   " + ex.Message);
			return ClsMsg.EnumMsgAppendPageField2Type.NotMsgAppendPageField2;
		}
	}

	private void WriteMsg(ref ClsMsg.EnumMsgAppendPageField2Type EnumMsgAppendPageField2Type, StreamReader srSrc, ref string strLineSrc, ref MemoryStream msDest)
	{
		try
		{
			do
			{
				if (strLineSrc.Replace('\0', ' ').Trim().Length > 0)
				{
					byte[] byteArrLineTmp = Encoding.Default.GetBytes(strLineSrc.Trim());
					msDest.Write(byteArrLineTmp, 0, byteArrLineTmp.Length);
					byte byteTmpEnd = 10;
					msDest.WriteByte(byteTmpEnd);
				}
				strLineSrc = srSrc.ReadLine();
				if (strLineSrc != null && strLineSrc.Length > 0)
				{
					EnumMsgAppendPageField2Type = CheckAppendPageField2(strLineSrc);
				}
			}
			while (strLineSrc != null && EnumMsgAppendPageField2Type == ClsMsg.EnumMsgAppendPageField2Type.NotMsgAppendPageField2);
			byte byteTmp = 10;
			msDest.WriteByte(byteTmp);
		}
		catch (Exception ex)
		{
			int num = (int)EnumMsgAppendPageField2Type;
			ClsLog.LogWrite("WriteMsg EnumMsgAppendPageField2Type=" + num + "exception" + ex.Message);
		}
	}

	private bool GroupMsg(string strDestMsg, MemoryStream msDestMsgInfo, MemoryStream msDestMsgObj, MemoryStream msDestMsgDataSrc, List<double> ListDWidthSpace, List<double> ListDHeightSpace, bool bWidthOffset, bool bHeightOffset)
	{
		FileStream fsMsgDest = new FileStream(strDestMsg, FileMode.Create, FileAccess.Write);
		StreamWriter swMsgDest = new StreamWriter(fsMsgDest, Encoding.Default);
		try
		{
			List<double> ListDWidth = new List<double>();
			List<double> ListDHeight = new List<double>();
			List<int> ListIAdtDataSrcFieldCount = new List<int>();
			List<ClsMsg.clsMsgObjT_Info> ListClsMsgObjT_Info = new List<ClsMsg.clsMsgObjT_Info>();
			int IDataSrcCount = 0;
			string strDataSrc = GroupMsgDataSrc(msDestMsgDataSrc, ref IDataSrcCount, ref ListIAdtDataSrcFieldCount, ref ListClsMsgObjT_Info);
			string strMsgInfo = GroupMsgMsgInfo(msDestMsgInfo, ref ListDWidth, ref ListDHeight, ListDWidthSpace, ListDHeightSpace, IDataSrcCount, bWidthOffset, bHeightOffset);
			string strObj = GroupMsgObj(msDestMsgObj, ListDWidthSpace, ListDHeightSpace, ListDWidth, ListDHeight, ListIAdtDataSrcFieldCount, bWidthOffset, bHeightOffset, ListClsMsgObjT_Info);
			swMsgDest.WriteLine(strMsgInfo);
			swMsgDest.WriteLine(strObj);
			swMsgDest.WriteLine(strDataSrc);
			swMsgDest.Close();
			fsMsgDest.Close();
			return true;
		}
		catch (Exception ex)
		{
			swMsgDest.Close();
			fsMsgDest.Close();
			ClsLog.LogWrite("GroupMsg exception:" + ex.Message);
			return false;
		}
	}

	private string GroupMsgMsgInfo(MemoryStream msDestMsgInfo, ref List<double> ListDWidth, ref List<double> ListDHeight, List<double> ListDWidthSpace, List<double> ListDHeightSpace, int IDataSrcCount, bool bWidthOffset, bool bHeightOffset)
	{
		StreamReader srMsgSrc = null;
		try
		{
			msDestMsgInfo.Position = 0L;
			srMsgSrc = new StreamReader(msDestMsgInfo, Encoding.Default);
			string strRlt = "";
			ClsMsg.clsMsgValue_DataHead clsMVDH = new ClsMsg.clsMsgValue_DataHead();
			for (string strLine = srMsgSrc.ReadLine(); strLine != null; strLine = srMsgSrc.ReadLine())
			{
				string strKey = "";
				string strValue = "";
				SeparateKeyValue(strLine, ref strKey, ref strValue);
				if (strKey == ClsMsg.clsMsgKey_DataHead.strVer)
				{
					clsMVDH.iVer = Convert.ToInt32(strValue);
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strDataNo)
				{
					clsMVDH.iNo = Convert.ToInt32(strValue);
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strDataID)
				{
					clsMVDH.iID = Convert.ToInt32(strValue);
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strName)
				{
					clsMVDH.strName = strValue;
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strVer)
				{
					clsMVDH.iVer = Convert.ToInt32(strValue);
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strDataWidth)
				{
					double dWidthTmp = Convert.ToDouble(strValue);
					string widthLog = "合成前主副模板資料總寬度 主模板clsMVDH.dWidth=" + clsMVDH.dWidth + " ,副模板dWidthTmp=" + dWidthTmp;
					widthLog += ((clsMVDH.dWidth > 0.0) ? "" : ",其中主模板clsMVDH.dWidth=0表示主模板不存在，將以副模板作為主模板;");
					if (bWidthOffset)
					{
						clsMVDH.dWidth += dWidthTmp;
					}
					else
					{
						clsMVDH.dWidth = ((clsMVDH.dWidth >= dWidthTmp) ? clsMVDH.dWidth : dWidthTmp);
					}
					ListDWidth.Add(dWidthTmp);
					widthLog = widthLog + "合成後模板資料總寬度 clsMVDH.dWidth=" + clsMVDH.dWidth;
					ClsLog.LogWrite(widthLog);
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strDataHeight)
				{
					double dHeightTmp = Convert.ToDouble(strValue);
					if (bHeightOffset)
					{
						clsMVDH.dHeight += dHeightTmp;
					}
					else
					{
						clsMVDH.dHeight = ((clsMVDH.dHeight >= dHeightTmp) ? clsMVDH.dHeight : dHeightTmp);
					}
					ListDHeight.Add(dHeightTmp);
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strDataAngle)
				{
					clsMVDH.iAngle = Convert.ToInt32(strValue);
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strCount)
				{
					clsMVDH.iCount += Convert.ToInt32(strValue);
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strdaCount)
				{
					clsMVDH.idaCount = IDataSrcCount;
				}
				else if (strKey == ClsMsg.clsMsgKey_DataHead.strdsid)
				{
					clsMVDH.idsid += Convert.ToInt32(strValue);
				}
			}
			srMsgSrc.Close();
			if (bWidthOffset)
			{
				foreach (double item in ListDWidthSpace)
				{
					double dWidthSpaceTmp = item;
					clsMVDH.dWidth += dWidthSpaceTmp;
				}
			}
			if (bHeightOffset)
			{
				foreach (double item2 in ListDHeightSpace)
				{
					double dHeightSpaceTmp = item2;
					clsMVDH.dHeight += dHeightSpaceTmp;
				}
			}
			strRlt = ClsMsg.clsMsgKey_DataHead.strMsgInfo + "\n" + ClsMsg.clsMsgKey_DataHead.strVer + "=" + clsMVDH.iVer + "\n" + ClsMsg.clsMsgKey_DataHead.strDataNo + "=" + clsMVDH.iNo + "\n" + ClsMsg.clsMsgKey_DataHead.strDataID + "=" + clsMVDH.iID + "\n" + ClsMsg.clsMsgKey_DataHead.strName + "=" + clsMVDH.strName + "\n" + ClsMsg.clsMsgKey_DataHead.strDataWidth + "=" + clsMVDH.dWidth + "\n" + ClsMsg.clsMsgKey_DataHead.strDataHeight + "=" + clsMVDH.dHeight + "\n" + ClsMsg.clsMsgKey_DataHead.strDataAngle + "=" + clsMVDH.iAngle + "\n" + ClsMsg.clsMsgKey_DataHead.strCount + "=" + clsMVDH.iCount + "\n" + ClsMsg.clsMsgKey_DataHead.strdaCount + "=" + clsMVDH.idaCount + "\n" + ClsMsg.clsMsgKey_DataHead.strdsid + "=" + clsMVDH.idsid + "\r\n";
			return strRlt.Trim() + "\r\n";
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("GroupMsgMsgInfo exception:" + ex.Message);
			srMsgSrc.Close();
			return null;
		}
	}

	private string GroupMsgObj(MemoryStream msDestMsgObj, List<double> ListDWidthSpace, List<double> ListDHeightSpace, List<double> ListDWidth, List<double> ListDHeight, List<int> ListIAdtDataSrcFieldCount, bool bWidthOffset, bool bHeightOffset, List<ClsMsg.clsMsgObjT_Info> ListClsMsgObjT_Info)
	{
		StreamReader srMsgSrc = null;
		try
		{
			msDestMsgObj.Position = 0L;
			srMsgSrc = new StreamReader(msDestMsgObj, Encoding.Default);
			string strRlt = "";
			string strLine = srMsgSrc.ReadLine();
			int iObjIndex = 1;
			int iMsgUnitIndex = -1;
			string strNextMsgUnitTag = "[" + ClsMsg.clsMsgCommonKey.strObject + "1]";
			while (strLine != null)
			{
				string strKey = "";
				string strValue = "";
				if (ClsMsg.EnumMsgAppendPageField2Type.MsgObj == CheckAppendPageField2(strLine))
				{
					if (strLine == strNextMsgUnitTag)
					{
						iMsgUnitIndex++;
					}
					strRlt = strRlt + "[" + ClsMsg.clsMsgCommonKey.strObject + iObjIndex++ + "]\n";
				}
				else
				{
					string strLineTmp = "";
					if (iMsgUnitIndex > 0)
					{
						SeparateKeyValue(strLine, ref strKey, ref strValue);
						if (strKey == ClsMsg.clsMsgCommonKey.strX)
						{
							double dWidthTmp = 0.0;
							if (bWidthOffset)
							{
								for (int iListDWidthIndex = 0; iListDWidthIndex < iMsgUnitIndex; iListDWidthIndex++)
								{
									dWidthTmp = dWidthTmp + ListDWidth[iListDWidthIndex] + ListDWidthSpace[iListDWidthIndex];
								}
							}
							strLineTmp = strKey + "=" + (Convert.ToInt32(strValue) + Convert.ToInt32(dWidthTmp));
						}
						else if (strKey == ClsMsg.clsMsgCommonKey.strY)
						{
							double dHeightTmp = 0.0;
							if (bHeightOffset)
							{
								for (int iListDHeightIndex = 0; iListDHeightIndex < iMsgUnitIndex; iListDHeightIndex++)
								{
									dHeightTmp = dHeightTmp + ListDHeight[iListDHeightIndex] + ListDHeightSpace[iListDHeightIndex];
								}
							}
							strLineTmp = strKey + "=" + (Convert.ToInt32(strValue) + Convert.ToInt32(dHeightTmp));
						}
						else if (strKey.Length > ClsMsg.clsMsgObjToSrcParam.strMsgObjT.Length)
						{
							if (strKey.Substring(0, ClsMsg.clsMsgObjToSrcParam.strMsgObjT.Length) == ClsMsg.clsMsgObjToSrcParam.strMsgObjT)
							{
								int iDataSrcIndex = 0;
								int iDataSrcFieldIndex = 0;
								string strDataSrcInfoOnCurObj = "";
								string[] strArrTmp = strValue.Split(',');
								iDataSrcIndex = Convert.ToInt32(strArrTmp[0]);
								iDataSrcFieldIndex = Convert.ToInt32(strArrTmp[1]);
								for (int iIndex = 2; iIndex < strArrTmp.Count(); iIndex++)
								{
									strDataSrcInfoOnCurObj += strArrTmp[iIndex];
								}
								if (ListClsMsgObjT_Info != null)
								{
									for (int iListMsgObjT_InfoIndex = 0; iListMsgObjT_InfoIndex < ListClsMsgObjT_Info.Count; iListMsgObjT_InfoIndex++)
									{
										if (ListClsMsgObjT_Info[iListMsgObjT_InfoIndex].iOriginalMsgUnitIndex == iMsgUnitIndex && ListClsMsgObjT_Info[iListMsgObjT_InfoIndex].iOriginalDataSrcIndex == iDataSrcIndex && ListClsMsgObjT_Info[iListMsgObjT_InfoIndex].iOriginalDataSrcFieldIndex == iDataSrcFieldIndex)
										{
											iDataSrcIndex = ListClsMsgObjT_Info[iListMsgObjT_InfoIndex].iNewDataSrcIndex;
											iDataSrcFieldIndex = ListClsMsgObjT_Info[iListMsgObjT_InfoIndex].iNewDataSrcFieldIndex;
											break;
										}
									}
								}
								strLineTmp = strKey + "=" + iDataSrcIndex + "," + iDataSrcFieldIndex + "," + strDataSrcInfoOnCurObj;
							}
							else
							{
								strLineTmp = strLine;
							}
						}
						else
						{
							strLineTmp = strLine;
						}
					}
					else
					{
						strLineTmp = strLine;
					}
					strRlt = strRlt + strLineTmp + "\n";
				}
				strLine = srMsgSrc.ReadLine();
			}
			srMsgSrc.Close();
			return strRlt.Trim() + "\r\n";
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("GroupMsgMsgInfo exception:" + ex.Message);
			srMsgSrc.Close();
			return null;
		}
	}

	private string GroupMsgDataSrc(MemoryStream msDestMsgInfo, ref int IDataSrcCount, ref List<int> ListIAdtDataSrcFieldCount, ref List<ClsMsg.clsMsgObjT_Info> ListClsMsgObjT_Info)
	{
		StreamReader srMsgSrc = null;
		try
		{
			msDestMsgInfo.Position = 0L;
			srMsgSrc = new StreamReader(msDestMsgInfo, Encoding.Default);
			int iDataSrcIndex = 0;
			string strRlt = "";
			string strLine = srMsgSrc.ReadLine();
			ClsMsg.clsMsgDataSrcValue clsMDSV = new ClsMsg.clsMsgDataSrcValue();
			int iOriginalMsgUnitIndex = 0;
			int iOriginalDataSrcIndex = 0;
			while (strLine != null)
			{
				string strKey = "";
				string strValue = "";
				SeparateKeyValue(strLine, ref strKey, ref strValue);
				if (CheckAppendPageField2(strLine) == ClsMsg.EnumMsgAppendPageField2Type.MsgDataSrc)
				{
					string strNewMsgUnitTag = "[" + ClsMsg.clsMsgSection.strDataSrc + "0]";
					if (strLine.Trim() == strNewMsgUnitTag)
					{
						iOriginalMsgUnitIndex++;
					}
					else
					{
						int iDataSrcIndexTmp = Convert.ToInt32(strLine.Substring(1, strLine.Length - ClsMsg.clsMsgSection.strDataSrc.Length - 2));
						iOriginalDataSrcIndex = iDataSrcIndexTmp;
					}
				}
				else if (strKey.Trim() == ClsMsg.clsMsgDataSrcKey.strUID)
				{
					clsMDSV.strUID = strValue;
					IDataSrcCount++;
				}
				else if (strKey.Trim() == ClsMsg.clsMsgDataSrcKey.strName)
				{
					clsMDSV.strName = strValue;
				}
				else if (strKey.Trim() == ClsMsg.clsMsgDataSrcKey.strGroup)
				{
					clsMDSV.strGroup = strValue;
				}
				else if (strKey.Trim() == ClsMsg.clsMsgDataSrcKey.strData)
				{
					int iNewDataSrcIndex = 0;
					if (clsMDSV.strUID == ClsMsg.clsDataSrcUID.strUID_DATASRC_ADT)
					{
						int iOriginalAdtFieldCount = 0;
						int iNewAdtFieldCount = 0;
						string strAdtFieldCount = "";
						string strOriginalAdtFieldInfo = "";
						string strNewAdtFieldInfo = "";
						string strDataTmp = clsMDSV.strData;
						string[] strArrOriginalAdtFieldInfo = null;
						iNewDataSrcIndex = 0;
						if (strDataTmp != null && strDataTmp.Length >= 2)
						{
							iOriginalAdtFieldCount = Convert.ToInt32(strDataTmp.Substring(0, 2), 16);
							strOriginalAdtFieldInfo = strDataTmp.Substring(2, iOriginalAdtFieldCount * (ClsMsg.clsDataSrc_AdtParam.iOneAdtField * 2)).Trim();
							strArrOriginalAdtFieldInfo = new string[iOriginalAdtFieldCount];
							for (int iOriginalAdtFieldInfoIndex = 0; iOriginalAdtFieldInfoIndex < iOriginalAdtFieldCount; iOriginalAdtFieldInfoIndex++)
							{
								strArrOriginalAdtFieldInfo[iOriginalAdtFieldInfoIndex] = strDataTmp.Substring(2 + iOriginalAdtFieldInfoIndex * (ClsMsg.clsDataSrc_AdtParam.iOneAdtField * 2), ClsMsg.clsDataSrc_AdtParam.iOneAdtField * 2);
							}
						}
						if (strValue != null && strValue.Length >= 2)
						{
							iNewAdtFieldCount = Convert.ToInt32(strValue.Substring(0, 2), 16);
							int iNewAdtFieldCountTmp = iNewAdtFieldCount;
							for (int iNewAdtFieldInfoIndex = 0; iNewAdtFieldInfoIndex < iNewAdtFieldCountTmp; iNewAdtFieldInfoIndex++)
							{
								string strNewAdtFieldInfoTmp = strValue.Substring(2 + iNewAdtFieldInfoIndex * (ClsMsg.clsDataSrc_AdtParam.iOneAdtField * 2), ClsMsg.clsDataSrc_AdtParam.iOneAdtField * 2);
								ClsMsg.clsMsgObjT_Info clsMBTI = new ClsMsg.clsMsgObjT_Info();
								if (strArrOriginalAdtFieldInfo != null)
								{
									if (strArrOriginalAdtFieldInfo.Contains(strNewAdtFieldInfoTmp))
									{
										iNewAdtFieldCount--;
										for (int iAdtFieldInfoIndexTmp = 0; iAdtFieldInfoIndexTmp < strArrOriginalAdtFieldInfo.Count(); iAdtFieldInfoIndexTmp++)
										{
											if (strArrOriginalAdtFieldInfo[iAdtFieldInfoIndexTmp] == strNewAdtFieldInfoTmp)
											{
												clsMBTI.iNewDataSrcFieldIndex = iAdtFieldInfoIndexTmp;
												strNewAdtFieldInfoTmp = "";
												break;
											}
										}
									}
									else
									{
										int iLastDataSrcFieldIndex = iOriginalAdtFieldCount - 1;
										if (ListClsMsgObjT_Info != null)
										{
											for (int iListMsgObjT_InfoIndex = 0; iListMsgObjT_InfoIndex < ListClsMsgObjT_Info.Count; iListMsgObjT_InfoIndex++)
											{
												if (ListClsMsgObjT_Info[iListMsgObjT_InfoIndex].iOriginalMsgUnitIndex == iOriginalMsgUnitIndex - 1)
												{
													int iNewDataSrcFieldIndexTmp = ListClsMsgObjT_Info[iListMsgObjT_InfoIndex].iNewDataSrcFieldIndex;
													iLastDataSrcFieldIndex = ((iLastDataSrcFieldIndex > iNewDataSrcFieldIndexTmp) ? iLastDataSrcFieldIndex : iNewDataSrcFieldIndexTmp);
												}
											}
										}
										clsMBTI.iNewDataSrcFieldIndex = iLastDataSrcFieldIndex + 1;
									}
								}
								else
								{
									clsMBTI.iNewDataSrcFieldIndex = iNewAdtFieldInfoIndex;
								}
								clsMBTI.iOriginalMsgUnitIndex = iOriginalMsgUnitIndex - 1;
								clsMBTI.iOriginalDataSrcIndex = iOriginalDataSrcIndex;
								clsMBTI.iOriginalDataSrcFieldIndex = iNewAdtFieldInfoIndex;
								clsMBTI.iNewDataSrcIndex = iNewDataSrcIndex;
								strNewAdtFieldInfo += strNewAdtFieldInfoTmp;
								ListClsMsgObjT_Info.Add(clsMBTI);
							}
							ListIAdtDataSrcFieldCount.Add(iNewAdtFieldCount);
						}
						strAdtFieldCount = $"{iOriginalAdtFieldCount + iNewAdtFieldCount:X2}";
						clsMDSV.strData = strAdtFieldCount + strOriginalAdtFieldInfo + strNewAdtFieldInfo;
					}
				}
				strLine = srMsgSrc.ReadLine();
			}
			srMsgSrc.Close();
			clsMDSV.strData = clsMDSV.strData.PadRight(ClsMsg.clsDataSrc_AdtParam.iAdtDataMaxLen * 2, '0');
			strRlt = strRlt + "\n[" + ClsMsg.clsMsgDataSrcKey.strDataSrcTag + iDataSrcIndex + "]\n" + ClsMsg.clsMsgDataSrcKey.strUID + "=" + clsMDSV.strUID + "\n" + ClsMsg.clsMsgDataSrcKey.strName + "=" + clsMDSV.strName + "\n" + ClsMsg.clsMsgDataSrcKey.strGroup + "=" + clsMDSV.strGroup + "\n" + ClsMsg.clsMsgDataSrcKey.strData + "=" + clsMDSV.strData + "\n";
			return strRlt.Trim() + "\r\n";
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("GroupMsgDataSrc exception:" + ex.Message);
			srMsgSrc.Close();
			return null;
		}
	}

	private void SeparateKeyValue(string strSrc, ref string strKey, ref string strValue)
	{
		int iSplit = strSrc.IndexOf("=");
		if (iSplit >= 0)
		{
			strKey = strSrc.Substring(0, iSplit);
			strValue = strSrc.Substring(iSplit + 1, strSrc.Length - (iSplit + 1));
		}
	}

	private void frmPrintCtrl_VisibleChanged(object sender, EventArgs e)
	{
		try
		{
			if (!base.Visible)
			{
				return;
			}
			string strKeyCode = textBoxKeyCodeValue.Text.Trim().ToUpper();
			if (strKeyCode.Length > 0)
			{
				string strTabName = ClsXml.GetPrintDataTableName();
				string strSql = "SELECT *  FROM " + strTabName + "  WHERE [" + strTabName.Trim().ToUpper().Substring(0, 2) + "Key code]='" + strKeyCode + "'";
				DataTable dtTmp = ClsDataBase.SqlReadData(strSql);
				if (dtTmp != null && dtTmp.Rows.Count > 0 && dtTmp.Columns.Count > 0)
				{
					for (int iColumnIndex = 0; iColumnIndex < dtTmp.Columns.Count; iColumnIndex++)
					{
						dtTmp.Columns[iColumnIndex].Caption = dtTmp.Columns[iColumnIndex].Caption.Substring(2);
					}
					ImportData(dtTmp);
				}
			}
			ImportCartonSize();
		}
		catch
		{
		}
	}

	private void btnPrint_Click(object sender, EventArgs e)
	{
		lblReadyPrintTip.Visible = true;
		Refresh();
		double dMsgWidthOutside = 0.0;
		double dMsgWidthInside = 0.0;
		if (comboBoxPrintOrder.SelectedIndex == 0)
		{
			m_bReversePrintOrder = false;
		}
		else if (comboBoxPrintOrder.SelectedIndex == 1)
		{
			m_bReversePrintOrder = true;
		}
		if (!ReadyPrint(ref dMsgWidthOutside, ref dMsgWidthInside))
		{
			lblReadyPrintTip.Visible = false;
			Refresh();
			return;
		}
		m_adtMutex.WaitOne(m_adtMutexTimeOut_ms);
		if (!m_bPause)
		{
			DataTable delayTable = GetPrintHeadDelaySet(dMsgWidthOutside, dMsgWidthInside);
			if (delayTable == null)
			{
				ClsLog.LogWrite("延遲列表為空");
				string appPath = Application.StartupPath;
				string strAdtTempFile = appPath + "\\Msg\\AdtTemp.ms";
				string strOutSideMsgFile = appPath + "\\Msg\\Temp_OutSide.ms";
				string strInSideMsgFile = appPath + "\\Msg\\Temp_InSide.ms";
				string strAdtTempFile_log = appPath + "\\Log\\AdtTemp" + $"{DateTime.Now.Year:0000}{DateTime.Now.Month:00}{DateTime.Now.Day:00}_{DateTime.Now.Hour:00}{DateTime.Now.Minute:00}{DateTime.Now.Second:00}" + ".ms";
				string strOutSideMsgFile_log = appPath + "\\Log\\Temp_OutSide" + $"{DateTime.Now.Year:0000}{DateTime.Now.Month:00}{DateTime.Now.Day:00}_{DateTime.Now.Hour:00}{DateTime.Now.Minute:00}{DateTime.Now.Second:00}" + ".ms";
				string strInSideMsgFile_log = appPath + "\\Log\\Temp_InSide" + $"{DateTime.Now.Year:0000}{DateTime.Now.Month:00}{DateTime.Now.Day:00}_{DateTime.Now.Hour:00}{DateTime.Now.Minute:00}{DateTime.Now.Second:00}" + ".ms";
				if (File.Exists(strOutSideMsgFile))
				{
					File.Delete(strOutSideMsgFile_log);
					File.Copy(strOutSideMsgFile, strOutSideMsgFile_log);
				}
				else
				{
					ClsLog.LogWrite("合成的表面模板Temp_OutSide.ms不存在!");
				}
				if (File.Exists(strInSideMsgFile))
				{
					File.Delete(strInSideMsgFile_log);
					File.Copy(strInSideMsgFile, strInSideMsgFile_log);
				}
				else
				{
					ClsLog.LogWrite("合成的裏面模板Temp_InSide.ms不存在!");
				}
				if (File.Exists(strAdtTempFile))
				{
					File.Delete(strAdtTempFile_log);
					File.Copy(strAdtTempFile, strAdtTempFile_log);
				}
				else
				{
					ClsLog.LogWrite("合成的整體模板AdtTemp.ms不存在!");
				}
				MessageBox.Show("延遲設定失敗，請重新開始噴印!");
				btnPrint.Enabled = true;
				btnStop.Enabled = false;
				lblReadyPrintTip.Visible = false;
				Refresh();
				return;
			}
			if (!PrintHeadDelaySet(delayTable))
			{
				lblReadyPrintTip.Visible = false;
				Refresh();
				m_adtMutex.ReleaseMutex();
				MessageBox.Show("噴印延遲設定失敗");
				return;
			}
		}
		if (ClsAdt.imGetAdtMsgFieldCount(m_strAdtMsgFile) != ClsXml.GetAdtFieldCount())
		{
			lblReadyPrintTip.Visible = false;
			Refresh();
			m_adtMutex.ReleaseMutex();
			MessageBox.Show("ADT字段數不符合");
			return;
		}
		if (!m_bPause)
		{
			byte[] array = new byte[39];
			array[0] = 1;
			ClsAdt.imInitADT(array);
			ClsAdt.imPrinterStart();
			ClsAdt.imClearReturnQueue();
			ClsAdt.imClearPrintQueue();
			ClsAdt.imSendCancel();
			bool bPrintStatus = false;
			while (!bPrintStatus)
			{
				Thread.Sleep(3000);
				int[] iArrTmp = new int[1];
				ClsAdt.imGetPrinterStatus(iArrTmp);
				if (iArrTmp[0] == 1)
				{
					bPrintStatus = true;
					break;
				}
			}
			if (SendAdtMsg(m_strAdtMsgFile) != 1)
			{
				MessageBox.Show("發送ADT模板失败，請確認配置是否正確!");
				m_adtMutex.ReleaseMutex();
				return;
			}
			IniPrintData();
			IniPrintParam();
			m_bSendDataThreadRun = true;
			m_strBatchNo = Guid.NewGuid().ToString();
			WriteToProRecMasterTab(m_strBatchNo, textBoxKeyCodeValue.Text.Trim(), textBoxMadeNoValue.Text.Trim(), textBoxCaseNo.Text.Trim(), m_strBeginPrintTime, textBoxBoxNoBeginValue.Text.Trim(), m_strMachineNoBegin);
			m_iCurPrintBoxNo = Convert.ToInt32(textBoxBoxNoBeginValue.Text) + 1;
			IniLastMachineNoValue(ref m_strLastMachineNoValue);
			m_strBoxNoEndValue = textBoxBoxNoEndValue.Text.Trim();
			m_strBeginMachineNoValue1 = textBoxMachineNoValue1Show.Text.Trim();
			m_strBeginMachineNoValue2 = textBoxMachineNoValue2Show.Text.Trim();
			IniSpecialSendDataParam();
			StartSendDataThread();
			m_updateFailSnList = new List<uint>();
			m_waitDataTime_s = ClsXml.GetDataBufferTime();
			if (m_waitDataTime_s > 0)
			{
				timerWaitData.Enabled = true;
			}
		}
		m_bPause = false;
		m_bSendDataThreadRun = true;
		this.ShowPrintStatusEvent(this, m_ClsShowPrintStatusEventArgs);
		m_adtMutex.ReleaseMutex();
	}

	private bool ReadyPrint(ref double dMsgWidthOutside, ref double dMsgWidthInside)
	{
		if (!CheckPrintConditon())
		{
			return false;
		}
		if (GetAdtMsgFile(ref m_strAdtMsgFile, ref dMsgWidthOutside, ref dMsgWidthInside) != EnumGetAdtMsgResult.AdtMsgOK)
		{
			ClsLog.LogWrite("合成模板失敗");
			return false;
		}
		return true;
	}

	private void IniLastMachineNoValue(ref string str)
	{
		string strTmp = textBoxMachineNoValue2Show.Text;
		long lMachineNoValue = Convert.ToInt64(strTmp) - 1;
		string strNextMachieNoValue = "";
		bool bHasSign = false;
		int iStrLen = strTmp.Length;
		if (lMachineNoValue < 0)
		{
			lMachineNoValue = Math.Abs(lMachineNoValue);
			bHasSign = true;
			iStrLen--;
		}
		strNextMachieNoValue = lMachineNoValue.ToString();
		strNextMachieNoValue = strNextMachieNoValue.PadLeft(iStrLen, '0');
		if (bHasSign)
		{
			str = "-" + strNextMachieNoValue;
		}
		else
		{
			str = strNextMachieNoValue;
		}
	}

	private void IniSpecialSendDataParam()
	{
		m_ClsMsgValueText_ModelValueFront = new ClsMsg.clsMsgValue_Text();
		ClsXml.GetModelValueObjIndex_Front();
		m_ClsMsgValueText_ModelValueFront.strFontName = ClsXml.GetModelValueFontName_Front();
		m_ClsMsgValueText_ModelValueFront.iFontSize = ClsXml.GetModelValueFontSize_Front();
		m_ClsMsgValueText_ModelValueFront.iStyleType = ClsXml.GetModelValueFontStyle_Front();
		Bitmap bmpModelValueFront = null;
		ConvertObjToBmpStream(m_ClsMsgValueText_ModelValueFront, textBoxModelValue.Text.Trim(), ref bmpModelValueFront);
		m_msModelValueFrontBmp = new MemoryStream();
		bmpModelValueFront.Save(m_msModelValueFrontBmp, ImageFormat.Bmp);
		m_ClsMsgValueText_ModelValueSide = new ClsMsg.clsMsgValue_Text();
		ClsXml.GetModelValueObjIndex_Side();
		m_ClsMsgValueText_ModelValueSide.strFontName = ClsXml.GetModelValueFontName_Side();
		m_ClsMsgValueText_ModelValueSide.iFontSize = ClsXml.GetModelValueFontSize_Side();
		m_ClsMsgValueText_ModelValueSide.iStyleType = ClsXml.GetModelValueFontStyle_Side();
		Bitmap bmpModelValueSide = null;
		ConvertObjToBmpStream(m_ClsMsgValueText_ModelValueSide, textBoxModelValue.Text.Trim(), ref bmpModelValueSide);
		m_ClsMsgValueText_Place = new ClsMsg.clsMsgValue_Text();
		ClsXml.GetPlaceObjIndex();
		m_ClsMsgValueText_Place.strFontName = ClsXml.GetPlaceFontName();
		m_ClsMsgValueText_Place.iFontSize = ClsXml.GetPlaceFontSize();
		m_ClsMsgValueText_Place.iStyleType = ClsXml.GetPlaceFontStyle();
		Bitmap bmpPlace = null;
		ConvertObjToBmpStream(m_ClsMsgValueText_Place, textBoxPlace.Text.Trim(), ref bmpPlace);
		RightAlignModelValueSideAndPlace(bmpModelValueSide, bmpPlace, ref m_msModelValueSideBmp, ref m_msPlaceBmp);
		string strSideMsgFilePath = ClsXml.GetSourceMsg2Path();
		int iObjIndexOfPlaceField = 1;
		string strSectionModelValueSide = ClsMsg.clsMsgSection.strObj + 2;
		string strSectionPlace = ClsMsg.clsMsgSection.strObj + iObjIndexOfPlaceField;
		string strKeyWidth = "Width";
		string strKeyHeight = "Height";
		int iModelValueSideWidth = Convert.ToInt32(ClsIni.GetValue(strSideMsgFilePath, strSectionModelValueSide, strKeyWidth));
		int iModelValueSideHeight = Convert.ToInt32(ClsIni.GetValue(strSideMsgFilePath, strSectionModelValueSide, strKeyHeight));
		int iPlaceWidth = Convert.ToInt32(ClsIni.GetValue(strSideMsgFilePath, strSectionPlace, strKeyWidth));
		int iPlaceHeight = Convert.ToInt32(ClsIni.GetValue(strSideMsgFilePath, strSectionPlace, strKeyHeight));
		RightAlignBitmap(bmpModelValueSide, ref m_msModelValueSideBmp, iModelValueSideWidth, iModelValueSideHeight);
		RightAlignBitmap(bmpPlace, ref m_msPlaceBmp, iPlaceWidth, iPlaceHeight);
		m_strSpaceBetweenVoltageUnitAndHzValue = m_strSpaceBetweenVoltageUnitAndHzValue.PadLeft(ClsXml.GetSpaceBetweenVoltageUnitAndHzValue());
	}

	private void ConvertObjToBmpStream(ClsMsg.clsMsgValue_Text clsMsgValueText, string strContent, ref Bitmap bmpDest)
	{
		Bitmap bmp = ClsDrawText.DrawText4(clsMsgValueText.strFontName, clsMsgValueText.iFontSize, (FontStyle)clsMsgValueText.iStyleType, strContent);
		bmpDest = bmp;
	}

	private void RightAlignBitmap(Bitmap bitmapSource, ref MemoryStream memoryStreamDest, int iFixWidth, int iFixHeight)
	{
		Bitmap bitmapTemp = new Bitmap(iFixWidth, iFixHeight);
		Graphics g = Graphics.FromImage(bitmapTemp);
		g.Clear(Color.White);
		int imageX = iFixWidth - bitmapSource.Width;
		int imageY = (iFixHeight - bitmapSource.Height) / 2;
		g.DrawImage(bitmapSource, imageX, imageY);
		Bitmap bmpDest = null;
		ClsPic.PicToBwBmp(bitmapTemp, ref bmpDest);
		memoryStreamDest = new MemoryStream();
		bmpDest.Save(memoryStreamDest, ImageFormat.Bmp);
		g.Dispose();
		bitmapTemp.Dispose();
	}

	private void ConvertPlaceObjToBmpStream(ClsMsg.clsMsgValue_Text clsMsgValueText, string strContent, ref MemoryStream msDest, int iBmpWidth)
	{
		Bitmap bmp = ClsDrawText.DrawText4(clsMsgValueText.strFontName, clsMsgValueText.iFontSize, (FontStyle)clsMsgValueText.iStyleType, strContent);
		Bitmap bmpTmp = new Bitmap(iBmpWidth, bmp.Height);
		Graphics g = Graphics.FromImage(bmpTmp);
		g.DrawImage(bmp, iBmpWidth - bmp.Width, 0);
		Bitmap bmpDest = null;
		ClsPic.PicToBwBmp(bmpTmp, ref bmpDest);
		msDest = new MemoryStream();
		bmpDest.Save(msDest, ImageFormat.Bmp);
	}

	private void RightAlignModelValueSideAndPlace(Bitmap bmpModelValueSide, Bitmap bmpPlace, ref MemoryStream msModelValueSideBmp, ref MemoryStream msPlaceBmp)
	{
		if (bmpModelValueSide.Width >= bmpPlace.Width)
		{
			msModelValueSideBmp = new MemoryStream();
			bmpModelValueSide.Save(msModelValueSideBmp, ImageFormat.Bmp);
			Bitmap bmpTmp = new Bitmap(bmpModelValueSide.Width, bmpPlace.Height);
			Graphics g = Graphics.FromImage(bmpTmp);
			g.DrawImage(bmpPlace, bmpModelValueSide.Width - bmpPlace.Width, 0);
			Bitmap bmpDest = null;
			ClsPic.PicToBwBmp(bmpTmp, ref bmpDest);
			msPlaceBmp = new MemoryStream();
			bmpDest.Save(msPlaceBmp, ImageFormat.Bmp);
		}
		else
		{
			msPlaceBmp = new MemoryStream();
			bmpPlace.Save(msPlaceBmp, ImageFormat.Bmp);
			Bitmap bmpTmp2 = new Bitmap(bmpPlace.Width, bmpModelValueSide.Height);
			Graphics g2 = Graphics.FromImage(bmpTmp2);
			g2.DrawImage(bmpModelValueSide, bmpPlace.Width - bmpModelValueSide.Width, 0);
			Bitmap bmpDest2 = null;
			ClsPic.PicToBwBmp(bmpTmp2, ref bmpDest2);
			msModelValueSideBmp = new MemoryStream();
			bmpDest2.Save(msModelValueSideBmp, ImageFormat.Bmp);
		}
	}

	private void StartSendDataThread()
	{
		m_ThreadSendData = new Thread(SendDataThread);
		m_ThreadSendData.IsBackground = true;
		m_ThreadSendData.Start();
	}

	private bool CheckPrintConditon()
	{
		try
		{
			if (!CheckPrintData())
			{
				return false;
			}
			if (!m_bPause && toolStripStatusLabelPrintStatusText.Text != ClsAdt.ClsPrintStatus.strPrintStop)
			{
				MessageBox.Show(toolStripStatusLabelPrintStatusText.Text);
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			return false;
		}
	}

	private bool CheckPrintData()
	{
		try
		{
			if (textBoxKeyCodeValue.Text.Length <= 0)
			{
				MessageBox.Show("Key code 不可為空!");
				return false;
			}
			if (textBoxMadeNoValue.Text.Length <= 0)
			{
				MessageBox.Show("製番不可為空!");
				return false;
			}
			string strBeginBoxCountNo = textBoxBoxNoBeginValue.Text;
			string strEndBoxCountNo = textBoxBoxNoEndValue.Text;
			if (strBeginBoxCountNo.Length <= 0)
			{
				MessageBox.Show("開始噴印箱數不可為空");
				return false;
			}
			if (strEndBoxCountNo.Length <= 0)
			{
				MessageBox.Show("最大噴印箱數不可為空");
				return false;
			}
			if (Convert.ToInt32(strBeginBoxCountNo) >= Convert.ToInt32(strEndBoxCountNo))
			{
				MessageBox.Show("開始噴印箱數必須小於噴印箱數");
				return false;
			}
			if (textBoxMachineNoValue1Show.Text.Trim().Length <= 0 || textBoxMachineNoValue2Show.Text.Trim().Length <= 0)
			{
				MessageBox.Show("機番不可為空!");
				return false;
			}
			int iMinimumMachineNo = 0;
			if (comboBoxPrintOrder.SelectedIndex == 0)
			{
				m_bReversePrintOrder = false;
			}
			else if (comboBoxPrintOrder.SelectedIndex == 1)
			{
				m_bReversePrintOrder = true;
			}
			if (m_bReversePrintOrder)
			{
				int canUseMachineNo = Convert.ToInt32(textBoxMachineNoValue2Show.Text.Trim()) - iMinimumMachineNo + 1;
				int boxCount = Convert.ToInt32(strEndBoxCountNo) - Convert.ToInt32(strBeginBoxCountNo);
				if (canUseMachineNo < boxCount)
				{
					MessageBox.Show("噴印箱數超過可用機番!");
					return false;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			return false;
		}
	}

	public void SendDataThread()
	{
		try
		{
			m_bSendingData = false;
			while (m_bSendDataThreadRun)
			{
				Thread.Sleep(500);
				if (m_iCurPrintMem >= m_iPrintMaxMem)
				{
					ClsLog.LogWrite("A-by gw  -> m_iCurPrintMem=" + m_iCurPrintMem + " ,  m_iPrintMaxMem=" + m_iPrintMaxMem);
					continue;
				}
				ClsLog.LogWrite("B-by gw  -> m_iCurPrintMem=" + m_iCurPrintMem + " ,  m_iPrintMaxMem=" + m_iPrintMaxMem);
				if (m_bPause)
				{
					SendDataThreadOperateUI(m_bSendDataThreadRun);
					continue;
				}
				if (m_iCurPrintBoxNo <= m_iBoxNoEnd && m_iCurPrintBoxNo != m_iLastPrintBoxNo)
				{
					while (!m_bSendDataThreadRun)
					{
						ClsLog.LogWrite("Wait can write status begin");
						m_adtMutex.WaitOne(m_adtMutexTimeOut_ms);
						if (ClsAdt.imGetADTStatus(canReadOrWrite: false) == 1)
						{
							m_adtMutex.ReleaseMutex();
							ClsLog.LogWrite("Wait can write status ok");
							break;
						}
						m_adtMutex.ReleaseMutex();
						ClsLog.LogWrite("Wait can write status end");
						Thread.Sleep(500);
					}
					if (m_bSendDataThreadRun)
					{
						m_iLastPrintBoxNo = m_iCurPrintBoxNo;
						SendDataSingle_Invoke(bSendDataSingle: true);
						while (m_bSendingData)
						{
							Thread.Sleep(1000);
							if (!m_bSendDataThreadRun)
							{
								break;
							}
						}
						if (!m_bSendDataResult)
						{
							ClsLog.LogWrite("發送ADT DATA失败");
						}
						else
						{
							m_bGetPageSnThreadRun = true;
							SendDataThreadOperateUI(m_bSendDataThreadRun);
							WriteProductRecordDetail2Sql(m_iSendPageSn);
							ClsLog.LogWrite("发送线程写数据庫 m_iSendPageSn=m_iSendPageSn.ToString(),m_iCurPrintBoxNo=" + m_iCurPrintBoxNo + ",m_iCurPrintMem=" + m_iCurPrintMem);
							Interlocked.Increment(ref m_iSendPageSn);
							Interlocked.Increment(ref m_iCurPrintBoxNo);
							Interlocked.Increment(ref m_iCurPrintMem);
							ClsLog.LogWrite("发送线程递增页码m_iSendPageSn=" + m_iSendPageSn + ",递增当前喷印箱号m_iCurPrintBoxNo=" + m_iCurPrintBoxNo + ",递增当前喷印内存m_iCurPrintMem=" + m_iCurPrintMem);
						}
					}
				}
				m_iPrintStopStatus = 1;
				m_strHasWritePrintDataEndBatchNo = "";
				if (m_bSendDataThreadRun)
				{
					continue;
				}
				m_bGetPageSnThreadRun = false;
				m_adtMutex.WaitOne(m_adtMutexTimeOut_ms);
				ClsAdt.imSendCancel();
				ClsAdt.imClearReturnQueue();
				ClsAdt.imPrinterStop();
				m_adtMutex.ReleaseMutex();
				break;
			}
			ClsLog.LogWrite("发送线程停止");
			if (base.Visible)
			{
				WritePrintDataEndToSQl(m_iPrintStopStatus);
			}
			SendDataThreadOperateUI(m_bSendDataThreadRun);
		}
		catch (Exception ex)
		{
			MessageBox.Show("SendDataThread exception:" + ex.Message);
		}
	}

	private void SendDataThreadOperateUI(bool bCtrlUIEnabled)
	{
		BeginInvoke((EventHandler)delegate
		{
			if (bCtrlUIEnabled)
			{
				if (m_bPause)
				{
					btnPrint.Enabled = true;
					btnStop.Enabled = true;
					btnPause.Enabled = false;
				}
				else
				{
					btnPrint.Enabled = false;
					btnStop.Enabled = true;
					btnPause.Enabled = true;
				}
				textBoxKeyCodeValue.ReadOnly = true;
				textBoxMadeNoValue.ReadOnly = true;
				textBoxBoxNoBeginValue.ReadOnly = true;
				textBoxBoxNoEndValue.ReadOnly = true;
				textBoxMachineNoValue1Show.ReadOnly = true;
				textBoxMachineNoValue2Show.ReadOnly = true;
				comboBoxPrintOrder.Enabled = false;
				btnReadData.Enabled = false;
				lblReadyPrintTip.Visible = false;
				pnlClientTopDataInput.Refresh();
				pnlBottomClientCtrl.Refresh();
			}
			else if (!lblStopPrintTip.Visible)
			{
				btnPrint.Enabled = true;
				btnStop.Enabled = false;
				btnPause.Enabled = false;
				textBoxKeyCodeValue.ReadOnly = false;
				textBoxMadeNoValue.ReadOnly = false;
				textBoxBoxNoBeginValue.ReadOnly = false;
				textBoxBoxNoEndValue.ReadOnly = false;
				textBoxMachineNoValue1Show.ReadOnly = false;
				textBoxMachineNoValue2Show.ReadOnly = false;
				comboBoxPrintOrder.Enabled = true;
				btnReadData.Enabled = true;
				lblReadyPrintTip.Visible = false;
				labelWaitData.Visible = false;
				timerWaitData.Enabled = false;
				pnlClientTopDataInput.Refresh();
				pnlBottomClientCtrl.Refresh();
			}
		});
	}

	private void SendDataSingle_Invoke(bool bSendDataSingle)
	{
		try
		{
			if (m_bSendingData)
			{
				return;
			}
			m_bSendingData = true;
			ClsLog.LogWrite("aaa1 发送线程发送ADT数据imPageBegin时的m_iSendPageSn=" + m_iSendPageSn);
			m_adtMutex.WaitOne(m_adtMutexTimeOut_ms);
			if (ClsAdt.imPageBegin(m_iSendPageSn) != 1)
			{
				ClsLog.LogWrite("aaa2 imPageBegin失敗");
				ClsAdt.imSendCancel();
				m_bSendDataResult = false;
				m_adtMutex.ReleaseMutex();
				return;
			}
			ClsLog.LogWrite("bbb1 发送线程发送ADT数据AppendPageField2 begin");
			if (!AppendPageField2())
			{
				ClsLog.LogWrite("bbb2 AppendPageField2失敗");
				ClsAdt.imSendCancel();
				m_bSendDataResult = false;
				m_adtMutex.ReleaseMutex();
				return;
			}
			ClsLog.LogWrite("ccc1 imPageEnd begin");
			if (ClsAdt.imPageEnd() != 1)
			{
				ClsLog.LogWrite("ccc2 imPageEnd失敗");
				ClsAdt.imSendCancel();
				m_bSendDataResult = false;
				m_adtMutex.ReleaseMutex();
			}
			else
			{
				ClsLog.LogWrite("发送线程发送ADT数据imPageEnd时的m_iSendPageSn=" + m_iSendPageSn);
				m_bSendingData = false;
				m_adtMutex.ReleaseMutex();
				m_bSendDataResult = true;
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SendDataSingle exception:" + ex.Message);
			m_bSendDataResult = false;
			m_bSendingData = false;
			m_adtMutex.ReleaseMutex();
		}
	}

	private bool SendDataSingle_2()
	{
		try
		{
			if (ClsAdt.imPageBegin(m_iSendPageSn) != 1)
			{
				ClsLog.LogWrite("imPageBegin失敗");
				return false;
			}
			AppendPageField2();
			if (ClsAdt.imPageEnd() != 1)
			{
				ClsLog.LogWrite("imPageEnd失敗");
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SendDataSingle exception:" + ex.Message);
			return false;
		}
	}

	private void IniPrintData()
	{
		m_strTextBoxModel = textBoxModel.Text;
		m_strTextBoxModelValue = textBoxModelValue.Text;
		m_strTextBoxBore = textBoxBore.Text;
		m_strTextBoxBoreValue = textBoxBoreValue.Text;
		m_strTextBoxBoreUnit = textBoxBoreUnit.Text;
		m_strTextBoxOutput = textBoxOutput.Text;
		m_strTextBoxOutputValue = textBoxOutputValue.Text;
		m_strTextBoxOutputUnit = textBoxOutputUnit.Text;
		m_strTextBoxPhaseValue = textBoxPhaseValue.Text;
		m_strTextBoxPhaseUnit = textBoxPhaseUnit.Text;
		m_strTextBoxVoltageValue = textBoxVoltageValue.Text;
		m_strTextBoxVoltageUnit = textBoxVoltageUnit.Text;
		m_strTextBoxHzValue = textBoxHzValue.Text;
		m_strTextBoxHzUnit = textBoxHzUnit.Text;
		m_strTextBoxMadeNoValue = textBoxMadeNoValue.Text;
		m_strTextBoxTokki = textBoxTokki.Text;
		m_strTextBoxCode = textBoxCode.Text;
		m_strTextBoxCodeValue = textBoxCodeValue.Text;
		m_strLblMacineNo = lblMacineNo.Text;
		m_strTextBoxMachineNoValue1 = textBoxMachineNoValue1Show.Text;
		m_strTextBoxMachineNoValue2 = textBoxMachineNoValue2Show.Text;
		m_strTextBoxPlace = textBoxPlace.Text;
	}

	private void IniPrintParam()
	{
		try
		{
			m_strMachineNoBegin = textBoxMachineNoValueResult.Text;
			m_iSendPageSn = 1;
			string strBoxNoBeginValue = textBoxBoxNoBeginValue.Text;
			if (strBoxNoBeginValue.Length <= 0)
			{
				strBoxNoBeginValue = "0";
			}
			m_iBoxNoBegin = Convert.ToInt32(strBoxNoBeginValue);
			m_iCurPrintBoxNo = Convert.ToInt32(strBoxNoBeginValue) + 1;
			string strBoxNoEndValue = textBoxBoxNoEndValue.Text;
			if (strBoxNoEndValue.Length <= 0)
			{
				strBoxNoEndValue = "1";
			}
			m_iBoxNoEnd = Convert.ToInt32(strBoxNoEndValue);
			m_iCurPrintMem = 0;
			m_iPrintMaxMem = ClsXml.GetPrintMaxMem();
			m_iLastPrintBoxNo = -1;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("IniPrintParam exception :" + ex.Message);
		}
	}

	private void btnStop_Click(object sender, EventArgs e)
	{
		timerWaitData.Enabled = false;
		labelWaitData.Visible = false;
		lblStopPrintTip.Visible = true;
		Refresh();
		m_bPause = false;
		m_bSendDataThreadRun = false;
	}

	private void WritePrintDataEndToSQl(int iPrintStopStatus)
	{
		try
		{
			if (iPrintStopStatus != -1 || !(m_strHasWritePrintDataEndBatchNo.Trim() == m_strBatchNo.Trim()))
			{
				Thread.Sleep(1000);
				string strProRecMasterTabName = ClsXml.GetProductRecordMasterTableName();
				string strPrefix = strProRecMasterTabName.Substring(0, 2).Trim().ToUpper();
				string strSql = "UPDATE " + strProRecMasterTabName + " SET  [" + strPrefix + ClsProductRecordMasterField.strEndPrintTime + "] = CONVERT(VARCHAR(100), GETDATE(), 121),  [" + strPrefix + ClsProductRecordMasterField.strEndBoxNo + "] = " + m_strBoxNoEndValue + ",  [" + strPrefix + ClsProductRecordMasterField.strEndMachineNo + "] = '" + m_strEndMachineNo + "',  [" + strPrefix + ClsProductRecordMasterField.strPrintStopStatus + "] = '" + iPrintStopStatus + "'   WHERE  [" + strPrefix + ClsProductRecordMasterField.strBatchNo + "] = '" + m_strBatchNo + "'";
				if (ClsDataBase.SqlOperateData(strSql) < 0)
				{
					ClsLog.LogWrite("WritePrintDataEndToSQl fail");
				}
				else
				{
					m_strHasWritePrintDataEndBatchNo = m_strBatchNo;
				}
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("WritePrintDataEndToSQl exception:" + ex.Message);
		}
	}

	private void pnlAllClient_Paint(object sender, PaintEventArgs e)
	{
	}

	private void btnPause_Click(object sender, EventArgs e)
	{
		m_bPause = true;
	}

	private void textBoxKeyCodeValue_ReadOnlyChanged(object sender, EventArgs e)
	{
		if (((TextBox)sender).ReadOnly)
		{
			((TextBox)sender).BackColor = Color.WhiteSmoke;
		}
		else
		{
			((TextBox)sender).BackColor = Color.White;
		}
	}

	private void btnPrint_EnabledChanged(object sender, EventArgs e)
	{
		if (!m_bPause)
		{
			btnPreview.Enabled = btnPrint.Enabled;
		}
	}

	private void pnlClientData_Paint(object sender, PaintEventArgs e)
	{
	}

	private void btnStop_EnabledChanged(object sender, EventArgs e)
	{
		if (!m_bSendDataThreadRun)
		{
			btnPreview.Enabled = btnPrint.Enabled;
		}
	}

	private void lblReadyPrintTip_VisibleChanged(object sender, EventArgs e)
	{
		btnPrint.Enabled = !lblReadyPrintTip.Visible;
	}

	private void textBoxMadeNoValue_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!ClsShare.OnlyInputAscii(e.KeyChar))
		{
			e.Handled = true;
		}
	}

	private void btnPreview_Click(object sender, EventArgs e)
	{
		try
		{
			if (!CheckPrintData())
			{
				return;
			}
			double dMsgWidthOutside = 0.0;
			double dMsgWidthInside = 0.0;
			lblReadyPreview.Visible = true;
			Refresh();
			string strCheckFieldExceedMaxLenMsg = "";
			bool bCheckFieldExceedMaxLenRlt = CheckFieldExceedMaxLen(ref strCheckFieldExceedMaxLenMsg);
			if (GetAdtMsgFile(ref m_strAdtMsgFile, ref dMsgWidthOutside, ref dMsgWidthInside) == EnumGetAdtMsgResult.AdtMsgOK)
			{
				string strPreviewMsgFilePath = Application.StartupPath + "\\Msg\\Preview.ms";
				ClsFile.DelExistFile(strPreviewMsgFilePath);
				File.Copy(m_strAdtMsgFile, strPreviewMsgFilePath);
				SetPreviewMsgObjContent(strPreviewMsgFilePath);
				if (!checkBoxIsPreviewRealResult.Checked)
				{
					frmPreview frmP = new frmPreview();
					frmP.m_strMsgFilePath = strPreviewMsgFilePath;
					frmP.m_strCaseNo = textBoxCaseNo.Text.Trim();
					frmP.m_bCheckFieldExceedMaxLenRlt = bCheckFieldExceedMaxLenRlt;
					frmP.m_strCheckFieldExceedMaxLenMsg = strCheckFieldExceedMaxLenMsg;
					frmP.Icon = base.Icon;
					frmP.labelReadyPreviewTip = lblReadyPreview;
					frmP.ShowDialog();
				}
				else
				{
					frmPreviewRealResult frmP2 = new frmPreviewRealResult();
					frmP2.PrintHeadCount = 4;
					frmP2.dMsgWidthOutside_Pixel = dMsgWidthOutside;
					frmP2.dMsgWidthInside_Pixel = dMsgWidthInside;
					frmP2.m_strMsgFilePath = strPreviewMsgFilePath;
					frmP2.m_strCaseNo = textBoxCaseNo.Text.Trim();
					frmP2.m_bCheckFieldExceedMaxLenRlt = bCheckFieldExceedMaxLenRlt;
					frmP2.m_strCheckFieldExceedMaxLenMsg = strCheckFieldExceedMaxLenMsg;
					frmP2.Icon = base.Icon;
					frmP2.labelReadyPreviewTip = lblReadyPreview;
					frmP2.ShowDialog();
				}
			}
			lblReadyPreview.Visible = false;
			Refresh();
		}
		catch (Exception ex)
		{
			lblReadyPreview.Visible = false;
			Refresh();
			MessageBox.Show(ex.Message);
		}
	}

	private void SetPreviewMsgObjContent(string strPreviewMsgFilePath)
	{
		if (!File.Exists(strPreviewMsgFilePath))
		{
			MessageBox.Show("預覽模板不存在，請確認配置是否正確!");
			return;
		}
		string strSection = ClsMsg.clsMsgSection.strObj;
		string strKey = "SimpleData";
		_ = ClsMsg.clsMsgObjToSrcParam.strMsgObjT + "0";
		string strModel = textBoxModel.Text.Trim();
		string strModelValue = textBoxModel.Text.Trim();
		if (strModel.Length > 0 && strModelValue.Length > 0)
		{
			strModel += ":";
		}
		int iObjIndexBase = 1;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strModel, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strModel, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strModel);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strModel);
		string strBore = textBoxBore.Text.Trim();
		string strBoreValue = textBoxBoreValue.Text.Trim();
		string strBoreUnit = textBoxBoreUnit.Text.Trim();
		if (strBore.Length > 0 && strBoreValue.Length > 0 && strBoreUnit.Length > 0)
		{
			strBore += ":";
		}
		iObjIndexBase = 2;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strBore, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strBore, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strBore);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strBore);
		iObjIndexBase = 3;
		string strBoreValueUnit = strBoreValue + strBoreUnit;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strBoreValueUnit, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strBoreValueUnit, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strBoreValueUnit);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strBoreValueUnit);
		iObjIndexBase = 4;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strBoreUnit, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strBoreUnit, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strBoreUnit);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strBoreUnit);
		string strOutput = textBoxOutput.Text.Trim();
		string strOutputValue = textBoxOutputValue.Text.Trim();
		string strOutputUnit = textBoxOutputUnit.Text.Trim();
		if (strOutput.Length > 0 && strOutputValue.Length > 0 && strOutputUnit.Length > 0)
		{
			strOutput += ":";
		}
		iObjIndexBase = 5;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strOutput, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strOutput, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strOutput);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strOutput);
		iObjIndexBase = 6;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strOutputValue + strOutputUnit, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strOutputValue + strOutputUnit, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strOutputValue + strOutputUnit);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strOutputValue + strOutputUnit);
		iObjIndexBase = 7;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strOutputUnit, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strOutputUnit, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strOutputUnit);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strOutputUnit);
		string strPhaseValue = textBoxPhaseValue.Text.Trim();
		string strPhaseUnit = textBoxPhaseUnit.Text.Trim();
		iObjIndexBase = 8;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strPhaseValue, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strPhaseValue, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strPhaseValue);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strPhaseValue);
		iObjIndexBase = 9;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strPhaseUnit, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strPhaseUnit, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strPhaseUnit);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strPhaseUnit);
		string strVoltageValue = textBoxVoltageValue.Text.Trim();
		string strVoltageUnit = textBoxVoltageUnit.Text.Trim();
		string strHzValue = textBoxHzValue.Text.Trim();
		string strHzUnit = textBoxHzUnit.Text.Trim();
		iObjIndexBase = 10;
		IniSpecialSendDataParam();
		string strVoltageHz = strVoltageValue + strVoltageUnit + m_strSpaceBetweenVoltageUnitAndHzValue + strHzValue + strHzUnit;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strVoltageHz, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strVoltageHz, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strVoltageHz);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strVoltageHz);
		iObjIndexBase = 11;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strVoltageUnit, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strVoltageUnit, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strVoltageUnit);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strVoltageUnit);
		iObjIndexBase = 12;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strHzValue + strHzUnit, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strHzValue + strHzUnit, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strHzValue + strHzUnit);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strHzValue + strHzUnit);
		iObjIndexBase = 13;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strHzUnit, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strHzUnit, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strHzUnit);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strHzUnit);
		string strMadeNo = textBoxMadeNoValueResult.Text.Trim();
		iObjIndexBase = 14;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strMadeNo, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strMadeNo, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strMadeNo);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strMadeNo);
		iObjIndexBase = 15;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strMadeNo, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strMadeNo, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strMadeNo);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strMadeNo);
		string strTokki = textBoxTokki.Text.Trim();
		iObjIndexBase = 16;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strTokki, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strTokki, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strTokki);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strTokki);
		string strCodeValue3SpaceMachineNo = textBoxCodeValue.Text.Trim() + "   " + textBoxMachineNoValueResult.Text.Trim();
		iObjIndexBase = 17;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strCodeValue3SpaceMachineNo, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strCodeValue3SpaceMachineNo, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strCodeValue3SpaceMachineNo);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strCodeValue3SpaceMachineNo);
		string strCodeValue = textBoxCodeValue.Text.Trim();
		string strCode = textBoxCode.Text.Trim();
		if (strCode.Length > 0)
		{
			strCode += ":";
		}
		iObjIndexBase = 18;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strCodeValue, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strCodeValue, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strCodeValue);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strCodeValue);
		iObjIndexBase = 19;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strCode, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strCode, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strCode);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strCode);
		string strMachineNo = lblMacineNo.Text.Trim();
		string strMachineNoValue = textBoxMachineNoValueResult.Text.Trim();
		iObjIndexBase = 20;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strMachineNo, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strMachineNo, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strMachineNo);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strMachineNo);
		iObjIndexBase = 21;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKey, strMachineNoValue, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKey, strMachineNoValue, strPreviewMsgFilePath);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase, strMachineNoValue);
		SetMsgObjT(strPreviewMsgFilePath, iObjIndexBase + 24, strMachineNoValue);
		iObjIndexBase = 22;
		string strKeyLogo = ClsMsg.clsMsgKey_Logo.strData;
		byte[] byteArrFrontModelValue = new byte[m_msModelValueFrontBmp.Length];
		m_msModelValueFrontBmp.Position = 0L;
		m_msModelValueFrontBmp.Read(byteArrFrontModelValue, 0, (int)m_msModelValueFrontBmp.Length);
		ClsGetHexStrFromBytesOnMultiThread get16SBB_Front = new ClsGetHexStrFromBytesOnMultiThread(byteArrFrontModelValue);
		string strFontModelValueBmp = get16SBB_Front.m_strRlt;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKeyLogo, strFontModelValueBmp, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKeyLogo, strFontModelValueBmp, strPreviewMsgFilePath);
		iObjIndexBase = 23;
		byte[] byteArrPlace = new byte[m_msPlaceBmp.Length];
		m_msPlaceBmp.Position = 0L;
		m_msPlaceBmp.Read(byteArrPlace, 0, (int)m_msPlaceBmp.Length);
		ClsGetHexStrFromBytesOnMultiThread get16SBB_Place = new ClsGetHexStrFromBytesOnMultiThread(byteArrPlace);
		string strPlaceBmp = get16SBB_Place.m_strRlt;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKeyLogo, strPlaceBmp, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKeyLogo, strPlaceBmp, strPreviewMsgFilePath);
		iObjIndexBase = 24;
		byte[] byteArrSideModelValue = new byte[m_msModelValueSideBmp.Length];
		m_msModelValueSideBmp.Position = 0L;
		m_msModelValueSideBmp.Read(byteArrSideModelValue, 0, (int)m_msModelValueSideBmp.Length);
		ClsGetHexStrFromBytesOnMultiThread get16SBB_Side = new ClsGetHexStrFromBytesOnMultiThread(byteArrSideModelValue);
		string strSideModelValueBmp = get16SBB_Side.m_strRlt;
		ClsIni.WritePrivateProfileString(strSection + iObjIndexBase, strKeyLogo, strSideModelValueBmp, strPreviewMsgFilePath);
		ClsIni.WritePrivateProfileString(strSection + (iObjIndexBase + 24), strKeyLogo, strSideModelValueBmp, strPreviewMsgFilePath);
	}

	private void SetMsgObjT(string strPreviewMsgFilePath, int iObjIndex, string strMsgObjT_Content)
	{
		string strSection = ClsMsg.clsMsgSection.strObj + iObjIndex;
		string strKeyMsgObj = ClsMsg.clsMsgObjToSrcParam.strMsgObjT + "0";
		string strMsgObjValueTmp = ClsIni.GetValue(strPreviewMsgFilePath, strSection, strKeyMsgObj);
		string strMsgObjValueNew = strMsgObjValueTmp.Split(',')[0] + "," + strMsgObjValueTmp.Split(',')[1] + "," + strMsgObjT_Content;
		ClsIni.WritePrivateProfileString(strSection, strKeyMsgObj, strMsgObjValueNew, strPreviewMsgFilePath);
	}

	private void groupBoxCaseNo_Paint(object sender, PaintEventArgs e)
	{
		ClsShare.RepainGroupBoxSide(sender, e, this);
	}

	private void btnRulerTest_Click(object sender, EventArgs e)
	{
	}

	private void timerWaitData_Tick(object sender, EventArgs e)
	{
		if (m_waitDataTime_s <= 0)
		{
			labelWaitData.Visible = false;
			timerWaitData.Enabled = false;
		}
		else if (!lblReadyPrintTip.Visible)
		{
			if (!labelWaitData.Visible)
			{
				labelWaitData.Visible = true;
			}
			labelWaitData.Text = "數據緩衝中，請在" + m_waitDataTime_s + "秒後放入紙箱...";
			m_waitDataTime_s--;
		}
	}

	private void checkBoxMutexControl_CheckedChanged(object sender, EventArgs e)
	{
		m_releaseMutex = (sender as CheckBox).Checked;
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
		this.pnlClientTopDataInput = new System.Windows.Forms.Panel();
		this.comboBoxPrintOrder = new System.Windows.Forms.ComboBox();
		this.textBoxMachineNoValue2Show = new System.Windows.Forms.TextBox();
		this.textBoxMachineNoValue1Show = new System.Windows.Forms.TextBox();
		this.label9 = new System.Windows.Forms.Label();
		this.btnReadData = new System.Windows.Forms.Button();
		this.label4 = new System.Windows.Forms.Label();
		this.textBoxBoxNoEndValue = new System.Windows.Forms.TextBox();
		this.textBoxBoxNoBeginValue = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.textBoxMadeNoValue = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.textBoxKeyCodeValue = new System.Windows.Forms.TextBox();
		this.lblKeyCode = new System.Windows.Forms.Label();
		this.pnlClientFrontSideCartonSize = new System.Windows.Forms.Panel();
		this.groupBoxCaseNo = new System.Windows.Forms.GroupBox();
		this.label6 = new System.Windows.Forms.Label();
		this.textBoxInsidePrintWordEnd = new System.Windows.Forms.TextBox();
		this.textBoxInsidePrintWordBegin = new System.Windows.Forms.TextBox();
		this.textBoxOutSidePrintWordEnd = new System.Windows.Forms.TextBox();
		this.textBoxOutSidePrintWordBegin = new System.Windows.Forms.TextBox();
		this.textBoxPrintWordHeight = new System.Windows.Forms.TextBox();
		this.textBoxCaseNo = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.gbDataSide = new System.Windows.Forms.GroupBox();
		this.textBoxPlace = new System.Windows.Forms.TextBox();
		this.textBoxModelValueSide = new System.Windows.Forms.TextBox();
		this.gbDataFront = new System.Windows.Forms.GroupBox();
		this.lblReadyPreview = new System.Windows.Forms.Label();
		this.lblStopPrintTip = new System.Windows.Forms.Label();
		this.lblReadyPrintTip = new System.Windows.Forms.Label();
		this.labelWaitData = new System.Windows.Forms.Label();
		this.textBoxHzUnit = new System.Windows.Forms.TextBox();
		this.textBoxHzValue = new System.Windows.Forms.TextBox();
		this.textBoxVoltageUnit = new System.Windows.Forms.TextBox();
		this.textBoxVoltageValue = new System.Windows.Forms.TextBox();
		this.textBoxPhaseUnit = new System.Windows.Forms.TextBox();
		this.textBoxPhaseValue = new System.Windows.Forms.TextBox();
		this.textBoxOutputUnit = new System.Windows.Forms.TextBox();
		this.textBoxOutputValue = new System.Windows.Forms.TextBox();
		this.label7 = new System.Windows.Forms.Label();
		this.textBoxOutput = new System.Windows.Forms.TextBox();
		this.textBoxBoreUnit = new System.Windows.Forms.TextBox();
		this.textBoxBoreValue = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.textBoxBore = new System.Windows.Forms.TextBox();
		this.textBoxMachineNoValueResult = new System.Windows.Forms.TextBox();
		this.lblMacineNo = new System.Windows.Forms.Label();
		this.textBoxCodeValue = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
		this.textBoxCode = new System.Windows.Forms.TextBox();
		this.textBoxTokki = new System.Windows.Forms.TextBox();
		this.textBoxMadeNoValueResult = new System.Windows.Forms.TextBox();
		this.textBoxModelValue = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.textBoxModel = new System.Windows.Forms.TextBox();
		this.pnlBottomClientCtrl = new System.Windows.Forms.Panel();
		this.checkBoxIsPreviewRealResult = new System.Windows.Forms.CheckBox();
		this.btnPreview = new System.Windows.Forms.Button();
		this.btnPause = new System.Windows.Forms.Button();
		this.btnStop = new System.Windows.Forms.Button();
		this.btnPrint = new System.Windows.Forms.Button();
		this.pnlBottomBottomStatus = new System.Windows.Forms.Panel();
		this.statusStripPrintCtrl = new System.Windows.Forms.StatusStrip();
		this.toolStripStatusLabelPrintStatusIcon = new System.Windows.Forms.ToolStripStatusLabel();
		this.toolStripStatusLabelPrintStatusText = new System.Windows.Forms.ToolStripStatusLabel();
		this.timerWaitData = new System.Windows.Forms.Timer(this.components);
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		this.pnlClientTopDataInput.SuspendLayout();
		this.pnlClientFrontSideCartonSize.SuspendLayout();
		this.groupBoxCaseNo.SuspendLayout();
		this.gbDataSide.SuspendLayout();
		this.gbDataFront.SuspendLayout();
		this.pnlBottomClientCtrl.SuspendLayout();
		this.pnlBottomBottomStatus.SuspendLayout();
		this.statusStripPrintCtrl.SuspendLayout();
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.pnlClientFrontSideCartonSize);
		base.pnlClientData.Controls.Add(this.pnlClientTopDataInput);
		base.pnlClientData.Margin = new System.Windows.Forms.Padding(4);
		base.pnlClientData.Size = new System.Drawing.Size(1012, 553);
		base.pnlClientData.Paint += new System.Windows.Forms.PaintEventHandler(pnlClientData_Paint);
		base.pnlBottomCtrl.Controls.Add(this.pnlBottomBottomStatus);
		base.pnlBottomCtrl.Controls.Add(this.pnlBottomClientCtrl);
		base.pnlBottomCtrl.Location = new System.Drawing.Point(0, 553);
		base.pnlBottomCtrl.Margin = new System.Windows.Forms.Padding(4);
		base.pnlBottomCtrl.Size = new System.Drawing.Size(1012, 145);
		this.pnlClientTopDataInput.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientTopDataInput.Controls.Add(this.comboBoxPrintOrder);
		this.pnlClientTopDataInput.Controls.Add(this.textBoxMachineNoValue2Show);
		this.pnlClientTopDataInput.Controls.Add(this.textBoxMachineNoValue1Show);
		this.pnlClientTopDataInput.Controls.Add(this.label9);
		this.pnlClientTopDataInput.Controls.Add(this.btnReadData);
		this.pnlClientTopDataInput.Controls.Add(this.label4);
		this.pnlClientTopDataInput.Controls.Add(this.textBoxBoxNoEndValue);
		this.pnlClientTopDataInput.Controls.Add(this.textBoxBoxNoBeginValue);
		this.pnlClientTopDataInput.Controls.Add(this.label3);
		this.pnlClientTopDataInput.Controls.Add(this.textBoxMadeNoValue);
		this.pnlClientTopDataInput.Controls.Add(this.label2);
		this.pnlClientTopDataInput.Controls.Add(this.textBoxKeyCodeValue);
		this.pnlClientTopDataInput.Controls.Add(this.lblKeyCode);
		this.pnlClientTopDataInput.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlClientTopDataInput.Location = new System.Drawing.Point(0, 0);
		this.pnlClientTopDataInput.Name = "pnlClientTopDataInput";
		this.pnlClientTopDataInput.Size = new System.Drawing.Size(1008, 117);
		this.pnlClientTopDataInput.TabIndex = 2;
		this.comboBoxPrintOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxPrintOrder.FormattingEnabled = true;
		this.comboBoxPrintOrder.Items.AddRange(new object[2] { "按順序噴印", "按逆序噴印" });
		this.comboBoxPrintOrder.Location = new System.Drawing.Point(840, 68);
		this.comboBoxPrintOrder.Name = "comboBoxPrintOrder";
		this.comboBoxPrintOrder.Size = new System.Drawing.Size(105, 20);
		this.comboBoxPrintOrder.TabIndex = 15;
		this.textBoxMachineNoValue2Show.BackColor = System.Drawing.Color.White;
		this.textBoxMachineNoValue2Show.Location = new System.Drawing.Point(720, 68);
		this.textBoxMachineNoValue2Show.MaxLength = 16;
		this.textBoxMachineNoValue2Show.Name = "textBoxMachineNoValue2Show";
		this.textBoxMachineNoValue2Show.Size = new System.Drawing.Size(114, 21);
		this.textBoxMachineNoValue2Show.TabIndex = 8;
		this.textBoxMachineNoValue2Show.ReadOnlyChanged += new System.EventHandler(textBoxKeyCodeValue_ReadOnlyChanged);
		this.textBoxMachineNoValue2Show.TextChanged += new System.EventHandler(textBoxMachineNoValue1Show_TextChanged);
		this.textBoxMachineNoValue2Show.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxMachineNoValue2Show_KeyPress);
		this.textBoxMachineNoValue2Show.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxMachineNoValue2Show_KeyUp);
		this.textBoxMachineNoValue1Show.BackColor = System.Drawing.Color.White;
		this.textBoxMachineNoValue1Show.Location = new System.Drawing.Point(653, 68);
		this.textBoxMachineNoValue1Show.MaxLength = 8;
		this.textBoxMachineNoValue1Show.Name = "textBoxMachineNoValue1Show";
		this.textBoxMachineNoValue1Show.Size = new System.Drawing.Size(61, 21);
		this.textBoxMachineNoValue1Show.TabIndex = 7;
		this.textBoxMachineNoValue1Show.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxMachineNoValue1Show.ReadOnlyChanged += new System.EventHandler(textBoxKeyCodeValue_ReadOnlyChanged);
		this.textBoxMachineNoValue1Show.TextChanged += new System.EventHandler(textBoxMachineNoValue1Show_TextChanged);
		this.textBoxMachineNoValue1Show.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxMadeNoValue_KeyPress);
		this.label9.AutoSize = true;
		this.label9.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label9.Location = new System.Drawing.Point(602, 71);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(45, 14);
		this.label9.TabIndex = 12;
		this.label9.Text = "機番:";
		this.btnReadData.Location = new System.Drawing.Point(779, 18);
		this.btnReadData.Name = "btnReadData";
		this.btnReadData.Size = new System.Drawing.Size(166, 34);
		this.btnReadData.TabIndex = 11;
		this.btnReadData.Text = "讀取資料";
		this.btnReadData.UseVisualStyleBackColor = true;
		this.btnReadData.Click += new System.EventHandler(btnReadData_Click);
		this.label4.AutoSize = true;
		this.label4.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label4.Location = new System.Drawing.Point(430, 71);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(15, 14);
		this.label4.TabIndex = 7;
		this.label4.Text = "/";
		this.textBoxBoxNoEndValue.BackColor = System.Drawing.Color.White;
		this.textBoxBoxNoEndValue.Location = new System.Drawing.Point(451, 68);
		this.textBoxBoxNoEndValue.MaxLength = 6;
		this.textBoxBoxNoEndValue.Name = "textBoxBoxNoEndValue";
		this.textBoxBoxNoEndValue.Size = new System.Drawing.Size(123, 21);
		this.textBoxBoxNoEndValue.TabIndex = 6;
		this.textBoxBoxNoEndValue.ReadOnlyChanged += new System.EventHandler(textBoxKeyCodeValue_ReadOnlyChanged);
		this.textBoxBoxNoEndValue.TextChanged += new System.EventHandler(textBoxBoxCountEndValue_TextChanged);
		this.textBoxBoxNoEndValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxBoxNoBeginValue_KeyPress);
		this.textBoxBoxNoEndValue.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxKeyCodeValue_KeyUp);
		this.textBoxBoxNoBeginValue.BackColor = System.Drawing.Color.White;
		this.textBoxBoxNoBeginValue.Location = new System.Drawing.Point(366, 68);
		this.textBoxBoxNoBeginValue.MaxLength = 6;
		this.textBoxBoxNoBeginValue.Name = "textBoxBoxNoBeginValue";
		this.textBoxBoxNoBeginValue.Size = new System.Drawing.Size(58, 21);
		this.textBoxBoxNoBeginValue.TabIndex = 5;
		this.textBoxBoxNoBeginValue.ReadOnlyChanged += new System.EventHandler(textBoxKeyCodeValue_ReadOnlyChanged);
		this.textBoxBoxNoBeginValue.TextChanged += new System.EventHandler(textBoxBoxCountBeginValue_TextChanged);
		this.textBoxBoxNoBeginValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxBoxNoBeginValue_KeyPress);
		this.textBoxBoxNoBeginValue.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxKeyCodeValue_KeyUp);
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label3.Location = new System.Drawing.Point(315, 71);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(45, 14);
		this.label3.TabIndex = 4;
		this.label3.Text = "箱數:";
		this.textBoxMadeNoValue.BackColor = System.Drawing.Color.White;
		this.textBoxMadeNoValue.Location = new System.Drawing.Point(93, 68);
		this.textBoxMadeNoValue.MaxLength = 15;
		this.textBoxMadeNoValue.Name = "textBoxMadeNoValue";
		this.textBoxMadeNoValue.Size = new System.Drawing.Size(197, 21);
		this.textBoxMadeNoValue.TabIndex = 3;
		this.textBoxMadeNoValue.ReadOnlyChanged += new System.EventHandler(textBoxKeyCodeValue_ReadOnlyChanged);
		this.textBoxMadeNoValue.TextChanged += new System.EventHandler(textBoxMadeNoValue_TextChanged);
		this.textBoxMadeNoValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxMadeNoValue_KeyPress);
		this.textBoxMadeNoValue.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxKeyCodeValue_KeyUp);
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label2.Location = new System.Drawing.Point(8, 71);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(61, 14);
		this.label2.TabIndex = 2;
		this.label2.Text = "製番NO:";
		this.textBoxKeyCodeValue.BackColor = System.Drawing.Color.White;
		this.textBoxKeyCodeValue.Location = new System.Drawing.Point(93, 25);
		this.textBoxKeyCodeValue.MaxLength = 500;
		this.textBoxKeyCodeValue.Name = "textBoxKeyCodeValue";
		this.textBoxKeyCodeValue.Size = new System.Drawing.Size(197, 21);
		this.textBoxKeyCodeValue.TabIndex = 1;
		this.textBoxKeyCodeValue.ReadOnlyChanged += new System.EventHandler(textBoxKeyCodeValue_ReadOnlyChanged);
		this.textBoxKeyCodeValue.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxKeyCodeValue_KeyUp);
		this.lblKeyCode.AutoSize = true;
		this.lblKeyCode.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblKeyCode.Location = new System.Drawing.Point(8, 28);
		this.lblKeyCode.Name = "lblKeyCode";
		this.lblKeyCode.Size = new System.Drawing.Size(79, 14);
		this.lblKeyCode.TabIndex = 0;
		this.lblKeyCode.Text = "Key code:";
		this.pnlClientFrontSideCartonSize.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientFrontSideCartonSize.Controls.Add(this.groupBoxCaseNo);
		this.pnlClientFrontSideCartonSize.Controls.Add(this.gbDataSide);
		this.pnlClientFrontSideCartonSize.Controls.Add(this.gbDataFront);
		this.pnlClientFrontSideCartonSize.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientFrontSideCartonSize.Location = new System.Drawing.Point(0, 117);
		this.pnlClientFrontSideCartonSize.Name = "pnlClientFrontSideCartonSize";
		this.pnlClientFrontSideCartonSize.Size = new System.Drawing.Size(1008, 432);
		this.pnlClientFrontSideCartonSize.TabIndex = 43;
		this.pnlClientFrontSideCartonSize.Paint += new System.Windows.Forms.PaintEventHandler(pnlClientFront_Paint);
		this.groupBoxCaseNo.Controls.Add(this.label6);
		this.groupBoxCaseNo.Controls.Add(this.textBoxInsidePrintWordEnd);
		this.groupBoxCaseNo.Controls.Add(this.textBoxInsidePrintWordBegin);
		this.groupBoxCaseNo.Controls.Add(this.textBoxOutSidePrintWordEnd);
		this.groupBoxCaseNo.Controls.Add(this.textBoxOutSidePrintWordBegin);
		this.groupBoxCaseNo.Controls.Add(this.textBoxPrintWordHeight);
		this.groupBoxCaseNo.Controls.Add(this.textBoxCaseNo);
		this.groupBoxCaseNo.Controls.Add(this.label10);
		this.groupBoxCaseNo.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.groupBoxCaseNo.Location = new System.Drawing.Point(11, 359);
		this.groupBoxCaseNo.Name = "groupBoxCaseNo";
		this.groupBoxCaseNo.Size = new System.Drawing.Size(947, 68);
		this.groupBoxCaseNo.TabIndex = 103;
		this.groupBoxCaseNo.TabStop = false;
		this.groupBoxCaseNo.Text = "紙箱規格";
		this.groupBoxCaseNo.Paint += new System.Windows.Forms.PaintEventHandler(groupBoxCaseNo_Paint);
		this.label6.AutoSize = true;
		this.label6.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label6.Font = new System.Drawing.Font("宋体", 15f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label6.ForeColor = System.Drawing.Color.Red;
		this.label6.Location = new System.Drawing.Point(433, 29);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(104, 20);
		this.label6.TabIndex = 104;
		this.label6.Text = "印字高度:";
		this.textBoxInsidePrintWordEnd.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxInsidePrintWordEnd.Location = new System.Drawing.Point(857, 39);
		this.textBoxInsidePrintWordEnd.Name = "textBoxInsidePrintWordEnd";
		this.textBoxInsidePrintWordEnd.ReadOnly = true;
		this.textBoxInsidePrintWordEnd.Size = new System.Drawing.Size(59, 23);
		this.textBoxInsidePrintWordEnd.TabIndex = 103;
		this.textBoxInsidePrintWordEnd.Visible = false;
		this.textBoxInsidePrintWordBegin.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxInsidePrintWordBegin.Location = new System.Drawing.Point(768, 39);
		this.textBoxInsidePrintWordBegin.Name = "textBoxInsidePrintWordBegin";
		this.textBoxInsidePrintWordBegin.ReadOnly = true;
		this.textBoxInsidePrintWordBegin.Size = new System.Drawing.Size(59, 23);
		this.textBoxInsidePrintWordBegin.TabIndex = 102;
		this.textBoxInsidePrintWordBegin.Visible = false;
		this.textBoxOutSidePrintWordEnd.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxOutSidePrintWordEnd.Location = new System.Drawing.Point(679, 39);
		this.textBoxOutSidePrintWordEnd.Name = "textBoxOutSidePrintWordEnd";
		this.textBoxOutSidePrintWordEnd.ReadOnly = true;
		this.textBoxOutSidePrintWordEnd.Size = new System.Drawing.Size(59, 23);
		this.textBoxOutSidePrintWordEnd.TabIndex = 101;
		this.textBoxOutSidePrintWordEnd.Visible = false;
		this.textBoxOutSidePrintWordBegin.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxOutSidePrintWordBegin.Location = new System.Drawing.Point(590, 39);
		this.textBoxOutSidePrintWordBegin.Name = "textBoxOutSidePrintWordBegin";
		this.textBoxOutSidePrintWordBegin.ReadOnly = true;
		this.textBoxOutSidePrintWordBegin.Size = new System.Drawing.Size(59, 23);
		this.textBoxOutSidePrintWordBegin.TabIndex = 100;
		this.textBoxOutSidePrintWordBegin.Visible = false;
		this.textBoxPrintWordHeight.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxPrintWordHeight.Font = new System.Drawing.Font("宋体", 15f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxPrintWordHeight.Location = new System.Drawing.Point(543, 25);
		this.textBoxPrintWordHeight.Name = "textBoxPrintWordHeight";
		this.textBoxPrintWordHeight.ReadOnly = true;
		this.textBoxPrintWordHeight.Size = new System.Drawing.Size(385, 30);
		this.textBoxPrintWordHeight.TabIndex = 99;
		this.textBoxCaseNo.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxCaseNo.Font = new System.Drawing.Font("宋体", 15f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxCaseNo.Location = new System.Drawing.Point(115, 25);
		this.textBoxCaseNo.Name = "textBoxCaseNo";
		this.textBoxCaseNo.ReadOnly = true;
		this.textBoxCaseNo.Size = new System.Drawing.Size(271, 30);
		this.textBoxCaseNo.TabIndex = 64;
		this.label10.AutoSize = true;
		this.label10.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label10.Font = new System.Drawing.Font("宋体", 15f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label10.ForeColor = System.Drawing.Color.Red;
		this.label10.Location = new System.Drawing.Point(5, 29);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(104, 20);
		this.label10.TabIndex = 63;
		this.label10.Text = "紙箱編號:";
		this.gbDataSide.Controls.Add(this.textBoxPlace);
		this.gbDataSide.Controls.Add(this.textBoxModelValueSide);
		this.gbDataSide.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.gbDataSide.Location = new System.Drawing.Point(11, 267);
		this.gbDataSide.Name = "gbDataSide";
		this.gbDataSide.Size = new System.Drawing.Size(947, 74);
		this.gbDataSide.TabIndex = 102;
		this.gbDataSide.TabStop = false;
		this.gbDataSide.Text = "側嘜";
		this.gbDataSide.Paint += new System.Windows.Forms.PaintEventHandler(groupBoxCaseNo_Paint);
		this.textBoxPlace.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxPlace.Location = new System.Drawing.Point(435, 32);
		this.textBoxPlace.Name = "textBoxPlace";
		this.textBoxPlace.ReadOnly = true;
		this.textBoxPlace.Size = new System.Drawing.Size(493, 23);
		this.textBoxPlace.TabIndex = 60;
		this.textBoxModelValueSide.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxModelValueSide.Location = new System.Drawing.Point(8, 32);
		this.textBoxModelValueSide.Name = "textBoxModelValueSide";
		this.textBoxModelValueSide.ReadOnly = true;
		this.textBoxModelValueSide.Size = new System.Drawing.Size(378, 23);
		this.textBoxModelValueSide.TabIndex = 59;
		this.gbDataFront.Controls.Add(this.lblReadyPreview);
		this.gbDataFront.Controls.Add(this.lblStopPrintTip);
		this.gbDataFront.Controls.Add(this.lblReadyPrintTip);
		this.gbDataFront.Controls.Add(this.labelWaitData);
		this.gbDataFront.Controls.Add(this.textBoxHzUnit);
		this.gbDataFront.Controls.Add(this.textBoxHzValue);
		this.gbDataFront.Controls.Add(this.textBoxVoltageUnit);
		this.gbDataFront.Controls.Add(this.textBoxVoltageValue);
		this.gbDataFront.Controls.Add(this.textBoxPhaseUnit);
		this.gbDataFront.Controls.Add(this.textBoxPhaseValue);
		this.gbDataFront.Controls.Add(this.textBoxOutputUnit);
		this.gbDataFront.Controls.Add(this.textBoxOutputValue);
		this.gbDataFront.Controls.Add(this.label7);
		this.gbDataFront.Controls.Add(this.textBoxOutput);
		this.gbDataFront.Controls.Add(this.textBoxBoreUnit);
		this.gbDataFront.Controls.Add(this.textBoxBoreValue);
		this.gbDataFront.Controls.Add(this.label5);
		this.gbDataFront.Controls.Add(this.textBoxBore);
		this.gbDataFront.Controls.Add(this.textBoxMachineNoValueResult);
		this.gbDataFront.Controls.Add(this.lblMacineNo);
		this.gbDataFront.Controls.Add(this.textBoxCodeValue);
		this.gbDataFront.Controls.Add(this.label8);
		this.gbDataFront.Controls.Add(this.textBoxCode);
		this.gbDataFront.Controls.Add(this.textBoxTokki);
		this.gbDataFront.Controls.Add(this.textBoxMadeNoValueResult);
		this.gbDataFront.Controls.Add(this.textBoxModelValue);
		this.gbDataFront.Controls.Add(this.label1);
		this.gbDataFront.Controls.Add(this.textBoxModel);
		this.gbDataFront.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.gbDataFront.Location = new System.Drawing.Point(11, 20);
		this.gbDataFront.Name = "gbDataFront";
		this.gbDataFront.Size = new System.Drawing.Size(947, 226);
		this.gbDataFront.TabIndex = 101;
		this.gbDataFront.TabStop = false;
		this.gbDataFront.Text = "正嘜";
		this.gbDataFront.Paint += new System.Windows.Forms.PaintEventHandler(groupBoxCaseNo_Paint);
		this.lblReadyPreview.AutoSize = true;
		this.lblReadyPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.lblReadyPreview.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblReadyPreview.Location = new System.Drawing.Point(343, 177);
		this.lblReadyPreview.Name = "lblReadyPreview";
		this.lblReadyPreview.Size = new System.Drawing.Size(303, 31);
		this.lblReadyPreview.TabIndex = 102;
		this.lblReadyPreview.Text = "準備預覽，請稍候...";
		this.lblReadyPreview.Visible = false;
		this.lblStopPrintTip.AutoSize = true;
		this.lblStopPrintTip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.lblStopPrintTip.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblStopPrintTip.Location = new System.Drawing.Point(326, 178);
		this.lblStopPrintTip.Name = "lblStopPrintTip";
		this.lblStopPrintTip.Size = new System.Drawing.Size(303, 31);
		this.lblStopPrintTip.TabIndex = 101;
		this.lblStopPrintTip.Text = "正在停止，請稍候...";
		this.lblStopPrintTip.Visible = false;
		this.lblReadyPrintTip.AutoSize = true;
		this.lblReadyPrintTip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.lblReadyPrintTip.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblReadyPrintTip.Location = new System.Drawing.Point(312, 177);
		this.lblReadyPrintTip.Name = "lblReadyPrintTip";
		this.lblReadyPrintTip.Size = new System.Drawing.Size(289, 31);
		this.lblReadyPrintTip.TabIndex = 37;
		this.lblReadyPrintTip.Text = "準備噴印,請稍候...";
		this.lblReadyPrintTip.Visible = false;
		this.lblReadyPrintTip.VisibleChanged += new System.EventHandler(lblReadyPrintTip_VisibleChanged);
		this.labelWaitData.AutoSize = true;
		this.labelWaitData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelWaitData.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.labelWaitData.ForeColor = System.Drawing.Color.Red;
		this.labelWaitData.Location = new System.Drawing.Point(248, 176);
		this.labelWaitData.Name = "labelWaitData";
		this.labelWaitData.Size = new System.Drawing.Size(515, 31);
		this.labelWaitData.TabIndex = 103;
		this.labelWaitData.Text = "數據緩衝中，請在60秒後放入紙箱...";
		this.labelWaitData.Visible = false;
		this.textBoxHzUnit.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxHzUnit.Location = new System.Drawing.Point(717, 107);
		this.textBoxHzUnit.Name = "textBoxHzUnit";
		this.textBoxHzUnit.ReadOnly = true;
		this.textBoxHzUnit.Size = new System.Drawing.Size(211, 23);
		this.textBoxHzUnit.TabIndex = 100;
		this.textBoxHzValue.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxHzValue.Location = new System.Drawing.Point(435, 107);
		this.textBoxHzValue.Name = "textBoxHzValue";
		this.textBoxHzValue.ReadOnly = true;
		this.textBoxHzValue.Size = new System.Drawing.Size(260, 23);
		this.textBoxHzValue.TabIndex = 99;
		this.textBoxHzValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxVoltageUnit.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxVoltageUnit.Location = new System.Drawing.Point(327, 107);
		this.textBoxVoltageUnit.Name = "textBoxVoltageUnit";
		this.textBoxVoltageUnit.ReadOnly = true;
		this.textBoxVoltageUnit.Size = new System.Drawing.Size(59, 23);
		this.textBoxVoltageUnit.TabIndex = 98;
		this.textBoxVoltageValue.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxVoltageValue.Location = new System.Drawing.Point(225, 107);
		this.textBoxVoltageValue.Name = "textBoxVoltageValue";
		this.textBoxVoltageValue.ReadOnly = true;
		this.textBoxVoltageValue.Size = new System.Drawing.Size(96, 23);
		this.textBoxVoltageValue.TabIndex = 97;
		this.textBoxVoltageValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxPhaseUnit.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxPhaseUnit.Location = new System.Drawing.Point(83, 107);
		this.textBoxPhaseUnit.Name = "textBoxPhaseUnit";
		this.textBoxPhaseUnit.ReadOnly = true;
		this.textBoxPhaseUnit.Size = new System.Drawing.Size(103, 23);
		this.textBoxPhaseUnit.TabIndex = 96;
		this.textBoxPhaseValue.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxPhaseValue.Location = new System.Drawing.Point(8, 107);
		this.textBoxPhaseValue.Name = "textBoxPhaseValue";
		this.textBoxPhaseValue.ReadOnly = true;
		this.textBoxPhaseValue.Size = new System.Drawing.Size(53, 23);
		this.textBoxPhaseValue.TabIndex = 95;
		this.textBoxPhaseValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxOutputUnit.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxOutputUnit.Location = new System.Drawing.Point(717, 69);
		this.textBoxOutputUnit.Name = "textBoxOutputUnit";
		this.textBoxOutputUnit.ReadOnly = true;
		this.textBoxOutputUnit.Size = new System.Drawing.Size(211, 23);
		this.textBoxOutputUnit.TabIndex = 94;
		this.textBoxOutputValue.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxOutputValue.Location = new System.Drawing.Point(527, 69);
		this.textBoxOutputValue.Name = "textBoxOutputValue";
		this.textBoxOutputValue.ReadOnly = true;
		this.textBoxOutputValue.Size = new System.Drawing.Size(168, 23);
		this.textBoxOutputValue.TabIndex = 93;
		this.textBoxOutputValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.label7.AutoSize = true;
		this.label7.BackColor = System.Drawing.Color.Transparent;
		this.label7.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label7.Location = new System.Drawing.Point(511, 72);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(15, 14);
		this.label7.TabIndex = 92;
		this.label7.Text = ":";
		this.textBoxOutput.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxOutput.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxOutput.Location = new System.Drawing.Point(435, 68);
		this.textBoxOutput.Name = "textBoxOutput";
		this.textBoxOutput.ReadOnly = true;
		this.textBoxOutput.Size = new System.Drawing.Size(59, 23);
		this.textBoxOutput.TabIndex = 91;
		this.textBoxBoreUnit.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxBoreUnit.Location = new System.Drawing.Point(225, 69);
		this.textBoxBoreUnit.Name = "textBoxBoreUnit";
		this.textBoxBoreUnit.ReadOnly = true;
		this.textBoxBoreUnit.Size = new System.Drawing.Size(161, 23);
		this.textBoxBoreUnit.TabIndex = 90;
		this.textBoxBoreValue.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxBoreValue.Location = new System.Drawing.Point(83, 69);
		this.textBoxBoreValue.Name = "textBoxBoreValue";
		this.textBoxBoreValue.ReadOnly = true;
		this.textBoxBoreValue.Size = new System.Drawing.Size(103, 23);
		this.textBoxBoreValue.TabIndex = 89;
		this.textBoxBoreValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.label5.AutoSize = true;
		this.label5.BackColor = System.Drawing.Color.Transparent;
		this.label5.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label5.Location = new System.Drawing.Point(62, 72);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(15, 14);
		this.label5.TabIndex = 88;
		this.label5.Text = ":";
		this.textBoxBore.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxBore.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxBore.Location = new System.Drawing.Point(8, 68);
		this.textBoxBore.Name = "textBoxBore";
		this.textBoxBore.ReadOnly = true;
		this.textBoxBore.Size = new System.Drawing.Size(53, 23);
		this.textBoxBore.TabIndex = 87;
		this.textBoxMachineNoValueResult.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxMachineNoValueResult.Location = new System.Drawing.Point(435, 183);
		this.textBoxMachineNoValueResult.MaxLength = 24;
		this.textBoxMachineNoValueResult.Name = "textBoxMachineNoValueResult";
		this.textBoxMachineNoValueResult.ReadOnly = true;
		this.textBoxMachineNoValueResult.Size = new System.Drawing.Size(493, 23);
		this.textBoxMachineNoValueResult.TabIndex = 86;
		this.lblMacineNo.AutoSize = true;
		this.lblMacineNo.BackColor = System.Drawing.Color.WhiteSmoke;
		this.lblMacineNo.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblMacineNo.Location = new System.Drawing.Point(396, 186);
		this.lblMacineNo.Name = "lblMacineNo";
		this.lblMacineNo.Size = new System.Drawing.Size(31, 14);
		this.lblMacineNo.TabIndex = 85;
		this.lblMacineNo.Text = "NO:";
		this.textBoxCodeValue.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxCodeValue.Location = new System.Drawing.Point(83, 183);
		this.textBoxCodeValue.Name = "textBoxCodeValue";
		this.textBoxCodeValue.ReadOnly = true;
		this.textBoxCodeValue.Size = new System.Drawing.Size(303, 23);
		this.textBoxCodeValue.TabIndex = 84;
		this.label8.AutoSize = true;
		this.label8.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label8.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label8.Location = new System.Drawing.Point(62, 186);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(15, 14);
		this.label8.TabIndex = 83;
		this.label8.Text = ":";
		this.textBoxCode.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxCode.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxCode.Location = new System.Drawing.Point(8, 182);
		this.textBoxCode.Name = "textBoxCode";
		this.textBoxCode.ReadOnly = true;
		this.textBoxCode.Size = new System.Drawing.Size(53, 23);
		this.textBoxCode.TabIndex = 82;
		this.textBoxTokki.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxTokki.Location = new System.Drawing.Point(435, 146);
		this.textBoxTokki.Name = "textBoxTokki";
		this.textBoxTokki.ReadOnly = true;
		this.textBoxTokki.Size = new System.Drawing.Size(493, 23);
		this.textBoxTokki.TabIndex = 81;
		this.textBoxMadeNoValueResult.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxMadeNoValueResult.Location = new System.Drawing.Point(8, 146);
		this.textBoxMadeNoValueResult.Name = "textBoxMadeNoValueResult";
		this.textBoxMadeNoValueResult.ReadOnly = true;
		this.textBoxMadeNoValueResult.Size = new System.Drawing.Size(378, 23);
		this.textBoxMadeNoValueResult.TabIndex = 80;
		this.textBoxModelValue.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxModelValue.Location = new System.Drawing.Point(83, 31);
		this.textBoxModelValue.Name = "textBoxModelValue";
		this.textBoxModelValue.ReadOnly = true;
		this.textBoxModelValue.Size = new System.Drawing.Size(845, 23);
		this.textBoxModelValue.TabIndex = 79;
		this.textBoxModelValue.TextChanged += new System.EventHandler(textBoxModelValue_TextChanged);
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.WhiteSmoke;
		this.label1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label1.Location = new System.Drawing.Point(62, 34);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(15, 14);
		this.label1.TabIndex = 78;
		this.label1.Text = ":";
		this.textBoxModel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.textBoxModel.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.textBoxModel.Location = new System.Drawing.Point(8, 30);
		this.textBoxModel.Name = "textBoxModel";
		this.textBoxModel.ReadOnly = true;
		this.textBoxModel.Size = new System.Drawing.Size(53, 23);
		this.textBoxModel.TabIndex = 77;
		this.pnlBottomClientCtrl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlBottomClientCtrl.Controls.Add(this.checkBoxIsPreviewRealResult);
		this.pnlBottomClientCtrl.Controls.Add(this.btnPreview);
		this.pnlBottomClientCtrl.Controls.Add(this.btnPause);
		this.pnlBottomClientCtrl.Controls.Add(this.btnStop);
		this.pnlBottomClientCtrl.Controls.Add(this.btnPrint);
		this.pnlBottomClientCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlBottomClientCtrl.Location = new System.Drawing.Point(0, 0);
		this.pnlBottomClientCtrl.Name = "pnlBottomClientCtrl";
		this.pnlBottomClientCtrl.Size = new System.Drawing.Size(1008, 141);
		this.pnlBottomClientCtrl.TabIndex = 4;
		this.checkBoxIsPreviewRealResult.AutoSize = true;
		this.checkBoxIsPreviewRealResult.Checked = true;
		this.checkBoxIsPreviewRealResult.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBoxIsPreviewRealResult.Location = new System.Drawing.Point(13, 13);
		this.checkBoxIsPreviewRealResult.Name = "checkBoxIsPreviewRealResult";
		this.checkBoxIsPreviewRealResult.Size = new System.Drawing.Size(144, 16);
		this.checkBoxIsPreviewRealResult.TabIndex = 11;
		this.checkBoxIsPreviewRealResult.Text = "是否預覽實際噴印結果";
		this.checkBoxIsPreviewRealResult.UseVisualStyleBackColor = true;
		this.checkBoxIsPreviewRealResult.Visible = false;
		this.btnPreview.Location = new System.Drawing.Point(11, 36);
		this.btnPreview.Name = "btnPreview";
		this.btnPreview.Size = new System.Drawing.Size(113, 46);
		this.btnPreview.TabIndex = 10;
		this.btnPreview.Text = "預覽";
		this.btnPreview.UseVisualStyleBackColor = true;
		this.btnPreview.Click += new System.EventHandler(btnPreview_Click);
		this.btnPause.Enabled = false;
		this.btnPause.Location = new System.Drawing.Point(749, 36);
		this.btnPause.Name = "btnPause";
		this.btnPause.Size = new System.Drawing.Size(113, 46);
		this.btnPause.TabIndex = 9;
		this.btnPause.Text = "暫停";
		this.btnPause.UseVisualStyleBackColor = true;
		this.btnPause.Click += new System.EventHandler(btnPause_Click);
		this.btnStop.Enabled = false;
		this.btnStop.Location = new System.Drawing.Point(503, 36);
		this.btnStop.Name = "btnStop";
		this.btnStop.Size = new System.Drawing.Size(113, 46);
		this.btnStop.TabIndex = 8;
		this.btnStop.Text = "停止";
		this.btnStop.UseVisualStyleBackColor = true;
		this.btnStop.EnabledChanged += new System.EventHandler(btnStop_EnabledChanged);
		this.btnStop.Click += new System.EventHandler(btnStop_Click);
		this.btnPrint.Location = new System.Drawing.Point(257, 36);
		this.btnPrint.Name = "btnPrint";
		this.btnPrint.Size = new System.Drawing.Size(113, 46);
		this.btnPrint.TabIndex = 7;
		this.btnPrint.Text = "噴印";
		this.btnPrint.UseVisualStyleBackColor = true;
		this.btnPrint.EnabledChanged += new System.EventHandler(btnPrint_EnabledChanged);
		this.btnPrint.Click += new System.EventHandler(btnPrint_Click);
		this.pnlBottomBottomStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlBottomBottomStatus.Controls.Add(this.statusStripPrintCtrl);
		this.pnlBottomBottomStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.pnlBottomBottomStatus.Location = new System.Drawing.Point(0, 118);
		this.pnlBottomBottomStatus.Name = "pnlBottomBottomStatus";
		this.pnlBottomBottomStatus.Size = new System.Drawing.Size(1008, 23);
		this.pnlBottomBottomStatus.TabIndex = 5;
		this.statusStripPrintCtrl.BackColor = System.Drawing.Color.WhiteSmoke;
		this.statusStripPrintCtrl.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripStatusLabelPrintStatusIcon, this.toolStripStatusLabelPrintStatusText });
		this.statusStripPrintCtrl.Location = new System.Drawing.Point(0, -3);
		this.statusStripPrintCtrl.Name = "statusStripPrintCtrl";
		this.statusStripPrintCtrl.Size = new System.Drawing.Size(1004, 22);
		this.statusStripPrintCtrl.TabIndex = 9;
		this.statusStripPrintCtrl.Text = "statusStripAdtDemo";
		this.toolStripStatusLabelPrintStatusIcon.BackColor = System.Drawing.Color.Red;
		this.toolStripStatusLabelPrintStatusIcon.Name = "toolStripStatusLabelPrintStatusIcon";
		this.toolStripStatusLabelPrintStatusIcon.Size = new System.Drawing.Size(32, 17);
		this.toolStripStatusLabelPrintStatusIcon.Text = "      ";
		this.toolStripStatusLabelPrintStatusText.Name = "toolStripStatusLabelPrintStatusText";
		this.toolStripStatusLabelPrintStatusText.Size = new System.Drawing.Size(77, 17);
		this.toolStripStatusLabelPrintStatusText.Text = "獲取狀態中...";
		this.timerWaitData.Interval = 1000;
		this.timerWaitData.Tick += new System.EventHandler(timerWaitData_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(1016, 702);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "frmPrintCtrl";
		this.Text = "噴印控制";
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmMain_FormClosing);
		base.Load += new System.EventHandler(frmMain_Load);
		base.VisibleChanged += new System.EventHandler(frmPrintCtrl_VisibleChanged);
		base.pnlClientData.ResumeLayout(false);
		base.pnlBottomCtrl.ResumeLayout(false);
		this.pnlClientTopDataInput.ResumeLayout(false);
		this.pnlClientTopDataInput.PerformLayout();
		this.pnlClientFrontSideCartonSize.ResumeLayout(false);
		this.groupBoxCaseNo.ResumeLayout(false);
		this.groupBoxCaseNo.PerformLayout();
		this.gbDataSide.ResumeLayout(false);
		this.gbDataSide.PerformLayout();
		this.gbDataFront.ResumeLayout(false);
		this.gbDataFront.PerformLayout();
		this.pnlBottomClientCtrl.ResumeLayout(false);
		this.pnlBottomClientCtrl.PerformLayout();
		this.pnlBottomBottomStatus.ResumeLayout(false);
		this.pnlBottomBottomStatus.PerformLayout();
		this.statusStripPrintCtrl.ResumeLayout(false);
		this.statusStripPrintCtrl.PerformLayout();
		base.ResumeLayout(false);
	}
}
