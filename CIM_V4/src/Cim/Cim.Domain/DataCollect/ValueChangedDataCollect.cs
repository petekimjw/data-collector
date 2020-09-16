using Cim.Domain.Driver;
using Cim.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Domain.DataCollect
{
    public class ValueChangedDataCollect : TimerDataCollect
    {
        private Dictionary<string, object> addressDatas = new Dictionary<string, object>();

        public ValueChangedDataCollect(IDriver driver, List<AddressMap> addressMaps, int interval) : base(driver, addressMaps, interval)
        {

        }

        public override async Task<List<AddressData>> ReadAddressMaps(bool useSameCollectTime = true)
        {
            var tempResults = new List<AddressData>();

            //var words = await ReadAddressMapsInternal(WordAddressMapsGroup, useSameCollectTime);
            //var strings = await ReadAddressMapsInternal(StringAddressMapsGroup, useSameCollectTime);
            //var bits = await ReadAddressMapsInternal(BitAddressMapsGroup, useSameCollectTime);

            //if (words?.Count > 0)
            //    tempResults.AddRange(words);
            //if (strings?.Count > 0)
            //    tempResults.AddRange(strings);
            //if (bits?.Count > 0)
            //    tempResults.AddRange(bits);

            //기존의 수집한 이력과 값이 틀리면 전송한다
            var results = new List<AddressData>();
            foreach (var item in tempResults)
            {
                if (addressDatas.ContainsKey(item.VariableId))
                {
                    if (addressDatas[item.VariableId]?.ToString() != item.Value?.ToString())
                    {
                        addressDatas[item.VariableId] = item.Value;
                        results.Add(item);
                    }
                }
                else
                {
                    addressDatas.Add(item.VariableId, item.Value);
                    results.Add(item);
                }
            }

            return tempResults;
        }
    }
}
