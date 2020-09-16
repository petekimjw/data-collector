using ActProgTypeLib;
using ActUtlTypeLib;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tectone.Common.Mvvm;

namespace Cim.Driver
{

    public class MelsecDriver : BindableBase, IDriver
    {
        #region 초기화, IDriver
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        protected IActUtlType Plc;

        public int LogicalStationNumber { get; set; }

        public MelsecDriver(int logicalStationNumber)
        {
            LogicalStationNumber = logicalStationNumber;
            Plc = new ActUtlTypeClass();

            Open();
            retryTimer.Elapsed += RetryTimer_Elapsed;
            retryTimer.Start();
        }

        private void RetryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Status != DriverStatus.Connected)
                RetryOpen();
        }

        #endregion

        #region IDriver

        private DriverStatus _Status;
        public DriverStatus Status
        {
            get { return _Status; }
            set { Set(ref _Status, value); }
        }

        public bool Open()
        {
            bool result = false;
            try
            {
                Plc.Open();
                result = true;
                Status = DriverStatus.Connected;
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
                Status = DriverStatus.Disconnected;
            }
            return result;
        }

        public bool Close()
        {
            bool result = false;
            try
            {
                Plc.Close();
                result = true;
                Status = DriverStatus.Disconnected;
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
            return result;
        }

        private Timer retryTimer = new Timer(3000);

        public bool RetryOpen()
        {
            bool result = false;
            try
            {
                if (Status != DriverStatus.Connected)
                {
                    result = Close();
                    result = Open();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Melsec 버퍼(1bit) 영역, 디바이스블럭(16bit Word) 영역 데이터읽기.
        /// 다른영역을 한번에 읽으면 예외발생.
        /// 더블워드(32bit int)는 바이트오더(ABCD, CDAB)를 고려하여 파싱하여야 한다.
        /// </summary>
        /// <param name="stringAddress"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<(int error, ushort[] results)> Read(
            string stringAddress, ushort startAddress, int count =1, bool isBit = false, 
            ushort slaveId = 1, int registerType = 1)
        {
            ushort[] results = new ushort[count];
            short[] tempResults = new short[count];

            int error = 0;
            await Task.Run(new Action(() =>
            {
                try
                {
                    error = Plc.ReadDeviceBlock2(stringAddress, count, out tempResults[0]);
                    //var a = mPlc.ReadDeviceBlock(startAddress, length, out results[0]);

                    results = tempResults.Select(m => Convert.ToUInt16(m)).ToArray();
                }
                catch (Exception ex)
                {
                    logger.Error($"ex={ex}");
                }
            }));

            return (error, results);
        }
    }
}
