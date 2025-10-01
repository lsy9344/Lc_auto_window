using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Lc_auto.Services;
using Lc_auto.UI;

namespace Lc_auto.ViewModels;

/// <summary>
/// 예약 알림 메시지를 모달 대화상자로 노출하는 ViewModel입니다.
/// </summary>
public class AlertsViewModel : INotifyPropertyChanged
{
    private readonly object _sync = new();
    private AlertDialog? _dialogInstance;
    private string _currentMessage = string.Empty;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 현재 대화상자에 표시할 알림 메시지입니다.
    /// </summary>
    public string CurrentMessage
    {
        get => _currentMessage;
        private set
        {
            if (_currentMessage == value)
            {
                return;
            }

            _currentMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 스케줄러에서 발생한 알림 메시지를 처리해 대화상자를 갱신합니다.
    /// </summary>
    /// <param name="message">표시할 알림 메시지</param>
    public void HandleAlert(string message)
    {
        if (message == null)
        {
            LoggingService.LogWarn("null 알림 메시지를 수신해 무시했습니다.");
            return;
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
        {
            LoggingService.LogWarn("Dispatcher를 확인할 수 없어 알림을 표시하지 못했습니다.");
            return;
        }

        dispatcher.Invoke(() =>
        {
            lock (_sync)
            {
                CurrentMessage = message;

                if (_dialogInstance == null)
                {
                    // 알림 표시용 대화상자를 재사용하기 위해 최초 한 번만 생성합니다.
                    var dialog = new AlertDialog
                    {
                        Owner = Application.Current?.MainWindow,
                        DataContext = this
                    };

                    dialog.Closed += (_, _) =>
                    {
                        lock (_sync)
                        {
                            _dialogInstance = null;
                        }
                    };

                    _dialogInstance = dialog;
                    _dialogInstance.Show();
                }
                else if (!_dialogInstance.IsVisible)
                {
                    _dialogInstance.Show();
                }

                _dialogInstance.Activate();
            }
        });
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
