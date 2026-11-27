using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SpCommon;

internal class ClsMusic
{
	public enum EnumMusicTimeMode
	{
		Days = 1,
		Hours,
		Minutes,
		Seconds,
		Milliseconds
	}

	private static Thread m_ThreadCtrlPlayMusicDuration = null;

	public static SoundPlayer m_sp = null;

	public static string m_strNormalMusicPath = "";

	public static string m_strWarmMusicPath = "";

	public static int m_iPlayDuration = 0;

	public static int m_iPlayNormalMusicDuration = 0;

	public static int m_iPlayWarmMusicDuration = 0;

	public static int m_iPlayOneMusicTotalTime = 0;

	public static int m_iPlayOneNormalMusicTotalTime = 0;

	public static int m_iPlayOneWarmMusicTotalTime = 0;

	public static MemoryStream m_msNormalMusic = null;

	public static MemoryStream m_msWarmMusic = null;

	public static MemoryStream m_msMusic = null;

	public static void IniMusicParam()
	{
		try
		{
			m_strNormalMusicPath = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSystemMusicNormalFileName, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
			m_strWarmMusicPath = ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSystemMusicWarmFileName, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue);
			m_iPlayNormalMusicDuration = ClsXml.GetNormalMusicDuration();
			m_iPlayWarmMusicDuration = ClsXml.GetWarmMusicDuration();
			m_iPlayOneNormalMusicTotalTime = Convert.ToInt32(ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSystemMusicNormalTotalTime, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue));
			m_iPlayOneWarmMusicTotalTime = Convert.ToInt32(ClsXml.getParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strSystemMusicWarmTotalTime, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue));
			if (File.Exists(m_strNormalMusicPath))
			{
				m_msNormalMusic = new MemoryStream();
				m_msNormalMusic = ClsShare.FileStreamToMemStream(m_strNormalMusicPath);
			}
			if (File.Exists(m_strWarmMusicPath))
			{
				m_msWarmMusic = new MemoryStream();
				m_msWarmMusic = ClsShare.FileStreamToMemStream(m_strWarmMusicPath);
			}
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Initializ music param exception:" + ex.Message);
		}
	}

	public static void PlayMusic_Normal()
	{
		m_iPlayDuration = m_iPlayNormalMusicDuration;
		m_iPlayOneMusicTotalTime = m_iPlayOneNormalMusicTotalTime;
		if (m_iPlayDuration > 0)
		{
			m_msMusic = new MemoryStream();
			m_msMusic = m_msNormalMusic;
			ThreadCtrlPlayMusicDurationStart();
		}
	}

	public static void PlayMusic_Warm()
	{
		m_iPlayDuration = m_iPlayWarmMusicDuration;
		m_iPlayOneMusicTotalTime = m_iPlayOneWarmMusicTotalTime;
		if (m_iPlayDuration > 0)
		{
			m_msMusic = new MemoryStream();
			m_msMusic = m_msWarmMusic;
			ThreadCtrlPlayMusicDurationStart();
		}
	}

	public static void PlayMusic(string strMusicFilePathSrc)
	{
		FileStream fsMusic = File.OpenRead(strMusicFilePathSrc);
		m_sp = new SoundPlayer(strMusicFilePathSrc);
		m_sp.Stream = fsMusic;
		m_sp.Play();
		if (m_iPlayOneMusicTotalTime > 0)
		{
			ThreadCtrlPlayMusicDurationStart();
			m_sp.PlayLooping();
		}
		else
		{
			Thread.Sleep(500);
		}
		fsMusic.Close();
		m_sp.Stop();
	}

	public static void PlayMusic(Stream streamSrc)
	{
	}

	public static void StopMusic()
	{
		try
		{
			m_ThreadCtrlPlayMusicDuration.Abort();
			m_ThreadCtrlPlayMusicDuration = null;
			m_sp.Stop();
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("提示音停止播放异常：" + ex.Message);
		}
	}

	private static void ThreadCtrlPlayMusicDurationStart()
	{
		m_ThreadCtrlPlayMusicDuration = new Thread(ListenPlayMusicDuration);
		m_ThreadCtrlPlayMusicDuration.IsBackground = true;
		m_ThreadCtrlPlayMusicDuration.Start();
	}

	private static void ListenPlayMusicDuration()
	{
		try
		{
			int iRecordTime = 0;
			if (m_msMusic == null)
			{
				return;
			}
			m_sp = new SoundPlayer();
			m_msMusic.Position = 0L;
			m_sp.Stream = m_msMusic;
			if (m_iPlayOneMusicTotalTime <= 0)
			{
				m_iPlayOneMusicTotalTime = 1;
			}
			int iLoopPlayCount = (m_iPlayDuration + (m_iPlayOneMusicTotalTime - 1)) / m_iPlayOneMusicTotalTime;
			while (true)
			{
				Thread.Sleep(1000);
				iRecordTime++;
				if (iRecordTime >= m_iPlayDuration)
				{
					break;
				}
				if (iRecordTime % m_iPlayOneMusicTotalTime == 0 && iRecordTime / m_iPlayOneMusicTotalTime < iLoopPlayCount)
				{
					m_sp.Stop();
					m_sp.Play();
				}
			}
			m_sp.Stop();
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("Loop play music:" + ex.Message);
		}
	}

	[DllImport("Kernel32", CharSet = CharSet.Auto)]
	private static extern int GetShortPathName(string path, StringBuilder shortPath, int shortPathLength);

	[DllImport("winmm.dll")]
	public static extern int mciSendString(string m_strCmd, StringBuilder m_strReceive, int m_v1, int m_v2);

	public static TimeSpan GetMusicTotalTime(string filePath)
	{
		try
		{
			StringBuilder shortpath = new StringBuilder(80);
			GetShortPathName(filePath, shortpath, shortpath.Capacity);
			string name = shortpath.ToString();
			StringBuilder buf = new StringBuilder(80);
			mciSendString("close all", buf, buf.Capacity, 0);
			mciSendString("open " + name + " alias media", buf, buf.Capacity, 0);
			mciSendString("status media length", buf, buf.Capacity, 0);
			return new TimeSpan(0, 0, 0, 0, (int)Convert.ToDouble(buf.ToString().Trim()));
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite(ex.Message);
			return default(TimeSpan);
		}
	}

	public static double GetMusicTotalTime(string filePath, int iMode)
	{
		TimeSpan tsRlt = GetMusicTotalTime(filePath);
		double dRlt = 0.0;
		switch (iMode)
		{
		case 1:
			dRlt = (double)tsRlt.Days + (double)tsRlt.Hours / 24.0 + (double)tsRlt.Minutes / 24.0 / 60.0 + (double)tsRlt.Seconds / 24.0 / 60.0 / 60.0 + (double)tsRlt.Milliseconds / 24.0 / 60.0 / 60.0 / 1000.0;
			break;
		case 2:
			dRlt = (double)tsRlt.Days * 24.0 + (double)tsRlt.Hours + (double)tsRlt.Minutes / 60.0 + (double)tsRlt.Seconds / 60.0 / 60.0 + (double)tsRlt.Milliseconds / 1000.0 / 60.0 / 60.0;
			break;
		case 3:
			dRlt = (double)tsRlt.Days * 24.0 * 60.0 + (double)tsRlt.Hours * 60.0 + (double)tsRlt.Minutes + (double)tsRlt.Seconds / 60.0 + (double)tsRlt.Milliseconds / 1000.0 / 60.0;
			break;
		case 4:
			dRlt = (double)tsRlt.Days * 24.0 * 60.0 * 60.0 + (double)tsRlt.Hours * 60.0 * 60.0 + (double)tsRlt.Minutes * 60.0 + (double)tsRlt.Seconds + (double)tsRlt.Milliseconds / 1000.0;
			break;
		case 5:
			dRlt = (double)tsRlt.Days * 24.0 * 60.0 * 60.0 * 1000.0 + (double)tsRlt.Hours * 60.0 * 60.0 * 1000.0 + (double)tsRlt.Minutes * 60.0 * 1000.0 + (double)tsRlt.Seconds * 1000.0 + (double)tsRlt.Milliseconds;
			break;
		}
		return dRlt;
	}
}
