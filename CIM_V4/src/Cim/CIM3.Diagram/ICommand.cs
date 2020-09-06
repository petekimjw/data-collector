using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Diagram
{
    public interface ICommand
    {
        /// <summary>
        /// 수집 시작
        /// </summary>
        void Start();

        /// <summary>
        /// 수집 중지
        /// </summary>
        Task<bool> StopAsync();

        /// <summary>
        /// 수집 종료
        /// </summary>
        Task<bool> CloseAsync();

        /// <summary>
        /// 수집 재시작
        /// </summary>
        Task RestartAsync();
    }
}