using Cim.Model;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomUI;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Extensions;

namespace Cim.Service
{
    public interface IAddressMapService
    {
        List<Controller> ParseAndWrite(string fileName);

    }

    /// <summary>
    /// 어드레스맵 엑셀을 읽어와서 어드레스목록 작성
    /// </summary>
    public class ExcelAddressMapService : IAddressMapService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 어드레스맵 엑셀(fileName)을 파싱하여 Controller목록으로 반환.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<Controller> ParseAndWrite(string fileName)
        {
            var controllers = new List<Controller>();

            var workbook = new XLWorkbook();
            if (File.Exists(fileName))
            {
                try
                {
                    workbook = new XLWorkbook(fileName);
                }
                catch (Exception ex)
                {
                    logger.Error($"ex={ex}"); //파일이 잠긴 경우
                    return controllers;
                }
            }

            try
            {
                #region Controller 파싱

                (List<string> columns, List<IXLTableRow> rows) = GetColumnAndRow(workbook, "Controller");

                //Controller 시트가 없으면 기본값
                if (rows == null)
                {
                    controllers.Add(new Controller());
                }
                else
                {
                    foreach (var row in rows)
                    {
                        var controller = new Controller();

                        controller = ParseController(controller, columns, row); //기본정보 파싱
                        controller = ParseCustomController(controller, columns, row);//사용자정의 추가파싱

                        if (controller != null)
                            controllers.Add(controller);
                    }
                }

                #endregion

                #region AddressMaps 파싱

                foreach (var controller in controllers)
                {
                    var sheets = controller.SheetNames.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (!(sheets?.Count > 0)) continue;

                    var addressMaps = new List<AddressMap>();
                    foreach (var sheet in sheets)
                    {
                        (columns, rows) = GetColumnAndRow(workbook, sheet);
                        //Controller 시트가 없으면 기본값
                        if (rows == null)
                        {
                            addressMaps.Add(new ModbusAddressMap());
                        }
                        else
                        {
                            foreach (var row in rows)
                            {
                                var input = new AddressMap();
                                switch (controller.Protocol)
                                {
                                    case ControllerProtocol.Modbus:
                                        input = new ModbusAddressMap();
                                        break;
                                    case ControllerProtocol.Melsec:
                                    case ControllerProtocol.None:
                                    default:
                                        input = new AddressMap();
                                        break;
                                }

                                var addressMap = ParseAddressMap(input, columns, row);//기본정보 파싱
                                addressMap = ParseCustomAddressMap(addressMap, columns, row);//사용자정의 추가파싱

                                if (addressMap != null)
                                    addressMaps.Add(addressMap);
                            }
                        }
                    }
                    controller.AddressMaps = addressMaps;
                } 

                #endregion
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            #region 엑셀 저장

            if (File.Exists(fileName))
                workbook.Save();
            else
            {
                var sheet = workbook.Worksheets.Add("Sheet1");
                sheet.Cell("A1").Value = "variable";
                sheet.Cell("B1").Value = "address";
                sheet.Cell("C1").Value = "size";
                sheet.Cell("D1").Value = "datatype";
                sheet.Cell("E1").Value = "decimalpoint";

                sheet.Cell("A2").Value = "item";
                sheet.Cell("B2").Value = "D10";
                sheet.Cell("C2").Value = "1";
                sheet.Cell("D2").Value = "int";
                sheet.Cell("E2").Value = "0";

                workbook.SaveAs(fileName);
            } 
            #endregion

            return controllers;
        }


        #region Util

        public List<IXLWorksheet> GetWorkSheets(string fileName, string sheetName=null)
        {
            var workbook = new XLWorkbook(fileName);

            if (string.IsNullOrEmpty(sheetName))
                return workbook.Worksheets.ToList();
            else
                return new List<IXLWorksheet> { workbook.Worksheets.FirstOrDefault(m => m.Name == sheetName) };
        }

        public (List<string>, List<IXLTableRow>) GetColumnAndRow(XLWorkbook workbook, string sheetName)
        {
            var columns = new List<string>();
            var rows = new List<IXLTableRow>();

            try
            {
                var sheet = workbook.Worksheets.FirstOrDefault(m => m.Name == sheetName);
                if (sheet == null)
                    return (null, null);

                var firstAddress = sheet.Cell(1, 1).Address;
                var lastAddress = sheet.LastCellUsed().Address;
                var range = sheet.Range(firstAddress, lastAddress).RangeUsed();
                var table = range.AsTable();

                columns = table.Fields.Select(m => m.Name).ToList();
                rows = table.DataRange.Rows().ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            return (columns, rows);
        }

        /// <summary>
        /// 엑셀의 1줄(row)에서 searchPattern(예: name,이름) 문자가 포함된 column 의 cell  값을 반환
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="row"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public (int, string) GetCellValue(List<string> columns, IXLTableRow row, string searchPattern)
        {
            int index = -1;
            string cell = null;
            var searchs = searchPattern.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                string column = null;

                foreach (var item in searchs)
                {
                    column = columns.FirstOrDefault(m => m.ToLower().Contains(item));
                    if (column != null)
                        break;
                }

                if (column == null)
                {
                    logger.Warn($"FindColumnString Find Column Fail !!! searchPattern={searchPattern}");
                    return (index, cell);
                }

                index = columns.IndexOf(column);
                if (index > -1)
                {
                    //parse
                    cell = row.Field(index).GetString();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            return (index, cell);
        } 

        #endregion

        #region ParseController

        public virtual Controller ParseController(Controller input, List<string> columns, IXLTableRow row)
        {
            if (input == null)
                input = new Controller();

            try
            {
                int index = -1;
                string cell = null;
                List<string> metaDataColumns = columns.DeepCopy(); //필수항목을 뺀 metaDatas 파싱할 항목

                #region name

                (index, cell) = GetCellValue(columns, row, "name,이름");
                if (index == -1)
                {
                    row.Field(0).SetValue($"필수항목");
                    row.Field(0).Style.Fill.BackgroundColor = XLColor.Red;
                }
                else if (string.IsNullOrEmpty(cell))
                {
                    row.Field(index).SetValue($"필수항목");
                    row.Field(index).Style.Fill.BackgroundColor = XLColor.Red;
                }
                else
                {
                    input.Name = cell;
                }

                #endregion

                #region ip

                (index, cell) = GetCellValue(columns, row, "ip,아이피");
                if (index > -1) metaDataColumns.Remove(cell);
                input.Ip = cell;
                #endregion

                #region port

                (index, cell) = GetCellValue(columns, row, "port,포트");
                if (index > -1) metaDataColumns.Remove(cell);
                if (int.TryParse(cell, out int port))
                    input.Port = port;
                #endregion

                #region protocol

                (index, cell) = GetCellValue(columns, row, "protocol,프로토콜");
                if (index > -1) metaDataColumns.Remove(cell);
                if (cell.ToLower() == "modbus")
                    input.Protocol = ControllerProtocol.Modbus;
                else if (cell.ToLower() == "melsec")
                    input.Protocol = ControllerProtocol.Melsec;
                else
                    input.Protocol = ControllerProtocol.None;

                #endregion

                #region sheetname

                (index, cell) = GetCellValue(columns, row, "sheetname,시트");
                if (index > -1) metaDataColumns.Remove(cell);
                input.SheetNames = cell;
                #endregion

                #region MetaDatas

                input.MetaDatas = new Dictionary<string, string>();
                foreach (var item in metaDataColumns)
                {
                    (index, cell) = GetCellValue(columns, row, item);
                    input.MetaDatas.Add(item, cell);
                }
                #endregion
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            return input;
        }

        public virtual Controller ParseCustomController(Controller input, List<string> columns, IXLTableRow row)
        {
            if (input == null)
                input = new Controller();


            return input;
        }

        #endregion

        #region ParseAddressMap

        public virtual AddressMap ParseAddressMap(AddressMap input, List<string> columns, IXLTableRow row)
        {
            if (input == null)
                input = new AddressMap();

            try
            {
                int index = -1;
                string cell = null;
                List<string> metaDataColumns = columns.DeepCopy(); //필수항목을 뺀 metaDatas 파싱할 항목

                #region variable

                (index, cell) = GetCellValue(columns, row, "variable,변수");
                if (index == -1)
                {
                    row.Field(0).SetValue($"필수항목");
                    row.Field(0).Style.Fill.BackgroundColor = XLColor.Red;
                }
                else if (string.IsNullOrEmpty(cell))
                {
                    row.Field(index).SetValue($"필수항목");
                    row.Field(index).Style.Fill.BackgroundColor = XLColor.Red;
                }
                else
                {
                    input.VariableId = cell;

                    metaDataColumns.Remove(cell);
                }
                #endregion

                #region address

                (index, cell) = GetCellValue(columns, row, "address,주소");
                if (index == -1)
                {
                    row.Field(0).SetValue($"필수항목");
                    row.Field(0).Style.Fill.BackgroundColor = XLColor.Red;
                }
                else if (string.IsNullOrEmpty(cell))
                {
                    row.Field(index).SetValue($"필수항목");
                    row.Field(index).Style.Fill.BackgroundColor = XLColor.Red;
                }
                else
                {
                    input.Address = cell;

                    metaDataColumns.Remove(cell);
                }
                #endregion

                #region size

                (index, cell) = GetCellValue(columns, row, "size,크기");
                if(ushort.TryParse(cell, out ushort size))
                    input.Size = size;

                #endregion

                #region decimalplace

                (index, cell) = GetCellValue(columns, row, "decimalpoint,소수점");
                if (ushort.TryParse(cell, out ushort decimalpoint))
                    input.DeciamlPoint = decimalpoint;

                #endregion

                #region datatype

                (index, cell) = GetCellValue(columns, row, "datatype,타입");
                //input.DataType = DataType.Word;

                #endregion

                #region MetaDatas

                input.MetaDatas = new Dictionary<string, string>();
                foreach (var item in metaDataColumns)
                {
                    (index, cell) = GetCellValue(columns, row, item);
                    input.MetaDatas.Add(item, cell);
                }
                #endregion
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            return input;
        }

        public virtual AddressMap ParseCustomAddressMap(AddressMap input, List<string> columns, IXLTableRow row)
        {
            if (input == null)
                input = new AddressMap();

            return input;
        } 

        #endregion

    }
}
