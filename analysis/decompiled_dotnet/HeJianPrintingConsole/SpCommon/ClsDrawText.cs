using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using HeJianPrintingConsole;

namespace SpCommon;

internal class ClsDrawText
{
	public Bitmap DrawObj(string strSrcMsgFilePath)
	{
		try
		{
			Bitmap bmpDest = new Bitmap(ClsMsg.GetMsgWidth(strSrcMsgFilePath), ClsMsg.GetMsgHeight(strSrcMsgFilePath));
			Graphics g = Graphics.FromImage(bmpDest);
			int iObjCount = ClsMsg.GetObjCount(strSrcMsgFilePath);
			bool bRlt = true;
			for (int iObjIndex = 1; iObjIndex <= iObjCount; iObjIndex++)
			{
				if (!DrawText(strSrcMsgFilePath, iObjIndex, g))
				{
					return null;
				}
			}
			return bmpDest;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			return null;
		}
	}

	public bool DrawText(string strSrcMsgFilePath, int iObjIndex, Graphics gDest)
	{
		try
		{
			int iObjWidth = ClsMsg.GetObjWidth(strSrcMsgFilePath, iObjIndex);
			int iObjHeight = ClsMsg.GetObjHeight(strSrcMsgFilePath, iObjIndex);
			string strDest = ClsMsg.GetText(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			string strFontName = ClsMsg.GetFontName(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			int iFontSize = ClsMsg.GetFontSize(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			FontStyle fontStyleTmp = ClsMsg.GetFontStyle(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			Font fontTmp = new Font(strFontName, iFontSize, fontStyleTmp);
			Brush brushTmp = new SolidBrush(Color.Black);
			SizeF strSize = gDest.MeasureString(strDest, fontTmp);
			Bitmap bmp = new Bitmap(Convert.ToInt32(strSize.Width), Convert.ToInt32((int)strSize.Height));
			Graphics g = Graphics.FromImage(bmp);
			string strBkImgFile = Application.StartupPath + "\\Image\\BkImg.bmp";
			g.DrawImage(Image.FromFile(strBkImgFile), 0, 0);
			g.DrawString(strDest, fontTmp, brushTmp, 0f, 0f);
			int iAngle = ClsMsg.GetObjAngle(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			bmp = PicRotation(bmp, iAngle);
			bmp = PicResumeMsgWidthHeight(bmp, iObjWidth, iObjHeight);
			gDest.DrawImage(bmp, 0, 0);
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show("Draw text exception:" + ex.Message);
			return false;
		}
	}

	public bool DrawText2(string strSrcMsgFilePath, int iObjIndex, Graphics gDest, string strDynamicData)
	{
		try
		{
			ClsMsg.GetObjWidth(strSrcMsgFilePath, iObjIndex);
			ClsMsg.GetObjHeight(strSrcMsgFilePath, iObjIndex);
			string strFontName = ClsMsg.GetFontName(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			int iFontSize = ClsMsg.GetFontSize(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			FontStyle fontStyleTmp = ClsMsg.GetFontStyle(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			Font fontTmp = new Font(strFontName, iFontSize, fontStyleTmp);
			Brush brushTmp = new SolidBrush(Color.Black);
			SizeF strSize = gDest.MeasureString(strDynamicData, fontTmp);
			Bitmap bmp = new Bitmap(Convert.ToInt32(strSize.Width), Convert.ToInt32((int)strSize.Height));
			Graphics g = Graphics.FromImage(bmp);
			string strBkImgFile = Application.StartupPath + "\\Image\\BkImg.bmp";
			g.DrawImage(Image.FromFile(strBkImgFile), 0, 0);
			g.DrawString(strDynamicData, fontTmp, brushTmp, 0f, 0f);
			bmp.Save(Application.StartupPath + "\\aaaa.bmp", ImageFormat.Bmp);
			gDest.DrawImage(bmp, 0, 0);
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show("Draw text exception:" + ex.Message);
			return false;
		}
	}

	public static Bitmap DrawText3(string strSrcMsgFilePath, int iObjIndex, string strDynamicData)
	{
		try
		{
			ClsMsg.GetObjWidth(strSrcMsgFilePath, iObjIndex);
			ClsMsg.GetObjHeight(strSrcMsgFilePath, iObjIndex);
			string strDest = strDynamicData.Trim();
			string strFontName = ClsMsg.GetFontName(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			int iFontSize = ClsMsg.GetFontSize(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			FontStyle fontStyleTmp = ClsMsg.GetFontStyle(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			Font fontTmp = new Font(strFontName, iFontSize, fontStyleTmp);
			Brush brushTmp = new SolidBrush(Color.Black);
			PictureBox picBox = new PictureBox();
			SizeF strSize = picBox.CreateGraphics().MeasureString(strDest, fontTmp);
			Bitmap bmp = new Bitmap(Convert.ToInt32(strSize.Width), Convert.ToInt32((int)strSize.Height));
			Graphics g = Graphics.FromImage(bmp);
			string strBkImgFile = Application.StartupPath + "\\Image\\BkImg.bmp";
			g.DrawImage(Image.FromFile(strBkImgFile), 0, 0);
			g.DrawString(strDest, fontTmp, brushTmp, 0f, 0f);
			Bitmap bmpDest = null;
			ClsPic.PicToBwBmp(bmp, ref bmpDest);
			return bmpDest;
		}
		catch (Exception ex)
		{
			MessageBox.Show("Draw text exception:" + ex.Message);
			return null;
		}
	}

	public static Bitmap DrawText4(string strFontName, int iFontSize, FontStyle fontStyleTmp, string strDynamicData)
	{
		try
		{
			string strDest = strDynamicData.Trim();
			if (strDest.Length <= 0)
			{
				strDest = " ";
			}
			Font fontTmp = new Font(strFontName, iFontSize, fontStyleTmp);
			Brush brushTmp = new SolidBrush(Color.Black);
			PictureBox picBox = new PictureBox();
			SizeF strSize = picBox.CreateGraphics().MeasureString(strDest, fontTmp);
			Bitmap bmp = new Bitmap(Convert.ToInt32(strSize.Width), Convert.ToInt32((int)strSize.Height));
			Graphics g = Graphics.FromImage(bmp);
			g.Clear(Color.White);
			g.DrawRectangle(new Pen(Color.White), new Rectangle(0, 0, (int)strSize.Width, (int)strSize.Height));
			g.DrawString(strDest, fontTmp, brushTmp, 0f, 0f);
			Bitmap bmpDest = null;
			ClsPic.PicToBwBmp(bmp, ref bmpDest);
			bmp = bmpDest;
			bmp = ClsPic.CutPicAroundSpace(bmp);
			bmpDest = null;
			ClsPic.PicToBwBmp(bmp, ref bmpDest);
			return bmpDest;
		}
		catch (Exception ex)
		{
			MessageBox.Show("Draw text exception:" + ex.Message);
			return null;
		}
	}

	public bool DrawText(string strSrcMsgFilePath, int iObjIndex, Graphics gDest, string strDynamicData)
	{
		try
		{
			ClsMsg.GetObjWidth(strSrcMsgFilePath, iObjIndex);
			ClsMsg.GetObjHeight(strSrcMsgFilePath, iObjIndex);
			string strFontName = ClsMsg.GetFontName(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			int iFontSize = ClsMsg.GetFontSize(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			FontStyle fontStyleTmp = ClsMsg.GetFontStyle(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			Font fontTmp = new Font(strFontName, iFontSize, fontStyleTmp);
			Brush brushTmp = new SolidBrush(Color.Black);
			SizeF strSize = gDest.MeasureString(strDynamicData, fontTmp);
			Bitmap bmp = new Bitmap(Convert.ToInt32(strSize.Width), Convert.ToInt32((int)strSize.Height));
			Graphics g = Graphics.FromImage(bmp);
			g.DrawString(strDynamicData, fontTmp, brushTmp, 0f, 0f);
			int iAngle = ClsMsg.GetObjAngle(strSrcMsgFilePath, iObjIndex, ClsMsg.EnumMsg_Type.Msg_Text);
			bmp = PicRotation(bmp, iAngle);
			double dZoomX = ClsMsg.GetZoomX(strSrcMsgFilePath, iObjIndex);
			double dZoomY = ClsMsg.GetZoomY(strSrcMsgFilePath, iObjIndex);
			bmp = PicZoom(bmp, dZoomX, dZoomY);
			int iX = ClsMsg.GetObjX(strSrcMsgFilePath, iObjIndex);
			int iY = Convert.ToInt32((double)ClsMsg.GetObjY(strSrcMsgFilePath, iObjIndex) / 0.746268656716418);
			gDest.DrawImage(bmp, iX, iY);
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show("Draw text exception:" + ex.Message);
			return false;
		}
	}

	private Bitmap PicResumeMsgWidthHeight(Bitmap bmpSrc, int iWidth, int iHeight)
	{
		try
		{
			return new Bitmap(bmpSrc, iWidth, iHeight);
		}
		catch (Exception ex)
		{
			MessageBox.Show("PicResumeMsgWidthHeight exception:" + ex.Message);
			return null;
		}
	}

	private Bitmap PicZoom(Bitmap bmpSrc, double dZoomX, double dZoomY)
	{
		try
		{
			int iWidth = Convert.ToInt32((double)bmpSrc.Width * dZoomX);
			int iHeight = Convert.ToInt32((double)bmpSrc.Height * dZoomY);
			return new Bitmap(bmpSrc, iWidth, iHeight);
		}
		catch (Exception ex)
		{
			MessageBox.Show("PicZoom exception:" + ex.Message);
			return null;
		}
	}

	private Bitmap PicRotation(Bitmap bmpSrc, int iAngle)
	{
		try
		{
			Bitmap bmpDest = null;
			RotateFlipType RotationFlipTypeTmp = RotateFlipType.RotateNoneFlipNone;
			switch (iAngle)
			{
			case 0:
				RotationFlipTypeTmp = RotateFlipType.RotateNoneFlipNone;
				break;
			case 90:
				RotationFlipTypeTmp = RotateFlipType.Rotate90FlipNone;
				break;
			case 180:
				RotationFlipTypeTmp = RotateFlipType.Rotate180FlipNone;
				break;
			case 270:
				RotationFlipTypeTmp = RotateFlipType.Rotate270FlipNone;
				break;
			}
			bmpSrc.RotateFlip(RotationFlipTypeTmp);
			return bmpSrc;
		}
		catch (Exception ex)
		{
			MessageBox.Show("PicRotation exception:" + ex.Message);
			return null;
		}
	}
}
