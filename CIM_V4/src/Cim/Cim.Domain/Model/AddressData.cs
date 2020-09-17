﻿using System;
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
        public override string ToString()
        {
            return $"Time={Time}, DeviceId={DeviceId}, VariableId={VariableId}, Address={Address}, Value={Value}";
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
                (value, RawValues) = ConvertValue(value, DataType);  
                Set(ref _Value, value); 
            }
        }

        public byte[] RawValues { get; set; }

        public static (object, byte[]) ConvertValue(object value, DataType dataType)
        {
            object result = null;
            byte[] bytes = null;
            try
            {
                //Melsec은 short[], Modbus는 ushort[] 으로 들어온다.
                switch (dataType)
                {
                    case DataType.Word16:
                        result = Convert.ToInt16(value);
                        break;
                    case DataType.Bit:
                    case DataType.WordU16:
                        result = Convert.ToUInt16(value);
                        break;

                    case DataType.Word32:
                        
                        result = Convert.ToInt32(value);
                        break;
                    case DataType.WordU32:
                        result = Convert.ToUInt32(value);
                        break;
                    case DataType.Real32:
                        result = Convert.ToSingle(value);
                        break;

                    case DataType.Real64:
                        result = Convert.ToDouble(value);
                        break;
                    case DataType.String:
                        break;

                    case DataType.None:
                    default:
                        result = value;
                        break;
                }
            }
            catch { }
            return (result, bytes);
        }

        /// <summary>
        /// 1개 워드 이상(32bit, 64bit, 문자열)만 변환해야 한다
        /// </summary>
        /// <param name="value"></param>
        /// <param name="byteOrder"></param>
        /// <returns></returns>
        public static object ApplyByteOrder(object value, ByteOrder byteOrder)
        {
            object result = null;
            switch (byteOrder)
            {
                case ByteOrder.ABCD:
                    int a = 1;
                    
                    break;
                case ByteOrder.CDAB:
                    break;
                case ByteOrder.BADC:
                    break;
                case ByteOrder.DCBA:
                    break;
                default:
                    break;
            }
            return result;
        }

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
