using CIM.Diagram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Diagram
{
    public interface IControlBehavior<T, R>
            where T : Message, new()
            where R : class, new()
    {
        /// <summary>
        /// 상태 사용여부<br/>
        /// false(미사용, 기본값) / true(사용)<br/>
        /// false 시, 장비 연결 상태로 연결/미연결 사용<br/>
        /// true 시, 장비 상태(어드레스멥에 지정해준 DeviceStatus Type) 사용
        /// </summary>
        bool CanDeviceStatus { get; }

        /// <summary>
        /// 컨트롤러가 Connected 이고 디바이스가 DisConnected 일 때 => None으로 표시<br/>
        /// 하나의 PLC에서 일부 설비의 상태를 제공하지 않는 설비(공정을 위해 등록한 설비) => None으로 표시<br/>
        /// false(로직사용, 기본값) / true(로직무시)<br/>
        /// false 시, None으로 바꾸어 주는 로직 사용<br/>
        /// true 시, 로직 미사용
        /// </summary>
        bool IgnoreParallelDeviceStatus { get; }

        /// <summary>
        /// 프로세스 모음 반환
        /// </summary>
        List<IProcess> GetProcessList();

        /// <summary>
        /// (상태 관련) 장비에서 수집한 메시지를 추가적으로 가공한 메시지 목록 반환
        /// </summary>
        List<T> GetProcessingDeviceStatusMessageList(List<T> messages);

        /// <summary>
        /// (알람 관련) 장비에서 수집한 메시지를 추가적으로 가공한 메시지 목록 반환
        /// </summary>
        List<T> GetProcessingAlarmMessageList(List<T> messages);

        /// <summary>
        /// (수집 관련) 장비에서 수집한 메시지를 추가적으로 가공한 메시지 목록 반환
        /// </summary>
        List<T> GetProcessingMessageList(List<T> messages);

        /// <summary>
        /// (시나리오 관련) 장비에서 수집한 메시지를 추가적으로 가공한 메시지 목록 반환
        /// </summary>
        List<T> GetProcessingTriggerMessageList(List<T> messages);

        /// <summary>
        /// 웹 서비스에서 받은 메시지를 처리하기위해 구현
        /// </summary>
        void SetRestMessageList(List<T> messages);

        /// <summary>
        /// 메시지 전송후 처리하기 위한 데이터
        /// </summary>
        void OnResponseData(R responseData);
    }
}
