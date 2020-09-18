using Cim.Domain.Driver;
using Cim.Domain.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tectone.Common.Utils;

namespace Cim.Domain.DataCollect
{
    /// <summary>
    /// Timer의 Interval 간격으로 주기적으로 데이터수집
    /// </summary>
    public class TimerDataCollect : DataCollectBase
    {
        #region 초기화

        protected Timer MainTimer = new Timer();


        public TimerDataCollect(IDriver driver, List<AddressMap> addressMaps, int interval, string name="") 
            : base(driver, addressMaps, name)
        {
            logger = LogManager.GetCurrentClassLogger();

            MainTimer.Interval = interval;
            MainTimer.Elapsed += MainTimer_Elapsed;
        }

        #endregion

        #region ICollectData

        public override void Start()
        {
            MainTimer.Start();
        }

        public override void Stop()
        {
            MainTimer.Stop();
        }

        #endregion

        #region 데이터수집 (override 가능)-MainTimer_Elapsed

        protected virtual async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MainTimer.Stop();

            DebugHelper.GetElapsedTime2();

            var addressDatas = await ReadAddressMaps();
            OnDataReceived(this, new AddressDataReceivedEventArgs(addressDatas, AddressMaps?.FirstOrDefault()?.DeviceId));

            logger.Debug($"addressDatas={addressDatas?.Count}, {DebugHelper.GetElapsedTime2()}");

            MainTimer.Start();
        }

        #endregion

    }
}
