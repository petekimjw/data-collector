using Cim.Model;
using Cim.Service;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Extensions;

namespace Cim.Test
{
    /// <summary>
    /// https://github.com/ClosedXML/ClosedXML/wiki
    /// </summary>
    [TestClass]
    public class ClosedXMLTest
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [TestMethod]
        public void 엑셀_저장_기본기능()
        {
            var fileName = "AddressMap_New.xlsx";
            var workbook = new XLWorkbook(fileName);
            var sheet = workbook.Worksheets.FirstOrDefault(m => m.Name == "Controller");
            sheet.Cell("A1").Value = "good";
            workbook.Save();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void 엑셀_Controller_파싱()
        {
            //File.Copy(fileInfo.FullName, $"{tempFolderName}\\{fileInfo.Name}", true);
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

            foreach (var row in rows)
            {
                var nameCell = row.Field("name");
                nameCell.SetValue($"{nameCell.GetString()}-Fail !!!");
                nameCell.Style.Fill.BackgroundColor = XLColor.Red;
                break;
            }

            workbook.Save();

            Assert.IsTrue(true);
        }




    }
}
