using Autofac;
using AutoMapper;
using Cim.Domain;
using Cim.Domain.Manager;
using Cim.Domain.Model;
using Cim.Manager.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Extensions;
using Tectone.Common.Mvvm;
using Tectone.Wpf.Common;

namespace Cim.Manager.Views
{
    public class ControllerViewModel : ViewModelBase
    {

        #region 초기화

        #region 속성
        public virtual ObservableCollection<ControllerManagerBase> ControllerManagers { get; set; } = new ObservableCollection<ControllerManagerBase>();

        public virtual ControllerManagerBase SelectedControllerManager { get; set; }

        public virtual ObservableCollection<AddressDataWrapper> AddressDatas { get; set; } = new ObservableCollection<AddressDataWrapper>();

        public virtual ObservableCollection<AddressDataWrapper> SelectedAddressDatas { get; set; } = new ObservableCollection<AddressDataWrapper>();

        private ConfigManagerBase configManager = null;

        #endregion

        public ControllerViewModel() : base()
        {
            //IoC
            MessageBox = Container?.Resolve<IMessageBox>();
            AlertManager = Container?.Resolve<IAlertManager>();

            if (GetIsInDesignMode()) return;

            //AutoMapper 기존거에 맵 추가하기
            Domain.AutoMapper.BaseMapping.CreateMap<AddressMap, AddressDataWrapper>();
            Domain.AutoMapper.BaseMapping.CreateMap<AddressData, AddressDataWrapper>();
            Domain.AutoMapper.Init(Domain.AutoMapper.BaseMapping);

            configManager = new DefaultConfigManager();
            configManager.Init();

            LoadControllerManagers();
        }

        public void LoadControllerManagers()
        {
            ControllerManagers = configManager.ControllerManagers;

            if (ControllerManagers.Count == 0)
                AlertManager.ShowAlert("Can't open Addressmap Excel !", styleName: "RedDesktopAlertStyle");

            SelectedControllerManager = ControllerManagers?.FirstOrDefault();

            var addressMaps = SelectedControllerManager?.Controller.AddressMaps;
            if (addressMaps?.Count > 0)
                AddressDatas = Domain.AutoMapper.Mapper.Map<ObservableCollection<AddressDataWrapper>>(addressMaps);

            //데이터 수신
            //foreach (var manager in ControllerManagers)
            //{
                //foreach (var dataCollect in item.DataCollects)
                //{
                //    dataCollect.DataReceived -= DataCollect_DataReceived;
                //    dataCollect.DataReceived += DataCollect_DataReceived;
                //}
            //}
        }
        #endregion

        #region DataCollect_DataReceived

        private void DataCollect_DataReceived(object sender, AddressDataReceivedEventArgs e)
        {
            foreach (var item in e.AddressDatas)
            {
                var addressData = AddressDatas?.FirstOrDefault(m => m.VariableId == item.VariableId);
                if (addressData != null)
                {
                    if (addressData.Value1 == null)
                        addressData.Value1 = item.Value;
                    else if (addressData.Value2 == null)
                        addressData.Value2 = item.Value;
                    else if (addressData.Value3 == null)
                        addressData.Value3 = item.Value;
                    else if (addressData.Value4 == null)
                        addressData.Value4 = item.Value;
                    else if (addressData.Value5 == null)
                        addressData.Value5 = item.Value;
                    else
                    {
                        addressData.Value1 = item.Value;
                        addressData.Value2 = null;
                        addressData.Value3 = null;
                        addressData.Value4 = null;
                        addressData.Value5 = null;
                    }
                }
            }
        }

        #endregion

        #region 명령

        public async Task ReloadAddressMapAndCreateManager()
        {
            Messenger.Default.Send(new BusyInfo(50, Tectone.Common.Resources.LocalizedStrings.Wait));

            await configManager.ReloadAddressMapAndCreateManager();

            LoadControllerManagers();

            Messenger.Default.Send(new BusyInfo(-1, null));
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

        /// <summary>
        /// 모니터링
        /// </summary>
        /// <returns></returns>
        public async Task GetDatas()
        {
            if (SelectedControllerManager == null) return;
            if (!(SelectedAddressDatas?.Count > 0)) return;

            var maps = SelectedAddressDatas.Cast<AddressMap>().ToList();

            var groups = SelectedControllerManager.MonitorDataCollect.GroupingAddressMaps(maps);
            foreach (var group in groups)
            {
                var datas = await SelectedControllerManager.MonitorDataCollect.ReadAddressMapsInternal(group.Value);
                foreach (var item in datas)
                {
                    var addressData = AddressDatas?.FirstOrDefault(m => m.VariableId == item.VariableId);
                    if (addressData != null)
                    {
                        if (addressData.Value1 == null)
                            addressData.Value1 = item.Value;
                        else if (addressData.Value2 == null)
                            addressData.Value2 = item.Value;
                        else if (addressData.Value3 == null)
                            addressData.Value3 = item.Value;
                        else if (addressData.Value4 == null)
                            addressData.Value4 = item.Value;
                        else if (addressData.Value5 == null)
                            addressData.Value5 = item.Value;
                        else
                        {
                            addressData.Value1 = item.Value;
                            addressData.Value2 = null;
                            addressData.Value3 = null;
                            addressData.Value4 = null;
                            addressData.Value5 = null;
                        }
                    }
                }
            }
            
        }

        public void StartContinuousMonitor()
        {

        }

        public void ClearMonitor()
        {

        }

        #endregion

    }



}
