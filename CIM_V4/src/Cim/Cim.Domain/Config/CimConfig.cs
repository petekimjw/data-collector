using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Domain.Config
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

        [ConfigurationProperty("mq")]
        public MqElement Mq => (MqElement)base["mq"];
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

    public class MqElement : ConfigurationElement
    {
        [ConfigurationProperty("userName", IsRequired = true)]
        public string UserName
        {
            get => (string)base["userName"];
            set => base["userName"] = value;
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get => (string)base["password"];
            set => base["password"] = value;
        }

        [ConfigurationProperty("virtualHost", IsRequired = true)]
        public string VirtualHost
        {
            get => (string)base["virtualHost"];
            set => base["virtualHost"] = value;
        }

        [ConfigurationProperty("hostname", IsRequired = true)]
        public string HostName
        {
            get => (string)base["hostname"];
            set => base["hostname"] = value;
        }

        [ConfigurationProperty("hostnames")]
        public string HostNames
        {
            get => (string)base["hostnames"];
            set => base["hostnames"] = value;
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get => (int)base["port"];
            set => base["port"] = value;
        }

        [ConfigurationProperty("exchange", IsRequired = true)]
        public string Exchange
        {
            get => (string)base["exchange"];
            set => base["exchange"] = value;
        }
    }
}
