using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CIM.Driver;
using CIM.Manager;
using CIM.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cim.Test
{
    [TestClass]
    public class CimTest
    {
        [TestMethod]
        public void Test()
        {

        }

        [TestMethod]
        public async Task Modbus_ReadAddressMap_Test()
        {
            string deviceName = "Device";
            var controller = new Controller { Name = deviceName };
            var addressMaps = new List<AddressMap>
            {
                new ModbusAddressMap("Device", "1", "1", 1, 1, 1, dataType:DataType.Word, registerType:ModbusRegisterType.Holding),
                new ModbusAddressMap("Device", "2", "2",  1, 1, 1, dataType:DataType.Word, registerType:ModbusRegisterType.Holding),
                new ModbusAddressMap("Device", "3", "3",  1, 1, 1, dataType:DataType.Word, registerType:ModbusRegisterType.Holding),
                new ModbusAddressMap("Device", "4", "4",  1, 1, 1, dataType:DataType.Word, registerType:ModbusRegisterType.Holding),
                new ModbusAddressMap("Device", "5", "5",  1, 1, 1, dataType:DataType.Word, registerType:ModbusRegisterType.Holding),
                new ModbusAddressMap("Device", "6", "7.1",  1, 1, 1, dataType:DataType.Bit, registerType:ModbusRegisterType.Holding),
                new ModbusAddressMap("Device", "7", "7.2",  1, 1, 1, dataType:DataType.Bit, registerType:ModbusRegisterType.Holding),
                new ModbusAddressMap("Device", "8", "7.3",  1, 1, 1, dataType:DataType.Bit, registerType:ModbusRegisterType.Holding),
            };
            var client = new ControllerManager(controller, addressMaps);

            //await client.ReadAddressMap(addressMaps, true);
        }

        [TestMethod]
        public void Melsec_ReadAddressMap_Test()
        {
            string deviceName = "Device";
            var controller = new Controller { Name = deviceName };
            var addressMaps = new List<AddressMap>
            {
                new AddressMap(deviceName, "1", "D1"),
                new AddressMap(deviceName, "2", "D2"),
                new AddressMap(deviceName, "3", "D3"),
                new AddressMap(deviceName, "4", "D4"),
                new AddressMap(deviceName, "5", "D5"),
                new AddressMap(deviceName, "6", "D7.1"),
                new AddressMap(deviceName, "7", "D7.2"),
                new AddressMap(deviceName, "8", "D7.3"),
            };
            var client = new ControllerManager(controller, addressMaps);

            //client.ReadAddressMap(addressMaps, true);
        }
    }
}
