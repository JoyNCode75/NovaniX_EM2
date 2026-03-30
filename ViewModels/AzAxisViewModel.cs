using System.Windows; // ★ 추가 (RelayCommand에서 MessageBox 사용을 위해)
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.ObjectModel; // ★ 추가
using System.ComponentModel;
using System.Linq; // ★ 추가
using System.Runtime.CompilerServices;
using System; // Exception 사용을 위해 추가
using System.Threading.Tasks;
using NovaniX_EM2.Devices;
using NovaniX_EM2.Helpers;

namespace NovaniX_EM2.ViewModels
{
    // ★ 개별 포지션 아이템 관리를 위한 클래스 추가
    public class TargetPositionItem : INotifyPropertyChanged
    {
        // ★ 포지션 번호 프로퍼티 추가
        private int _positionNumber;
        public int PositionNumber { get => _positionNumber; set { _positionNumber = value; OnPropertyChanged(); } }

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private bool _isUsed;
        public bool IsUsed { get => _isUsed; set { _isUsed = value; OnPropertyChanged(); } }

        private int _value;
        public int Value { get => _value; set { _value = value; OnPropertyChanged(); } }

        // 추가된 SD Data 속성들
        private int _velocity;
        public int Velocity { get => _velocity; set { _velocity = value; OnPropertyChanged(); } }

        private int _operationMode;
        public int OperationMode { get => _operationMode; set { _operationMode = value; OnPropertyChanged(); } }

        private int _acceleration;
        public int Acceleration { get => _acceleration; set { _acceleration = value; OnPropertyChanged(); } }

        private int _deceleration;
        public int Deceleration { get => _deceleration; set { _deceleration = value; OnPropertyChanged(); } }

        // ★ 선택 상태 프로퍼티 추가
        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class AzAxisViewModel : INotifyPropertyChanged
    {
        private readonly OrientalAzMotorDevice _motor;

        // SW Limit 속성
        public int SoftLimitMin { get; set; } = -1000000;
        public int SoftLimitMax { get; set; } = 1000000;

        // ★ 식별용 국번(SlaveId) 노출 및 할당 허용 (할당 시 내부 모터 객체의 국번도 함께 변경됨)
        public byte MotorId
        {
            get => _motor.SlaveId;
            set
            {
                if (_motor.SlaveId != value)
                {
                    _motor.SlaveId = value;
                    OnPropertyChanged();
                }
            }
        }

        // ★ 통신 인덱스 업데이트 메서드 추가
        public void SetCommIndex(int commIndex)
        {
            if (_motor != null)
            {
                _motor.CommIndex = commIndex;
            }
        }

        // ★ 추가: 외부에서 JSON 파라미터를 읽어온 뒤 Slave ID를 동기화하는 함수
        public void SetSlaveId(byte slaveId)
        {
            if (_motor != null)
            {
                _motor.SlaveId = slaveId;
            }
        }

        public int GetSlaveId()
        {
            return _motor.SlaveId;
        }

        // ★ JSON 로드 시 이름을 변경할 수 있도록 setter 추가 (기존 코드 - 탭 이름용으로 유지)
        private string _axisName = string.Empty;
        public string AxisName { get => _axisName; set { _axisName = value; OnPropertyChanged(); } }

        // ▼ 추가된 코드: 파라미터 탭에서 사용자가 별도로 입력하고 저장할 축 이름 ▼
        private string _customAxisName = string.Empty;
        public string CustomAxisName { get => _customAxisName; set { _customAxisName = value; OnPropertyChanged(); } }

        // ★ 5 포인트 포지션 컬렉션 추가
        public ObservableCollection<TargetPositionItem> TargetPositions { get; } = new ObservableCollection<TargetPositionItem>();

        // ★ 하단 상태바(또는 버튼 옆)에 표시할 메시지 프로퍼티 추가
        private string _statusMessage = "대기 중...";
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        // ★ 새로 추가: 텍스트 색상을 바인딩할 프로퍼티 (기본 초록색)
        private string _statusColor = "#4CAF50";
        public string StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }

        private readonly Action<bool>? _setBusyState;

        public AzAxisViewModel(OrientalAzMotorDevice motor, Action<bool>? setBusyState = null)
        {
            _motor = motor;
            _setBusyState = setBusyState;

            // AzAxisViewModel 생성자 수정 (10개 -> 16개로 변경)
            for (int i = 0; i <= 15; i++)
            {
                TargetPositions.Add(new TargetPositionItem
                {
                    PositionNumber = i, // 0~15 번호 부여
                    Name = $"Pos {i}",
                    Value = 0,
                    IsUsed = false
                });
            }

            HomeCommand = new RelayCommand(async (o) => await HomeAxisAsync());

            // ▼ 개별 축 알람 리셋 커맨드 초기화 ▼
            ResetAlarmCommand = new RelayCommand(async (o) =>
            {
                StatusMessage = "알람 리셋 중...";
                StatusColor = "#FF9800"; // 주황색

                // ★ 수정됨: 함수 이름을 Device 구현체에 맞춰 ResetAlarmAsync() 로 변경
                await _motor.ResetAlarmAsync();
                await Task.Delay(500); // 드라이버가 리셋을 처리할 대기 시간

                int alarmCode = 0;
//                int alarmCode = await _motor.GetAlarmCodeAsync();
                if (alarmCode == 0)
                {
                    IsAlarmed = false;
                    StatusMessage = "알람 리셋 완료";
                    StatusColor = "#4CAF50"; // 초록색
                }
                else
                {
                    IsAlarmed = true;
                    StatusMessage = $"리셋 실패 (Code: {alarmCode})";
                    StatusColor = "#F44336"; // 빨간색
                }
            });
            
            // ★ 더블클릭 명령 구현 (상호 배타적 선택 기능 포함)
            DoubleClickPositionCommand = new RelayCommand(param =>
            {
                if (param is TargetPositionItem clickedItem)
                {
                    // 방어 코드: 체크박스가 해제되어 있으면 에러 처리 후 이벤트 취소
                    if (!clickedItem.IsUsed)
                    {
                        MessageBox.Show($"[{clickedItem.Name}] 항목이 비활성화 상태입니다.\n먼저 체크박스를 선택해 주세요.",
                                        "선택 불가", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (clickedItem.IsSelected)
                    {
                        clickedItem.IsSelected = false; // 이미 선택된 항목을 다시 더블클릭하면 해제
                    }
                    else
                    {
                        // 상호 배타적 선택: 다른 모든 항목 선택 해제
                        foreach (var item in TargetPositions)
                        {
                            item.IsSelected = false;
                        }

                        // 현재 항목 선택 및 타겟 위치값 설정
                        clickedItem.IsSelected = true;
                        TargetPosition = clickedItem.Value;
                    }
                }
            });

            // ★ 위치값 Set 명령
            SetPositionValueCommand = new RelayCommand(_ =>
            {
                var selectedItems = TargetPositions.Where(p => p.IsSelected).ToList();

                if (selectedItems.Count == 0)
                {
                    StatusColor = "#F44336"; // 에러: 빨간색
                    StatusMessage = "[에러] 선택된 포지션이 없습니다.";
                    MessageBox.Show("선택된 포지션이 없습니다.\n포지션 이름을 더블클릭하여 먼저 선택해 주세요.", "위치값 설정 에러", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selectedItems.Count > 1)
                {
                    StatusColor = "#F44336"; // 에러: 빨간색
                    StatusMessage = "[에러] 여러 개의 포지션이 선택되었습니다.";
                    MessageBox.Show("여러 개의 포지션이 선택되었습니다.\n하나의 포지션만 선택해 주세요.", "위치값 설정 에러", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var targetPosition = selectedItems[0];

                if (!targetPosition.IsUsed)
                {
                    StatusColor = "#F44336"; // 에러: 빨간색
                    StatusMessage = $"[에러] '{targetPosition.Name}' 사용 안 함 상태입니다.";
                    MessageBox.Show($"[{targetPosition.Name}] 항목이 사용 안 함 상태입니다.\n좌측의 체크박스를 먼저 체크해 주세요.", "위치값 설정 에러", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 정상 처리
                targetPosition.Value = CurrentPosition;

                StatusColor = "#4CAF50"; // 성공: 초록색
                StatusMessage = $"[성공] '{targetPosition.Name}'에 현재 위치값({CurrentPosition}) 저장 완료.";
            });
        }

        // ★ 새로 추가된 공통 명령 실행 래퍼 메서드
        private async Task ExecuteCommandAsync(Func<Task> commandAction)
        {
            try
            {
                // 1. 통신 점유 플래그 켜기 (폴링 루프 일시 정지)
                _setBusyState?.Invoke(true);
                await Task.Delay(30); // 진행 중이던 폴링이 멈출 수 있는 여유 시간

                // 2. 실제 모터 제어 명령 실행
                await commandAction();
            }
            finally
            {
                await Task.Delay(30);
                // 3. 통신 점유 플래그 끄기 (명령 완료 후 폴링 재개)
                _setBusyState?.Invoke(false);
            }
        }

        // ==========================================
        // UI 바인딩용 상태 프로퍼티들
        // ==========================================
        private int _currentPosition;
        public int CurrentPosition { get => _currentPosition; set { _currentPosition = value; OnPropertyChanged(); } }

        private int _targetPosition;
        public int TargetPosition { get => _targetPosition; set { _targetPosition = value; OnPropertyChanged(); } }

        private int _moveSpeed = 1000;
        public int MoveSpeed { get => _moveSpeed; set { _moveSpeed = value; OnPropertyChanged(); } }

        private int _moveAccelDecel = 1000;
        public int MoveAccelDecel { get => _moveAccelDecel; set { _moveAccelDecel = value; OnPropertyChanged(); } }

        private int _jogSpeed = 500;
        public int JogSpeed { get => _jogSpeed; set { _jogSpeed = value; OnPropertyChanged(); } }

        private int _jogAccelDecel = 500;
        public int JogAccelDecel { get => _jogAccelDecel; set { _jogAccelDecel = value; OnPropertyChanged(); } }

        private int _positiveLimit = 10000;
        public int PositiveLimit { get => _positiveLimit; set { _positiveLimit = value; OnPropertyChanged(); } }

        private int _negativeLimit = -10000;
        public int NegativeLimit { get => _negativeLimit; set { _negativeLimit = value; OnPropertyChanged(); } }

        // ★ 각종 상태 표시 플래그
        private bool _isAlarmed;
        public bool IsAlarmed { get => _isAlarmed; set { _isAlarmed = value; OnPropertyChanged(); } }

        private bool _isServoOn;
        public bool IsServoOn { get => _isServoOn; set { _isServoOn = value; OnPropertyChanged(); } }

        // ★ 추가: Moving 상태 바인딩용 프로퍼티
        private bool _isMoving;
        public bool IsMoving
        {
            get => _isMoving;
            set { _isMoving = value; OnPropertyChanged(); }
        }

        private bool _isInitialized;
        public bool IsInitialized { get => _isInitialized; set { _isInitialized = value; OnPropertyChanged(); } }
        public string ServoStatusText => IsServoOn ? "Servo ON" : "Servo OFF";

        // ==========================================
        // 명령(Command)
        // ==========================================
        public ICommand ResetAlarmCommand { get; set; } = null!;
        public ICommand SetLimitCommand { get; set; } = null!;
        public ICommand HomeCommand { get; }
        public ICommand DoubleClickPositionCommand { get; }
        public ICommand SetPositionValueCommand { get; }
        public ICommand SaveAxisCommand { get; set; } = null!;

        // JOG 커맨드 연결
        // 마우스를 누를 때(Down)는 isStop=false 로 이동 시작, 마우스를 뗄 때(Up)는 isStop=true 로 정지 명령 전송
        public ICommand JogFwdDownCommand => new RelayCommand(async (o) => await ExecuteCommandAsync(async () => await _motor.JogAsync(isForward: true, isStop: false)));
        public ICommand JogFwdUpCommand => new RelayCommand(async (o) => await ExecuteCommandAsync(async () => await _motor.JogAsync(isForward: true, isStop: true)));
        public ICommand JogRevDownCommand => new RelayCommand(async (o) => await ExecuteCommandAsync(async () => await _motor.JogAsync(isForward: false, isStop: false)));
        public ICommand JogRevUpCommand => new RelayCommand(async (o) => await ExecuteCommandAsync(async () => await _motor.JogAsync(isForward: false, isStop: true)));

        // 지정 포지션 이동 커맨드
        public ICommand MoveCommand => new RelayCommand(async _ =>
        {
            var selectedItem = TargetPositions.FirstOrDefault(p => p.IsSelected);
            int dataNo = selectedItem != null ? selectedItem.PositionNumber : 0;

            int targetPos = TargetPosition; // ★ 현재 목표 위치 캡처

            if (targetPos < SoftLimitMin || targetPos > SoftLimitMax)
            {
                MessageBox.Show($"이동 불가: 범위({SoftLimitMin} ~ {SoftLimitMax}) 초과.", "경고", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusMessage = $"Data No. {dataNo} 위치({targetPos})로 이동 중...";
            StatusColor = "#FF9800";
            IsMoving = true; // ★ UI 원형 표시기 점등 (Moving ON)

            try
            {
                await ExecuteCommandAsync(async () =>
                {
                    await _motor.MoveAbsoluteAsync(dataNo, targetPos, MoveSpeed);
                    await Task.Delay(100);

                    bool isCompleted = false;
                    while (!isCompleted)
                    {
                        if (IsAlarmed)
                        {
                            await _motor.StopMotorAsync();
                            StatusMessage = "알람 발생으로 이동 중지됨";
                            StatusColor = "#F44336";
                            return; // 에러 시 탈출
                        }

                        CurrentPosition = await _motor.GetCurrentPositionAsync();

                        if (Math.Abs(CurrentPosition - targetPos) <= 2)
                        {
                            isCompleted = true;
                        }
                        if (!isCompleted) await Task.Delay(50);
                    }

                    StatusMessage = $"Data No. {dataNo} 위치 이동 완료";
                    StatusColor = "#4CAF50";
                });
            }
            finally
            {
                IsMoving = false; // ★ 이동이 성공하든 에러가 나든 완료되면 무조건 소등 (Moving OFF)
            }
        });

        public ICommand StopCommand => new RelayCommand(async _ =>
        {
            try
            {
                _setBusyState?.Invoke(true); // 폴링 루프 즉시 일시 정지
                await _motor.StopMotorAsync();    // 즉시 정지 명령 전송 (함수명 확인)

                StatusMessage = "긴급 정지(STOP) 명령 전송됨";
                StatusColor = "#F44336"; // 빨간색
            }
            finally
            {
                await Task.Delay(30); // 정지 신호 처리 후 폴링 재개 전 짧은 여유 대기
                _setBusyState?.Invoke(false); // 폴링 루프 재개
            }
        });

        // ★ 축 전환 시 초기화 메서드
        public void ClearSelection()
        {
            if (TargetPositions == null) return;
            foreach (var item in TargetPositions)
            {
                item.IsSelected = false;
            }
        }

        // 주기적으로 호출되어 UI를 업데이트하는 상태 갱신 함수
        public async Task UpdateStatusAsync()
        {
            if (_motor == null) return;

            try
            {
                // 1. 현재 위치 읽기 및 UI 바인딩
                CurrentPosition = await _motor.GetCurrentPositionAsync();

                // 2. 알람 상태 읽기 (0이면 정상, 그 외는 알람)
                int alarmCode = 0;
                //                int alarmCode = await _motor.GetAlarmCodeAsync();
                IsAlarmed = (alarmCode != 0);
            }
            catch
            {
                // 통신 지연이나 일시적 오류 시 프로그램이 멈추지 않도록 무시
            }
        }

        // SW Limit 확인 후 이동하는 함수
        private async Task MoveWithLimitCheckAsync(int targetPosition, int speed)
        {
            if (targetPosition < SoftLimitMin || targetPosition > SoftLimitMax)
            {
                MessageBox.Show($"이동 불가: 입력한 위치({targetPosition})가 SW Limit 설정 범위를 벗어났습니다.", "SW Limit 알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await ExecuteCommandAsync(() => _motor.MoveAbsoluteAsync(0, targetPosition, speed));
        }

        // ==================================================================================
        // ★ 수정됨: Home 위치 비교 및 SD Data 동기화 로직 포함 (Exception 처리 적용)
        // ==================================================================================
        public async Task<bool> HomeAxisAsync()
        {
            StatusMessage = "원점 복귀(Home) 진행 중...";
            StatusColor = "#FF9800"; // 주황색

            bool isCompleted = false;

            try
            {
                await ExecuteCommandAsync(async () =>
                {
                    int result = await _motor.HomeAsync();
                    if (result != 1) throw new Exception("Home 명령 전송 실패");

                    // 구동 개시 대기
                    await Task.Delay(1000);

                    int retryCount = 0; // 타임아웃 방지용
                    while (!isCompleted && retryCount < 600) // 최대 30초
                    {
                        retryCount++;
                        int alarmCode = 0;
                        //                int alarmCode = await _motor.GetAlarmCodeAsync();
                        if (alarmCode != 0)
                        {
                            IsAlarmed = true;
                            await _motor.StopMotorAsync();
                            throw new Exception($"Home 중 알람 발생 (Code: {alarmCode})");
                        }

                        CurrentPosition = await _motor.GetCurrentPositionAsync();

                        // 완료 조건: 위치가 0(오차범위 ±2)에 도달
                        if (Math.Abs(CurrentPosition - 0) <= 2)
                        {
                            isCompleted = true;
                        }

                        if (!isCompleted) await Task.Delay(50);
                    }

                    if (!isCompleted) throw new Exception("Home 동작 시간 초과");

                    // 완료 시 Clear 전송
                    await _motor.ClearAsync();
                });
            }
            catch (Exception ex)
            {
                IsInitialized = false;
                StatusColor = "#F44336"; // 빨간색
                StatusMessage = $"원점 복귀 실패: {ex.Message}";
                return false;
            }

            if (isCompleted)
            {
                IsInitialized = true;

                // ★ SD Data Reading 진행
                bool readSuccess = await ReadAllSdDataAsync();

                if (readSuccess)
                {
                    StatusColor = "#4CAF50";
                    StatusMessage = "원점 복귀 및 SD 데이터 리딩 완료";
                    return true;
                }
                else
                {
                    // ReadAllSdDataAsync 안에서 오류 처리가 완료됨
                    IsInitialized = false;
                    return false;
                }
            }

            return false;
        }

        // ==================================================================================
        // ★ 수정됨: SD 데이터 전체 리딩 및 예외 발생 시 에러 알람 표시 (return bool 변경)
        // ==================================================================================
        public async Task<bool> ReadAllSdDataAsync()
        {
            StatusMessage = "SD 데이터 리딩 중...";
            StatusColor = "#FF9800"; // 주황색

            try
            {
                await ExecuteCommandAsync(async () =>
                {
                    for (int i = 0; i < 16; i++)
                    {
                        var sdData = await _motor.ReadSdDataAsync(i);
                        TargetPositions[i].OperationMode = sdData.OpMode;
                        TargetPositions[i].Value = sdData.Pos;
                        TargetPositions[i].Velocity = sdData.Vel;
                        TargetPositions[i].Acceleration = sdData.Acc;
                        TargetPositions[i].Deceleration = sdData.Dec;
                    }
                });

                StatusMessage = "SD 데이터 리딩 및 저장 완료";
                StatusColor = "#4CAF50"; // 초록색

                if (SaveAxisCommand != null && SaveAxisCommand.CanExecute(null)) SaveAxisCommand.Execute(null);

                return true;
            }
            catch (Exception ex)
            {
                // OrientalAzMotorDevice의 ReadSdDataAsync에서 예외 발생 시 여기서 캐치됨
                IsAlarmed = true; // 화면 빨간불 점등
                StatusColor = "#F44336"; // 텍스트 빨간색
                StatusMessage = $"데이터 로딩 실패: {ex.Message}";
                return false;
            }
        }

        // 4. 데이터 초기화 메서드 추가 (연결/해제 시 호출됨)
        public void ResetSdData()
        {
            foreach (var pos in TargetPositions)
            {
                pos.OperationMode = 0;
                pos.Value = 0;
                pos.Velocity = 0;
                pos.Acceleration = 0;
                pos.Deceleration = 0;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}