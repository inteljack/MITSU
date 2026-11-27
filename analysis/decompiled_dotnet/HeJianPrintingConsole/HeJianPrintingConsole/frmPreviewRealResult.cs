using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmPreviewRealResult : frmBase
{
	private ClsCartonSizeFieldValue m_ClsCSFV;

	private Bitmap m_bmpPreview;

	private int m_bmpPreviewOriginalWidth;

	private int m_iWidthDPI = 402;

	private int m_iHeightDPI = 300;

	private int m_iWidthDPIOriginal = 402;

	private int m_iHeightDPIOriginal = 300;

	private int m_iWidthMaxLen_mm = 1800;

	private int m_iHeightMaxLen_mm = 1000;

	private int m_iDrawDigitValue = 5;

	private Bitmap m_bmpRulerWidthShow;

	private Bitmap m_bmpRulerHeightShow;

	private Bitmap m_bmpRulerWidthOriginal;

	private Bitmap m_bmpRulerHeightOriginal;

	private int m_iDivideHeightBetweenOutsideAndInSide = 300;

	private int m_iCaseSideWidth = 30;

	private double m_dInchToMM_Factor = 25.39;

	private int m_iLastWidthRulerStartValue;

	private int m_iLastHeightRulerStartValue;

	private IContainer components;

	private Panel pnlClientPreview;

	private Label lblZoom;

	private GroupBox groupBoxZoom;

	private TrackBar trackBarZoom;

	private Panel pnlClientClient;

	private Panel pnlClientClientClientShow;

	private PictureBox pictureBoxPreview;

	private Panel pnlClientClientLeftHeightRuler;

	private Panel pnlClientClientTop;

	private Panel pnlClientClientTopClientWidthRuler;

	private Panel pnlClientClientTopLeftTip;

	private Label lblRulerTip;

	private GroupBox groupBoxCoordinate;

	private Label label4;

	private Label labelY;

	private Label labelX;

	private Label label1;

	public string m_strCaseNo { get; set; }

	public string m_strMsgFilePath { get; set; }

	public string m_strCheckFieldExceedMaxLenMsg { get; set; }

	public bool m_bCheckFieldExceedMaxLenRlt { get; set; }

	public DataTable PrintHeadDelayDataTable { get; set; }

	public int PrintHeadCount { get; set; }

	public double dMsgWidthOutside_Pixel { get; set; }

	public double dMsgWidthInside_Pixel { get; set; }

	public Label labelReadyPreviewTip { get; set; }

	public frmPreviewRealResult()
	{
		InitializeComponent();
		IniPreviewFormParam();
	}

	private void IniPreviewFormParam()
	{
		base.Left = ClsXml.GetPreviewFormX();
		base.Top = ClsXml.GetPreviewFormY();
		base.Width = ClsXml.GetPreviewFormWidth();
		base.Height = ClsXml.GetPreviewFormHeight();
	}

	private void frmPreview_Load(object sender, EventArgs e)
	{
		trackBarZoom.Value = ClsXml.GetPreviewZoomScale();
		string strZoomScale = trackBarZoom.Value * 2 + "%";
		strZoomScale = strZoomScale.Trim().PadLeft(4);
		lblZoom.Text = strZoomScale;
		m_iWidthMaxLen_mm = ClsXml.GetWidthRulerMaxLen();
		m_iHeightMaxLen_mm = ClsXml.GetHeightRulerMaxLen();
		PreviewMsg(m_strMsgFilePath);
		IniRuler((double)trackBarZoom.Value / 100.0);
	}

	private void IniRuler(double dZoomScale)
	{
		m_iWidthDPIOriginal = Convert.ToInt32(ClsXml.GetWidthDPI());
		m_iHeightDPIOriginal = ClsXml.GetHeightDPI();
		m_iWidthDPI = m_iWidthDPIOriginal;
		m_iHeightDPI = m_iHeightDPIOriginal;
		int iWidthRulerLen_pixel = Convert.ToInt32((double)(m_iWidthMaxLen_mm * m_iWidthDPI) / m_dInchToMM_Factor);
		int iHeightRulerLen_pixel = Convert.ToInt32((double)(m_iHeightMaxLen_mm * m_iHeightDPI) / m_dInchToMM_Factor);
		PainRuler(0, bPaintWidthRuler: true, iWidthRulerLen_pixel, bFirstPaintWidthRuler: true, 0, bPaintHeightRuler: true, iHeightRulerLen_pixel, bFirstPaintHeightRuler: true);
		ShowRuler(dZoomScale);
		lblRulerTip.Text = "分辨率:\n      " + m_iWidthDPI + "\n單位：\n     毫米";
	}

	private void PainRuler(int iWidthRulerStartValue, bool bPaintWidthRuler, int iWidthRulerLen_pixel, bool bFirstPaintWidthRuler, int iHeightRulerStartValue, bool bPaintHeightRuler, int iHeightRulerLen_pixel, bool bFirstPaintHeightRuler)
	{
		try
		{
			if (bPaintWidthRuler)
			{
				m_bmpRulerWidthShow = null;
				if (bFirstPaintWidthRuler)
				{
					m_bmpRulerWidthOriginal = PaintWidthRuler(pnlClientClientTopClientWidthRuler.Height, iWidthRulerLen_pixel, m_iWidthMaxLen_mm, m_iDrawDigitValue);
					m_bmpRulerWidthShow = (Bitmap)m_bmpRulerWidthOriginal.Clone();
				}
				else
				{
					m_bmpRulerWidthShow = PaintWidthRuler(pnlClientClientTopClientWidthRuler.Height, iWidthRulerLen_pixel, m_iWidthMaxLen_mm, m_iDrawDigitValue);
				}
				Bitmap bmpWidthRuler = new Bitmap(pnlClientClientTopClientWidthRuler.Width, m_bmpRulerWidthShow.Height);
				Graphics gWidthRuler = Graphics.FromImage(bmpWidthRuler);
				gWidthRuler.DrawImage(m_bmpRulerWidthShow, 0, 0);
				gWidthRuler.Dispose();
				Bitmap bmpDestWidthRuler = null;
				ClsPic.PicToBwBmp(bmpWidthRuler, ref bmpDestWidthRuler);
				pnlClientClientTopClientWidthRuler.BackgroundImage = bmpDestWidthRuler;
			}
			if (bPaintHeightRuler)
			{
				m_bmpRulerHeightShow = null;
				if (bFirstPaintHeightRuler)
				{
					m_bmpRulerHeightOriginal = PaintHeightRuler(pnlClientClientLeftHeightRuler.Width, iHeightRulerLen_pixel, m_iHeightMaxLen_mm, m_iDrawDigitValue);
					m_bmpRulerHeightShow = (Bitmap)m_bmpRulerHeightOriginal.Clone();
				}
				else
				{
					m_bmpRulerHeightShow = PaintHeightRuler(pnlClientClientLeftHeightRuler.Width, iHeightRulerLen_pixel, m_iHeightMaxLen_mm, m_iDrawDigitValue);
				}
				Bitmap bmpHeightRuler = new Bitmap(m_bmpRulerHeightShow.Width, pnlClientClientLeftHeightRuler.Height);
				Graphics gHeightRuler = Graphics.FromImage(bmpHeightRuler);
				gHeightRuler.DrawImage(m_bmpRulerHeightShow, 0, 0);
				gHeightRuler.Dispose();
				Bitmap bmpDestHeightRuler = null;
				ClsPic.PicToBwBmp(bmpHeightRuler, ref bmpDestHeightRuler);
				pnlClientClientLeftHeightRuler.BackgroundImage = bmpDestHeightRuler;
			}
		}
		catch (Exception)
		{
		}
	}

	private Bitmap PaintWidthRuler(int iRulerHeight, int iRulerLen, int iRulerMaxLen_mm, int iDrawDigitValue)
	{
		Bitmap bmp = new Bitmap(iRulerLen, iRulerHeight);
		Graphics g = Graphics.FromImage(bmp);
		g.DrawLine(new Pen(Color.White, bmp.Width), 0, 0, bmp.Width, bmp.Height);
		g.DrawLine(new Pen(Color.White, bmp.Width), 0, bmp.Height, bmp.Width, 0);
		Pen penTmp = new Pen(Color.Black, 1f);
		int iLineLen = iRulerHeight / 2;
		int iMillimeterCount = 0;
		Font fontTmp = new Font("Arial", 10f, FontStyle.Regular);
		Brush brushTmp = new SolidBrush(Color.Black);
		int iYBase = bmp.Height;
		int iXBase = 0;
		double dPixelPerMillimeter = Convert.ToDouble(iRulerLen) / (double)iRulerMaxLen_mm;
		int iPixelPerMillimeterInteger = Convert.ToInt32(dPixelPerMillimeter - dPixelPerMillimeter % 1.0);
		double dTinyDifferencePixel = (((double)iPixelPerMillimeterInteger == dPixelPerMillimeter) ? 0.0 : ((double)(iPixelPerMillimeterInteger + 1) - dPixelPerMillimeter));
		int iTottingUpCount = 0;
		for (iMillimeterCount = 0; iMillimeterCount < iRulerMaxLen_mm; iMillimeterCount++)
		{
			int iX = iXBase + Convert.ToInt32((double)iMillimeterCount * dPixelPerMillimeter);
			if (iMillimeterCount % iDrawDigitValue == 0)
			{
				iLineLen = 2 * iRulerHeight / 5;
				if (iMillimeterCount != 0)
				{
					SizeF strSize = g.MeasureString(iMillimeterCount.ToString(), fontTmp);
					g.DrawString(iMillimeterCount.ToString(), fontTmp, brushTmp, (float)iX - strSize.Width / 2f + 2f, (float)(iYBase - iLineLen) - strSize.Height - 5f);
				}
			}
			else
			{
				iLineLen = iRulerHeight / 2;
			}
			iTottingUpCount++;
			if ((double)iTottingUpCount * dTinyDifferencePixel > 1.0)
			{
				iX--;
				iTottingUpCount = 0;
			}
			if (iMillimeterCount % (iDrawDigitValue / 5) == 0)
			{
				g.DrawLine(penTmp, iX, iYBase, iX, iYBase - iLineLen);
			}
		}
		g.Dispose();
		return bmp;
	}

	private Bitmap PaintHeightRuler(int iRulerHeight, int iRulerLen, int iRulerMaxLen_mm, int iDrawDigitValue)
	{
		Bitmap bmp = new Bitmap(iRulerHeight, iRulerLen);
		Graphics g = Graphics.FromImage(bmp);
		g.DrawLine(new Pen(Color.White, bmp.Width), 0, 0, bmp.Width, bmp.Height);
		g.DrawLine(new Pen(Color.White, bmp.Width), 0, bmp.Height, bmp.Width, 0);
		Pen penTmp = new Pen(Color.Black, 1f);
		int iLineLen = iRulerHeight / 2;
		int iMillimeterCount = 0;
		Font fontTmp = new Font("Arial", 10f, FontStyle.Regular);
		Brush brushTmp = new SolidBrush(Color.Black);
		int iXBase = bmp.Width;
		int iYBase = 0;
		double dPixelPerMillimeter = Convert.ToDouble(iRulerLen) / (double)iRulerMaxLen_mm;
		int iPixelPerMillimeterInteger = Convert.ToInt32(dPixelPerMillimeter - dPixelPerMillimeter % 1.0);
		double dTinyDifferencePixel = (((double)iPixelPerMillimeterInteger == dPixelPerMillimeter) ? 0.0 : ((double)(iPixelPerMillimeterInteger + 1) - dPixelPerMillimeter));
		int iTottingUpCount = 0;
		for (iMillimeterCount = 0; iMillimeterCount < iRulerMaxLen_mm; iMillimeterCount++)
		{
			int iY = iYBase + Convert.ToInt32((double)iMillimeterCount * dPixelPerMillimeter);
			if (iMillimeterCount % iDrawDigitValue == 0)
			{
				iLineLen = 2 * iRulerHeight / 5;
				if (iMillimeterCount != 0)
				{
					SizeF strSize = g.MeasureString(iMillimeterCount.ToString(), fontTmp);
					g.DrawString(iMillimeterCount.ToString(), fontTmp, brushTmp, (float)(iXBase - iLineLen) - strSize.Width - 5f, (float)iY - strSize.Height / 2f);
				}
			}
			else
			{
				iLineLen = iRulerHeight / 2;
			}
			iTottingUpCount++;
			if ((double)iTottingUpCount * dTinyDifferencePixel > 1.0)
			{
				iY--;
				iTottingUpCount = 0;
			}
			if (iMillimeterCount % (iDrawDigitValue / 5) == 0)
			{
				g.DrawLine(penTmp, iXBase, iY, iXBase - iLineLen, iY);
			}
		}
		g.Dispose();
		return bmp;
	}

	private void pnlClientClientClientShow_Scroll(object sender, ScrollEventArgs e)
	{
		int iWidthRulerStartValue = 0;
		int iHeightRulerStartValue = 0;
		iWidthRulerStartValue = ((Panel)sender).HorizontalScroll.Value;
		iHeightRulerStartValue = ((Panel)sender).VerticalScroll.Value;
		RepainRuler(iWidthRulerStartValue, m_iLastWidthRulerStartValue != iWidthRulerStartValue, iHeightRulerStartValue, m_iLastHeightRulerStartValue != iHeightRulerStartValue);
		m_iLastWidthRulerStartValue = iWidthRulerStartValue;
		m_iLastHeightRulerStartValue = iHeightRulerStartValue;
	}

	private void RepainRuler(int iCutOutLenFromOriginalWidth, bool bPaintWidthRuler, int iCutOutLenFromOriginalHeight, bool bPaintHeightRuler)
	{
		if (bPaintWidthRuler)
		{
			Bitmap bmpWidthRuler = CutWidthRulerFromBmp(m_bmpRulerWidthShow, iCutOutLenFromOriginalWidth);
			pnlClientClientTopClientWidthRuler.BackgroundImage = bmpWidthRuler;
		}
		if (bPaintHeightRuler)
		{
			Bitmap bmpHeightRuler = CutHeightRulerFromBmp(m_bmpRulerHeightShow, iCutOutLenFromOriginalHeight);
			pnlClientClientLeftHeightRuler.BackgroundImage = bmpHeightRuler;
		}
	}

	private Bitmap CutWidthRulerFromBmp(Bitmap bmpSrcRuler, int iCutOutLenFromOriginal)
	{
		try
		{
			Bitmap bmp = new Bitmap(bmpSrcRuler.Width - iCutOutLenFromOriginal, bmpSrcRuler.Height);
			Graphics g = Graphics.FromImage(bmp);
			g.DrawLine(new Pen(Color.White, bmp.Width), 0, 0, bmp.Width, bmp.Height);
			g.DrawLine(new Pen(Color.White, bmp.Width), 0, bmp.Height, bmp.Width, 0);
			g.DrawImage(bmpSrcRuler, -iCutOutLenFromOriginal, 0);
			g.Dispose();
			return bmp;
		}
		catch
		{
			return bmpSrcRuler;
		}
	}

	private Bitmap CutHeightRulerFromBmp(Bitmap bmpSrcRuler, int iCutOutLenFromOriginal)
	{
		try
		{
			Bitmap bmp = new Bitmap(bmpSrcRuler.Width, bmpSrcRuler.Height - iCutOutLenFromOriginal);
			Graphics g = Graphics.FromImage(bmp);
			g.DrawLine(new Pen(Color.White, bmp.Height), 0, 0, bmp.Width, bmp.Height);
			g.DrawLine(new Pen(Color.White, bmp.Height), bmp.Width, 0, 0, bmp.Height);
			g.DrawImage(bmpSrcRuler, 0, -iCutOutLenFromOriginal);
			g.Dispose();
			return bmp;
		}
		catch
		{
			return bmpSrcRuler;
		}
	}

	private void ShowRuler(double dZoomScale)
	{
		m_iDrawDigitValue = ResetDrawDititValue(dZoomScale);
		PainRuler(0, bPaintWidthRuler: true, Convert.ToInt32((double)m_bmpRulerWidthOriginal.Width * dZoomScale), bFirstPaintWidthRuler: false, 0, bPaintHeightRuler: true, Convert.ToInt32((double)m_bmpRulerHeightOriginal.Height * dZoomScale), bFirstPaintHeightRuler: false);
		pnlClientClientClientShow_Scroll(pnlClientClientClientShow, null);
	}

	private void frmPreviewRealResult_Resize(object sender, EventArgs e)
	{
		IniRuler((double)trackBarZoom.Value / 100.0);
	}

	[DllImport("acmmil.dll", CallingConvention = CallingConvention.StdCall)]
	private static extern byte miPreViewMessagetoStream(byte[] cbmpdata, byte[] msgdata);

	[DllImport("acmmil.dll", CallingConvention = CallingConvention.StdCall)]
	private static extern byte miInitMsgParamforPreview();

	[DllImport("acmmil.dll", CallingConvention = CallingConvention.StdCall)]
	private static extern byte miGetPreViewMessagetoStreamLength(ref int len, byte[] msgdata);

	[DllImport("acbcl.dll", CallingConvention = CallingConvention.StdCall)]
	private static extern byte bcInitPluginsforPreveiw();

	[DllImport("acmmil.dll", CallingConvention = CallingConvention.StdCall)]
	private static extern byte miPreViewMessagetoStream1(byte[] cpath, byte[] data);

	private void PreviewMsg(string strMsgFilePath)
	{
		m_bmpPreview = new Bitmap(MsgToBmp(strMsgFilePath));
		m_bmpPreviewOriginalWidth = m_bmpPreview.Width;
		ConvertPreviewToRealResult(ref m_bmpPreview, PrintHeadCount);
		double dZoom = (double)trackBarZoom.Value / 100.0;
		ShowPreviewBmp(m_bmpPreview, dZoom, dZoom);
	}

	public void ConvertPreviewToRealResult(ref Bitmap bmpPreview, int printHeadCount)
	{
		if (printHeadCount <= 0)
		{
			MessageBox.Show("噴頭數必須大於零!");
		}
		if (printHeadCount % 2 != 0)
		{
			MessageBox.Show("噴頭數必須為偶數!");
			return;
		}
		GetCartonSize();
		int CartonSizeWidthAndLength_Pixel = Convert.ToInt32((m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL) * (double)m_iWidthDPI / m_dInchToMM_Factor);
		Convert.ToInt32(m_ClsCSFV.dCartonSizeH * (double)m_iWidthDPI / m_dInchToMM_Factor);
		int iInsidePrintWordBegin_Pixel = Convert.ToInt32(m_ClsCSFV.dInsidePrintWordBegin * (double)m_iWidthDPI / m_dInchToMM_Factor);
		int iOutsidePrintWordBegin_Pixel = Convert.ToInt32(m_ClsCSFV.dOutsidePrintWordBegin * (double)m_iWidthDPI / m_dInchToMM_Factor);
		int iOutsidePrintWordEnd_Pixel = Convert.ToInt32(m_ClsCSFV.dOutsidePrintWordEnd * (double)m_iWidthDPI / m_dInchToMM_Factor);
		Bitmap outsideBitmap = new Bitmap(bmpPreview.Width, bmpPreview.Height / 2);
		Graphics gOutside = Graphics.FromImage(outsideBitmap);
		gOutside.Clear(Color.White);
		gOutside.DrawImage(bmpPreview, 0, 0);
		gOutside.Dispose();
		if (dMsgWidthInside_Pixel > dMsgWidthOutside_Pixel)
		{
			int outsideBitmapTempX = CartonSizeWidthAndLength_Pixel - iOutsidePrintWordBegin_Pixel;
			if (outsideBitmap.Width >= CartonSizeWidthAndLength_Pixel)
			{
				outsideBitmapTempX = iOutsidePrintWordEnd_Pixel - iOutsidePrintWordBegin_Pixel;
			}
			Bitmap outsideBitmapTemp = new Bitmap(outsideBitmapTempX, outsideBitmap.Height);
			Graphics gOutsideTemp = Graphics.FromImage(outsideBitmapTemp);
			gOutsideTemp.Clear(Color.White);
			gOutsideTemp.DrawImage(outsideBitmap, 0, 0);
			gOutsideTemp.Dispose();
			outsideBitmap = outsideBitmapTemp;
		}
		Bitmap insideBitmap = new Bitmap(bmpPreview.Width, bmpPreview.Height / 2);
		Graphics gInside = Graphics.FromImage(insideBitmap);
		gInside.Clear(Color.White);
		gInside.DrawImage(bmpPreview, 0, -(bmpPreview.Height / 2));
		insideBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
		gInside.Dispose();
		if (dMsgWidthInside_Pixel < dMsgWidthOutside_Pixel)
		{
			int iNeedCutSpaceWidth = Convert.ToInt32(dMsgWidthOutside_Pixel - dMsgWidthInside_Pixel);
			Bitmap insideBitmapTemp = new Bitmap(insideBitmap.Width - iNeedCutSpaceWidth, insideBitmap.Height);
			Graphics gInsideTemp = Graphics.FromImage(insideBitmapTemp);
			gInsideTemp.Clear(Color.White);
			gInsideTemp.DrawImage(insideBitmap, -iNeedCutSpaceWidth, 0);
			gInsideTemp.Dispose();
			insideBitmap = insideBitmapTemp;
		}
		int iBmpPreviewSimulateWidth = insideBitmap.Width + iInsidePrintWordBegin_Pixel;
		if (outsideBitmap.Width + iOutsidePrintWordBegin_Pixel > insideBitmap.Width + iInsidePrintWordBegin_Pixel)
		{
			iBmpPreviewSimulateWidth = outsideBitmap.Width + iOutsidePrintWordBegin_Pixel;
		}
		Bitmap bmpPreviewSimulate = new Bitmap(iBmpPreviewSimulateWidth, outsideBitmap.Height + insideBitmap.Height + m_iDivideHeightBetweenOutsideAndInSide);
		Graphics g = Graphics.FromImage(bmpPreviewSimulate);
		g.Clear(Color.White);
		g.DrawImage(insideBitmap, iInsidePrintWordBegin_Pixel, 0);
		g.DrawImage(outsideBitmap, iOutsidePrintWordBegin_Pixel, insideBitmap.Height + m_iDivideHeightBetweenOutsideAndInSide);
		g.Dispose();
		Bitmap bmpPreviewAndCaseSide = new Bitmap((CartonSizeWidthAndLength_Pixel > bmpPreviewSimulate.Width) ? (CartonSizeWidthAndLength_Pixel + m_iCaseSideWidth * 2) : (bmpPreviewSimulate.Width + m_iCaseSideWidth), bmpPreviewSimulate.Height);
		Graphics graphicsAddCaseSide = Graphics.FromImage(bmpPreviewAndCaseSide);
		graphicsAddCaseSide.Clear(Color.White);
		Pen penCaseSide = new Pen(new SolidBrush(Color.Red), m_iCaseSideWidth);
		graphicsAddCaseSide.DrawImage(bmpPreviewSimulate, m_iCaseSideWidth, 0);
		Bitmap divideBitmap = new Bitmap((bmpPreviewAndCaseSide.Width > CartonSizeWidthAndLength_Pixel) ? bmpPreviewAndCaseSide.Width : (bmpPreviewAndCaseSide.Width - m_iCaseSideWidth * 2), m_iDivideHeightBetweenOutsideAndInSide);
		Graphics gDivide = Graphics.FromImage(divideBitmap);
		gDivide.Clear(Color.SkyBlue);
		Font fontDivide = new Font("Arial", 150f, FontStyle.Bold);
		Brush brushDivide = new SolidBrush(Color.Black);
		string divideString = "分-------------------------界-------------------------線";
		SizeF sizeFDivide = gDivide.MeasureString(divideString, fontDivide);
		gDivide.DrawString(divideString, fontDivide, brushDivide, Convert.ToInt32(((float)divideBitmap.Width - sizeFDivide.Width) / 2f), Convert.ToInt32(((float)divideBitmap.Height - sizeFDivide.Height) / 2f));
		gDivide.Dispose();
		graphicsAddCaseSide.DrawImage(divideBitmap, m_iCaseSideWidth, insideBitmap.Height - 15);
		graphicsAddCaseSide.DrawLine(penCaseSide, new Point(0, 0), new Point(0, bmpPreviewAndCaseSide.Height));
		graphicsAddCaseSide.DrawLine(penCaseSide, new Point(CartonSizeWidthAndLength_Pixel + m_iCaseSideWidth, 0), new Point(CartonSizeWidthAndLength_Pixel + m_iCaseSideWidth, bmpPreviewAndCaseSide.Height));
		graphicsAddCaseSide.Dispose();
		bmpPreview = bmpPreviewAndCaseSide;
	}

	private Bitmap MsgToBmp(string strMsgFilePath)
	{
		FileStream fs = new FileStream(strMsgFilePath, FileMode.Open);
		byte[] data = new byte[fs.Length];
		fs.Read(data, 0, data.Length);
		fs.Close();
		bcInitPluginsforPreveiw();
		miInitMsgParamforPreview();
		int ilen = 0;
		byte rlt = 0;
		rlt = miGetPreViewMessagetoStreamLength(ref ilen, data);
		Bitmap bmpDest = null;
		if (rlt > 0)
		{
			byte[] bmpdata = new byte[ilen + 1];
			bmpdata[ilen] = 0;
			miPreViewMessagetoStream(bmpdata, data);
			MemoryStream ms = new MemoryStream();
			ms.Write(bmpdata, 0, ilen);
			bmpDest = new Bitmap(ms);
			ms.Close();
		}
		return bmpDest;
	}

	private void ShowPreviewBmp(Bitmap bmpSrc, double dZoomX, double dZoomY)
	{
		try
		{
			int iNewWidth = (int)((double)bmpSrc.Width * dZoomX);
			int iNewHeight = (int)((double)bmpSrc.Height * dZoomY);
			pictureBoxPreview.Width = iNewWidth;
			pictureBoxPreview.Height = iNewHeight;
			pictureBoxPreview.Image = bmpSrc;
		}
		catch (Exception ex)
		{
			trackBarZoom.Value = 10;
			double dZoomScale = trackBarZoom.Value;
			ShowPreviewBmp(m_bmpPreview, dZoomScale, dZoomScale);
			ClsLog.LogWrite("ShowPreviewBmp exception:" + ex.Message);
		}
	}

	private void frmPreview_FormClosing(object sender, FormClosingEventArgs e)
	{
		string strZoomScale = trackBarZoom.Value.ToString();
		ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPreviewZoomScale, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, strZoomScale);
		SaveFormParam();
	}

	private void SaveFormParam()
	{
		try
		{
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPreviewFormX, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, base.Left.ToString());
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPreviewFormY, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, base.Top.ToString());
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPreviewFormWidth, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, base.Width.ToString());
			ClsXml.setParaValue(ClsXml.ClsXmlMember.strSecondFloorSubNodeNameKey, ClsXml.ClsXmlMember.strPreviewFormHeight, ClsXml.ClsXmlMember.strSecondFloorSubNodeNameValue, base.Height.ToString());
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("SavePreviewFormParam exception:" + ex.Message);
		}
	}

	private void trackBarZoom_Scroll(object sender, EventArgs e)
	{
		TrackBar tbTmp = (TrackBar)sender;
		ShowZoom(tbTmp);
	}

	private void ShowZoom(TrackBar tbTmp)
	{
		try
		{
			string strZoomScale = trackBarZoom.Value * 2 + "%";
			strZoomScale = strZoomScale.Trim().PadLeft(4);
			lblZoom.Text = strZoomScale;
			Refresh();
			double dZoomScale = (double)tbTmp.Value / 100.0;
			ShowPreviewBmp(m_bmpPreview, dZoomScale, dZoomScale);
			ShowRuler(dZoomScale);
		}
		catch
		{
		}
	}

	private int ResetDrawDititValue(double dZoomScale)
	{
		int iDrawDigitValueRlt = 5;
		if (dZoomScale > 0.8)
		{
			return 5;
		}
		if (dZoomScale > 0.5)
		{
			return 10;
		}
		if (dZoomScale > 0.25)
		{
			return 15;
		}
		if (dZoomScale > 0.166)
		{
			return 20;
		}
		if (dZoomScale > 0.125)
		{
			return 30;
		}
		if (dZoomScale > 0.1)
		{
			return 40;
		}
		if (dZoomScale > 0.05)
		{
			return 50;
		}
		if (dZoomScale > 0.04)
		{
			return 100;
		}
		if (dZoomScale > 0.03)
		{
			return 200;
		}
		if (dZoomScale > 0.02)
		{
			return 300;
		}
		if (dZoomScale > 0.01)
		{
			return 400;
		}
		return 500;
	}

	private void groupBoxZoom_Paint(object sender, PaintEventArgs e)
	{
		ClsShare.RepainGroupBoxSide(sender, e, this);
	}

	private bool CheckPrintParam(ref string strCheckPrintParamMsg)
	{
		try
		{
			GetCartonSize();
			bool bRlt = true;
			strCheckPrintParamMsg = "噴印圖形校驗:\n\n";
			if (!CheckCartonsize(ref strCheckPrintParamMsg))
			{
				return false;
			}
			if (!CheckCartonsizeAndMsg(ref strCheckPrintParamMsg))
			{
				return false;
			}
			if (!CheckPrintData(ref strCheckPrintParamMsg))
			{
				return false;
			}
			return bRlt;
		}
		catch
		{
			return false;
		}
	}

	private void GetCartonSize()
	{
		try
		{
			string strCaseNo = m_strCaseNo.Trim();
			if (strCaseNo.Length <= 0)
			{
				ClsLog.LogWrite("GetCartonSize error:CaseNo error");
				return;
			}
			string strTabName = ClsXml.GetCartonSizeTableName();
			string strPrefix = strTabName.Substring(0, 2).Trim().ToUpper();
			string strSql = "SELECT * FROM " + strTabName + "  WHERE [" + strPrefix + ClsCartonSizeFieldKey.strCaseNo + "]='" + strCaseNo + "'";
			DataTable dt = ClsDataBase.SqlReadData(strSql);
			if (dt == null || dt.Rows.Count <= 0)
			{
				return;
			}
			m_ClsCSFV = new ClsCartonSizeFieldValue();
			string cartonsizeLog = "GetCartonSize frmPreviewRealResult 紙箱規格:";
			for (int iIndex = 0; iIndex < dt.Columns.Count; iIndex++)
			{
				string strCaption = dt.Columns[iIndex].Caption.Substring(2).Trim().ToUpper();
				string strValue = dt.Rows[0][iIndex].ToString();
				string text = cartonsizeLog;
				cartonsizeLog = text + strCaption + "=" + strValue + ";  ";
				if (strCaption == ClsCartonSizeFieldKey.strID)
				{
					m_ClsCSFV.strID = strValue;
				}
				else if (strCaption == ClsCartonSizeFieldKey.strCartonSizeL)
				{
					m_ClsCSFV.dCartonSizeL = Convert.ToDouble((strValue.Length <= 0) ? "0" : strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strCartonSizeW)
				{
					m_ClsCSFV.dCartonSizeW = Convert.ToDouble((strValue.Length <= 0) ? "0" : strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strCartonSizeH)
				{
					m_ClsCSFV.dCartonSizeH = Convert.ToDouble((strValue.Length <= 0) ? "0" : strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strPrintWordHeight)
				{
					m_ClsCSFV.dPrintWordHeight = Convert.ToDouble((strValue.Length <= 0) ? "0" : strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordBegin)
				{
					m_ClsCSFV.dOutsidePrintWordBegin = Convert.ToDouble((strValue.Length <= 0) ? "0" : strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strOutsidePrintWordEnd)
				{
					m_ClsCSFV.dOutsidePrintWordEnd = Convert.ToDouble((strValue.Length <= 0) ? "0" : strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strInsidePrintWordBegin)
				{
					m_ClsCSFV.dInsidePrintWordBegin = Convert.ToDouble((strValue.Length <= 0) ? "0" : strValue);
				}
				else if (strCaption == ClsCartonSizeFieldKey.strInsidePrintWordEnd)
				{
					m_ClsCSFV.dInsidePrintWordEnd = Convert.ToDouble((strValue.Length <= 0) ? "0" : strValue);
				}
			}
			ClsLog.LogWrite(cartonsizeLog);
		}
		catch (Exception ex)
		{
			ClsLog.LogWrite("GetCartonSize exception in the preview form:" + ex.Message);
		}
	}

	private bool CheckCartonsize(ref string strCheckCartonsizeMsg)
	{
		try
		{
			bool bRlt = true;
			if (m_ClsCSFV.dOutsidePrintWordBegin > m_ClsCSFV.dOutsidePrintWordEnd)
			{
				strCheckCartonsizeMsg += "表面印字開始超過表面印字終了!";
				return false;
			}
			if (m_ClsCSFV.dOutsidePrintWordBegin > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL)
			{
				strCheckCartonsizeMsg += "表面印字開始超過紙箱邊界!";
				return false;
			}
			if (m_ClsCSFV.dOutsidePrintWordEnd > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL)
			{
				strCheckCartonsizeMsg += "表面印字終了超過紙箱邊界!";
				return false;
			}
			if (m_ClsCSFV.dOutsidePrintWordEnd - m_ClsCSFV.dOutsidePrintWordBegin > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL)
			{
				strCheckCartonsizeMsg += "表面噴印區域超過紙箱邊界，請確認紙箱規格!";
				return false;
			}
			if (m_ClsCSFV.dInsidePrintWordBegin > m_ClsCSFV.dInsidePrintWordEnd)
			{
				strCheckCartonsizeMsg += "裏面印字開始超過裏面印字終了!";
				return false;
			}
			if (m_ClsCSFV.dInsidePrintWordBegin > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL)
			{
				strCheckCartonsizeMsg += "裏面印字開始超過紙箱邊界!";
				return false;
			}
			if (m_ClsCSFV.dInsidePrintWordEnd > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL)
			{
				strCheckCartonsizeMsg += "裏面印字終了超過紙箱邊界!";
				return false;
			}
			if (m_ClsCSFV.dInsidePrintWordEnd - m_ClsCSFV.dInsidePrintWordBegin > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL)
			{
				strCheckCartonsizeMsg += "裏面噴印區域超過紙箱邊界，請確認紙箱規格!";
				return false;
			}
			return bRlt;
		}
		catch
		{
			return false;
		}
	}

	private bool CheckPrintData(ref string strCheckPrintDataMsg)
	{
		try
		{
			bool bRlt = true;
			double dOutsidePrintAreaInMsg = m_ClsCSFV.dOutsidePrintWordEnd - m_ClsCSFV.dOutsidePrintWordBegin;
			double dInsidePrintAreaInMsg = m_ClsCSFV.dInsidePrintWordEnd - m_ClsCSFV.dInsidePrintWordBegin;
			double dDifferentValue = 0.0;
			int iPreviewBmpWidth_mm = Convert.ToInt32((double)m_bmpPreviewOriginalWidth / (double)m_iWidthDPIOriginal * m_dInchToMM_Factor);
			if (dOutsidePrintAreaInMsg >= dInsidePrintAreaInMsg)
			{
				dDifferentValue = dOutsidePrintAreaInMsg - dInsidePrintAreaInMsg;
				double dOutsidePrintAreaReal = iPreviewBmpWidth_mm;
				double dInsidePrintAreaReal = (double)iPreviewBmpWidth_mm - dDifferentValue;
				if (dOutsidePrintAreaReal > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL - m_ClsCSFV.dOutsidePrintWordBegin)
				{
					strCheckPrintDataMsg += "表面噴印區域超過紙箱邊界，請確認噴印資料是否過長!";
					return false;
				}
				if (dInsidePrintAreaReal > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL - m_ClsCSFV.dInsidePrintWordBegin)
				{
					strCheckPrintDataMsg += "裏面噴印區域超過紙箱邊界，請確認噴印資料是否過長!";
					return false;
				}
			}
			else
			{
				dDifferentValue = dInsidePrintAreaInMsg - dOutsidePrintAreaInMsg;
				double dOutsidePrintAreaReal2 = (double)iPreviewBmpWidth_mm - dDifferentValue;
				double dInsidePrintAreaReal2 = iPreviewBmpWidth_mm;
				if (dOutsidePrintAreaReal2 > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL - m_ClsCSFV.dOutsidePrintWordBegin)
				{
					strCheckPrintDataMsg += "表面噴印區域超過紙箱邊界，請確認噴印資料是否過長!";
					return false;
				}
				if (dInsidePrintAreaReal2 > m_ClsCSFV.dCartonSizeW + m_ClsCSFV.dCartonSizeL - m_ClsCSFV.dInsidePrintWordBegin)
				{
					strCheckPrintDataMsg += "裏面噴印區域超過紙箱邊界，請確認噴印資料是否過長!";
					return false;
				}
			}
			return bRlt;
		}
		catch
		{
			return false;
		}
	}

	private bool CheckCartonsizeAndMsg(ref string strCheckCartonsizeAndMsgMsg)
	{
		try
		{
			bool bRlt = true;
			double dOutsidePrintAreaInMsg = m_ClsCSFV.dOutsidePrintWordEnd - m_ClsCSFV.dOutsidePrintWordBegin;
			double dInsidePrintAreaInMsg = m_ClsCSFV.dInsidePrintWordEnd - m_ClsCSFV.dInsidePrintWordBegin;
			double dFrontMsgWidth_mm = (double)ClsMsg.GetMsgWidth(ClsXml.GetSourceMsg1Path()) / (double)m_iWidthDPIOriginal * m_dInchToMM_Factor;
			double dSideMsgWidth_mm = (double)ClsMsg.GetMsgWidth(ClsXml.GetSourceMsg2Path()) / (double)m_iWidthDPIOriginal * m_dInchToMM_Factor;
			if (dOutsidePrintAreaInMsg < dFrontMsgWidth_mm + dSideMsgWidth_mm)
			{
				strCheckCartonsizeAndMsgMsg += "表面噴印區域小於正側嘜模板寬度之和，噴印資料可能出現重疊!";
				return false;
			}
			if (dInsidePrintAreaInMsg < dSideMsgWidth_mm + dSideMsgWidth_mm)
			{
				strCheckCartonsizeAndMsgMsg += "裏面噴印區域小於正側嘜模板寬度之和，噴印資料可能出現重疊!";
				return false;
			}
			return bRlt;
		}
		catch
		{
			return false;
		}
	}

	private void frmPreview_Shown(object sender, EventArgs e)
	{
		string strCheckRltMsg = "";
		if (m_bCheckFieldExceedMaxLenRlt)
		{
			strCheckRltMsg = m_strCheckFieldExceedMaxLenMsg;
		}
		string strCheckPrintParamMsg = "";
		bool bCheckPrintParamRlt = CheckPrintParam(ref strCheckPrintParamMsg);
		string strSplitLine = "";
		if (m_bCheckFieldExceedMaxLenRlt && !bCheckPrintParamRlt)
		{
			int iPadLen = 0;
			iPadLen = ((m_strCheckFieldExceedMaxLenMsg.Length < strCheckPrintParamMsg.Length) ? strCheckPrintParamMsg.Length : m_strCheckFieldExceedMaxLenMsg.Length);
			strSplitLine = "\n";
			while (iPadLen-- > 0)
			{
				strSplitLine += "-";
			}
			strSplitLine += "\n\n";
		}
		if (!bCheckPrintParamRlt)
		{
			strCheckRltMsg = strCheckRltMsg + strSplitLine + strCheckPrintParamMsg + "\n";
		}
		if (m_bCheckFieldExceedMaxLenRlt || !bCheckPrintParamRlt)
		{
			ClsMsgBox.MessageBoxShowDefWarn(this, strCheckRltMsg);
		}
		labelReadyPreviewTip.Visible = false;
		labelReadyPreviewTip.Parent.Refresh();
	}

	private void pictureBoxPreview_MouseMove(object sender, MouseEventArgs e)
	{
		pictureBoxPreview.Refresh();
		Graphics g = pictureBoxPreview.CreateGraphics();
		Pen pen = new Pen(new SolidBrush(Color.Blue), 2f);
		g.DrawLines(pen, new Point[3]
		{
			new Point(0, e.Y),
			new Point(e.X, e.Y),
			new Point(e.X, 0)
		});
		ShowCurrentCoordinate(e.X, e.Y);
	}

	private void pictureBoxPreview_MouseLeave(object sender, EventArgs e)
	{
		pictureBoxPreview.Refresh();
	}

	public void ShowCurrentCoordinate(int x_pixel, int y_pixel)
	{
		double dZoomScale = (double)trackBarZoom.Value / 100.0;
		double x_pixel_real = (double)x_pixel / dZoomScale;
		double y_pixel_real = (double)y_pixel / dZoomScale;
		double dPixelPerMillimeter_Width = (double)m_iWidthDPI / m_dInchToMM_Factor;
		double dPixelPerMillimeter_Height = (double)m_iHeightDPI / m_dInchToMM_Factor;
		double x_mm_real = x_pixel_real / dPixelPerMillimeter_Width;
		double y_mm_real = y_pixel_real / dPixelPerMillimeter_Height;
		labelX.Text = Convert.ToInt32(x_mm_real).ToString();
		labelY.Text = Convert.ToInt32(y_mm_real).ToString();
	}

	private void groupBoxCoordinate_Paint(object sender, PaintEventArgs e)
	{
		ClsShare.RepainGroupBoxSide(sender, e, this);
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
		this.pnlClientPreview = new System.Windows.Forms.Panel();
		this.pnlClientClient = new System.Windows.Forms.Panel();
		this.pnlClientClientClientShow = new System.Windows.Forms.Panel();
		this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
		this.pnlClientClientLeftHeightRuler = new System.Windows.Forms.Panel();
		this.pnlClientClientTop = new System.Windows.Forms.Panel();
		this.pnlClientClientTopClientWidthRuler = new System.Windows.Forms.Panel();
		this.pnlClientClientTopLeftTip = new System.Windows.Forms.Panel();
		this.lblRulerTip = new System.Windows.Forms.Label();
		this.lblZoom = new System.Windows.Forms.Label();
		this.groupBoxZoom = new System.Windows.Forms.GroupBox();
		this.trackBarZoom = new System.Windows.Forms.TrackBar();
		this.groupBoxCoordinate = new System.Windows.Forms.GroupBox();
		this.label4 = new System.Windows.Forms.Label();
		this.labelY = new System.Windows.Forms.Label();
		this.labelX = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		base.pnlClientData.SuspendLayout();
		base.pnlBottomCtrl.SuspendLayout();
		this.pnlClientPreview.SuspendLayout();
		this.pnlClientClient.SuspendLayout();
		this.pnlClientClientClientShow.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBoxPreview).BeginInit();
		this.pnlClientClientTop.SuspendLayout();
		this.pnlClientClientTopLeftTip.SuspendLayout();
		this.groupBoxZoom.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackBarZoom).BeginInit();
		this.groupBoxCoordinate.SuspendLayout();
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.pnlClientPreview);
		base.pnlClientData.Size = new System.Drawing.Size(915, 316);
		base.pnlBottomCtrl.Controls.Add(this.groupBoxCoordinate);
		base.pnlBottomCtrl.Controls.Add(this.groupBoxZoom);
		base.pnlBottomCtrl.Location = new System.Drawing.Point(0, 316);
		this.pnlClientPreview.AutoScroll = true;
		this.pnlClientPreview.Controls.Add(this.pnlClientClient);
		this.pnlClientPreview.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientPreview.Location = new System.Drawing.Point(0, 0);
		this.pnlClientPreview.Name = "pnlClientPreview";
		this.pnlClientPreview.Size = new System.Drawing.Size(911, 312);
		this.pnlClientPreview.TabIndex = 0;
		this.pnlClientClient.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlClientClient.Controls.Add(this.pnlClientClientClientShow);
		this.pnlClientClient.Controls.Add(this.pnlClientClientLeftHeightRuler);
		this.pnlClientClient.Controls.Add(this.pnlClientClientTop);
		this.pnlClientClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientClient.Location = new System.Drawing.Point(0, 0);
		this.pnlClientClient.Name = "pnlClientClient";
		this.pnlClientClient.Size = new System.Drawing.Size(911, 312);
		this.pnlClientClient.TabIndex = 3;
		this.pnlClientClientClientShow.AutoScroll = true;
		this.pnlClientClientClientShow.Controls.Add(this.pictureBoxPreview);
		this.pnlClientClientClientShow.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientClientClientShow.Location = new System.Drawing.Point(78, 61);
		this.pnlClientClientClientShow.Name = "pnlClientClientClientShow";
		this.pnlClientClientClientShow.Size = new System.Drawing.Size(829, 247);
		this.pnlClientClientClientShow.TabIndex = 4;
		this.pnlClientClientClientShow.Scroll += new System.Windows.Forms.ScrollEventHandler(pnlClientClientClientShow_Scroll);
		this.pictureBoxPreview.Location = new System.Drawing.Point(0, 0);
		this.pictureBoxPreview.Name = "pictureBoxPreview";
		this.pictureBoxPreview.Size = new System.Drawing.Size(772, 196);
		this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureBoxPreview.TabIndex = 0;
		this.pictureBoxPreview.TabStop = false;
		this.pictureBoxPreview.MouseLeave += new System.EventHandler(pictureBoxPreview_MouseLeave);
		this.pictureBoxPreview.MouseMove += new System.Windows.Forms.MouseEventHandler(pictureBoxPreview_MouseMove);
		this.pnlClientClientLeftHeightRuler.BackColor = System.Drawing.Color.White;
		this.pnlClientClientLeftHeightRuler.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pnlClientClientLeftHeightRuler.Dock = System.Windows.Forms.DockStyle.Left;
		this.pnlClientClientLeftHeightRuler.Location = new System.Drawing.Point(0, 61);
		this.pnlClientClientLeftHeightRuler.Name = "pnlClientClientLeftHeightRuler";
		this.pnlClientClientLeftHeightRuler.Size = new System.Drawing.Size(78, 247);
		this.pnlClientClientLeftHeightRuler.TabIndex = 1;
		this.pnlClientClientTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pnlClientClientTop.Controls.Add(this.pnlClientClientTopClientWidthRuler);
		this.pnlClientClientTop.Controls.Add(this.pnlClientClientTopLeftTip);
		this.pnlClientClientTop.Dock = System.Windows.Forms.DockStyle.Top;
		this.pnlClientClientTop.Location = new System.Drawing.Point(0, 0);
		this.pnlClientClientTop.Name = "pnlClientClientTop";
		this.pnlClientClientTop.Size = new System.Drawing.Size(907, 61);
		this.pnlClientClientTop.TabIndex = 0;
		this.pnlClientClientTopClientWidthRuler.BackColor = System.Drawing.Color.White;
		this.pnlClientClientTopClientWidthRuler.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlClientClientTopClientWidthRuler.Location = new System.Drawing.Point(78, 0);
		this.pnlClientClientTopClientWidthRuler.Name = "pnlClientClientTopClientWidthRuler";
		this.pnlClientClientTopClientWidthRuler.Size = new System.Drawing.Size(827, 59);
		this.pnlClientClientTopClientWidthRuler.TabIndex = 1;
		this.pnlClientClientTopLeftTip.BackColor = System.Drawing.Color.White;
		this.pnlClientClientTopLeftTip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pnlClientClientTopLeftTip.Controls.Add(this.lblRulerTip);
		this.pnlClientClientTopLeftTip.Dock = System.Windows.Forms.DockStyle.Left;
		this.pnlClientClientTopLeftTip.Location = new System.Drawing.Point(0, 0);
		this.pnlClientClientTopLeftTip.Name = "pnlClientClientTopLeftTip";
		this.pnlClientClientTopLeftTip.Size = new System.Drawing.Size(78, 59);
		this.pnlClientClientTopLeftTip.TabIndex = 0;
		this.lblRulerTip.AutoSize = true;
		this.lblRulerTip.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblRulerTip.Location = new System.Drawing.Point(1, 1);
		this.lblRulerTip.Name = "lblRulerTip";
		this.lblRulerTip.Size = new System.Drawing.Size(31, 14);
		this.lblRulerTip.TabIndex = 0;
		this.lblRulerTip.Text = "Tip";
		this.lblZoom.AutoSize = true;
		this.lblZoom.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblZoom.Location = new System.Drawing.Point(204, 35);
		this.lblZoom.Name = "lblZoom";
		this.lblZoom.Size = new System.Drawing.Size(77, 29);
		this.lblZoom.TabIndex = 13;
		this.lblZoom.Text = "100%";
		this.lblZoom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.groupBoxZoom.Controls.Add(this.lblZoom);
		this.groupBoxZoom.Controls.Add(this.trackBarZoom);
		this.groupBoxZoom.Font = new System.Drawing.Font("宋体", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.groupBoxZoom.Location = new System.Drawing.Point(13, 14);
		this.groupBoxZoom.Name = "groupBoxZoom";
		this.groupBoxZoom.Size = new System.Drawing.Size(310, 84);
		this.groupBoxZoom.TabIndex = 14;
		this.groupBoxZoom.TabStop = false;
		this.groupBoxZoom.Text = "縮放";
		this.groupBoxZoom.Paint += new System.Windows.Forms.PaintEventHandler(groupBoxZoom_Paint);
		this.trackBarZoom.BackColor = System.Drawing.Color.WhiteSmoke;
		this.trackBarZoom.Location = new System.Drawing.Point(26, 27);
		this.trackBarZoom.Maximum = 50;
		this.trackBarZoom.Minimum = 1;
		this.trackBarZoom.Name = "trackBarZoom";
		this.trackBarZoom.Size = new System.Drawing.Size(170, 45);
		this.trackBarZoom.TabIndex = 12;
		this.trackBarZoom.TickFrequency = 10;
		this.trackBarZoom.TickStyle = System.Windows.Forms.TickStyle.Both;
		this.trackBarZoom.Value = 50;
		this.trackBarZoom.Scroll += new System.EventHandler(trackBarZoom_Scroll);
		this.groupBoxCoordinate.Controls.Add(this.label4);
		this.groupBoxCoordinate.Controls.Add(this.labelY);
		this.groupBoxCoordinate.Controls.Add(this.labelX);
		this.groupBoxCoordinate.Controls.Add(this.label1);
		this.groupBoxCoordinate.Font = new System.Drawing.Font("宋体", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.groupBoxCoordinate.Location = new System.Drawing.Point(362, 14);
		this.groupBoxCoordinate.Name = "groupBoxCoordinate";
		this.groupBoxCoordinate.Size = new System.Drawing.Size(310, 84);
		this.groupBoxCoordinate.TabIndex = 15;
		this.groupBoxCoordinate.TabStop = false;
		this.groupBoxCoordinate.Text = "座標(單位:mm)";
		this.groupBoxCoordinate.Paint += new System.Windows.Forms.PaintEventHandler(groupBoxCoordinate_Paint);
		this.label4.AutoSize = true;
		this.label4.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label4.Location = new System.Drawing.Point(20, 35);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(45, 29);
		this.label4.TabIndex = 16;
		this.label4.Text = "X:";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.labelY.AutoSize = true;
		this.labelY.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.labelY.Location = new System.Drawing.Point(211, 35);
		this.labelY.Name = "labelY";
		this.labelY.Size = new System.Drawing.Size(29, 29);
		this.labelY.TabIndex = 15;
		this.labelY.Text = "0";
		this.labelY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.labelX.AutoSize = true;
		this.labelX.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.labelX.Location = new System.Drawing.Point(71, 35);
		this.labelX.Name = "labelX";
		this.labelX.Size = new System.Drawing.Size(29, 29);
		this.labelX.TabIndex = 14;
		this.labelX.Text = "0";
		this.labelX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("宋体", 21.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label1.Location = new System.Drawing.Point(160, 35);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(45, 29);
		this.label1.TabIndex = 13;
		this.label1.Text = "Y:";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(919, 435);
		base.MinimizeBox = false;
		base.Name = "frmPreviewRealResult";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "預覽";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmPreview_FormClosing);
		base.Load += new System.EventHandler(frmPreview_Load);
		base.Shown += new System.EventHandler(frmPreview_Shown);
		base.Resize += new System.EventHandler(frmPreviewRealResult_Resize);
		base.pnlClientData.ResumeLayout(false);
		base.pnlBottomCtrl.ResumeLayout(false);
		this.pnlClientPreview.ResumeLayout(false);
		this.pnlClientClient.ResumeLayout(false);
		this.pnlClientClientClientShow.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.pictureBoxPreview).EndInit();
		this.pnlClientClientTop.ResumeLayout(false);
		this.pnlClientClientTopLeftTip.ResumeLayout(false);
		this.pnlClientClientTopLeftTip.PerformLayout();
		this.groupBoxZoom.ResumeLayout(false);
		this.groupBoxZoom.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackBarZoom).EndInit();
		this.groupBoxCoordinate.ResumeLayout(false);
		this.groupBoxCoordinate.PerformLayout();
		base.ResumeLayout(false);
	}
}
