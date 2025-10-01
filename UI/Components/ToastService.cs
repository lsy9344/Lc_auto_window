using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lc_auto.UI.Components;

/// <summary>
/// 비차단 토스트 알림을 표시하는 서비스 (설정 로드 성공/실패 등 정보성 메시지)
/// </summary>
public static class ToastService
{
    /// <summary>
    /// 화면 우하단에 토스트 알림을 표시합니다. FadeIn/FadeOut 애니메이션과 함께 지정된 시간 후 자동으로 사라집니다.
    /// </summary>
    /// <param name="message">토스트에 표시할 메시지 내용</param>
    /// <param name="durationMs">토스트 표시 시간 (밀리초, 기본값: 3000ms = 3초)</param>
    public static void Show(string message, int durationMs = 3000)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // TextBlock 생성 (토스트 메시지 텍스트)
            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 14,
                Padding = new Thickness(16, 12, 16, 12),
                Background = new SolidColorBrush(Color.FromArgb(220, 50, 50, 50)),
                MaxWidth = 300,
                TextWrapping = TextWrapping.Wrap
            };

            // Border 생성 (둥근 모서리 및 스타일링)
            var border = new Border
            {
                Child = textBlock,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromArgb(220, 50, 50, 50)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(180, 100, 100, 100)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(20)
            };

            // Popup 생성 (오버레이 동작, 비차단)
            var popup = new Popup
            {
                Child = border,
                Placement = PlacementMode.Absolute,
                AllowsTransparency = true,
                IsOpen = true,
                Opacity = 0
            };

            // 화면 우하단 위치 계산 (20px 마진)
            var workArea = SystemParameters.WorkArea;
            popup.HorizontalOffset = workArea.Right - 340; // 300px width + 20px margin + 20px border margin
            popup.VerticalOffset = workArea.Bottom - 100; // 높이 + 마진 고려

            // FadeIn 애니메이션 (Opacity 0 → 1, 300ms)
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            // FadeOut 애니메이션 (Opacity 1 → 0, 300ms, durationMs 이후 시작)
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                BeginTime = TimeSpan.FromMilliseconds(durationMs)
            };

            // FadeOut 완료 후 Popup 닫기
            fadeOut.Completed += (sender, args) =>
            {
                popup.IsOpen = false;
            };

            // Storyboard 생성 및 애니메이션 연결
            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeIn);
            storyboard.Children.Add(fadeOut);

            Storyboard.SetTarget(fadeIn, popup);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Popup.OpacityProperty));
            Storyboard.SetTarget(fadeOut, popup);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(Popup.OpacityProperty));

            // 애니메이션 시작
            storyboard.Begin();
        });
    }
}
