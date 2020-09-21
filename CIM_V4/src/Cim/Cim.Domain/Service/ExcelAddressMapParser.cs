using Cim.Domain.Model;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using NLog;
using System;
using System.Collections.Generic;
using Tectone.Common.Extensions;
using DataType = Cim.Domain.Model.DataType;

namespace Cim.Domain.Service
{
    /// <summary>
    /// 엑셀에서 AddressMap 부분을 파싱
    /// </summary>
    public class ExcelAddressMapParser : ExcelParser
    {

        public ExcelAddressMapParser()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        #region ParseAddressMap (override 가능)

        public virtual AddressMap ParseAddressMap(AddressMap input, List<string> columns, IXLTableRow row)
        {
            if (input == null)
                input = new AddressMap();

            try
            {
                int index = -1;
                int index2 = -1;
                string cellValue = null;
                string cellValue2 = null;
                int intCell = -1;
                DataType dataType = DataType.None;
                AddressMapParseErrors = new List<(string, string)>();
                List<string> metaDataColumns = columns.DeepCopy(); //필수항목을 뺀 metaDatas 파싱할 항목

                //VariableId, VariableName
                (index, cellValue, index2, cellValue2) = ParseVariableIdAndName(columns, row);
                if (!string.IsNullOrEmpty(cellValue))
                {
                    input.VariableId = cellValue;
                    input.VariableName = cellValue2;
                    input.SetCellAddress("Variableid", row.Field(index)?.Address?.ToString());
                    input.SetCellAddress("VariableName", row.Field(index2)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                    metaDataColumns.Remove(cellValue2);
                }
                //Address
                (index, cellValue) = ParseAddress(columns, row);
                if (!string.IsNullOrEmpty(cellValue))
                {
                    input.Address = cellValue;
                    input.SetCellAddress("Address", row.Field(index)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                }

                //Size
                (index, cellValue, intCell) = ParseSize(columns, row);
                if (intCell > -1)
                {
                    input.Size = intCell;
                    input.SetCellAddress("Size", row.Field(index)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                }

                //decimalplace
                (index, cellValue, intCell) = ParseDecimalPoint(columns, row);
                if (intCell > -1)
                {
                    input.DecimalPoint = intCell;
                    input.SetCellAddress("DecimalPoint", row.Field(index)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                }

                //datatype
                (index, cellValue, dataType) = ParseDataType(columns, row);
                if (dataType != DataType.None)
                {
                    input.DataType = dataType;
                    input.SetCellAddress("DataType", row.Field(index)?.Address?.ToString());
                    metaDataColumns.Remove(cellValue);
                }

                #region MetaDatas (아직 파싱안한 컬럼(metaDataColumns)을 파싱)

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

        public virtual (int, string, int, string) ParseVariableIdAndName(List<string> columns, IXLTableRow row)
        {
            int index2 = -1;
            string cellValue2 = null;
            (var index, var cellValue) = GetCellValue(columns, row, "variable,변수");
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
                //VariableName에는 VariableId 수정전 문자를 할당
                (index2, cellValue2) = GetCellValue(columns, row, "variablename,변수이름");
                if (index2 != -1)
                {
                    if (string.IsNullOrEmpty(row.Field(index2).Value.ToString()))
                        row.Field(index2).Value = cellValue;
                }

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

            return (index, cellValue, index2, cellValue2);
        }

        public virtual (int, string) ParseAddress(List<string> columns, IXLTableRow row)
        {
            (var index, var cellValue) = GetCellValue(columns, row, "address,주소");
            if (index == -1)
            {
                SetErrorCell(row.Field(0), RequiredErrorString);
            }
            else if (string.IsNullOrEmpty(cellValue))
            {
                SetErrorCell(row.Field(index), RequiredErrorString);
            }

            return (index, cellValue);
        }

        public virtual (int, string, int) ParseSize(List<string> columns, IXLTableRow row)
        {
            (var index, var cellValue) = GetCellValue(columns, row, "size,크기");
            if (int.TryParse(cellValue, out int size))
                return (index, cellValue, size);

            return (index, cellValue, size);
        }

        public virtual (int, string, DataType) ParseDataType(List<string> columns, IXLTableRow row)
        {
            (var index, var cellValue) = GetCellValue(columns, row, "datatype,타입");

            if (Enum.TryParse<DataType>(cellValue, out DataType dataType))
                return (index, cellValue, dataType);

            //아래의 타입 검사는 if--else 이므로 성능을 고려하여, 많이 발생하는 순서로 작성 요망
            if (cellValue.ToLower() == "bit" || cellValue.ToLower() == "bool" || cellValue.ToLower() == "boolean")
                dataType = DataType.Bit;
            else if (cellValue.ToLower() == "short" || cellValue.ToLower() == "word" || cellValue.ToLower() == "word16" || cellValue.ToLower() == "int16")
                dataType = DataType.Word16;
            else if (cellValue.ToLower() == "int" || cellValue.ToLower() == "word2" || cellValue.ToLower() == "word32" || cellValue.ToLower() == "int32")
                dataType = DataType.Word32;

            else if (cellValue.ToLower() == "string" || cellValue.ToLower() == "ascii" || cellValue.ToLower() == "asc" || cellValue.ToLower() == "text")
                dataType = DataType.String;

            else if (cellValue.ToLower() == "ushort" || cellValue.ToLower() == "unsinged word")
                dataType = DataType.WordU16;
            else if (cellValue.ToLower() == "uint")
                dataType = DataType.WordU32;

            else if (cellValue.ToLower() == "float" || cellValue.ToLower() == "real32")
                dataType = DataType.Real32;
            else if (cellValue.ToLower() == "double" || cellValue.ToLower() == "real64")
                dataType = DataType.Real64;

            return (index, cellValue, DataType.None);
        }

        public virtual (int, string, int) ParseDecimalPoint(List<string> columns, IXLTableRow row)
        {
            (var index, var cellValue) = GetCellValue(columns, row, "decimalpoint,소수점");
            if (int.TryParse(cellValue, out int decimalpoint))
                return (index, cellValue, decimalpoint);
            else
            {
                //문자열로 소수점 표시 예) 00.00

                //Scale로 소수점 표시
            }

            return (index, cellValue, decimalpoint);
        }

        public virtual AddressMap ParseCustomAddressMap(AddressMap input, List<string> columns, IXLTableRow row)
        {
            if (input == null)
                input = new AddressMap();

            return input;
        }

        #endregion
    }

    public class ModbusExcelAddressMapParser : ExcelAddressMapParser
    {
        public ModbusExcelAddressMapParser()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        public override (int, string) ParseAddress(List<string> columns, IXLTableRow row)
        {
            (var index, var cell) = GetCellValue(columns, row, "address,주소");
            if (index == -1)
            {
                SetErrorCell(row.Field(0), RequiredErrorString);
            }
            else if (string.IsNullOrEmpty(cell))
            {
                SetErrorCell(row.Field(index), RequiredErrorString);
            }
            else
            {
                if (!int.TryParse(cell, out int address)) //숫자가 아니면 에러!
                {
                    SetErrorCell(row.Field(index), InvalidErrorString);
                    return (index, null);
                }
            }
            return (index, cell);
        }
    }

    public class MelsecExcelAddressMapParser : ExcelAddressMapParser
    {
        public MelsecExcelAddressMapParser()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        public override (int, string) ParseAddress(List<string> columns, IXLTableRow row)
        {
            (var index, var cell) = GetCellValue(columns, row, "address,주소");
            if (index == -1)
            {
                SetErrorCell(row.Field(0), RequiredErrorString);
            }
            else if (string.IsNullOrEmpty(cell))
            {
                SetErrorCell(row.Field(index), RequiredErrorString);
            }

            return (index, cell);
        }
    }

}
