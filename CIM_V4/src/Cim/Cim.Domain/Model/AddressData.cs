using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;

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

        public AddressData(DateTime time, string deviceId, string variableId, string address, object value)
        {
            Time = time;
            DeviceId = deviceId;
            VariableId = variableId;
            Address = address;
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
            set { Set(ref _Value, value); }
        }

        public byte[] RawValues { get; set; }


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
