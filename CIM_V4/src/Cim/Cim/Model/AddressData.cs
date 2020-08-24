using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Model
{
    public class AddressData
    {
        public override string ToString()
        {
            return $"Time={Time}, DeviceId={DeviceName}, VariableId={VariableName}, Address={Address}, Value={Value}";
        }

        public AddressData(DateTime time, string deviceName, string variableName, string address, object value)
        {
            Time = time;
            DeviceName = deviceName;
            VariableName = variableName;
            Address = address;
            Value = value;
        }
        public DateTime Time { get; set; }
        public string DeviceName { get; set; }
        public string VariableName { get; set; }
        public string Address { get; set; }
        public object Value { get; set; }
        public byte[] RawValues { get; set; }

        public DataCategory DataCategory { get; set; } = DataCategory.Data;

        #region MQ

        /// <summary>
        /// 메시지 서버에 전송할 메시지 문자열을 반환<br/>
        /// 기본: {DeviceId}|{Time}|{VariableId}|{Value}
        /// </summary>
        /// <returns></returns>
        public virtual string ToMqString()
        {
            return $"{DeviceName}|{Time}|{VariableName}|{Value}";
        }

        /// <summary>
        /// RabbitMQ Exchange 에 바인딩 된 Queue로 분기할 때, 사용되는 라우팅키<br/>
        /// </summary>
        /// <returns></returns>
        public virtual string GetRoutingKey()
        {
            return $"{DeviceName}.{GetDataTypeString()}.{DataCategory}";
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
                return $"{DeviceName}|{Time}|{VariableName}|{(int)DeviceStatus}";
            }
            else
            {
                return $"{DeviceName}|{Time}|{VariableName}|{Value}";
            }
        }
    }

}
