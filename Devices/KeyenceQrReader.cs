using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NovaniX_EM2.Devices
{
    public class KeyenceQrReader : IDisposable
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;
        private FileSystemWatcher? _fileWatcher;

        // 비동기 콜백 이벤트
        public event Action<string>? OnDataReceived;
        public event Action<string>? OnImageReceived;
        public event Action<string>? OnError;

        public bool IsConnected => _tcpClient?.Connected == true;

        // 1. 소켓 연결 (비동기)
        public async Task<bool> ConnectAsync(string ip, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);
                _stream = _tcpClient.GetStream();
                _cts = new CancellationTokenSource();

                // 백그라운드 수신 대기 시작
                _ = ReceiveLoopAsync(_cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"연결 실패: {ex.Message}");
                return false;
            }
        }

        // 2. 데이터 수신 루프
        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (!token.IsCancellationRequested && _stream != null)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break; // 연결 끊김

                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    if (!string.IsNullOrEmpty(data))
                    {
                        OnDataReceived?.Invoke(data);
                    }
                }
            }
            catch (Exception) { /* 수신 오류 무시 */ }
            finally { Disconnect(); }
        }

        // 3. FTP 수신 폴더 감시 (이미지 자동 표출)
        public void StartImageMonitor(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;

            _fileWatcher?.Dispose();
            _fileWatcher = new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*", // 필터 (보통 jpg, bmp 등)
                EnableRaisingEvents = true
            };

            _fileWatcher.Created += (s, e) =>
            {
                if (e.FullPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    e.FullPath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                {
                    // 파일이 완전히 쓰여질 때까지 약간의 지연 후 이벤트 발생
                    Task.Delay(200).ContinueWith(_ => OnImageReceived?.Invoke(e.FullPath));
                }
            };
        }

        // 4-1. 트리거 ON (LON 명령어 전송)
        public async Task<bool> TriggerOnAsync()
        {
            if (!IsConnected || _stream == null) return false;
            try
            {
                byte[] cmd = Encoding.ASCII.GetBytes("LON\r");
                await _stream.WriteAsync(cmd, 0, cmd.Length);
                return true;
            }
            catch { return false; }
        }

        // 4-2. 트리거 OFF (LOFF 명령어 전송)
        public async Task<bool> TriggerOffAsync()
        {
            if (!IsConnected || _stream == null) return false;
            try
            {
                byte[] cmd = Encoding.ASCII.GetBytes("LOFF\r");
                await _stream.WriteAsync(cmd, 0, cmd.Length);
                return true;
            }
            catch { return false; }
        }

        // 5. 연결 해제
        public void Disconnect()
        {
            _cts?.Cancel();
            _stream?.Close();
            _tcpClient?.Close();
            _tcpClient = null;
        }

        public void Dispose()
        {
            Disconnect();
            _fileWatcher?.Dispose();
        }
    }
}