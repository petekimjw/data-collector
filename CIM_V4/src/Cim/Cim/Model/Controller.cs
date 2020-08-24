using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Model
{
    public class Controller
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public Dictionary<string, string> MetaDatas { get; set; }
        
        public int Id { get; set; }
    }
}
