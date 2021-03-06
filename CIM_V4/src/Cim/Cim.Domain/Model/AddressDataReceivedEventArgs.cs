﻿using Cim.Domain.Model;
using System;
using System.Collections.Generic;

namespace Cim.Domain.Model
{
    public class AddressDataReceivedEventArgs : EventArgs
    {
        public AddressDataReceivedEventArgs(List<AddressData> addressDatas, string deviceName)
        {
            AddressDatas = addressDatas;
            DeviceName = deviceName;
        }
        public List<AddressData> AddressDatas { get; set; }
        public string DeviceName { get; set; }
    }
}
