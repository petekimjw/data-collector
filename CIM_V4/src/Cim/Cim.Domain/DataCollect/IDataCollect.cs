using Cim.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Domain.DataCollect
{
    public interface IDataCollect
    {
        string Name { get; set; }
        void Start();
        void Stop();
        event EventHandler<AddressDataReceivedEventArgs> DataReceived;
    }
}
