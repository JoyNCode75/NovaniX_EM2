using NovaniX_EM2.Communication;
using NovaniX_EM2.Devices;
using NovaniX_EM2.Helpers;
using NovaniX_EM2.Models; // ★ Models 네임스페이스 추가
using NovaniX_EM2.Controllers; // MainTaskController가 위치한 네임스페이스
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NovaniX_EM2.ViewModels
{
    // UI와 바인딩될 핀 상태 모델
    #region [ Fastech I/O Variables ]
    public class IoPin : INotifyPropertyChanged
    {
        private bool _isOn;
        private bool _isUse;
        private string _name = string.Empty;

        public int PinIndex { get; set; }
        public string Number { get; set; } = string.Empty; // 예: X000, Y000
        public bool IsInput { get; set; } // 입력 여부 구분

        public bool IsUse
        {
            get => _isUse;
            set
            {
                _isUse = value;
                OnPropertyChanged(nameof(IsUse));
                if (!_isUse)
                {
                    IsOn = false; // 미사용 시 램프 회색 처리
                    Name = $"{(IsInput ? "Input" : "Output")}_Spare {Number}"; // 기본 이름으로 되돌림
                }
            }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public bool IsOn
        {
            get => _isOn;
            set
            {
                // 사용 중일 때만 ON으로 바뀔 수 있도록 제어
                if (_isOn != value && (!value || IsUse))
                {
                    _isOn = value;
                    OnPropertyChanged(nameof(IsOn));
                }
            }
        }

        public ICommand ToggleCommand { get; set; } = null!;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion

    // ★ DataGrid에 표시할 파티클 결과 데이터 클래스
    public class ParticleDataModel
    {
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string ElapsedTime { get; set; } = string.Empty;
        public string Part05_Sum { get; set; } = string.Empty;
        public string Part50_Sum { get; set; } = string.Empty;
    }

    // ★ DataGrid에 표시할 에어샘플러 결과 데이터 클래스
    public class AirSamplerDataModel
    {
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string ElapseTime { get; set; } = string.Empty;
        public string Volume { get; set; } = string.Empty;
        public string FlowRate { get; set; } = string.Empty;
        public string IntervalTime { get; set; } = string.Empty;
    }


    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged; // 여긴 ?를 붙여줍니다.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // 리스트 객체 (생성자 또는 필드에서 초기화되어야 함)
        // 파티클 데이터 리스트 (기존에 있다면 형식을 맞춰서 사용)
        public ObservableCollection<ParticleDataModel> ParticleCounterDataList { get; set; } = new ObservableCollection<ParticleDataModel>();
        public ObservableCollection<AirSamplerDataModel> AirSamplerDataList { get; set; } = new ObservableCollection<AirSamplerDataModel>();

        // UI에서 바인딩할 Task Controller
        public MainTaskController TaskController { get; } = new MainTaskController();

        private int _baudRate = 115200;
        public int BaudRate { get => _baudRate; set { _baudRate = value; OnPropertyChanged(); } }

        private System.IO.Ports.Parity _parity = System.IO.Ports.Parity.Even;
        public System.IO.Ports.Parity Parity { get => _parity; set { _parity = value; OnPropertyChanged(); } }

        private System.IO.Ports.StopBits _stopBits = System.IO.Ports.StopBits.One;
        public System.IO.Ports.StopBits StopBits { get => _stopBits; set { _stopBits = value; OnPropertyChanged(); } }

        // --- LiveCharts 속성 (CS8618 워닝 해결) ---
        // --- LiveCharts 속성 ---
        public SeriesCollection ParticleSeries { get; set; } = new SeriesCollection
        {
            new LineSeries
            {
                Title = "0.5㎛",
                Values = new ChartValues<double>(),
                ScalesYAt = 0  // 0번째(왼쪽) Y축을 기준으로 매핑
            },
            new LineSeries
            {
                Title = "5.0㎛",
                Values = new ChartValues<double>(),
                ScalesYAt = 1  // 1번째(오른쪽) Y축을 기준으로 매핑
            }
        };
        public ObservableCollection<string> ParticleLabels { get; set; } = new ObservableCollection<string>();

        public SeriesCollection AirSamplerSeries { get; set; } = new SeriesCollection
        {
            new LineSeries { Title = "Interval Time(초)", Values = new ChartValues<double>() }
        };
        public ObservableCollection<string> AirSamplerLabels { get; set; } = new ObservableCollection<string>();

        // 에어샘플러 raw Interval 초(Seconds) 저장을 위한 임시 변수
        private double _currentAirSamplerIntervalSeconds = 0;

        #region [ 1. AZ Motor Control ]
        // ★ 오리엔탈 모터 디바이스 추가
        public ObservableCollection<AzAxisViewModel> MotionAxes { get; set; } = new ObservableCollection<AzAxisViewModel>();
        // AZ Polling Timer
        private CancellationTokenSource? _azPollingCts;

        public ICommand ConnectCommand { get; private set; } = null!;
        // --- Commands ---
        // 필드에는 ?를 붙이고, Command에는 = null!; 을 붙이는 것이 MVVM에서 깔끔합니다.
        public ICommand MoveCommand { get; } = null!;
        public ICommand StopCommand { get; } = null!;
        public ICommand RefreshPortsCommand { get; } = null!;
        public ICommand HomeAllCommand { get; private set; } = null!;
        // ★ 추가할 부분: 글로벌 알람 리셋 커맨드
        public ICommand ResetAlarmCommand { get; } = null!;

        // ★ 새로 추가된 Command
        public ICommand SaveParametersCommand { get; } = null!;
        // ▼ 추가된 코드: 전체 파라미터 불러오기 커맨드 ▼
        public ICommand LoadParametersCommand { get; } = null!;


        // --- AZ 모터 통신 연결 및 모니터링 (기존 코드 변형) ---
        // 통신 포트 번호 변수 (사용할 COM 포트 번호)
        // 클래스 상단 전역 변수
        private int _azCommIndex = -1; // -1: 연결안됨
        private int _azCommPortNo = 0;

        private bool _isAllInitialized;
        public bool IsAllInitialized
        {
            get => _isAllInitialized;
            set { _isAllInitialized = value; OnPropertyChanged(); }
        }

        private bool _isAzCommBusy;
        public bool IsAzCommBusy
        {
            get => _isAzCommBusy;
            set { _isAzCommBusy = value; OnPropertyChanged(); }
        }

        // [통신 연결 로직 (Connect 버튼 클릭 시 등)]
        // MainViewModel.cs 내부 통신 연결 부분
        public async Task ConnectAzMotorAsync()
        {
            // LoadParameters()에서 이미 세팅된 SelectedPort(예: "COM3")를 그대로 사용합니다.
            int portNo = 0;
            if (string.IsNullOrEmpty(SelectedPort)) // 바인딩된 포트 프로퍼티명 확인
            {
                MessageBox.Show("포트를 선택해 주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // "COM3" 같은 문자열에서 숫자(3)만 추출
//            int portNumber = int.Parse(SelectedPort.Replace("COM", ""));
            if (!string.IsNullOrEmpty(SelectedPort) && SelectedPort.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(SelectedPort.Substring(3), out portNo);
            }

            _azCommPortNo = portNo;

            if (portNo > 0)
            {
                int nBaudRate = BaudRate > 0 ? BaudRate : 115200; // JSON에서 읽은 BaudRate 사용
                int nDataBit = 8;
                int nParityBit = 2;
                int nStopBit = 0;

                int result = await Task.Run(() =>
                {
                    return OrientalAzMotorDevice.INA_AZ_INITIALIZE(out _azCommIndex, portNo, nBaudRate, nDataBit, nParityBit, nStopBit);
                });

                if (result == 1) // 성공
                {
                    IsConnected = true;

                    // ★ 2. 중요: 반환받은 라이브러리 통신 인덱스를 모든 축 모터 객체에 할당
                    foreach (var axis in MotionAxes)
                    {
                        axis.SetCommIndex(_azCommIndex);
                    }

                    MessageBox.Show("AZ 모터 통신 연결 성공!", "알림", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 필요 시 상태 갱신 타이머 시작
                    // StartStatusMonitor();
                }
                else
                {
                    IsConnected = false;
                    MessageBox.Show($"모터 초기화 실패 (Port: {portNo})", "연결 에러", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                IsConnected = (result == 1 && _azCommIndex >= 0);

                if (IsConnected)
                {
                    // 통신 핸들 획득 후 각 축에 반영하기 위해 축 초기화 및 모니터링 시작
                    InitializeMotionAxes();

                    // 만약 InitializeMotionAxes가 1~8 기본 국번으로 세팅해버리므로, 
                    // 연결 직후 파라미터를 다시 한번 읽어 국번 매칭 및 위치값을 씌워줍니다.
                    LoadParameters();

                    StartAzBackgroundTask();
                }
                else
                {
                    MessageBox.Show($"AZ 모터 통신 포트({SelectedPort}) 열기에 실패했습니다.\n에러코드: {result}",
                                    "연결 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("유효한 COM 포트 정보를 찾을 수 없습니다.", "설정 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // 연결 및 해제 토글 로직
        private async Task ToggleConnectionAsync()
        {
            if (IsConnected)
            {
                // ==========================================
                // [연결 해제 로직 (Disconnect)]
                // ==========================================
                if (_azCommIndex != -1)
                {
                    // 💡 수정된 부분: UI가 멈추지 않도록 비동기 백그라운드 작업으로 해제
                    int currentIndex = _azCommIndex; // Task 안에서 안전하게 사용하기 위해 로컬 변수에 복사
                    await Task.Run(() => OrientalAzMotorDevice.INA_AZ_UNINITIALIZE(currentIndex));

                    _azCommIndex = -1;
                }

                IsConnected = false;

                if (MotionAxes != null)
                {
                    foreach (var axis in MotionAxes)
                    {
                        axis.IsInitialized = false;
                        axis.ResetSdData(); // ★ 해제 시 SD 데이터 0으로 초기화
                        axis.SetCommIndex(-1);
                    }
                }
            }
            else
            {
                // ==========================================
                // [연결 로직 (Connect)]
                // ==========================================
                if (string.IsNullOrEmpty(SelectedPort))
                {
                    MessageBox.Show("포트를 선택해 주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(SelectedPort.Replace("COM", "").Trim(), out int portNumber))
                {
                    int tempCommIndex = -1; // Task.Run 내부에서 out 파라미터를 받기 위한 임시 변수
                    int nBaudRate = BaudRate > 0 ? BaudRate : 115200; // JSON에서 읽은 BaudRate 사용
                    int nDataBit = 8;
                    int nParityBit = 2;
                    int nStopBit = 0;

                    // 💡 수정된 부분: 모터 연결 시 발생할 수 있는 딜레이 동안 UI가 멈추지 않도록 Task.Run 적용
                    int initResult = await Task.Run(() =>
                    {
                        return OrientalAzMotorDevice.INA_AZ_INITIALIZE(out tempCommIndex, portNumber, nBaudRate, nDataBit, nParityBit, nStopBit);
                    });

                    if (initResult == 1) // 1: 성공
                    {
                        _azCommIndex = tempCommIndex;
                        IsConnected = true;

                        if (MotionAxes != null)
                        {
                            foreach (var axis in MotionAxes)
                            {
                                axis.SetCommIndex(_azCommIndex);
                                axis.ResetSdData(); // ★ 새로운 접속 시 깔끔하게 0으로 초기화한 상태에서 시작
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"모터 통신 연결 실패 (Port: COM{portNumber})\n모터 전원 및 케이블 연결 상태를 확인하세요.",
                                        "연결 에러", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("올바른 COM 포트를 선택해 주세요.", "입력 에러", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DisconnectAzMotor() // (또는 DisconnectCommand 연결 메서드)
        {
            if (IsConnected)
            {
                // ★ 해제할 때도 반드시 포트번호가 아닌 _azCommIndex 사용
                OrientalAzMotorDevice.INA_AZ_UNINITIALIZE(_azCommIndex);

                IsConnected = false;
                _azCommIndex = -1; // 초기화
            }
        }

        private void InitializeMotionAxes()
        {
            MotionAxes.Clear();
            // ★ 이전 초기화 시 남아있던 데이터 지우기 (안전장치)
            TaskController.AzMotors.Clear();
            TaskController.AxisSdData.Clear();

            for (byte slaveId = 1; slaveId <= 8; slaveId++)
            {
                // ★ Modbus 연결 대신 Port 번호 전달
                var motorDevice = new OrientalAzMotorDevice(_azCommPortNo, slaveId);

                var axisVm = new AzAxisViewModel(motorDevice, (isBusy) => IsAzCommBusy = isBusy)
                {
                    AxisName = $"Axis {slaveId}",
                    SoftLimitMin = -100000000,
                    SoftLimitMax = 100000000
                };

                // 기존 뷰모델에 정의해둔 SaveAxisCommand 등 할당
                axisVm.SaveAxisCommand = new RelayCommand(_ => SaveSingleAxisParameters(axisVm));

                // ====================================================================
                // ★ 2. MainTaskController에 해당 축의 모터 객체 및 SD Data 리스트 참조 주입
                // (axisVm이 내부적으로 0~15번의 TargetPositionItem을 생성/관리하므로 그 참조를 넘김)
                // ====================================================================
                TaskController.AzMotors[slaveId] = motorDevice;

                // AxisSdData 딕셔너리에 TargetPositions 리스트(ObservableCollection)의 요소를
                // List 형태로 복사/참조하여 저장합니다. 
                // (.ToList()를 써도 되지만, 객체 참조형이라 속성값은 동일하게 공유됨)
                TaskController.AxisSdData[slaveId] = axisVm.TargetPositions.ToList();
                // ====================================================================

                MotionAxes.Add(axisVm);
            }
        }

        // 모터 폴링 시작 메서드 (MainViewModel 내부의 통신 Connect 성공 시 호출되도록 구성)
        // 백그라운드 모니터링 및 자동 정지 로직
        private void StartAzBackgroundTask()
        {
            _azPollingCts = new CancellationTokenSource();
            var token = _azPollingCts.Token;

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // ★ 1. 다른 명령(축 이동, JOG 등)이 실행 중이면 폴링 일시 정지
                        if (IsAzCommBusy)
                        {
                            await Task.Delay(50, token);
                            continue;
                        }

                        bool anyAlarm = false;

                        foreach (var axis in MotionAxes)
                        {
                            // 축 순회 도중 명령이 들어오면 즉시 폴링 중단
                            if (IsAzCommBusy) break;

                            // ★ 1. 여기서 위치값과 알람 상태가 모두 갱신됨 (UI 즉시 반영)
                            await axis.UpdateStatusAsync();

                            if (axis.IsAlarmed)
                            {
                                anyAlarm = true;
                            }
                        }
                        // 폴링이 강제 중단되었다면 아래 알람 로직 스킵
                        if (IsAzCommBusy) continue;

                        // 2. 전체 알람 상태 연동 및 긴급 정지 처리
                        if (anyAlarm && !IsAlarmed)
                        {
                            IsAlarmed = true;
                            foreach (var axis in MotionAxes)
                            {
                                if (axis.StopCommand.CanExecute(null))
                                {
                                    axis.StopCommand.Execute(null);
                                }
                            }
                        }
                        else if (!anyAlarm && IsAlarmed)
                        {
                            IsAlarmed = false;
                        }

                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            if (MotionAxes.Count > 0)
                                CurrentPositionPct = MotionAxes[0].CurrentPosition / 10.0;
                        });
                    }
                    catch { /* 통신 오류 무시하고 재시도 */ }

                    await Task.Delay(100, token);
                }
            }, token);
        }

        public ObservableCollection<string> AvailablePorts { get; } = new ObservableCollection<string>();

        private string? _selectedPort;
        public string? SelectedPort
        {
            get => _selectedPort;
            set { _selectedPort = value; OnPropertyChanged(); }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }

        private void RefreshComPorts()
        {
            AvailablePorts.Clear();
            foreach (string port in SerialPort.GetPortNames())
            {
                AvailablePorts.Add(port);
            }
            if (AvailablePorts.Count > 0) SelectedPort = AvailablePorts[0];
        }

        // --- ★ 파라미터 관리 로직 추가 ---
        // --- ★ 파라미터 관리 로직 통합 (실행파일 기준 ../Parameter/MOTION.json, POSITION.json) ---
        private void LoadParameters()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string paramDir = Path.GetFullPath(Path.Combine(baseDir, @"..\Parameter"));
            string motionFilePath = Path.Combine(paramDir, "MOTION.json");
            string posFilePath = Path.Combine(paramDir, "POSITION.json");

            // 1. MOTION.json 로드
            // (※ 프로젝트에 정의된 실제 클래스명에 맞게 MotionSystemParameter 또는 MotionParameter 사용)
            var sysParam = JsonHelper.Load<MotionSystemParameter>(motionFilePath);
            List<MotionParameter> loadedMotion = null!;

            if (sysParam != null && sysParam.Axes != null && sysParam.Axes.Count > 0)
            {
                // ★ 파일에 저장된 통신 설정값 가져오기 (UI 콤보박스 및 변수에 바인딩 됨)
                if (!string.IsNullOrEmpty(sysParam.ComPort)) SelectedPort = sysParam.ComPort;
                if (sysParam.BaudRate > 0) BaudRate = sysParam.BaudRate;
                Parity = sysParam.Parity;
                StopBits = sysParam.StopBits;

                loadedMotion = sysParam.Axes;
            }

            if (loadedMotion != null && loadedMotion.Count > 0)
            {
                // 축 순서대로 데이터 및 Slave ID 매칭
                for (int i = 0; i < Math.Min(MotionAxes.Count, loadedMotion.Count); i++)
                {
                    var p = loadedMotion[i];
                    var axis = MotionAxes[i];

                    // 1) 뷰모델의 MotorId 속성 업데이트
                    axis.MotorId = p.MotorNumber;

                    // ★ 2) 실제 AZ 모터 통신 객체(DLL 통신)의 Slave ID 동기화
                    axis.SetSlaveId((byte)p.MotorNumber);

                    // 3) 기타 파라미터 업데이트
                    axis.CustomAxisName = string.IsNullOrWhiteSpace(p.MotorName) ? axis.AxisName : p.MotorName;
                    axis.MoveSpeed = p.MoveSpeed;
                    axis.MoveAccelDecel = p.MoveAccelDecel;
                    axis.JogSpeed = p.JogSpeed;
                    axis.JogAccelDecel = p.JogAccelDecel;

                    // 4) SW Limit 업데이트 (AzAxisViewModel 속성에 맞게)
                    axis.SoftLimitMin = p.NegativeLimit;
                    axis.SoftLimitMax = p.PositiveLimit;
                }
            }
            else
            {
                foreach (var axis in MotionAxes) axis.CustomAxisName = axis.AxisName;
            }

            // 2. POSITION.json 로드 (기존 로직 유지)
            var loadedPos = JsonHelper.Load<List<PositionParameter>>(posFilePath);
            if (loadedPos != null && loadedPos.Count > 0)
            {
                foreach (var axis in MotionAxes)
                {
                    var p = loadedPos.FirstOrDefault(x => x.MotorNumber == axis.MotorId);
                    if (p != null && p.Positions != null)
                    {
                        for (int i = 0; i < Math.Min(axis.TargetPositions.Count, p.Positions.Count); i++)
                        {
                            axis.TargetPositions[i].PositionNumber = p.Positions[i].PositionNumber > 0 ? p.Positions[i].PositionNumber : (i + 1);
                            axis.TargetPositions[i].IsUsed = p.Positions[i].IsUsed;
                            axis.TargetPositions[i].Name = p.Positions[i].Name;
                            axis.TargetPositions[i].Value = p.Positions[i].Value;
                        }
                    }
                }
            }
        }

        private void SaveParameters(bool showPopup = true)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string paramDir = Path.GetFullPath(Path.Combine(baseDir, @"..\Parameter"));
            if (!Directory.Exists(paramDir)) Directory.CreateDirectory(paramDir);

            string motionFilePath = Path.Combine(paramDir, "MOTION.json");
            string posFilePath = Path.Combine(paramDir, "POSITION.json");

            // ★ 래퍼 클래스를 통해 통신 설정과 함께 저장
            var sysParam = new MotionSystemParameter
            {
                ComPort = SelectedPort ?? "",
                BaudRate = this.BaudRate,
                Parity = this.Parity,
                StopBits = this.StopBits,
                Axes = new List<MotionParameter>()
            };

            var posParams = new List<PositionParameter>();

            foreach (var axis in MotionAxes)
            {
                string nameToSave = string.IsNullOrWhiteSpace(axis.CustomAxisName) ? axis.AxisName : axis.CustomAxisName;

                sysParam.Axes.Add(new MotionParameter
                {
                    MotorNumber = axis.MotorId,
                    MotorName = nameToSave,
                    MoveSpeed = axis.MoveSpeed,
                    MoveAccelDecel = axis.MoveAccelDecel,
                    JogSpeed = axis.JogSpeed,
                    JogAccelDecel = axis.JogAccelDecel,
                    NegativeLimit = axis.NegativeLimit,
                    PositiveLimit = axis.PositiveLimit
                });

                var positions = axis.TargetPositions.Select(tp => new PositionPoint
                {
                    PositionNumber = tp.PositionNumber,
                    Name = tp.Name,
                    IsUsed = tp.IsUsed,
                    Value = tp.Value
                }).ToList();

                posParams.Add(new PositionParameter { MotorNumber = axis.MotorId, MotorName = nameToSave, Positions = positions });
            }

            JsonHelper.Save(motionFilePath, sysParam);
            JsonHelper.Save(posFilePath, posParams);

            if (showPopup) MessageBox.Show("전체 파라미터 및 포지션 데이터가 성공적으로 저장되었습니다.", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ▼ 추가된 코드: 선택한 단일 축의 파라미터 및 포지션만 기존 JSON 파일에 덮어쓰기 저장 ▼
        private void SaveSingleAxisParameters(AzAxisViewModel axis)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string paramDir = Path.GetFullPath(Path.Combine(baseDir, @"..\Parameter"));
            if (!Directory.Exists(paramDir)) Directory.CreateDirectory(paramDir);

            string motionFilePath = Path.Combine(paramDir, "MOTION.json");
            string posFilePath = Path.Combine(paramDir, "POSITION.json");
            string nameToSave = string.IsNullOrWhiteSpace(axis.CustomAxisName) ? axis.AxisName : axis.CustomAxisName;

            // ★ 기존 설정 유지한 채 축만 갱신
            var sysParam = JsonHelper.Load<MotionSystemParameter>(motionFilePath) ?? new MotionSystemParameter
            {
                ComPort = SelectedPort ?? "",
                BaudRate = this.BaudRate,
                Parity = this.Parity,
                StopBits = this.StopBits
            };

            var existingMotion = sysParam.Axes.FirstOrDefault(m => m.MotorNumber == axis.MotorId);
            if (existingMotion != null)
            {
                existingMotion.MotorName = nameToSave;
                existingMotion.MoveSpeed = axis.MoveSpeed;
                existingMotion.MoveAccelDecel = axis.MoveAccelDecel;
                existingMotion.JogSpeed = axis.JogSpeed;
                existingMotion.JogAccelDecel = axis.JogAccelDecel;
                existingMotion.NegativeLimit = axis.NegativeLimit;
                existingMotion.PositiveLimit = axis.PositiveLimit;
            }
            else
            {
                sysParam.Axes.Add(new MotionParameter
                {
                    MotorNumber = axis.MotorId,
                    MotorName = nameToSave,
                    MoveSpeed = axis.MoveSpeed,
                    MoveAccelDecel = axis.MoveAccelDecel,
                    JogSpeed = axis.JogSpeed,
                    JogAccelDecel = axis.JogAccelDecel,
                    NegativeLimit = axis.NegativeLimit,
                    PositiveLimit = axis.PositiveLimit
                });
            }
            JsonHelper.Save(motionFilePath, sysParam);

            // POSITION.json 갱신 생략 (기존 코드와 동일하게 처리해 주시면 됩니다.)
            MessageBox.Show($"[{nameToSave}] 축 데이터가 개별 저장되었습니다.", "개별 저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region [ 2. Gripper Control ]
        private readonly IModbusConnection _gripperConnection = null!;
        private readonly ModbusDeviceState _deviceState = null!;
        private readonly RGIC100_Motion _motion = null!;
        private CancellationTokenSource? _gripperPollingCts;
        private int _gripperBaudRate = 115200;
        private Parity _gripperParity = Parity.Even;
        private StopBits _gripperStopBits = StopBits.One;
        
        private string? _gripperSelectedPort;
        public string? GripperSelectedPort { get => _gripperSelectedPort; set { _gripperSelectedPort = value; OnPropertyChanged(); } }

        private byte _gripperSlaveId = 1;
        public byte GripperSlaveId
        {
            get => _gripperSlaveId;
            set
            {
                _gripperSlaveId = value;
                OnPropertyChanged();
                // UI나 JSON에서 값이 바뀌면 즉시 디바이스 객체에 동기화
                if (_deviceState != null) _deviceState.SlaveId = value;
                if (_motion != null) _motion.SlaveId = value;
            }
        }

        private bool _isGripperConnected;
        public bool IsGripperConnected { get => _isGripperConnected; set { _isGripperConnected = value; OnPropertyChanged(); } }

        // 그리퍼 커맨드 선언부
        public ICommand ConnectGripperCommand { get; } = null!;
        public ICommand InitAllCommand { get; private set; } = null!;
        public ICommand MoveGripperCommand { get; } = null!;
        public ICommand GripperOpenCommand { get; private set; } = null!;
        public ICommand GripperCloseCommand { get; private set; } = null!;
        public ICommand RotateGripperCommand { get; } = null!;
        public ICommand Rotate0Command { get; private set; } = null!;
        public ICommand Rotate180Command { get; private set; } = null!;
        public ICommand SaveGripperCommand { get; } = null!;
        public ICommand LoadGripperCommand { get; private set; } = null!; // ★ 이 줄을 추가합니다.

        // 1. 상태 텍스트용 프로퍼티 추가 (클래스 내부에 선언)
        private string _gripperInitStatusText = "Unknown";
        public string GripperInitStatusText { get => _gripperInitStatusText; set { _gripperInitStatusText = value; OnPropertyChanged(); } }

        private string _gripperClampStatusText = "Unknown";
        public string GripperClampStatusText { get => _gripperClampStatusText; set { _gripperClampStatusText = value; OnPropertyChanged(); } }

        private string _gripperLocationText = "0 %";
        public string GripperLocationText { get => _gripperLocationText; set { _gripperLocationText = value; OnPropertyChanged(); } }

        private string _gripperAlarmText = "Normal";
        public string GripperAlarmText { get => _gripperAlarmText; set { _gripperAlarmText = value; OnPropertyChanged(); } }

        // --- Gripper 제어 및 상태 프로퍼티 ---
        private int _gripperTargetPosPercent = 0;
        public int GripperTargetPosPercent { get => _gripperTargetPosPercent; set { _gripperTargetPosPercent = value; OnPropertyChanged(); } }

        private int _gripperMoveSpeedPercent = 50;
        public int GripperMoveSpeedPercent { get => _gripperMoveSpeedPercent; set { _gripperMoveSpeedPercent = value; OnPropertyChanged(); } }

        private int _gripperForcePercent = 100;
        public int GripperForcePercent { get => _gripperForcePercent; set { _gripperForcePercent = value; OnPropertyChanged(); } }

        // --- 회전 제어 파라미터 속성 ---
        // (기존 _gripperTargetRotationPercent 삭제 후 대체)
        private int _gripperTargetRotationAngle = 0;
        public int GripperTargetRotationAngle { get => _gripperTargetRotationAngle; set { _gripperTargetRotationAngle = value; OnPropertyChanged(); } }

        private int _gripperRotationSpeedPercent = 50;
        public int GripperRotationSpeedPercent { get => _gripperRotationSpeedPercent; set { _gripperRotationSpeedPercent = value; OnPropertyChanged(); } }

        private int _gripperTorquePercent = 50;
        public int GripperTorquePercent { get => _gripperTorquePercent; set { _gripperTorquePercent = value; OnPropertyChanged(); } }

        private double _currentRotationAngle;
        public double CurrentRotationAngle { get => _currentRotationAngle; set { _currentRotationAngle = value; OnPropertyChanged(); } }

        private bool _isGripperInitialized = false;
        public bool IsGripperInitialized { get => _isGripperInitialized; set { _isGripperInitialized = value; OnPropertyChanged(); } }

        private bool _isRotationInitialized = false;
        public bool IsRotationInitialized { get => _isRotationInitialized; set { _isRotationInitialized = value; OnPropertyChanged(); } }

        private double _currentPositionPct;
        public double CurrentPositionPct
        {
            get => _currentPositionPct;
            set
            {
                _currentPositionPct = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LeftFingerXOffset));
                OnPropertyChanged(nameof(RightFingerXOffset));
            }
        }

        private double _currentRotationPct;
        public double CurrentRotationPct
        {
            get => _currentRotationPct;
            set
            {
                _currentRotationPct = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentRotationAngle));
            }
        }

        // 시각화 애니메이션 바인딩용
        public double LeftFingerXOffset => 160 - (CurrentPositionPct * 0.5); // 0% 닫힘, 100% 열림
        public double RightFingerXOffset => 200 + (CurrentPositionPct * 0.5);

        // StartGripperBackgroundTask() 내부의 폴링 루프 수정 (상태 피드백 반영 및 알람 오류 해결)
        private void StartGripperBackgroundTask()
        {
            _gripperPollingCts = new CancellationTokenSource();
            var token = _gripperPollingCts.Token;

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && _gripperConnection.IsConnected)
                {
                    try
                    {
                        // [수정] 실제 RGI 매뉴얼 스펙에 맞춘 레지스터 주소
                        int initStatus = await _deviceState.ReadRegisterAsync(0x0200);  // 클램프 초기화 상태
                        int clampStatus = await _deviceState.ReadRegisterAsync(0x0201); // 클램프 상태
                        int rawPosition = await _deviceState.ReadRegisterAsync(0x0202); // 클램프 위치
                        int alarmVal = await _deviceState.ReadRegisterAsync(0x0205);    // 에러/경고 피드백

                        // ★ [수정] RGI 매뉴얼의 올바른 회전부 피드백 레지스터
                        int rotInitStatus = await _deviceState.ReadRegisterAsync(0x020A); // 회전 초기화 상태 (0x020A)
                        int rawRotAngle = await _deviceState.ReadRegisterAsync(0x0208);   // 회전 절대 각도 (0x0208)

                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            // ★ 로직 반영: 전체 초기화(initStatus == 1) 시 Clamp/Rotate UI 플래그 모두 True
                            IsGripperInitialized = (initStatus == 1);
                            IsRotationInitialized = (initStatus == 1) || (rotInitStatus == 1);
                            IsAlarmed = (alarmVal != 0);

                            // (폴링 루프 내부의 Application.Current?.Dispatcher.Invoke 구간)
                            // 반전 로직을 적용하여 실시간 위치 표시 (장비가 1000이면 UI 0%로)
                            CurrentPositionPct = 100.0 - (rawPosition / 10.0);

                            // 각도 계산: 0x0208은 실제 도(Degree) 단위로 들어옵니다. (음수 캐스팅 추가)
                            short signedRotAngle = (short)rawRotAngle;
                            CurrentRotationPct = signedRotAngle / 3.6; // (선택사항) % 환산 유지
                            CurrentRotationAngle = signedRotAngle; // UI 애니메이션용 실제 각도 적용

                            // 상태 피드백 문자열 업데이트
                            GripperInitStatusText = initStatus == 1 ? "Initialized" : (initStatus == 2 ? "Initializing" : "Not Init");

                            switch (clampStatus)
                            {
                                case 0: GripperClampStatusText = "Moving"; break;
                                case 1: GripperClampStatusText = "Reached (No Object)"; break;
                                case 2: GripperClampStatusText = "Gripped (Object Caught)"; break;
                                case 3: GripperClampStatusText = "Dropped"; break;
                                default: GripperClampStatusText = $"Unknown ({clampStatus})"; break;
                            }

                            GripperLocationText = $"{CurrentPositionPct:F1} %";
                            GripperAlarmText = alarmVal == 0 ? "Normal" : $"Code: {alarmVal}";
                        });
                    }
                    catch { /* 통신 오류 무시 */ }
                    await Task.Delay(150);
                }
            }, token);
        }

        // --- 그리퍼 통신 연결 및 모니터링 ---
        private async Task ConnectGripperAsync()
        {
            if (IsGripperConnected)
            {
                _gripperPollingCts?.Cancel();
                _gripperConnection.Disconnect();
                IsGripperConnected = false;

                // ★ 추가: 연결 해제 시 TaskController의 그리퍼 연동도 안전하게 해제
                TaskController.GripperDevice = null;

                return;
            }

            if (string.IsNullOrEmpty(GripperSelectedPort)) return;

            IsGripperConnected = await _gripperConnection.ConnectAsync(GripperSelectedPort, _gripperBaudRate, _gripperParity, _gripperStopBits);

            if (IsGripperConnected)
            {
                StartGripperBackgroundTask();

                // =====================================================================
                // ★ 추가: 통신 연결 성공 후 MainTaskController에 그리퍼 제어 권한(객체) 전달
                // ※ 주의: MainViewModel에 선언해두신 RGIC100_Motion 인스턴스 변수명(예: _gripperDevice, _rgicMotion 등)
                // 을 확인하시고 아래 우변에 맞게 적어주세요.
                TaskController.GripperDevice = _motion;
                // =====================================================================
            }
        }

        private void LoadGripperParameters()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string paramDir = Path.GetFullPath(Path.Combine(baseDir, @"..\Parameter"));
            string gripperFilePath = Path.Combine(paramDir, "GRIPPER.json");

            var param = JsonHelper.Load<GripperParameter>(gripperFilePath);
            if (param != null)
            {
                GripperSelectedPort = string.IsNullOrEmpty(param.ComPort) ? null : param.ComPort;
                _gripperBaudRate = param.BaudRate > 0 ? param.BaudRate : 115200;
                _gripperParity = param.Parity;
                _gripperStopBits = param.StopBits;
                GripperSlaveId = param.SlaveId > 0 ? param.SlaveId : (byte)1;
            }
            else
            {
                SaveGripperParameters(showPopup: false);
            }
        }

        private void SaveGripperParameters(bool showPopup = false)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string paramDir = Path.GetFullPath(Path.Combine(baseDir, @"..\Parameter"));
            if (!Directory.Exists(paramDir)) Directory.CreateDirectory(paramDir);
            string gripperFilePath = Path.Combine(paramDir, "GRIPPER.json");

            var param = new GripperParameter
            {
                ComPort = GripperSelectedPort ?? "",
                BaudRate = _gripperBaudRate,
                Parity = _gripperParity,
                StopBits = _gripperStopBits,
                SlaveId = GripperSlaveId
            };

            JsonHelper.Save(gripperFilePath, param);
            if (showPopup) MessageBox.Show("그리퍼 통신 설정이 저장되었습니다.", "저장 완료");
        }
        #endregion

        #region [ 3. PMS Particle Counter ]
        // (BioControlView와 공용으로 사용하는 인스턴스)
        public PMS_ParticleControl? _particleDevice;

        private DispatcherTimer _particleTimer = new DispatcherTimer();
        public string ParticleIpAddress { get; set; } = "192.168.1.11";
        public int ParticlePort { get; set; } = 502;
        private bool _isParticleConnected;
        public bool IsParticleConnected { get => _isParticleConnected; set { _isParticleConnected = value; OnPropertyChanged(); } }

        public ICommand ConnectParticleCommand { get; } = null!;
        public ICommand DisconnectParticleCommand { get; } = null!;
        public ICommand StartParticleCommand { get; } = null!;
        public ICommand StopParticleCommand { get; } = null!;

        private string _part05_Sum = "0";
        public string Part05_Sum { get => _part05_Sum; set { _part05_Sum = value; OnPropertyChanged(); } }
        private string _part50_Sum = "0";
        public string Part50_Sum { get => _part50_Sum; set { _part50_Sum = value; OnPropertyChanged(); } }

        // 파티클 카운터용 기기 정보 리스트
        private ObservableCollection<DeviceInfoData> _deviceInfoList = new ObservableCollection<DeviceInfoData>();
        public ObservableCollection<DeviceInfoData> DeviceInfoList
        {
            get => _deviceInfoList;
            set { _deviceInfoList = value; OnPropertyChanged(); }
        }

        private bool _isFloatMode;
        public bool IsFloatMode
        {
            get => _isFloatMode;
            set
            {
                if (_isFloatMode != value)
                {
                    _isFloatMode = value;
                    OnPropertyChanged();
                    // ▼ 체크 상태가 바뀔 때마다 텍스트도 갱신되도록 알림
                    OnPropertyChanged(nameof(ModeText));

                    // UI에서 체크박스 변경 시 장비로 값 전송 (체크=true=Float, 해제=false=Integer)
                    if (IsParticleConnected && _particleDevice != null)
                    {
                        _ = _particleDevice.SetIeeeFloatModeAsync(value);
                    }
                }
            }
        }

        // ▼ 추가: XAML의 화면 글씨와 연동될 텍스트 속성
        public string ModeText => IsFloatMode ? "Float Mode" : "Integer Mode";

        private async Task ConnectParticleAsync()
        {
            try
            {
                StatusMessage = "Particle Counter 연결 중...";
                // ★ 수정된 부분: 포트 번호 502로 고정
                _particleDevice = new PMS_ParticleControl(ParticleIpAddress, 502);

                // 비동기로 연결 시도 결과 받아오기
                bool isSuccess = await _particleDevice.ConnectAsync();

                if (isSuccess)
                {
                    IsParticleConnected = true;
                    ResetParticleUI();

                    // ★ 추가된 부분: 접속 성공 시 장비의 현재 세팅(Coils 값)을 읽어와 UI 동기화
                    // ConnectParticleAsync() 메서드 내부 수정
                    bool isFloatMode = await _particleDevice.ReadIeeeFloatModeAsync();

                    _isFloatMode = isFloatMode; // 기존의 !isFloatMode 에서 변경
                    OnPropertyChanged(nameof(IsFloatMode));
                    OnPropertyChanged(nameof(ModeText)); // 텍스트 갱신

                    // ▼ 추가: 접속 성공 시 기기 정보 읽어와서 DataGrid 구성
                    var pmsInfo = await _particleDevice.ReadDeviceInfoAsync();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        DeviceInfoList.Clear();
                        foreach (var item in pmsInfo)
                        {
                            DeviceInfoList.Add(new DeviceInfoData { ControllerInfo = item.Title, ReadingData = item.Value });
                        }
                    });

                    // ===============================================
                    // 파티클 카운터 접속 메서드 업데이트 (ConnectParticleAsync 내부)
                    // ===============================================
                    // isSuccess == true 내부 블록 어딘가에 다음 코드를 추가하세요.
                    var pmsSettings = await _particleDevice.ReadSettingsAsync();
                    _sampleInterval = pmsSettings.sampleInterval;
                    _delayStart = pmsSettings.delayStart;
                    _repeatCount = pmsSettings.repeatCount;
                    OnPropertyChanged(nameof(SampleInterval));
                    OnPropertyChanged(nameof(DelayStart));
                    OnPropertyChanged(nameof(RepeatCount));

                    _particleTimer.Start();
                    TaskController.ParticleDevice = _particleDevice;
                    StatusMessage = "Particle Counter 연결 성공";
                }
                else
                {
                    IsParticleConnected = false;
                    StatusMessage = "[에러] Particle Counter 연결 실패";
                    MessageBox.Show("IP나 포트를 확인해주세요. 접속을 거부했습니다.", "접속 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                IsParticleConnected = false;
                StatusMessage = $"[에러] {ex.Message}";
                MessageBox.Show($"상세오류: {ex.Message}", "접속 에러", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectParticle()
        {
            _particleTimer.Stop();
            _particleDevice?.Disconnect();
            IsParticleConnected = false;
            ResetParticleUI(); // 접속 해제 시 데이터 초기화
            StatusMessage = "Particle Counter 접속 해제됨";
        }

        private async Task PollParticleDataAsync()
        {
            if (!IsParticleConnected) return;
            try
            {
                if (_particleDevice != null)
                {
                    var data = await _particleDevice.ReadParticleDataAsync();

                    // ch1(Item1) 데이터를 0.5㎛에, ch2(Item2) 데이터를 5.0㎛에 바인딩
                    Part05_Sum = data.ch1.ToString("N0");
                    Part50_Sum = data.ch2.ToString("N0");

                    // ▼ 추가된 부분: 1초마다 장비 상태를 읽어와 UI 갱신 (유휴/측정/홀드)
                    ushort status = await _particleDevice.ReadDeviceStatusAsync();
                    ParticleStatusCode = status;

                    // ▼ 추가된 부분: 측정 시간 계산 및 포맷 적용 ▼
                    var times = await _particleDevice.ReadMeasurementTimesAsync();

                    if (times.endTime.HasValue && times.elapsedTime.HasValue)
                    {
                        DateTime end = times.endTime.Value;
                        TimeSpan elapsed = times.elapsedTime.Value;

                        // 시작 시간 = 종료 시간 - 경과 시간
                        DateTime start = end - elapsed;

                        // UI 포맷에 맞춰 적용
                        ParticleEndTime = end.ToString("MM/dd/yyyy HH:mm:ss");
                        ParticleStartTime = start.ToString("MM/dd/yyyy HH:mm:ss");

                        // 경과 시간 포맷 (hh:mm:ss 형태로 표시, 만약 시간이 24시간 이상이면 TotalHours 사용)
                        ParticleElapsedTime = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                    }
                }
            }
            catch (Exception ex)
            {
                DisconnectParticle();
                StatusMessage = $"[통신 끊김] Particle: {ex.Message}";
                MessageBox.Show("Particle Counter와의 통신이 끊어졌습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ★ 파티클 동작 스텝이 끝났을 때 데이터그리드(리스트)에 결과를 추가하는 함수
        private async Task FinalizeParticleMeasurementAsync()
        {
            await PollParticleDataAsync();

            // 콤마(,)가 포함된 문자열 포맷을 double로 변환
            double.TryParse(this.Part05_Sum.Replace(",", ""), out double val05);
            double.TryParse(this.Part50_Sum.Replace(",", ""), out double val50);

            Application.Current.Dispatcher.Invoke(() =>
            {
                ParticleCounterDataList.Add(new ParticleDataModel
                {
                    StartTime = this.ParticleStartTime,
                    EndTime = this.ParticleEndTime,
                    ElapsedTime = this.ParticleElapsedTime,
                    Part05_Sum = this.Part05_Sum,
                    Part50_Sum = this.Part50_Sum
                });

                // 🟢 LiveChart에 데이터 추가
                ParticleSeries[0].Values.Add(val05);
                ParticleSeries[1].Values.Add(val50);
                ParticleLabels.Add(ParticleCounterDataList.Count.ToString()); // X축: 1, 2, 3...
            });
            Console.WriteLine("파티클 카운터 측정 완료: DataGrid 및 차트에 추가되었습니다.");
        }

        // ▼ 파티클 카운터 상태 코드 (0=유휴, 1=측정중, 257(0x0101)=홀드)
        private int _particleStatusCode;
        public int ParticleStatusCode
        {
            get => _particleStatusCode;
            set
            {
                if (_particleStatusCode != value)
                {
                    // 변경되기 이전 상태 기록
                    int previousState = _particleStatusCode;
                    _particleStatusCode = value;
                    OnPropertyChanged();

                    // ★ 추가된 로직: 파티클 카운터의 정상 상태 목록 (0, 1, 257)
                    bool isNormalState = (value == 0 || value == 1 || value == 257);

                    if (!isNormalState)
                    {
                        // 정의되지 않은 에러 값(Measurement Flow Alarm 등)이 들어오면 자동으로 정지 시퀀스 전송
                        if (IsParticleConnected && _particleDevice != null)
                        {
                            _ = _particleDevice.StopMeasurementAsync();
                        }
                    }
                    else if (previousState != 0 && value == 0)
                    {
                        // 동작 중이다가 스스로 유휴 상태(0)로 바뀌었을 때 자동 정지
                        if (IsParticleConnected && _particleDevice != null)
                        {
                            _ = _particleDevice.StopMeasurementAsync();
                        }
                    }

                    // 상태가 바뀔 때마다 Start/Stop 버튼의 활성화 상태 즉시 갱신
                    Application.Current?.Dispatcher.InvokeAsync(() =>
                        System.Windows.Input.CommandManager.InvalidateRequerySuggested());
                }
            }
        }

        // ▼ 파티클 측정 시간 관련 변수 ▼
        private string _particleStartTime = "--";
        public string ParticleStartTime { get => _particleStartTime; set { _particleStartTime = value; OnPropertyChanged(); } }

        private string _particleEndTime = "--";
        public string ParticleEndTime { get => _particleEndTime; set { _particleEndTime = value; OnPropertyChanged(); } }

        private string _particleElapsedTime = "00:00:00";
        public string ParticleElapsedTime { get => _particleElapsedTime; set { _particleElapsedTime = value; OnPropertyChanged(); } }

        private void ResetParticleUI()
        {
            // (이전 답변에서 수정한 변수명 기준 적용)
            Part05_Sum = "0";
            Part50_Sum = "0";

            // ▼ 접속 해제 시 상태를 0(유휴)으로 초기화
            ParticleStatusCode = 0;

            // ▼ 추가: 시간 초기화
            ParticleStartTime = "--";
            ParticleEndTime = "--";
            ParticleElapsedTime = "00:00:00";
            App.Current.Dispatcher.Invoke(() => DeviceInfoList.Clear());
            /*
                        // 접속 해제 시 체크박스 체크 해제 (Unchecked = Integer Mode)
                        _isFloatMode = false;
                        OnPropertyChanged(nameof(IsFloatMode));
                        OnPropertyChanged(nameof(ModeText));
                        OnPropertyChanged(nameof(SampleInterval));
                        OnPropertyChanged(nameof(DelayStart));
                        OnPropertyChanged(nameof(RepeatCount));
            */
        }

        private ushort _repeatCount;
        public ushort RepeatCount
        {
            get => _repeatCount;
            set { if (_repeatCount != value) { _repeatCount = value; OnPropertyChanged(); _ = _particleDevice?.SetRepeatCountAsync(value); } }
        }

        private ushort _sampleInterval;
        public ushort SampleInterval
        {
            get => _sampleInterval;
            set { if (_sampleInterval != value) { _sampleInterval = value; OnPropertyChanged(); _ = _particleDevice?.SetSampleIntervalAsync(value); } }
        }

        private ushort _delayStart;
        public ushort DelayStart
        {
            get => _delayStart;
            set { if (_delayStart != value) { _delayStart = value; OnPropertyChanged(); _ = _particleDevice?.SetDelayStartAsync(value); } }
        }
        #endregion

        #region [ 4. PMS Air Sampler ]
        public PMS_MiniCaptControl? _airSamplerDevice;

        private DispatcherTimer _airSamplerTimer = new DispatcherTimer();
        public string AirSamplerIpAddress { get; set; } = "192.168.1.12";
        public int AirSamplerPort { get; set; } = 502;
        private bool _isAirSamplerConnected;
        public bool IsAirSamplerConnected { get => _isAirSamplerConnected; set { _isAirSamplerConnected = value; OnPropertyChanged(); } }

        public ICommand ConnectAirSamplerCommand { get; } = null!;
        public ICommand DisconnectAirSamplerCommand { get; } = null!;
        public ICommand StartAirSamplerCommand { get; } = null!;
        public ICommand StopAirSamplerCommand { get; } = null!;

        private string _airSamplerVolumeResult = "0";
        public string AirSamplerVolumeResult { get => _airSamplerVolumeResult; set { _airSamplerVolumeResult = value; OnPropertyChanged(); } }
        private string _airSamplerIntervalTimeResult = "00m : 00s";
        public string AirSamplerIntervalTimeResult { get => _airSamplerIntervalTimeResult; set { _airSamplerIntervalTimeResult = value; OnPropertyChanged(); } }

        // 상태 메세지 (UI 바인딩 추천)
        private string _statusMessage = "Ready";
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        // 에어 샘플러용 기기 정보 리스트
        private ObservableCollection<DeviceInfoData> _airSamplerDeviceInfoList = new ObservableCollection<DeviceInfoData>();
        public ObservableCollection<DeviceInfoData> AirSamplerDeviceInfoList
        {
            get => _airSamplerDeviceInfoList;
            set { _airSamplerDeviceInfoList = value; OnPropertyChanged(); }
        }

        private async Task ConnectAirSamplerAsync()
        {
            try
            {
                StatusMessage = "Air Sampler 연결 중...";
                // ★ 수정된 부분: 포트 번호 502로 고정
                _airSamplerDevice = new PMS_MiniCaptControl(AirSamplerIpAddress, 502);

                // 비동기로 연결 시도 결과 받아오기
                bool isSuccess = await _airSamplerDevice.ConnectAsync();

                if (isSuccess)
                {
                    IsAirSamplerConnected = true;
                    bool isFloatMode = await _airSamplerDevice.ReadIeeeFloatModeAsync();

                    ResetAirSamplerUI();

                    _airSamplerIsFloatMode = isFloatMode;
                    OnPropertyChanged(nameof(AirSamplerIsFloatMode));
                    OnPropertyChanged(nameof(AirSamplerModeText));

                    // ▼ 추가: 에어 샘플러 기기 정보 읽어와서 DataGrid 구성
                    var asInfo = await _airSamplerDevice.ReadDeviceInfoAsync();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AirSamplerDeviceInfoList.Clear();
                        foreach (var item in asInfo)
                        {
                            AirSamplerDeviceInfoList.Add(new DeviceInfoData { ControllerInfo = item.Title, ReadingData = item.Value });
                        }
                    });

                    // ===============================================
                    // 에어 샘플러 접속 메서드 업데이트 (ConnectAirSamplerAsync 내부)
                    // ===============================================
                    // isSuccess == true 내부 블록 어딘가에 다음 코드를 추가하세요.
                    var asSettings = await _airSamplerDevice.ReadSettingsAsync();
                    _airSamplerInterval = asSettings.interval;
                    _airSamplerHoldTime = asSettings.holdTime;
                    _airSamplerRepeatCount = asSettings.repeat;
                    _airSamplerDelayTime = asSettings.delayTime;
                    OnPropertyChanged(nameof(AirSamplerInterval));
                    OnPropertyChanged(nameof(AirSamplerHoldTime));
                    OnPropertyChanged(nameof(AirSamplerRepeatCount));
                    OnPropertyChanged(nameof(AirSamplerDelayTime));

                    bool isVolumeMode = await _airSamplerDevice.ReadVolumeModeAsync();
                    _airSamplerIsVolumeMode = isVolumeMode;
                    OnPropertyChanged(nameof(AirSamplerIsVolumeMode));
                    OnPropertyChanged(nameof(AirSamplerVolumeModeText));

                    _airSamplerTimer.Start();
                    TaskController.AirSamplerDevice = _airSamplerDevice;
                    StatusMessage = "Air Sampler 연결 성공";
                }
                else
                {
                    IsAirSamplerConnected = false;
                    StatusMessage = "[에러] Air Sampler 연결 실패";
                    MessageBox.Show("IP나 포트를 확인해주세요. 접속을 거부했습니다.", "접속 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                IsAirSamplerConnected = false;
                StatusMessage = $"[에러] {ex.Message}";
                MessageBox.Show($"상세오류: {ex.Message}", "접속 에러", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectAirSampler()
        {
            _airSamplerTimer.Stop();
            _airSamplerDevice?.Disconnect();
            IsAirSamplerConnected = false;
            ResetAirSamplerUI(); // 접속 해제 시 데이터 초기화
            StatusMessage = "Air Sampler 접속 해제됨";
        }

        // 1. 에어 샘플러 제어 로직 영역(#region [ Air Sampler 제어 로직 ])의 변수 선언부에 다음 프로퍼티 추가
        private string _airSamplerMeasureVolume = "0.0 L";
        public string AirSamplerMeasureVolume
        {
            get => _airSamplerMeasureVolume;
            set { _airSamplerMeasureVolume = value; OnPropertyChanged(); }
        }

        private string _airSamplerMeasureFlow = "0.0 LPM";
        public string AirSamplerMeasureFlow
        {
            get => _airSamplerMeasureFlow;
            set { _airSamplerMeasureFlow = value; OnPropertyChanged(); }
        }

        // 2. PollAirSamplerDataAsync() 메서드 내부 수정
        private async Task PollAirSamplerDataAsync()
        {
            if (!IsAirSamplerConnected) return;
            try
            {
                if (_airSamplerDevice != null)
                {
                    // ▼ 수정된 부분: 인자(AirSamplerIsFloatMode) 추가 및 반환된 튜플 이름(totalVolume, totalInterval) 매칭
                    var data = await _airSamplerDevice.ReadSamplerDataAsync(AirSamplerIsFloatMode);

                    // 기존 Total Volume 결과 표시 (N0 또는 N1)
                    AirSamplerVolumeResult = data.totalVolume.ToString("N1");

                    // ▼ 수정된 부분: timeSeconds -> totalInterval 로 변경
                    TimeSpan ts = TimeSpan.FromSeconds(data.totalInterval);
                    AirSamplerIntervalTimeResult = $"{(int)ts.TotalMinutes:D2}m : {ts.Seconds:D2}s";

                    // 🟢 새로 추가: 차트의 Y축으로 사용할 Raw 초(Seconds) 데이터 저장
                    _currentAirSamplerIntervalSeconds = data.totalInterval;

                    // ▼ 새로 추가했던 Flow Rate 및 Volume 읽기 후 UI 업데이트 ▼
                    var flowVolData = await _airSamplerDevice.ReadFlowAndVolumeAsync(AirSamplerIsFloatMode);
                    // 읽어온 값을 바탕으로 포맷팅하여 바인딩 (소수점 1자리 + 단위 표시)
                    AirSamplerMeasureFlow = $"{flowVolData.flowRate:F1} LPM";
                    AirSamplerMeasureVolume = $"{flowVolData.volume:F1} L";

                    // 상태 코드를 지속적으로 읽어와 UI와 버튼 동기화
                    ushort status = await _airSamplerDevice.ReadDeviceStatusAsync();
                    AirSamplerStatusCode = status;

                    // 측정 시간 계산 및 포맷 적용
                    var times = await _airSamplerDevice.ReadMeasurementTimesAsync();
                    if (times.endTime.HasValue && times.elapsedTime.HasValue)
                    {
                        DateTime end = times.endTime.Value;
                        TimeSpan elapsed = times.elapsedTime.Value;
                        DateTime start = end - elapsed;

                        AirSamplerEndTime = end.ToString("MM/dd/yyyy HH:mm:ss");
                        AirSamplerStartTime = start.ToString("MM/dd/yyyy HH:mm:ss");
                        AirSamplerElapseTime = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                    }
                }
            }
            catch (Exception ex)
            {
                DisconnectAirSampler();
                StatusMessage = $"[통신 끊김] Air Sampler: {ex.Message}";
                MessageBox.Show("Air Sampler와의 통신이 끊어졌습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // -------------------------------------------------------------------
        // ★ 2. TaskController에서 'ProcessStep.Air_Sampling' 완료 이벤트를 받으면 실행될 함수
        private async Task FinalizeAirSamplingAsync()
        {
            await PollAirSamplerDataAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                AirSamplerDataList.Add(new AirSamplerDataModel
                {
                    StartTime = this.AirSamplerStartTime,
                    EndTime = this.AirSamplerEndTime,
                    ElapseTime = this.AirSamplerElapseTime,
                    Volume = this.AirSamplerMeasureVolume,
                    FlowRate = this.AirSamplerMeasureFlow,
                    IntervalTime = this.AirSamplerIntervalTimeResult
                });

                // 🟢 LiveChart에 데이터 추가
                AirSamplerSeries[0].Values.Add(_currentAirSamplerIntervalSeconds);
                AirSamplerLabels.Add(AirSamplerDataList.Count.ToString()); // X축: 1, 2, 3...
            });
            Console.WriteLine("에어 샘플러 측정 완료: DataGrid 및 차트에 추가되었습니다.");
        }

        private bool _airSamplerIsFloatMode;
        public bool AirSamplerIsFloatMode
        {
            get => _airSamplerIsFloatMode;
            set
            {
                if (_airSamplerIsFloatMode != value)
                {
                    _airSamplerIsFloatMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AirSamplerModeText)); // 텍스트 갱신 알림

                    // UI에서 체크박스 변경 시 장비로 값 전송
                    if (IsAirSamplerConnected && _airSamplerDevice != null)
                    {
                        _ = _airSamplerDevice.SetIeeeFloatModeAsync(value);
                    }
                }
            }
        }

        // 화면에 보여질 글씨 속성
        public string AirSamplerModeText => AirSamplerIsFloatMode ? "Float Mode" : "Integer Mode";

        // ▼ 에어 샘플러 상태 코드 (0=유휴, 1=측정중, 257=홀드, 513=Lo Flow, 769=Hi Flow 등)
        private int _airSamplerStatusCode;
        public int AirSamplerStatusCode
        {
            get => _airSamplerStatusCode;
            set
            {
                if (_airSamplerStatusCode != value)
                {
                    // 변경되기 이전 상태 기록
                    int previousState = _airSamplerStatusCode;
                    _airSamplerStatusCode = value;
                    OnPropertyChanged();

                    // ★ 추가된 로직: 매뉴얼에 정의된 정상 상태 목록
                    bool isNormalState = (value == 0 || value == 1 || value == 257 || value == 513 || value == 769);

                    if (!isNormalState)
                    {
                        // 정의되지 않은 에러 값(Flow Alarm 등)이 들어오면 자동으로 정지 시퀀스(보호) 전송
                        if (IsAirSamplerConnected && _airSamplerDevice != null)
                        {
                            _ = _airSamplerDevice.StopMeasurementAsync();
                        }
                    }
                    else if (previousState != 0 && value == 0)
                    {
                        // 동작 중이다가 스스로 유휴 상태(0)로 바뀌었을 때 자동 정지
                        if (IsAirSamplerConnected && _airSamplerDevice != null)
                        {
                            _ = _airSamplerDevice.StopMeasurementAsync();
                        }
                    }

                    // 상태가 바뀔 때마다 Start/Stop 버튼의 활성화 상태 즉시 갱신
                    Application.Current?.Dispatcher.InvokeAsync(() =>
                        System.Windows.Input.CommandManager.InvalidateRequerySuggested());
                }
            }
        }

        // ▼ 에어 샘플러 측정 시간 관련 변수 ▼
        private string _airSamplerStartTime = "--";
        public string AirSamplerStartTime { get => _airSamplerStartTime; set { _airSamplerStartTime = value; OnPropertyChanged(); } }

        private string _airSamplerEndTime = "--";
        public string AirSamplerEndTime { get => _airSamplerEndTime; set { _airSamplerEndTime = value; OnPropertyChanged(); } }

        private string _airSamplerElapseTime = "00:00:00";
        public string AirSamplerElapseTime { get => _airSamplerElapseTime; set { _airSamplerElapseTime = value; OnPropertyChanged(); } }

        private void ResetAirSamplerUI()
        {
            AirSamplerVolumeResult = "0";
            AirSamplerIntervalTimeResult = "00m : 00s";

            // ▼ 추가: 접속 해제 시 Flow 및 Volume UI 초기화
            AirSamplerMeasureFlow = "0.0 LPM";
            AirSamplerMeasureVolume = "0.0 L";

            // ▼ 접속 해제 시 상태를 0(유휴)으로 초기화
            AirSamplerStatusCode = 0;

            // ▼ 추가: 에어 샘플러 시간 초기화
            AirSamplerStartTime = "--";
            AirSamplerEndTime = "--";
            AirSamplerElapseTime = "00:00:00";
            App.Current.Dispatcher.Invoke(() => AirSamplerDeviceInfoList.Clear());

            /*
                        // ▼ 추가: 접속 해제 시 체크박스와 텍스트 초기화
                        _airSamplerIsFloatMode = false;
                        OnPropertyChanged(nameof(AirSamplerIsFloatMode));
                        OnPropertyChanged(nameof(AirSamplerModeText));

                        _airSamplerInterval = 0;
                        _airSamplerHoldTime = 0;
                        _airSamplerRepeatCount = 0;
                        _airSamplerDelayTime = 0;
                        OnPropertyChanged(nameof(AirSamplerInterval));
                        OnPropertyChanged(nameof(AirSamplerHoldTime));
                        OnPropertyChanged(nameof(AirSamplerRepeatCount));
                        OnPropertyChanged(nameof(AirSamplerDelayTime));

                        _airSamplerIsVolumeMode = false;
                        OnPropertyChanged(nameof(AirSamplerIsVolumeMode));
                        OnPropertyChanged(nameof(AirSamplerVolumeModeText));
            */
        }

        private ushort _airSamplerRepeatCount;
        public ushort AirSamplerRepeatCount
        {
            get => _airSamplerRepeatCount;
            set { if (_airSamplerRepeatCount != value) { _airSamplerRepeatCount = value; OnPropertyChanged(); _ = _airSamplerDevice?.SetRepeatCountAsync(value); } }
        }

        private ushort _airSamplerInterval;
        public ushort AirSamplerInterval
        {
            get => _airSamplerInterval;
            set { if (_airSamplerInterval != value) { _airSamplerInterval = value; OnPropertyChanged(); _ = _airSamplerDevice?.SetSampleIntervalAsync(value); } }
        }

        private ushort _airSamplerHoldTime;
        public ushort AirSamplerHoldTime
        {
            get => _airSamplerHoldTime;
            set { if (_airSamplerHoldTime != value) { _airSamplerHoldTime = value; OnPropertyChanged(); _ = _airSamplerDevice?.SetHoldTimeAsync(value); } }
        }

        private ushort _airSamplerDelayTime;
        public ushort AirSamplerDelayTime
        {
            get => _airSamplerDelayTime;
            set { if (_airSamplerDelayTime != value) { _airSamplerDelayTime = value; OnPropertyChanged(); _ = _airSamplerDevice?.SetDelayTimeAsync(value); } }
        }

        private bool _airSamplerIsVolumeMode;
        public bool AirSamplerIsVolumeMode
        {
            get => _airSamplerIsVolumeMode;
            set
            {
                if (_airSamplerIsVolumeMode != value)
                {
                    _airSamplerIsVolumeMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AirSamplerVolumeModeText));

                    // ▼ 추가된 부분: Volume Mode 변경 시 Interval 라벨 텍스트도 갱신하도록 알림
                    OnPropertyChanged(nameof(AirSamplerIntervalLabelText));

                    // UI 체인지 시 장비에 상태 전송
                    if (IsAirSamplerConnected && _airSamplerDevice != null)
                        _ = _airSamplerDevice.SetVolumeModeAsync(value);
                }
            }
        }

        // 화면에 보여질 글씨 속성: 체크 시 "Sample Volume", 해제 시 "Interval Mode"
        public string AirSamplerVolumeModeText => AirSamplerIsVolumeMode ? "Sample Volume" : "Interval Mode";

        // ▼ 새로 추가할 부분: 볼륨 모드 체크 여부에 따른 입력 라벨 텍스트 동적 변경
        public string AirSamplerIntervalLabelText => AirSamplerIsVolumeMode ? "Volume(L) :" : "Sample Interval(S) :";
        #endregion

        #region [ 5. PMS Data Handling ]
        public class DeviceInfoData
        {
            // 생성 시 기본적으로 빈 문자열("")을 가지도록 할당하여 Null 경고 해결
            public string ControllerInfo { get; set; } = string.Empty;
            public string ReadingData { get; set; } = string.Empty;
        }

        // ==========================================
        // ▼ PMS 파라미터 저장을 위한 모델 클래스 추가 ▼
        // ==========================================

        // ▼ 새로 추가된 Command (PMS 파라미터 전용)
        public ICommand SavePmsParametersCommand { get; } = null!;

        public class PmsParameter
        {
            public PmsParticleSettings PMS_Particle { get; set; } = new PmsParticleSettings();
            public PmsAirSamplerSettings PMS_AirSampler { get; set; } = new PmsAirSamplerSettings();
        }

        public class PmsParticleSettings
        {
            public string IpAddress { get; set; } = "192.168.33.70";
            public ushort RepeatCount { get; set; } = 0;
            public bool IsFloatMode { get; set; } = false;
            public ushort SampleInterval { get; set; } = 60;
            public ushort DelayStart { get; set; } = 0;
        }

        public class PmsAirSamplerSettings
        {
            public string IpAddress { get; set; } = "192.168.33.71";
            public ushort RepeatCount { get; set; } = 0;
            public bool IsFloatMode { get; set; } = false;
            public bool IsVolumeMode { get; set; } = false;
            public ushort Interval { get; set; } = 0;
            public ushort HoldTime { get; set; } = 0;
            public ushort DelayTime { get; set; } = 0;
        }

        // ========================================================
        // ▼ 추가: PMS 파라미터 불러오기 및 저장 메서드
        // ========================================================
        private void LoadPmsParameters()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string paramDir = Path.GetFullPath(Path.Combine(baseDir, @"..\Parameter"));
            string pmsFilePath = Path.Combine(paramDir, "PMS_Parameter.json");

            var pmsParams = JsonHelper.Load<PmsParameter>(pmsFilePath);
            if (pmsParams != null)
            {
                // 파티클 카운터 셋팅 반영
                ParticleIpAddress = pmsParams.PMS_Particle.IpAddress;
                _repeatCount = pmsParams.PMS_Particle.RepeatCount;
                _isFloatMode = pmsParams.PMS_Particle.IsFloatMode;
                _sampleInterval = pmsParams.PMS_Particle.SampleInterval;
                _delayStart = pmsParams.PMS_Particle.DelayStart;

                // 에어 샘플러 셋팅 반영
                AirSamplerIpAddress = pmsParams.PMS_AirSampler.IpAddress;
                _airSamplerRepeatCount = pmsParams.PMS_AirSampler.RepeatCount;
                _airSamplerIsFloatMode = pmsParams.PMS_AirSampler.IsFloatMode;
                _airSamplerIsVolumeMode = pmsParams.PMS_AirSampler.IsVolumeMode;
                _airSamplerInterval = pmsParams.PMS_AirSampler.Interval;
                _airSamplerHoldTime = pmsParams.PMS_AirSampler.HoldTime;
                _airSamplerDelayTime = pmsParams.PMS_AirSampler.DelayTime;

                // UI에 바인딩 갱신 알림
                OnPropertyChanged(nameof(RepeatCount));
                OnPropertyChanged(nameof(IsFloatMode));
                OnPropertyChanged(nameof(ModeText));
                OnPropertyChanged(nameof(SampleInterval));
                OnPropertyChanged(nameof(DelayStart));

                OnPropertyChanged(nameof(AirSamplerRepeatCount));
                OnPropertyChanged(nameof(AirSamplerIsFloatMode));
                OnPropertyChanged(nameof(AirSamplerModeText));
                OnPropertyChanged(nameof(AirSamplerIsVolumeMode));
                OnPropertyChanged(nameof(AirSamplerVolumeModeText));
                OnPropertyChanged(nameof(AirSamplerIntervalLabelText));
                OnPropertyChanged(nameof(AirSamplerInterval));
                OnPropertyChanged(nameof(AirSamplerHoldTime));
                OnPropertyChanged(nameof(AirSamplerDelayTime));
            }
            else
            {
                // 파일이 없으면 초기값으로 생성
                SavePmsParameters(false);
            }
        }

        private void SavePmsParameters(bool showPopup = true)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string paramDir = Path.GetFullPath(Path.Combine(baseDir, @"..\Parameter"));
            if (!Directory.Exists(paramDir)) Directory.CreateDirectory(paramDir);

            string pmsFilePath = Path.Combine(paramDir, "PMS_Parameter.json");

            var pmsParams = new PmsParameter
            {
                PMS_Particle = new PmsParticleSettings
                {
                    IpAddress = ParticleIpAddress,
                    RepeatCount = RepeatCount,
                    IsFloatMode = IsFloatMode,
                    SampleInterval = SampleInterval,
                    DelayStart = DelayStart
                },
                PMS_AirSampler = new PmsAirSamplerSettings
                {
                    IpAddress = AirSamplerIpAddress,
                    RepeatCount = AirSamplerRepeatCount,
                    IsFloatMode = AirSamplerIsFloatMode,
                    IsVolumeMode = AirSamplerIsVolumeMode,
                    Interval = AirSamplerInterval,
                    HoldTime = AirSamplerHoldTime,
                    DelayTime = AirSamplerDelayTime
                }
            };

            JsonHelper.Save(pmsFilePath, pmsParams);

            if (showPopup)
            {
                MessageBox.Show("PMS 설정 파라미터가 성공적으로 저장되었습니다.", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region [ 6. Fastech I/O Control ]
        // Fastech I/O Control
        private FastechIoControl? _fastechIo;
        private DispatcherTimer _ioTimer = new DispatcherTimer();

        private string _ioIpAddress = "192.168.1.20";
        public string IoIpAddress
        {
            get => _ioIpAddress;
            set { _ioIpAddress = value; OnPropertyChanged(); }
        }
        private int _ioPort = 502;
        public int IoPort
        {
            get => _ioPort;
            set { _ioPort = value; OnPropertyChanged(); }
        }
        private bool _isIoConnected;
        public bool IsIoConnected { get => _isIoConnected; set { _isIoConnected = value; OnPropertyChanged(); } }

        public ObservableCollection<IoPin> Inputs { get; set; } = new ObservableCollection<IoPin>();
        public ObservableCollection<IoPin> Outputs { get; set; } = new ObservableCollection<IoPin>();

        public ICommand ConnectIoCommand { get; } = null!;
        public ICommand DisconnectIoCommand { get; } = null!;
        public ICommand SaveIoCommand { get; } = null!;
        public ICommand LoadIoCommand { get; } = null!;

        // 분리된 페이징 커맨드
        public ICommand PrevInputPageCommand { get; } = null!;
        public ICommand NextInputPageCommand { get; } = null!;
        public ICommand PrevOutputPageCommand { get; } = null!;
        public ICommand NextOutputPageCommand { get; } = null!;

        // 전체 I/O 리스트
        private List<IoPin> _allInputs = new List<IoPin>();

        private List<IoPin> _allOutputs = new List<IoPin>();
        // UI 바인딩용 (현재 페이지)
        private int _totalInputPoints = 8;
        private int _totalOutputPoints = 8;
        private int _ioItemsPerPage = 16; // 한 페이지 최대 16점

        // 개별 페이지 변수
        private int _currentInputPage = 0;
        private int _currentOutputPage = 0;

        // 총 수량이 16점(1페이지)을 초과할 때만 버튼 영역 표시
        public bool IsInputPaginationVisible => _totalInputPoints > _ioItemsPerPage;
        public bool IsOutputPaginationVisible => _totalOutputPoints > _ioItemsPerPage;

        private async Task ConnectIoAsync()
        {
            try
            {
                _fastechIo = new FastechIoControl(IoIpAddress, IoPort);
                bool success = await _fastechIo.ConnectAsync();
                if (success)
                {
                    IsIoConnected = true;
                    _ioTimer.Start();
                }
                else MessageBox.Show("I/O 모듈 연결에 실패했습니다.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void DisconnectIo()
        {
            _ioTimer.Stop();
            _fastechIo?.Disconnect();
            IsIoConnected = false;

            // 화면 초기화
            foreach (var pin in Inputs) pin.IsOn = false;
            foreach (var pin in Outputs) pin.IsOn = false;
        }

        private void LoadIoParameters()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string paramDir = Path.GetFullPath(Path.Combine(baseDir, @"..\Parameter"));
            string ioFilePath = Path.Combine(paramDir, "IOList.json");

            var ioParam = JsonHelper.Load<IoParameter>(ioFilePath);
            if (ioParam == null)
            {
                // 파일이 없으면 8점 및 기본 IP/Port로 기본 생성 후 저장
                ioParam = new IoParameter
                {
                    IpAddress = "192.168.1.20",
                    Port = 502,
                    Input = new IoListCategory { Total = 8 },
                    Output = new IoListCategory { Total = 8 }
                };

                for (int i = 0; i < 8; i++)
                {
                    ioParam.Input.Items.Add(new IoItemInfo { Number = $"X{i:D3}", Name = $"Input_Spare X{i:D3}", IsUse = false });
                    ioParam.Output.Items.Add(new IoItemInfo { Number = $"Y{i:D3}", Name = $"Output_Spare Y{i:D3}", IsUse = false });
                }
                JsonHelper.Save(ioFilePath, ioParam);
            }

            // ▼ 추가된 로직: IP 및 Port 불러오기 (값이 비어있을 경우 기본값 유지) ▼
            IoIpAddress = string.IsNullOrWhiteSpace(ioParam.IpAddress) ? "192.168.1.20" : ioParam.IpAddress;
            IoPort = ioParam.Port == 0 ? 502 : ioParam.Port;

            _totalInputPoints = ioParam.Input.Total;
            _totalOutputPoints = ioParam.Output.Total;
            _allInputs.Clear();
            _allOutputs.Clear();

            // Input 모델화
            for (int i = 0; i < _totalInputPoints; i++)
            {
                var item = i < ioParam.Input.Items.Count ? ioParam.Input.Items[i] : new IoItemInfo { Number = $"X{i:D3}", Name = $"Input_Spare X{i:D3}" };
                _allInputs.Add(new IoPin { PinIndex = i, IsInput = true, Number = item.Number, Name = item.Name, IsUse = item.IsUse });
            }

            // Output 모델화
            for (int i = 0; i < _totalOutputPoints; i++)
            {
                int pinIdx = i;
                var item = pinIdx < ioParam.Output.Items.Count ? ioParam.Output.Items[pinIdx] : new IoItemInfo { Number = $"Y{pinIdx:D3}", Name = $"Output_Spare Y{pinIdx:D3}" };
                _allOutputs.Add(new IoPin
                {
                    PinIndex = pinIdx,
                    IsInput = false,
                    Number = item.Number,
                    Name = item.Name,
                    IsUse = item.IsUse,
                    ToggleCommand = new RelayCommand(async _ => await ToggleOutputAsync(pinIdx))
                });
            }

            _currentInputPage = 0;
            _currentOutputPage = 0;
            UpdateInputPage();
            UpdateOutputPage();
            OnPropertyChanged(nameof(IsInputPaginationVisible));
            OnPropertyChanged(nameof(IsOutputPaginationVisible));
        }

        private void SaveIoParameters(bool showPopup = false)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string paramDir = Path.GetFullPath(Path.Combine(baseDir, @"..\Parameter"));
            if (!Directory.Exists(paramDir)) Directory.CreateDirectory(paramDir);
            string ioFilePath = Path.Combine(paramDir, "IOList.json");

            var ioParam = new IoParameter
            {
                // ▼ 추가된 로직: 현재 입력되어 있는 IP와 Port 저장 ▼
                IpAddress = IoIpAddress,
                Port = IoPort,
                Input = new IoListCategory { Total = _totalInputPoints },
                Output = new IoListCategory { Total = _totalOutputPoints }
            };

            foreach (var input in _allInputs)
                ioParam.Input.Items.Add(new IoItemInfo { Number = input.Number, Name = input.Name, IsUse = input.IsUse });

            foreach (var output in _allOutputs)
                ioParam.Output.Items.Add(new IoItemInfo { Number = output.Number, Name = output.Name, IsUse = output.IsUse });

            JsonHelper.Save(ioFilePath, ioParam);
            if (showPopup) MessageBox.Show("I/O 설정이 저장되었습니다.", "저장 완료");
        }

        // 개별 페이지 업데이트 함수
        private void UpdateInputPage()
        {
            Inputs.Clear();
            int startIdx = _currentInputPage * _ioItemsPerPage;
            for (int i = startIdx; i < startIdx + _ioItemsPerPage && i < _allInputs.Count; i++)
            {
                Inputs.Add(_allInputs[i]);
            }
        }

        private void UpdateOutputPage()
        {
            Outputs.Clear();
            int startIdx = _currentOutputPage * _ioItemsPerPage;
            for (int i = startIdx; i < startIdx + _ioItemsPerPage && i < _allOutputs.Count; i++)
            {
                Outputs.Add(_allOutputs[i]);
            }
        }

        // 모니터링 폴링 함수 수정
        private async Task PollIoDataAsync()
        {
            if (!IsIoConnected || _fastechIo == null) return;

            // Total 갯수에 따라 전체 읽어오기
            ushort[] inDataArray = await _fastechIo.ReadAllInputsAsync(1, _totalInputPoints);

            if (inDataArray != null && inDataArray.Length > 0)
            {
                for (int i = 0; i < _allInputs.Count; i++)
                {
                    int wordIdx = i / 16;
                    int bitIdx = i % 16;
                    if (wordIdx < inDataArray.Length)
                    {
                        // IsUse 가 True일 때만 상태 갱신
                        if (_allInputs[i].IsUse)
                        {
                            _allInputs[i].IsOn = (inDataArray[wordIdx] & (1 << bitIdx)) != 0;
                        }
                    }
                }
            }
        }

        // 개별 출력 함수 수정
        private async Task ToggleOutputAsync(int pinIndex)
        {
            if (!IsIoConnected || _fastechIo == null) return;
            if (!_allOutputs[pinIndex].IsUse) return; // 미사용 핀은 동작 안 함

            bool newState = !_allOutputs[pinIndex].IsOn;
            _allOutputs[pinIndex].IsOn = newState;

            await _fastechIo.WriteOutputAsync(1, (ushort)pinIndex, newState);
        }
         #endregion


        public MainViewModel()
        {
            // ★ 추가된 부분: Visual Studio XAML 디자이너 화면(미리보기)일 때는 아래 하드웨어 로직을 실행하지 않고 빠져나감
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            // ★ 프로그램 종료 시 자동으로 두 JSON 파일 저장
            if (Application.Current != null)
            {
                Application.Current.Exit += (s, e) =>
                {
                    SaveParameters(showPopup: false);
                    // ▼ 추가: 프로그램 종료 시 PMS 파라미터도 자동 저장
                    SavePmsParameters(showPopup: false);
                };
            }

            TaskController = new MainTaskController();

            // ★ 1초마다 시간 업데이트 (추가된 코드)
            // ★ 수정 1: CS4014 경고 해결 (_ = 할당으로 의도적 백그라운드 실행 명시)
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    CurrentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    await Task.Delay(1000);
                }
            });
            
            
            #region [ Gripper 생성자 수정 ]                                             
            _gripperConnection = new ModbusRtuConnection();
            _deviceState = new ModbusDeviceState(_gripperConnection, slaveId: GripperSlaveId);
            _motion = new RGIC100_Motion(_gripperConnection);

            // 그리퍼 전용 커맨드 초기화
            ConnectGripperCommand = new RelayCommand(async _ => await ConnectGripperAsync());
            SaveGripperCommand = new RelayCommand(_ => SaveGripperParameters(showPopup: true));

            // ★ 새로 추가할 부분: 그리퍼 전용 불러오기 커맨드 연결
            LoadGripperCommand = new RelayCommand(_ => {
                LoadGripperParameters();
                MessageBox.Show("그리퍼 설정 및 파라미터를 불러왔습니다.", "불러오기 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            LoadGripperParameters(); // 프로그램 시작 시 그리퍼 설정 로드

            // 3. 생성자 내부에서 커맨드 초기화 로직 수정 (기존 그리퍼 커맨드 매핑 부분을 교체)
            InitAllCommand = new RelayCommand(async _ => { if (IsGripperConnected) await _motion.InitializeAllAsync(); });

            MoveGripperCommand = new RelayCommand(async _ => await _motion.MoveGripperAsync(GripperTargetPosPercent, GripperMoveSpeedPercent, GripperForcePercent));
            GripperOpenCommand = new RelayCommand(async _ => { GripperTargetPosPercent = 100; await _motion.MoveGripperAsync(100, GripperMoveSpeedPercent, GripperForcePercent); });
            GripperCloseCommand = new RelayCommand(async _ => { GripperTargetPosPercent = 0; await _motion.MoveGripperAsync(0, GripperMoveSpeedPercent, GripperForcePercent); });

            // 바인딩된 변수명과 동작 각도를 %가 아닌 실제 각도(Degree)로 수정
            RotateGripperCommand = new RelayCommand(async _ => await _motion.RotateGripperAsync(GripperTargetRotationAngle, GripperRotationSpeedPercent, GripperTorquePercent));
            Rotate0Command = new RelayCommand(async _ => { GripperTargetRotationAngle = 0; await _motion.RotateGripperAsync(0, GripperRotationSpeedPercent, GripperTorquePercent); });
            // 50%가 아닌 180도 직관적 전송
            Rotate180Command = new RelayCommand(async _ => { GripperTargetRotationAngle = 180; await _motion.RotateGripperAsync(180, GripperRotationSpeedPercent, GripperTorquePercent); });            
            #endregion

            // ★ AZ 모터 8축 초기화 (국번 1~8 지정)
            for (byte i = 1; i <= 8; i++)
            {
                var device = new OrientalAzMotorDevice(_azCommPortNo, slaveId: i);
                var axisVm = new AzAxisViewModel(device, (isBusy) => IsAzCommBusy = isBusy);
                axisVm.SaveAxisCommand = new RelayCommand(_ => SaveSingleAxisParameters(axisVm));
                MotionAxes.Add(axisVm);
            }

            // 축(Axes) 생성 후 파라미터 로딩 실행
            LoadParameters();

            // ▼ 추가된 코드: 전체 불러오기 커맨드 로직 ▼
            LoadParametersCommand = new RelayCommand(_ =>
            {
                LoadParameters();
                MessageBox.Show("전체 파라미터 및 포지션 데이터를 불러왔습니다.", "불러오기 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            // ▼ 추가: PMS 파라미터 저장 커맨드 초기화 및 초기 로딩 실행
            SavePmsParametersCommand = new RelayCommand(_ => SavePmsParameters(showPopup: true));
            LoadPmsParameters(); // 저장된 PMS JSON 파라미터를 읽어 UI에 반영

            // 단일 커맨드에서 IsConnected 상태에 따라 연결 / 해제 분기
            ConnectCommand = new RelayCommand(async (o) => await ToggleConnectionAsync());
            // ★ AZ 모터 8축 초기화 함수 호출
            InitializeMotionAxes();

            RefreshPortsCommand = new RelayCommand(_ => RefreshComPorts());

            // ★ Home Command 추가 (지정된 순서로 진행 및 SD 데이터 리딩 동기화 적용)
            HomeAllCommand = new RelayCommand(async (o) =>
            {
                bool allSuccess = true;

                // ★ 요청하신 Home 진행 순서 (SlaveId 기준: 2 -> 1 -> 3 -> 4 -> 6 -> 7 -> 5 -> 8)
                int[] homeSequence = { 2, 1, 3, 4, 6, 7, 5, 8 };

                foreach (int slaveId in homeSequence)
                {
                    // 해당 국번(MotorId)을 가진 축 뷰모델을 찾음
                    var axis = MotionAxes.FirstOrDefault(a => a.MotorId == slaveId);
                    if (axis != null)
                    {
                        // 각 축의 Home 진행 및 SD Data Reading이 모두 완료될 때까지 대기
                        bool success = await axis.HomeAxisAsync();

                        if (!success)
                        {
                            MessageBox.Show($"축 {slaveId}번의 원점 복귀가 실패하여, 전체 Home 진행을 중단합니다.", "Home All 중단", MessageBoxButton.OK, MessageBoxImage.Error);
                            allSuccess = false;
                            break; // 실패 시 다음 축 진행 중단
                        }
                    }
                }

                // 모든 축의 원점 복귀 및 리딩이 성공했을 때만 전체 초기화 상태를 True로 설정
                IsAllInitialized = allSuccess;

                if (allSuccess)
                {
                    MessageBox.Show("모든 축의 원점 복귀 및 SD 파라미터 리딩이 완료되었습니다.", "All Home 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            // ★ 수정: 전체 파라미터 저장 명령
            SaveParametersCommand = new RelayCommand(_ => SaveParameters(showPopup: true));

            // ★ 예전 테스트용 Move & Stop 커맨드 (새로운 그리퍼 함수로 변경하여 에러 해결)
            // ★ 예전 테스트용 Move & Stop 커맨드 (새로운 그리퍼 함수로 변경하여 에러 해결)
            MoveCommand = new RelayCommand(async _ =>
            {
                if (IsGripperConnected)
                {
                    // TargetPositionPct -> GripperTargetPosPercent 로 변경
                    await _motion.MoveGripperAsync(GripperTargetPosPercent, 50, 100);
                }
            }, _ => !IsAlarmed);
            
            StopCommand = new RelayCommand(async _ =>
            {
                if (IsGripperConnected)
                {
                    await _motion.StopAsync();
                }
            });

            // ★ 수정된 부분: UI 알람 상태 초기화 및 등록된 모든 축의 알람 리셋 명령 수행
            ResetAlarmCommand = new RelayCommand(/*async*/ _ =>
            {
                if (MotionAxes != null)
                {
                    // 1. 모든 축을 순회하며 알람 상태인 축만 리셋 명령 실행
                    foreach (var axis in MotionAxes)
                    {
                        if (axis.IsAlarmed)
                        {
                            // 개별 축 뷰모델에 정의된 ResetAlarmCommand를 실행
                            if (axis.ResetAlarmCommand != null && axis.ResetAlarmCommand.CanExecute(null))
                            {
                                axis.ResetAlarmCommand.Execute(null);
                            }
                        }
                    }
                }

                // 2. 메인 화면의 전체 알람 상태 플래그 해제
                IsAlarmed = false;
            });

            RefreshComPorts(); // 실제 실행될 때만 포트 검색

            #region [ PMS Control Initialization ]
            // 프로그램 실행 시 UI 초기화 (기본값)
            ResetParticleUI();
            ResetAirSamplerUI();
            StatusMessage = "Ready";

            // 커맨드 초기화
            // MainViewModel 생성자 내부의 RelayCommand 초기화 부분을 async를 사용하도록 변경합니다.
            ConnectParticleCommand = new RelayCommand(async o => await ConnectParticleAsync(), o => !IsParticleConnected);
            DisconnectParticleCommand = new RelayCommand(o => DisconnectParticle(), o => IsParticleConnected);

            ConnectAirSamplerCommand = new RelayCommand(async o => await ConnectAirSamplerAsync(), o => !IsAirSamplerConnected);
            DisconnectAirSamplerCommand = new RelayCommand(o => DisconnectAirSampler(), o => IsAirSamplerConnected);

            // ⭕ 버튼 연동 수정: 
            // 시작 버튼은 연결되어 있고 '유휴 상태(0)'일 때만 활성화
            StartParticleCommand = new RelayCommand(async o =>
            {
                if (_particleDevice != null)
                {
                    await _particleDevice.StartMeasurementAsync();
                }
            }, o => IsParticleConnected && ParticleStatusCode == 0);

            // 정지 버튼은 연결되어 있고 '동작 중(0이 아님)'일 때만 활성화
            StopParticleCommand = new RelayCommand(async o =>
            {
                if (_particleDevice != null)
                {
                    await _particleDevice.StopMeasurementAsync();
                }
            }, o => IsParticleConnected && ParticleStatusCode != 0);

            // 시작 버튼: 연결되어 있고 '유휴 상태(0)'일 때만 활성화
            StartAirSamplerCommand = new RelayCommand(async o =>
            {
                if (_airSamplerDevice != null)
                {
                    await _airSamplerDevice.StartMeasurementAsync(); // 기존 메서드명에 맞게 유지
                }
            }, o => IsAirSamplerConnected && AirSamplerStatusCode == 0);

            // 정지 버튼: 연결되어 있고 '동작 중(0이 아님)'일 때만 활성화
            StopAirSamplerCommand = new RelayCommand(async o =>
            {
                if (_airSamplerDevice != null)
                {
                    await _airSamplerDevice.StopMeasurementAsync(); // 기존 메서드명에 맞게 유지
                }
            }, o => IsAirSamplerConnected && AirSamplerStatusCode != 0);

            // 폴링 타이머 셋업 (1초마다 데이터 읽기)
            _particleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _particleTimer.Tick += async (s, e) => await PollParticleDataAsync();

            _airSamplerTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _airSamplerTimer.Tick += async (s, e) => await PollAirSamplerDataAsync();

            // 1. 완료 이벤트 구독 (TaskController에서 측정이 끝나면 호출됨)
            // =========================================================
            // ★ 파티클 카운터 TaskController 이벤트 연동
            // =========================================================

            // 측정 중 실시간으로 UI 업데이트 (파티클 통신 읽기 함수 호출)
            TaskController.OnParticleDataPolling += async () => await PollParticleDataAsync();
            // 측정이 완료되었을 때 최종 업데이트 및 리스트 추가
            TaskController.OnParticleMeasurementCompleted += async () => await FinalizeParticleMeasurementAsync();
            // 1. 측정 중 실시간으로 UI 업데이트
            TaskController.OnAirSamplerDataPolling += async () => await PollAirSamplerDataAsync();
            // 2. 측정이 완료되었을 때 최종 업데이트 및 리스트 추가
            TaskController.OnAirSamplingCompleted += async () => await FinalizeAirSamplingAsync();
            // =========================================================
            // ★ TaskController 이벤트와 ViewModel의 데이터 읽기 함수 연동
            // =========================================================
            #endregion

            #region [ Fastech I/O Control Initialization ]
            // 생성자(MainViewModel) 내부의 IO 초기화 및 페이징 커맨드를 아래와 같이 분리합니다.
            SaveIoCommand = new RelayCommand(_ => SaveIoParameters(showPopup: true));
            LoadIoCommand = new RelayCommand(_ => { LoadIoParameters(); MessageBox.Show("I/O 설정을 불러왔습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information); });

            // Input 페이징 커맨드
            PrevInputPageCommand = new RelayCommand(_ => { if (_currentInputPage > 0) { _currentInputPage--; UpdateInputPage(); } });
            NextInputPageCommand = new RelayCommand(_ => {
                int maxPage = (_totalInputPoints - 1) / _ioItemsPerPage;
                if (_currentInputPage < maxPage) { _currentInputPage++; UpdateInputPage(); }
            });

            // Output 페이징 커맨드
            PrevOutputPageCommand = new RelayCommand(_ => { if (_currentOutputPage > 0) { _currentOutputPage--; UpdateOutputPage(); } });
            NextOutputPageCommand = new RelayCommand(_ => {
                int maxPage = (_totalOutputPoints - 1) / _ioItemsPerPage;
                if (_currentOutputPage < maxPage) { _currentOutputPage++; UpdateOutputPage(); }
            });

            LoadIoParameters(); // 시작 시 IO 파라미터 로드

            // 커맨드 연결
            ConnectIoCommand = new RelayCommand(async o => await ConnectIoAsync(), o => !IsIoConnected);
            DisconnectIoCommand = new RelayCommand(o => DisconnectIo(), o => IsIoConnected);

            // 폴링 타이머 설정 (0.1초마다 입력 상태 모니터링)
            _ioTimer.Interval = TimeSpan.FromMilliseconds(100);
            _ioTimer.Tick += async (s, e) => await PollIoDataAsync();
            #endregion
        }


        // ★ 실시간 시계 바인딩용 프로퍼티
        private string _currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string CurrentDateTime
        {
            get => _currentDateTime;
            set { _currentDateTime = value; OnPropertyChanged(); }
        }

        private bool _isAlarmed;
        public bool IsAlarmed
        {
            get => _isAlarmed;
            set { _isAlarmed = value; OnPropertyChanged(); }
        }

        // ★ Servo 상태 Property 추가
        private bool _isServoOn;
        public bool IsServoOn
        {
            get => _isServoOn;
            set { _isServoOn = value; OnPropertyChanged(); }
        }
    }
}