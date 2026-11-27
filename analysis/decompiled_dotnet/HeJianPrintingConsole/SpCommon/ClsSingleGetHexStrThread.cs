using System;
using System.Threading;

namespace SpCommon;

public class ClsSingleGetHexStrThread
{
	private int iStartPos;

	private int iEndPos;

	private int iSortID;

	private byte[] byteArrSrc;

	private string strRlt = "";

	public Thread myt;

	public ClsSingleGetHexStrThread(int iSortID, int iStartPos, int iEndPos, byte[] byteArrSrc)
	{
		this.iSortID = iSortID;
		this.iStartPos = iStartPos;
		this.iEndPos = iEndPos;
		this.byteArrSrc = byteArrSrc;
		myt = new Thread(ConvertToHexStr);
		myt.Priority = ThreadPriority.Normal;
		myt.Start();
	}

	public void ConvertToHexStr()
	{
		for (int iIndex = iStartPos; iIndex < iEndPos; iIndex++)
		{
			string strTmp = Convert.ToString(byteArrSrc[iIndex], 16);
			if (strTmp.Length < 2)
			{
				strTmp = "0" + strTmp;
			}
			strRlt += strTmp;
		}
		ClsGetHexStrFromBytesOnMultiThread.m_strArrForSaveOneSectionStr[iSortID] = strRlt;
	}
}
