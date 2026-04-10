using NovaniX_EM2.Devices;
using NovaniX_EM2.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NovaniX_EM2.ViewModels
{
    public class QrReaderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private KeyenceQrReader _qrReader;

        // ▼▼▼ MainViewModel에서 이 객체를 가져갈 수 있도록 public 속성 추가 ▼▼▼
        public KeyenceQrReader QrReaderInstance => _qrReader;

        // --- 바인딩 프로퍼티들 ---
        private string _readerIp = "192.168.100.100";
        public string ReaderIp { get => _readerIp; set { _readerIp = value; OnPropertyChanged(nameof(ReaderIp)); } }

        private int _readerPort = 9004; // 키엔스 기본 통신 포트
        public int ReaderPort { get => _readerPort; set { _readerPort = value; OnPropertyChanged(nameof(ReaderPort)); } }

        private string _ftpFolderPath = @"C:\FTP_Images"; // 로컬 FTP 서버 수신 폴더
        public string FtpFolderPath { get => _ftpFolderPath; set { _ftpFolderPath = value; OnPropertyChanged(nameof(FtpFolderPath)); } }

        private string _receivedDataText = string.Empty;
        public string ReceivedDataText { get => _receivedDataText; set { _receivedDataText = value; OnPropertyChanged(nameof(ReceivedDataText)); } }

        private bool _isConnected;
        public bool IsConnected { get => _isConnected; set { _isConnected = value; OnPropertyChanged(nameof(IsConnected)); } }

        // WPF Image 컨트롤에 바인딩할 소스
        private ImageSource? _receivedImage;
        public ImageSource? ReceivedImage { get => _receivedImage; set { _receivedImage = value; OnPropertyChanged(nameof(ReceivedImage)); } }

        // Commands
        public RelayCommand ConnectCommand { get; }
        public RelayCommand DisconnectCommand { get; }
        public RelayCommand TriggerCommand { get; }
        public RelayCommand SetFolderCommand { get; }

        private bool _isTriggerOn;
        public bool IsTriggerOn
        {
            get => _isTriggerOn;
            set
            {
                _isTriggerOn = value;
                OnPropertyChanged(nameof(IsTriggerOn));
                OnPropertyChanged(nameof(TriggerButtonText));
            }
        }

        // 상태에 따라 버튼에 표시될 텍스트 변경
        public string TriggerButtonText => IsTriggerOn ? "TRIGGER OFF (중지)" : "TRIGGER ON (시작)";

        public QrReaderViewModel()
        {
            _qrReader = new KeyenceQrReader();

            // 데이터 수신 이벤트 (UI 스레드에서 텍스트 업데이트)
            _qrReader.OnDataReceived += (data) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ReceivedDataText += $"[{timeStamp}] DATA: {data}\n";
                });
            };

            // 이미지 파일 생성 이벤트 (WPF 이미지 생성 및 바인딩)
            _qrReader.OnImageReceived += (imagePath) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // BitmapCacheOption.OnLoad를 사용하여 파일이 잠기는(Lock) 현상을 방지
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();

                        ReceivedImage = bmp;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"이미지 로드 실패: {ex.Message}");
                    }
                });
            };

            _qrReader.OnError += (msg) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show(msg, "QR Reader Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                });
            };

            // 커맨드 연결
            ConnectCommand = new RelayCommand(async _ => await ExecuteConnectAsync(), _ => !IsConnected);
            DisconnectCommand = new RelayCommand(_ => ExecuteDisconnect(), _ => IsConnected);

            // (기존 생성자 내부의 TriggerCommand 연결부를 아래 코드로 수정)
            TriggerCommand = new RelayCommand(async _ => await ExecuteTriggerToggleAsync(), _ => IsConnected);

            SetFolderCommand = new RelayCommand(_ =>
            {
                // 실제 배포 시에는 FolderBrowserDialog(WinForms 또는 패키지) 연동 권장
                if (!Directory.Exists(FtpFolderPath))
                {
                    Directory.CreateDirectory(FtpFolderPath);
                }
                _qrReader.StartImageMonitor(FtpFolderPath);
                System.Windows.MessageBox.Show($"폴더 감시를 시작합니다:\n{FtpFolderPath}");
            });
        }

        // (클래스 내부에 새로운 토글 메서드 추가)
        private async System.Threading.Tasks.Task ExecuteTriggerToggleAsync()
        {
            if (IsTriggerOn)
            {
                bool success = await _qrReader.TriggerOffAsync();
                if (success) IsTriggerOn = false;
            }
            else
            {
                bool success = await _qrReader.TriggerOnAsync();
                if (success) IsTriggerOn = true;
            }
        }

        private async System.Threading.Tasks.Task ExecuteConnectAsync()
        {
            IsConnected = await _qrReader.ConnectAsync(ReaderIp, ReaderPort);
            if (IsConnected)
            {
                // 연결 성공 시 지정된 폴더 감시도 함께 시작
                if (!Directory.Exists(FtpFolderPath)) Directory.CreateDirectory(FtpFolderPath);
                _qrReader.StartImageMonitor(FtpFolderPath);
            }
        }

        // (기존 ExecuteDisconnect 메서드 수정 - 연결 해제 시 트리거 상태도 초기화)
        private void ExecuteDisconnect()
        {
            _qrReader.Disconnect();
            IsConnected = false;
            IsTriggerOn = false; // 추가: 끊어질 때 트리거 상태 초기화
        }

        public void Dispose()
        {
            _qrReader?.Dispose();
        }
    }
}