using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NovaniX_EM2.ViewModels;

namespace NovaniX_EM2.Views
{
    public partial class MotionControlView : System.Windows.Controls.UserControl
    {
        public MotionControlView()
        {
            InitializeComponent();

        }

        // DataContext를 현재 선택된 AzAxisViewModel로 캐스팅하여 명령 실행
        // 반환 타입에 '?'를 추가하여 null 반환을 허용 (경고 해결)
        private AzAxisViewModel? GetCurrentAxisViewModel(object sender)
        {
            return (sender as FrameworkElement)?.DataContext as AzAxisViewModel;
        }

        private void BtnJogFwd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = GetCurrentAxisViewModel(sender);
            if (vm != null && vm.JogFwdDownCommand.CanExecute(null))
                vm.JogFwdDownCommand.Execute(null);
        }

        private void BtnJogFwd_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = GetCurrentAxisViewModel(sender);
            if (vm != null && vm.JogFwdUpCommand.CanExecute(null))
                vm.JogFwdUpCommand.Execute(null);
        }

        private void BtnJogRev_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = GetCurrentAxisViewModel(sender);
            if (vm != null && vm.JogRevDownCommand.CanExecute(null))
                vm.JogRevDownCommand.Execute(null);
        }

        private void BtnJogRev_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = GetCurrentAxisViewModel(sender);
            if (vm != null && vm.JogRevUpCommand.CanExecute(null))
                vm.JogRevUpCommand.Execute(null);
        }

        // ★ 축 Tab 전환 감지 (Unloaded 이벤트 대신 사용)
        private void AxisTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != sender) return; // 자식 TabControl의 이벤트는 무시

            // 1. TabSwitch는 비즈니스 로직(저장)과 분리하여 즉시 초기화
            // '다른 축으로 전환하면 초기화' 기능 구현
            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is AzAxisViewModel oldAxisVm)
            {
                oldAxisVm.ClearSelection();
            }

            if (e.AddedItems.Count > 0 && e.AddedItems[0] is AzAxisViewModel newAxisVm)
            {
                newAxisVm.ClearSelection();
            }
        }
    }
}