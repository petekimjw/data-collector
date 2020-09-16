using Cim.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Mvvm;

namespace Cim.Model
{
    /// <summary>
    /// 빅인디언, 리틀인디언등을 따지는 32bit 바이트오더. 8bit는 16진수로 표현된다.
    /// </summary>
    public enum ByteOrder { ABCD, CDAB, BADC, DCBA }
    /// <summary>
    /// 자료형. int, short, float 가 아닌, PLC 입장에서 자료형(Bit, Word16, Word32, String)
    /// </summary>
    public enum DataType { None, Bit, Word16, Word32, String, Real32, Real64, WordU16, WordU32 }
    /// <summary>
    /// 데이터 성격별 분류 (Status, Alarm, Data, Trigger)
    /// </summary>
    public enum DataCategory { Data, Status, Alarm }

    public class AddressMap : BindableBase
    {
        #region 초기화
        public override string ToString()
        {
            return $"DeviceId={DeviceId}, Id={Id}, VariableId={VariableId}, VariableName={VariableName}, Address={Address}, Size={Size}, Scale={DeciamlPoint}, " +
                $"UseYN={IsUsed}, DataType={DataType}, DataCategory={DataCategory}," +
                $"Description={Description}, Group={Group}";
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AddressMap()
        {

        }

        public AddressMap(string deviceId, string variableId, string address, int size =1, int decimalPoint=0, string variableName = null,
            int id=0, bool useYN=true, DataType dataType=DataType.Word16, DataCategory dataCategory=DataCategory.Data, 
            string description=null, string group=null)
        {
            DeviceId = deviceId;
            VariableId = variableId;
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

            Size = size;
            DeciamlPoint = decimalPoint;

            Id = id;
            IsUsed = useYN;
            DataType = dataType;
            DataCategory = dataCategory;
            Description = description;
            Group = group;
        }
        #endregion

        #region 속성
        private string _DeviceName = "Controller";
        public string DeviceId
        {
            get { return _DeviceName; }
            set { Set(ref _DeviceName, value); }
        }

        private string _VariableId = "Item1";
        public string VariableId
        {
            get { return _VariableId; }
            set { Set(ref _VariableId, value); }
        }

        private string _VariableName;
        public string VariableName
        {
            get { return _VariableName; }
            set { Set(ref _VariableName, value); }
        }

        private string _Address = "0";
        public string Address
        {
            get { return _Address; }
            set { Set(ref _Address, value); }
        }

        private int _Size = 1;
        /// <summary>
        /// 레지스터 사이즈
        /// </summary>
        public int Size
        {
            get { return _Size; }
            set { Set(ref _Size, value); }
        }

        private int _DeciamlPoint = 0;
        /// <summary>
        ///  소수점 자리수. 1 => 0.0, 2=> 0.00
        /// </summary>
        public int DeciamlPoint
        {
            get { return _DeciamlPoint; }
            set { Set(ref _DeciamlPoint, value); }
        }


        public int Id { get; set; } = 0;
        /// <summary>
        /// Address주소에서 숫자. 예) D100 에서 100
        /// </summary>
        public int AddressNumber { get; set; }
        /// <summary>
        /// Address주소에서 문자. 예) D100 에서 D
        /// </summary>
        public string AddressSymbol { get; set; }

        private bool _IsUsed;
        public bool IsUsed
        {
            get { return _IsUsed; }
            set { Set(ref _IsUsed, value); }
        }

        private DataType _DataType = DataType.Word16;
        public DataType DataType
        {
            get { return _DataType; }
            set { Set(ref _DataType, value); }
        }


        private DataCategory _DataCategory;
        public DataCategory DataCategory
        {
            get { return _DataCategory; }
            set { Set(ref _DataCategory, value); }
        }


        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { Set(ref _Description, value); }
        }

        /// <summary>
        /// 어드레스항목을 도메인 논리에 따라 분리합니다. 예) Trigger1, 1-1-1
        /// </summary>
        private string _Group;
        public string Group
        {
            get { return _Group; }
            set { Set(ref _Group, value); }
        }

        private Dictionary<string, string> _MetaDatas;
        /// <summary>
        /// 기본 항목 이외의 파싱된 속성.
        /// </summary>
        public Dictionary<string, string> MetaDatas
        {
            get { return _MetaDatas; }
            set { Set(ref _MetaDatas, value); }
        }

        #endregion
    }

    public class ModbusAddressMap : AddressMap
    {
        #region 초기화
        public override string ToString()
        {
            return $"{base.ToString()}, SlaveId={SlaveId}, RegesterType={FunctionCode}";
        }

        public ModbusAddressMap() : base()
        {

        }

        public ModbusAddressMap(string deviceId, string variableId, string address, int size =1, int decimalPoint = 0, string variableName=null,
            int id=0, bool useYN=true, DataType dataType=DataType.Word16, DataCategory dataCategory=DataCategory.Data, 
            string description=null, string group=null,
            int slaveId = 1, FunctionCode functionCode = FunctionCode.HoldingRegister)
             : base(deviceId, variableId, address, size , decimalPoint, variableName, id, useYN, dataType, dataCategory, description, group)
        {
            Id = id;
            IsUsed = useYN;
            DataType = dataType;
            DataCategory = dataCategory;
            Description = description;
            Group = group;

            SlaveId = slaveId;
            FunctionCode = functionCode;
        }

        #endregion

        public int SlaveId { get; set; } = 1;
        public FunctionCode FunctionCode { get; set; } = FunctionCode.HoldingRegister;

    }

    
}
