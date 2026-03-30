using System.Windows;
using System.Windows.Input;

namespace NovaniX_EM2.Helpers
{
    public static class MouseBehavior
    {
        // --- 1. MouseDown (눌렀을 때) 커맨드 ---
        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseDownCommand", typeof(ICommand), typeof(MouseBehavior), new UIPropertyMetadata(null, OnMouseDownCommandChanged));

        public static void SetMouseDownCommand(DependencyObject target, ICommand value) => target.SetValue(MouseDownCommandProperty, value);
        public static ICommand GetMouseDownCommand(DependencyObject target) => (ICommand)target.GetValue(MouseDownCommandProperty);

        private static void OnMouseDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewMouseLeftButtonDown -= Element_PreviewMouseLeftButtonDown;
                if (e.NewValue != null)
                {
                    element.PreviewMouseLeftButtonDown += Element_PreviewMouseLeftButtonDown;
                }
            }
        }

        private static void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                var command = GetMouseDownCommand(element);
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                }
            }
        }

        // --- 2. MouseUp (뗐을 때) 커맨드 ---
        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseUpCommand", typeof(ICommand), typeof(MouseBehavior), new UIPropertyMetadata(null, OnMouseUpCommandChanged));

        public static void SetMouseUpCommand(DependencyObject target, ICommand value) => target.SetValue(MouseUpCommandProperty, value);
        public static ICommand GetMouseUpCommand(DependencyObject target) => (ICommand)target.GetValue(MouseUpCommandProperty);

        private static void OnMouseUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewMouseLeftButtonUp -= Element_PreviewMouseLeftButtonUp;
                element.MouseLeave -= Element_MouseLeave; // 누른 채로 버튼 밖으로 나갔을 때를 대비한 안전장치

                if (e.NewValue != null)
                {
                    element.PreviewMouseLeftButtonUp += Element_PreviewMouseLeftButtonUp;
                    element.MouseLeave += Element_MouseLeave;
                }
            }
        }

        private static void Element_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ExecuteMouseUpCommand(sender);
        }

        private static void Element_MouseLeave(object sender, MouseEventArgs e)
        {
            // 마우스 왼쪽 버튼을 누른 상태로 버튼 영역을 벗어나면 강제로 Stop 커맨드 실행
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ExecuteMouseUpCommand(sender);
            }
        }

        private static void ExecuteMouseUpCommand(object sender)
        {
            if (sender is UIElement element)
            {
                var command = GetMouseUpCommand(element);
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                }
            }
        }
    }
}