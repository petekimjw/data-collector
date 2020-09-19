using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Extensions;

namespace Cim.Domain.Model
{
    public class AddressData : AddressMap
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public override string ToString()
        {
            return $"DeviceId={DeviceId}, VariableId={VariableId}, Address={Address}, Value={Value}, Time={Time.ToString("yyyy-MM-dd HH:mm:ss.fff")}";
        }
        public AddressData()
        {

        }

        public AddressData(DateTime time, string deviceId, string variableId, string address, object value) : base(deviceId, variableId, address)
        {
            Time = time;
            //DeviceId = deviceId;
            //VariableId = variableId;
            //Address = address;
            Value = value;
        }
        //public string DeviceName { get; set; }
        //public string VariableName { get; set; }
        //public string Address { get; set; }
        //public DataCategory DataCategory { get; set; } = DataCategory.Data;

        private DateTime _Time;
        public DateTime Time
        {
            get { return _Time; }
            set { Set(ref _Time, value); }
        }

        private object _Value;
        public object Value
        {
            get { return _Value; }
            set 
            { 
                (value, RawValues) = ConvertUShortValues(value, DataType, ByteOrder);  
                Set(ref _Value, value); 
            }
        }

        public List<byte> RawValues { get; set; }

        #region ConvertUShortValues

        /// <summary>
        /// ushort, ushort[]을 DataType 맞게 변환.
        /// </summary>
        /// <param name="value">입력(value)는 무조건 ushort, ushort[] 가정한다!</param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static (object, List<byte>) ConvertUShortValues(object value, DataType dataType, ByteOrder byteOrder)
        {
            //입력(value)는 무조건 ushort, ushort[] 가정한다!
            object result = null;
            (var byteValues, var ushortValues) = ConvertUShortsToBytes(value);

            try
            {
                //Melsec은 short[], Modbus는 ushort[] 으로 들어온다.
                byte[] temp;
                switch (dataType)
                {
                    case DataType.Bit:
                    case DataType.WordU16:
                        result = ushortValues[0];
                        break;
                    case DataType.Word16:
                        result = BitConverter.ToInt16(byteValues.ToArray(), 0);
                        break;

                    case DataType.Word32:
                        temp = Apply32BitByteOrder(byteValues.ToArray(), byteOrder);
                        result = BitConverter.ToInt32(temp, 0);
                        break;
                    case DataType.WordU32:
                        temp = Apply32BitByteOrder(byteValues.ToArray(), byteOrder);
                        result = BitConverter.ToUInt32(temp, 0);
                        break;
                    case DataType.Real32:
                        temp = Apply32BitByteOrder(byteValues.ToArray(), byteOrder);
                        result = BitConverter.ToSingle(temp, 0);
                        break;

                    case DataType.Real64:
                        temp = Apply64BitByteOrder(byteValues.ToArray(), byteOrder);
                        result = BitConverter.ToDouble(temp, 0);
                        break;

                    case DataType.String:
                        result = ApplyStringByteOrder(Encoding.ASCII.GetString(byteValues.ToArray()), byteOrder);
                        break;

                    case DataType.None:
                    default:
                        result = value;
                        break;
                }
            }
            catch { }
            return (result, byteValues);
        }

        /// <summary>
        /// ushort, ushort[] 을 byte[] 변환
        /// </summary>
        /// <param name="value">입력(value)는 무조건 ushort, ushort[] 가정한다!</param>
        /// <returns></returns>
        public static (List<byte>, ushort[]) ConvertUShortsToBytes(object value)
        {
            ushort[] values = null;
            List<byte> byteValues = new List<byte>();

            try
            {
                if (value is Array items)
                {
                    // ushort[] 변환
                    if (!(items.GetValue(0) is ushort))
                    {
                        values = new ushort[items.Length];
                        for (int i = 0; i < items.Length; i++)
                        {
                            values[i] = Convert.ToUInt16(items.GetValue(i));
                        }
                    }
                    else
                        values = (ushort[])value;

                    // List<byte> 변환
                    foreach (var item in values)
                    {
                        byteValues.AddRange(BitConverter.GetBytes(item));
                    }
                }
                else if (value is ushort)
                {
                    byteValues.AddRange(BitConverter.GetBytes((ushort)value));
                    values = new ushort[1] { (ushort)value };
                }
                else if(value is short)
                {
                    byteValues.AddRange(BitConverter.GetBytes((short)value));
                    values = new ushort[1] { BitConverter.ToUInt16(byteValues.ToArray(), 0) };
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }

            return (byteValues, values);
        }

        /// <summary>
        /// string 을 ByteOrder 적용.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="byteOrder"></param>
        /// <returns></returns>
        public static string ApplyStringByteOrder(string text, ByteOrder byteOrder)
        {
            string result = "";
            var chars = text.ToCharArray();

            int count = chars.Length / 4;

            switch (byteOrder)//todo: string byteorder
            {
                case ByteOrder.DCBA:
                case ByteOrder.ABCD:
                    for (int i = 0; i < count; i++)
                    {

                    }
                    break;
                case ByteOrder.BADC:
                case ByteOrder.CDAB:
                    break;
            }

            result = new string(chars);
            return result;
        }

        /// <summary>
        /// 64bit 에 ByteOrder 적용
        /// </summary>
        /// <param name="value"></param>
        /// <param name="byteOrder"></param>
        /// <returns></returns>
        public static byte[] Apply64BitByteOrder(byte[] bytes, ByteOrder byteOrder)
        {
            if (bytes.Length != 8) //64bit
                return null;

            //if(BitConverter.IsLittleEndian)//Windows 는 LittleEndian, Linux는 BigEndian
            byte[] results = new byte[bytes.Length];

            var temp = new List<byte>();
            temp.AddRange(Apply32BitByteOrder(bytes.Take(4).ToArray(), byteOrder));
            temp.AddRange(Apply32BitByteOrder(bytes.Skip(4).Take(4).ToArray(), byteOrder));
            results = temp.ToArray();

            return results;
        }

        /// <summary>
        ///  32bit 에 ByteOrder 적용
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="byteOrder"></param>
        /// <returns></returns>
        public static byte[] Apply32BitByteOrder(byte[] bytes, ByteOrder byteOrder)
        {
            if (bytes.Length == 4) //32bit
                return null;

            //if(BitConverter.IsLittleEndian)//Windows 는 LittleEndian, Linux는 BigEndian
            byte[] results = new byte[bytes.Length];

            byte a = bytes[0];
            byte b = bytes[1];
            byte c = bytes[2];
            byte d = bytes[3];

            switch (byteOrder)
            {
                case ByteOrder.ABCD:
                    results = new byte[4] { a, b, c, d };
                    break;
                case ByteOrder.CDAB:
                    results = new byte[4] { c, d, a, b };
                    break;
                case ByteOrder.BADC:
                    results = new byte[4] { b, a, d, c };
                    break;
                case ByteOrder.DCBA:
                    results = new byte[4] { d, c, b, a };
                    break;
            }
            return results;
        } 

        #endregion

        #region MQ

        /// <summary>
        /// 메시지 서버에 전송할 메시지 문자열을 반환<br/>
        /// 기본: {DeviceId}|{Time}|{VariableId}|{Value}
        /// </summary>
        /// <returns></returns>
        public virtual string ToMqString()
        {
            return $"{DeviceId}|{Time}|{VariableName}|{Value}";
        }

        /// <summary>
        /// RabbitMQ Exchange 에 바인딩 된 Queue로 분기할 때, 사용되는 라우팅키<br/>
        /// </summary>
        /// <returns></returns>
        public virtual string GetRoutingKey()
        {
            return $"{DeviceId}.{GetDataTypeString()}.{DataCategory}";
        }

        private string GetDataTypeString()
        {
            return double.TryParse(Value.ToString(), out double output) ? "N" : "S";
        }

        #endregion
    }

    public class AlarmAddressData : AddressData
    {
        public AlarmAddressData(DateTime time, string deviceName, string variableName, string address, object value)
            : base(time, deviceName, variableName, address, value)
        {

        }
        public DeviceStatus DeviceStatus { get; set; } = DeviceStatus.None;

        public AlarmStatus AlarmStatus { get; set; } = AlarmStatus.None;

        /// <summary>
        /// AddressType 이 Alarm 인 경우, 알람코드값을 설정하는 속성
        /// </summary>
        public int? AlarmCode { get; set; }

        /// <summary>
        /// 하위 알람코드 목록 (LS엠트론 사출기 알람시 서브 알람코드가 존재함)
        /// </summary>
        public string[] SubAlarmCodeList { get; set; }

        /// <summary>
        /// 메시지 서버에 전송할 메시지 문자열을 반환<br/>
        /// 기본: {DeviceId}|{Time}|{VariableId}|{Value}
        /// </summary>
        /// <returns></returns>
        public override string ToMqString()
        {
            // TODO 설비 상태 값을 MQ에 전송할때 코드값으로 전송
            if (DataCategory == DataCategory.Status)
            {
                return $"{DeviceId}|{Time}|{VariableName}|{(int)DeviceStatus}";
            }
            else
            {
                return $"{DeviceId}|{Time}|{VariableName}|{Value}";
            }
        }
    }

}
