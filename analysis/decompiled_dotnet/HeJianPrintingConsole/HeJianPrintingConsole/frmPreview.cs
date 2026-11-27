using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SpCommon;

namespace HeJianPrintingConsole;

public class frmPreview : frmBase
{
	private ClsCartonSizeFieldValue m_ClsCSFV;

	private Bitmap m_bmpPreview;

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

	public string m_strCaseNo { get; set; }

	public string m_strMsgFilePath { get; set; }

	public string m_strCheckFieldExceedMaxLenMsg { get; set; }

	public bool m_bCheckFieldExceedMaxLenRlt { get; set; }

	public Label labelReadyPreviewTip { get; set; }

	public frmPreview()
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
		string strZoomScale = trackBarZoom.Value + "%";
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
		int iWidthRulerLen_pixel = Convert.ToInt32((double)(m_iWidthMaxLen_mm * m_iWidthDPI) / 25.4);
		int iHeightRulerLen_pixel = Convert.ToInt32((double)(m_iHeightMaxLen_mm * m_iHeightDPI) / 25.4);
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
		double dZoom = (double)trackBarZoom.Value / 100.0;
		ShowPreviewBmp(m_bmpPreview, dZoom, dZoom);
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
			string strZoomScale = tbTmp.Value + "%";
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
			string cartonsizeLog = "GetCartonSize frmPreivew 紙箱規格:";
			for (int iIndex = 0; iIndex < dt.Columns.Count; iIndex++)
			{
				string strCaption = dt.Columns[iIndex].Caption.Substring(2).Trim().ToUpper();
				string strValue = dt.Rows[0][iIndex].ToString();
				cartonsizeLog = cartonsizeLog + strCaption + strValue + ";  ";
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
			int iPreviewBmpWidth_mm = Convert.ToInt32((double)(m_bmpPreview.Width / m_iWidthDPIOriginal) * 25.4);
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
			double dFrontMsgWidth_mm = Convert.ToInt32((double)(ClsMsg.GetMsgWidth(ClsXml.GetSourceMsg1Path()) / m_iWidthDPIOriginal) * 25.4);
			double dSideMsgWidth_mm = Convert.ToInt32((double)(ClsMsg.GetMsgWidth(ClsXml.GetSourceMsg2Path()) / m_iWidthDPIOriginal) * 25.4);
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
		base.SuspendLayout();
		base.pnlClientData.Controls.Add(this.pnlClientPreview);
		base.pnlClientData.Size = new System.Drawing.Size(915, 316);
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
		this.trackBarZoom.Maximum = 100;
		this.trackBarZoom.Minimum = 1;
		this.trackBarZoom.Name = "trackBarZoom";
		this.trackBarZoom.Size = new System.Drawing.Size(170, 45);
		this.trackBarZoom.TabIndex = 12;
		this.trackBarZoom.TickFrequency = 10;
		this.trackBarZoom.TickStyle = System.Windows.Forms.TickStyle.Both;
		this.trackBarZoom.Value = 100;
		this.trackBarZoom.Scroll += new System.EventHandler(trackBarZoom_Scroll);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.ClientSize = new System.Drawing.Size(919, 435);
		base.MinimizeBox = false;
		base.Name = "frmPreview";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "預覽";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmPreview_FormClosing);
		base.Load += new System.EventHandler(frmPreview_Load);
		base.Shown += new System.EventHandler(frmPreview_Shown);
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
		base.ResumeLayout(false);
	}
}
