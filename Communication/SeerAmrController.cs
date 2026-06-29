using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NovaniX_EM2.Communication
{
    public class SeerAmrController : INotifyPropertyChanged
    {
        // 글로벌 싱글톤 인스턴스
        private static SeerAmrController _instance;
        public static SeerAmrController Instance => _instance ?? (_instance = new SeerAmrController());

        private TcpClient _statusClient;
        private NetworkStream _statusStream;
        private TcpClient _controlClient;
        private NetworkStream _controlStream;
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

        private string _logMessage = "";
        public string LogMessage
        {
            get => _logMessage;
            set { _logMessage = value; OnPropertyChanged(); }
        }

        private SeerAmrController() { }

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
            byte[] headerBuffer = new byte[16];
            try
            {
                while (IsStatusConnected)
                {
                    int bytesRead = await _statusStream.ReadAsync(headerBuffer, 0, 16);
                    if (bytesRead < 16) break;
                    if (headerBuffer[0] != 0x5A) continue;

                    uint dataLength = ParseBigEndianUInt32(headerBuffer, 4);
                    ushort apiType = ParseBigEndianUInt16(headerBuffer, 8);

                    byte[] jsonBuffer = new byte[dataLength];
                    int totalRead = 0;
                    while (totalRead < dataLength)
                    {
                        int read = await _statusStream.ReadAsync(jsonBuffer, totalRead, (int)dataLength - totalRead);
                        if (read == 0) throw new Exception("Socket Closed");
                        totalRead += read;
                    }

                    string jsonResponse = Encoding.UTF8.GetString(jsonBuffer);

                    if (apiType == 1000) // 공통 상태 정보 파싱
                    {
                        using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                        {
                            JsonElement root = doc.RootElement;
                            // 기존 코드
                            // Application.Current.Dispatcher.Invoke(() =>
                            // 변경 후 코드 (System.Windows.를 앞에 붙여줍니다)
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (root.TryGetProperty("battery_level", out JsonElement batEl))
                                    BatteryLevel = batEl.GetDouble() * 100;
                                if (root.TryGetProperty("x", out JsonElement xEl)) PosX = xEl.GetDouble();
                                if (root.TryGetProperty("y", out JsonElement yEl)) PosY = yEl.GetDouble();
                            });
                        }
                    }
                }
            }
            catch { IsStatusConnected = false; }
        }

        public void AddLog(string msg)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessage = $"[{DateTime.Now:HH:mm:ss}] {msg}\n" + LogMessage;
            });
        }

        #region Big-Endian Helpers
        private byte[] ToBigEndian(ushort value) { var b = BitConverter.GetBytes(value); if (BitConverter.IsLittleEndian) Array.Reverse(b); return b; }
        private byte[] ToBigEndian(uint value) { var b = BitConverter.GetBytes(value); if (BitConverter.IsLittleEndian) Array.Reverse(b); return b; }
        private uint ParseBigEndianUInt32(byte[] buf, int idx) { byte[] b = new byte[4]; Buffer.BlockCopy(buf, idx, b, 0, 4); if (BitConverter.IsLittleEndian) Array.Reverse(b); return BitConverter.ToUInt32(b, 0); }
        private ushort ParseBigEndianUInt16(byte[] buf, int idx) { byte[] b = new byte[2]; Buffer.BlockCopy(buf, idx, b, 0, 2); if (BitConverter.IsLittleEndian) Array.Reverse(b); return BitConverter.ToUInt16(b, 0); }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}