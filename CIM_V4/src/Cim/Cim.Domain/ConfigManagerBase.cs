using AutoMapper;
using Cim.Domain.Config;
using Cim.Domain.Manager;
using Cim.Domain.Model;
using Cim.Domain.Service;
using Cim.Domain.Transfer;
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

        private IDbSyncService _DbSyncService;
        public IDbSyncService DbSyncService
        {
            get { return _DbSyncService; }
            set { Set(ref _DbSyncService, value); }
        }

        #endregion

        #region Cim 파이프라인 구성 (CimConfig, DataInputService, Transfers, AddressMapService, DbSyncService)

        private List<Controller> controllers = new List<Controller>();

        public virtual void Init()
        {
            try
            {
                //CimConfig : app.config 설정
                CimConfig = ConfigurationManager.GetSection("cim") as CimConfig;

                //ITransfer : Mq 전송등 상위전송
                Transfers = new ObservableCollection<ITransfer> { new MqTransfer() };

                //Excel 어드레스맵 제공
                AddressMapService = new ExcelAddressMapService();

                //LoadAddressMaps, ControllerManagers
                (var result, _) = LoadAddressMapAndCreateManager(CimConfig.AddressMapFileName, Transfers);
                if (result == false)
                    return; //파싱 실패시 !!!

                //t_dvc_info, t_var_info 동기화 서비스
                var connectionStrings = ConfigurationManager.ConnectionStrings["pie"];
                if (string.IsNullOrEmpty(connectionStrings.ConnectionString))
                {
                    logger.Error($"connectionString Fail!={connectionStrings.ConnectionString}");
                    return;
                }
                else
                {
                    DbSyncService = new PieDbSyncService(connectionStrings.ProviderName, connectionStrings.ConnectionString);
                    DbSyncService.Sync(controllers);

                    //IDataInputService : 데이터수집을 REST로 제공
                    DataInputService = new DataInputService();

                    //Start
                    Start();
                }
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
        private (bool, List<Controller>) LoadAddressMapAndCreateManager(string fileName, IEnumerable<ITransfer> transfers)
        {
            ControllerManagers = new ObservableCollection<ControllerManagerBase>();

            //ParseAndWrite
            controllers = AddressMapService.ParseAndWrite(fileName);
            if (AddressMapService.AddressMapParseErrors?.Count > 0)
            {
                foreach (var item in AddressMapService.AddressMapParseErrors)
                {
                    (var a, var b) = item;
                    logger.Error($"AddressMapParseErrors={a}, {b}");
                }
                return (false, controllers);
            }

            //controllerManagers
            foreach (var item in controllers)
            {
                ControllerManagers.Add(new DefaultControllerManager(item, Transfers));
            }
            return (true, controllers);
        }

        /// <summary>
        /// 어드레스맵 파싱 후 재시작(ControllerManager.InitDataCollects)
        /// </summary>
        public async Task ReloadAddressMapAndCreateManager(string addressMapFileName=null)
        {
            Stop();
            await Task.Delay(1000);

            if(!string.IsNullOrEmpty(addressMapFileName))
                LoadAddressMapAndCreateManager(addressMapFileName, Transfers);
            else
                LoadAddressMapAndCreateManager(CimConfig.AddressMapFileName, Transfers);

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
