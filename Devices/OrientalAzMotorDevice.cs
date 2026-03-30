using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NovaniX_EM2.Devices
{
    public class OrientalAzMotorDevice
    {
//        private const string DllPath = "INA_DRIVE_DLL.dll";
        private const string DllPath = "INA_AZ_DLL.dll";

        public int CommIndex { get; set; }
        public byte SlaveId { get; set; }

        public OrientalAzMotorDevice(int commIndex, byte slaveId)
        {
            CommIndex = commIndex;
            SlaveId = slaveId;
        }

        // 1. 통신 초기화 및 해제 (28p)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_INITIALIZE(out int nIndex, int nPort, int nBaudrate, int nDatabit, int nParitybit, int nStopbit);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_UNINITIALIZE(int nIndex);

        // 2. Servo On/Off (MSO 입력 신호) (32p)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_MSO(int nIndex, int nSlaveNo, int nOnOff);

        // 3. 정지 (30p)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_STOP(int nIndex, int nSlaveNo);

        // 4. JOG 동작 (31p)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_JOG_P(int nIndex, int nSlaveNo, int nOnOff);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_JOG_N(int nIndex, int nSlaveNo, int nOnOff);

        // 5. 모니터링 명령 (34p)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_CURRENT_POSITION(int nIndex, int nSlaveNo);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_CURRENT_ALARM(int nIndex, int nSlaveNo);

        // 6. 알람 리셋 (33p)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_ALARM_RESET(int nIndex, int nSlaveNo);

        // 7. 위치 이동 설정 및 시작 (39p, 30p)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo, int nValue);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_START(int nIndex, int nSlaveNo, int nDataNo);

        // 기존 DllImport 선언부 아래에 추가
        // 8. 원점 복귀 (Home)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_HOME(int nIndex, int nSlaveNo);
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_ZHOME(int nIndex, int nSlaveNo);

        // 9. 상태 클리어 (Clear)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_SET_CLEAR(int nIndex, int nSlaveNo);

        // 기존 DllImport 선언부 하단에 추가
        // 10. SD Data 읽기 함수
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_DATA_ACC_RATE(int nIndex, int nSlaveNo, int nDataNo);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_DATA_DEC_RATE(int nIndex, int nSlaveNo, int nDataNo);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_DATA_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nDataNo);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_DATA_DELAY_TIME(int nIndex, int nSlaveNo, int nDataNo);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_DATA_LINK(int nIndex, int nSlaveNo, int nDataNo);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_DATA_NEXT_DATA_NO(int nIndex, int nSlaveNo, int nDataNo);

        // ========================================================
        // ▼ MainTaskController 연동용 상태 확인 및 제어 메서드 ▼
        // ========================================================
        // 알람 코드 읽기 (0이면 정상, 그 외는 알람 코드)
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_ALARM(int nIndex, int nSlaveNo);

        // 현재 지령 위치 읽기
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int INA_AZ_GET_COMMAND_POSITION(int nIndex, int nSlaveNo);
        // =========================================================
        // [ C# Wrapper Methods ]
        // =========================================================
        public async Task SetServoOnAsync(bool isOn)
        {
            await Task.Run(() => INA_AZ_SET_MSO(CommIndex, SlaveId, isOn ? 1 : 0));
        }

        public async Task StopAsync()
        {
            await Task.Run(() => INA_AZ_SET_STOP(CommIndex, SlaveId));
        }

        public async Task JogAsync(bool isForward, bool isStop = false)
        {
            await Task.Run(() =>
            {
                if (isStop)
                {
                    INA_AZ_SET_JOG_P(CommIndex, SlaveId, 0);
                    INA_AZ_SET_JOG_N(CommIndex, SlaveId, 0);
                }
                else
                {
                    if (isForward) INA_AZ_SET_JOG_P(CommIndex, SlaveId, 1);
                    else INA_AZ_SET_JOG_N(CommIndex, SlaveId, 1);
                }
            });
        }

        public async Task MoveAbsoluteAsync(int dataNo, int targetPosition, int velocity)
        {
            await Task.Run(() =>
            {
                INA_AZ_SET_DATA_POSITION(CommIndex, SlaveId, dataNo, targetPosition);
//                INA_AZ_SET_DATA_VELOCITY(CommIndex, SlaveId, dataNo, velocity);
                INA_AZ_SET_START(CommIndex, SlaveId, dataNo); // 설정한 DataNo로 동작 개시
            });
        }

        // [ C# Wrapper Methods ] 영역 내에 다음 메서드 추가
        public async Task<int> HomeAsync()
        {
            return await Task.Run(() =>
            {
                // 원점 복귀 명령 실행 후 결과 반환 (성공 시 1, 실패 시 0) CommIndex
//                int i = INA_AZ_SET_ZHOME(CommIndex, SlaveId);
                int i = INA_AZ_SET_HOME(CommIndex, SlaveId);
                return INA_AZ_SET_CLEAR(CommIndex, SlaveId);
            });
        }

        // 상태 모니터링 (위치 및 알람)
        public async Task<(int position, int alarm)> ReadStatusAsync()
        {
            return await Task.Run(() =>
            {
                int pos = INA_AZ_GET_CURRENT_POSITION(CommIndex, SlaveId);
                int alarm = INA_AZ_GET_CURRENT_ALARM(CommIndex, SlaveId);
                return (pos, alarm);
            });
        }

        public async Task ResetAlarmAsync()
        {
            await Task.Run(() => INA_AZ_SET_ALARM_RESET(CommIndex, SlaveId));
        }

        public async Task<int> ClearAsync()
        {
            return await Task.Run(() =>
            {
                return INA_AZ_SET_CLEAR(CommIndex, SlaveId);
            });
        }

        // 특정 DataNo(0~15)의 SD 데이터를 읽어오는 메서드
        public async Task<(int OpMode, int Pos, int Vel, int Acc, int Dec)> ReadSdDataAsync(int dataNo)
        {
            int maxRetries = 2;             // 최대 재시도 횟수
            int delayBetweenRetriesMs = 50; // 재시도 간 대기 시간 (ms)

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                // 1. 외부 DLL 통신 (UI 멈춤 방지를 위해 Task.Run 내부에서 실행)
                var result = await Task.Run(() =>
                {
                    int mode = INA_AZ_GET_DATA_POSITION_MODE(CommIndex, SlaveId, dataNo);
                    int pos = INA_AZ_GET_DATA_POSITION(CommIndex, SlaveId, dataNo);
                    int vel = INA_AZ_GET_DATA_VELOCITY(CommIndex, SlaveId, dataNo);
                    int acc = INA_AZ_GET_DATA_ACC_RATE(CommIndex, SlaveId, dataNo);
                    int dec = INA_AZ_GET_DATA_DEC_RATE(CommIndex, SlaveId, dataNo);
                    int opt = INA_AZ_GET_DATA_OPERATING_CURRENT(CommIndex, SlaveId, dataNo);
                    int delay = INA_AZ_GET_DATA_DELAY_TIME(CommIndex, SlaveId, dataNo);
                    int link = INA_AZ_GET_DATA_LINK(CommIndex, SlaveId, dataNo);
                    int next = INA_AZ_GET_DATA_NEXT_DATA_NO(CommIndex, SlaveId, dataNo);

                    return new { mode, pos, vel, acc, dec, opt, delay, link, next };
                });

                // 2. 쓰레기 값(Range) 판별 로직
                bool isValid = true;

                // [주의] 아래 Range 값들은 일반적인 AZ 모터 스펙을 기준으로 작성되었습니다.
                // 실제 사용하시는 장비 세팅에 맞춰 범위를 타이트하게 조절해 주시면 더 확실하게 쓰레기 값을 걸러낼 수 있습니다.

                // 1) Operation Mode (운전 방식: 보통 0 ~ 20 내외)
                if (result.mode < 0 || result.mode > 20) isValid = false;

                // 2) Position (위치값: -2,147,483,648 ~ 2,147,483,647 이지만, 쓰레기값 방지를 위해 실사용 최대 범위로 제한 추천)
                if (result.pos < -1000000000 || result.pos > 1000000000) isValid = false;

                // 3) Velocity (이동 속도: 1000 ~ 10,000 Hz)
                if (result.vel < 1000 || result.vel > 10000) isValid = false;

                // 4) Accel / Decel (가감속: 100 ~ 10,000)
                if (result.acc < 100 || result.acc > 10000) isValid = false;
                if (result.dec < 100 || result.dec > 10000) isValid = false;

                // 5) Operating Current (운전 전류: 0 ~ 1000, 0.1% 단위)
//                if (result.opt < 0 || result.opt > 1000) isValid = false;

                // 6) Delay Time (대기 시간: 0 ~ 100,000 ms)
//                if (result.delay < 0 || result.delay > 100000) isValid = false;

                // 7) Link (연결 방식: 0=No Link, 1=Manual, 2=Auto 등 0~3)
//                if (result.link < 0 || result.link > 3) isValid = false;

                // 8) Next Data No (다음 데이터 번호: -1 ~ 255)
//                if (result.next < -1 || result.next > 255) isValid = false;

                // 3. 데이터가 모두 정상 범위인 경우 즉시 반환
                if (isValid)
                {
                    return (result.mode, result.pos, result.vel, result.acc, result.dec);
                }

                // 4. 비정상 값이면 약간 대기 후 다음 루프(재시도) 진행
                if (attempt < maxRetries)
                {
                    await Task.Delay(delayBetweenRetriesMs);
                }
            }

            // 5. 최대 재시도 횟수를 초과한 경우 처리
            // ★ 최대 재시도 횟수 초과 시 예외 발생 (ViewModel에서 Catch)
            throw new Exception($"SD 데이터 통신 실패 (No.{dataNo})");
        }

        public async Task<(int Pos, int Vel)> ReadSdDataPosAsync(int dataNo)
        {
            return await Task.Run(() =>
            {
                int pos = INA_AZ_GET_DATA_POSITION(CommIndex, SlaveId, dataNo);
                int vel = INA_AZ_GET_DATA_VELOCITY(CommIndex, SlaveId, dataNo);
                return (pos, vel);
            });
        }

        // ========================================================
        // ▼ MainTaskController 연동용 상태 확인 및 제어 메서드 ▼
        // ========================================================

        // 1. 알람 코드 읽기 (0이면 정상, 그 외는 알람 코드)
        public async Task<int> GetAlarmCodeAsync()
        {
            return await Task.Run(() => INA_AZ_GET_ALARM(CommIndex, SlaveId));
        }

        // 2. 모터 정지 명령
        public async Task StopMotorAsync()
        {
            await Task.Run(() => INA_AZ_SET_STOP(CommIndex, SlaveId));
        }

        // 3. 현재 지령 위치 읽기
        public async Task<int> GetCurrentPositionAsync()
        {
            return await Task.Run(() => INA_AZ_GET_CURRENT_POSITION(CommIndex, SlaveId));
        }
    }
}