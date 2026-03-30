using NovaniX_EM2.Helpers;
using NovaniX_EM2.Devices;
using NovaniX_EM2.ViewModels;
using NovaniX_EM2.Views; // PetriEmptyDialog 호출을 위해 추가
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel; // ObservableCollection 처리를 위해 추가
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows; // Visibility 처리를 위해 추가
using System.Windows.Input;

namespace NovaniX_EM2.Controllers
{
    // --- 상태 및 공정 Step Enum ---
    public enum ProcessStep
    {
        Ready,
        Particle_Measurement,
        Petri_Loading,
        QRTable_Loading,
        QR_Reading,
        PetriToSampler_Loading,
        SamplerCap_Loading,
        Air_Sampling,
        SamplerCap_Unloading,
        SamplerToPetri_Unloading,
        Completed,
        Error,
        Stop
    }

    public enum MachineState
    {
        Idle,
        Running,
        Paused,
        Stopped,
        Error
    }

    // --- 공정 제어 전담 클래스 ---
    public class MainTaskController : INotifyPropertyChanged
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);

        // UI 바인딩용 이벤트 구현
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- UI 바인딩 프로퍼티 ---
        private ProcessStep _currentStep = ProcessStep.Ready;
        public ProcessStep CurrentStep
        {
            get => _currentStep;
            set { _currentStep = value; OnPropertyChanged(); }
        }

        private MachineState _currentState = MachineState.Idle;
        public MachineState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(StartStopToggleIsChecked));
            }
        }

        private string _currentTaskName = "대기 중";
        public string CurrentTaskName
        {
            get => _currentTaskName;
            set { _currentTaskName = value; OnPropertyChanged(); }
        }

        private int _progressRate = 0;
        public int ProgressRate
        {
            get => _progressRate;
            set { _progressRate = value; OnPropertyChanged(); }
        }

        private string _recentAlarm = "알람 없음";
        public string RecentAlarm
        {
            get => _recentAlarm;
            set { _recentAlarm = value; OnPropertyChanged(); }
        }

        // 상태 확인용 프로퍼티
        public bool IsRunning => CurrentState == MachineState.Running || CurrentState == MachineState.Paused;

        // 토글 버튼 상태 (XAML에서 강제 변경 시 오류 방지를 위해 빈 set 추가)
        public bool StartStopToggleIsChecked
        {
            get => IsRunning;
            set { /* 무시: 상태는 로직에서만 제어함 */ }
        }

        // =========================================================
        // ★ Petri Dish UI 바인딩 및 시각화용 컬렉션 추가
        // =========================================================
        private int _initialPetriLoadCount = 14;
        public int InitialPetriLoadCount
        {
            get => _initialPetriLoadCount;
            set
            {
                // ★ 14 초과 입력 시 에러 메세지 출력 및 14로 고정
                if (value > 14)
                {
                    System.Windows.MessageBox.Show("Loading Tray 수량은 최대 14개까지만 입력 가능합니다.", "입력 오류", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    _initialPetriLoadCount = 14;
                }
                // ★ 1 미만 (0 이하) 입력 시 에러 메세지 출력 및 1로 고정
                else if (value <= 0)
                {
                    System.Windows.MessageBox.Show("수량은 1개 이상 입력해야 합니다.", "입력 오류", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    _initialPetriLoadCount = 1;
                }
                else
                {
                    _initialPetriLoadCount = value;
                }

                OnPropertyChanged();

                // ★ 장비가 대기(Idle) 상태일 때 수량을 변경하면, 
                // 즉시 남은 수량과 UI 시각화 기둥에 반영되도록 동기화
                if (CurrentState == MachineState.Idle)
                {
                    Petri_LoadCount = _initialPetriLoadCount;
                    Petri_UnloadCount = 0;
                }
            }
        }

        private int _petri_LoadCount = 14;
        public int Petri_LoadCount
        {
            get => _petri_LoadCount;
            set { _petri_LoadCount = value; OnPropertyChanged(); UpdateVisualPetris(); }
        }

        private int _petri_UnloadCount = 0;
        public int Petri_UnloadCount
        {
            get => _petri_UnloadCount;
            set { _petri_UnloadCount = value; OnPropertyChanged(); UpdateVisualPetris(); }
        }

        public ObservableCollection<Visibility> LoadPetriVisibilities { get; } = new ObservableCollection<Visibility>();
        public ObservableCollection<Visibility> UnloadPetriVisibilities { get; } = new ObservableCollection<Visibility>();
        // =========================================================
        // ★ 추가: 다축(Multi-Axis) AZ 모터 및 SD Data 관리 딕셔너리
        // =========================================================

        public Dictionary<int, OrientalAzMotorDevice> AzMotors { get; set; } = new Dictionary<int, OrientalAzMotorDevice>();
        public Dictionary<int, List<TargetPositionItem>> AxisSdData { get; set; } = new Dictionary<int, List<TargetPositionItem>>();

        // MainViewModel에서 주입받을 그리퍼 인스턴스
        public RGIC100_Motion? GripperDevice { get; set; }

        // =========================================================
        // ★ 추가: MainViewModel에서 주입받을 PMS 장비 인스턴스
        // =========================================================
        public PMS_ParticleControl? ParticleDevice { get; set; }
        public PMS_MiniCaptControl? AirSamplerDevice { get; set; }

        public event Action? OnParticleDataPolling;
        public event Action? OnParticleMeasurementCompleted;
        public event Action? OnAirSamplerDataPolling;
        public event Action? OnAirSamplingCompleted;
        // =========================================================

        // --- Commands ---
        public ICommand InitializeCommand { get; }
        public ICommand ToggleStartStopCommand { get; }
        public ICommand PauseResumeCommand { get; }

        public MainTaskController()
        {
            InitializeCommand = new RelayCommand(_ => ExecuteInitialize());
            ToggleStartStopCommand = new RelayCommand(_ => ExecuteToggleStartStop());
            PauseResumeCommand = new RelayCommand(_ => ExecutePauseResume(), _ => IsRunning);

            // ★ UI 시각화를 위해 14개의 항목을 미리 초기화합니다.
            for (int i = 0; i < 14; i++)
            {
                LoadPetriVisibilities.Add(Visibility.Visible);
                UnloadPetriVisibilities.Add(Visibility.Hidden);
            }
            UpdateVisualPetris();
        }

        // ★ Load, Unload 카운트에 따라 UI의 기둥 속 디쉬 표시/숨김을 업데이트하는 함수
        private void UpdateVisualPetris()
        {
            // UI를 조작하는 ObservableCollection은 반드시 UI 스레드(Dispatcher)에서 갱신해야 합니다.
            if (System.Windows.Application.Current == null) return;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // 값 타입 인덱스 수정 에러를 방지하기 위해 완전히 지우고 다시 그립니다.
                LoadPetriVisibilities.Clear();
                UnloadPetriVisibilities.Clear();

                for (int i = 0; i < 14; i++)
                {
                    // 인덱스 0이 화면의 맨 위(Top)를 의미합니다.
                    LoadPetriVisibilities.Add(i >= (14 - _petri_LoadCount) ? Visibility.Visible : Visibility.Hidden);
                    UnloadPetriVisibilities.Add(i >= (14 - _petri_UnloadCount) ? Visibility.Visible : Visibility.Hidden);
                }
            });
        } 
        
        // --- 제어 메서드 ---
        // ★ 내부 변수 초기화 전용 함수 (동작 시작 시 호출)
        private void ResetStateVariables()
        {
            CurrentStep = ProcessStep.Ready;
            Petri_LoadCount = InitialPetriLoadCount; // ★ 입력된 초기값 반영
            Petri_UnloadCount = 0;                   // ★ Unload는 0으로 초기화
            ProgressRate = 0;
            RecentAlarm = "알람 없음";
        }

        // ★ 장비 초기화 버튼 동작 (Z축 구동 포함)
        private async void ExecuteInitialize()
        {
            try
            {
                CurrentState = MachineState.Running; // 이동 중 조작 방지
                ResetStateVariables();
                CurrentTaskName = "장비 초기화 진행 중 (Z축 이동)...";

                // Load/Unload Petri Z축 초기화 (CancellationToken.None 사용)
                await MoveAxisToSdDataAsync(3, 1, CancellationToken.None); // SlaveId 3번 축(Unload Petri Z)의 SD Data No.1 위치
                await MoveAxisToSdDataAsync(4, 0, CancellationToken.None); // SlaveId 4번 축(Load Petri Z)의 SD Data No.0 위치

                // ★ 추가: 입력된 수량(InitialPetriLoadCount)에 따라 Load Z축(4번 축) 보상 이동 
                int emptySlots = 14 - InitialPetriLoadCount;
                if (emptySlots > 0 && emptySlots <= 14)
                {
                    for (int i = 0; i < emptySlots; i++)
                    {
                        await MoveAxisToSdDataAsync(4, 2, CancellationToken.None); // SD Data No.2 반복 호출
                    }
                }

                // Load/Unload Cap Z축 초기화
                await MoveAxisToSdDataAsync(6, 1, CancellationToken.None); // SlaveId 6번 축(Sampler Cap Load Z)의 SD Data No.1 위치
                await MoveAxisToSdDataAsync(7, 1, CancellationToken.None); // SlaveId 7번 축(Sampler Cap UnLoad Z)의 SD Data No.1 위치

                CurrentState = MachineState.Idle;
                CurrentTaskName = "장비 초기화 완료";
            }
            catch (Exception ex)
            {
                CurrentState = MachineState.Error;
                RecentAlarm = $"[초기화 에러] {ex.Message}";
                CurrentTaskName = "초기화 실패";
            }
        }

        private async void ExecuteToggleStartStop()
        {
            if (IsRunning)
            {
                _cancellationTokenSource?.Cancel();
                _pauseEvent.Set();
                CurrentState = MachineState.Stopped;
                CurrentStep = ProcessStep.Stop;
                CurrentTaskName = "장비 정지됨";
            }
            else
            {
                // ★ 동작 시작 시에는 물리적 축 이동 대신 변수만 초기화
                ResetStateVariables();
                CurrentState = MachineState.Running;
                _cancellationTokenSource = new CancellationTokenSource();
                _pauseEvent.Set();

                try
                {
                    await Task.Run(() => MainTaskProcessAsync(_cancellationTokenSource.Token));
                }
                catch (OperationCanceledException)
                {
                    CurrentTaskName = "작업이 정지되었습니다.";
                }
                catch (Exception ex)
                {
                    CurrentState = MachineState.Error;
                    RecentAlarm = $"[에러] {ex.Message}";
                    CurrentTaskName = "알람 발생으로 인한 정지";
                }
            }
        }

        private void ExecutePauseResume()
        {
            if (CurrentState == MachineState.Running)
            {
                CurrentState = MachineState.Paused;
                _pauseEvent.Reset(); // 쓰레드 멈춤
                CurrentTaskName = $"{CurrentStep} (일시정지 중)";
            }
            else if (CurrentState == MachineState.Paused)
            {
                CurrentState = MachineState.Running;
                CurrentTaskName = $"{CurrentStep} (진행 중)";
                _pauseEvent.Set(); // 쓰레드 재개
            }
        }

        // --- Main Task 제어 ---
        private async Task MainTaskProcessAsync(CancellationToken token)
        {
            var steps = new[]
            {
                ProcessStep.Particle_Measurement,
                ProcessStep.Petri_Loading,
                ProcessStep.QRTable_Loading,
                ProcessStep.QR_Reading,
                ProcessStep.PetriToSampler_Loading,
                ProcessStep.SamplerCap_Loading,
                ProcessStep.Air_Sampling,
                ProcessStep.SamplerCap_Unloading,
                ProcessStep.SamplerToPetri_Unloading,
                ProcessStep.Completed
            };

            // ★ 토글(정지) 버튼이 눌려 취소 요청이 들어오기 전까지 무한 반복
            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < steps.Length; i++)
                {
                    token.ThrowIfCancellationRequested(); // 정지 시 즉시 탈출
                    _pauseEvent.Wait(token);              // 일시 정지 시 대기

                    CurrentStep = steps[i];
                    CurrentTaskName = CurrentStep.ToString();
                    ProgressRate = (int)((double)i / steps.Length * 100);

                    await ExecuteSubTaskAsync(CurrentStep, token);
                }

                // 한 사이클이 끝났을 때의 처리 (선택 사항)
                ProgressRate = 100;
                CurrentTaskName = "1 사이클 완료, 곧 다음 사이클을 시작합니다.";

                // 사이클 간 너무 빠르게 도는 것을 방지 (예: 1초 대기)
                await Task.Delay(1000, token);

                // 다음 사이클 시작을 위해 프로그레스바 초기화
                ProgressRate = 0;
            }
        }

        private async Task ExecuteSubTaskAsync(ProcessStep step, CancellationToken token)
        {
            // 실제 장비 구동 로직이 들어갈 곳
            switch (step)
            {
                case ProcessStep.Particle_Measurement:
                    await RunParticleMeasurementTaskAsync(token);
                    break;
                case ProcessStep.Petri_Loading:
                    await RunPetriLoadingTaskAsync(token);
                    break;
                case ProcessStep.QRTable_Loading:
                    await RunQRTableLoadingTaskAsync(token);
                    break;
                case ProcessStep.QR_Reading:
                    await RunQRCodeReadingTaskAsync(token);
                    break;
                case ProcessStep.PetriToSampler_Loading:
                    await RunPetriToSamplerLoadingTaskAsync(token);
                    break;
                case ProcessStep.SamplerCap_Loading:
                    await RunSamplerCapLoadingTaskAsync(token);
                    break;
                case ProcessStep.Air_Sampling:
                    await RunAirSamplingTaskAsync(token);
                    break;
                case ProcessStep.SamplerCap_Unloading:
                    await RunSamplerCapUnloadingTaskAsync(token);
                    break;
                case ProcessStep.SamplerToPetri_Unloading:
                    await MoveSamplerToPetriUnloadTaskAsync(token);
                    break;
                case ProcessStep.Completed:
                    await RunCompleteInitTaskAsync(token);

                    // ★ Petri_LoadCount 소진 시 팝업 로직 처리
                    if (Petri_LoadCount <= 0)
                    {
                        await MoveAxisToSdDataAsync(4, 0, token); // 대기 위치로 이동

                        bool isResumed = false;

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            var dialog = new PetriEmptyDialog();
                            if (dialog.ShowDialog() == true)
                            {
                                Petri_LoadCount = InitialPetriLoadCount; // UI 입력값으로 카운트 초기화
                                Petri_UnloadCount = 0; // Unload 적재 카운트 초기화
                                isResumed = true;
                            }
                        });

                        if (!isResumed)
                        {
                            _cancellationTokenSource?.Cancel();
                            throw new OperationCanceledException("Petri Dish 보충 대기 중 정지됨");
                        }

                        // ★ 메세지창 재가동 시 다시 채워진 갯수만큼 Z축 보상 이동 반복
                        int emptySlots = 14 - InitialPetriLoadCount;
                        if (emptySlots > 0 && emptySlots <= 14)
                        {
                            for (int i = 0; i < emptySlots; i++)
                            {
                                await MoveAxisToSdDataAsync(4, 2, token);
                            }
                        }
                    }
                    break;
                default:
                    await Task.Delay(1000, token);
                    break;
            }
        }

        // ====================================================================
        // ▼ [AZ 모터 베이스 제어 함수] ▼
        // ====================================================================

        private async Task MoveAxisToSdDataAsync(int slaveId, int sdDataNo, CancellationToken token, bool CheckDone = true)
        {
            if (!AzMotors.ContainsKey(slaveId)) throw new Exception($"축 {slaveId} 모터가 연결되지 않았습니다.");
            if (!AxisSdData.ContainsKey(slaveId) || AxisSdData[slaveId].Count <= sdDataNo)
                throw new Exception($"축 {slaveId}의 SD 데이터({sdDataNo}번)를 찾을 수 없습니다.");

            var motor = AzMotors[slaveId];
            var sdData = AxisSdData[slaveId][sdDataNo];

            int targetPos = sdData.Value;

            await motor.MoveAbsoluteAsync(sdDataNo, targetPos, sdData.Velocity);

            if (CheckDone)
                await WaitForAxisCompletionAsync(motor, targetPos, token);
        }

        private async Task MoveAxisTargetPosChangeAsync(int slaveId, int sdDataNo, int ChangePos, CancellationToken token)
        {
            if (!AzMotors.ContainsKey(slaveId)) throw new Exception($"축 {slaveId} 모터가 연결되지 않았습니다.");
            if (!AxisSdData.ContainsKey(slaveId) || AxisSdData[slaveId].Count <= sdDataNo)
                throw new Exception($"축 {slaveId}의 SD 데이터({sdDataNo}번)를 찾을 수 없습니다.");

            var motor = AzMotors[slaveId];
            var sdData = AxisSdData[slaveId][sdDataNo];

            await motor.MoveAbsoluteAsync(sdDataNo, ChangePos, sdData.Velocity);
            await WaitForAxisCompletionAsync(motor, ChangePos, token);
        }

        private async Task StopAxisAsync(int slaveId)
        {
            if (AzMotors.ContainsKey(slaveId))
            {
                await AzMotors[slaveId].StopMotorAsync();
            }
        }

        private async Task<int> GetAxisCurrentPositionAsync(int slaveId)
        {
            if (AzMotors.ContainsKey(slaveId))
            {
                return await AzMotors[slaveId].GetCurrentPositionAsync();
            }
            return 0;
        }

        private async Task MoveToLoadingPetriPickUpTaskAsync(int Step, CancellationToken token)
        {
            if (Step == 1)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 2, token);
            }
            else if (Step == 2)
            {
                await MoveAxisToSdDataAsync(2, 2, token);
                await MoveAxisToSdDataAsync(1, 2, token);
            }
            else
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 2, token);
            }
        }

        private async Task MoveQRTableTaskAsync(int Step, CancellationToken token)
        {
            if (Step == 1)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 5, token);
            }
            else if (Step == 2)
            {
                await MoveAxisToSdDataAsync(2, 5, token);
                await MoveAxisToSdDataAsync(1, 5, token);
            }
            else if (Step == 3)
            {
                await MoveAxisToSdDataAsync(2, 6, token);
                await MoveAxisToSdDataAsync(1, 5, token);
            }
            else
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 5, token);
            }
        }

        private async Task MoveQRTableTurnTaskAsync(CancellationToken token, bool CheckDone = false)
        {
            if (CheckDone == true)
                await MoveAxisToSdDataAsync(5, 0, token, CheckDone);
            else
                await MoveAxisToSdDataAsync(5, 1, token, CheckDone);
        }

        private async Task MoveQRTableToSamplerTaskAsync(int Step, CancellationToken token)
        {
            if (Step == 1)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 6, token);
            }
            else if (Step == 2)
            {
                await MoveAxisToSdDataAsync(1, 6, token);
                await MoveAxisToSdDataAsync(2, 6, token);
            }
            else if (Step == 3)
            {
                await MoveAxisTargetPosChangeAsync(2, 6, 7100, token);
            }
            else if (Step == 4)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 5, token);
                await MoveAxisTargetPosChangeAsync(2, 5, 17200, token);
            }
            else
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 5, token);
            }
        }

        private async Task MoveSamplerCapLoadTaskAsync(int Step, CancellationToken token)
        {
            if (Step == 1)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 3, token);
            }
            else if (Step == 2)
            {
                await MoveAxisToSdDataAsync(2, 3, token);
            }
            else if (Step == 3)
            {
                await MoveAxisToSdDataAsync(1, 6, token);
                await MoveAxisToSdDataAsync(2, 7, token);
            }
            else
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 0, token);
            }
        }

        private async Task MoveSamplerCapUnloadTaskAsync(int Step, CancellationToken token)
        {
            if (Step == 1)
            {
                await MoveAxisToSdDataAsync(1, 6, token);
                await MoveAxisToSdDataAsync(2, 7, token);
            }
            else if (Step == 2)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 4, token);
                await MoveAxisToSdDataAsync(2, 4, token);
            }
            else if (Step == 3)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 4, token);
            }
            else if (Step == 4)
            {
                await MoveAxisToSdDataAsync(1, 4, token);
                await MoveAxisToSdDataAsync(2, 4, token);
            }
            else if (Step == 5)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 3, token);
                await MoveAxisToSdDataAsync(2, 3, token);
            }
            else
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 5, token);
            }
        }

        private async Task MoveSamplerToPetriUnloadTaskAsync(int Step, CancellationToken token)
        {
            if (Step == 1)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 5, token);
                await MoveAxisTargetPosChangeAsync(2, 5, 17200, token);
            }
            else if (Step == 2)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 6, token);
                await MoveAxisTargetPosChangeAsync(2, 6, 7100, token);
            }
            else if (Step == 3)
            {
                await MoveAxisToSdDataAsync(2, 6, token);
            }
            else if (Step == 4)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 1, token);
            }
            else if (Step == 5)
            {
                await MoveAxisToSdDataAsync(2, 1, token);
                await MoveAxisToSdDataAsync(1, 1, token);
            }
            else if (Step == 6)
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 0, token);
            }
            else
            {
                await MoveAxisToSdDataAsync(2, 0, token);
                await MoveAxisToSdDataAsync(1, 2, token);
            }
        }

        private async Task MovePetriZAxisInitTaskAsync(CancellationToken token)
        {
            await MoveAxisToSdDataAsync(3, 2, token);
            await MoveAxisToSdDataAsync(4, 2, token);
        }

        private async Task MoveMachineTaskAsync(CancellationToken token)
        {
            await MoveAxisToSdDataAsync(3, 1, token);
            await MoveAxisToSdDataAsync(4, 0, token);
        }

        private async Task WaitForAxisCompletionAsync(OrientalAzMotorDevice motor, int targetPosition, CancellationToken token)
        {
            bool isCompleted = false;
            await Task.Delay(100, token); // 구동 개시 딜레이

            while (!isCompleted)
            {
                token.ThrowIfCancellationRequested();
                _pauseEvent.Wait(token);

                int currentPos = await motor.GetCurrentPositionAsync();

                if (Math.Abs(currentPos - targetPosition) <= 2)
                {
                    isCompleted = true;
                }

                if (!isCompleted)
                {
                    await Task.Delay(50, token);
                }
            }
        }

        // -------------------------------------------------------------------
        // ▼ 파티클 카운터 별도 동작 함수
        // -------------------------------------------------------------------
        private async Task RunParticleMeasurementTaskAsync(CancellationToken token)
        {
            if (ParticleDevice == null || !ParticleDevice.IsConnected)
            {
                throw new Exception("파티클 카운터가 연결되지 않았습니다.");
            }

            CurrentTaskName = "파티클 카운터 측정 중...";
            await ParticleDevice.StartMeasurementAsync();

            bool isCompleted = false;
            ushort previousStatus = 0;

            while (!isCompleted)
            {
                token.ThrowIfCancellationRequested();
                _pauseEvent.Wait(token);

                OnParticleDataPolling?.Invoke();

                ushort currentStatus = await ParticleDevice.ReadDeviceStatusAsync();
                bool isNormalState = (currentStatus == 0 || currentStatus == 1 || currentStatus == 257);

                if (!isNormalState)
                {
                    await ParticleDevice.StopMeasurementAsync();
                    throw new Exception($"파티클 카운터에서 비정상적인 상태 코드({currentStatus})가 감지되어 동작을 중단합니다.");
                }
                else if (previousStatus != 0 && currentStatus == 0)
                {
                    await ParticleDevice.StopMeasurementAsync();
                    isCompleted = true;
                }

                previousStatus = currentStatus;
                await Task.Delay(1000, token);
            }

            OnParticleMeasurementCompleted?.Invoke();
        }

        private async Task RunAirSamplingTaskAsync(CancellationToken token)
        {
            if (AirSamplerDevice == null || !AirSamplerDevice.IsConnected)
            {
                throw new Exception("에어 샘플러가 연결되지 않았습니다.");
            }

            CurrentTaskName = "에어 샘플링 동작 중...";
            await AirSamplerDevice.StartMeasurementAsync();

            bool isCompleted = false;
            ushort previousStatus = 0;

            while (!isCompleted)
            {
                token.ThrowIfCancellationRequested();
                _pauseEvent.Wait(token);

                OnAirSamplerDataPolling?.Invoke();

                ushort currentStatus = await AirSamplerDevice.ReadDeviceStatusAsync();
                bool isNormalState = (currentStatus == 0 || currentStatus == 1 || currentStatus == 257 || currentStatus == 513 || currentStatus == 769);

                if (!isNormalState)
                {
                    await AirSamplerDevice.StopMeasurementAsync();
                    throw new Exception($"에어 샘플러에서 비정상적인 상태 코드({currentStatus})가 감지되어 동작을 중단합니다.");
                }
                else if (previousStatus != 0 && currentStatus == 0)
                {
                    await AirSamplerDevice.StopMeasurementAsync();
                    isCompleted = true;
                }

                previousStatus = currentStatus;
                await Task.Delay(1000, token);
            }

            OnAirSamplingCompleted?.Invoke();
        }

        // ====================================================================
        // ▼ [각 스텝별 그리퍼 동작 시퀀스] ▼
        // ====================================================================

        private async Task RunPetriLoadingTaskAsync(CancellationToken token)
        {
            CurrentTaskName = "Petri Loading 시작 (그리퍼 Open)";
            await SetGripperOpenAsync(token);
            await MoveToLoadingPetriPickUpTaskAsync(1, token);

            CurrentTaskName = "Petri Pick Up, 그립퍼 Close";
            await MoveToLoadingPetriPickUpTaskAsync(2, token);
            await SetGripperCloseAsync(token);

            CurrentTaskName = "Petri Loading Wait Pos";
            await MoveToLoadingPetriPickUpTaskAsync(3, token);

            // ★ Petri Pick Up 완료 직후 Load 카운트 1 감소 (UI 트레이 상단부터 사라짐)
            if (Petri_LoadCount > 0)
                --Petri_LoadCount;
        }

        private async Task RunQRTableLoadingTaskAsync(CancellationToken token)
        {
            CurrentTaskName = "Petri QR Table Posion 이동";
            await MoveQRTableTaskAsync(1, token);

            CurrentTaskName = "Petri Place Down, 그립퍼 Open";
            await MoveQRTableTaskAsync(2, token);
            await SetGripperOpenAsync(token);

            CurrentTaskName = "QR_Table Wait Pos";
            await MoveQRTableTaskAsync(3, token);
        }

        private async Task RunQRCodeReadingTaskAsync(CancellationToken token)
        {
            CurrentTaskName = "Petri QR Code Reading";
            await MoveQRTableTurnTaskAsync(token, true);

            CurrentTaskName = "Petri QR Code Reading (Wait)";
            await MoveQRTableTurnTaskAsync(token, true);
        }

        private async Task RunPetriToSamplerLoadingTaskAsync(CancellationToken token)
        {
            CurrentTaskName = "Petri QR Table Pick Up Petri";
            await MoveQRTableTaskAsync(2, token);
            await SetGripperCloseAsync(token);
            await MoveQRTableTaskAsync(4, token);

            CurrentTaskName = "QR Table ▶▶▶ Sampler Pos 이동";
            await MoveQRTableToSamplerTaskAsync(1, token);

            CurrentTaskName = "Petri TO Sampler Place Down";
            await MoveQRTableToSamplerTaskAsync(2, token);
            await SetGripperOpenAsync(token);

            CurrentTaskName = "Petri Cap(Sampler Pos) ▶▶▶ QR Table 이동(Pick Up & Move)";
            await MoveQRTableToSamplerTaskAsync(3, token);
            await SetGripperCloseAsync(token);
            await MoveQRTableToSamplerTaskAsync(4, token);
            await SetGripperOpenAsync(token);
            await MoveQRTableToSamplerTaskAsync(5, token);
            CurrentTaskName = "Petri Cap 이동 완료";
        }

        private async Task RunSamplerCapLoadingTaskAsync(CancellationToken token)
        {
            CurrentTaskName = "Sampler Cap Loading ▶▶▶ CAP Unit 이동(Pick Up & Move)";
            await MoveSamplerCapLoadTaskAsync(1, token);

            await MoveSamplerCapLoadTaskAsync(2, token);
            await SetGripperCloseAsync(token);
            await MoveSamplerCapLoadTaskAsync(1, token);

            CurrentTaskName = "Sampler CAP Pos ▶▶▶ Sampler Pos 이동";
            await MoveSamplerCapLoadTaskAsync(3, token);
            await SetGripperOpenAsync(token);

            CurrentTaskName = "Sampling Wait Pos Move";
            await MoveSamplerCapLoadTaskAsync(4, token);
        }

        private async Task RunSamplerCapUnloadingTaskAsync(CancellationToken token)
        {
            CurrentTaskName = "Sampler Cap Unloading ▶▶▶ CAP Unit 이동(Pick Up & Move)";
            await MoveSamplerCapUnloadTaskAsync(1, token);
            await SetGripperCloseAsync(token);
            await MoveSamplerCapUnloadTaskAsync(2, token);
            await SetGripperOpenAsync(token);
            await MoveSamplerCapUnloadTaskAsync(3, token);

            CurrentTaskName = "Sampling Wait Pos Move";
            await MoveSamplerCapUnloadTaskAsync(6, token);
        }

        private async Task MoveSamplerToPetriUnloadTaskAsync(CancellationToken token)
        {
            CurrentTaskName = "QR Table(Petri Cap) ▶▶▶ Sampler Pos 이동";
            await MoveSamplerToPetriUnloadTaskAsync(1, token);
            await SetGripperCloseAsync(token);

            await MoveSamplerToPetriUnloadTaskAsync(2, token);
            await SetGripperOpenAsync(token);

            CurrentTaskName = "Petri Unload Sampler Pos ▶▶▶ Petri Unloading 작업";
            await MoveSamplerToPetriUnloadTaskAsync(3, token);
            await SetGripperCloseAsync(token);

            await MoveSamplerToPetriUnloadTaskAsync(4, token);
            await SetGripperRotateAsync(180, token);

            await MoveSamplerToPetriUnloadTaskAsync(5, token);
            await SetGripperOpenAsync(token);

            CurrentTaskName = "Petri Unloading 완료 Wait Pos 이동";
            await MoveSamplerToPetriUnloadTaskAsync(6, token);
            await SetGripperRotateAsync(0, token);

            // ★ Unloading 트레이에 적재 완료 직후 Unload 카운트 1 증가 (UI 트레이 하단부터 쌓임)
            if (Petri_UnloadCount < 14)
                ++Petri_UnloadCount;
        }

        private async Task RunCompleteInitTaskAsync(CancellationToken token)
        {
            CurrentTaskName = "Petri Data Init & 장비 초기화";

            if (Petri_LoadCount != 0)
            {
                await MovePetriZAxisInitTaskAsync(token);
            }

            CurrentTaskName = "Sampler CAP Pos ▶▶▶ Sampler Cap Load Pos 이동";
            await SetGripperOpenAsync(token);
            await SetGripperRotateAsync(0, token);
            await MoveSamplerCapUnloadTaskAsync(4, token);
            await SetGripperCloseAsync(token);
            await MoveSamplerCapUnloadTaskAsync(5, token);
            await SetGripperOpenAsync(token);
            await MoveSamplerToPetriUnloadTaskAsync(7, token);
        }

        // ====================================================================
        // ▼ [그리퍼 제어 및 대기 헬퍼 함수] ▼
        // ====================================================================

        private async Task SetGripperCloseAsync(CancellationToken token)
        {
            if (GripperDevice == null) throw new Exception("그리퍼가 연결되지 않았습니다.");
            await GripperDevice.MoveGripperAsync(0, 50, 50);
            await WaitForGripperCompletionAsync(token);
        }

        private async Task SetGripperOpenAsync(CancellationToken token)
        {
            if (GripperDevice == null) throw new Exception("그리퍼가 연결되지 않았습니다.");
            await GripperDevice.MoveGripperAsync(1000, 50, 50);
            await WaitForGripperCompletionAsync(token);
        }

        private async Task SetGripperRotateAsync(int targetAngle, CancellationToken token)
        {
            if (GripperDevice == null) throw new Exception("그리퍼가 연결되지 않았습니다.");
            await GripperDevice.RotateGripperAsync(targetAngle, 50, 50);
            await WaitForRotationCompletionAsync(token);
        }

        private async Task WaitForRotationCompletionAsync(CancellationToken token)
        {
            if (GripperDevice == null) throw new Exception("그리퍼가 연결되지 않았습니다.");

            bool isCompleted = false;
            while (!isCompleted)
            {
                token.ThrowIfCancellationRequested();
                _pauseEvent.Wait(token);

                try
                {
                    int rotStatus = await GripperDevice.ReadRotationStatusAsync();

                    if (rotStatus == 1)
                    {
                        isCompleted = true;
                    }
                    else if (rotStatus == 2 || rotStatus == 3)
                    {
                        throw new Exception($"그리퍼 회전 중 충돌/블로킹이 발생했습니다. (Status: {rotStatus})");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("블로킹")) throw;
                }

                if (!isCompleted)
                {
                    await Task.Delay(150, token);
                }
            }
        }

        private async Task WaitForGripperCompletionAsync(CancellationToken token)
        {
            if (GripperDevice == null) throw new Exception("그리퍼가 연결되지 않았습니다.");

            bool isCompleted = false;
            while (!isCompleted)
            {
                token.ThrowIfCancellationRequested();
                _pauseEvent.Wait(token);

                try
                {
                    int clampStatus = await GripperDevice.ReadClampStatusAsync();
                    if (clampStatus != 0)
                    {
                        isCompleted = true;
                    }
                }
                catch
                {
                    // 일시적인 통신 끊김이나 딜레이 시 무시하고 다음 주기에 재시도
                }

                if (!isCompleted)
                {
                    await Task.Delay(150, token);
                }
            }
        }

        private async Task WaitWithPauseAsync(int delayMilliseconds, CancellationToken token)
        {
            int waitedTime = 0;
            int stepMs = 100;

            while (waitedTime < delayMilliseconds)
            {
                token.ThrowIfCancellationRequested();
                _pauseEvent.Wait(token);

                await Task.Delay(stepMs, token);
                waitedTime += stepMs;
            }
        }
    }
}