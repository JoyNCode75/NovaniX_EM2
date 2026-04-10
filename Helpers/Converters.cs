using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NovaniX_EM2.Helpers
{
    // 1. 상태에 따라 색상을 반환하는 컨버터 (방금 추가한 부분)
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAlarmed && isAlarmed)
                return System.Windows.Media.Brushes.Red; // ▼ WPF용 빨간색으로 명확히 지정

            return System.Windows.Media.Brushes.Green; // ▼ WPF용 초록색으로 명확히 지정
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 2. True/False를 반대로 뒤집어주는 컨버터 (오류가 발생한 부분 - 복구)
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return false;
        }
    }

    // 3. True/False를 화면에 보이기/숨기기로 변환해주는 컨버터 (UI Visibility 용도)
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis)
                return vis == Visibility.Visible;
            return false;
        }
    }

    // True/False를 반대의 화면 표시 상태(Collapsed/Visible)로 변환해주는 컨버터
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 값이 True이면 숨기고(Collapsed), False이면 보여줍니다(Visible).
            if (value is bool b && b)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis)
                return vis != Visibility.Visible;

            return false;
        }
    }
}