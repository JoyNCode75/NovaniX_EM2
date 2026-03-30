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
    /// <summary>
    /// BioControlView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BioControlView : UserControl
    {
        public BioControlView()
        {
            InitializeComponent();
            // ★ UI와 MainViewModel을 런타임(실제 실행 시)에 연결해 주는 핵심 코드
//            this.DataContext = new MainViewModel();
        }
    }
}
