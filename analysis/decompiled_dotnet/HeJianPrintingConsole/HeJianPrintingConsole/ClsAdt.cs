using System;
using System.Data;
using System.Runtime.InteropServices;

namespace HeJianPrintingConsole;

public class ClsAdt
{
	public enum EnumANSER_PRINTER_STATUS
	{
		apsOffline,
		apsPrint,
		apsStop,
		apsAlarm
	}

	public enum EnumANSER_PRINTER_ALARM
	{
		apaNoAlarm,
		apaPHError,
		apaSysError,
		apaPHWarning,
		apaSysWarning
	}

	public enum EnumANSER_PRINTER_LED
	{
		aplNormal,
		aplError,
		aplAlarm
	}

	public enum EnumAllPrintStatusCode
	{
		NotOpenPrinter = 1,
		PSOffLine,
		Printing,
		PrintStop,
		Alarm,
		GetStatusFail,
		SystemNormal,
		PHWarning,
		PHError,
		SystemWarning,
		SystemError,
		GetAlarmDetailFail
	}

	public static class ClsPrintStatus
	{
		public static string strNotOpenPrinter = "未打開PrintingPro!";

		public static string strPSOffLine = "噴印站離線，請打開PrintingPro查看!";

		public static string strPrinting = "噴印中...";

		public static string strPrintStop = "噴印停止";

		public static string strAlarm = "警告，請打開PrintingPro查看!";

		public static string strGetStatusFail = "獲取狀態失敗,請確認是否打開PrintingPro!";

		public static string strSystemNormal = "系統正常";

		public static string strPHWarning = "卡匣警告，請打開PrintingPro查看!";

		public static string strPHError = "卡匣錯誤，請打開PrintingPro查看!";

		public static string strSystemWarning = "系統警告，請打開PrintingPro查看!";

		public static string strSystemError = "系統錯誤，請打開PrintingPro查看!";

		public static string strGetAlarmDetailFail = "獲取警告明細失敗,請確認PrintingPro是否正常!";
	}

	public static class ClsPrintHeadItems
	{
		public static string strColumnPhNoName = "PrintHeadN0";

		public static string strColumnPhNoCaption = "PrintHead #";

		public static string strColumnPhDelayName = "PrintHeadDelay";

		public static string strColumnPhDelayCaption = "Delay (Unit: mm)";

		public static int iDelayUnit = 1;
	}

	[DllImport("aimdtl.dll")]
	public static extern int imOpenPrinter();

	[DllImport("aimdtl.dll")]
	public static extern int imGetExclusiver(byte[] byteArrExclusiverName, int[] iArrExclusiverPid);

	[DllImport("aimdtl.dll")]
	public static extern int imSendCancel();

	[DllImport("aimdtl.dll")]
	public static extern int imInitADT(byte[] byteArrInitAdt);

	[DllImport("aimdtl.dll")]
	public static extern int imPrinterStart();

	[DllImport("aimdtl.dll")]
	public static extern int imGetAdtMsgFieldCount(string strMsgFile);

	[DllImport("aimdtl.dll")]
	public static extern int imGetAdtMsgFieldType(string strMsgFile, int iFieldIndex, byte[] byteArrMsgFieldName);

	[DllImport("aimdtl.dll")]
	public static extern int imAppendMessage(byte[] ByteArrAdtMsgData, uint uiAdtMsgSize, uint uiBeginPage, uint uiRepeat);

	[DllImport("aimdtl.dll")]
	public static extern int imGetADTStatus(bool canReadOrWrite);

	[DllImport("aimdtl.dll")]
	public static extern int imPageBegin(int iPageSn);

	[DllImport("aimdtl.dll")]
	public static extern int imAppendPageField(int iDataType, string strSendData, int iSendDataLen);

	[DllImport("aimdtl.dll")]
	public static extern int imAppendPageField(int iDataType, byte[] byteArrBmpData, int iSendDataLen);

	[DllImport("aimdtl.dll")]
	public static extern int imPageEnd();

	[DllImport("aimdtl.dll")]
	public static extern int imPrinterStop();

	[DllImport("aimdtl.dll")]
	public static extern int imClosePrinter();

	[DllImport("aimdtl.dll")]
	public static extern int imGetPrinttedPagesn(ref uint uiPrintedPageSn);

	[DllImport("aimdtl.dll")]
	public static extern int imGetPrinterStatus(int[] iArrStatus);

	[DllImport("aimdtl.dll")]
	public static extern int imGetPrinterAlarm(int[] iArrAlarm);

	[DllImport("aimdtl.dll")]
	public static extern int imClearPrintQueue();

	[DllImport("aimdtl.dll")]
	public static extern int imClearReturnQueue();

	[DllImport("aimdtl.dll")]
	public static extern int imCheckIsAdtMsg(string strMsgFile);

	[DllImport("aimdtl.dll")]
	public static extern int imGetLgphDelay(int iPhIndex);

	[DllImport("aimdtl.dll")]
	public static extern int imSetLgphDelay(int index, int delay);

	[DllImport("aimdtl.dll")]
	public static extern int imGetPHsHeight();

	[DllImport("aimdtl.dll")]
	public static extern int imSetVMirrorCount(int iMirCount);

	public static DataTable GetPrintHeadDelayList()
	{
		int iPhIndex = 0;
		string strDelayVal = "";
		DataTable dtPhDelayList = new DataTable();
		dtPhDelayList.Columns.Add();
		dtPhDelayList.Columns.Add();
		dtPhDelayList.Columns[0].Caption = ClsPrintHeadItems.strColumnPhNoCaption;
		dtPhDelayList.Columns[1].Caption = ClsPrintHeadItems.strColumnPhDelayCaption;
		dtPhDelayList.Columns[0].ColumnName = ClsPrintHeadItems.strColumnPhNoName;
		dtPhDelayList.Columns[1].ColumnName = ClsPrintHeadItems.strColumnPhDelayName;
		int iVal;
		while ((iVal = imGetLgphDelay(iPhIndex)) >= 0)
		{
			dtPhDelayList.Rows.Add();
			dtPhDelayList.Rows[iPhIndex][ClsPrintHeadItems.strColumnPhNoName] = "PH #" + (iPhIndex + 1);
			strDelayVal = ((ClsPrintHeadItems.iDelayUnit != 1) ? $"{(double)(float)iVal / 2540.0 + 0.001:f1}" : $"{(double)(float)iVal / 100.0 + 0.01:f1}");
			dtPhDelayList.Rows[iPhIndex][ClsPrintHeadItems.strColumnPhDelayName] = strDelayVal;
			iPhIndex++;
		}
		return dtPhDelayList;
	}

	public static void SetPrintHeadDelay(DataTable dtPrintHeadData, ref bool bSetResult, ref int iSetPrintHeadNo)
	{
		for (int iRowIndex = 0; iRowIndex < dtPrintHeadData.Rows.Count; iRowIndex++)
		{
			int iPhIndex = iRowIndex;
			float fPhDelay = (float)Convert.ToDouble(dtPrintHeadData.Rows[iRowIndex][ClsPrintHeadItems.strColumnPhDelayName].ToString());
			int iSetRlt = 0;
			iSetRlt = ((ClsPrintHeadItems.iDelayUnit != 1) ? imSetLgphDelay(iPhIndex, (int)(((double)fPhDelay + 0.001) * 2540.0)) : imSetLgphDelay(iPhIndex, (int)(((double)fPhDelay + 0.01) * 100.0)));
			if (iSetRlt == 1)
			{
				bSetResult = true;
				continue;
			}
			bSetResult = false;
			break;
		}
	}
}
