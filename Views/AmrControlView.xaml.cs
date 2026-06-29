using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using NovaniX_EM2.Communication;

namespace NovaniX_EM2.Views
{
    public partial class AmrControlView : System.Windows.Controls.UserControl
    {
        private bool _isLoaded = false;

        public AmrControlView()
        {
            InitializeComponent();
            _isLoaded = true;
        }

        private void CbDriveMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            // 인덱스 0: 차동구동 (Vy 비활성화), 1: 쌍타발 (Vy 활성화)
            if (cbDriveMode.SelectedIndex == 0)
            {
                if (sdVy != null) { sdVy.Value = 0; sdVy.IsEnabled = false; }
            }
            else
            {
                if (sdVy != null) sdVy.IsEnabled = true;
            }
        }

        private async void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isLoaded || !SeerAmrController.Instance.IsControlConnected) return;

            var payload = new
            {
                vx = sdVx.Value,
                vy = cbDriveMode.SelectedIndex == 0 ? 0 : sdVy.Value,
                w = sdW.Value
            };
            string json = JsonSerializer.Serialize(payload);
            await SeerAmrController.Instance.SendControlPacketAsync(2010, json);
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _isLoaded = false;
            sdVx.Value = 0; sdVy.Value = 0; sdW.Value = 0;
            _isLoaded = true;

            await SeerAmrController.Instance.SendControlPacketAsync(2000, "{}");
            SeerAmrController.Instance.AddLog("[긴급 정지] API 2000 실행");
        }
    }
}