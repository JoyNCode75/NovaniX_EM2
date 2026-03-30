using System.Threading.Tasks;
using NovaniX_EM2.Communication;

namespace NovaniX_EM2.Devices
{
    // DH Gripper 전용 동작 클래스 (RGI Series V3.1)
    public class RGIC100_Motion
    {
        private readonly IModbusConnection _connection;
        public byte SlaveId { get; set; } = 1;

        public RGIC100_Motion(IModbusConnection connection)
        {
            _connection = connection;
        }

        #region [ 1. Gripper Linear Control ]
        // 1. 전체 초기화 (그립 + 회전)
        public async Task InitializeAllAsync()
        {
            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0100, 1);
        }

        // 2. 그립만 초기화 (Clamping recalibration)
        public async Task InitializeClampAsync()
        {
            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0100, 4);
        }

        // 그리퍼 Linear 이동 (위치 0~1000, 속도 1~100, 힘 20~100)
        public async Task MoveGripperAsync(int positionPercent, int speedPercent, int forcePercent)
        {
            // UI의 0%를 Close(장비 1000), 100%를 Open(장비 0)으로 인식하도록 반전
            int invertedPos = 100 - positionPercent;
            ushort regPos = (ushort)(invertedPos * 10);
            ushort regSpeed = (ushort)(speedPercent);
            ushort regForce = (ushort)(forcePercent);

            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0101, regForce);
            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0104, regSpeed);
            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0103, regPos);
        }

        // ==========================================
        // ★ RGIC100_Motion.cs 내부에 추가할 코드
        // ==========================================
        // 클램프 상태(0x0201)를 읽어 반환하는 메서드
        // ==========================================
        // ★ RGIC100_Motion.cs 내부에 추가할 코드
        // ==========================================

        // 클램프 상태(0x0201)를 읽어 반환하는 메서드
        public async Task<int> ReadClampStatusAsync()
        {
            try
            {
                // 0x0201 번지 1개의 워드를 읽어옵니다. (단수형 함수 사용)
                int result = await _connection.ReadHoldingRegisterAsync(SlaveId, 0x0201);
                return result;
            }
            catch
            {
                // 통신 실패 시 -1 반환
                return -1;
            }
        }
        #endregion

        #region [ 2. Rotation Angle Control ]
        // 3. 로테이션만 초기화
        public async Task InitializeRotationAsync()
        {
            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0300, 1);
        }

        // 그리퍼 각도 회전(위치 0~360도, 속도 1~100, 토크 20~100)
        public async Task RotateGripperAsync(int angleDegree, int speedPercent, int torquePercent)
        {
            // RGI 매뉴얼: 각도는 도(Degree) 단위 그대로, 속도와 토크는 1~100% 그대로 전송
            ushort regAngle = (ushort)angleDegree;
            ushort regSpeed = (ushort)speedPercent;
            ushort regTorque = (ushort)torquePercent;

            // 0x0107(속도), 0x0108(토크), 0x0105(절대각도 위치)
            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0107, regSpeed);
            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0108, regTorque);
            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0105, regAngle);
        }

        // ==========================================
        // ★ RGIC100_Motion.cs 내부에 추가할 코드
        // ==========================================

        // 회전 상태(0x020B)를 읽어 반환하는 메서드
        public async Task<int> ReadRotationStatusAsync()
        {
            try
            {
                // 0x020B 번지 1개의 워드를 읽어옵니다.
                int result = await _connection.ReadHoldingRegisterAsync(SlaveId, 0x020B);
                return result;
            }
            catch
            {
                // 통신 실패 시 -1 반환
                return -1;
            }
        }
        #endregion

        public async Task StopAsync()
        {
            // 비상 정지 레지스터
            await _connection.WriteSingleRegisterAsync(SlaveId, 0x0502, 1);
        }
    }
}