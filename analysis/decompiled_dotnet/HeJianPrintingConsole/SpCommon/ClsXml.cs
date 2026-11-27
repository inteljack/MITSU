using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SpCommon;

internal class ClsXml
{
	public static class ClsXmlMember
	{
		public static string strRootNodeName = "Anser";

		public static string strFirstFloorSubNodeName = "ARow";

		public static string strSecondFloorSubNodeNameKey = "Key";

		public static string strSecondFloorSubNodeNameValue = "Value";

		public static string strServerName = "ServerName";

		public static string strDatabaseName = "DatabaseName";

		public static string strDatabaseUserID = "DatabaseUserID";

		public static string strDatabasePwd = "DatabasePwd";

		public static string strInitializDataBaseTabsFileSrc = "InitializDataBaseTabsFileSrc";

		public static string strPrintDataTabel = "PrintDataTabel";

		public static string strProductRecordMasterTabel = "ProductRecordMasterTabel";

		public static string strProductRecordDetailTabel = "ProductRecordDetailTabel";

		public static string strCartonSizeTable = "CartonSizeTable";

		public static string strModelFolder = "ModelFolder";

		public static string strModelName = "ModelName";

		public static string strSystemPwd = "SystemPwd";

		public static string strWidthDPI = "WidthDPI";

		public static string strHeightDPI = "HeightDPI";

		public static string strPrintWaitTotalTime = "PrintWaitTotalTime";

		public static string strPrintWaitScanFrequency = "PrintWaitScanFrequency";

		public static string strRestartAppFlag = "RestartAppFlag";

		public static string strAdtFieldCount = "AdtFieldCount";

		public static string strPreviewZoomScale = "PreviewZoomScale";

		public static string strPrintMaxMem = "PrintMaxMem";

		public static string strAppHasRunFlag = "AppHasRunFlag";

		public static string strWidthRulerMaxLen = "WidthRulerMaxLen";

		public static string strHeightRulerMaxLen = "HeightRulerMaxLen";

		public static string strAddSpaceLengthWhenTextToBmp = "AddSpaceLengthWhenTextToBmp";

		public static string strDataBufferTime_s = "DataBufferTime_s";

		public static string strSystemMusicNormalFileName = "SystemMusicNormalFileName";

		public static string strSystemMusicNormalDuration = "SystemMusicNormalDuration";

		public static string strSystemMusicNormalTotalTime = "SystemMusicNormalTotalTime";

		public static string strSystemMusicWarmFileName = "MusicWarmFileName";

		public static string strSystemMusicWarmDuration = "MusicWarmDuration";

		public static string strSystemMusicWarmTotalTime = "SystemMusicWarmTotalTime";

		public static string strImportFolder = "ImportFolder";

		public static string strExportFolder = "ExportFolder";

		public static string strMainFormX = "MainFormX";

		public static string strMainFormY = "MainFormY";

		public static string strMainFormWidth = "MainFormWidth";

		public static string strMainFormHeight = "MainFormHeight";

		public static string strShowPrintStatusFormX = "ShowPrintStatusFormX";

		public static string strShowPrintStatusFormY = "ShowPrintStatusFormY";

		public static string strShowPrintStatusFormWidth = "ShowPrintStatusFormWidth";

		public static string strShowPrintStatusFormHeight = "ShowPrintStatusFormHeight";

		public static string strPreviewFormX = "PreviewFormX";

		public static string strPreviewFormY = "PreviewFormY";

		public static string strPreviewFormWidth = "PreviewFormWidth";

		public static string strPreviewFormHeight = "PreviewFormHeight";

		public static string strSourceMsg1Path = "SourceMsg1Path";

		public static string strSourceMsg2Path = "SourceMsg2Path";

		public static string strOneMsgMaxPrintHeadCountOnGroupMsg = "OneMsgMaxPrintHeadCountOnGroupMsg";

		public static string strSinglePrintHeadHeight = "SinglePrintHeadHeight";

		public static string strPrintHead1FixedDelay1 = "PrintHead1FixedDelay1";

		public static string strPrintHead1FixedDelay2 = "PrintHead1FixedDelay2";

		public static string strPrintHead2FixedDelay1 = "PrintHead2FixedDelay1";

		public static string strPrintHead2FixedDelay2 = "PrintHead2FixedDelay2";

		public static string strPrintHead3FixedDelay1 = "PrintHead3FixedDelay1";

		public static string strPrintHead3FixedDelay2 = "PrintHead3FixedDelay2";

		public static string strPrintHead4FixedDelay1 = "PrintHead4FixedDelay1";

		public static string strPrintHead4FixedDelay2 = "PrintHead4FixedDelay2";

		public static string strModelValueFrontObjIndex = "ModelValueFrontObjIndex";

		public static string strModelValueFrontFontName = "ModelValueFrontFontName";

		public static string strModelValueFrontFontSize = "ModelValueFrontFontSize";

		public static string strModelValueFrontFontStyle = "ModelValueFrontFontStyle";

		public static string strModelValueSideObjIndex = "ModelValueSideObjIndex";

		public static string strModelValueSideFontName = "ModelValueSideFontName";

		public static string strModelValueSideFontSize = "ModelValueSideFontName";

		public static string strModelValueSideFontStyle = "ModelValueSideFontName";

		public static string strPlaceObjIndex = "PlaceObjIndex";

		public static string strPlaceFontName = "PlaceFontName";

		public static string strPlaceFontSize = "PlaceFontSize";

		public static string strPlaceFontStyle = "PlaceFontStyle";

		public static string strSpaceBetweenVoltageUnitAndHzValue = "SpaceBetweenVoltageUnitAndHzValue";

		public static string strPrintDataLen_KeyCode = "PrintDataLen_KeyCode";

		public static string strPrintDataLen_CaseNo = "PrintDataLen_CaseNo";

		public static string strPrintDataLen_Model = "PrintDataLen_Model";

		public static string strPrintDataLen_ModelValue = "PrintDataLen_ModelValue";

		public static string strPrintDataLen_Bore = "PrintDataLen_Bore";

		public static string strPrintDataLen_BoreValue = "PrintDataLen_BoreValue";

		public static string strPrintDataLen_BoreUnit = "PrintDataLen_BoreUnit";

		public static string strPrintDataLen_Output = "PrintDataLen_Output";

		public static string strPrintDataLen_OutputValue = "PrintDataLen_OutputValue";

		public static string strPrintDataLen_OutputUnit = "PrintDataLen_OutputUnit";

		public static string strPrintDataLen_PhaseValue = "PrintDataLen_PhaseValue";

		public static string strPrintDataLen_PhaseUnit = "PrintDataLen_PhaseUnit";

		public static string strPrintDataLen_VoltageValue = "PrintDataLen_VoltageValue";

		public static string strPrintDataLen_VoltageUnit = "PrintDataLen_VoltageUnit";

		public static string strPrintDataLen_HzValue = "PrintDataLen_HzValue";

		public static string strPrintDataLen_HzUnit = "PrintDataLen_HzUnit";

		public static string strPrintDataLen_Code = "PrintDataLen_Code";

		public static string strPrintDataLen_CodeValue = "PrintDataLen_CodeValue";

		public static string strPrintDataLen_Tokki = "PrintDataLen_Tokki";

		public static string strPrintDataLen_Place = "PrintDataLen_Place";
	}

	public static string getXmlCurPath()
	{
		return Application.StartupPath + "\\Config.xml";
	}

	public static string getParaValue(string ParaKeyField, string ParaKeyValue, string ParaValueField)
	{
		DataSet ds = new DataSet();
		string strXmlPath = getXmlCurPath();
		ds.ReadXml(strXmlPath);
		DataTable dt = ds.Tables[0];
		for (int i = 0; i < dt.Rows.Count; i++)
		{
			if (dt.Rows[i][ParaKeyField].ToString().Trim() == ParaKeyValue.Trim())
			{
				return dt.Rows[i][ParaValueField].ToString();
			}
		}
		return null;
	}

	public static bool setParaValue(string strParaKeyField, string strParaKeyValue, string strParaValueField, string strValueFieldValue)
	{
		try
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode xmlRoot = null;
			string strRootNodeName = ClsXmlMember.strRootNodeName;
			string strFirstFloorSubNode = ClsXmlMember.strFirstFloorSubNodeName;
			string strXmlPath = getXmlCurPath();
			if (!File.Exists(strXmlPath))
			{
				XmlDeclaration xdl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
				xmlDoc.PrependChild(xdl);
				xmlRoot = xmlDoc.CreateElement(strRootNodeName);
				xmlDoc.AppendChild(xmlRoot);
				xmlDoc.Save(strXmlPath);
			}
			xmlDoc.Load(strXmlPath);
			xmlRoot = xmlDoc.SelectSingleNode(strRootNodeName);
			if (xmlRoot == null)
			{
				xmlRoot = xmlDoc.CreateElement(strRootNodeName);
				xmlDoc.AppendChild(xmlRoot);
			}
			int iIdIndex = 1;
			bool bExistNode = false;
			for (int iIndex = 0; iIndex < xmlRoot.ChildNodes.Count; iIndex++)
			{
				iIdIndex++;
				if (xmlRoot.ChildNodes[iIndex].ChildNodes[0].InnerText == strParaKeyValue)
				{
					xmlRoot.ChildNodes[iIndex].ChildNodes[1].InnerText = strValueFieldValue;
					bExistNode = true;
					break;
				}
			}
			if (!bExistNode)
			{
				XmlElement xeFirstFloorSubNode = xmlDoc.CreateElement(strFirstFloorSubNode);
				xeFirstFloorSubNode.SetAttribute("ID", iIdIndex.ToString());
				xmlRoot.AppendChild(xeFirstFloorSubNode);
				XmlElement xeSecondFloorSubNode1 = xmlDoc.CreateElement(strParaKeyField);
				xeSecondFloorSubNode1.InnerText = strParaKeyValue;
				xeFirstFloorSubNode.AppendChild(xeSecondFloorSubNode1);
				XmlElement xeSecondFloorSubNode2 = xmlDoc.CreateElement(strParaValueField);
				xeSecondFloorSubNode2.InnerText = strValueFieldValue;
				xeFirstFloorSubNode.AppendChild(xeSecondFloorSubNode2);
			}
			xmlDoc.Save(strXmlPath);
			return true;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite(ex.Message);
			return false;
		}
	}

	public static string GetServerName()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strServerName, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetDataBaseName()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strDatabaseName, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetDataBaseUserID()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strDatabaseUserID, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetDataBasePwd()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strDatabasePwd, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetInitializDataBaseTabsFileSrc()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strInitializDataBaseTabsFileSrc, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetPrintDataTableName()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintDataTabel, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetProductRecordMasterTableName()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strProductRecordMasterTabel, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetProductRecordDetailTableName()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strProductRecordDetailTabel, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetCartonSizeTableName()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strCartonSizeTable, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetModelFolder()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelFolder, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetModelName()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelName, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetModelFolderAndName()
	{
		string strModelFolder = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelFolder, ClsXmlMember.strSecondFloorSubNodeNameValue);
		string strModelName = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelName, ClsXmlMember.strSecondFloorSubNodeNameValue);
		return strModelFolder + "\\" + strModelName;
	}

	public static string GetImportFloder()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strImportFolder, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetExportFloder()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strExportFolder, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetSysPwd()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strSystemPwd, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static double GetWidthDPI()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strWidthDPI, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToDouble(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get WidthDPI Exception:" + ex.Message);
			return 1.0;
		}
	}

	public static bool GetRestartAppFlag()
	{
		try
		{
			bool bRlt = false;
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strRestartAppFlag, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return strRlt.Trim().ToUpper() == "TRUE";
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("GetRestartAppFlag Exception:" + ex.Message);
			return false;
		}
	}

	public static int GetAdtFieldCount()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strAdtFieldCount, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get WidthDPI Exception:" + ex.Message);
			return 0;
		}
	}

	public static int GetPreviewZoomScale()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPreviewZoomScale, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get WidthDPI Exception:" + ex.Message);
			return 100;
		}
	}

	public static int GetPrintMaxMem()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintMaxMem, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get PrintMaxMem Exception:" + ex.Message);
			return 500;
		}
	}

	public static int GetSpaceBetweenVoltageUnitAndHzValue()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strSpaceBetweenVoltageUnitAndHzValue, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get SpaceBetweenVoltageUnitAndHzValue Exception:" + ex.Message);
			return 5;
		}
	}

	public static bool GetAppHasRunFlag()
	{
		try
		{
			bool bRlt = false;
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strAppHasRunFlag, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return strRlt.Trim().ToUpper() == "TRUE";
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Application has run Exception:" + ex.Message);
			return false;
		}
	}

	public static int GetHeightDPI()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strHeightDPI, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get height dpi Exception:" + ex.Message);
			return 300;
		}
	}

	public static int GetWidthRulerMaxLen()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strWidthRulerMaxLen, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get height ruler max len Exception:" + ex.Message);
			return 1800;
		}
	}

	public static int GetHeightRulerMaxLen()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strHeightRulerMaxLen, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get height ruler max len Exception:" + ex.Message);
			return 1000;
		}
	}

	public static int GetAddSpaceLengthWhenTextToBmp()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strAddSpaceLengthWhenTextToBmp, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get AddSpaceLengthWhenTextToBmp Exception:" + ex.Message);
			return 1;
		}
	}

	public static int GetDataBufferTime()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strDataBufferTime_s, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get data buffer time Exception:" + ex.Message);
			return 60;
		}
	}

	public static string GetNormalMusicPath()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strSystemMusicNormalFileName, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static int GetNormalMusicDuration()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strSystemMusicNormalDuration, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("获取正常提示音持续时间异常:" + ex.Message);
			return -1;
		}
	}

	public static int GetWarmMusicDuration()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strSystemMusicWarmDuration, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("获取警告提示音持续时间异常:" + ex.Message);
			return -1;
		}
	}

	public static string GetWarmMusicPath()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strSystemMusicWarmFileName, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static int GetMainFormX()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strMainFormX, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 0;
		}
	}

	public static int GetMainFormY()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strMainFormY, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 0;
		}
	}

	public static int GetMainFormWidth()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strMainFormWidth, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 1;
		}
	}

	public static int GetMainFormHeight()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strMainFormHeight, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 1;
		}
	}

	public static int GetShowPrintStatusFormX()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strShowPrintStatusFormX, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 0;
		}
	}

	public static int GetShowPrintStatusFormY()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strShowPrintStatusFormY, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 0;
		}
	}

	public static int GetShowPrintStatusFormWidth()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strShowPrintStatusFormWidth, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 1;
		}
	}

	public static int GetShowPrintStatusFormHeight()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strShowPrintStatusFormHeight, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 1;
		}
	}

	public static int GetPreviewFormX()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPreviewFormX, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 0;
		}
	}

	public static int GetPreviewFormY()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPreviewFormY, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 0;
		}
	}

	public static int GetPreviewFormWidth()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPreviewFormWidth, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 1;
		}
	}

	public static int GetPreviewFormHeight()
	{
		try
		{
			return Convert.ToInt32(getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPreviewFormHeight, ClsXmlMember.strSecondFloorSubNodeNameValue));
		}
		catch
		{
			return 1;
		}
	}

	public static string GetSourceMsg1Path()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strSourceMsg1Path, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetSourceMsg2Path()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strSourceMsg2Path, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetOneMsgMaxPrintHeadCountOnGroupMsg()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strOneMsgMaxPrintHeadCountOnGroupMsg, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static string GetSinglePrintHeadHeight()
	{
		return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strSinglePrintHeadHeight, ClsXmlMember.strSecondFloorSubNodeNameValue);
	}

	public static double GetPH1Delay1()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintHead1FixedDelay1, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToDouble(strRlt);
		}
		catch
		{
			return 0.0;
		}
	}

	public static double GetPH1Delay2()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintHead1FixedDelay2, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToDouble(strRlt);
		}
		catch
		{
			return 0.0;
		}
	}

	public static double GetPH2Delay1()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintHead2FixedDelay1, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToDouble(strRlt);
		}
		catch
		{
			return 0.0;
		}
	}

	public static double GetPH2Delay2()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintHead2FixedDelay2, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToDouble(strRlt);
		}
		catch
		{
			return 0.0;
		}
	}

	public static double GetPH3Delay1()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintHead3FixedDelay1, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToDouble(strRlt);
		}
		catch
		{
			return 0.0;
		}
	}

	public static double GetPH3Delay2()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintHead3FixedDelay2, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToDouble(strRlt);
		}
		catch
		{
			return 0.0;
		}
	}

	public static double GetPH4Delay1()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintHead4FixedDelay1, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToDouble(strRlt);
		}
		catch
		{
			return 0.0;
		}
	}

	public static double GetPH4Delay2()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintHead4FixedDelay2, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToDouble(strRlt);
		}
		catch
		{
			return 0.0;
		}
	}

	public static long GetPrintWaitTotalTime()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintWaitTotalTime, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt64(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get PrintWaitTotalTime Exception:" + ex.Message);
			return 1000L;
		}
	}

	public static long GetPrintWaitScanFrequency()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPrintWaitScanFrequency, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt64(strRlt);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Get PrintWaitScanFrequency Exception:" + ex.Message);
			return 1000L;
		}
	}

	public static int GetModelValueObjIndex_Front()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelValueFrontObjIndex, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch
		{
			return 22;
		}
	}

	public static string GetModelValueFontName_Front()
	{
		try
		{
			return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelValueFrontFontName, ClsXmlMember.strSecondFloorSubNodeNameValue);
		}
		catch
		{
			return "Arial";
		}
	}

	public static int GetModelValueFontSize_Front()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelValueFrontFontSize, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch
		{
			return 140;
		}
	}

	public static int GetModelValueFontStyle_Front()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelValueFrontFontStyle, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch
		{
			return 0;
		}
	}

	public static int GetModelValueObjIndex_Side()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelValueSideObjIndex, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch
		{
			return 24;
		}
	}

	public static string GetModelValueFontName_Side()
	{
		try
		{
			return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelValueSideFontName, ClsXmlMember.strSecondFloorSubNodeNameValue);
		}
		catch
		{
			return "Arial";
		}
	}

	public static int GetModelValueFontSize_Side()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelValueSideFontSize, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch
		{
			return 280;
		}
	}

	public static int GetModelValueFontStyle_Side()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strModelValueSideFontStyle, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch
		{
			return 0;
		}
	}

	public static int GetPlaceObjIndex()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPlaceObjIndex, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch
		{
			return 23;
		}
	}

	public static string GetPlaceFontName()
	{
		try
		{
			return getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPlaceFontName, ClsXmlMember.strSecondFloorSubNodeNameValue);
		}
		catch
		{
			return "Arial";
		}
	}

	public static int GetPlaceFontSize()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPlaceFontSize, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch
		{
			return 80;
		}
	}

	public static int GetPlaceFontStyle()
	{
		try
		{
			string strRlt = getParaValue(ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXmlMember.strPlaceFontStyle, ClsXmlMember.strSecondFloorSubNodeNameValue);
			return Convert.ToInt32(strRlt);
		}
		catch
		{
			return 0;
		}
	}

	public static bool DataGridViewToXml(DataGridView dgvSrc, string strXmlDestFileName)
	{
		try
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlDeclaration xdl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			xmlDoc.PrependChild(xdl);
			for (int iRowIndex = 0; iRowIndex < dgvSrc.Rows.Count; iRowIndex++)
			{
				XmlNode xmlRoot = null;
				xmlRoot = xmlDoc.CreateElement("Row" + (iRowIndex + 1));
				xmlDoc.AppendChild(xmlRoot);
				for (int iColumnIndex = 0; iColumnIndex < dgvSrc.Columns.Count; iColumnIndex++)
				{
					XmlElement xeSubNode = xmlDoc.CreateElement(dgvSrc.Columns[iColumnIndex].HeaderText);
					xeSubNode.InnerText = dgvSrc.Rows[iRowIndex].Cells[iColumnIndex].Value.ToString();
					xmlRoot.AppendChild(xeSubNode);
				}
			}
			xmlDoc.Save(strXmlDestFileName);
			return false;
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("DataGridViewToXml exception:" + ex.Message);
			return false;
		}
	}

	public static bool Ini2Xml(string strIniPath, string strXmlPath)
	{
		XmlDocument xdoc;
		XmlNode root;
		try
		{
			xdoc = new XmlDocument();
			XmlDeclaration xdl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			xdoc.PrependChild(xdl);
			root = xdoc.CreateElement("Msg");
			xdoc.AppendChild(root);
		}
		catch
		{
			throw;
		}
		try
		{
			FileStream fs = new FileStream(strIniPath, FileMode.Open);
			try
			{
				StreamReader sr = new StreamReader(fs, Encoding.Default, detectEncodingFromByteOrderMarks: false);
				sr.BaseStream.Seek(0L, SeekOrigin.Begin);
				XmlNode Header = null;
				string s = sr.ReadLine();
				while (s != null)
				{
					s = s.Trim();
					if (s.Length > 0 && s.Substring(0, 1) == "[" && s.Substring(s.Length - 1, 1) == "]")
					{
						Header = xdoc.CreateElement(s.Substring(1, s.Length - 2));
						root.AppendChild(Header);
					}
					else
					{
						if (Header == null)
						{
							s = sr.ReadLine();
							continue;
						}
						if (s == "")
						{
							s = sr.ReadLine();
							continue;
						}
						string[] ss = null;
						StringBuilder sb = new StringBuilder(s);
						for (int i = 0; i < sb.Length; i++)
						{
							if (sb[i] == '=')
							{
								ss = new string[2]
								{
									s.Substring(0, i),
									s.Substring(i + 1)
								};
								break;
							}
						}
						if (ss == null || ss.Length < 2)
						{
							s = sr.ReadLine();
							continue;
						}
						XmlNode xn = ((XmlDocument)root.ParentNode).CreateElement("MsgRow");
						Header.AppendChild(xn);
						XmlNode xn2 = ((XmlDocument)root.ParentNode).CreateElement("Key");
						xn2.InnerText = ss[0];
						xn.AppendChild(xn2);
						XmlNode xn3 = ((XmlDocument)root.ParentNode).CreateElement("Value");
						xn3.InnerText = ss[1];
						xn.AppendChild(xn3);
					}
					s = sr.ReadLine();
				}
				xdoc.Save(strXmlPath);
				sr.Close();
				fs.Close();
				return true;
			}
			catch
			{
				fs.Close();
				return false;
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Ini2Xml exception:" + ex.Message);
			return false;
		}
	}

	public static bool Xml2Ini(string strXmlPath, string strIniPath)
	{
		XmlNode root;
		try
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(strXmlPath);
			root = xdoc.SelectSingleNode("Msg");
		}
		catch
		{
			throw;
		}
		try
		{
			FileStream fs = new FileStream(strIniPath, FileMode.Create);
			try
			{
				StreamWriter sw = new StreamWriter(fs, Encoding.Default);
				foreach (XmlNode xnHeader in root.ChildNodes)
				{
					sw.WriteLine("[" + xnHeader.Name + "]");
					foreach (XmlNode xnRow in xnHeader.ChildNodes)
					{
						XmlNode xnKey = xnRow["Key"];
						XmlNode xnValue = xnRow["Value"];
						string sDetail = ((xnKey == null) ? "" : xnKey.InnerText) + "=" + ((xnValue == null) ? "" : xnValue.InnerText);
						sw.WriteLine(sDetail);
					}
					sw.WriteLine("");
				}
				sw.Flush();
				sw.Close();
				fs.Close();
				return true;
			}
			catch
			{
				fs.Close();
				return false;
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("exml2ini exception:" + ex.Message);
			return false;
		}
	}

	public static void WriteValue(string strRootNodeName, string strSubNodeName, string strSubNodeValue)
	{
		XmlDocument xmlDoc = new XmlDocument();
		XmlNode xmlRoot = null;
		string strXmlPath = getXmlCurPath();
		if (!File.Exists(strXmlPath))
		{
			XmlDeclaration xdl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			xmlDoc.PrependChild(xdl);
			xmlRoot = xmlDoc.CreateElement(strRootNodeName);
			xmlDoc.AppendChild(xmlRoot);
			xmlDoc.Save(strXmlPath);
		}
		xmlDoc.Load(strXmlPath);
		xmlRoot = xmlDoc.SelectSingleNode(strRootNodeName);
		if (xmlRoot == null)
		{
			xmlRoot = xmlDoc.CreateElement(strRootNodeName);
			xmlDoc.AppendChild(xmlRoot);
		}
		XmlElement xe1 = xmlDoc.CreateElement(strSubNodeName);
		xe1.SetAttribute("Time", DateTime.Now.ToString());
		xe1.InnerText = strSubNodeValue;
		xmlRoot.AppendChild(xe1);
		xmlDoc.Save(strXmlPath);
	}
}
