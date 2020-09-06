using Cim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.DataCollect
{
    public interface IDataCollect
    {
        void Start();
        void Stop();
        event EventHandler<AddressDataReceivedEventArgs> DataReceived;
    }
}
