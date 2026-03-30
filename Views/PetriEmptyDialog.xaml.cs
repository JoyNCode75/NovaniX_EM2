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

namespace NovaniX_EM2.Views
{
    public partial class PetriEmptyDialog : Window
    {
        public PetriEmptyDialog()
        {
            InitializeComponent();
        }

        private void BtnResume_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // 재실행 선택
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // 동작 정지 선택
        }
    }
}