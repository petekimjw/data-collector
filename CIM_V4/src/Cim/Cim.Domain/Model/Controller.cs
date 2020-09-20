using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Mvvm;

namespace Cim.Domain.Model
{
    public enum ControllerProtocol { None, Modbus, Melsec };

    public class Controller : BindableBase
    {
        private string _Name = "Controller";
        public string Name
        {
            get { return _Name; }
            set { Set(ref _Name, value); }
        }

        private string _Ip = "127.0.0.1";
        public string Ip
        {
            get { return _Ip; }
            set { Set(ref _Ip, value); }
        }

        private int _Port = 502;
        public int Port
        {
            get { return _Port; }
            set { Set(ref _Port, value); }
        }

        private int _LogicalStationNumber = 1;
        public int LogicalStationNumber
        {
            get { return _LogicalStationNumber; }
            set { Set(ref _LogicalStationNumber, value); }
        }

        private ControllerProtocol _Protocol = ControllerProtocol.Modbus;
        public ControllerProtocol Protocol
        {
            get { return _Protocol; }
            set { Set(ref _Protocol, value); }
        }

        /// <summary>
        /// AddressMap 이 정의된 시트목록. 예) Lami,STK
        /// </summary>
        private string _SheetNames = "Sheet1";
        public string SheetNames
        {
            get { return _SheetNames; }
            set { Set(ref _SheetNames, value); }
        }

        /// <summary>
        /// 기본속성 이외의 컬럼은 MetaDatas 에 key/value 형태로 파싱됩니다. 예) DataInterval=5000, StatusInterval=1000,
        /// </summary>
        private Dictionary<string, string> _MetaDatas;
        public Dictionary<string, string> MetaDatas
        {
            get { return _MetaDatas; }
            set { Set(ref _MetaDatas, value); }
        }

        private bool _IsUsed = true;
        public bool IsUsed
        {
            get { return _IsUsed; }
            set { Set(ref _IsUsed, value); }
        }

        private Dictionary<string, string> _CellAddresses;
        /// <summary>
        /// 엑셀에서 파싱시 Cell. 예) A1, B10
        /// </summary>
        [ReadOnly(true), Display(GroupName = "3.Etc", Order = 31)]
        public Dictionary<string, string> CellAddresses
        {
            get { return _CellAddresses; }
            set { Set(ref _CellAddresses, value); }
        }

        private int _Id = 0;
        public int Id
        {
            get { return _Id; }
            set { Set(ref _Id, value); }
        }

        private ObservableCollection<AddressMap> _AddressMaps;
        public ObservableCollection<AddressMap> AddressMaps
        {
            get { return _AddressMaps; }
            set { Set(ref _AddressMaps, value); }
        }

    }
}
