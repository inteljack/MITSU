using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmGroupMsg : frmBase
{
	private IContainer components;

	private Button btnSelDestModelPath;

	private TextBox textBoxDestModelPath;

	private Label lblDestMsg1;

	private CheckBox chkBoxSameMsgSrc;

	private CheckBox chkBoxHeightOffset;

	private CheckBox chkBoxWidthOffset;

	private TextBox textBoxHeightSpace;

	private Label lblHeightSpace;

	private TextBox textBoxWidthSpace;

	private Label lblWidthSpace;

	private Button btnMsg1Path;

	private TextBox textBoxMsg1Path;

	private Label lblSrcMsg1;

	private Button btnMsg2Path;

	private TextBox textBoxMsg2Path;

	private Label lblSrcMsg2;

	private Button btnGroupMsg;

	private Label label2;

	private Label label1;

	private Label label5;

	private TextBox textBoxMaxSinglePrintHeadHeight;

	private Label label4;

	private TextBox textBoxMaxPrintHeadCount;

	private Label label3;

	public frmGroupMsg()
	{
		InitializeComponent();
	}

	private void frmGroupMsg_Load(object sender, EventArgs e)
	{
	}

	private void btnMsg1Path_Click(object sender, EventArgs e)
	{
		string strGetFileName = "";
		if (ClsFile.GetOpenDlgFileName("Select Msg", "All Files(*.*)|*.*|Msg(*.ms)|*.ms", 2, Application.StartupPath, ref strGetFileName))
		{
			if ((Button)sender == btnMsg1Path)
			{
				textBoxMsg1Path.Text = strGetFileName;
			}
			else if ((Button)sender == btnMsg2Path)
			{
				textBoxMsg2Path.Text = strGetFileName;
			}
		}
	}

	private void btnGroupMsg_Click(object sender, EventArgs e)
	{
		MemoryStream msDestMsgInfo = new MemoryStream();
		MemoryStream msDestMsgObj = new MemoryStream();
		MemoryStream msDestMsgDataSrc = new MemoryStream();
		List<string> ListStrMsgPath = new List<string>();
		if (textBoxMsg1Path.Text.Length <= 0)
		{
			MessageBox.Show("正嘜模板路徑1錯誤");
			return;
		}
		if (textBoxMsg2Path.Text.Length <= 0)
		{
			MessageBox.Show("側嘜模板路徑2錯誤");
			return;
		}
		ListStrMsgPath.Add(textBoxMsg1Path.Text);
		ListStrMsgPath.Add(textBoxMsg2Path.Text);
		string strDestMsgTmp = Application.StartupPath + "\\Temp.ms";
		if (SeparateMsgList(ListStrMsgPath, msDestMsgInfo, msDestMsgObj, msDestMsgDataSrc, bSameMsgSrc: false))
		{
			List<double> ListDWidthSpace = new List<double>();
			List<double> ListDHeightSpace = new List<double>();
			ListDWidthSpace.Add(Convert.ToDouble(textBoxWidthSpace.Text));
			ListDHeightSpace.Add(0.0);
			ClsFile.DelExistFile(strDestMsgTmp);
			if (GroupMsg(strDestMsgTmp, msDestMsgInfo, msDestMsgObj, msDestMsgDataSrc, ListDWidthSpace, ListDHeightSpace, bWidthOffset: true, bHeightOffset: false))
			{
				if (!CheckDataHeight(strDestMsgTmp, Convert.ToInt32(textBoxMaxPrintHeadCount.Text), Convert.ToInt32(textBoxMaxSinglePrintHeadHeight.Text)) && DialogResult.No == MessageBox.Show("模板高度過大，是否繼續？", "系統提示", MessageBoxButtons.YesNo))
				{
					return;
				}
				ResetDataHeight(strDestMsgTmp, Convert.ToInt32(textBoxMaxPrintHeadCount.Text), Convert.ToInt32(textBoxMaxSinglePrintHeadHeight.Text));
				ClsLog.LogWrite("合成模板AB成功");
			}
			else
			{
				ClsLog.LogWrite("合成模板AB失敗");
			}
		}
		else
		{
			ClsLog.LogWrite("合成模板AB失敗");
			ClsLog.LogWrite("Separate msg fail to cause Group msg fail.");
		}
		msDestMsgInfo = new MemoryStream();
		msDestMsgObj = new MemoryStream();
		msDestMsgDataSrc = new MemoryStream();
		ListStrMsgPath = new List<string>();
		ListStrMsgPath.Add(strDestMsgTmp);
		ListStrMsgPath.Add(strDestMsgTmp);
		if (SeparateMsgList(ListStrMsgPath, msDestMsgInfo, msDestMsgObj, msDestMsgDataSrc, bSameMsgSrc: true))
		{
			List<double> ListDWidthSpace2 = new List<double>();
			List<double> ListDHeightSpace2 = new List<double>();
			ListDWidthSpace2.Add(0.0);
			ListDHeightSpace2.Add(Convert.ToDouble(textBoxHeightSpace.Text));
			string strDestMsg = textBoxDestModelPath.Text;
			if (strDestMsg.Length <= 0)
			{
				MessageBox.Show("目標模板路徑錯誤");
			}
			else if (GroupMsg(strDestMsg, msDestMsgInfo, msDestMsgObj, msDestMsgDataSrc, ListDWidthSpace2, ListDHeightSpace2, bWidthOffset: false, bHeightOffset: true))
			{
				MessageBox.Show("合成模板成功");
			}
			else
			{
				MessageBox.Show("合成模板失敗");
			}
		}
		else
		{
			MessageBox.Show("合成模板失敗");
			ClsLog.LogWrite("Separate msg fail to cause Group msg fail.");
		}
	}

	private bool CheckDataHeight(string strMsgSrc, int iMaxPrintHeadCount, int iMaxSinglePrintHeadHeight)
	{
		StringBuilder StrBuiTemp = new StringBuilder(255);
		ClsIni.GetPrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgCommonKey.strHeight, "", StrBuiTemp, 255, strMsgSrc);
		try
		{
			int iHeightTmp = Convert.ToInt32(StrBuiTemp.ToString());
			int iMaxHeight = iMaxPrintHeadCount * iMaxSinglePrintHeadHeight;
			if (iHeightTmp <= iMaxHeight)
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

	private void ResetDataHeight(string strMsgSrc, int iMaxPrintHeadCount, int iMaxSinglePrintHeadHeight)
	{
		try
		{
			ClsIni.WritePrivateProfileString(ClsMsg.clsMsgSection.strMsgInfo, ClsMsg.clsMsgCommonKey.strHeight, (iMaxPrintHeadCount * iMaxSinglePrintHeadHeight).ToString(), strMsgSrc);
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
					if (bWidthOffset)
					{
						clsMVDH.dWidth += dWidthTmp;
					}
					else
					{
						clsMVDH.dWidth = ((clsMVDH.dWidth >= dWidthTmp) ? clsMVDH.dWidth : dWidthTmp);
					}
					ListDWidth.Add(dWidthTmp);
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

	private void btnIniToXml_Click(object sender, EventArgs e)
	{
		string strSrcFileName = "";
		string strSaveFileName = "";
		if (ClsFile.GetOpenDlgFileName("Select file", "All Files(*.*)|*.*|Msg(*.ms)|*.ms", 2, Application.StartupPath, ref strSrcFileName))
		{
			if (ClsFile.GetSaveDlgFileName("Save file", "All Files(*.*)|*.*|Xml(*.xml)|*.xml", 2, Application.StartupPath, ref strSaveFileName))
			{
				if (ClsXml.Ini2Xml(strSrcFileName, strSaveFileName))
				{
					MessageBox.Show("Convert successful!");
				}
				else
				{
					MessageBox.Show("Convert fail!");
				}
			}
			else
			{
				MessageBox.Show("Save path error");
			}
		}
		else
		{
			MessageBox.Show("Select file path error");
		}
	}

	private void btnXmlToIni_Click(object sender, EventArgs e)
	{
		string strSrcFileName = "";
		string strSaveFileName = "";
		if (ClsFile.GetOpenDlgFileName("Select file", "All Files(*.*)|*.*|Xml(*.xml)|*.xml", 2, Application.StartupPath, ref strSrcFileName))
		{
			if (ClsFile.GetSaveDlgFileName("Save file", "All Files(*.*)|*.*|Ini(*.ini)|*.ini|Msg(*.ms)|*.ms", 3, Application.StartupPath, ref strSaveFileName))
			{
				if (ClsXml.Xml2Ini(strSrcFileName, strSaveFileName))
				{
					MessageBox.Show("Convert successful!");
				}
				else
				{
					MessageBox.Show("Convert fail!");
				}
			}
			else
			{
				MessageBox.Show("Save path error");
			}
		}
		else
		{
			MessageBox.Show("Select file path error");
		}
	}

	private void btnSelDestModelPath_Click(object sender, EventArgs e)
	{
		string strSaveFielName = "";
		if (!ClsFile.GetSaveDlgFileName("Save msg", "All Files(*.*)|*.*|Msg(*.ms)|*.ms", 2, Application.StartupPath, ref strSaveFielName))
		{
			MessageBox.Show("Save file name error");
		}
		else
		{
			textBoxDestModelPath.Text = strSaveFielName;
		}
	}

	private void chkBoxSameMsgSrc_CheckedChanged(object sender, EventArgs e)
	{
		if ((CheckBox)sender == chkBoxSameMsgSrc)
		{
			if (((CheckBox)sender).Checked)
			{
				btnMsg2Path.Enabled = false;
			}
			else
			{
				btnMsg2Path.Enabled = true;
			}
		}
		else if ((CheckBox)sender == chkBoxWidthOffset)
		{
			if (((CheckBox)sender).Checked)
			{
				textBoxWidthSpace.ReadOnly = false;
			}
			else
			{
				textBoxWidthSpace.ReadOnly = true;
			}
		}
		else if ((CheckBox)sender == chkBoxHeightOffset)
		{
			if (((CheckBox)sender).Checked)
			{
				textBoxHeightSpace.ReadOnly = false;
			}
			else
			{
				textBoxHeightSpace.ReadOnly = true;
			}
		}
	}

	private void textBoxWidthSpace_KeyPress(object sender, KeyPressEventArgs e)
	{
		try
		{
			if (!ClsShare.OnlyInputNum(e.KeyChar, bHasSign: true))
			{
				e.Handled = true;
			}
		}
		catch
		{
		}
	}

	private void textBoxHeightSpace_KeyUp(object sender, KeyEventArgs e)
	{
		try
		{
			string strTmp = ((TextBox)sender).Text;
			if (strTmp.Length > 1 && strTmp.Contains('-'))
			{
				string[] strArrTmp = strTmp.Substring(1).Split('-');
				strTmp = strTmp.Substring(0, 1);
				string[] array = strArrTmp;
				foreach (string strT in array)
				{
					strTmp += strT;
				}
				((TextBox)sender).Text = strTmp;
				e.Handled = true;
			}
		}
		catch
		{
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
		this.btnSelDestModelPath = new System.Windows.Forms.Button();
		this.textBoxDestModelPath = new System.Windows.Forms.TextBox();
		this.lblDestMsg1 = new System.Windows.Forms.Label();
		this.chkBoxSameMsgSrc = new System.Windows.Forms.CheckBox();
		this.chkBoxHeightOffset = new System.Windows.Forms.CheckBox();
		this.chkBoxWidthOffset = new System.Windows.Forms.CheckBox();
		this.textBoxHeightSpace = new System.Windows.Forms.TextBox();
		this.lblHeightSpace = new System.Windows.Forms.Label();
		this.textBoxWidthSpace = new System.Windows.Forms.TextBox();
		this.lblWidthSpace = new System.Windows.Forms.Label();
		this.btnMsg1Path = new System.Windows.Forms.Button();
		this.textBoxMsg1Path = new System.Windows.Forms.TextBox();
		this.lblSrcMsg1 = new System.Windows.Forms.Label();
		this.btnMsg2Path = new System.Windows.Forms.Button();
		this.textBoxMsg2Path = new System.Windows.Forms.TextBox();
		this.lblSrcMsg2 = new System.Windows.Forms.Label();
		this.btnGroupMsg = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.textBoxMaxPrintHeadCount = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.textBoxMaxSinglePrintHeadHeight = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.label5);
		base.pnlClientData.Controls.Add(this.textBoxMaxSinglePrintHeadHeight);
		base.pnlClientData.Controls.Add(this.label4);
		base.pnlClientData.Controls.Add(this.textBoxMaxPrintHeadCount);
		base.pnlClientData.Controls.Add(this.label3);
		base.pnlClientData.Controls.Add(this.label2);
		base.pnlClientData.Controls.Add(this.label1);
		base.pnlClientData.Controls.Add(this.btnSelDestModelPath);
		base.pnlClientData.Controls.Add(this.textBoxDestModelPath);
		base.pnlClientData.Controls.Add(this.lblDestMsg1);
		base.pnlClientData.Controls.Add(this.chkBoxSameMsgSrc);
		base.pnlClientData.Controls.Add(this.chkBoxHeightOffset);
		base.pnlClientData.Controls.Add(this.chkBoxWidthOffset);
		base.pnlClientData.Controls.Add(this.textBoxHeightSpace);
		base.pnlClientData.Controls.Add(this.lblHeightSpace);
		base.pnlClientData.Controls.Add(this.textBoxWidthSpace);
		base.pnlClientData.Controls.Add(this.lblWidthSpace);
		base.pnlClientData.Controls.Add(this.btnMsg1Path);
		base.pnlClientData.Controls.Add(this.textBoxMsg1Path);
		base.pnlClientData.Controls.Add(this.lblSrcMsg1);
		base.pnlClientData.Controls.Add(this.btnMsg2Path);
		base.pnlClientData.Controls.Add(this.textBoxMsg2Path);
		base.pnlClientData.Controls.Add(this.lblSrcMsg2);
		base.pnlBottomCtrl.Controls.Add(this.btnGroupMsg);
		this.btnSelDestModelPath.Location = new System.Drawing.Point(693, 112);
		this.btnSelDestModelPath.Name = "btnSelDestModelPath";
		this.btnSelDestModelPath.Size = new System.Drawing.Size(30, 23);
		this.btnSelDestModelPath.TabIndex = 69;
		this.btnSelDestModelPath.Text = "...";
		this.btnSelDestModelPath.UseVisualStyleBackColor = true;
		this.btnSelDestModelPath.Click += new System.EventHandler(btnSelDestModelPath_Click);
		this.textBoxDestModelPath.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxDestModelPath.Enabled = false;
		this.textBoxDestModelPath.Location = new System.Drawing.Point(119, 112);
		this.textBoxDestModelPath.Name = "textBoxDestModelPath";
		this.textBoxDestModelPath.ReadOnly = true;
		this.textBoxDestModelPath.Size = new System.Drawing.Size(574, 21);
		this.textBoxDestModelPath.TabIndex = 68;
		this.lblDestMsg1.AutoSize = true;
		this.lblDestMsg1.BackColor = System.Drawing.Color.Transparent;
		this.lblDestMsg1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblDestMsg1.Location = new System.Drawing.Point(17, 113);
		this.lblDestMsg1.Name = "lblDestMsg1";
		this.lblDestMsg1.Size = new System.Drawing.Size(75, 14);
		this.lblDestMsg1.TabIndex = 67;
		this.lblDestMsg1.Text = "目標模板:";
		this.chkBoxSameMsgSrc.AutoSize = true;
		this.chkBoxSameMsgSrc.Location = new System.Drawing.Point(750, 27);
		this.chkBoxSameMsgSrc.Name = "chkBoxSameMsgSrc";
		this.chkBoxSameMsgSrc.Size = new System.Drawing.Size(96, 16);
		this.chkBoxSameMsgSrc.TabIndex = 66;
		this.chkBoxSameMsgSrc.Text = "同源模板合成";
		this.chkBoxSameMsgSrc.UseVisualStyleBackColor = true;
		this.chkBoxSameMsgSrc.Visible = false;
		this.chkBoxSameMsgSrc.CheckedChanged += new System.EventHandler(chkBoxSameMsgSrc_CheckedChanged);
		this.chkBoxHeightOffset.AutoSize = true;
		this.chkBoxHeightOffset.Checked = true;
		this.chkBoxHeightOffset.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkBoxHeightOffset.Location = new System.Drawing.Point(749, 192);
		this.chkBoxHeightOffset.Name = "chkBoxHeightOffset";
		this.chkBoxHeightOffset.Size = new System.Drawing.Size(72, 16);
		this.chkBoxHeightOffset.TabIndex = 65;
		this.chkBoxHeightOffset.Text = "縱向合成";
		this.chkBoxHeightOffset.UseVisualStyleBackColor = true;
		this.chkBoxHeightOffset.Visible = false;
		this.chkBoxHeightOffset.CheckedChanged += new System.EventHandler(chkBoxSameMsgSrc_CheckedChanged);
		this.chkBoxWidthOffset.AutoSize = true;
		this.chkBoxWidthOffset.Checked = true;
		this.chkBoxWidthOffset.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkBoxWidthOffset.Location = new System.Drawing.Point(749, 159);
		this.chkBoxWidthOffset.Name = "chkBoxWidthOffset";
		this.chkBoxWidthOffset.Size = new System.Drawing.Size(72, 16);
		this.chkBoxWidthOffset.TabIndex = 64;
		this.chkBoxWidthOffset.Text = "橫向合成";
		this.chkBoxWidthOffset.UseVisualStyleBackColor = true;
		this.chkBoxWidthOffset.Visible = false;
		this.chkBoxWidthOffset.CheckedChanged += new System.EventHandler(chkBoxSameMsgSrc_CheckedChanged);
		this.textBoxHeightSpace.Location = new System.Drawing.Point(119, 190);
		this.textBoxHeightSpace.MaxLength = 8;
		this.textBoxHeightSpace.Name = "textBoxHeightSpace";
		this.textBoxHeightSpace.Size = new System.Drawing.Size(574, 21);
		this.textBoxHeightSpace.TabIndex = 63;
		this.textBoxHeightSpace.Text = "100";
		this.textBoxHeightSpace.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxWidthSpace_KeyPress);
		this.textBoxHeightSpace.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxHeightSpace_KeyUp);
		this.lblHeightSpace.AutoSize = true;
		this.lblHeightSpace.BackColor = System.Drawing.Color.Transparent;
		this.lblHeightSpace.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblHeightSpace.Location = new System.Drawing.Point(18, 193);
		this.lblHeightSpace.Name = "lblHeightSpace";
		this.lblHeightSpace.Size = new System.Drawing.Size(75, 14);
		this.lblHeightSpace.TabIndex = 62;
		this.lblHeightSpace.Text = "縱向間距:";
		this.textBoxWidthSpace.Location = new System.Drawing.Point(119, 157);
		this.textBoxWidthSpace.MaxLength = 8;
		this.textBoxWidthSpace.Name = "textBoxWidthSpace";
		this.textBoxWidthSpace.Size = new System.Drawing.Size(574, 21);
		this.textBoxWidthSpace.TabIndex = 61;
		this.textBoxWidthSpace.Text = "100";
		this.textBoxWidthSpace.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBoxWidthSpace_KeyPress);
		this.textBoxWidthSpace.KeyUp += new System.Windows.Forms.KeyEventHandler(textBoxHeightSpace_KeyUp);
		this.lblWidthSpace.AutoSize = true;
		this.lblWidthSpace.BackColor = System.Drawing.Color.Transparent;
		this.lblWidthSpace.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblWidthSpace.Location = new System.Drawing.Point(18, 160);
		this.lblWidthSpace.Name = "lblWidthSpace";
		this.lblWidthSpace.Size = new System.Drawing.Size(75, 14);
		this.lblWidthSpace.TabIndex = 60;
		this.lblWidthSpace.Text = "橫向間距:";
		this.btnMsg1Path.Location = new System.Drawing.Point(693, 23);
		this.btnMsg1Path.Name = "btnMsg1Path";
		this.btnMsg1Path.Size = new System.Drawing.Size(30, 23);
		this.btnMsg1Path.TabIndex = 59;
		this.btnMsg1Path.Text = "...";
		this.btnMsg1Path.UseVisualStyleBackColor = true;
		this.btnMsg1Path.Click += new System.EventHandler(btnMsg1Path_Click);
		this.textBoxMsg1Path.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxMsg1Path.Enabled = false;
		this.textBoxMsg1Path.Location = new System.Drawing.Point(119, 23);
		this.textBoxMsg1Path.Name = "textBoxMsg1Path";
		this.textBoxMsg1Path.ReadOnly = true;
		this.textBoxMsg1Path.Size = new System.Drawing.Size(574, 21);
		this.textBoxMsg1Path.TabIndex = 58;
		this.lblSrcMsg1.AutoSize = true;
		this.lblSrcMsg1.BackColor = System.Drawing.Color.Transparent;
		this.lblSrcMsg1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSrcMsg1.Location = new System.Drawing.Point(17, 24);
		this.lblSrcMsg1.Name = "lblSrcMsg1";
		this.lblSrcMsg1.Size = new System.Drawing.Size(83, 14);
		this.lblSrcMsg1.TabIndex = 57;
		this.lblSrcMsg1.Text = "正嘜模板:";
		this.btnMsg2Path.Location = new System.Drawing.Point(693, 70);
		this.btnMsg2Path.Name = "btnMsg2Path";
		this.btnMsg2Path.Size = new System.Drawing.Size(30, 23);
		this.btnMsg2Path.TabIndex = 56;
		this.btnMsg2Path.Text = "...";
		this.btnMsg2Path.UseVisualStyleBackColor = true;
		this.btnMsg2Path.Click += new System.EventHandler(btnMsg1Path_Click);
		this.textBoxMsg2Path.BackColor = System.Drawing.SystemColors.Window;
		this.textBoxMsg2Path.Enabled = false;
		this.textBoxMsg2Path.Location = new System.Drawing.Point(119, 70);
		this.textBoxMsg2Path.Name = "textBoxMsg2Path";
		this.textBoxMsg2Path.ReadOnly = true;
		this.textBoxMsg2Path.Size = new System.Drawing.Size(574, 21);
		this.textBoxMsg2Path.TabIndex = 55;
		this.lblSrcMsg2.AutoSize = true;
		this.lblSrcMsg2.BackColor = System.Drawing.Color.Transparent;
		this.lblSrcMsg2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSrcMsg2.Location = new System.Drawing.Point(17, 71);
		this.lblSrcMsg2.Name = "lblSrcMsg2";
		this.lblSrcMsg2.Size = new System.Drawing.Size(83, 14);
		this.lblSrcMsg2.TabIndex = 54;
		this.lblSrcMsg2.Text = "側嘜模板:";
		this.btnGroupMsg.Location = new System.Drawing.Point(129, 32);
		this.btnGroupMsg.Name = "btnGroupMsg";
		this.btnGroupMsg.Size = new System.Drawing.Size(123, 48);
		this.btnGroupMsg.TabIndex = 35;
		this.btnGroupMsg.Text = "合成模板";
		this.btnGroupMsg.UseVisualStyleBackColor = true;
		this.btnGroupMsg.Click += new System.EventHandler(btnGroupMsg_Click);
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.Transparent;
		this.label1.Font = new System.Drawing.Font("宋体", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label1.Location = new System.Drawing.Point(696, 160);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(42, 16);
		this.label1.TabIndex = 70;
		this.label1.Text = "像素";
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.Transparent;
		this.label2.Font = new System.Drawing.Font("宋体", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label2.Location = new System.Drawing.Point(696, 193);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(42, 16);
		this.label2.TabIndex = 71;
		this.label2.Text = "像素";
		this.textBoxMaxPrintHeadCount.Location = new System.Drawing.Point(221, 231);
		this.textBoxMaxPrintHeadCount.MaxLength = 8;
		this.textBoxMaxPrintHeadCount.Name = "textBoxMaxPrintHeadCount";
		this.textBoxMaxPrintHeadCount.Size = new System.Drawing.Size(77, 21);
		this.textBoxMaxPrintHeadCount.TabIndex = 73;
		this.textBoxMaxPrintHeadCount.Text = "8";
		this.textBoxMaxPrintHeadCount.Visible = false;
		this.label3.AutoSize = true;
		this.label3.BackColor = System.Drawing.Color.Transparent;
		this.label3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label3.Location = new System.Drawing.Point(18, 235);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(202, 14);
		this.label3.TabIndex = 72;
		this.label3.Text = "單面模板所需的最大噴頭數：";
		this.label3.Visible = false;
		this.textBoxMaxSinglePrintHeadHeight.Location = new System.Drawing.Point(221, 267);
		this.textBoxMaxSinglePrintHeadHeight.MaxLength = 8;
		this.textBoxMaxSinglePrintHeadHeight.Name = "textBoxMaxSinglePrintHeadHeight";
		this.textBoxMaxSinglePrintHeadHeight.Size = new System.Drawing.Size(77, 21);
		this.textBoxMaxSinglePrintHeadHeight.TabIndex = 75;
		this.textBoxMaxSinglePrintHeadHeight.Text = "150";
		this.textBoxMaxSinglePrintHeadHeight.Visible = false;
		this.label4.AutoSize = true;
		this.label4.BackColor = System.Drawing.Color.Transparent;
		this.label4.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label4.Location = new System.Drawing.Point(18, 270);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(150, 14);
		this.label4.TabIndex = 74;
		this.label4.Text = "單個噴頭的最大高度:";
		this.label4.Visible = false;
		this.label5.AutoSize = true;
		this.label5.BackColor = System.Drawing.Color.Transparent;
		this.label5.Font = new System.Drawing.Font("宋体", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label5.Location = new System.Drawing.Point(304, 268);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(42, 16);
		this.label5.TabIndex = 76;
		this.label5.Text = "像素";
		this.label5.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(919, 449);
		base.Name = "frmGroupMsg";
		base.Load += new System.EventHandler(frmGroupMsg_Load);
		base.pnlClientData.ResumeLayout(false);
		base.pnlClientData.PerformLayout();
		base.pnlBottomCtrl.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
