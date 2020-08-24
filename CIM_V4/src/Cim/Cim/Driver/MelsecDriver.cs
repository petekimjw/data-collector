using ActProgTypeLib;
using ActUtlTypeLib;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Driver
{
    public enum MelsecDeviceType { None=0, Block=1, Buffer=3 }

    public class MelsecDriver : IDriver
    {
        #region 초기화
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected IActProgType Plc2;
        protected readonly IActUtlType Plc;

        public MelsecDriver()
        {

        }

        public bool Init()
        {
            return Open();
        }
        public bool Open()
        {
            bool result = false;
            try
            {
                Plc.Open();
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
                Plc.Close();
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
        /// Melsec 버퍼(1bit) 영역, 디바이스블럭(16bit Word) 영역 데이터읽기.
        /// 다른영역을 한번에 읽으면 예외발생.
        /// 더블워드(32bit int)는 바이트오더(ABCD, CDAB)를 고려하여 파싱하여야 한다.
        /// </summary>
        /// <param name="stringAddress"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<(int error, ushort[] results)> ReadRegister(
            string stringAddress, ushort startAddress, int count =1, bool isBit = false, 
            ushort slaveId = 1, int registerType = 1)
        {
            ushort[] results = new ushort[count];
            short[] tempResults = new short[count];

            int error = 0;
            await Task.Run(new Action(() =>
            {
                try
                {
                    error = Plc.ReadDeviceBlock2(stringAddress, count, out tempResults[0]);
                    //var a = mPlc.ReadDeviceBlock(startAddress, length, out results[0]);

                    results = tempResults.Select(m => Convert.ToUInt16(m)).ToArray();
                }
                catch (Exception ex)
                {
                    logger.Error($"ex={ex}");
                }
            }));

            return (error, results);
        }
    }
}
