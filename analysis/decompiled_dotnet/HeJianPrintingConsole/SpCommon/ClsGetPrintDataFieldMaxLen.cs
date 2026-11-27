using System;

namespace SpCommon;

public class ClsGetPrintDataFieldMaxLen
{
	public int iLen_Bore
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Bore, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_BoreUnit
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_BoreUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_BoreValue
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_BoreValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_CaseNo
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_CaseNo, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_Code
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Code, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_CodeValue
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_CodeValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_HzUnit
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_HzUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_HzValue
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_HzValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_Keycode
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_KeyCode, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_Model
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Model, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_ModelValue
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_ModelValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_Output
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Output, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_OutputUnit
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_OutputUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_OutputValue
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_OutputValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_PhaseUnit
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_PhaseUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_PhaseValue
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_PhaseValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_Place
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Place, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_Tokki
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_Tokki, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_VoltageUnit
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_VoltageUnit, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}

	public int iLen_VoltageValue
	{
		get
		{
			try
			{
				string strRlt = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPrintDataLen_VoltageValue, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
				return Convert.ToInt32(strRlt);
			}
			catch
			{
				return 1;
			}
		}
	}
}
