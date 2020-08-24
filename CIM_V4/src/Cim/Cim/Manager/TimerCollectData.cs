using CIM.Driver;
using CIM.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CIM.Manager
{
    public class TimerCollectData : ICollectData
    {
        #region 초기화
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected Timer MainTimer = new Timer();

        public Device Device { get; set; }
        public IDriver Driver { get; set; }

        public List<AddressMap> AddressMaps { get; set; }
        public List<List<AddressMap>> WordAddressMapsGroup { get; set; }
        public List<List<AddressMap>> BitAddressMapsGroup { get; set; }
        public List<List<AddressMap>> StringAddressMapsGroup { get; set; }


        public TimerCollectData(IDriver driver, List<AddressMap> addressMaps, int interval)
        {
            Driver = driver;
            AddressMaps = addressMaps;
            InitAddressMaps(addressMaps);

            MainTimer.Interval = interval;
            MainTimer.Elapsed += MainTimer_Elapsed;
        } 
        #endregion

        #region ICollectData

        public event EventHandler<AddressDataReceivedEventArgs> DataReceived;

        public void Start()
        {
            MainTimer.Start();
        }

        public void Stop()
        {
            MainTimer.Stop();
        }
        #endregion

        #region 데이터수집

        protected virtual async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MainTimer.Stop();

            var addressDatas = await ReadAddressMaps();
            DataReceived?.Invoke(this, new AddressDataReceivedEventArgs(addressDatas, AddressMaps?.FirstOrDefault()?.DeviceName));

            MainTimer.Start();
        }

        /// <summary>
        /// AddressMaps을 수집목적에 따라 분리.
        /// </summary>
        /// <param name="addressMaps"></param>
        protected virtual void InitAddressMaps(List<AddressMap> addressMaps)
        {
            #region Word
            var addressMapsGroup = new List<List<AddressMap>>();
            var filtered = addressMaps.Where(m => m.DataType == DataType.Word).OrderBy(m => m.AddressNumber).ToList();
            var list = new List<AddressMap>();

            for (int i = 0; i < filtered.Count; i++)
            {
                if (i > 0 && filtered.Count > 1)
                {
                    if (filtered[i].AddressNumber - filtered[i - 1].AddressNumber > 1)//연속적인 주소로 그룹핑
                    {
                        if (list.Count > 0)
                            addressMapsGroup.Add(list);
                        list = new List<AddressMap>();
                    }
                }
                list.Add(filtered[i]);
            }

            if (list?.Count > 0)
                addressMapsGroup.Add(list);
            #endregion
            WordAddressMapsGroup = addressMapsGroup;

            #region String
            addressMapsGroup = new List<List<AddressMap>>();
            filtered = addressMaps.Where(m => m.DataType == DataType.String).OrderBy(m => m.AddressNumber).ToList();
            list = new List<AddressMap>();

            for (int i = 0; i < filtered.Count; i++)
            {
                if (i > 0 && filtered.Count > 1)
                {
                    if (filtered[i].AddressNumber - filtered[i - 1].AddressNumber > 1)//연속적인 주소로 그룹핑
                    {
                        if (list.Count > 0)
                            addressMapsGroup.Add(list);
                        list = new List<AddressMap>();
                    }
                }
                list.Add(filtered[i]);
            }

            if (list?.Count > 0)
                addressMapsGroup.Add(list);
            #endregion
            StringAddressMapsGroup = addressMapsGroup;

            #region Bit
            addressMapsGroup = new List<List<AddressMap>>();
            filtered = addressMaps.Where(m => m.DataType == DataType.Bit).OrderBy(m => m.AddressNumber).ToList();
            list = new List<AddressMap>();

            for (int i = 0; i < filtered.Count; i++)
            {
                if (i > 0 && filtered.Count > 1)
                {
                    if (filtered[i].AddressNumber - filtered[i - 1].AddressNumber > 1)//연속적인 주소로 그룹핑
                    {
                        if (list.Count > 0)
                            addressMapsGroup.Add(list);
                        list = new List<AddressMap>();
                    }
                }
                list.Add(filtered[i]);
            }

            if (list?.Count > 0)
                addressMapsGroup.Add(list);
            #endregion
            BitAddressMapsGroup = addressMapsGroup;
        }

        protected virtual async Task<List<AddressData>> ReadAddressMaps(bool useSameCollectTime = true)
        {
            var results = new List<AddressData>();

            var words = await ReadAddressMapsInternal(WordAddressMapsGroup, useSameCollectTime);
            var strings = await ReadAddressMapsInternal(StringAddressMapsGroup, useSameCollectTime);
            var bits = await ReadAddressMapsInternal(BitAddressMapsGroup, useSameCollectTime);

            if (words?.Count > 0)
                results.AddRange(words);
            if (strings?.Count > 0)
                results.AddRange(strings);
            if (bits?.Count > 0)
                results.AddRange(bits);

            return results;
        }

        protected virtual async Task<List<AddressData>> ReadAddressMapsInternal(
            List<List<AddressMap>> addressMapsGroup, bool useSameCollectTime = true)
        {
            var nowTime = DateTime.Now;
            var addressDatas = new List<AddressData>();

            foreach (var item in addressMapsGroup)
            {
                var address = item.Select(m => m.Address).ToList();
                var start = ushort.Parse(address.FirstOrDefault());
                var deviceId = item.FirstOrDefault().DeviceName;
                var registerType = (item.FirstOrDefault() as ModbusAddressMap).RegesterType;

                (int error, var results) = await Driver.ReadRegister(null, start, address.Count, registerType: (int)registerType);

                if (error == 0 && results.Count() > 0)
                {
                    for (int index = 0; index < results.Length; index++)
                    {
                        DateTime collectTime = DateTime.Now;
                        if (useSameCollectTime)
                            collectTime = nowTime;
                        addressDatas.Add(new AddressData(collectTime, deviceId, item[index].VariableName, item[index].Address, results[index]));
                    }
                }
            }
            return addressDatas;
        }

        #endregion

    }
}
