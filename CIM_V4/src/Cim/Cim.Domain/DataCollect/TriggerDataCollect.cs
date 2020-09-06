using Cim.Driver;
using Cim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.DataCollect
{
    /// <summary>
    /// 트리거의 모든 값이 1인 경우 다른 주소의 값을 수집하여 전송한다
    /// </summary>
    public class TriggerDataCollect : TimerDataCollect
    {
        private List<AddressMap> triggerAddressMaps;

        public TriggerDataCollect(IDriver driver, List<AddressMap> addressMaps, int interval, List<AddressMap> triggers) : base(driver, addressMaps, interval)
        {
            triggerAddressMaps = triggers;
        }

        public override async Task<List<AddressData>> ReadAddressMaps(bool useSameCollectTime = true)
        {
            var results = new List<AddressData>();

            //트리거의 모든 값이 1인 경우 다른 주소의 값을 수집하여 전송한다
            var triggers = await ReadAddressMapsInternal(new List<List<AddressMap>> { triggerAddressMaps }, useSameCollectTime);
            if (triggers.All(m => m.Value?.ToString() == "1"))
            {

                var words = await ReadAddressMapsInternal(WordAddressMapsGroup, useSameCollectTime);
                var strings = await ReadAddressMapsInternal(StringAddressMapsGroup, useSameCollectTime);
                var bits = await ReadAddressMapsInternal(BitAddressMapsGroup, useSameCollectTime);

                if (words?.Count > 0)
                    results.AddRange(words);
                if (strings?.Count > 0)
                    results.AddRange(strings);
                if (bits?.Count > 0)
                    results.AddRange(bits);
            }

            return results;
        }

    }
}
