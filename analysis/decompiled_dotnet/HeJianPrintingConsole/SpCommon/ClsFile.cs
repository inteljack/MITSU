using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SpCommon;

internal class ClsFile
{
	public static bool GetOpenDlgFileName(string strTitle, string strFilter, int iFilterIndex, string strIniDirectory, ref string strFileName)
	{
		OpenFileDialog ofdBmp = new OpenFileDialog();
		ofdBmp.Title = strTitle;
		ofdBmp.Filter = strFilter;
		ofdBmp.FilterIndex = iFilterIndex;
		ofdBmp.InitialDirectory = strIniDirectory;
		if (DialogResult.OK == ofdBmp.ShowDialog())
		{
			strFileName = ofdBmp.FileName;
			return true;
		}
		strFileName = "";
		return false;
	}

	public static bool GetSaveDlgFileName(string strTitle, string strFilter, int iFilterIndex, string strIniDirectory, ref string strFileName)
	{
		SaveFileDialog sfd = new SaveFileDialog();
		sfd.Title = strTitle;
		sfd.Filter = strFilter;
		sfd.FilterIndex = iFilterIndex;
		sfd.InitialDirectory = strIniDirectory;
		if (DialogResult.OK == sfd.ShowDialog())
		{
			strFileName = sfd.FileName;
			return true;
		}
		strFileName = "";
		return false;
	}

	public static DialogResult GetFolderPath(ref string strSelPath)
	{
		FolderBrowserDialog fbd = new FolderBrowserDialog();
		DialogResult DlgRlt = fbd.ShowDialog();
		strSelPath = fbd.SelectedPath;
		return DlgRlt;
	}

	public static bool DelExistFile(string strSrc)
	{
		try
		{
			bool bRlt = true;
			if (File.Exists(strSrc))
			{
				File.Delete(strSrc);
				if (File.Exists(strSrc))
				{
					bRlt = false;
				}
			}
			return bRlt;
		}
		catch
		{
			return false;
		}
	}

	public static MemoryStream FileStreamToMemStream(string strFileNameSrc)
	{
		FileStream fsSrc = new FileStream(strFileNameSrc, FileMode.Open, FileAccess.Read, FileShare.Read);
		MemoryStream msDest = new MemoryStream();
		msDest.SetLength(fsSrc.Length - fsSrc.Position);
		fsSrc.Read(msDest.GetBuffer(), 0, (int)(fsSrc.Length - fsSrc.Position));
		return msDest;
	}

	public static string ReadAppointLineFromFile(string strFilePathSrc, int iLineNo)
	{
		string strRlt = "";
		FileStream fsSrc = File.OpenRead(strFilePathSrc);
		StreamReader srSrc = new StreamReader(fsSrc, Encoding.Default);
		string strLine = srSrc.ReadLine();
		int iLineNoTmp = 0;
		while (strLine != null && iLineNoTmp != iLineNo)
		{
			strLine = srSrc.ReadLine();
			iLineNoTmp++;
		}
		return strLine;
	}

	public static string ReadAllLineFromFile(string strFilePathSrc)
	{
		string strRlt = "";
		FileStream fsSrc = File.OpenRead(strFilePathSrc);
		StreamReader srSrc = new StreamReader(fsSrc, Encoding.Default);
		for (string strLine = srSrc.ReadLine(); strLine != null; strLine = srSrc.ReadLine())
		{
			strRlt += strLine;
		}
		return strRlt;
	}

	public static void ClearSingleFolder(string strFolderPath)
	{
		string[] files = Directory.GetFiles(strFolderPath);
		foreach (string str in files)
		{
			FileInfo info = new FileInfo(str);
			info.Attributes = FileAttributes.Normal;
			info.Delete();
		}
	}

	public static void ClearWholeFolder(string strFolderPath, bool bClearRootFolder)
	{
		DirectoryInfo theFolder = new DirectoryInfo(strFolderPath);
		if (bClearRootFolder)
		{
			theFolder.Delete(recursive: true);
			return;
		}
		DirectoryInfo[] dirInfo = theFolder.GetDirectories();
		DirectoryInfo[] array = dirInfo;
		foreach (DirectoryInfo NextFolder in array)
		{
			NextFolder.Delete(recursive: true);
		}
		ClearSingleFolder(strFolderPath);
	}

	public static List<DirectoryInfo> GetAllFolder(string strFolderPath)
	{
		try
		{
			List<DirectoryInfo> ListFolderDirInfo = new List<DirectoryInfo>();
			DirectoryInfo theFolder = new DirectoryInfo(strFolderPath);
			DirectoryInfo[] dirInfo = theFolder.GetDirectories();
			DirectoryInfo[] array = dirInfo;
			foreach (DirectoryInfo NextFolder in array)
			{
				ListFolderDirInfo.Add(NextFolder);
			}
			return ListFolderDirInfo;
		}
		catch
		{
			return null;
		}
	}

	public static bool CreateDir(string strDir)
	{
		try
		{
			if (!Directory.Exists(strDir))
			{
				Directory.CreateDirectory(strDir);
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static string getHexStringFromFile(string filename)
	{
		FileStream fs = new FileStream(filename, FileMode.Open);
		byte[] bs = new byte[fs.Length];
		fs.Seek(0L, SeekOrigin.Begin);
		for (int j = 0; j < bs.Length; j++)
		{
			bs[j] = (byte)fs.ReadByte();
		}
		fs.Close();
		string s = "";
		for (int i = 0; i < bs.Length; i++)
		{
			string qq = Convert.ToString(bs[i], 16);
			if (qq.Length < 2)
			{
				qq = "0" + qq;
			}
			s += qq;
		}
		return s.ToUpper();
	}

	public static string ByteToHexString(byte[] InBytes)
	{
		string StringOut = "";
		foreach (byte InByte in InBytes)
		{
			StringOut += $"{InByte:X2}";
		}
		return StringOut;
	}

	public static bool CopyDirectory(string srcdir, string desdir)
	{
		try
		{
			string folderName = srcdir.Substring(srcdir.LastIndexOf("\\") + 1);
			string desfolderdir = desdir + "\\" + folderName;
			if (desdir.LastIndexOf("\\") == desdir.Length - 1)
			{
				desfolderdir = desdir + folderName;
			}
			string[] filenames = Directory.GetFileSystemEntries(srcdir);
			string[] array = filenames;
			foreach (string file in array)
			{
				if (Directory.Exists(file))
				{
					string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
					if (!Directory.Exists(currentdir))
					{
						Directory.CreateDirectory(currentdir);
					}
					CopyDirectory(file, desfolderdir);
					continue;
				}
				string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);
				srcfileName = desfolderdir + "\\" + srcfileName;
				if (!Directory.Exists(desfolderdir))
				{
					Directory.CreateDirectory(desfolderdir);
				}
				File.Copy(file, srcfileName);
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
