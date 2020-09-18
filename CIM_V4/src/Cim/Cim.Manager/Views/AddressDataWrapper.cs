using Cim.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Manager.Views
{
    public class AddressDataWrapper : AddressData
    {
        public AddressDataWrapper()
        {

        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { Set(ref _IsSelected, value); }
        }

        private DateTime _LastDataCollectTime;
        public DateTime LastDataCollectTime
        {
            get { return _LastDataCollectTime; }
            set { Set(ref _LastDataCollectTime, value); }
        }


        private object _Value1;
        public object Value1
        {
            get { return _Value1; }
            set { Set(ref _Value1, value); }
        }

        private object _Value2;
        public object Value2
        {
            get { return _Value2; }
            set { Set(ref _Value2, value); }
        }

        private object _Value3;
        public object Value3
        {
            get { return _Value3; }
            set { Set(ref _Value3, value); }
        }

        private object _Value4;
        public object Value4
        {
            get { return _Value4; }
            set { Set(ref _Value4, value); }
        }

        private object _Value5;
        public object Value5
        {
            get { return _Value5; }
            set { Set(ref _Value5, value); }
        }

    }
}
