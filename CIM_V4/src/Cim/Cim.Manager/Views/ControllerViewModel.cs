using AutoMapper;
using Cim.Domain;
using Cim.Domain.Manager;
using Cim.Domain.Model;
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

        public ControllerViewModel() : base()
        {
            configManager = new DefaultConfigManager();
            configManager.Init();
            LoadControllerManagers();
        }

        public void LoadControllerManagers()
        {
            ControllerManagers = configManager.ControllerManagers;

            Cim.Domain.AutoMapper.Config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AddressMap, AddressMap>();
                cfg.CreateMap<AddressMap, ModbusAddressMap>();
                cfg.CreateMap<AddressMap, AddressData>();
                cfg.CreateMap<ModbusAddressMap, AddressData>();
                cfg.CreateMap<AddressMap, AddressDataWrapper>();
                cfg.CreateMap<AddressData, AddressDataWrapper>();
            });
            Cim.Domain.AutoMapper.Mapper = new Mapper(Cim.Domain.AutoMapper.Config);

            var addressMaps = ControllerManagers?.FirstOrDefault()?.Controller.AddressMaps;
            if (addressMaps?.Count > 0)
                AddressDatas = Cim.Domain.AutoMapper.Mapper.Map<ObservableCollection<AddressDataWrapper>>(addressMaps);

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

        #region 명령

        public async Task ReloadAddressMapAndCreateManager()
        {
            await configManager.ReloadAddressMapAndCreateManager();

            LoadControllerManagers();
        }

        public void Start()
        {
            configManager.Start();
        }

        public void Stop()
        {
            configManager.Stop();
        }

        private int _ContinuousMonitorInterval = 5;
        public int ContinuousMonitorInterval
        {
            get { return _ContinuousMonitorInterval; }
            set { Set(ref _ContinuousMonitorInterval, value); }
        }

        public void GetDatas()
        {

        }

        public void StartContinuousMonitor()
        {

        }

        public void ClearMonitor()
        {

        }

        #endregion

    }

    public class AddressDataWrapper : AddressData
    {

    }

}
