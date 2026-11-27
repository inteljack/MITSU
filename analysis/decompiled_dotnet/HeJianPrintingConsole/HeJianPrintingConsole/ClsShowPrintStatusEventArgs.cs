using System;
using System.Drawing;

namespace HeJianPrintingConsole;

public class ClsShowPrintStatusEventArgs : EventArgs
{
	public bool bFormVisible { get; set; }

	public string strHasPrintBoxCount { get; set; }

	public string strAllPrintBoxCount { get; set; }

	public Color printStatusLedColor { get; set; }

	public string printStatusText { get; set; }
}
