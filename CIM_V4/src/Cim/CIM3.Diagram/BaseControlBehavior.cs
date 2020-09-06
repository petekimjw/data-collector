using CIM.Diagram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Diagram
{
    public abstract class BaseControlBehavior<T, R> : IControlBehavior<T, R>
            where T : Message, new()
            where R : class, new()
    {
        public virtual bool CanDeviceStatus { get; }
        public virtual bool IgnoreParallelDeviceStatus { get; }

        protected IClient<T, R> Client;

        public List<IProcess> GetProcessList()
        {
            throw new NotImplementedException();
        }

        public List<T> GetProcessingDeviceStatusMessageList(List<T> messages)
        {
            throw new NotImplementedException();
        }

        public List<T> GetProcessingAlarmMessageList(List<T> messages)
        {
            throw new NotImplementedException();
        }

        public List<T> GetProcessingMessageList(List<T> messages)
        {
            throw new NotImplementedException();
        }

        public List<T> GetProcessingTriggerMessageList(List<T> messages)
        {
            throw new NotImplementedException();
        }

        public void SetRestMessageList(List<T> messages)
        {
            throw new NotImplementedException();
        }

        public void OnResponseData(R responseData)
        {
            throw new NotImplementedException();
        }
    }
}
