using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Test
{
    [TestClass]
    public class ExcelTest
    {
        [TestMethod]
        public void Test()
        {
            var fileName = "AddressMap_New.xlsx";
            var workbook = new XLWorkbook(fileName);
            var sheet = workbook.Worksheets.FirstOrDefault(m => m.Name == "Controller");
            sheet.Cell("A1").Value = "good";
            workbook.Save();

            Assert.IsTrue(true);

        }

        [TestMethod]
        public void DataTable_Test()
        {
            var fileName = "AddressMap_New.xlsx";
            var workbook = new XLWorkbook(fileName);
            var sheet = workbook.Worksheets.FirstOrDefault(m => m.Name == "Controller");

            var firstAddress = sheet.Cell(1, 1).Address;
            var lastAddress = sheet.LastCellUsed().Address;
            var range = sheet.Range(firstAddress, lastAddress).RangeUsed();
            var table = range.AsTable();

            var columns = table.Fields.Select(m => m.Name).ToList();
            var rows = table.DataRange.Rows().ToList();

            var list = rows
                .Select(m => new
                {
                    Name = m.Field("name").GetString(),
                    Ip = m.Field("ip").GetString(),
                    Port = m.Field("port").GetString(),
                })
                .ToList();

            Assert.IsTrue(true);
        }

    }
}
