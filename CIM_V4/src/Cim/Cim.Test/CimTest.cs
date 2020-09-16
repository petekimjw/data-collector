using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cim.DataCollect;
using Cim.Driver;
using Cim.Manager;
using Cim.Model;
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
                new ModbusAddressMap("Lami", "1", "1", 1, 0, null, 1, dataType:DataType.Word16, functionCode:FunctionCode.HoldingRegister),
                new ModbusAddressMap("Lami", "2", "2",  1, 0, null, 1, dataType:DataType.Word16, functionCode:FunctionCode.HoldingRegister),
                new ModbusAddressMap("Lami", "3", "3",  1, 0, null, 1, dataType:DataType.Word16, functionCode:FunctionCode.HoldingRegister),
                new ModbusAddressMap("Lami", "4", "4",  1, 0, null, 1, dataType:DataType.Word16, functionCode:FunctionCode.HoldingRegister),
                new ModbusAddressMap("Lami", "5", "5",  1, 0, null, 1, dataType:DataType.Word16, functionCode:FunctionCode.HoldingRegister),
                new ModbusAddressMap("Lami", "6", "7.1",  1, 0, null, 1, dataType:DataType.Bit, functionCode:FunctionCode.Coil),
                new ModbusAddressMap("Lami", "7", "7.2",  1, 0, null, 1, dataType:DataType.Bit, functionCode:FunctionCode.Coil),
                new ModbusAddressMap("Lami", "8", "7.3",  1, 0, null, 1, dataType:DataType.Bit, functionCode:FunctionCode.Coil),
            };
            //var client = new ControllerManager(controller, addressMaps);

            //await client.ReadAddressMap(addressMaps, true);
            var timer = new TimerDataCollect(new FakeDriver(), addressMaps, 1000, null);
            var a = timer.GroupingAddressMapsByFunctionCode(addressMaps.Cast<ModbusAddressMap>().ToList());
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
            //var client = new ControllerManager(controller, addressMaps);

            //client.ReadAddressMap(addressMaps, true);
        }

        [TestMethod]
        public void ConfigManager_초기화테스트()
        {
            var config = new DefaultConfigManager();
            config.Init();
        }


    }
}
