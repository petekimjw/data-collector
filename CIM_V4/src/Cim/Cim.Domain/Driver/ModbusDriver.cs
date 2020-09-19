using NLog;
using NModbus;
using NModbus.Extensions.Enron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tectone.Common.Mvvm;

namespace Cim.Domain.Driver
{
    /// <summary>
    /// Modbus Function Code. Bit(CoilStatus = 0, InputStatus=1) Word(InputRegister = 2, HoldingRegister =3)
    /// </summary>
    public enum FunctionCode { None=-1, Coil = 0, Input=1, InputRegister = 2, HoldingRegister =3}

    public class ModbusDriver : BindableBase, IDriver
    {
        #region 초기화
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IModbusMaster Plc { get; set; }

        public string Ip { get; set; }
        public int Port { get; set; }
        public int ReceiveTimeout { get; set; }

        public ModbusDriver(string ip = "127.0.0.1", int port = 502, int receiveTimeout = 5000)
        {
            Ip = ip;
            Port = port;
            ReceiveTimeout = receiveTimeout;

            Open();
            retryTimer.Elapsed += RetryTimer_Elapsed;
            retryTimer.Start();
        }

        private void RetryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Status != DriverStatus.Connected)
                RetryOpen();
        }

        protected IModbusMaster CreateModbusClient(string ip, int port, int receiveTimeout)
        {
            try
            {
                var tcpClient = new TcpClient(ip, port)
                {
                    ReceiveTimeout = receiveTimeout
                };
                var master = new ModbusFactory().CreateMaster(tcpClient);
                return master;
            }
            catch (Exception ex)
            {
                logger.Error($"CreateModbusClient Fail! op: {ip}, port: {port}, ex={ex}");
            }
            return null;
        }

        #endregion

        #region IDriver

        private DriverStatus _Status;
        public DriverStatus Status
        {
            get { return _Status; }
            set { Set(ref _Status, value); }
        }

        public bool Open()
        {
            bool result = false;
            try
            {
                Plc = CreateModbusClient(Ip, Port, ReceiveTimeout);
                if (Plc != null)
                {
                    result = true;
                    Status = DriverStatus.Connected;
                }
                else
                {
                    result = false;
                    Status = DriverStatus.Disconnected;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
                Status = DriverStatus.Disconnected;
            }
            logger.Debug($"[{Status}] [{result}] Ip={Ip}, Port={Port}, ReceiveTimeout={ReceiveTimeout}");
            return result;
        }

        public bool Close()
        {
            bool result = false;
            try
            {
                if(Plc != null)
                    Plc.Dispose();
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
            Status = DriverStatus.Disconnected;
            logger.Debug($"[{Status}] [{result}] Ip={Ip}, Port={Port}, ReceiveTimeout={ReceiveTimeout}");
            return result;
        }

        private Timer retryTimer = new Timer(3000);

        public bool RetryOpen()
        {
            bool result = false;
            try
            {
                if (Status != DriverStatus.Connected)
                {
                    result = Close();
                    result = Open();
                }
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
        public async Task<(int error, ushort[] results)> Read(
            string stringAddress, ushort startAddress= 1, int count = 1, bool isBit = false, 
            ushort slaveId = 1, int registerType = 1)
        {
            int error = 0;
            ushort[] results = null;

            if (Plc == null) return (-1, results);

            //Modbus Function Code. Bit(CoilStatus = 0, InputStatus=1) Word(InputRegister = 2, HoldingRegister =3)
            var type = (FunctionCode)Enum.Parse(typeof(FunctionCode), $"{registerType}");
            try
            {
                switch (type)
                {
                    case FunctionCode.Coil:
                        var boolResults = await Plc.ReadCoilsAsync((byte)slaveId, startAddress, (ushort)count);
                        results = boolResults.Select(m => Convert.ToUInt16(m)).ToArray();
                        break;
                    case FunctionCode.Input:
                        var boolResults2 = await Plc.ReadInputsAsync((byte)slaveId, startAddress, (ushort)count);
                        results = boolResults2.Select(m => Convert.ToUInt16(m)).ToArray();
                        break;
                    case FunctionCode.InputRegister:
                        results = await Plc.ReadInputRegistersAsync((byte)slaveId, startAddress, (ushort)count);
                        //var results2 = ModbusMaster.ReadHoldingRegisters32((byte)slaveAddress, startAddress, count);
                        break;
                    case FunctionCode.HoldingRegister:
                        results = await Plc.ReadHoldingRegistersAsync((byte)slaveId, startAddress, (ushort)count);
                        break;
                    default:
                        logger.Error($"Invalid registerType={registerType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ex.HResult={ex.HResult}, slaveId={slaveId}, startAddress={startAddress}, count={count}, ex={ex}");
                error = ex.HResult;
                Status = DriverStatus.Error;
            }

            return (error, results);
        }


    }
}
