using NovaniX_EM2.Models;
using NovaniX_EM2.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NovaniX_EM2.Communication
{
    // SeerAmrController 클래스 외부에 (동일한 네임스페이스 안) 추가합니다.

    public class SeerAmrController : INotifyPropertyChanged
    {
        // 글로벌 싱글톤 인스턴스
        private static SeerAmrController? _instance; // ? 추가
        public static SeerAmrController Instance => _instance ?? (_instance = new SeerAmrController());

        private TcpClient? _statusClient;     // ? 추가
        private NetworkStream? _statusStream; // ? 추가
        private TcpClient? _controlClient;    // ? 추가
        private NetworkStream? _controlStream;// ? 추가
        
        private ushort _sequenceNumber = 1;

        // 바인딩용 프로퍼티
        private bool _isStatusConnected;
        public bool IsStatusConnected
        {
            get => _isStatusConnected;
            set { _isStatusConnected = value; OnPropertyChanged(); }
        }

        private bool _isControlConnected;
        public bool IsControlConnected
        {
            get => _isControlConnected;
            set { _isControlConnected = value; OnPropertyChanged(); }
        }

        private double _batteryLevel;
        public double BatteryLevel
        {
            get => _batteryLevel;
            set { _batteryLevel = value; OnPropertyChanged(); }
        }

        private double _posX;
        public double PosX
        {
            get => _posX;
            set { _posX = value; OnPropertyChanged(); }
        }

        private double _posY;
        public double PosY
        {
            get => _posY;
            set { _posY = value; OnPropertyChanged(); }
        }

        private double _speedVx;
        public double SpeedVx
        {
            get => _speedVx;
            set { _speedVx = value; OnPropertyChanged(); }
        }

        private double _speedVy;
        public double SpeedVy
        {
            get => _speedVy;
            set { _speedVy = value; OnPropertyChanged(); }
        }

        private double _speedW;
        public double SpeedW
        {
            get => _speedW;
            set { _speedW = value; OnPropertyChanged(); }
        }

        private string _logMessage = "";
        public string LogMessage
        {
            get => _logMessage;
            set { _logMessage = value; OnPropertyChanged(); }
        }

//        private SeerAmrController() { }

        public async Task ConnectAsync(string ip)
        {
            try
            {
                if (!IsStatusConnected)
                {
                    _statusClient = new TcpClient();
                    await _statusClient.ConnectAsync(ip, 19204);
                    _statusStream = _statusClient.GetStream();
                    IsStatusConnected = true;
                    _ = ReceiveStatusLoopAsync();
                }

                if (!IsControlConnected)
                {
                    _controlClient = new TcpClient();
                    await _controlClient.ConnectAsync(ip, 19205);
                    _controlStream = _controlClient.GetStream();
                    IsControlConnected = true;
                }
                AddLog("AMR 컨트롤러 통신 연결 성공");
            }
            catch (Exception ex)
            {
                AddLog($"연결 실패: {ex.Message}");
                throw;
            }
        }

        public void Disconnect()
        {
            _statusStream?.Close();
            _statusClient?.Close();
            _controlStream?.Close();
            _controlClient?.Close();
            IsStatusConnected = false;
            IsControlConnected = false;
            AddLog("AMR 연결 해제됨");
        }

        // 제어 명령 전송 (2010: 속도, 2000: 정지 등)
        public async Task SendControlPacketAsync(ushort apiType, string jsonPayload)
        {
            if (!IsControlConnected || _controlStream == null) return;
            await SendPacketAsync(_controlStream, apiType, jsonPayload);
        }

        // 상태 요청 (API 1000)
        public async Task ReqStatusAsync()
        {
            if (!IsStatusConnected || _statusStream == null) return;
            await SendPacketAsync(_statusStream, 1000, "{}");
        }

        private async Task SendPacketAsync(NetworkStream stream, ushort apiType, string jsonPayload)
        {
            try
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);
                uint dataLength = (uint)jsonBytes.Length;

                byte[] header = new byte[16];
                header[0] = 0x5A; header[1] = 0x01;
                Buffer.BlockCopy(ToBigEndian(_sequenceNumber++), 0, header, 2, 2);
                Buffer.BlockCopy(ToBigEndian(dataLength), 0, header, 4, 4);
                Buffer.BlockCopy(ToBigEndian(apiType), 0, header, 8, 2);

                byte[] packet = new byte[16 + dataLength];
                Buffer.BlockCopy(header, 0, packet, 0, 16);
                Buffer.BlockCopy(jsonBytes, 0, packet, 16, (int)dataLength);

                await stream.WriteAsync(packet, 0, packet.Length);
            }
            catch (Exception ex) { AddLog($"전송 오류: {ex.Message}"); }
        }

        private async Task ReceiveStatusLoopAsync()
        {
            // 1. 혹시 모를 예외를 방지하기 위해 시작 전 방어 코드 추가
            if (_statusStream == null) return;

            byte[] headerBuffer = new byte[16];
            try
            {
                while (IsStatusConnected)
                {
                    // 2. 컴파일러에게 "절대 null이 아님"을 알리는 ! 연산자 사용
                    int bytesRead = await _statusStream!.ReadAsync(headerBuffer, 0, 16);
                    if (bytesRead < 16) break;
                    if (headerBuffer[0] != 0x5A) continue;

                    uint dataLength = ParseBigEndianUInt32(headerBuffer, 4);
                    ushort apiType = ParseBigEndianUInt16(headerBuffer, 8);

                    byte[] jsonBuffer = new byte[dataLength];
                    int totalRead = 0;
                    while (totalRead < dataLength)
                    {
                        // 3. 여기도 ! 연산자 사용
                        int read = await _statusStream!.ReadAsync(jsonBuffer, totalRead, (int)dataLength - totalRead);
                        if (read == 0) throw new Exception("Socket Closed");
                        totalRead += read;
                    }

                    string jsonResponse = Encoding.UTF8.GetString(jsonBuffer);

                    if (apiType == 1000) // 공통 상태 정보 파싱
                    {
                        using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                        {
                            JsonElement root = doc.RootElement;
                            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                            {
                                if (root.TryGetProperty("battery_level", out JsonElement batEl))
                                    BatteryLevel = batEl.GetDouble() * 100;

                                if (root.TryGetProperty("x", out JsonElement xEl)) PosX = xEl.GetDouble();
                                if (root.TryGetProperty("y", out JsonElement yEl)) PosY = yEl.GetDouble();
                                if (root.TryGetProperty("angle", out JsonElement angleEl)) PosAngle = angleEl.GetDouble() * (180.0 / Math.PI); // Radian을 Degree로 변환 시 적용

                                // [추가된 속도 모니터링 파싱]
                                if (root.TryGetProperty("vx", out JsonElement vxEl)) SpeedVx = vxEl.GetDouble();
                                if (root.TryGetProperty("vy", out JsonElement vyEl)) SpeedVy = vyEl.GetDouble();
                                if (root.TryGetProperty("w", out JsonElement wEl)) SpeedW = wEl.GetDouble();

                                // DIO 추가
                                if (root.TryGetProperty("DI", out JsonElement diEl) && diEl.ValueKind == JsonValueKind.Array)
                                {
                                    for (int i = 0; i < diEl.GetArrayLength() && i < 10; i++)
                                        AmrInputs[i].IsOn = diEl[i].GetBoolean();
                                }

                                if (root.TryGetProperty("DO", out JsonElement doEl) && doEl.ValueKind == JsonValueKind.Array)
                                {
                                    for (int i = 0; i < doEl.GetArrayLength() && i < 10; i++)
                                        AmrOutputs[i].IsOn = doEl[i].GetBoolean();
                                }
                            });
                        }
                    }
                }
            }
            catch { IsStatusConnected = false; }
        }

        // --- 새로 추가되는 AMR 상태 모니터링 프로퍼티 ---

        private string _motionStateText = "정지 상태";
        public string MotionStateText
        {
            get => _motionStateText;
            set { _motionStateText = value; OnPropertyChanged(); }
        }

        private double _posAngle;
        public double PosAngle // AMR의 현재 회전 각도 (Degree)
        {
            get => _posAngle;
            set { _posAngle = value; OnPropertyChanged(); }
        }

        // Map UI에 보여주기 위한 변환된 화면(Pixel) 좌표
        // 실제 X,Y (m)에 배율(Scale)을 곱하고 오프셋을 더해 구해야 합니다.
        private double _posX_UI;
        public double PosX_UI
        {
            get => _posX_UI;
            set { _posX_UI = value; OnPropertyChanged(); }
        }

        private double _posY_UI;
        public double PosY_UI
        {
            get => _posY_UI;
            set { _posY_UI = value; OnPropertyChanged(); }
        }

        // 클릭하여 지정된 타겟 포인트 UI 좌표
        private double _targetPosX_UI;
        public double TargetPosX_UI
        {
            get => _targetPosX_UI;
            set { _targetPosX_UI = value; OnPropertyChanged(); }
        }

        private double _targetPosY_UI;
        public double TargetPosY_UI
        {
            get => _targetPosY_UI;
            set { _targetPosY_UI = value; OnPropertyChanged(); }
        }

        // ---------------------------------------------

        // 수정 후 코드 (마찬가지로 ? 추가)
        public void AddLog(string msg)
        {
            // 기존 코드: System.Windows.Application.Current.Dispatcher.Invoke(() =>
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                LogMessage = $"[{DateTime.Now:HH:mm:ss}] {msg}\n" + LogMessage;
            });
        }

        // ------------------------------------------------------------------
        // [SeerAmrController 클래스 내부에 아래 프로퍼티와 메서드를 추가합니다]
        // ------------------------------------------------------------------

        // 1. 스케줄링 및 경로 UI 바인딩 변수
        private ObservableCollection<AmrWaypoint> _waypointQueue = new ObservableCollection<AmrWaypoint>();
        public ObservableCollection<AmrWaypoint> WaypointQueue
        {
            get => _waypointQueue;
            set { _waypointQueue = value; OnPropertyChanged(); }
        }

        // LiDAR Point Cloud UI 바인딩 (UI 성능을 위해 간단한 Point 배열 사용)
        private PointCollection _lidarPoints = new PointCollection();
        public PointCollection LidarPoints
        {
            get => _lidarPoints;
            set { _lidarPoints = value; OnPropertyChanged(); }
        }

        private bool _isNavigating = false;
        private int _currentWaypointIndex = -1;

        // 2. 다중 포인트 스케줄 추가 함수
        public void AddWaypointToSchedule(double realX, double realY, double uiX, double uiY)
        {
            char pointName = (char)('A' + WaypointQueue.Count);
            WaypointQueue.Add(new AmrWaypoint
            {
                Name = pointName.ToString(),
                TargetX = realX,
                TargetY = realY,
                UiX = uiX,
                UiY = uiY,
                Status = "예정"
            });
        }

        // 3. 스케줄링 주행 시작 (API 3051)
        public async void StartNavigationSchedule()
        {
            if (WaypointQueue.Count == 0 || _isNavigating) return;

            _isNavigating = true;
            _currentWaypointIndex = 0;
            await GoToNextWaypoint();
        }

        private async System.Threading.Tasks.Task GoToNextWaypoint()
        {
            if (_currentWaypointIndex >= WaypointQueue.Count)
            {
                _isNavigating = false;
                AddLog("[스케줄링 종료] 모든 경로 이동 완료");
                return;
            }

            var target = WaypointQueue[_currentWaypointIndex];
            target.Status = "이동중";

            // Seer API 3051: 특정 좌표로 자율 주행 (장애물 회피 기본 포함)
            var payload = new
            {
                x = target.TargetX,
                y = target.TargetY,
                angle = 0.0 // 필요에 따라 목표 각도 설정
            };
            string json = JsonSerializer.Serialize(payload);

            await SendControlPacketAsync(3051, json);
            AddLog($"[네비게이션] {target.Name} 지점(X:{target.TargetX:F2}, Y:{target.TargetY:F2})으로 이동 명령 전송");
        }

        // 4. 상태 파싱 루프(ReceiveStatusLoopAsync) 내부에 추가할 체크 로직
        // API 1000 또는 1020 수신 시 도착 여부 판단
        public async void CheckNavigationStatus(int taskStatus)
        {
            // taskStatus == 4 (Completed/도착) 이라고 가정 (Seer 프로토콜 버전에 따라 다를 수 있음)
            if (_isNavigating && taskStatus == 4)
            {
                var target = WaypointQueue[_currentWaypointIndex];
                target.Status = "도착";
                AddLog($"[네비게이션] {target.Name} 지점 도착 완료");

                _currentWaypointIndex++;

                // 잠시 대기 후 다음 지점으로 출발
                await System.Threading.Tasks.Task.Delay(2000);
                await GoToNextWaypoint();
            }
        }

        // 5. LiDAR 데이터(API 1004 등) 파싱 시 호출할 함수 (가상 예시)
        public void UpdateLidarData(double[] distances, double[] angles)
        {
            // Application 모호성 에러 방지를 위해 System.Windows 명시
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // PointCollection은 System.Windows.Media 소속이므로 Using이 잘 되어 있다면 문제없음
                System.Windows.Media.PointCollection pts = new System.Windows.Media.PointCollection();

                for (int i = 0; i < distances.Length; i++)
                {
                    // 실제 거리와 각도를 UI 화면 Pixel(UiX, UiY) 좌표계로 변환하는 스케일링 로직 필요
                    double px = PosX_UI + (distances[i] * Math.Cos(angles[i]) * 50); // 50은 배율
                    double py = PosY_UI + (distances[i] * Math.Sin(angles[i]) * 50);

                    // Point 모호성 에러(System.Drawing.Point 충돌) 방지를 위해 System.Windows.Point 명시
                    pts.Add(new System.Windows.Point(px, py));
                }
                LidarPoints = pts;
            });
        }

        // --- 새로 추가되는 AMR I/O 프로퍼티 ---
        private const string IO_CONFIG_FILE_PATH = "Config/AmrIoSettings.json";

        private ObservableCollection<AmrIoNode> _amrInputs = new ObservableCollection<AmrIoNode>();
        public ObservableCollection<AmrIoNode> AmrInputs
        {
            get => _amrInputs;
            set { _amrInputs = value; OnPropertyChanged(); }
        }

        private ObservableCollection<AmrIoNode> _amrOutputs = new ObservableCollection<AmrIoNode>();
        public ObservableCollection<AmrIoNode> AmrOutputs
        {
            get => _amrOutputs;
            set { _amrOutputs = value; OnPropertyChanged(); }
        }

        private SeerAmrController()
        {
            LoadIoConfiguration();
        }

        // I/O 설정 불러오기 및 초기화 로직
        public void LoadIoConfiguration()
        {
            AmrIoConfig? config = JsonHelper.Load<AmrIoConfig>(IO_CONFIG_FILE_PATH);

            if (config == null)
            {
                // 1. 파일이 없으면 기본 설정 생성
                config = new AmrIoConfig();
                for (int i = 0; i < config.TotalInputCount; i++)
                    config.Inputs.Add(new AmrIoNode { Index = i, Name = $"Input {i}", IsUsed = true, Address = $"DI_{i}" });

                for (int i = 0; i < config.TotalOutputCount; i++)
                    config.Outputs.Add(new AmrIoNode { Index = i, Name = $"Output {i}", IsUsed = true, Address = $"DO_{i}" });

                // 2. 기본값 JSON으로 저장
                JsonHelper.Save(IO_CONFIG_FILE_PATH, config);
            }

            // 3. UI 바인딩용 컬렉션에 적용
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                AmrInputs.Clear();
                foreach (var item in config.Inputs) AmrInputs.Add(item);

                AmrOutputs.Clear();
                foreach (var item in config.Outputs) AmrOutputs.Add(item);
            });
        }

        // (참고) 설정 화면에서 내용을 바꾸고 저장 버튼을 눌렀을 때 호출할 메서드
        public void SaveIoConfiguration()
        {
            var config = new AmrIoConfig
            {
                TotalInputCount = AmrInputs.Count,
                TotalOutputCount = AmrOutputs.Count,
                Inputs = new List<AmrIoNode>(AmrInputs),
                Outputs = new List<AmrIoNode>(AmrOutputs)
            };
            JsonHelper.Save(IO_CONFIG_FILE_PATH, config);
        }
        #region Big-Endian Helpers
        private byte[] ToBigEndian(ushort value) { var b = BitConverter.GetBytes(value); if (BitConverter.IsLittleEndian) Array.Reverse(b); return b; }
        private byte[] ToBigEndian(uint value) { var b = BitConverter.GetBytes(value); if (BitConverter.IsLittleEndian) Array.Reverse(b); return b; }
        private uint ParseBigEndianUInt32(byte[] buf, int idx) { byte[] b = new byte[4]; Buffer.BlockCopy(buf, idx, b, 0, 4); if (BitConverter.IsLittleEndian) Array.Reverse(b); return BitConverter.ToUInt32(b, 0); }
        private ushort ParseBigEndianUInt16(byte[] buf, int idx) { byte[] b = new byte[2]; Buffer.BlockCopy(buf, idx, b, 0, 2); if (BitConverter.IsLittleEndian) Array.Reverse(b); return BitConverter.ToUInt16(b, 0); }
        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}