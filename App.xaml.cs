using System;
using System.Windows;
using Lc_auto.Bootstrap;
using Lc_auto.Services;
using Lc_auto.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Lc_auto;

/// <summary>
/// 애플리케이션 수명 주기를 제어하는 App 클래스입니다.
/// </summary>
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;
    private AlertsViewModel? _alertsViewModel;
    private IAlertsScheduler? _alertsScheduler;
    private EventHandler<string>? _alertHandler;

    /// <summary>
    /// 애플리케이션 시작 시 DI 구성과 주 창 초기화를 수행합니다.
    /// </summary>
    /// <param name="e">시작 이벤트 인자입니다.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow();

        _serviceProvider = CompositionRoot.ConfigureServices(mainWindow);

        MainWindow = mainWindow;

        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;

        _alertsViewModel = _serviceProvider.GetRequiredService<AlertsViewModel>();
        _alertsScheduler = _serviceProvider.GetRequiredService<IAlertsScheduler>();

        _alertHandler = (_, message) =>
        {
            _alertsViewModel?.HandleAlert(message);
        };

        _alertsScheduler.AlertTriggered += _alertHandler;

        LoggingService.LogInfo("애플리케이션이 시작되었습니다.");

        mainWindow.Show();
    }

    /// <summary>
    /// 애플리케이션 종료 시 로그를 플러시하고 서비스 공급자를 해제합니다.
    /// </summary>
    /// <param name="e">종료 이벤트 인자입니다.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        if (_alertsScheduler != null && _alertHandler != null)
        {
            _alertsScheduler.AlertTriggered -= _alertHandler;

            // ConfigService에서 Stop을 호출하지만, 종료 시점에 확실한 정리를 위해 한 번 더 호출합니다.
            _alertsScheduler.Stop();
        }

        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        LoggingService.CloseAndFlush();

        base.OnExit(e);
    }
}
