using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SpCommon;

internal class ClsIni
{
	[DllImport("kernel32")]
	public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

	[DllImport("kernel32")]
	public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

	[DllImport("kernel32")]
	public static extern int GetPrivateProfileString(string section, string key, string def, byte[] byteArrValue, int size, string filePath);

	public static string GetValue(string strSrcFilePath, string strSection, string strKey, int iValueLen = 255, string strDefaultValue = "0")
	{
		try
		{
			string strDest = "";
			StringBuilder StrBuiTemp = new StringBuilder(iValueLen);
			GetPrivateProfileString(strSection, strKey, strDefaultValue, StrBuiTemp, iValueLen, strSrcFilePath);
			return StrBuiTemp.ToString();
		}
		catch
		{
			return strDefaultValue;
		}
	}

	public static string GetValue2(string strSrcFilePath, string strSection, string strKey, int iValueLen = 255, string strDefaultValue = "0")
	{
		try
		{
			string strDest = "";
			byte[] byteArrTmp = new byte[iValueLen];
			GetPrivateProfileString(strSection, strKey, strDefaultValue, byteArrTmp, iValueLen, strSrcFilePath);
			return Encoding.Default.GetString(byteArrTmp);
		}
		catch
		{
			return strDefaultValue;
		}
	}

	public static string GetValue(string strSrcFilePath, string strSection, string strKey, Encoding EncodeStyle, int iValueLen = 255, string strDefaultValue = "0")
	{
		try
		{
			string strDest = "";
			bool bReadValue = false;
			FileStream fsSrc = new FileStream(strSrcFilePath, FileMode.Open, FileAccess.Read);
			StreamReader srSrc = new StreamReader(fsSrc, EncodeStyle);
			for (string strLine = srSrc.ReadLine(); strLine != null; strLine = srSrc.ReadLine())
			{
				if (strLine.Trim().ToUpper() == ("[" + strSection + "]").Trim().ToUpper())
				{
					bReadValue = true;
				}
				if (bReadValue)
				{
					string strKeyTmp = "";
					string strValueTmp = "";
					SeparateKeyValue(strLine, ref strKeyTmp, ref strValueTmp);
					if (strKeyTmp.Trim().ToUpper() == strKey.Trim().ToUpper())
					{
						strDest = strValueTmp;
						break;
					}
				}
			}
			srSrc.Close();
			fsSrc.Close();
			return strDest;
		}
		catch
		{
			return strDefaultValue;
		}
	}

	private static void SeparateKeyValue(string strSrc, ref string strKey, ref string strValue)
	{
		int iSplit = strSrc.IndexOf("=");
		if (iSplit >= 0)
		{
			strKey = strSrc.Substring(0, iSplit);
			strValue = strSrc.Substring(iSplit + 1, strSrc.Length - (iSplit + 1));
		}
	}
}
