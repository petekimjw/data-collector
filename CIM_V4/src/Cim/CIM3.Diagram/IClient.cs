using CIM.Diagram.Bind;
using CIM.Diagram.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Diagram
{
    public interface IClient<T, R> : ICancel
            where T : Message, new()
            where R : class, new()
    {
        ControllerInfo Controller { get; }

        IControlBehavior<T, R> Logic { get; set; }

        IBindBehavior Binder { get; set; }


        List<AddressMapInfo> AddressMapList { get; }

        ENetworkStatusKind NetworkStatus { get; set; }

        void SendMessage(params T[] messages);

    }

    public enum ENetworkStatusKind { }

    public interface IClientObserver : ICommand, ICancel, INotifyPropertyChanged
    {
        ControllerInfo Controller { get; }
        ControllerEntity GetControllerEntity();

        EControllerStatusKind ControllerStatus { get; }
        ENetworkStatusKind NetworkStatus { get; set; }

    }
}
