using System;
using System.Drawing;
using System.Text;
using SpCommon;

namespace HeJianPrintingConsole;

public class ClsMsg
{
	public static class clsMsgBaseParam
	{
		public static int iMESSAGE_VERSION = 6;

		public static int iMAX_LEN_BARTEXT = 50;

		public static int iDEFAU_LINE_WIDTH = 100;

		public static int iDEFAU_LINE_HEIGHT = 10;
	}

	public static class clsBarcodeUid
	{
		public static string strUID_OBJ_UPCA = "ABCUPCA";

		public static string strUID_OBJ_UPCE = "ABCUPCE";

		public static string strUID_OBJ_EAN13 = "ABCEAN13";

		public static string strUID_OBJ_EAN8 = "ABCEAN8";

		public static string strUID_OBJ_INTER25 = "ABCINT25";

		public static string strUID_OBJ_CODEBAR = "ABCCOBAR";

		public static string strUID_OBJ_CODE39 = "ABCC39";

		public static string strUID_OBJ_CODE128 = "ABCC128";

		public static string strUID_OBJ_SCC18 = "ABCSSC18";
	}

	public static class clsDataSrcUID
	{
		public static string strUID_DATASRC_COUNTER = "ADSCOUNTER";

		public static string strUID_DATASRC_EXPIRYDATE = "ADSEXPIRYDATE";

		public static string strUID_DATASRC_PRODATE = "ADSPRODATE";

		public static string strUID_DATASRC_SHIFT = "ADSSHIFT";

		public static string strUID_DATASRC_SN = "ADSSN";

		public static string strUID_DATASRC_ADT = "ADSADT";

		public static string strUID_DATASRC_string = "ADSstring";

		public static string strUID_DATASRC_XLS = "ADSXLS";

		public static string strUID_DATASRC_CSV = "ADSCSV";

		public static string strUID_DATASRC_MDB = "ADSMDB";

		public static string strUID_DATASRC_MSSQL = "ADSMSSQL";
	}

	public static class clsDataSrc_AdtParam
	{
		public static int iAdtDataMaxLen = 2601;

		public static int iOneAdtField = 52;
	}

	private enum EnumMsg_StyleType
	{
		stNomal,
		stStrikeOut,
		stUnderline,
		stStrikUnder,
		stItalic,
		stItaStrick,
		stItaUnder,
		stItaUnderStrick,
		stBold,
		stBoldStrick,
		stBoldUnder,
		stBoldUnderStrick,
		stBoldIta,
		stBoldItaStrick,
		stBoldItaUnder,
		stBoldItaUnderStrick
	}

	public enum EnumMsg_Type
	{
		Msg_Text = 1,
		Msg_Logo = 6,
		Msg_BarCode = 7,
		Msg_Line = 9
	}

	private enum EnumBarCode_Type
	{
		UPCA = 1,
		UPCE = 2,
		EAN13 = 3,
		EAN8 = 4,
		INTER25 = 5,
		CODEBAR = 6,
		CODE39 = 7,
		CODE128 = 8,
		SCC18 = 10
	}

	public static class clsMsgSection
	{
		public static string strMsgInfo = "MsgInfo";

		public static string strObj = "Object";

		public static string strDataSrc = "DataSrc";
	}

	public enum EnumMsgAppendPageField2Type
	{
		NotMsgAppendPageField2,
		MsgInfo,
		MsgObj,
		MsgDataSrc
	}

	public class clsMsgKey_DataHead
	{
		public static string strMsgInfo = "[MsgInfo]";

		public static string strVer = "Ver";

		public static string strDataNo = "No";

		public static string strDataID = "ID";

		public static string strName = "Name";

		public static string strDataWidth = "Width";

		public static string strDataHeight = "Height";

		public static string strDataAngle = "Angle";

		public static string strCount = "Count";

		public static string strdaCount = "daCount";

		public static string strdsid = "dsid";
	}

	public class clsMsgValue_DataHead
	{
		public string strMsgInfo;

		public int iVer;

		public int iNo;

		public int iID;

		public string strName;

		public double dWidth;

		public double dHeight;

		public int iAngle;

		public int iCount;

		public int idaCount;

		public int idsid;
	}

	public class clsMsgCommonKey
	{
		public static string strObject = "Object";

		public static string strType = "Type";

		public static string strX = "X";

		public static string strY = "Y";

		public static string strZoomX = "ZoomX";

		public static string strZoomY = "ZoomY";

		public static string strWidth = "Width";

		public static string strHeight = "Height";
	}

	public class clsMsgCommonValue
	{
		public int iObjectX;

		public int iType;

		public int iX;

		public int iY;

		public float fZoomX;

		public float fZoomY;

		public double dWidth_Obj;

		public double dHeight_Obj;
	}

	public class clsMsgDataSrcKey
	{
		public static string strDataSrcTag = "DataSrc";

		public static string strUID = "UID";

		public static string strName = "Name";

		public static string strGroup = "Group";

		public static string strData = "Data";
	}

	public class clsMsgDataSrcValue
	{
		public string strDataSrcTab;

		public string strUID;

		public string strName;

		public string strGroup;

		public string strData;
	}

	public class clsMsgObjToSrcParam
	{
		public static string strMsgObjT = "MsgObjT";
	}

	public class clsMsgObjT_Info
	{
		public int iOriginalMsgUnitIndex;

		public int iOriginalDataSrcIndex;

		public int iOriginalDataSrcFieldIndex;

		public int iNewDataSrcIndex;

		public int iNewDataSrcFieldIndex;
	}

	public static class clsMsgKey_Text
	{
		public static clsMsgKey_DataHead clsMsgKeyDataHead;

		public static clsMsgCommonKey clsMsgComKey;

		public static string strAngle = "Angle";

		public static string strHide = "Hide";

		public static string strSpace = "Space";

		public static string strUsedPSFont = "UsedPSFont";

		public static string strFontSize = "FontSize";

		public static string strText = "Text";

		public static string strFontName = "FontName";

		public static string strStyleType = "StyleType";

		public static string strSimpleData = "SimpleData";

		public static string strMsgObjT = "MsgObjT";
	}

	public class clsMsgValue_Text
	{
		public clsMsgValue_DataHead clsMsgValueDataHead = new clsMsgValue_DataHead();

		public clsMsgCommonValue clsMsgComValue = new clsMsgCommonValue();

		public int iAngle_Obj;

		public int iHide;

		public int iSpace;

		public int iUsedPSFont;

		public int iFontSize;

		public string strText;

		public string strFontName;

		public int iStyleType;

		public string strSimpleData;

		public int iMsgObjTX;

		public int iMsgObjTY;

		public string strMsgObjTText;
	}

	public static class clsMsgKey_Logo
	{
		public static clsMsgKey_DataHead clsMsgKeyDataHead;

		public static clsMsgCommonKey clsMsgComKey;

		public static string strAngle = "Angle";

		public static string strLogoNo = "No";

		public static string strLogoID = "Id";

		public static string strDataType = "DataType";

		public static string strLogoFile = "LogoFile";

		public static string strData = "Data";

		public static string strMsgObjT = "MsgObjT";
	}

	public class clsMsgValue_Logo
	{
		public clsMsgValue_DataHead clsMsgValueDataHead = new clsMsgValue_DataHead();

		public clsMsgCommonValue clsMsgComValue = new clsMsgCommonValue();

		public int iAngle_Obj;

		public int iLogoNo;

		public int iLogoId;

		public int iDataType;

		public string strLogoFile;

		public string strData;

		public int iMsgObjTX;

		public int iMsgObjTY;

		public string strMsgObjTText;
	}

	public static class clsMsgKey_Barcode
	{
		public static clsMsgKey_DataHead clsMsgKeyDataHead;

		public static clsMsgCommonKey clsMsgComKey;

		public static string strAngle = "Angle";

		public static string strHide = "Hide";

		public static string strUID = "UID";

		public static string strBarType = "BarType";

		public static string strTextModel = "TextModel";

		public static string strWidthLevel = "WidthLevel";

		public static string strBarHeight = "BarHeight";

		public static string strECLevel = "ECLevel";

		public static string strPrivateData = "PrivateData";

		public static string strSimpleData = "SimpleData";

		public static string strContent = "Content";

		public static string strGroupMode = "";
	}

	public class clsMsgValue_Barcode
	{
		public clsMsgValue_DataHead clsMsgValueDataHead = new clsMsgValue_DataHead();

		public clsMsgCommonValue clsMsgComValue = new clsMsgCommonValue();

		public static int iAngle_Obj;

		public int iHide;

		public string strUID;

		public int iBarType;

		public int iTextModel;

		public int iWidthLevel;

		public int iBarHeight;

		public int iECLevel;

		public int iPrivateData;

		public string strSimpleData;

		public string strContent;

		public string strGroupMode;
	}

	public class clsMsgKey_Line
	{
		public clsMsgValue_DataHead clsMsgValueDataHead = new clsMsgValue_DataHead();

		public clsMsgCommonValue clsMsgComValue = new clsMsgCommonValue();
	}

	public static class clsMsgValue_Line
	{
		public static clsMsgValue_DataHead clsMsgValueDataHead;

		public static clsMsgCommonValue clsMsgComValue;
	}

	public static int GetMsgWidth(string strMsgFilePath)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strMsgInfo;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgKey_DataHead.strDataWidth);
			return Convert.ToInt32(strDest);
		}
		catch
		{
			return 1;
		}
	}

	public static int GetMsgHeight(string strMsgFilePath)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strMsgInfo;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgKey_DataHead.strDataHeight);
			return Convert.ToInt32(strDest);
		}
		catch
		{
			return 1;
		}
	}

	public static int GetObjCount(string strMsgFilePath)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strMsgInfo;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgKey_DataHead.strCount);
			return Convert.ToInt32(strDest);
		}
		catch
		{
			return 1;
		}
	}

	public static string GetText(string strMsgFilePath, int iObjIndex, EnumMsg_Type EnumType)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			switch (EnumType)
			{
			case EnumMsg_Type.Msg_Text:
			{
				string strTmp = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgKey_Text.strMsgObjT + "0", Encoding.Default);
				string[] strArrTmp = strTmp.Split(',');
				strDest = strTmp.Substring(strArrTmp[0].Length + 1 + strArrTmp[1].Length + 1);
				strDest = strDest.Substring(0, strDest.Length - 1);
				break;
			}
			}
			return strDest;
		}
		catch
		{
			return "";
		}
	}

	public static string GetFontName(string strMsgFilePath, int iObjIndex, EnumMsg_Type EnumType)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			return ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgKey_Text.strFontSize);
		}
		catch
		{
			return "Arial";
		}
	}

	public static int GetFontSize(string strMsgFilePath, int iObjIndex, EnumMsg_Type EnumType)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgKey_Text.strFontSize);
			return Convert.ToInt32(strDest);
		}
		catch
		{
			return 10;
		}
	}

	public static FontStyle GetFontStyle(string strMsgFilePath, int iObjIndex, EnumMsg_Type EnumType)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgKey_Text.strStyleType);
			int iTmp = Convert.ToInt32(strDest);
			FontStyle fontStyleTmp = FontStyle.Regular;
			switch (iTmp)
			{
			case 8:
				fontStyleTmp = FontStyle.Bold;
				break;
			case 12:
				fontStyleTmp = FontStyle.Bold | FontStyle.Italic;
				break;
			case 13:
				fontStyleTmp = FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout;
				break;
			case 14:
				fontStyleTmp = FontStyle.Bold | FontStyle.Italic | FontStyle.Underline;
				break;
			case 15:
				fontStyleTmp = FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout;
				break;
			case 9:
				fontStyleTmp = FontStyle.Bold | FontStyle.Strikeout;
				break;
			case 10:
				fontStyleTmp = FontStyle.Bold | FontStyle.Underline;
				break;
			case 11:
				fontStyleTmp = FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout;
				break;
			case 4:
				fontStyleTmp = FontStyle.Italic;
				break;
			case 5:
				fontStyleTmp = FontStyle.Italic | FontStyle.Strikeout;
				break;
			case 6:
				fontStyleTmp = FontStyle.Italic | FontStyle.Underline;
				break;
			case 7:
				fontStyleTmp = FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout;
				break;
			case 0:
				fontStyleTmp = FontStyle.Regular;
				break;
			case 1:
				fontStyleTmp = FontStyle.Strikeout;
				break;
			case 3:
				fontStyleTmp = FontStyle.Underline | FontStyle.Strikeout;
				break;
			case 2:
				fontStyleTmp = FontStyle.Underline;
				break;
			}
			return fontStyleTmp;
		}
		catch
		{
			return FontStyle.Regular;
		}
	}

	public static int GetObjX(string strMsgFilePath, int iObjIndex)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgCommonKey.strX);
			return Convert.ToInt32(strDest);
		}
		catch
		{
			return 0;
		}
	}

	public static int GetObjY(string strMsgFilePath, int iObjIndex)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgCommonKey.strY);
			return Convert.ToInt32(strDest);
		}
		catch
		{
			return 0;
		}
	}

	public static double GetZoomX(string strMsgFilePath, int iObjIndex)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgCommonKey.strZoomX);
			return Convert.ToDouble(strDest);
		}
		catch
		{
			return 1.0;
		}
	}

	public static double GetZoomY(string strMsgFilePath, int iObjIndex)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgCommonKey.strZoomY);
			return Convert.ToDouble(strDest);
		}
		catch
		{
			return 1.0;
		}
	}

	public static int GetObjWidth(string strMsgFilePath, int iObjIndex)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgCommonKey.strWidth);
			return Convert.ToInt32(strDest);
		}
		catch
		{
			return 1;
		}
	}

	public static int GetObjHeight(string strMsgFilePath, int iObjIndex)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgCommonKey.strHeight);
			return Convert.ToInt32(strDest);
		}
		catch
		{
			return 1;
		}
	}

	public static int GetObjAngle(string strMsgFilePath, int iObjIndex, EnumMsg_Type EnumType)
	{
		try
		{
			string strDest = "";
			string strAppendPageField2 = clsMsgSection.strObj + iObjIndex;
			switch (EnumType)
			{
			case EnumMsg_Type.Msg_Text:
				strDest = ClsIni.GetValue(strMsgFilePath, strAppendPageField2, clsMsgKey_Text.strAngle);
				break;
			}
			return Convert.ToInt32(strDest);
		}
		catch
		{
			return 0;
		}
	}

	public static void RecordTextObjParam(string strMsgFilePath, int iObjIndex)
	{
		GetFontName(strMsgFilePath, iObjIndex, EnumMsg_Type.Msg_Text);
		GetFontSize(strMsgFilePath, iObjIndex, EnumMsg_Type.Msg_Text);
		GetFontStyle(strMsgFilePath, iObjIndex, EnumMsg_Type.Msg_Text);
	}

	public static void ReplaceTextObjToLogoObj(string strMsgFilePath, int iObjIndex)
	{
	}
}
