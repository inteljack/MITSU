using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace SpCommon;

internal class ClsExcel
{
	public static DataTable GetDataTableFromExcel(string XLSpath, int iSheetIndex = 0, bool SetFirtRowAsColumnName = true, int colCount = 0)
	{
		DataTable dt = new DataTable();
		using FileStream fs = new FileStream(XLSpath, FileMode.Open);
		HSSFWorkbook hssfworkbook = new HSSFWorkbook(fs);
		HSSFSheet sheet = (HSSFSheet)hssfworkbook.GetSheetAt(iSheetIndex);
		IEnumerator rows = sheet.GetRowEnumerator();
		rows.MoveNext();
		int cols = ((colCount > 0) ? colCount : ((HSSFRow)rows.Current).Cells.Count);
		rows.Reset();
		rows.MoveNext();
		for (int j = 0; j < cols; j++)
		{
			if (SetFirtRowAsColumnName)
			{
				HSSFCell cell = (HSSFCell)((HSSFRow)rows.Current).GetCell(j);
				if (cell != null)
				{
					dt.Columns.Add(cell.ToString());
				}
				else
				{
					dt.Columns.Add(Convert.ToChar(65 + j).ToString());
				}
			}
			else
			{
				dt.Columns.Add(Convert.ToChar(65 + j).ToString());
			}
		}
		rows.Reset();
		if (SetFirtRowAsColumnName)
		{
			rows.MoveNext();
		}
		while (rows.MoveNext())
		{
			HSSFRow row = (HSSFRow)rows.Current;
			DataRow dr = dt.NewRow();
			for (int i = 0; i < dt.Columns.Count; i++)
			{
				HSSFCell cell2 = (HSSFCell)row.GetCell(i);
				if (cell2 == null)
				{
					dr[i] = null;
				}
				else
				{
					dr[i] = cell2.ToString();
				}
			}
			dt.Rows.Add(dr);
		}
		return dt;
	}

	public static void ExportExcel(DataGridView dgvSrc, string strDestPath, string strSheetName)
	{
		HSSFWorkbook hssfworkbook = new HSSFWorkbook();
		ISheet sheet1 = hssfworkbook.CreateSheet(strSheetName);
		IRow RowHead = sheet1.CreateRow(0);
		for (int iColumnIndex = 0; iColumnIndex < dgvSrc.Columns.Count; iColumnIndex++)
		{
			RowHead.CreateCell(iColumnIndex).SetCellValue(dgvSrc.Columns[iColumnIndex].HeaderText);
		}
		for (int iRowIndex = 0; iRowIndex < dgvSrc.Rows.Count; iRowIndex++)
		{
			IRow RowBody = sheet1.CreateRow(iRowIndex + 1);
			for (int i = 0; i < dgvSrc.Columns.Count; i++)
			{
				RowBody.CreateCell(i).SetCellValue(dgvSrc.Rows[iRowIndex].Cells[i].Value.ToString());
				sheet1.AutoSizeColumn(i);
			}
		}
		using FileStream file = new FileStream(strDestPath, FileMode.Create);
		hssfworkbook.Write(file);
		file.Close();
	}

	public static void ExportExcel(DataTable dtSrc, string strDestPath, string strSheetName)
	{
		HSSFWorkbook hssfworkbook = null;
		hssfworkbook = ((!File.Exists(strDestPath)) ? new HSSFWorkbook() : new HSSFWorkbook(new FileStream(strDestPath, FileMode.Open)));
		ISheet sheet1 = null;
		sheet1 = hssfworkbook.GetSheet(strSheetName);
		IRow RowHead = null;
		if (sheet1 == null)
		{
			int iSheetCount = hssfworkbook.NumberOfSheets;
			if (iSheetCount > 0)
			{
				sheet1 = hssfworkbook.GetSheetAt(0);
			}
			if (sheet1 != null)
			{
				hssfworkbook.SetSheetName(hssfworkbook.GetSheetIndex(sheet1), strSheetName);
			}
			else
			{
				sheet1 = hssfworkbook.CreateSheet(strSheetName);
			}
		}
		int iRowsCount = sheet1.PhysicalNumberOfRows;
		for (int iRowIndex = 0; iRowIndex < iRowsCount; iRowIndex++)
		{
			sheet1.RemoveRow(sheet1.GetRow(iRowIndex));
		}
		RowHead = sheet1.GetRow(sheet1.FirstRowNum);
		if (RowHead == null)
		{
			RowHead = sheet1.CreateRow(sheet1.FirstRowNum);
		}
		for (int iColumnIndex = 0; iColumnIndex < dtSrc.Columns.Count; iColumnIndex++)
		{
			RowHead.CreateCell(iColumnIndex).SetCellValue(dtSrc.Columns[iColumnIndex].Caption);
		}
		for (int i = 0; i < dtSrc.Rows.Count; i++)
		{
			IRow RowBody = sheet1.CreateRow(sheet1.LastRowNum + 1);
			for (int j = 0; j < dtSrc.Columns.Count; j++)
			{
				RowBody.CreateCell(j).SetCellValue(dtSrc.Rows[i][j].ToString());
			}
		}
		using FileStream file = new FileStream(strDestPath, FileMode.Create);
		hssfworkbook.Write(file);
		file.Close();
	}

	public static void CreateBlankExcel(string strDestPath)
	{
		using FileStream file = new FileStream(strDestPath, FileMode.Create);
		HSSFWorkbook hssfworkbook = new HSSFWorkbook();
		hssfworkbook.Write(file);
		file.Close();
	}

	public static bool ExportExcelFromDataGridView(DataGridView dgvSrc, string strDestFilePath)
	{
		Stream myStream = new FileStream(strDestFilePath, FileMode.Create, FileAccess.Write);
		StreamWriter sw = new StreamWriter(myStream, Encoding.GetEncoding(0));
		string str = "";
		try
		{
			for (int i = 0; i < dgvSrc.ColumnCount; i++)
			{
				if (i > 0)
				{
					str += "\t";
				}
				str += dgvSrc.Columns[i].HeaderText;
			}
			sw.WriteLine(str);
			for (int j = 0; j < dgvSrc.Rows.Count; j++)
			{
				string tempStr = "";
				for (int k = 0; k < dgvSrc.Columns.Count; k++)
				{
					if (k > 0)
					{
						tempStr += "\t";
					}
					tempStr += dgvSrc.Rows[j].Cells[k].Value.ToString();
				}
				sw.WriteLine(tempStr);
			}
			sw.Close();
			myStream.Close();
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString());
			return false;
		}
		finally
		{
			sw.Close();
			myStream.Close();
		}
	}

	public static void DelSpaceColumn(ref DataTable dt)
	{
		string strSpaceColumnCaption = "COLUMN";
		int iSpaceColumnIndex = 1;
		int iColumnIndex = 0;
		while (iColumnIndex < dt.Columns.Count)
		{
			string strCaption = dt.Columns[iColumnIndex].Caption.Trim().ToUpper();
			if (strCaption == strSpaceColumnCaption + iSpaceColumnIndex)
			{
				dt.Columns.RemoveAt(iColumnIndex);
				iSpaceColumnIndex++;
			}
			else
			{
				iColumnIndex++;
			}
		}
	}

	public static void DelSpaceRow(int iCheckColumnIsSpaceIndex, ref DataTable dt)
	{
		int iRowIndex = 0;
		while (iRowIndex < dt.Rows.Count)
		{
			string strKeyCode = dt.Rows[iRowIndex][iCheckColumnIsSpaceIndex].ToString().Trim().ToUpper();
			if (string.IsNullOrWhiteSpace(strKeyCode) || string.IsNullOrEmpty(strKeyCode))
			{
				dt.Rows.RemoveAt(iRowIndex);
			}
			else
			{
				iRowIndex++;
			}
		}
	}
}
