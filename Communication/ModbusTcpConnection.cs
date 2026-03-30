// 수정 후 (최신 NModbus 방식)
using Modbus.Device;
using System;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace NovaniX_EM2.Communication
{
    public class ModbusTcpConnection : IModbusConnection
    {
        private TcpClient? _tcpClient;
        private SerialPort? _serialPort;
        private IModbusMaster? _modbusMaster; // 타입 변경

        public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

        public async Task<bool> ConnectAsync(string address, int portOrBaudRate)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(address, portOrBaudRate);

                // ★ TCP 연결 성공 시 NModbus 마스터 객체 생성
                // NModbus4 방식으로 객체 생성
                _modbusMaster = ModbusIpMaster.CreateIp(_tcpClient);

                return true;
            }
            catch { return false; }
        }

        public Task<bool> ConnectAsync(string portName, int baudRate, Parity parity)
        {
            // (기존의 시리얼 포트 연결 로직 유지)
            return Task.Run(() =>
            {
                try
                {
                    _serialPort = new SerialPort(portName, baudRate, parity, 8, StopBits.One)
                    {
                        ReadTimeout = 500,
                        WriteTimeout = 500
                    };
                    _serialPort.Open();
                    return true;
                }
                catch { return false; }
            });
        }

        // 파라미터에 stopBits 추가
        public Task<bool> ConnectAsync(string portName, int baudRate, Parity parity, StopBits stopBits = StopBits.One)
        {
            return Task.Run(() =>
            {
                try
                {
                    // 전달받은 통신 설정값을 시리얼 포트에 적용
                    _serialPort = new SerialPort(portName, baudRate, parity, 8, stopBits)
                    {
                        ReadTimeout = 500,
                        WriteTimeout = 500
                    };
                    _serialPort.Open();
                    return true;
                }
                catch { return false; }
            });
        }

        public void Disconnect()
        {
            _modbusMaster?.Dispose(); // 리소스 해제
            _tcpClient?.Close();
            _serialPort?.Close();
        }

        // ========================================================
        // ★ NModbus4를 이용한 실제 통신 메서드 (비동기 처리)
        // ========================================================

        public async Task<int> ReadHoldingRegisterAsync(byte slaveId, ushort registerAddress)
        {
            if (_modbusMaster == null) return 0;
            var result = await _modbusMaster.ReadHoldingRegistersAsync(slaveId, registerAddress, 1);
            return result.Length > 0 ? result[0] : 0;
        }

        public async Task<bool> WriteSingleRegisterAsync(byte slaveId, ushort registerAddress, ushort value)
        {
            if (_modbusMaster == null) return false;
            await _modbusMaster.WriteSingleRegisterAsync(slaveId, registerAddress, value);
            return true;
        }

        public async Task WriteSingleCoilAsync(byte slaveId, ushort coilAddress, bool value)
        {
            if (_modbusMaster == null) throw new InvalidOperationException("장비가 연결되지 않았습니다.");
            await _modbusMaster.WriteSingleCoilAsync(slaveId, coilAddress, value);
        }

        public async Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (_modbusMaster == null) throw new InvalidOperationException("장비가 연결되지 않았습니다.");
            return await _modbusMaster.ReadInputRegistersAsync(slaveId, startAddress, numberOfPoints);
        }

        // ▼ 새로 추가된 메서드: 코일(Coils) 상태 읽기 ▼
        public async Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (_modbusMaster == null) throw new InvalidOperationException("장비가 연결되지 않았습니다.");
            return await _modbusMaster.ReadCoilsAsync(slaveId, startAddress, numberOfPoints);
        }

        // ▼ 새로 추가된 메서드: 개별 입력(Discrete Inputs) 상태 읽기 ▼
        public async Task<bool[]> ReadDiscreteInputsAsync(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (_modbusMaster == null) throw new InvalidOperationException("장비가 연결되지 않았습니다.");
            // NModbus4에서 Discrete Input(Function Code 2)을 읽는 메서드는 ReadInputsAsync 입니다.
            return await _modbusMaster.ReadInputsAsync(slaveId, startAddress, numberOfPoints);
        }
    }
}