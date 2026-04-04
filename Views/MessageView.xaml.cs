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
    public partial class MessageView : Window
    {
        // ▼ 반환값: 0(확인), 1(대기), 2(취소)
        // (X를 눌러 강제종료할 경우를 대비해 기본값은 2(취소)로 설정)
        public int Result { get; private set; } = 2;

        /// <summary>
        /// 공용 메시지 창
        /// </summary>
        /// <param name="message">화면에 표시할 내용</param>
        /// <param name="title">창 제목</param>
        /// <param name="btnConfirmText">1번 버튼 이름 (null이면 숨김)</param>
        /// <param name="btnWaitText">2번 버튼 이름 (null이면 숨김)</param>
        /// <param name="btnCancelText">3번 버튼 이름 (null이면 숨김)</param>
        public MessageView(string message, string title = "알림", string btnConfirmText = "확인", string btnWaitText = null!, string btnCancelText = null!)
        {
            InitializeComponent();

            this.Title = title;
            TxtMessage.Text = message;

            // 1번 버튼 (확인 등)
            if (!string.IsNullOrEmpty(btnConfirmText))
            {
                BtnConfirm.Content = btnConfirmText;
                BtnConfirm.Visibility = Visibility.Visible;
            }

            // 2번 버튼 (대기 등)
            if (!string.IsNullOrEmpty(btnWaitText))
            {
                BtnWait.Content = btnWaitText;
                BtnWait.Visibility = Visibility.Visible;
            }

            // 3번 버튼 (취소 등)
            if (!string.IsNullOrEmpty(btnCancelText))
            {
                BtnCancel.Content = btnCancelText;
                BtnCancel.Visibility = Visibility.Visible;
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            Result = 0; // 0 반환
            this.DialogResult = true;
        }

        private void BtnWait_Click(object sender, RoutedEventArgs e)
        {
            Result = 1; // 1 반환
            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = 2; // 2 반환
            this.DialogResult = false;
        }
    }
}