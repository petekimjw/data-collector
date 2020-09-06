using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Diagram
{
    public class MainForm
    {
        public void Init()
        {
            // (ISubject)Activator.CreateInstance(Globals.Settings.ClientManagerType)}
            // CIM3.Cnb.Bts.Client.ClientManager, CIM
            ISubject manager = ISAManagerFactory.CreateISAManager();


            //public class ClientManager : ClientManager<Message, object>

        }


    }

    public class ISAManagerFactory
    {
        public static ISubject CreateISAManager() { throw new NotImplementedException(); }
    }
    public class Globals
    {
        public static class Settings
        {
            public static Type ClientManagerType { get; set; }
        }
    }

}
