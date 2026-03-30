using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace NovaniX_EM2.Communication
{
    public class ModbusRtuConnection : IModbusConnection
    {
        private SerialPort? _serialPort;    // Null 허용(?) 기호 붙여주기
        private readonly object _lock = new object();

        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

        // ------------------------------------------------------------------
        // [수정된 부분] 1. 인터페이스(IModbusConnection) 규칙을 만족시키는 메서드 (에러 해결)
        // ------------------------------------------------------------------
        public Task<bool> ConnectAsync(string portName, int baudRate = 115200)
        {
            // 오리엔탈 모터는 기본적으로 Even(짝수) 패리티를 사용하므로 Parity.Even을 강제로 넘겨줍니다.
            return ConnectAsync(portName, baudRate, Parity.Even);
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

        // ------------------------------------------------------------------
        // [수정된 부분] 2. 실제 Parity 옵션을 받아서 통신 포트를 여는 로직
        // ------------------------------------------------------------------
        public Task<bool> ConnectAsync(string portName, int baudRate, Parity parity)
        {
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

        public void Disconnect()
        {
            if (IsConnected)
            {
                _serialPort!.Close();
                _serialPort!.Dispose();
            }
        }

        public async Task<int> ReadHoldingRegisterAsync(byte slaveId, ushort registerAddress)
        {
            if (!IsConnected) return 0;

            byte[] request = new byte[8];
            request[0] = slaveId;
            request[1] = 0x03;
            request[2] = (byte)(registerAddress >> 8);
            request[3] = (byte)(registerAddress & 0xFF);
            request[4] = 0x00;
            request[5] = 0x01;
            AddCRC16(request, 6);

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        // try 블록 내부의 _serialPort 호출 부분에 느낌표(!) 추가
                        _serialPort!.DiscardInBuffer();
                        _serialPort!.Write(request, 0, request.Length);

                        byte[] response = new byte[7];
                        int bytesRead = 0;
                        while (bytesRead < 7)
                            bytesRead += _serialPort!.Read(response, bytesRead, 7 - bytesRead);

                        return (response[3] << 8) | response[4];
                    }
                    catch { return 0; }
                }
            });
        }

        public async Task<bool> WriteSingleRegisterAsync(byte slaveId, ushort registerAddress, ushort value)
        {
            if (!IsConnected) return false;

            byte[] request = new byte[8];
            request[0] = slaveId;
            request[1] = 0x06;
            request[2] = (byte)(registerAddress >> 8);
            request[3] = (byte)(registerAddress & 0xFF);
            request[4] = (byte)(value >> 8);
            request[5] = (byte)(value & 0xFF);
            AddCRC16(request, 6);

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        // try 블록 내부의 _serialPort 호출 부분에 느낌표(!) 추가
                        _serialPort!.DiscardInBuffer();
                        _serialPort!.Write(request, 0, request.Length);

                        byte[] response = new byte[8];
                        int bytesRead = 0;
                        while (bytesRead < 8)
                            bytesRead += _serialPort!.Read(response, bytesRead, 8 - bytesRead);
                        return true;
                    }
                    catch { return false; }
                }
            });
        }

        // ------------------------------------------------------------------
        // [추가된 부분] IModbusConnection 인터페이스의 ReadCoilsAsync 구현
        // ------------------------------------------------------------------
        public async Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (!IsConnected) return new bool[0];

            byte[] request = new byte[8];
            request[0] = slaveId;
            request[1] = 0x01; // Function Code 0x01 (Read Coils)
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)(startAddress & 0xFF);
            request[4] = (byte)(numberOfPoints >> 8);
            request[5] = (byte)(numberOfPoints & 0xFF);
            AddCRC16(request, 6);

            // 응답 데이터 길이 계산: SlaveID(1) + FunctionCode(1) + ByteCount(1) + DataBytes(N) + CRC(2)
            int byteCount = (numberOfPoints + 7) / 8;
            int responseLength = 5 + byteCount;

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        _serialPort!.DiscardInBuffer();
                        _serialPort!.Write(request, 0, request.Length);

                        byte[] response = new byte[responseLength];
                        int bytesRead = 0;
                        while (bytesRead < responseLength)
                        {
                            bytesRead += _serialPort!.Read(response, bytesRead, responseLength - bytesRead);
                        }

                        // 받은 바이트 배열을 bool 배열로 변환
                        bool[] result = new bool[numberOfPoints];
                        for (int i = 0; i < numberOfPoints; i++)
                        {
                            int byteIndex = 3 + (i / 8);
                            int bitIndex = i % 8;
                            result[i] = (response[byteIndex] & (1 << bitIndex)) != 0;
                        }
                        return result;
                    }
                    catch
                    {
                        // 타임아웃 등의 에러 발생 시 지정된 크기의 빈 배열 반환
                        return new bool[numberOfPoints];
                    }
                }
            });
        }

        // ------------------------------------------------------------------
        // [추가된 부분] IModbusConnection 인터페이스의 누락된 메서드 구현
        // ------------------------------------------------------------------

        // 1. ReadInputRegistersAsync (Function Code 0x04)
        public async Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (!IsConnected) return new ushort[0];

            byte[] request = new byte[8];
            request[0] = slaveId;
            request[1] = 0x04; // Function Code 0x04
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)(startAddress & 0xFF);
            request[4] = (byte)(numberOfPoints >> 8);
            request[5] = (byte)(numberOfPoints & 0xFF);
            AddCRC16(request, 6);

            int responseLength = 5 + (numberOfPoints * 2);

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        _serialPort!.DiscardInBuffer();
                        _serialPort!.Write(request, 0, request.Length);

                        byte[] response = new byte[responseLength];
                        int bytesRead = 0;
                        while (bytesRead < responseLength)
                        {
                            bytesRead += _serialPort!.Read(response, bytesRead, responseLength - bytesRead);
                        }

                        ushort[] result = new ushort[numberOfPoints];
                        for (int i = 0; i < numberOfPoints; i++)
                        {
                            result[i] = (ushort)((response[3 + (i * 2)] << 8) | response[4 + (i * 2)]);
                        }
                        return result;
                    }
                    catch { return new ushort[0]; }
                }
            });
        }

        // 2. ReadDiscreteInputsAsync (Function Code 0x02)
        public async Task<bool[]> ReadDiscreteInputsAsync(byte slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if (!IsConnected) return new bool[0];

            byte[] request = new byte[8];
            request[0] = slaveId;
            request[1] = 0x02; // Function Code 0x02
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)(startAddress & 0xFF);
            request[4] = (byte)(numberOfPoints >> 8);
            request[5] = (byte)(numberOfPoints & 0xFF);
            AddCRC16(request, 6);

            int byteCount = (numberOfPoints + 7) / 8;
            int responseLength = 5 + byteCount;

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        _serialPort!.DiscardInBuffer();
                        _serialPort!.Write(request, 0, request.Length);

                        byte[] response = new byte[responseLength];
                        int bytesRead = 0;
                        while (bytesRead < responseLength)
                        {
                            bytesRead += _serialPort!.Read(response, bytesRead, responseLength - bytesRead);
                        }

                        bool[] result = new bool[numberOfPoints];
                        for (int i = 0; i < numberOfPoints; i++)
                        {
                            int byteIndex = 3 + (i / 8);
                            int bitIndex = i % 8;
                            result[i] = (response[byteIndex] & (1 << bitIndex)) != 0;
                        }
                        return result;
                    }
                    catch { return new bool[numberOfPoints]; }
                }
            });
        }

        // 3. WriteSingleCoilAsync (Function Code 0x05)
        public async Task WriteSingleCoilAsync(byte slaveId, ushort coilAddress, bool value)
        {
            if (!IsConnected) return;

            byte[] request = new byte[8];
            request[0] = slaveId;
            request[1] = 0x05; // Function Code 0x05
            request[2] = (byte)(coilAddress >> 8);
            request[3] = (byte)(coilAddress & 0xFF);
            // 0xFF00 은 ON, 0x0000 은 OFF
            request[4] = value ? (byte)0xFF : (byte)0x00;
            request[5] = 0x00;
            AddCRC16(request, 6);

            await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        _serialPort!.DiscardInBuffer();
                        _serialPort!.Write(request, 0, request.Length);

                        byte[] response = new byte[8];
                        int bytesRead = 0;
                        while (bytesRead < 8)
                        {
                            bytesRead += _serialPort!.Read(response, bytesRead, 8 - bytesRead);
                        }
                    }
                    catch { /* 무시 */ }
                }
            });
        }

        // Modbus 규격 CRC16 (0xA001 Polynomial)
        private void AddCRC16(byte[] buffer, int length)
        {
            ushort crc = 0xFFFF;
            for (int pos = 0; pos < length; pos++)
            {
                crc ^= buffer[pos];
                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                        crc >>= 1;
                }
            }
            buffer[length] = (byte)(crc & 0xFF);
            buffer[length + 1] = (byte)((crc >> 8) & 0xFF);
        }
    }
}