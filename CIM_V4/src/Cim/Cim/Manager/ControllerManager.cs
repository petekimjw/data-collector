using AutoMapper;
using CIM.Driver;
using CIM.Model;
using CIM.Transfer;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Manager
{
    public class ControllerManager : IDisposable
    {
        #region 초기화

        protected Logger logger { get; set; }
        public Controller Controller { get; set; }
        public IDriver Driver { get; set; }

        public List<AddressMap> AddressMaps { get; set; }

        public List<ICollectData> CollectDatas { get; set; } = new List<ICollectData>();
        public int TraceInterval { get; set; } = 5000;
        public int StatusInterval { get; set; } = 1000;
        public int AlarmInterval { get; set; } = 5000;

        public List<ITransfer> Transfers { get; set; } = new List<ITransfer>();


        public ControllerManager(Controller controller, List<AddressMap> addressMaps)
        {
            try
            {
                // PLC 전역에 로그를 기록하기 위한 로그 인스턴스
                InitDeviceLogger($"Device.{controller.Name}");
                logger = LogManager.GetLogger($"Device.{controller.Name}");

                Controller = controller;
                AddressMaps = addressMaps;

                Driver = new ModbusDriver("127.0.0.1", 502, 5000);

                var traceAddressMaps = addressMaps.Where(m => m.DataCategory == DataCategory.Trace || m.DataCategory == DataCategory.Data).ToList();
                var statusAddressMaps = addressMaps.Where(m => m.DataCategory == DataCategory.Status).ToList();
                var alarmAddressMaps = addressMaps.Where(m => m.DataCategory == DataCategory.Alarm).ToList();

                var traceTimerCollectData = new TimerCollectData(Driver, addressMaps, TraceInterval);
                var statusTimerCollectData = new TimerCollectData(Driver, statusAddressMaps, StatusInterval);
                var alarmTimerCollectData = new TimerCollectData(Driver, alarmAddressMaps, AlarmInterval);

                CollectDatas.Add(traceTimerCollectData);
                CollectDatas.Add(statusTimerCollectData);
                CollectDatas.Add(alarmTimerCollectData);

                Start();
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        }

        public void Dispose()
        {
            Stop();
        } 

        #endregion

        #region CollectData_DataReceived

        private void CollectData_DataReceived(object sender, AddressDataReceivedEventArgs e)
        {
            try
            {
                //Transfer
                foreach (var transfer in Transfers)
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

        #region ICollectData

        public event EventHandler<AddressDataReceivedEventArgs> DataReceived;

        public void Start()
        {
            try
            {
                foreach (var item in CollectDatas)
                {
                    item.DataReceived -= CollectData_DataReceived;
                    item.DataReceived += CollectData_DataReceived;
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
                foreach (var item in CollectDatas)
                {
                    item.DataReceived -= CollectData_DataReceived;
                    item.Stop();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        } 

        #endregion

        #region Logger

        /// <summary>
        /// Device Logger 초기화
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
