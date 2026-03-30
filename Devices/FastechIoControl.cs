using NovaniX_EM2.Communication;
using System;
using System.Threading.Tasks;

namespace NovaniX_EM2.Devices
{
    public class FastechIoControl
    {
        private ModbusTcpConnection _modbus;
        private string _ipAddress;
        private int _port;

        public bool IsConnected => _modbus != null && _modbus.IsConnected;

        public FastechIoControl(string ipAddress, int port = 502)
        {
            _ipAddress = ipAddress;
            _port = port;
            _modbus = new ModbusTcpConnection();
        }

        public async Task<bool> ConnectAsync()
        {
            return await _modbus.ConnectAsync(_ipAddress, _port);
        }

        public void Disconnect()
        {
            _modbus?.Disconnect();
        }

        // 전체 Input 모니터링 (Total 갯수에 따라 읽어오는 워드 수 계산)
        public async Task<ushort[]> ReadAllInputsAsync(byte slaveId = 1, int totalPoints = 8)
        {
            if (!IsConnected) return new ushort[0];
            try
            {
                int wordCount = (totalPoints + 15) / 16; // 16포인트당 1워드 계산
                return await _modbus.ReadInputRegistersAsync(slaveId, 0x0000, (ushort)wordCount);
            }
            catch { return new ushort[0]; }
        }

        // 전체 Output 모니터링 (출력 램프 상태 동기화용)
        public async Task<ushort[]> ReadAllOutputsAsync(byte slaveId = 1, int totalPoints = 8)
        {
            if (!IsConnected) return new ushort[0];
            try
            {
                int wordCount = (totalPoints + 15) / 16;
                // Output 상태를 읽어올 수 있는 레지스터 주소 (매뉴얼에 따라 변경 필요. 예: 0x0010)
                return await _modbus.ReadInputRegistersAsync(slaveId, 0x0010, (ushort)wordCount);
            }
            catch { return new ushort[0]; }
        }

        // 개별 Input 읽기 (Discrete Inputs)
        public async Task<bool> ReadSingleInputAsync(byte slaveId, ushort pinAddress)
        {
            if (!IsConnected) return false;
            try
            {
                var result = await _modbus.ReadDiscreteInputsAsync(slaveId, pinAddress, 1);
                return result != null && result.Length > 0 && result[0];
            }
            catch { return false; }
        }

        // 특정 Output 핀 1개 제어 (Modbus Coil Write)
        public async Task WriteOutputAsync(byte slaveId, ushort pinAddress, bool isON)
        {
            if (!IsConnected) return;
            try
            {
                await _modbus.WriteSingleCoilAsync(slaveId, pinAddress, isON);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"I/O 쓰기 에러: {ex.Message}");
            }
        }
    }
}