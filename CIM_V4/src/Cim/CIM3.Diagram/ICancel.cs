using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CIM.Diagram
{
    public interface ICancel
    {
        /// <summary>
        /// 취소 요청 여부
        /// </summary>
        bool IsCancelRequested { get; set; }
    }
}