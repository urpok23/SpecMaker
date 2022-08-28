using Excel = Microsoft.Office.Interop.Excel;
using System.IO;

namespace SpecMaker
{
    internal class PDFPrinters
    {
        public static void PrintViaInterop(string excelFilePath)
        {
            var xlApp = new Excel.Application
            {
                Visible = false,
                DisplayAlerts = false,
                //AutomationSecurity = 0
            };

            var xlWb = xlApp.Workbooks.Open(Filename:excelFilePath, 
                UpdateLinks: 0,  // External references(links) will not be updated when the workbook is opened.
                ReadOnly: true);

            string pdfName = Path.ChangeExtension(excelFilePath, ".pdf");
            xlWb.ExportAsFixedFormat(
                Type: Excel.XlFixedFormatType.xlTypePDF, 
                Filename: pdfName,
                Quality: Excel.XlFixedFormatQuality.xlQualityStandard,
                IgnorePrintAreas: false,
                OpenAfterPublish: false);

            xlWb.Close(SaveChanges: false);
            xlApp.Quit();
        }
    }
}
