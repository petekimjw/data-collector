using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Model
{
    /// <summary>
    /// 설비 상태(Idle, Run, Down)
    /// </summary>
    public enum DeviceStatus
    {
        None = -1,

        /// <summary>
        /// Idle
        /// </summary>
        Idle = 0,

        /// <summary>
        /// Run
        /// </summary>
        Run = 1,

        /// <summary>
        /// Down (설비와 네트워크 연결은 정상이나 설비쪽에서 통신 오류가 발생한 경우)
        /// </summary>
        Down = 2,

        /// <summary>
        /// 설비와 데이터 수집시 연결이 끊어진 경우
        /// </summary>
        DisConnected = 3,
    }
}
