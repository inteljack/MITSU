using System.Threading;

namespace SpCommon;

public class ClsGetHexStrFromBytesOnMultiThread
{
	private int m_ThreadCount = 20;

	public ClsSingleGetHexStrThread[] m_threadArr;

	public static string[] m_strArrForSaveOneSectionStr;

	public string m_strRlt = "";

	public ClsGetHexStrFromBytesOnMultiThread(byte[] byteArrSrc)
	{
		m_threadArr = new ClsSingleGetHexStrThread[m_ThreadCount];
		m_strArrForSaveOneSectionStr = new string[m_ThreadCount];
		int iTheroyLenPerSection = byteArrSrc.Length / m_threadArr.Length;
		for (int iThreadIndex = 0; iThreadIndex < m_threadArr.Length; iThreadIndex++)
		{
			int iStartPos = iTheroyLenPerSection * iThreadIndex;
			int iEndPos = ((iThreadIndex != m_threadArr.Length - 1) ? (iTheroyLenPerSection * (iThreadIndex + 1)) : byteArrSrc.Length);
			m_threadArr[iThreadIndex] = new ClsSingleGetHexStrThread(iThreadIndex, iStartPos, iEndPos, byteArrSrc);
		}
		bool exits;
		do
		{
			Thread.Sleep(200);
			exits = true;
			for (int i = 0; i < m_ThreadCount; i++)
			{
				if (m_threadArr[i].myt.IsAlive)
				{
					exits = false;
					break;
				}
			}
		}
		while (!exits);
		for (int j = 0; j < m_ThreadCount; j++)
		{
			m_strRlt += m_strArrForSaveOneSectionStr[j];
		}
	}
}
