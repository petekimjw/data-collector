using AutoMapper;
using Cim.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Extensions;
using Tectone.Common.Mvvm;

namespace Cim.Manager.Views
{
    public class ControllerViewModel : ViewModelBase
    {

        public virtual ObservableCollection<ControllerManagerBase> ControllerManagers { get; set; }

        public virtual ControllerManagerBase SelectedControllerManager { get; set; }

        public virtual ObservableCollection<AddressData> AddressDatas { get; set; }

        public virtual ObservableCollection<AddressData> SelectedAddressDatas { get; set; }

        private ConfigManagerBase configManager = null;

        public ControllerViewModel()
        {
            configManager = new DefaultConfigManager();
            configManager.Init();

            ControllerManagers = configManager.ControllerManagers;

            var addressMaps = ControllerManagers?.FirstOrDefault().AddressMaps;
            AddressDatas = AutoMapper.Mapper.Map<ObservableCollection<AddressData>>(addressMaps);

            //데이터 수신
            foreach (var item in ControllerManagers)
            {
                foreach (var dataCollect in item.DataCollects)
                {
                    dataCollect.DataReceived -= DataCollect_DataReceived;
                    dataCollect.DataReceived += DataCollect_DataReceived;
                }
            }
        }

        private void DataCollect_DataReceived(object sender, AddressDataReceivedEventArgs e)
        {
            foreach (var item in e.AddressDatas)
            {
                var addressData = AddressDatas?.FirstOrDefault(m => m.VariableId == item.VariableId);
                if (addressData != null)
                    addressData.Value = item.Value;
            }
        }
    }
}
