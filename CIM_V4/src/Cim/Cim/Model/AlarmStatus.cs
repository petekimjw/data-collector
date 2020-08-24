using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Model
{
    /// <summary>
    /// 알람값 설정 관련
    /// </summary>
    public enum AlarmStatus
    {
        None = -1,

        /// <summary>
        /// 알람 해제
        /// </summary>
        OFF = 0,

        /// <summary>
        /// 알람 설정
        /// </summary>
        ON = 1,
    }
}
