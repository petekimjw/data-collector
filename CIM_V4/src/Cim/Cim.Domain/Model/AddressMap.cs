using Cim.Domain.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Mvvm;

namespace Cim.Domain.Model
{
    /// <summary>
    /// 32bit 일때 바이트오더. A(8bit), B(8bit), C(8bit), D(8bit)=2Words
    /// </summary>
    public enum ByteOrder { ABCD, CDAB, BADC, DCBA }
    /// <summary>
    /// 자료형. int, short, float 가 아닌, PLC 입장에서 자료형(Bit, Word16, Word32, String)
    /// </summary>
    public enum DataType { None, Bit, Word16, Word32, String, Real32, Real64, WordU16, WordU32 }
    /// <summary>
    /// 데이터 성격별 분류 (Status, Alarm, Data)
    /// </summary>
    public enum DataCategory { Data, Status, Alarm }

    public class AddressMap : BindableBase
    {
        #region 초기화
        public override string ToString()
        {
            return $"DeviceId={DeviceId}, Id={Id}, VariableId={VariableId}, VariableName={VariableName}, Address={Address}, Size={Size}, Scale={DeciamlPoint}, " +
                $"UseYN={IsUsed}, DataType={DataType}, DataCategory={DataCategory}," +
                $"Group={Group}, ByteOrder={ByteOrder}";
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AddressMap()
        {

        }

        public AddressMap(string deviceId, string variableId, string address, int size =1, int decimalPoint=0, string variableName = null,
            int id=0, bool useYN=true, DataType dataType=DataType.Word16, DataCategory dataCategory=DataCategory.Data, 
            string description=null, string group=null, ByteOrder byteOrder = ByteOrder.CDAB,
            int slaveId = 1, FunctionCode functionCode = FunctionCode.None)
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
            ByteOrder = byteOrder;
            DataCategory = dataCategory;
            Group = group;

            SlaveId = slaveId;
            FunctionCode = functionCode;
        }
        #endregion

        #region 속성 (Required)
        private string _DeviceName = "Controller";
        /// <summary>
        /// 장치의 Id
        /// </summary>
        [Display(GroupName="1.Required", Order=1, Name ="Device Id", Description ="Device Id. Ex) Lami, Stakcer, Coater")]
        public string DeviceId
        {
            get { return _DeviceName; }
            set { Set(ref _DeviceName, value); }
        }

        private string _VariableId = "Item1";
        /// <summary>
        /// 어드레스주소 Id, 변수이름, 라벨이름
        /// </summary>
        [Display(GroupName = "1.Required", Order = 2, Name = "Variable Id", Description = "Address Id, Variable, Label Name")]
        public string VariableId
        {
            get { return _VariableId; }
            set { Set(ref _VariableId, value); }
        }

        private string _Address = "0";
        /// <summary>
        /// PLC 디바이스 주소. 
        /// </summary>
        [Display(GroupName = "1.Required", Order = 3, Name = "PLC Device Address", Description = "PLC Device Address. Ex) D10, 20")]
        public string Address
        {
            get { return _Address; }
            set { Set(ref _Address, value); }
        }

        private int _Size = 1;
        /// <summary>
        /// 레지스터 사이즈
        /// </summary>
        [Display(GroupName = "1.Required", Order = 4, Name = "Register Size", Description = "Register Size. Ex) 2=2Words")]
        public int Size
        {
            get { return _Size; }
            set { Set(ref _Size, value); }
        }

        private int _DeciamlPoint = 0;
        /// <summary>
        ///  소수점 자리수. 1 => 0.0, 2=> 0.00
        /// </summary>
        [Display(GroupName = "1.Required", Order = 5, Name = "Deciaml Point", Description = "Deciaml Point. Ex) 1 => 0.0, 2=> 0.00")]
        public int DeciamlPoint
        {
            get { return _DeciamlPoint; }
            set { Set(ref _DeciamlPoint, value); }
        }

        private DataType _DataType = DataType.Word16;
        /// <summary>
        /// 자료형. int, short, float 가 아닌, PLC 입장에서 자료형(Bit, Word16, Word32, String)
        /// </summary>
        [Display(GroupName = "1.Required", Order = 6, Name = "Data Type", Description = "PLC Data Type(Bit, Word16, Word32, String)")]
        public DataType DataType
        {
            get { return _DataType; }
            set { Set(ref _DataType, value); }
        }

        private ByteOrder _ByteOrder;
        /// <summary>
        /// 32bit 일때 바이트오더. A(8bit), B(8bit), C(8bit), D(8bit)=2Words
        /// </summary>
        [Display(GroupName = "1.Required", Order = 7, Name = "Byte Order", Description = "32bit. A(8bit), B(8bit), C(8bit), D(8bit)=2Words")]
        public ByteOrder ByteOrder
        {
            get { return _ByteOrder; }
            set { Set(ref _ByteOrder, value); }
        }

        #endregion

        #region 속성-Category

        private DataCategory _DataCategory;
        [Display(GroupName = "2.Category", Order = 11, Name = "DataCategory", Description = "Data Category (Status, Alarm, Data)")]
        public DataCategory DataCategory
        {
            get { return _DataCategory; }
            set { Set(ref _DataCategory, value); }
        }

        /// <summary>
        /// 어드레스항목을 도메인 논리에 따라 분리합니다. 예) Trigger1, 1-1-1
        /// </summary>
        private string _Group;
        [Display(GroupName = "2.Category", Order = 12, Name = "Group", Description = "Ex) Trigger1, 1-1-1")]
        public string Group
        {
            get { return _Group; }
            set { Set(ref _Group, value); }
        }

        #endregion

        #region 속성

        private Dictionary<string, string> _CellAddresses;
        /// <summary>
        /// 엑셀에서 파싱시 Cell. 예) A1, B10
        /// </summary>
        [ReadOnly(true), Display(GroupName = "3.Etc", Order = 31)]
        public Dictionary<string, string> CellAddresses
        {
            get { return _CellAddresses; }
            set { Set(ref _CellAddresses, value); }
        }

        private int _Id;
        [Display(GroupName = "3.Etc", Order = 31)]
        public int Id
        {
            get { return _Id; }
            set { Set(ref _Id, value); }
        }

        private string _VariableName;
        [Display(GroupName = "3.Etc", Order = 31)]
        public string VariableName
        {
            get { return _VariableName; }
            set { Set(ref _VariableName, value); }
        }

        /// <summary>
        /// Address주소에서 숫자. 예) D100 에서 100
        /// </summary>
        private int _AddressNumber;
        [Display(GroupName = "3.Etc", Order = 31)]
        public int AddressNumber
        {
            get { return _AddressNumber; }
            set { Set(ref _AddressNumber, value); }
        }

        /// <summary>
        /// Address주소에서 문자. 예) D100 에서 D
        /// </summary>
        private string _AddressSymbol;
        [Display(GroupName = "3.Etc", Order = 31)]
        public string AddressSymbol
        {
            get { return _AddressSymbol; }
            set { Set(ref _AddressSymbol, value); }
        }

        private bool _IsUsed;
        [Display(GroupName = "3.Etc", Order = 31)]
        public bool IsUsed
        {
            get { return _IsUsed; }
            set { Set(ref _IsUsed, value); }
        }

        private Dictionary<string, string> _MetaDatas;
        /// <summary>
        /// 기본 항목 이외의 파싱된 속성.
        /// </summary>
        [Display(GroupName = "3.Etc", Order = 31)]
        public Dictionary<string, string> MetaDatas
        {
            get { return _MetaDatas; }
            set { Set(ref _MetaDatas, value); }
        }

        #endregion

        #region Modbus

        private int _SlaveId = 1;
        [Display(GroupName = "5.Modbus", Order = 51, Name = "SlaveId", Description = "SlaveId")]
        public int SlaveId
        {
            get { return _SlaveId; }
            set { Set(ref _SlaveId, value); }
        }

        private FunctionCode _FunctionCode = FunctionCode.None;
        /// <summary>
        /// Modbus Function Code. Bit(CoilStatus = 0, InputStatus=1) Word(InputRegister = 2, HoldingRegister =3)
        /// </summary>
        [Display(GroupName = "5.Modbus", Order = 52, Name = "FunctionCode", Description = "Modbus Function Code. Bit(CoilStatus = 0, InputStatus=1) Word(InputRegister = 2, HoldingRegister =3)")]
        public FunctionCode FunctionCode
        {
            get { return _FunctionCode; }
            set { Set(ref _FunctionCode, value); }
        }


        #endregion
    }

    //주의! 공변성/반공변성 어려움으로 AddressMap으로 통일
    //public class ModbusAddressMap : AddressMap
    //{
    //    #region 초기화
    //    public override string ToString()
    //    {
    //        return $"{base.ToString()}, SlaveId={SlaveId}, RegesterType={FunctionCode}";
    //    }

    //    public ModbusAddressMap() : base()
    //    {

    //    }

    //    public ModbusAddressMap(string deviceId, string variableId, string address, int size =1, int decimalPoint = 0, string variableName=null,
    //        int id=0, bool useYN=true, DataType dataType=DataType.Word16, DataCategory dataCategory=DataCategory.Data, 
    //        string description=null, string group=null, ByteOrder byteOrder = ByteOrder.CDAB,
    //        int slaveId = 1, FunctionCode functionCode = FunctionCode.HoldingRegister)
    //         : base(deviceId, variableId, address, size , decimalPoint, variableName, id, useYN, dataType, dataCategory, description, group, byteOrder)
    //    {
    //        Id = id;
    //        IsUsed = useYN;
    //        DataType = dataType;
    //        DataCategory = dataCategory;
    //        Description = description;
    //        Group = group;

    //        SlaveId = slaveId;
    //        FunctionCode = functionCode;
    //    }

    //    #endregion

    //    public int SlaveId { get; set; } = 1;
    //    public FunctionCode FunctionCode { get; set; } = FunctionCode.None;

    //}

    
}
