using System.Threading.Tasks;

namespace CIM.Driver
{
    public interface IDriver
    {
        bool Open();
        bool Close();
        /// <summary>
        /// 장치의 데이터를 읽기.
        /// Melsec 버퍼(1bit) 영역, 디바이스블럭(16bit Word) 영역 데이터읽기.
        /// Modbus 코일(1bit bool) 영역, 레지스터(16bit ushort, Word) 영역, 레지스터 2개 더블워드(32bit int) 데이터를 읽기.
        /// </summary>
        /// <param name="stringAddress">Melsec등 어드레스주소. stringAddress/startAddress 둘중 하나만 사용</param>
        /// <param name="startAddress">Modbus등 어드레스주소. stringAddress/startAddress 둘중 하나만 사용</param>
        /// <param name="count">startAddress 에서 읽을 블럭개수</param>
        /// <param name="isBit">Bit여부</param>
        /// <param name="slaveId">Modbus경우 slaveId</param>
        /// <param name="registerType">ModbusRegisterType등 별도의 구분이 필요한 값</param>
        /// <returns></returns>
        Task<(int error, ushort[] results)> ReadRegister(
            string stringAddress, ushort startAddress, int count=1, bool isBit=false, 
            ushort slaveId=1, int registerType=1);
    }
}
