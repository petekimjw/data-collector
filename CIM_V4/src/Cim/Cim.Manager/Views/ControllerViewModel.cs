using Autofac;
using AutoMapper;
using Cim.Domain;
using Cim.Domain.Manager;
using Cim.Domain.Model;
using Cim.Manager.Resources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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

        public virtual ObservableCollection<AddressDataWrapper> AddressDataWrappers { get; set; } = new ObservableCollection<AddressDataWrapper>();

        public virtual ObservableCollection<AddressDataWrapper> SelectedAddressDataWrappers { get; set; } = new ObservableCollection<AddressDataWrapper>();

        public virtual AddressDataWrapper SelectedAddressDataWrapper { get; set; }

        private ConfigManagerBase configManager = null;

        #endregion

        public ControllerViewModel() : base()
        {
            if (GetIsInDesignMode()) return;

            //IoC
            MessageBox = Container?.Resolve<IMessageBox>();
            AlertManager = Container?.Resolve<IAlertManager>();


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
        }

        public void LoadAddressMaps()
        {
            var addressMaps = SelectedControllerManager?.Controller.AddressMaps;
            if (addressMaps?.Count > 0)
                AddressDataWrappers = Domain.AutoMapper.Mapper.Map<ObservableCollection<AddressDataWrapper>>(addressMaps);
        }

        #endregion

        #region DataCollect_DataReceived

        private void DataCollect_DataReceived(object sender, AddressDataReceivedEventArgs e)
        {
            foreach (var item in e.AddressDatas)
            {
                var addressData = AddressDataWrappers?.FirstOrDefault(m => m.VariableId == item.VariableId);
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

        #region 명령 (AddressMap)

        public async Task OpenAddressMapAndCreateManager()
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            dialog.Filter = "excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            dialog.FilterIndex = 2;
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() != true)
                return;

            Messenger.Default.Send(new BusyInfo(50, Tectone.Common.Resources.LocalizedStrings.Wait));

            await configManager.ReloadAddressMapAndCreateManager(dialog.FileName);

            LoadControllerManagers();

            Messenger.Default.Send(new BusyInfo(-1, null));
        }

        public async Task ReloadAddressMapAndCreateManager()
        {
            Messenger.Default.Send(new BusyInfo(50, Tectone.Common.Resources.LocalizedStrings.Wait));

            await configManager.ReloadAddressMapAndCreateManager();

            LoadControllerManagers();

            Messenger.Default.Send(new BusyInfo(-1, null));
        }

        public async Task SaveAddressMapAndCreateManager()
        {

        }

        public void AddAddressMap()
        {
            SelectedAddressDataWrapper = SelectedAddressDataWrappers.FirstOrDefault();
            var vm = MvvmInstanceExtension.Create<AddressMapEditViewModel>();
            vm.SelectedAddressDataWrapper = new AddressDataWrapper() { State = State.Insert};

            window = new AddressMapEditView
            {
                DataContext = vm,
            };
            window.ShowDialog();
        }

        public void DeleteAddressMap()
        {
            SelectedAddressDataWrappers.ForEach(m => m.State = State.Delete);
        }

        public void EditAddressMap()
        {
            SelectedAddressDataWrapper = SelectedAddressDataWrappers.FirstOrDefault();
            var vm = MvvmInstanceExtension.Create<AddressMapEditViewModel>();
            vm.SelectedAddressDataWrapper = Domain.AutoMapper.Mapper.Map<AddressDataWrapper>(SelectedAddressDataWrapper);

            window = new AddressMapEditView
            {
                DataContext = vm,
            };
            var result = window.ShowDialog();
            if (result == true)
            {
                foreach (var item in vm.PropertyChangeds)//변경내용 적용
                {
                    if(item.Value != SelectedAddressDataWrapper.GetPropertyValue(item.Key))
                    {
                        SelectedAddressDataWrapper.State = State.Update;
                        SelectedAddressDataWrapper.SetPropertyValue(item.Key, item.Value);
                    }
                }
                //if (vm.SelectedAddressDataWrapper.State == State.Update)
                //{
                //    SelectedAddressDataWrapper = vm.SelectedAddressDataWrapper;
                //}
            }

        }

        AddressMapEditView window = null;
        public void CloseWindow()
        {
            window?.Close();
        }

        #endregion

        #region 명령 (Controller)

        public void Start()
        {
            configManager.Start();
            
        }

        public void Stop()
        {
            configManager.Stop();
            
        }

        #endregion

        #region 명령 (Monitoring)

        private int _ContinuousMonitorInterval = 5;
        public int ContinuousMonitorInterval
        {
            get { return _ContinuousMonitorInterval; }
            set { Set(ref _ContinuousMonitorInterval, value); }
        }

        private DispatcherTimer monitorTimer = new DispatcherTimer();

        /// <summary>
        /// 모니터링
        /// </summary>
        /// <returns></returns>
        public async Task GetDatas()
        {
            if (SelectedControllerManager == null) return;
            if (!(SelectedAddressDataWrappers?.Count > 0)) return;

            var maps = SelectedAddressDataWrappers.Cast<AddressMap>().ToList();

            var groups = SelectedControllerManager.MonitorDataCollect.GroupingAddressMaps(maps);
            foreach (var group in groups)
            {
                var datas = await SelectedControllerManager.MonitorDataCollect.ReadAddressMapsInternal(group.Value);
                foreach (var item in datas)
                {
                    var addressData = AddressDataWrappers?.FirstOrDefault(m => m.VariableId == item.VariableId);
                    if (addressData == null)
                        continue;

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

        public void StartContinuousMonitor(Telerik.Windows.Controls.RadRibbonToggleButton button)
        {
            if (button?.IsChecked == true)
            {
                monitorTimer.Interval = TimeSpan.FromSeconds(ContinuousMonitorInterval);
                monitorTimer.Tick -= MonitorTimer_Tick;
                monitorTimer.Tick += MonitorTimer_Tick;

                monitorTimer.Start();
            }
            else
                monitorTimer.Stop();
        }

        private async void MonitorTimer_Tick(object sender, EventArgs e)
        {
            await GetDatas();
        }

        public void ClearMonitor()
        {
            monitorTimer.Stop();
        }

        #endregion

    }



}
