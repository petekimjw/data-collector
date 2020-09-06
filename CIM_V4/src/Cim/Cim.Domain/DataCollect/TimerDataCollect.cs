using Cim.Driver;
using Cim.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tectone.Common.Utils;

namespace Cim.DataCollect
{
    public class TimerDataCollect : IDataCollect
    {
        #region 초기화
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected Timer MainTimer = new Timer();


        public IDriver Driver { get; set; }

        public List<AddressMap> AddressMaps { get; set; }
        public List<List<AddressMap>> WordAddressMapsGroup { get; set; } = new List<List<AddressMap>>();
        public List<List<AddressMap>> BitAddressMapsGroup { get; set; } = new List<List<AddressMap>>();
        public List<List<AddressMap>> StringAddressMapsGroup { get; set; } = new List<List<AddressMap>>();


        public TimerDataCollect(IDriver driver, List<AddressMap> addressMaps, int interval)
        {
            if (driver == null)
                throw new InvalidOperationException("driver is null");
            if (addressMaps == null)
                throw new InvalidOperationException("addressMaps is null");

            Driver = driver;
            AddressMaps = addressMaps;
            GroupingAddressMaps(addressMaps);

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

        #region 데이터수집 (override 가능)

        protected virtual async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MainTimer.Stop();

            DebugHelper.GetElapsedTime2();

            var addressDatas = await ReadAddressMaps();
            DataReceived?.Invoke(this, new AddressDataReceivedEventArgs(addressDatas, AddressMaps?.FirstOrDefault()?.DeviceId));

            logger.Debug($"addressDatas={addressDatas?.Count}, {DebugHelper.GetElapsedTime2()}");

            MainTimer.Start();
        }

        /// <summary>
        /// AddressMaps을 수집목적에 따라 분리. DeviceName, Group  속성 및 연속된주소로 그룹핑.
        /// 전체 AddressMap을 DeviceName 그룹핑 => Word, Bit, String 그룹핑
        /// 여기서 수집목적에 따른 그룹핑을 원하는 여기서 수행하세요! (예: Group=Trigger1, Trigger2)
        /// </summary>
        /// <param name="addressMaps"></param>
        public virtual void GroupingAddressMaps(List<AddressMap> addressMaps)
        {
            var deviceGroups = addressMaps.GroupBy(m => m.DeviceId).ToList();

            foreach (var deviceGroup in deviceGroups)
            {
                (var words, var bits, var strings) = GroupingAddressMapsByDataType(deviceGroup.ToList());

                WordAddressMapsGroup.AddRange(words);
                BitAddressMapsGroup.AddRange(bits);
                StringAddressMapsGroup.AddRange(strings);
            }
            
        }

        /// <summary>
        /// Word, Bit, String 에 따른 분류
        /// </summary>
        /// <param name="addressMaps"></param>
        /// <returns></returns>
        public (List<List<AddressMap>> words, List<List<AddressMap>> bits, List<List<AddressMap>> strings) 
            GroupingAddressMapsByDataType(List<AddressMap> addressMaps)
        {
            var words = new List<List<AddressMap>>();
            var bits = new List<List<AddressMap>>();
            var strings = new List<List<AddressMap>>();

            #region Word

            var filtered = addressMaps.Where(m => m.DataType == DataType.Word).OrderBy(m => m.AddressNumber).ToList();
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
            
            return (words, bits, strings);
        }

        /// <summary>
        /// 수집목적에 따른 그룹(WordAddressMapsGroup, StringAddressMapsGroup, BitAddressMapsGroup, Trigger1Group, Trigger2Group 등)에 따른 수집
        /// </summary>
        /// <param name="useSameCollectTime"></param>
        /// <returns></returns>
        public virtual async Task<List<AddressData>> ReadAddressMaps(bool useSameCollectTime = true)
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
                        registerType = (int)modbus.RegesterType;
                        ushort.TryParse(start, out ushort startUShort);

                        (error, results) = await Driver.ReadRegister(start, startUShort, item.Count, registerType: registerType);
                    }
                    else
                        (error, results) = await Driver.ReadRegister(start, 0, item.Count);

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
