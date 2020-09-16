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
            base.logger = LogManager.GetCurrentClassLogger();
        }

        #region ParseAddressMap

        public virtual AddressMap ParseAddressMap(AddressMap input, List<string> columns, IXLTableRow row)
        {
            if (input == null)
                input = new AddressMap();

            try
            {
                int index = -1;
                string cell = null;
                string cell2 = null;
                int intCell = -1;
                DataType dataType = DataType.None;
                AddressMapParseErrors = new List<(string, string)>();
                List<string> metaDataColumns = columns.DeepCopy(); //필수항목을 뺀 metaDatas 파싱할 항목

                //VariableId, VariableName
                (index, cell, cell2) = ParseVariableIdAndName(columns, row);
                if (!string.IsNullOrEmpty(cell))
                {
                    input.VariableId = cell;
                    input.VariableName = cell2;
                    metaDataColumns.Remove(cell);
                    metaDataColumns.Remove(cell2);
                }
                //Address
                (index, cell) = ParseAddress(columns, row);
                if (!string.IsNullOrEmpty(cell))
                {
                    input.Address = cell;
                    metaDataColumns.Remove(cell);
                }

                //Size
                (cell, intCell) = ParseSize(columns, row);
                if (intCell > -1)
                {
                    input.Size = intCell;
                    metaDataColumns.Remove(cell);
                }

                //decimalplace
                (cell, intCell) = ParseDecimalPoint(columns, row);
                if (intCell > -1)
                {
                    input.DeciamlPoint = intCell;
                    metaDataColumns.Remove(cell);
                }

                //datatype
                (cell, dataType) = ParseDataType(columns, row);
                if (dataType != DataType.None)
                {
                    input.DataType = dataType;
                    metaDataColumns.Remove(cell);
                }

                #region MetaDatas (아직 파싱안한 컬럼(metaDataColumns)을 파싱)

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

        public virtual (int, string, string) ParseVariableIdAndName(List<string> columns, IXLTableRow row)
        {
            int index2 = -1;
            string cell2 = null;
            (var index, var cell) = GetCellValue(columns, row, "variable,변수");
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
                //VariableName에는 VariableId 수정전 문자를 할당
                (index2, cell2) = GetCellValue(columns, row, "variablename,변수이름");
                if (index2 != -1)
                {
                    if (string.IsNullOrEmpty(row.Field(index2).Value.ToString()))
                        row.Field(index2).Value = cell;
                }

                //특수문자(&,',",<,>,%,#,$), 공백 제거
                var temp = cell.Replace(" ", "_");
                foreach (var item in EscapeCharacters)
                {
                    temp = temp.Replace(item, "");
                }
                if (temp != cell)
                {
                    cell = temp;
                    row.Field(index).Value = temp;
                    SetWarningCell(row.Field(index), "");
                }
            }

            return (index, cell, cell2);
        }

        public virtual (int, string) ParseAddress(List<string> columns, IXLTableRow row)
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

        public virtual (string, int) ParseSize(List<string> columns, IXLTableRow row)
        {
            (var index, var cell) = GetCellValue(columns, row, "size,크기");
            if (int.TryParse(cell, out int size))
                return (cell, size);

            return (cell, size);
        }

        public virtual (string, DataType) ParseDataType(List<string> columns, IXLTableRow row)
        {
            (var index, var cell) = GetCellValue(columns, row, "datatype,타입");
            if (Enum.TryParse<DataType>(cell, out DataType dataType))
                return (cell, dataType);

            return (cell, DataType.None);
        }

        public virtual (string, int) ParseDecimalPoint(List<string> columns, IXLTableRow row)
        {
            (var index, var cell) = GetCellValue(columns, row, "decimalpoint,소수점");
            if (int.TryParse(cell, out int decimalpoint))
                return (cell, decimalpoint);
            else
            {
                //문자열로 소수점 표시 예) 00.00

                //Scale로 소수점 표시
            }

            return (cell, decimalpoint);
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
            base.logger = LogManager.GetCurrentClassLogger();
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
            base.logger = LogManager.GetCurrentClassLogger();
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
