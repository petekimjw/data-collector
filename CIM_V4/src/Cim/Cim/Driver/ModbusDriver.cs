using NLog;
using NModbus;
using NModbus.Extensions.Enron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Driver
{
    public enum ModbusRegisterType { None=0, Holding=1, Input=2, Coil=3 }

    public class ModbusDriver : IDriver
    {
        #region 초기화
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IModbusMaster Plc { get; set; }

        protected IModbusMaster CreateModbusClient(string ip, int port, int receiveTimeout)
        {
            try
            {
                var mTcpClient = new TcpClient(ip, port);
                mTcpClient.ReceiveTimeout = receiveTimeout;
                var master = new ModbusFactory().CreateMaster(mTcpClient);
                return master;
            }
            catch (Exception ex)
            {
                logger.Error($"CreateModbusClient Fail! op: {ip}, port: {port}, ex={ex}");
            }
            return null;
        }

        public string Ip { get; set; }
        public int Port { get; set; }
        public int ReceiveTimeout { get; set; }

        public ModbusDriver(string ip = "127.0.0.1", int port = 502, int receiveTimeout = 5000)
        {
            Ip = ip;
            Port = port;
            ReceiveTimeout = receiveTimeout;

            Open();
        }

        public bool Open()
        {
            bool result = false;
            try
            {
                Plc = CreateModbusClient(Ip, Port, ReceiveTimeout);
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
            return result;
        }

        public bool Close()
        {
            bool result = false;
            try
            {

                result = true;
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
            return result;
        } 
        #endregion

        /// <summary>
        /// Modbus 코일(1bit bool) 영역, 레지스터(16bit ushort, Word) 영역, 레지스터 2개 더블워드(32bit int) 데이터를 읽기.
        /// 다른영역을 한번에 읽으면 예외발생.
        /// 더블워드(32bit int)는 바이트오더(ABCD, CDAB)를 고려하여 파싱하여야 한다.
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="startAddress"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<(int error, ushort[] results)> ReadRegister(
            string stringAddress, ushort startAddress= 1, int count = 1, bool isBit = false, 
            ushort slaveId = 1, int registerType = 1)
        {
            int error = 0;
            ushort[] results = null;
            var type = (ModbusRegisterType)Enum.Parse(typeof(ModbusRegisterType), $"{registerType}");
            try
            {
                if (isBit)
                {
                    var boolResults = await Plc.ReadCoilsAsync((byte)slaveId, startAddress, (ushort)count);
                    results = boolResults.Select(m => Convert.ToUInt16(m)).ToArray();
                }
                else if(type == ModbusRegisterType.Holding)
                {
                    results = await Plc.ReadHoldingRegistersAsync((byte)slaveId, startAddress, (ushort)count);
                }
                else if (type == ModbusRegisterType.Input)
                {
                    results = await Plc.ReadInputRegistersAsync((byte)slaveId, startAddress, (ushort)count);
                    //var results2 = ModbusMaster.ReadHoldingRegisters32((byte)slaveAddress, startAddress, count);
                }
                else
                {
                    logger.Error($"Invalid registerType={registerType}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
                error = ex.HResult;
            }

            return (error, results);
        }


    }
}
