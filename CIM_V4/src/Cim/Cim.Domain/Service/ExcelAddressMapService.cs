﻿using AutoMapper.Internal;
using Cim.Domain.Model;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Spreadsheet;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Extensions;

namespace Cim.Domain.Service
{
    public interface IAddressMapService
    {
        /// <summary>
        /// 어드레스맵 파싱시 에러
        /// </summary>
        List<(string, string)> AddressMapParseErrors { get; set; }
        List<Controller> ParseAndWrite(string fileName);
    }

    /// <summary>
    /// 어드레스맵 엑셀을 읽어와서 Controller, AddressMaps 작성
    /// </summary>
    public class ExcelAddressMapService : ExcelParser, IAddressMapService
    {
        public ExcelAddressMapService()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

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
                    logger.Error($"{fileName} 파일 열기 실패!!! ex={ex}"); //파일이 잠긴 경우
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

                    var addressMaps = new ObservableCollection<AddressMap>();
                    foreach (var sheet in sheets)
                    {
                        CheckAndInsertColumn(workbook, sheet, "variablename"); //VariableName 컬럼이 없으면 삽입
                        (columns, rows) = GetColumnAndRow(workbook, sheet);
                        
                        //Controller 시트가 없으면 기본값
                        if (rows == null)
                        {
                            addressMaps.Add(new AddressMap { FunctionCode = Driver.FunctionCode.HoldingRegister });
                        }
                        else
                        {
                            var input = new AddressMap();
                            ExcelAddressMapParser parser = null;

                            foreach (var row in rows)
                            {
                                switch (controller.Protocol)
                                {
                                    case ControllerProtocol.Modbus:
                                        input = new AddressMap { FunctionCode = Driver.FunctionCode.HoldingRegister };
                                        parser = new ModbusExcelAddressMapParser();
                                        break;
                                    case ControllerProtocol.Melsec:
                                        parser = new MelsecExcelAddressMapParser();
                                        break;
                                    case ControllerProtocol.None:
                                    default:
                                        input = new AddressMap();
                                        break;
                                }

                                var addressMap = parser.ParseAddressMap(input, columns, row);//기본정보 파싱
                                addressMap = parser.ParseCustomAddressMap(addressMap, columns, row);//사용자정의 추가파싱

                                if (addressMap != null)
                                    addressMaps.Add(addressMap);
                            }

                            AddressMapParseErrors.AddRange(parser.AddressMapParseErrors);
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
                sheet.Cell("B2").Value = "0";
                sheet.Cell("C2").Value = "1";
                sheet.Cell("D2").Value = "word";
                sheet.Cell("E2").Value = "0";

                workbook.SaveAs(fileName);
            } 
            #endregion

            return controllers;
        }

        #region ParseController (override 가능)

        /// <summary>
        /// 사용안함
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="input"></param>
        /// <param name="cellValue"></param>
        private void SetParsingPropertyInfo<T>(Expression<Func<T>> property, object input, string cellValue)
        {
            //if (!string.IsNullOrEmpty(cellValue))
            //    return;

            ////string propertyName = ((MemberExpression)property.Body).Member.Name;
            //var propertyInfo = property.GetMemberInfo() as PropertyInfo;

            //propertyInfo.SetValue(input, cellValue);
            //var cellAddresses = input.GetPropertyValue("CellAddresses") as Dictionary<string, object>;

            //if (cellAddresses.ContainsKey(propertyInfo.Name))
            //    cellAddresses[propertyInfo.Name] = row.Field(index)?.Address?.ToString();
            //else
            //    cellAddresses.Add(propertyInfo.Name, row.Field(index)?.Address?.ToString());
            //metaDataColumns.Remove(cellValue);

        }

        public virtual Controller ParseController(Controller input, List<string> columns, IXLTableRow row)
        {
            if (input == null)
                input = new Controller();

            try
            {
                int index = -1;
                string cellValue = null;
                
                int intCell = -1;
                var protocol = ControllerProtocol.None;
                
                List<string> metaDataColumns = columns.DeepCopy(); //필수항목을 뺀 metaDatas 파싱할 항목

                //name
                (index, cellValue) = ParseControllerName(columns, row);
                if(!string.IsNullOrEmpty(cellValue))
                {
                    input.Name = cellValue;
                    input.SetCellAddress( "Name", row.Field(index)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                }

                //ip
                (index, cellValue) = ParseIp(columns, row);
                if (index > -1)
                {
                    input.Ip = cellValue;
                    input.SetCellAddress("Ip", row.Field(index)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                }

                //port
                (index, cellValue, intCell) = ParsePort(columns, row);
                if (intCell > -1)
                {
                    input.Port = intCell;
                    input.SetCellAddress("Port", row.Field(index)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                }

                //protocol
                (index, cellValue, protocol) = ParseControllerProtocol(columns, row);      
                if (protocol != ControllerProtocol.None)
                {
                    input.Protocol = protocol;
                    input.SetCellAddress("Protocol", row.Field(index)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                }

                //sheetname
                (index, cellValue) = ParseSheetName(columns, row);
                if (index > -1)
                {
                    input.SheetNames = cellValue;
                    input.SetCellAddress("SheetName", row.Field(index)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                }

                #region MetaDatas

                input.MetaDatas = new Dictionary<string, string>();
                foreach (var item in metaDataColumns)
                {
                    (index, cellValue) = GetCellValue(columns, row, item);
                    input.MetaDatas.Add(item, cellValue);
                    input.SetCellAddress(item, row.Field(index)?.Address?.ToString());
                }
                #endregion
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            return input;
        }

        public virtual (int, string) ParseControllerName(List<string> columns, IXLTableRow row)
        {
            (var index, var cellValue) = GetCellValue(columns, row, "name,이름");
            if (index == -1)
            {
                SetErrorCell(row.Field(0), RequiredErrorString);
            }
            else if (string.IsNullOrEmpty(cellValue))
            {
                SetErrorCell(row.Field(index), RequiredErrorString);
            }
            else
            {
                //특수문자(&,',",<,>,%,#,$), 공백 제거
                var temp = cellValue.Replace(" ", "_");
                foreach (var item in EscapeCharacters)
                {
                    temp = temp.Replace(item, "");
                }
                if (temp != cellValue)
                {
                    cellValue = temp;
                    row.Field(index).Value = temp;
                    SetWarningCell(row.Field(index), "");
                }
            }

            return (index, cellValue);
        }

        public virtual (int, string) ParseIp(List<string> columns, IXLTableRow row)
        {
            (var index, var cellValue) = GetCellValue(columns, row, "ip,아이피");
            if (index > -1)
                return (index, cellValue);
            else
                return (index, null);
        }

        public virtual (int, string, int) ParsePort(List<string> columns, IXLTableRow row)
        {
            (var index, var cellValue) = GetCellValue(columns, row, "port,포트");
            if (int.TryParse(cellValue, out int port))
                return (index, cellValue, port);

            return (index, cellValue, port);
        }

        public virtual (int, string, ControllerProtocol) ParseControllerProtocol(List<string> columns, IXLTableRow row)
        {
            (var index, var cellValue) = GetCellValue(columns, row, "protocol,프로토콜");
            if (Enum.TryParse<ControllerProtocol>(cellValue, out ControllerProtocol protocol))
                return (index, cellValue, protocol);

            return (index, cellValue, ControllerProtocol.None);
        }

        public virtual (int, string) ParseSheetName(List<string> columns, IXLTableRow row)
        {
            (var index, var cellValue) = GetCellValue(columns, row, "sheetname,시트");
            if (index > -1)
                return (index, cellValue);
            else
                return (index, null);
        }

        public virtual Controller ParseCustomController(Controller input, List<string> columns, IXLTableRow row)
        {
            if (input == null)
                input = new Controller();


            return input;
        }

        #endregion

    }

    /// <summary>
    /// Excel 파싱 공통 기능
    /// </summary>
    public class ExcelParser
    {
        protected Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// todo: VariableId 에서 제거할 특수문자 목록
        /// </summary>
        public List<string> EscapeCharacters { get; set; } = new List<string> { "'", "\"", "<", ">", "%", "#", "$", "&", "-"};

        #region Util

        public List<IXLWorksheet> GetWorkSheets(string fileName, string sheetName = null)
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
        /// columnName 있는지 확인하여, 없으면 맨 오른쪽에 추가한다.
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="sheetName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool CheckAndInsertColumn(XLWorkbook workbook, string sheetName, string columnName)
        {
            var columns = new List<string>();
            var rows = new List<IXLTableRow>();

            try
            {
                var sheet = workbook.Worksheets.FirstOrDefault(m => m.Name == sheetName);
                if (sheet == null)
                    return false;

                var firstAddress = sheet.Cell(1, 1).Address;
                var lastAddress = sheet.LastCellUsed().Address;
                var range = sheet.Range(firstAddress, lastAddress).RangeUsed();
                var table = range.AsTable();

                columns = table.Fields.Select(m => m.Name).ToList();

                var find = columns.FirstOrDefault(m => m.ToLower() == columnName.ToLower());
                if(find == null)
                {
                    sheet.Cell(1, lastAddress.ColumnNumber + 1).Value = columnName;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            return true;
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
            string cellValue = null;
            var searchs = searchPattern.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string searchedColumn = null;
            try
            {
                string column = null;

                foreach (string item in searchs)
                {
                    searchedColumn = item;
                    column = columns.FirstOrDefault(m => m.ToLower().Contains(searchedColumn));
                    if (column != null)
                        break;
                }

                if (column == null)
                {
                    logger.Warn($"FindColumnString Find Column Fail !!! searchPattern={searchPattern}");
                    return (index, cellValue);
                }

                index = columns.IndexOf(column);
                if (index > -1)
                {
                    //parse
                    cellValue = row.Field(index).GetString();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            return (index, cellValue);
        }

        #endregion

        #region 파싱에러

        public const string RequiredErrorString = "필수항목";
        public const string InvalidErrorString = "잘못됨";

        /// <summary>
        /// 어드레스맵 파싱시 에러
        /// </summary>
        public List<(string, string)> AddressMapParseErrors { get; set; } = new List<(string, string)>();

        /// <summary>
        /// Cell 파싱시 에러표시
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="originValue"></param>
        /// <param name="error"></param>
        public void SetErrorCell(IXLCell cell, string error)
        {
            try
            {
                if (!cell.Value.ToString().StartsWith($"{error}"))
                {
                    cell.SetValue($"{error}-{cell.Value}");
                    cell.Style.Fill.BackgroundColor = XLColor.Red;

                    AddressMapParseErrors.Add(($"{cell.Address}", $"{error}{cell.Value}"));
                }
                else
                    AddressMapParseErrors.Add(($"{cell.Address}", $"{cell.Value}"));
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        }

        public void SetWarningCell(IXLCell cell, string warning)
        {
            try
            {
                if(string.IsNullOrEmpty(warning))
                    cell.Style.Font.FontColor = XLColor.Red;

                else if (!cell.Value.ToString().StartsWith($"{warning}"))
                {
                    cell.SetValue($"{warning}{cell.Value}");
                    cell.Style.Font.FontColor = XLColor.Red;
                }
                //AddressMapParseErrors.Add(($"{cell.Address}", $"{warning}-{cell.Value}"));
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        }

        #endregion

    }

}
