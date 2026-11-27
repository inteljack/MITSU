using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace SpCommon;

internal class ClsPic
{
	public enum EnumBmpMemberOffset
	{
		BMP_OFFSET_SIGN = 0,
		BMP_OFFSET_FILESIZE = 2,
		BMP_OFFSET_RESERVED1 = 6,
		BMP_OFFSET_RESERVED2 = 8,
		BMP_OFFSET_HEAD2DATA = 10,
		BMP_OFFSET_INFOHEADER = 14,
		BMP_OFFSET_WIDTH = 18,
		BMP_OFFSET_HEIGHT = 22,
		BMP_OFFSET_PLANES = 26,
		BMP_OFFSET_BITCOUNTE = 28,
		BMP_OFFSET_COMPRESSION = 30,
		BMP_OFFSET_IMAGESIZE = 34,
		BMP_OFFSET_XPPM = 38,
		BMP_OFFSET_YPPM = 42,
		BMP_OFFSET_COLORUSED = 46,
		BMP_OFFSET_COLORIMPORTANT = 46,
		BMP_OFFSET_FG_COLOR = 54,
		BMP_OFFSET_BG_COLOR = 58
	}

	public static void PicToBwBmp(string strSrc, string strDest)
	{
		Image img = Image.FromFile(strSrc);
		ImageAttributes ta = new ImageAttributes();
		float[][] mm = new float[5][]
		{
			new float[5] { 0.4f, 0.4f, 0.4f, 0f, 0f },
			new float[5] { 0.3f, 0.3f, 0.3f, 0f, 0f },
			new float[5] { 0.3f, 0.3f, 0.3f, 0f, 0f },
			new float[5] { 0f, 0f, 0f, 1f, 0f },
			new float[5] { 0f, 0f, 0f, 0f, 1f }
		};
		ColorMatrix cmt = new ColorMatrix(mm);
		ta.SetColorMatrix(cmt);
		Bitmap bmp = new Bitmap(img.Width, img.Height);
		Graphics g = Graphics.FromImage(bmp);
		g.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ta);
		g.Dispose();
		int midrgb = Color.FromArgb(128, 128, 128).ToArgb();
		int stride = ((bmp.Width % 8 == 0) ? (bmp.Width / 8) : (bmp.Width / 8 + 1));
		stride = ((stride % 4 == 0) ? stride : ((stride / 4 + 1) * 4));
		int k = bmp.Height * stride;
		byte[] buf = new byte[k];
		for (int j = 0; j < bmp.Height; j++)
		{
			k = j * stride;
			int x = 0;
			int ab = 0;
			for (int i = 0; i < bmp.Width; i++)
			{
				ab = ((bmp.GetPixel(i, j).ToArgb() <= midrgb) ? (ab * 2) : (ab * 2 + 1));
				x++;
				if (x == 8)
				{
					buf[k++] = (byte)ab;
					ab = 0;
					x = 0;
				}
			}
			if (x > 0)
			{
				for (int t = x; t < 8; t++)
				{
					ab *= 2;
				}
				buf[k++] = (byte)ab;
			}
		}
		Bitmap bb = new Bitmap(img.Width, img.Height, PixelFormat.Format1bppIndexed);
		BitmapData dt = bb.LockBits(new Rectangle(0, 0, bb.Width, bb.Height), ImageLockMode.ReadWrite, bb.PixelFormat);
		Marshal.Copy(buf, 0, dt.Scan0, buf.Length);
		bb.UnlockBits(dt);
		bb.Save(strDest, ImageFormat.Bmp);
		bb.Dispose();
		bmp.Dispose();
		img.Dispose();
	}

	public static void PicToBwBmp(Bitmap bmpSrc, ref Bitmap bmpDest)
	{
		ImageAttributes ta = new ImageAttributes();
		float[][] mm = new float[5][]
		{
			new float[5] { 0.4f, 0.4f, 0.4f, 0f, 0f },
			new float[5] { 0.3f, 0.3f, 0.3f, 0f, 0f },
			new float[5] { 0.3f, 0.3f, 0.3f, 0f, 0f },
			new float[5] { 0f, 0f, 0f, 1f, 0f },
			new float[5] { 0f, 0f, 0f, 0f, 1f }
		};
		ColorMatrix cmt = new ColorMatrix(mm);
		ta.SetColorMatrix(cmt);
		Bitmap bmp = new Bitmap(bmpSrc.Width, bmpSrc.Height);
		Graphics g = Graphics.FromImage(bmp);
		g.Clear(Color.White);
		g.DrawImage(bmpSrc, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmpSrc.Width, bmpSrc.Height, GraphicsUnit.Pixel, ta);
		g.Dispose();
		int midrgb = Color.FromArgb(128, 128, 128).ToArgb();
		int stride = ((bmp.Width % 8 == 0) ? (bmp.Width / 8) : (bmp.Width / 8 + 1));
		stride = ((stride % 4 == 0) ? stride : ((stride / 4 + 1) * 4));
		int k = bmp.Height * stride;
		byte[] buf = new byte[k];
		for (int j = 0; j < bmp.Height; j++)
		{
			k = j * stride;
			int x = 0;
			int ab = 0;
			for (int i = 0; i < bmp.Width; i++)
			{
				ab = ((bmp.GetPixel(i, j).ToArgb() <= midrgb) ? (ab * 2) : (ab * 2 + 1));
				x++;
				if (x == 8)
				{
					buf[k++] = (byte)ab;
					ab = 0;
					x = 0;
				}
			}
			if (x > 0)
			{
				for (int t = x; t < 8; t++)
				{
					ab *= 2;
				}
				buf[k++] = (byte)ab;
			}
		}
		bmpDest = new Bitmap(bmpSrc.Width, bmpSrc.Height, PixelFormat.Format1bppIndexed);
		BitmapData dt = bmpDest.LockBits(new Rectangle(0, 0, bmpDest.Width, bmpDest.Height), ImageLockMode.ReadWrite, bmpDest.PixelFormat);
		Marshal.Copy(buf, 0, dt.Scan0, buf.Length);
		bmpDest.UnlockBits(dt);
		bmp.Dispose();
		bmpSrc.Dispose();
	}

	public static void PicToOppPhaBmp(string strSrc, string strDest)
	{
		Image img = Image.FromFile(strSrc);
		ImageAttributes ta = new ImageAttributes();
		float[][] mm2 = new float[5][]
		{
			new float[5] { -0.4f, -0.4f, -0.4f, 0f, 0f },
			new float[5] { -0.3f, -0.3f, -0.3f, 0f, 0f },
			new float[5] { -0.3f, -0.3f, -0.3f, 0f, 0f },
			new float[5] { 1f, 1f, 1f, 1f, 0f },
			new float[5] { 0f, 0f, 0f, 0f, 1f }
		};
		ColorMatrix cmt = new ColorMatrix(mm2);
		ta.SetColorMatrix(cmt);
		Bitmap bmp = new Bitmap(img.Width, img.Height);
		Graphics g = Graphics.FromImage(bmp);
		g.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ta);
		g.Dispose();
		int midrgb = Color.FromArgb(128, 128, 128).ToArgb();
		int stride = ((bmp.Width % 8 == 0) ? (bmp.Width / 8) : (bmp.Width / 8 + 1));
		stride = ((stride % 4 == 0) ? stride : ((stride / 4 + 1) * 4));
		int k = bmp.Height * stride;
		byte[] buf = new byte[k];
		for (int j = 0; j < bmp.Height; j++)
		{
			k = j * stride;
			int x = 0;
			int ab = 0;
			for (int i = 0; i < bmp.Width; i++)
			{
				ab = ((bmp.GetPixel(i, j).ToArgb() <= midrgb) ? (ab * 2) : (ab * 2 + 1));
				x++;
				if (x == 8)
				{
					buf[k++] = (byte)ab;
					ab = 0;
					x = 0;
				}
			}
			if (x > 0)
			{
				for (int t = x; t < 8; t++)
				{
					ab *= 2;
				}
				buf[k++] = (byte)ab;
			}
		}
		Bitmap bb = new Bitmap(img.Width, img.Height, PixelFormat.Format1bppIndexed);
		BitmapData dt = bb.LockBits(new Rectangle(0, 0, bb.Width, bb.Height), ImageLockMode.ReadWrite, bb.PixelFormat);
		Marshal.Copy(buf, 0, dt.Scan0, buf.Length);
		bb.UnlockBits(dt);
		bb.Save(strDest, ImageFormat.Bmp);
		bb.Dispose();
		bmp.Dispose();
		img.Dispose();
	}

	public static void PicToOppPhaBmp(Bitmap bmpSrc, ref Bitmap bmpDest)
	{
		ImageAttributes ta = new ImageAttributes();
		float[][] mm2 = new float[5][]
		{
			new float[5] { -0.4f, -0.4f, -0.4f, 0f, 0f },
			new float[5] { -0.3f, -0.3f, -0.3f, 0f, 0f },
			new float[5] { -0.3f, -0.3f, -0.3f, 0f, 0f },
			new float[5] { 1f, 1f, 1f, 1f, 0f },
			new float[5] { 0f, 0f, 0f, 0f, 1f }
		};
		ColorMatrix cmt = new ColorMatrix(mm2);
		ta.SetColorMatrix(cmt);
		Bitmap bmp = new Bitmap(bmpSrc.Width, bmpSrc.Height);
		Graphics g = Graphics.FromImage(bmp);
		g.DrawImage(bmpSrc, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmpSrc.Width, bmpSrc.Height, GraphicsUnit.Pixel, ta);
		g.Dispose();
		int midrgb = Color.FromArgb(128, 128, 128).ToArgb();
		int stride = ((bmp.Width % 8 == 0) ? (bmp.Width / 8) : (bmp.Width / 8 + 1));
		stride = ((stride % 4 == 0) ? stride : ((stride / 4 + 1) * 4));
		int k = bmp.Height * stride;
		byte[] buf = new byte[k];
		for (int j = 0; j < bmp.Height; j++)
		{
			k = j * stride;
			int x = 0;
			int ab = 0;
			for (int i = 0; i < bmp.Width; i++)
			{
				ab = ((bmp.GetPixel(i, j).ToArgb() <= midrgb) ? (ab * 2) : (ab * 2 + 1));
				x++;
				if (x == 8)
				{
					buf[k++] = (byte)ab;
					ab = 0;
					x = 0;
				}
			}
			if (x > 0)
			{
				for (int t = x; t < 8; t++)
				{
					ab *= 2;
				}
				buf[k++] = (byte)ab;
			}
		}
		bmpDest = new Bitmap(bmpSrc.Width, bmpSrc.Height, PixelFormat.Format1bppIndexed);
		BitmapData dt = bmpDest.LockBits(new Rectangle(0, 0, bmpDest.Width, bmpDest.Height), ImageLockMode.ReadWrite, bmpDest.PixelFormat);
		Marshal.Copy(buf, 0, dt.Scan0, buf.Length);
		bmpDest.UnlockBits(dt);
		bmp.Dispose();
		bmpSrc.Dispose();
	}

	public static Bitmap CutPicAroundSpace(Bitmap bmpSrc)
	{
		try
		{
			int iBottomSpacePixel = GetBottomSpace(bmpSrc);
			bmpSrc.RotateFlip(RotateFlipType.Rotate90FlipNone);
			int iRightSpacePixel = GetBottomSpace(bmpSrc);
			bmpSrc.RotateFlip(RotateFlipType.Rotate90FlipNone);
			int iTopSpacePixel = GetBottomSpace(bmpSrc);
			bmpSrc.RotateFlip(RotateFlipType.Rotate90FlipNone);
			int iLeftSpacePixel = GetBottomSpace(bmpSrc);
			bmpSrc.RotateFlip(RotateFlipType.Rotate90FlipNone);
			Bitmap bmpDest = new Bitmap(bmpSrc.Width - iRightSpacePixel - iLeftSpacePixel, bmpSrc.Height - iBottomSpacePixel - iTopSpacePixel);
			Graphics g = Graphics.FromImage(bmpDest);
			g.Clear(Color.White);
			g.DrawImage(bmpSrc, -iLeftSpacePixel, -iTopSpacePixel);
			return bmpDest;
		}
		catch
		{
			return bmpSrc;
		}
	}

	private static int GetBottomSpace(Bitmap bmpSrc)
	{
		MemoryStream msSrc = new MemoryStream();
		bmpSrc.Save(msSrc, ImageFormat.Bmp);
		msSrc.Position = 0L;
		msSrc.Position = 10L;
		int iDataStartPos = msSrc.ReadByte();
		msSrc.Position = iDataStartPos;
		int iBpp = 1;
		int iByteCountPerLine = 4 * ((iBpp * bmpSrc.Width + 31) / 32);
		int iByteCountPerLineSavePixel = (bmpSrc.Width + 7) / 8;
		int iAddBitCountPerLineSavePixel = iByteCountPerLineSavePixel * 8 - bmpSrc.Width;
		int iBottomSpacePixel = 0;
		for (int iY = 0; iY < bmpSrc.Height; iY++)
		{
			byte[] byteArrPerLine = new byte[iByteCountPerLine];
			msSrc.Read(byteArrPerLine, 0, iByteCountPerLine);
			bool bSkipOutLoop = false;
			for (int iByteIndexPerLine = 0; iByteIndexPerLine < iByteCountPerLine; iByteIndexPerLine++)
			{
				int iTmp = Convert.ToInt32(byteArrPerLine[iByteIndexPerLine]);
				if (iByteIndexPerLine < iByteCountPerLineSavePixel - 1)
				{
					if ((iTmp & 0xFF) != 255)
					{
						bSkipOutLoop = true;
						break;
					}
					continue;
				}
				if (iByteIndexPerLine != iByteCountPerLineSavePixel - 1)
				{
					break;
				}
				int iAddBitCountToIntValue = (255 << iAddBitCountPerLineSavePixel) & 0xFF;
				if ((iTmp & iAddBitCountToIntValue) != iAddBitCountToIntValue)
				{
					bSkipOutLoop = true;
					break;
				}
			}
			if (bSkipOutLoop)
			{
				break;
			}
			iBottomSpacePixel++;
		}
		return iBottomSpacePixel;
	}
}
