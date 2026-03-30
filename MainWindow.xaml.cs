using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input; // MouseButtonEventArgs를 위해 필요합니다.
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NovaniX_EM2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // ▼ 추가할 코드: Exit 버튼 클릭 시 프로그램 종료 ▼
        private void BtnExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        // ▼ 추가된 코드: 로고 이미지를 더블 클릭했을 때만 최대화/복구 기능 수행 ▼
        private void Logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 1. 마우스 왼쪽 버튼 더블 클릭 감지 (전체 화면 <-> 기본 크기 토글)
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal; // 원래 크기로 복구
                }
                else
                {
                    this.WindowState = WindowState.Maximized; // 모니터 해상도에 맞춰 최대화
                }

                // 더블 클릭 이벤트가 부모(TopBar)로 전달되어 DragMove 오류가 발생하는 것을 방지
                e.Handled = true;
            }
        }

        // ▼ 수정된 코드: 상단 파란색 바(로고 포함)를 클릭해서 창을 드래그하여 이동하는 기능만 유지 ▼
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 마우스 왼쪽 버튼 클릭 누른 채로 드래그 이동
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}