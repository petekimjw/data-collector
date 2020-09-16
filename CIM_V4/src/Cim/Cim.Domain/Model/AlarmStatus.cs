using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Domain.Model
{
    /// <summary>
    /// 알람상태(Off, On)
    /// </summary>
    public enum AlarmStatus
    {
        None = -1,

        /// <summary>
        /// 알람 해제
        /// </summary>
        Off = 0,

        /// <summary>
        /// 알람 설정
        /// </summary>
        On = 1,
    }
}
