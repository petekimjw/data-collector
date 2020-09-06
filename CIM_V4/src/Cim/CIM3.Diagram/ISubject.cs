using CIM.Diagram.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CIM.Diagram
{
    public interface ISubject : ICommand, ICancel, INotifyPropertyChanged
    {
        LocalServerInfo LocalServer { get; }
        /// <summary>
        /// 클라이언트 목록
        /// </summary>
        IEnumerable<IClientObserver> Controlls { get; }

        /// <summary>
        /// 관리자 상태
        /// </summary>
        EManagerStatus Status { get; set; }

        /// <summary>
        /// 통신 상태
        /// </summary>
        CommunicationState WebServiceState { get; }

    }

    public enum EManagerStatus { }

    

}