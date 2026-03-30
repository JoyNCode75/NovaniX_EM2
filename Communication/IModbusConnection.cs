using System.Threading.Tasks;

namespace NovaniX_EM2.Communication
{
    public interface IModbusConnection
    {
        bool IsConnected { get; }
        Task<bool> ConnectAsync(string address, int portOrBaudRate);
        void Disconnect();
        Task<int> ReadHoldingRegisterAsync(byte slaveId, ushort registerAddress);
        Task<bool> WriteSingleRegisterAsync(byte slaveId, ushort registerAddress, ushort value);
        // IModbusConnection.cs 내 수정
        Task<bool> ConnectAsync(string address, int portOrBaudRate, System.IO.Ports.Parity parity = System.IO.Ports.Parity.Even);
        Task<bool> ConnectAsync(string address, int portOrBaudRate, System.IO.Ports.Parity parity = System.IO.Ports.Parity.Even, System.IO.Ports.StopBits stopBits = System.IO.Ports.StopBits.One);
        // ▼ 새로 추가된 메서드 선언 ▼
        Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort numberOfPoints);

        // ▼ 새로 추가할 I/O 제어용 메서드들 ▼
        Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort numberOfPoints);
        Task<bool[]> ReadDiscreteInputsAsync(byte slaveId, ushort startAddress, ushort numberOfPoints);
        Task WriteSingleCoilAsync(byte slaveId, ushort coilAddress, bool value);
    }
}