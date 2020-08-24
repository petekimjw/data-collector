using CIM.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Model
{
    /// <summary>
    /// 빅인디언, 리틀인디언등을 따지는 32bit 바이트오더. 8bit는 16진수로 표현된다.
    /// </summary>
    public enum ByteOrder { ABCD, CDAB, BADC, DCBA }
    /// <summary>
    /// 자료형. int, short, float 가 아닌, PLC 입장에서 자료형
    /// </summary>
    public enum DataType { Word, DoubleWord, String, Bit }
    /// <summary>
    /// 데이터 성격별 분류 (Status, Alarm, Data, Trace)
    /// </summary>
    public enum DataCategory { Status, Alarm, Data, Trace }

    public class AddressMap
    {
        #region 초기화
        public override string ToString()
        {
            return $"Id={Id}, VariableId={VariableName}, Address={Address}, Count={Count}, Scale={Scale}, " +
                $"UseYN={UseYN}, DataType={DataType}, DataCategory={DataCategory}," +
                $"Description={Description}, Description2={Description2}, GroupNo1={GroupNo1}, GroupNo2={GroupNo2}";
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AddressMap(string deviceId, string variableId, string address, ushort count=1, float scale=1,
            int id=0, bool useYN=true, DataType dataType=DataType.Word, DataCategory dataCategory=DataCategory.Trace, 
            string description=null, string description2=null, int groupNo1=1, int groupNo2=1)
        {
            DeviceName = deviceId;
            VariableName = variableId;
            Address = address;

            #region AddressNumber
            try
            {
                //var addressNumber = 1;
                int dotIndex = address.IndexOf(".");
                if (dotIndex > 0)
                {
                    address = address.Substring(0, dotIndex);
                }
                int numberIndex = -1;
                for (int i = 0; i < address.Length; i++)
                {
                    if (int.TryParse(address.Substring(i, 1), out int number))
                    {
                        numberIndex = i;
                        break;
                    }
                }
                address = address.Substring(numberIndex);
                AddressNumber = int.Parse(address);
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            } 
            #endregion

            Count = count;
            Scale = scale;

            Id = id;
            UseYN = useYN;
            DataType = dataType;
            DataCategory = dataCategory;
            Description = description;
            Description2 = description2;
            GroupNo1 = groupNo1;
            GroupNo2 = groupNo2;

        }
        #endregion

        #region 속성
        public string DeviceName { get; set; }
        public string VariableName { get; set; }
        public string Address { get; set; }
        public ushort Count { get; set; } = 1;
        public float Scale { get; set; } = 1;

        public int Id { get; set; } = 0;
        /// <summary>
        /// Address주소에서 숫자
        /// </summary>
        public int AddressNumber { get; set; }
        /// <summary>
        /// Address주소에서 문자
        /// </summary>
        public string AddressSymbol { get; set; }
        public bool UseYN { get; set; } = false;
        public DataType DataType { get; set; } = DataType.Word;
        public DataCategory DataCategory { get; set; } = DataCategory.Data;


        public string Description { get; set; }
        public string Description2 { get; set; }
        public int GroupNo1 { get; set; }
        public int GroupNo2 { get; set; }

        public Dictionary<string, string> MetaDatas { get; set; }

        #endregion
    }

    public class ModbusAddressMap : AddressMap
    {
        #region 초기화
        public override string ToString()
        {
            return $"{base.ToString()}, SlaveId={SlaveId}, RegesterType={RegesterType}";
        }

        public ModbusAddressMap(string deviceId, string variableId, string address, ushort count=1, float scale=1,
            int id=0, bool useYN=true, DataType dataType=DataType.Word, DataCategory dataCategory=DataCategory.Trace, 
            string description=null, string description2=null, int groupNo=1, int groupNo2=1,
            int slaveId = 1, ModbusRegisterType registerType = ModbusRegisterType.Holding)
             : base(deviceId, variableId, address, count, scale, id, useYN, dataType, dataCategory, description, description2, groupNo, groupNo2)
        {
            Id = id;
            UseYN = useYN;
            DataType = dataType;
            DataCategory = dataCategory;
            Description = description;
            Description2 = description2;
            GroupNo1 = groupNo;
            GroupNo2 = groupNo2;

            SlaveId = slaveId;
            RegesterType = registerType;
        } 

        #endregion

        public int SlaveId { get; set; }
        public ModbusRegisterType RegesterType { get; set; }

    }

    public class MelsecAddressMap : AddressMap
    {
        public override string ToString()
        {
            return $"{base.ToString()}, DeviceType={DeviceType}";
        }

        public MelsecAddressMap(string deviceId, string variableId, string address, 
            ushort count=1, float scale=1, MelsecDeviceType deviceType=MelsecDeviceType.Block)
            : base(deviceId, variableId, address, count, scale)
        {
            DeviceType = deviceType;
        }
        public MelsecDeviceType DeviceType { get; set; }
    }

}
