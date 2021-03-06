﻿using Cim.Domain.Driver;
using Cim.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Domain.DataCollect
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
                results = await base.ReadAddressMaps(useSameCollectTime);
            }

            return results;
        }

    }
}
