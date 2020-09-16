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
    public class TimerDataCollect : IDataCollect
    {
        #region 초기화
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected Timer MainTimer = new Timer();


        public IDriver Driver { get; set; }

        public List<AddressMap> AddressMaps { get; set; }
        public Dictionary<string, List<List<AddressMap>>> AddressMapsGroup { get; set; } = new Dictionary<string, List<List<AddressMap>>>();

        public TimerDataCollect(IDriver driver, List<AddressMap> addressMaps, int interval, string name="")
        {
            if (driver == null)
                throw new InvalidOperationException("driver is null");
            if (addressMaps == null)
                throw new InvalidOperationException("addressMaps is null");

            Name = name;
            Driver = driver;
            AddressMaps = addressMaps;
            GroupingAddressMaps(addressMaps);

            MainTimer.Interval = interval;
            MainTimer.Elapsed += MainTimer_Elapsed;
        }

        #endregion

        #region ICollectData

        public string Name { get; set; }

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

        #region 데이터수집 (override 가능)-MainTimer_Elapsed

        protected virtual async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MainTimer.Stop();

            DebugHelper.GetElapsedTime2();

            var addressDatas = await ReadAddressMaps();
            DataReceived?.Invoke(this, new AddressDataReceivedEventArgs(addressDatas, AddressMaps?.FirstOrDefault()?.DeviceId));

            logger.Debug($"addressDatas={addressDatas?.Count}, {DebugHelper.GetElapsedTime2()}");

            MainTimer.Start();
        }

        #endregion

        #region 데이터수집 (override 가능)-GroupingAddressMaps

        /// <summary>
        /// AddressMaps을 수집목적에 따라 분리. DeviceName, Group  속성 및 연속된주소로 그룹핑.
        /// 전체 AddressMap을 DeviceName 그룹핑 => Word, Bit, String 그룹핑 (Modbus의 경우 다른 Register 같이 읽으면 에러남)
        /// 여기서 수집목적에 따른 그룹핑을 원하는 여기서 수행하세요! (예: Group=Trigger1, Trigger2)
        /// </summary>
        /// <param name="addressMaps"></param>
        public virtual void GroupingAddressMaps(List<AddressMap> addressMaps)
        {
            if(addressMaps?.FirstOrDefault() is ModbusAddressMap)
                AddressMapsGroup = GroupingAddressMapsByFunctionCode(addressMaps.Cast<ModbusAddressMap>().ToList());
            else
                AddressMapsGroup = GroupingAddressMapsByDataType(addressMaps.ToList());

        }

        /// <summary>
        /// Word, Bit, String 에 따른 분류
        /// </summary>
        /// <param name="addressMaps"></param>
        /// <returns></returns>
        public Dictionary<string, List<List<AddressMap>>> GroupingAddressMapsByDataType(List<AddressMap> addressMaps)
        {
            var results = new Dictionary<string, List<List<AddressMap>>>();
            var words = new List<List<AddressMap>>();
            var bits = new List<List<AddressMap>>();
            var strings = new List<List<AddressMap>>();

            #region Word16, Word32, Real32, Real64, WordU16, WordU32

            var filtered = addressMaps.Where(m => m.DataType != DataType.Bit && m.DataType != DataType.String).OrderBy(m => m.AddressNumber).ToList();
            var list = new List<AddressMap>();

            for (int i = 0; i < filtered.Count; i++)
            {
                if (i > 0 && filtered.Count > 1)
                {
                    if (filtered[i].AddressNumber - filtered[i - 1].AddressNumber > 1)//연속적인 주소로 그룹핑
                    {
                        if (list.Count > 0)
                            words.Add(list);
                        list = new List<AddressMap>();
                    }
                }
                list.Add(filtered[i]);
            }

            if (list?.Count > 0)
                words.Add(list);
            #endregion 

            #region Bit
            bits = new List<List<AddressMap>>();
            filtered = addressMaps.Where(m => m.DataType == DataType.Bit).OrderBy(m => m.AddressNumber).ToList();
            list = new List<AddressMap>();

            for (int i = 0; i < filtered.Count; i++)
            {
                if (i > 0 && filtered.Count > 1)
                {
                    if (filtered[i].AddressNumber - filtered[i - 1].AddressNumber > 1)//연속적인 주소로 그룹핑
                    {
                        if (list.Count > 0)
                            words.Add(list);
                        list = new List<AddressMap>();
                    }
                }
                list.Add(filtered[i]);
            }

            if (list?.Count > 0)
                bits.Add(list);
            #endregion

            #region String
            strings = new List<List<AddressMap>>();
            filtered = addressMaps.Where(m => m.DataType == DataType.String).OrderBy(m => m.AddressNumber).ToList();
            list = new List<AddressMap>();

            for (int i = 0; i < filtered.Count; i++)
            {
                if (i > 0 && filtered.Count > 1)
                {
                    if (filtered[i].AddressNumber - filtered[i - 1].AddressNumber > 1)//연속적인 주소로 그룹핑
                    {
                        if (list.Count > 0)
                            words.Add(list);
                        list = new List<AddressMap>();
                    }
                }
                list.Add(filtered[i]);
            }

            if (list?.Count > 0)
                strings.Add(list);
            #endregion

            results.Add("Word", words);
            results.Add("Bit", bits);
            results.Add("String", strings);

            return results;
        }

        /// <summary>
        /// Modbus FunctionCode 에 따른 분류 (같은 레지스터끼리만 1번에 조회가 가능. 최대126개만 조회 가능)
        /// </summary>
        /// <param name="addressMaps"></param>
        /// <returns></returns>
        public Dictionary<string, List<List<AddressMap>>> GroupingAddressMapsByFunctionCode(List<ModbusAddressMap> addressMaps)
        {
            var results = new Dictionary<string, List<List<AddressMap>>>();
            var continuousMaps = new List<List<AddressMap>>();

            var groups = addressMaps.GroupBy(m => m.FunctionCode).ToList();
            foreach (var group in groups)
            {
                var filtered = group.OrderBy(m => m.AddressNumber).ToList();
                var list = new List<AddressMap>();

                for (int i = 0; i < filtered.Count; i++)
                {
                    if (i > 0 && filtered.Count > 1)
                    {
                        if ((filtered[i].AddressNumber - filtered[i - 1].AddressNumber > 1) || list.Count >= 126)//연속적인 주소가 아니면 신규목록
                        {
                            if (list.Count > 0)
                                continuousMaps.Add(list);
                            list = new List<AddressMap>();
                        }
                    }
                    list.Add(filtered[i]);//연속적인 주소이면 기존목록
                }

                if (list?.Count > 0)
                    continuousMaps.Add(list);

                //FunctionCode 별로 그룹핑
                results.Add($"{group.Key}", continuousMaps);
                continuousMaps = new List<List<AddressMap>>();
            }
            return results;
        }

        #endregion

        #region 데이터수집 (override 가능)-ReadAddressMaps

        /// <summary>
        /// 수집목적에 따른 그룹(WordAddressMapsGroup, StringAddressMapsGroup, BitAddressMapsGroup, Trigger1Group, Trigger2Group 등)에 따른 수집
        /// </summary>
        /// <param name="useSameCollectTime"></param>
        /// <returns></returns>
        public virtual async Task<List<AddressData>> ReadAddressMaps(bool useSameCollectTime = true)
        {
            var results = new List<AddressData>();

            foreach (var group in AddressMapsGroup)
            {
                var words = await ReadAddressMapsInternal(group.Value, useSameCollectTime);

                if (words?.Count > 0)
                    results.AddRange(words);
            }

            return results;
        }

        public virtual async Task<List<AddressData>> ReadAddressMapsInternal(
            List<List<AddressMap>> addressMapsGroup, bool useSameCollectTime = true)
        {
            var nowTime = DateTime.Now;
            var addressDatas = new List<AddressData>();

            foreach (var item in addressMapsGroup)
            {
                try
                {
                    int error = -1;
                    ushort[] results;
                    var startAddress = item?.FirstOrDefault();
                    var deviceId = startAddress.DeviceId;
                    var start = item?.FirstOrDefault().Address;
                    
                    int registerType = 1;
                    var modbus = startAddress as ModbusAddressMap;
                    if (modbus != null)
                    {
                        registerType = (int)modbus.FunctionCode;
                        if (ushort.TryParse(start, out ushort startUShort) == false)
                            logger.Error($"start address parse Fail! start={start}");

                        (error, results) = await Driver.Read(start, startUShort, item.Count, registerType: registerType);
                    }
                    else
                        (error, results) = await Driver.Read(start, 0, item.Count);

                    if (error == 0 && results.Count() > 0)
                    {
                        for (int index = 0; index < results.Length; index++)
                        {
                            DateTime collectTime = DateTime.Now;
                            if (useSameCollectTime)
                                collectTime = nowTime;
                            addressDatas.Add(new AddressData(collectTime, deviceId, item[index].VariableId, item[index].Address, results[index]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"itemCount={item?.Count}, item.Address={item?.FirstOrDefault().Address} ex={ex}");
                }
            }
            return addressDatas;
        }

        #endregion

    }
}
