using System.ComponentModel;
using System.Windows;

namespace Lc_auto.UI;

/// <summary>
/// 예약 알림을 표시하는 모달 느낌의 대화상자입니다.
/// </summary>
public partial class AlertDialog : Window
{
    /// <summary>
    /// 대화상자를 초기화합니다.
    /// </summary>
    public AlertDialog()
    {
        InitializeComponent();
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    /// <inheritdoc />
    protected override void OnClosing(CancelEventArgs e)
    {
        if (Application.Current?.Dispatcher.HasShutdownStarted == true ||
            Application.Current?.Dispatcher.HasShutdownFinished == true)
        {
            base.OnClosing(e);
            return;
        }

        e.Cancel = true;
        Hide();
    }
}
