using AutoMapper;
using Cim.Domain.Model;
using Cim.Manager;
using Cim.Manager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tectone.Common.Mvvm;
using Tectone.Wpf.Common;
using Tectone.Wpf.Controls;

namespace OChang.AutoCotnrol.Pie
{
    public class ShellViewModel: WindowViewModelBase
    {

        public override void Init()
        {
            RegisterMessengers();

            //AutoMapper 기존거에 맵 추가하기
            //Cim.Domain.AutoMapper.BaseMapping.CreateMap<AddressData, AddressDataWrapper>();
            //Cim.Domain.AutoMapper.Init(Cim.Domain.AutoMapper.BaseMapping);
            
        }

        protected override void RegisterMessengers()
        {
            Messenger.Default.Register<BusyInfo>("BusyInfo", HandleBusyInfo);
        }

        public void HandleBusyInfo(BusyInfo busyInfo)
        {
            if (!string.IsNullOrEmpty(busyInfo.Message))
            {
                IsBusy = true;
                BusyMessage = busyInfo.Message;
            }
            else
            {
                IsBusy = false;
            }
        }

        #region Test

        public void Test1()
        {
            BusyMessage = "test 111";
            IsBusy = true;
        }

        public void Test2()
        {
            IsBusy = false;
        }

        #endregion

    }
}
