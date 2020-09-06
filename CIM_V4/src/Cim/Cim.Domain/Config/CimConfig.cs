using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Config
{
    public class CimConfig : ConfigurationSection
    {
        /// <summary>
        /// 어드레스맵 엑셀 파일 이름
        /// </summary>
        [ConfigurationProperty("addressMapFileName", IsRequired = true, DefaultValue ="AddressMap_Modbus_Test.xlsx")]
        public string AddressMapFileName
        {
            get => (string)base["addressMapFileName"];
            set => base["addressMapFileName"] = value;
        }

        [ConfigurationProperty("csvFile")]
        public CsvFileElement CsvFile => (CsvFileElement)base["csvFile"];
    }

    public class CsvFileElement : ConfigurationElement
    {

        [ConfigurationProperty("WriteColumnCsvFile", IsRequired = true, DefaultValue = true)]
        public bool WriteColumnCsvFile
        {
            get => (bool)base["WriteColumnCsvFile"];
            set => base["WriteColumnCsvFile"] = value;
        }

        [ConfigurationProperty("StoreDateOfColumnCsv", IsRequired = true, DefaultValue = 30)]
        public int StoreDateOfColumnCsv
        {
            get => (int)base["StoreDateOfColumnCsv"];
            set => base["StoreDateOfColumnCsv"] = value;
        }

        [ConfigurationProperty("DateTimeFormatOfColumnCsv", IsRequired = true, DefaultValue = "yyyy.MM.dd HH")]
        public string DateTimeFormatOfColumnCsv
        {
            get => (string)base["DateTimeFormatOfColumnCsv"];
            set => base["DateTimeFormatOfColumnCsv"] = value;
        }

        [ConfigurationProperty("ColumnCsvFilePath", IsRequired = true, DefaultValue = @"d:\PIE\CIM\CSV")]
        public string ColumnCsvFilePath
        {
            get => (string)base["ColumnCsvFilePath"];
            set => base["ColumnCsvFilePath"] = value;
        }
    }

}
