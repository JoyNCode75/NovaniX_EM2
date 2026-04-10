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

            // ★ 추가된 코드: 창이 완전히 로드된 직후 MainControlView를 강제로 첫 화면에 띄웁니다.
            this.Loaded += (s, e) =>
            {
                MainContent.Content = new Views.MainControlView();
            };
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

        // ▼ 메뉴바 확장 상태를 저장하는 변수
        // ★ 기본값을 축소 모드(false)로 변경
        private bool _isMenuExpanded = false;

        // ▼ 추가할 코드: 메뉴바 토글 버튼 클릭 이벤트
        private void BtnToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            _isMenuExpanded = !_isMenuExpanded;
            UpdateMenuState();
        }

        private void UpdateMenuState()
        {
            if (_isMenuExpanded)
            {
                // [확장 모드] : 넓이를 215로 늘림 (컨텐츠 200 + 버튼 15)
                MenuBorder.Width = 215;
                BtnToggleMenu.Content = "◀"; // 버튼 방향 반전

                // 텍스트 보이기
                TxtMenuMain.Visibility = Visibility.Visible;
                TxtMenuBio.Visibility = Visibility.Visible;
                TxtMenuMotion.Visibility = Visibility.Visible;
                TxtMenuEtc.Visibility = Visibility.Visible;
                TxtMenuSys.Visibility = Visibility.Visible;

                BtnExit.Content = "⏻ EXIT";
            }
            else
            {
                // [축소 모드] : 넓이를 65로 줄임 (아이콘 50 + 버튼 15)
                MenuBorder.Width = 65;
                BtnToggleMenu.Content = "▶"; // 버튼 방향 복구

                // 텍스트 숨김
                TxtMenuMain.Visibility = Visibility.Collapsed;
                TxtMenuBio.Visibility = Visibility.Collapsed;
                TxtMenuMotion.Visibility = Visibility.Collapsed;
                TxtMenuEtc.Visibility = Visibility.Collapsed;
                TxtMenuSys.Visibility = Visibility.Collapsed;

                BtnExit.Content = "⏻";
            }
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedItem is ListBoxItem selectedItem)
            {
                string? tag = selectedItem.Tag?.ToString();

                switch (tag)
                {
                    case "Main":
                        MainContent.Content = new Views.MainControlView();
                        break;
                    case "Bio":
                        MainContent.Content = new Views.BioControlView();
                        break;
                    case "Motion":
                        MainContent.Content = new Views.MotionControlView();
                        break;
                        // (Etc, System 부분은 작성하신 기존 코드 그대로 유지)
                }
            }

            // ★ 추가된 로직: 메뉴(아이콘/텍스트)를 클릭해서 화면이 전환되면 오버레이 메뉴를 자동으로 축소시킵니다.
            if (_isMenuExpanded)
            {
                _isMenuExpanded = false;
                UpdateMenuState();
            }
        }

        // ▼ 수정된 코드: Exit 버튼 클릭 시 MessageView 창을 띄워 확인 후 종료 ▼
        private void BtnExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 버튼 2개(확인, 취소)만 사용하는 모드로 호출합니다. 대기 버튼(2번째 파라미터)은 null을 줍니다.
            var msgView = new Views.MessageView(
                message: "프로그램을 종료하시겠습니까?",
                title: "종료 확인",
                btnConfirmText: "종료",
                btnWaitText: null!,
                btnCancelText: "취소"
            );

            msgView.ShowDialog();

            // 반환값이 0(확인/종료 버튼)일 경우에만 종료 수행
            if (msgView.Result == 0)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}