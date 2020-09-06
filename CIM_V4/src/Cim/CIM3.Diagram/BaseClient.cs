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
    public class BaseClient<T, R> : IClientObserver, IClient<T, R>
            where T : Message, new()
            where R : class, new()
    {
        public IControlBehavior<T, R> Logic { get; set; }
        public IBindBehavior Binder { get; set; }
        /// <summary>
        /// 사용자의 취소 요청여부를 가져오거나 설정
        /// </summary>
        public bool IsCancelRequested { get; set; } = true;


        public ControllerInfo Controller { get; }

        public EControllerStatusKind ControllerStatus => Binder.ControllerStatus;

        public int Interval
        {
            get => Controller.Interval;
            set => Controller.Interval = value;
        }

        public int StatusInterval
        {
            get => Controller.StatusInterval;
            set => Controller.StatusInterval = value;
        }

        public bool IsUseMessageLog
        {
            get => Controller.UseLogSave;
            set => Controller.UseLogSave = value;
        }

        public bool IsSwitch { get; set; }

        public List<AddressMapInfo> AddressMapList { get; set; }

        public List<TimerHelper> TimerList { get; set; }

        public ENetworkStatusKind NetworkStatus { get; set; }
        IBindBehavior IClient<T, R>.Binder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetToggleControllerSwitch()
        {
            IsSwitch = !IsSwitch;
        }

        public void SendRestMessage(DataItem item)
        {
  
        }

        public bool WriteRest(AddressMapInfo addressMap, object value)
        {
            return Binder.Write(addressMap, value);
        }

        public void Start() { }

        public async Task<bool> StopAsync() { return true; }

        public async Task<bool> CloseAsync() { return true; }

        public async Task RestartAsync() { }

        public void SendMessage(params T[] messages) { }

        public ControllerEntity GetControllerEntity()
        {
            throw new NotImplementedException();
        }
    }

    public class ControllerEntity { }
    public class DataItem { }
    public class TimerHelper { }
}
