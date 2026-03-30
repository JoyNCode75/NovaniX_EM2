using System.Threading.Tasks;
using NovaniX_EM2.Communication;

namespace NovaniX_EM2.Devices
{
    // 범용 상태 모니터링 클래스 (다른 모터, 컨베이어 등에서도 사용 가능)
    public class ModbusDeviceState
    {
        private readonly IModbusConnection _connection;
        private readonly byte _slaveId;
        // 해당 클래스 내부의 SlaveId 속성을 찾아 아래처럼 수정합니다.
        public byte SlaveId { get; set; }

        public ModbusDeviceState(IModbusConnection connection, byte slaveId = 1)
        {
            _connection = connection;
            _slaveId = slaveId;
        }

        public async Task<int> ReadRegisterAsync(ushort address)
        {
            return await _connection.ReadHoldingRegisterAsync(_slaveId, address);
        }
    }
}