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

        #region 초기화

        #region 속성
        public virtual ObservableCollection<ControllerManagerBase> ControllerManagers { get; set; }

        public virtual ControllerManagerBase SelectedControllerManager { get; set; }

        public virtual ObservableCollection<AddressDataWrapper> AddressDatas { get; set; }

        public virtual ObservableCollection<AddressDataWrapper> SelectedAddressDatas { get; set; }

        private ConfigManagerBase configManager = null;
        #endregion

        public ControllerViewModel()
        {
            configManager = new DefaultConfigManager();
            configManager.Init();

            ControllerManagers = configManager.ControllerManagers;

            AutoMapper.Config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AddressMap, AddressMap>();
                cfg.CreateMap<AddressMap, ModbusAddressMap>();
                cfg.CreateMap<AddressMap, AddressData>();
                cfg.CreateMap<ModbusAddressMap, AddressData>();
                cfg.CreateMap<AddressMap, AddressDataWrapper>();
                cfg.CreateMap<AddressData, AddressDataWrapper>();
            });
            AutoMapper.Mapper = new Mapper(AutoMapper.Config);

            var addressMaps = ControllerManagers?.FirstOrDefault()?.Controller.AddressMaps;
            if(addressMaps?.Count > 0)
                AddressDatas = AutoMapper.Mapper.Map<ObservableCollection<AddressDataWrapper>>(addressMaps);

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
        #endregion

        #region DataCollect_DataReceived

        private void DataCollect_DataReceived(object sender, AddressDataReceivedEventArgs e)
        {
            foreach (var item in e.AddressDatas)
            {
                var addressData = AddressDatas?.FirstOrDefault(m => m.VariableId == item.VariableId);
                if (addressData != null)
                    addressData.Value = item.Value;
            }
        } 

        #endregion
    }

    public class AddressDataWrapper : AddressData
    {

    }

}
