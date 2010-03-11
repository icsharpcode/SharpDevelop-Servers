using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.IO;

namespace ExcelReport
{
    class ExcelWorkbook : IDisposable
    {
        Application excel;
        Workbook wb;
        bool hasSheets;

        public ExcelWorkbook()
        {
            excel = new Application();
            excel.Visible = false;
            wb = excel.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
        }

        public Worksheet CreateSheet(string name)
        {
            Console.WriteLine("Creating '" + name + "'...");
            Worksheet ws;
            if (!hasSheets)
                ws = wb.Worksheets[1];
            else
                ws = wb.Worksheets.Add();
            hasSheets = true;
            ws.Name = name;
            return ws;
        }

        public void Save(string filename)
        {
            wb.SaveAs(filename);
        }

        public void Dispose()
        {
            wb.Close();
            excel.Quit();
        }
    }
}
