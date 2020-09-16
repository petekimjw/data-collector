using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Mvvm;

namespace Cim.Domain.Model
{
    public enum ControllerProtocol { None, Modbus, Melsec };

    public class Controller : BindableBase
    {
        public string Name { get; set; } = "Controller";
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 502;
        public int LogicalStationNumber { get; set; } = 1;
        public ControllerProtocol Protocol { get; set; } = ControllerProtocol.Modbus;
        /// <summary>
        /// AddressMap 이 정의된 시트목록. 예) Lami,STK
        /// </summary>
        public string SheetNames { get; set; } = "Sheet1";

        /// <summary>
        /// 기본속성 이외의 컬럼은 MetaDatas 에 key/value 형태로 파싱됩니다. 예) DataInterval=5000, StatusInterval=1000,
        /// </summary>
        public Dictionary<string, string> MetaDatas { get; set; }
        
        public int Id { get; set; }

        private ObservableCollection<AddressMap> _AddressMaps;
        public ObservableCollection<AddressMap> AddressMaps
        {
            get { return _AddressMaps; }
            set { Set(ref _AddressMaps, value); }
        }

    }
}
