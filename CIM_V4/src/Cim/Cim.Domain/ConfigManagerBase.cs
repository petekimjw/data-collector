using Cim.Config;
using Cim.Manager;
using Cim.Model;
using Cim.Service;
using Cim.Transfer;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Extensions;
using Tectone.Common.Mvvm;

namespace Cim
{
    public class DefaultConfigManager : ConfigManagerBase { }

    public abstract class ConfigManagerBase : BindableBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region 주요속성

        private CimConfig _CimConfig;
        public CimConfig CimConfig
        {
            get { return _CimConfig; }
            set { Set(ref _CimConfig, value); }
        }

        private IAddressMapService _AddressMapService;
        public IAddressMapService AddressMapService
        {
            get { return _AddressMapService; }
            set { Set(ref _AddressMapService, value); }
        }

        private ObservableCollection<ControllerManagerBase> _ControllerManagers;
        public ObservableCollection<ControllerManagerBase> ControllerManagers
        {
            get { return _ControllerManagers; }
            set { Set(ref _ControllerManagers, value); }
        }

        private IDataInputService _DataInputService;
        public IDataInputService DataInputService
        {
            get { return _DataInputService; }
            set { Set(ref _DataInputService, value); }
        }

        private ObservableCollection<ITransfer> _Transfers;
        public ObservableCollection<ITransfer> Transfers
        {
            get { return _Transfers; }
            set { Set(ref _Transfers, value); }
        }
        #endregion

        #region Cim 파이프라인 구성 (CimConfig, DataInputService, Transfers, AddressMapService)

        private List<Controller> controllers = new List<Controller>();

        public void Init()
        {
            try
            {
                //CimConfig
                CimConfig = ConfigurationManager.GetSection("cim") as CimConfig;

                //IDataInputService
                DataInputService = new DataInputService();

                //ITransfer
                Transfers = new ObservableCollection<ITransfer> { new MqTransfer()};

                // IAddressMapService
                AddressMapService = new ExcelAddressMapService();

                // LoadAddressMaps, ControllerManagers
                LoadAddressMapAndCreateManager(CimConfig.AddressMapFileName, Transfers);

                //Start
                Start();
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        }

        #endregion

        public void Start()
        {
            foreach (var item in ControllerManagers)
            {
                //item.DataReceived -= ControllerManager_DataReceived;
                //item.DataReceived += ControllerManager_DataReceived;
                item.Start();
            }
        }

        public void Stop()
        {
            foreach (var item in ControllerManagers)
            {
                //item.DataReceived -= ControllerManager_DataReceived;
                item.Stop();
            }
        }

        /// <summary>
        /// 어드레스맵 파싱 및 ControllerManager 생성
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="transfers"></param>
        private void LoadAddressMapAndCreateManager(string fileName, IEnumerable<ITransfer> transfers)
        {
            controllers = AddressMapService.ParseAndWrite(fileName);

            //controllerManagers
            ControllerManagers = new ObservableCollection<ControllerManagerBase>();
            foreach (var item in controllers)
            {
                ControllerManagers.Add(new DefaultControllerManager(item, Transfers));
            }
        }

        /// <summary>
        /// 어드레스맵 파싱 후 재시작(ControllerManager.InitDataCollects)
        /// </summary>
        public void ReloadAddressMapAndCreateManager(string fileName)
        {
            Stop();

            controllers = AddressMapService.ParseAndWrite(fileName);

            //controllerManagers

            foreach (var item in ControllerManagers)
            {
                item.InitDataCollects(item.AddressMaps);
            }

            Start();
        }

        #region ControllerManager_DataReceived (ControllerManagers 콜렉션에서 직접 이벤트 구독 바람)

        //public event EventHandler<AddressDataReceivedEventArgs> DataReceived;

        //private void ControllerManager_DataReceived(object sender, AddressDataReceivedEventArgs e)
        //{
        //    DataReceived?.Invoke(sender, e);
        //} 

        #endregion

    }
}
