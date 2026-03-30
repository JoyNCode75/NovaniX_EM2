using NovaniX_EM2.Communication;
using System;
using System.Threading.Tasks;

namespace NovaniX_EM2.Devices
{
    public class PMS_MiniCaptControl
    {
        private ModbusTcpConnection _modbus;
        private string _ipAddress;
        private int _port;

        public bool IsConnected => _modbus != null && _modbus.IsConnected;

        public PMS_MiniCaptControl(string ipAddress, int port)
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

        // ▼ 1. 시작 시퀀스 수정 ▼
        public async Task StartMeasurementAsync()
        {
            if (!IsConnected) return;

            // 1. Data Clear On
            await SetDataClearAsync();
            // 2. 잠시 대기 (100ms)
            await Task.Delay(100);
            // 3. Data Available Off
            await SetDataAvailableAsync(false);
            // 4. 잠시 대기 (100ms)
            await Task.Delay(100);

            // 5. 에어 샘플링 시작 (00/01 코일 - Offset 0 번지 On)
            await _modbus.WriteSingleCoilAsync(1, 0, true);
        }

        // ▼ 2. 정지 시퀀스 수정 ▼
        public async Task StopMeasurementAsync()
        {
            if (!IsConnected) return;

            // 1. Data Available On
            await SetDataAvailableAsync(true);
            // 2. 잠시 대기 (100ms)
            await Task.Delay(100);

            // 3. 에어 샘플링 정지 (00/01 코일 - Offset 0 번지 Off)
            await _modbus.WriteSingleCoilAsync(1, 0, false);
        }

        // --------------------------------------------------------
        // ▼ 새로 추가할 함수들 (Coils 제어) ▼
        // --------------------------------------------------------

        // Data Available (00/02 코일 -> Offset 1)
        public async Task SetDataAvailableAsync(bool isOn)
        {
            if (!IsConnected) return;
            await _modbus.WriteSingleCoilAsync(1, 1, isOn);
        }

        // Data Clear (00/03 코일 -> Offset 2)
        public async Task SetDataClearAsync()
        {
            if (!IsConnected) return;
            await _modbus.WriteSingleCoilAsync(1, 2, true);
            // 잠시 대기 (500ms)
            await Task.Delay(500);
            // Data Clear Off
            await _modbus.WriteSingleCoilAsync(1, 2, false);
        }

        // ▼ 수정된 부분: 에어 샘플러 결과 데이터 읽기 (Total Volume, Total Interval)
        public async Task<(float totalVolume, float totalInterval)> ReadSamplerDataAsync(bool isFloatMode)
        {
            if (!IsConnected) throw new Exception("Air Sampler와의 연결이 끊어졌습니다.");

            // 30231 ~ 30234 (Offset 230부터 4개 워드 읽기)
            ushort[] regs = await _modbus.ReadInputRegistersAsync(1, 230, 4);

            if (regs != null && regs.Length >= 4)
            {
                if (isFloatMode)
                {
                    // Float Mode (IEEE-754) 변환
                    byte[] volBytes = new byte[4];
                    volBytes[0] = (byte)(regs[1] & 0xFF);
                    volBytes[1] = (byte)(regs[1] >> 8);
                    volBytes[2] = (byte)(regs[0] & 0xFF);
                    volBytes[3] = (byte)(regs[0] >> 8);
                    float volume = BitConverter.ToSingle(volBytes, 0);

                    byte[] timeBytes = new byte[4];
                    timeBytes[0] = (byte)(regs[3] & 0xFF);
                    timeBytes[1] = (byte)(regs[3] >> 8);
                    timeBytes[2] = (byte)(regs[2] & 0xFF);
                    timeBytes[3] = (byte)(regs[2] >> 8);
                    float interval = BitConverter.ToSingle(timeBytes, 0);

                    return (volume, interval);
                }
                else
                {
                    // Integer Mode 변환
                    // Total Volume: 30231(High), 30232(Low) -> 10 Scale (10으로 나눔)
                    uint volRaw = (uint)((regs[0] << 16) | regs[1]);

                    // Total Interval: 30233(High), 30234(Low) -> 1000 Scale (1000으로 나눔)
                    uint timeRaw = (uint)((regs[2] << 16) | regs[3]);

                    return (volRaw / 10.0f, timeRaw / 1000.0f);
                }
            }
            return (0f, 0f);
        }

        // ▼ 추가: IEEE-754 Float (Coil 8번, Offset 7) 상태 읽기
        public async Task<bool> ReadIeeeFloatModeAsync()
        {
            if (!IsConnected) return false;
            // Slave 1번, Offset 7번지부터 1개의 코일 상태 읽기
            bool[] coils = await _modbus.ReadCoilsAsync(1, 7, 1);
            if (coils != null && coils.Length > 0)
            {
                return coils[0]; // True면 Float, False면 Integer
            }
            return false;
        }

        // ▼ 추가: IEEE-754 Float (Coil 8번, Offset 7) 상태 쓰기
        public async Task SetIeeeFloatModeAsync(bool isFloat)
        {
            if (!IsConnected) return;
            // Slave 1번, Offset 7번지 코일에 값 쓰기
            await _modbus.WriteSingleCoilAsync(1, 7, isFloat);
        }

        // ▼ 새로 추가할 부분: 장비 상태(Device Status) 읽기 ▼
        public async Task<ushort> ReadDeviceStatusAsync()
        {
            if (!IsConnected) return 0;

            // 30214 번지 (30001 기준 Offset 213) 읽기
            ushort[] regs = await _modbus.ReadInputRegistersAsync(1, 213, 1);
            if (regs != null && regs.Length > 0)
            {
                return regs[0]; // 0, 1, 257, 513, 769 등 반환
            }
            return 0;
        }

        // ▼ 에어 샘플러 측정 결과 시간 읽기 (수정본)
        public async Task<(DateTime? endTime, TimeSpan? elapsedTime)> ReadMeasurementTimesAsync()
        {
            if (!IsConnected) return (null, null);

            try
            {
                // 30216 ~ 30219 (Offset 215부터 4개 워드 읽기)
                ushort[] regs = await _modbus.ReadInputRegistersAsync(1, 215, 4);

                if (regs != null && regs.Length >= 4)
                {
                    // ★ 1. Time Stamp 변환 (프로토콜 규격상 time_t는 무조건 32비트 Integer로 처리해야 함)
                    uint timeStampSec = (uint)((regs[0] << 16) | regs[1]);

                    // 데이터가 아직 없거나 초기화 상태일 경우
                    if (timeStampSec == 0) return (null, null);

                    // Unix 초(Epoch)를 현재 PC의 로컬 시간(DateTime)으로 변환
                    DateTime endTime = DateTimeOffset.FromUnixTimeSeconds(timeStampSec).ToLocalTime().DateTime;

                    // ★ 2. Sample Time 변환 (요청하신 대로 Float 방식으로 파싱)
                    byte[] stBytes = new byte[4];
                    stBytes[0] = (byte)(regs[3] & 0xFF);
                    stBytes[1] = (byte)(regs[3] >> 8);
                    stBytes[2] = (byte)(regs[2] & 0xFF);
                    stBytes[3] = (byte)(regs[2] >> 8);
                    float sampleTimeFloat = BitConverter.ToSingle(stBytes, 0);

                    // 경과 시간 변환
                    // (주의: Float 모드에서는 스케일링(*100) 없이 초 단위 원본 값이 그대로 올 수 있습니다.)
                    // 만약 값이 100배 뻥튀기되어 표기된다면 `TimeSpan.FromSeconds(sampleTimeFloat / 100.0);` 로 수정하세요.
                    TimeSpan elapsedTime = TimeSpan.FromSeconds(sampleTimeFloat);

                    return (endTime, elapsedTime);
                }
            }
            catch
            {
                // 통신 에러나 변환 오류 발생 시 무시
            }
            return (null, null);
        }
        // --------------------------------------------------------
        // ▼ 새로 추가할 부분: 기기 정보 읽기 및 문자열 디코딩 ▼
        // --------------------------------------------------------

        // 16비트 워드(ushort) 배열을 ASCII 문자열로 변환하는 헬퍼 함수
        private string DecodeModbusString(ushort[] regs, int startIndex, int length)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                byte high = (byte)(regs[startIndex + i] >> 8);
                byte low = (byte)(regs[startIndex + i] & 0xFF);

                // (참고: 만약 글자가 "rPdoctua" 처럼 순서가 뒤집혀 나온다면 high와 low의 add 순서를 바꿔주세요)
                if (high != 0) bytes.Add(high); // Null(0x00) 문자 제외
                if (low != 0) bytes.Add(low);
            }
            return System.Text.Encoding.ASCII.GetString(bytes.ToArray()).Trim();
        }

        // 전체 기기 정보(Device Info) 읽어오기
        public async Task<List<(string Title, string Value)>> ReadDeviceInfoAsync()
        {
            var infoList = new List<(string Title, string Value)>();
            if (!IsConnected) return infoList;

            try
            {
                // 1. Firmware & Product Name (30001 ~ 30009 -> Offset 0부터 9개 워드)
                ushort[] regs1 = await _modbus.ReadInputRegistersAsync(1, 0, 9);
                string firmware = "Unknown";
                string productName = "Unknown";

                if (regs1 != null && regs1.Length >= 9)
                {
                    // 30002 번지 (Offset 1)에서 Firmware Version 읽기
                    ushort fwRaw = regs1[1];

                    // ▼ 수정된 부분: Firmware Version 디코딩 (입력값 / 100 = Major, 입력값 % 100 = Minor)
                    uint major = (uint)(fwRaw / 100);
                    uint minor = (uint)(fwRaw % 100);

                    // 요청하신 형식으로 문자열 조립 (110 입력 시 -> ver 1.10, 101 입력 시 -> ver 1.01)
                    firmware = $"ver {major}.{minor:D2}";

                    // 30003 ~ 30009 (Offset 2부터 7개 워드) - Product Name
                    productName = DecodeModbusString(regs1, 2, 7);
                }

                // 2. Calibration Date & Serial Number (30201 ~ 30211 -> Offset 200부터 11개 워드)
                ushort[] regs2 = await _modbus.ReadInputRegistersAsync(1, 200, 11);
                string calDate = "Unknown";
                string serialNum = "Unknown";

                if (regs2 != null && regs2.Length >= 11)
                {
                    // 30201 ~ 30203 (Offset 0, 1, 2)
                    ushort month = regs2[0];
                    ushort day = regs2[1];
                    ushort year = regs2[2];
                    calDate = $"{month:D2}/{day:D2}/{year:D4}";

                    // 30204 ~ 30211 (Offset 3부터 8개 워드)
                    serialNum = DecodeModbusString(regs2, 3, 8);
                }

                // 1열은 Title, 2열은 값으로 구성 (요청하신 2행~5행 순서대로 삽입)
                infoList.Add(("기기 이름", productName));
                infoList.Add(("시리얼번호", serialNum));
                infoList.Add(("교정일(M/D/Y)", calDate));
                infoList.Add(("Firmware Version", firmware));
            }
            catch
            {
                // 통신 에러 발생 시 무시
            }

            return infoList;
        }

        // ▼ 에어 샘플러 Flow Rate 및 Volume 읽기 (30222 ~ 30225)
        public async Task<(float flowRate, float volume)> ReadFlowAndVolumeAsync(bool isFloatMode)
        {
            if (!IsConnected) return (0f, 0f);

            try
            {
                // 30222 ~ 30225 (Offset 221부터 4개 워드 읽기)
                ushort[] regs = await _modbus.ReadInputRegistersAsync(1, 221, 4);

                if (regs != null && regs.Length >= 4)
                {
                    if (isFloatMode)
                    {
                        // IEEE-754 Float 변환
                        byte[] flowBytes = new byte[4];
                        flowBytes[0] = (byte)(regs[1] & 0xFF);
                        flowBytes[1] = (byte)(regs[1] >> 8);
                        flowBytes[2] = (byte)(regs[0] & 0xFF);
                        flowBytes[3] = (byte)(regs[0] >> 8);
                        float flowRate = BitConverter.ToSingle(flowBytes, 0);

                        byte[] volBytes = new byte[4];
                        volBytes[0] = (byte)(regs[3] & 0xFF);
                        volBytes[1] = (byte)(regs[3] >> 8);
                        volBytes[2] = (byte)(regs[2] & 0xFF);
                        volBytes[3] = (byte)(regs[2] >> 8);
                        float volume = BitConverter.ToSingle(volBytes, 0);

                        return (flowRate, volume);
                    }
                    else
                    {
                        // Integer 모드 변환 (매뉴얼 기준 Flow 10 Scale, Volume 10 Scale)
                        uint flowRaw = (uint)((regs[0] << 16) | regs[1]);
                        uint volRaw = (uint)((regs[2] << 16) | regs[3]);

                        return (flowRaw / 10.0f, volRaw / 10.0f);
                    }
                }
            }
            catch
            {
                // 통신 에러 무시
            }
            return (0f, 0f);
        }
        // --------------------------------------------------------
        // ▼ 에어 샘플러 Holding Registers 읽기/쓰기 (40003 ~ 40006) ▼
        // --------------------------------------------------------
        public async Task<(ushort interval, ushort holdTime, ushort repeat, ushort delayTime)> ReadSettingsAsync()
        {
            if (!IsConnected) return (0, 0, 0, 0);

            try
            {
                int interval = await _modbus.ReadHoldingRegisterAsync(1, 2); // 40003
                int hold = await _modbus.ReadHoldingRegisterAsync(1, 3);     // 40004
                int repeat = await _modbus.ReadHoldingRegisterAsync(1, 4);   // 40005
                int delay = await _modbus.ReadHoldingRegisterAsync(1, 5);    // 40006

                return ((ushort)interval, (ushort)hold, (ushort)repeat, (ushort)delay);
            }
            catch
            {
                return (0, 0, 0, 0);
            }
        }

        public async Task SetSampleIntervalAsync(ushort val) { if (IsConnected) await _modbus.WriteSingleRegisterAsync(1, 2, val); }
        public async Task SetHoldTimeAsync(ushort val) { if (IsConnected) await _modbus.WriteSingleRegisterAsync(1, 3, val); }
        public async Task SetRepeatCountAsync(ushort val) { if (IsConnected) await _modbus.WriteSingleRegisterAsync(1, 4, val); }
        public async Task SetDelayTimeAsync(ushort val) { if (IsConnected) await _modbus.WriteSingleRegisterAsync(1, 5, val); }

        // --------------------------------------------------------
        // ▼ 에어 샘플러 Volume Mode 제어 (Coil 00/09 -> Offset 8) ▼
        // --------------------------------------------------------
        public async Task<bool> ReadVolumeModeAsync()
        {
            if (!IsConnected) return false;
            try
            {
                // Offset 8 (00/09)
                bool[] coils = await _modbus.ReadCoilsAsync(1, 8, 1);
                if (coils != null && coils.Length > 0) return coils[0];
            }
            catch { }
            return false;
        }

        public async Task SetVolumeModeAsync(bool isVolumeMode)
        {
            if (!IsConnected) return;
            await _modbus.WriteSingleCoilAsync(1, 8, isVolumeMode);
        }
    }
}