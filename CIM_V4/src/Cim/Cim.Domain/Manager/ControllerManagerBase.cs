using AutoMapper;
using Cim.Domain.DataCollect;
using Cim.Domain.Driver;
using Cim.Domain.Model;
using Cim.Domain.Service;
using Cim.Domain.Transfer;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Extensions;
using Tectone.Common.Mvvm;

namespace Cim.Domain.Manager
{
    public class DefaultControllerManager : ControllerManagerBase
    {
        public DefaultControllerManager(Controller controller, IEnumerable<ITransfer> transfers)
            : base(controller, transfers)
        {

        }
    }
    public abstract class ControllerManagerBase : BindableBase
    {
        #region 주요속성

        private Controller _Controller;
        public Controller Controller
        {
            get { return _Controller; }
            set { Set(ref _Controller, value); }
        }

        private ObservableCollection<string> _DeviceIds = new ObservableCollection<string>();
        public ObservableCollection<string> DeviceIds
        {
            get { return _DeviceIds; }
            set { Set(ref _DeviceIds, value); }
        }

        private IDriver _Driver;
        public IDriver Driver
        {
            get { return _Driver; }
            set { Set(ref _Driver, value); }
        }

        private ObservableCollection<IDataCollect> _DataCollects = new ObservableCollection<IDataCollect>();
        public ObservableCollection<IDataCollect> DataCollects
        {
            get { return _DataCollects; }
            set { Set(ref _DataCollects, value); }
        }


        private List<ITransfer> transfers { get; set; } = new List<ITransfer>();

        #endregion

        #region 초기화

        protected Logger logger { get; set; }

        public int ReceiveTimeout { get; set; } = 5000;

        public ControllerManagerBase(Controller controller, IEnumerable<ITransfer> transfers)
        {
            try
            {
                Controller = controller;

                // Logger
                InitDeviceLogger($"Device.{controller.Name}");
                logger = LogManager.GetLogger($"Device.{controller.Name}");

                //Driver
                InitDriver(controller);

                //ITransfer
                this.transfers = transfers.ToList();

                //DataCollects
                InitDataCollects(controller.AddressMaps.ToObservableCollection());

            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        }

        #endregion

        #region InitDriver

        protected virtual void InitDriver(Controller controller)
        {
            switch (controller.Protocol)
            {
                case ControllerProtocol.None:
                    break;
                case ControllerProtocol.Modbus:
                    Driver = new ModbusDriver(controller.Ip, controller.Port, ReceiveTimeout);
                    break;
                case ControllerProtocol.Melsec:   
                    Driver = new MelsecDriver(controller.LogicalStationNumber);
                    break;
                default:
                    logger.Error($"ControllerManagerBase controller.Protocol={controller.Protocol}");
                    break;
            }
        }

        #endregion

        #region DataCollects

        /// <summary>
        /// 어드레스맵 다시 갱신 (Stop, Start)
        /// </summary>
        /// <param name="addressMaps"></param>
        public virtual void InitDataCollects(ObservableCollection<AddressMap> addressMaps)
        {
            Stop();

            DataCollects?.Clear();

            #region DeviceIds

            var groups = addressMaps.GroupBy(m => m.DeviceId).ToDictionary(m => m.Key, m => m.ToList());
            DeviceIds = groups.Keys.ToObservableCollection();

            #endregion

            //DeviceIds 별 그룹핑 => Data, Status, Alarm 별 그룹핑 DataCollects 생성
            foreach (var group in groups)
            {
                var groupName = group.Key;
                var maps = group.Value;

                // Data 는 5초에 1번 폴링
                var dataCollect = CreateDataTypeDataCollect(groupName, maps);
                if(dataCollect != null)
                    DataCollects.Add(dataCollect);

                // Status 는 1초에 1번 폴링
                dataCollect = CreateStatusDataCollect(groupName, maps);
                if (dataCollect != null)
                    DataCollects.Add(dataCollect);

                // Alarm 는 1초에 1번 폴링. 값이 변경시 전송
                dataCollect = CreateAlarmDataCollect(groupName, maps);
                if (dataCollect != null)
                    DataCollects.Add(dataCollect);

            }

            Start();
        }

        public virtual IDataCollect CreateDataTypeDataCollect(string groupName, List<AddressMap> maps)
        {
            var dataAddressMaps = maps.Where(m => m.DataCategory == DataCategory.Data).ToList();
            if (dataAddressMaps?.Count > 0)
            {
                int dataInterval = 5000;
                if (Controller?.MetaDatas?.ContainsKey("DataInterval") == true)
                    int.TryParse(Controller.MetaDatas["DataInterval"], out dataInterval);
                var dataTimerDataCollect = new TimerDataCollect(Driver, dataAddressMaps, dataInterval, $"Data-{groupName}");

                return dataTimerDataCollect;
            }

            return null;
        }

        public virtual IDataCollect CreateStatusDataCollect(string groupName, List<AddressMap> maps)
        {
            var statusAddressMaps = maps.Where(m => m.DataCategory == DataCategory.Status).ToList();
            if (statusAddressMaps?.Count > 0)
            {
                int statusInterval = 1000;
                if (Controller?.MetaDatas?.ContainsKey("StatusInterval") == true)
                    int.TryParse(Controller.MetaDatas["StatusInterval"], out statusInterval);
                var statusTimerDataCollect = new TimerDataCollect(Driver, statusAddressMaps, statusInterval, $"Status-{groupName}");

                return statusTimerDataCollect;
            }

            return null;
        }

        public virtual IDataCollect CreateAlarmDataCollect(string groupName, List<AddressMap> maps)
        {
            var alarmAddressMaps = maps.Where(m => m.DataCategory == DataCategory.Alarm).ToList();
            if (alarmAddressMaps?.Count > 0)
            {
                int alarmInterval = 10000;
                if (Controller?.MetaDatas?.ContainsKey("AlarmInterval") == true)
                    int.TryParse(Controller.MetaDatas["AlarmInterval"], out alarmInterval);
                var alarmTimerDataCollect = new TimerDataCollect(Driver, alarmAddressMaps, alarmInterval, $"Alarm-{groupName}");

                return alarmTimerDataCollect;
            }

            return null;
        }


        #endregion

        #region IDataCollect

        public event EventHandler<AddressDataReceivedEventArgs> DataReceived;

        public void Start()
        {
            try
            {
                foreach (var item in DataCollects)
                {
                    item.DataReceived -= DataCollect_DataReceived;
                    item.DataReceived += DataCollect_DataReceived;
                    item.Start();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        }

        public void Stop()
        {
            try
            {
                foreach (var item in DataCollects)
                {
                    item.DataReceived -= DataCollect_DataReceived;
                    item.Stop();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        }

        #endregion

        #region DataCollect_DataReceived

        private void DataCollect_DataReceived(object sender, AddressDataReceivedEventArgs e)
        {
            if (!(e?.AddressDatas?.Count > 0))
                return;
            try
            {
                //Transfer
                foreach (var transfer in transfers)
                {
                    //transfer.Transfer();
                }

                //Log
                CsvHelper.WriteColumnCsvFile(e.AddressDatas, e.DeviceName);

                //Event
                DataReceived?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        }

        #endregion

        #region Logger

        /// <summary>
        /// Device Logger 초기화. app.config 에 AsyncDevice 로거를 복사해서 사용
        /// </summary>
        protected virtual void InitDeviceLogger(string loggerName)
        {
            IMapper mapper = GetMapper();
            AsyncTargetWrapper newTargetWrapper = null;
            FileTarget newTarget = null;

            LoggingConfiguration logConfig = LogManager.Configuration;
            if (logConfig.FindTargetByName("AsyncDevice") is AsyncTargetWrapper asyncDeviceWrapper)
            {
                newTargetWrapper = mapper.Map<AsyncTargetWrapper>(asyncDeviceWrapper);
                newTarget = mapper.Map<FileTarget>(newTargetWrapper.WrappedTarget);
            }
            else
            {
                logger.Error($"app.config 로그설정에 AsyncDevice 타겟이 없습니다");
                throw new ArgumentNullException(nameof(asyncDeviceWrapper));
            }

            newTargetWrapper.Name = loggerName;
            newTargetWrapper.WrappedTarget = newTarget;

            newTarget.Name = $"{loggerName}_Sub";
            newTarget.FileName = newTarget.FileName.ToString().Replace("{loggername}", loggerName).Replace("'", string.Empty);
            newTarget.ArchiveFileName = newTarget.ArchiveFileName.ToString().Replace("{loggername}", loggerName).Replace("{controllerid}", Controller.Name).Replace("'", string.Empty);

            logConfig.AddTarget(newTarget.Name, newTarget);
            logConfig.AddTarget(newTargetWrapper.Name, newTargetWrapper);

            var rule = logConfig.LoggingRules.FirstOrDefault(x => x.LoggerNamePattern == "Device.*");
            if (rule != null)
            {
                var newRule = new LoggingRule(loggerName, rule.Levels.First(), rule.Levels.Last(), newTargetWrapper) { Final = true };

                foreach (Target target in rule.Targets)
                {
                    newRule.Targets.Add(target);
                }

                // Final 적용을 위해 0번째에 추가
                logConfig.LoggingRules.Insert(0, newRule);
            }

            // 주의! 이렇게 해야 변경사항이 적용됨
            LogManager.Configuration = logConfig;
        }

        /// <summary>
        /// AutoMapper 초기화
        /// </summary>
        protected IMapper GetMapper()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AsyncTargetWrapper, AsyncTargetWrapper>();
                cfg.CreateMap<FileTarget, FileTarget>();
            });

            return mapperConfig.CreateMapper();
        }

        #endregion
    }
}
