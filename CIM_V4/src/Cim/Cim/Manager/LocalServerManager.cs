using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Manager
{
    public class LocalServerManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public List<ControllerManager> ControllerManagers { get; set; } = new List<ControllerManager>();

        public LocalServerManager()
        {

        }
    }
}
