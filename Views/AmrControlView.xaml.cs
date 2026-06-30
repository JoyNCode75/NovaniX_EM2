using System;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NovaniX_EM2.Communication;

namespace NovaniX_EM2.Views
{
    public partial class AmrControlView : System.Windows.Controls.UserControl
    {
        private bool _isLoaded = false;

        // 동시 다중 키 입력 처리를 위한 상태 변수
        private bool _isWPressed;
        private bool _isSPressed;
        private bool _isAPressed;
        private bool _isDPressed;
        private bool _isQPressed;
        private bool _isRPressed;

        public AmrControlView()
        {
            InitializeComponent();
            _isLoaded = true;
        }

        // [통합] AMR 연결/해제 토글 클릭 이벤트 핸들러
        private async void BtnToggleConnect_Click(object sender, RoutedEventArgs e)
        {
            if (SeerAmrController.Instance.IsControlConnected)
            {
                SeerAmrController.Instance.Disconnect();
            }
            else
            {
                try
                {
                    await SeerAmrController.Instance.ConnectAsync(txtAmrIp.Text);
                    this.Focus(); // 연결 완료 후 키보드 제어를 위해 포커스 부여
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("AMR 연결 실패: " + ex.Message, "에러", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // [오류 해결] cbDriveMode 선택 이벤트 핸들러 정상 작동
        private void CbDriveMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            // 인덱스 0: 차동구동 (Q, R 비활성화)
            if (cbDriveMode.SelectedIndex == 0)
            {
                btnJogLeft.IsEnabled = false;
                btnJogRight.IsEnabled = false;
            }
            else // 인덱스 1: 전방향구동 (Q, R 활성화)
            {
                btnJogLeft.IsEnabled = true;
                btnJogRight.IsEnabled = true;
            }
        }

        // ==============================================================
        // [다중 입력 처리 핵심 엔진]
        // ==============================================================
        private void UpdateJogFromKeys()
        {
            if (!_isLoaded || !SeerAmrController.Instance.IsControlConnected) return;

            double speed = 0.3;
            double rotSpeed = 0.2;
            double.TryParse(txtJogSpeed.Text, out speed);
            double.TryParse(txtJogRotSpeed.Text, out rotSpeed);

            double vx = 0.0, vy = 0.0, w = 0.0;

            if (_isWPressed) vx += speed;
            if (_isSPressed) vx -= speed;

            // Q, R 횡이동은 쌍타발 모드일 때만 적용
            if (cbDriveMode.SelectedIndex == 1)
            {
                if (_isQPressed) vy += speed;
                if (_isRPressed) vy -= speed;
            }

            if (_isAPressed) w += rotSpeed; // 좌회전 (반시계)
            if (_isDPressed) w -= rotSpeed; // 우회전 (시계)

            vx = Math.Clamp(vx, -speed, speed);
            vy = Math.Clamp(vy, -speed, speed);
            w = Math.Clamp(w, -rotSpeed, rotSpeed);

            var payload = new { vx = vx, vy = vy, w = w };
            string json = JsonSerializer.Serialize(payload);
            _ = SeerAmrController.Instance.SendControlPacketAsync(2010, json);
        }

        // Tag(버튼 정보)에 따라 상태 업데이트
        private void SetJogState(string tag, bool isPressed)
        {
            switch (tag)
            {
                case "Forward": _isWPressed = isPressed; break;
                case "Backward": _isSPressed = isPressed; break;
                case "Left": _isQPressed = isPressed; break;
                case "Right": _isRPressed = isPressed; break;
                case "TurnLeft": _isAPressed = isPressed; break;
                case "TurnRight": _isDPressed = isPressed; break;
            }
            UpdateJogFromKeys();
        }

        // =========================================================================
        // [마우스/키보드 이벤트 처리] - 모호성 에러 원천 차단 적용 완료
        // =========================================================================

        private void Jog_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string tag = (sender as System.Windows.Controls.Button)?.Tag?.ToString() ?? "";
            SetJogState(tag, true);
        }

        private void Jog_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string tag = (sender as System.Windows.Controls.Button)?.Tag?.ToString() ?? "";
            SetJogState(tag, false);
        }

        private void Jog_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            string tag = (sender as System.Windows.Controls.Button)?.Tag?.ToString() ?? "";
            SetJogState(tag, false);
        }

        private void UserControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.IsRepeat) return;
            if (System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox) return;

            bool handled = true;
            switch (e.Key)
            {
                case Key.W: _isWPressed = true; break;
                case Key.S: _isSPressed = true; break;
                case Key.A: _isAPressed = true; break;
                case Key.D: _isDPressed = true; break;
                case Key.Q: _isQPressed = true; break;
                case Key.R: _isRPressed = true; break;
                default: handled = false; break;
            }

            if (handled)
            {
                UpdateJogFromKeys();
                e.Handled = true;
            }
        }

        private void UserControl_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox) return;

            bool handled = true;
            switch (e.Key)
            {
                case Key.W: _isWPressed = false; break;
                case Key.S: _isSPressed = false; break;
                case Key.A: _isAPressed = false; break;
                case Key.D: _isDPressed = false; break;
                case Key.Q: _isQPressed = false; break;
                case Key.R: _isRPressed = false; break;
                default: handled = false; break;
            }

            if (handled)
            {
                UpdateJogFromKeys();
                e.Handled = true;
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Focus();
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _isWPressed = _isSPressed = _isAPressed = _isDPressed = _isQPressed = _isRPressed = false;

            await SeerAmrController.Instance.SendControlPacketAsync(2000, "{}");
            SeerAmrController.Instance.AddLog("[긴급 정지] API 2000 실행");
        }

        // =========================================================================
        // [맵핑 및 스케줄링 이벤트]
        // =========================================================================

        private void MapCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_isLoaded || !SeerAmrController.Instance.IsControlConnected) return;

            var clickPos = e.GetPosition(sender as System.Windows.Controls.Canvas);

            // TODO: UI 픽셀을 실제 단위(meter)로 변환하는 오프셋/배율은 상황에 맞게 수정
            double realX = (clickPos.X - 250) / 50.0;
            double realY = (clickPos.Y - 250) / 50.0;

            SeerAmrController.Instance.AddWaypointToSchedule(realX, realY, clickPos.X, clickPos.Y);
            SeerAmrController.Instance.AddLog($"스케줄 등록: X={realX:F2}m, Y={realY:F2}m");
        }

        private void BtnClearSchedule_Click(object sender, RoutedEventArgs e)
        {
            SeerAmrController.Instance.WaypointQueue.Clear();
            SeerAmrController.Instance.AddLog("스케줄 경로가 초기화되었습니다.");
        }

        private void BtnStartSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (SeerAmrController.Instance.WaypointQueue.Count == 0)
            {
                System.Windows.MessageBox.Show("맵을 클릭하여 먼저 목적지를 설정해주세요.", "알림");
                return;
            }
            SeerAmrController.Instance.StartNavigationSchedule();
        }
    }
}