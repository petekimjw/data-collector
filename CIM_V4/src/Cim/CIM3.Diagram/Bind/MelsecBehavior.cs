using CIM.Diagram.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Diagram.Bind
{
    public class MelsecBehavior : BaseDeviceBehavior, IBindBehavior, INotifyPropertyChanged
    {
        public EControllerStatusKind ControllerStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged;

        public Task<bool> CloseAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public List<AddressMapInfo> GetAddressMapList()
        {
            throw new NotImplementedException();
        }

        public List<T> Read<T>(List<AddressMapInfo> addressMaps) where T : Message, new()
        {
            throw new NotImplementedException();
        }

        public bool Write(AddressMapInfo addressMap, object value)
        {
            throw new NotImplementedException();
        }
    }

    public class ModbusBehavior : BaseDeviceBehavior, IBindBehavior, INotifyPropertyChanged
    {
        public EControllerStatusKind ControllerStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged;

        public Task<bool> CloseAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public List<AddressMapInfo> GetAddressMapList()
        {
            throw new NotImplementedException();
        }

        public List<T> Read<T>(List<AddressMapInfo> addressMaps) where T : Message, new()
        {
            throw new NotImplementedException();
        }

        public bool Write(AddressMapInfo addressMap, object value)
        {
            throw new NotImplementedException();
        }
    }
}
