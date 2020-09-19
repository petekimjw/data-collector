using Cim.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Mvvm;

namespace Cim.Manager.Views
{
    public class AddressDataWrapper : AddressData
    {
        public override string ToString()
        {
            return $"{base.ToString()}, State={State}, LastDataCollectTime={LastDataCollectTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}, Value1={Value1}, Value2={Value2}, Value3={Value3}, Value4={Value4}, Value5={Value5}";
        }
        public AddressDataWrapper()
        {

        }

        private State _State = State.None;
        /// <summary>
        /// 모델의 변경상태
        /// </summary>
        [Browsable(false)]
        public State State
        {
            get { return _State; }
            set { Set(ref _State, value); }
        }

        private bool _IsSelected;
        [Browsable(false)]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { Set(ref _IsSelected, value); }
        }

        private DateTime _LastDataCollectTime;
        [Browsable(false)]
        public DateTime LastDataCollectTime
        {
            get { return _LastDataCollectTime; }
            set { Set(ref _LastDataCollectTime, value); }
        }


        private object _Value1;
        [Browsable(false)]
        public object Value1
        {
            get { return _Value1; }
            set { Set(ref _Value1, value); }
        }

        private object _Value2;
        [Browsable(false)]
        public object Value2
        {
            get { return _Value2; }
            set { Set(ref _Value2, value); }
        }

        private object _Value3;
        [Browsable(false)]
        public object Value3
        {
            get { return _Value3; }
            set { Set(ref _Value3, value); }
        }

        private object _Value4;
        [Browsable(false)]
        public object Value4
        {
            get { return _Value4; }
            set { Set(ref _Value4, value); }
        }

        private object _Value5;
        [Browsable(false)]
        public object Value5
        {
            get { return _Value5; }
            set { Set(ref _Value5, value); }
        }

    }
}
