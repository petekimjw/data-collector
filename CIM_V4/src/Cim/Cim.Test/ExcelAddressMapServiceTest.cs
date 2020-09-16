using System;
using System.IO;
using System.Linq;
using Cim.Domain.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cim.Test
{
    [TestClass]
    public class ExcelAddressMapServiceTest
    {
        private string fileName = "AddressMap_New1.xlsx";

        private ExcelAddressMapService service = null;

        [TestInitialize]
        public void Init()
        {
            service = new ExcelAddressMapService();
        }

        [TestMethod]
        public void 파일이없어도_기본정보로파싱_파일생성()
        {         
            var controllers = service.ParseAndWrite(fileName);

            Assert.IsTrue(File.Exists(fileName));
            Assert.IsTrue(controllers?.Count > 0);
        }

        [TestMethod]
        public void 어드레스맵시트size일부만있어도_기본정보로파싱()
        {
            //어드레스 추가
            var sheet = service.GetWorkSheets(fileName, "Sheet1")?.FirstOrDefault();
            sheet.Cell("C1").Value = "size";
            sheet.Cell("C3").Value = "2";
            for (int i = 3; i < 10; i++)
            {
                sheet.Cell($"A{i}").Value = "item2";
                sheet.Cell($"B{i}").Value = $"1{i}";
            }
            sheet.Workbook.Save();

            //추가한 어드레스 파싱
            var controllers = service.ParseAndWrite(fileName);

            Assert.IsTrue(controllers?.Count > 0);
            Assert.IsTrue(controllers?.FirstOrDefault()?.AddressMaps.All(m => m.Size > -1) == true);
        }

        [TestMethod]
        public void 고객엑셀1_붙여넣기후_파싱()
        {

        }

        [TestMethod]
        public void 고객엑셀2_붙여넣기후_파싱()
        {

        }

    }
}
