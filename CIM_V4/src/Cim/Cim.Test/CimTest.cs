using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using Cim.Domain.DataCollect;
using Cim.Domain.Driver;
using Cim.Domain.Model;
using Cim.Manager.Views;
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


        [TestMethod]
        public void AddressMap_Convert테스트()
        {
            //주의! PLC는 char 가 16bit(UTF-16) 아닌, 8bit(ASCII) 이다
            char[] chars = new char[4];
            chars[0] = 'A';
            chars[1] = 'B';
            chars[2] = 'C';
            chars[3] = 'D';

            var charsBytes = new List<byte>();
            foreach (var item in chars)
            {
                charsBytes.Add(BitConverter.GetBytes(item)[0]);
            }

            var ushorts = new List<ushort>();
            for (int i = 0; i < charsBytes.Count; i += 2)
            {
                ushorts.Add(BitConverter.ToUInt16(charsBytes.ToArray(), i));
            }

            //test(ushorts);

        }

        [TestMethod]
        public void test()
        {
            short a = -5512;
            var aBit = BitConverter.GetBytes(a);

            var c = BitConverter.ToUInt16(aBit, 0);

            var b = Convert.ToUInt16(a);
            
        }

        [TestMethod]
        public void AddressMap_Ascii테스트()
        {
            //BitConverter.GetBytes
            //Convert.ToUInt16
            //Encoding.ASCII.GetString

            //주의! PLC는 char 가 16bit(UTF-16) 아닌, 8bit(ASCII) 이다
            char[] chars = new char[4];
            chars[0] = 'A';
            chars[1] = 'B';
            chars[2] = 'C';
            chars[3] = 'D';

            var charsBytes = new List<byte>();
            foreach (var item in chars)
            {
                charsBytes.Add(BitConverter.GetBytes(item)[0]);
            }

            var ushorts = new List<ushort>();
            for (int i = 0; i < charsBytes.Count; i += 2)
            {
                ushorts.Add(BitConverter.ToUInt16(charsBytes.ToArray(), i));
            }

            var text = Encoding.ASCII.GetString(charsBytes.ToArray());

            Assert.IsTrue(text == "ABCD");
        }

        [TestMethod]
        public void AddressMap_ByteOrder테스트()
        {
            var config = new DefaultConfigManager();
            config.Init();
        }

        [TestMethod]
        public void AddressMap_Map테스트()
        {
            var mapping = new MapperConfigurationExpression();

            mapping.CreateMap<AddressMap, AddressMap>();
            mapping.CreateMap<AddressMap, ModbusAddressMap>();
            mapping.CreateMap<AddressMap, AddressData>();
            mapping.CreateMap<ModbusAddressMap, AddressData>();
            mapping.CreateMap<AddressData, AddressDataWrapper>();

            var config = new MapperConfiguration(mapping);
            var Mapper = new Mapper(config);

            string deviceName = "Device";
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

            var a = Mapper.Map<List<AddressData>>(addressMaps);
            var b = Mapper.Map<List<AddressDataWrapper>>(a);
            var c = Mapper.Map<List<AddressDataWrapper>>(addressMaps);
        }

    }
}
