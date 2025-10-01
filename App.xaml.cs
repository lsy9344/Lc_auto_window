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

    /// <summary>
    /// 애플리케이션 시작 시 DI 구성과 주 창 초기화를 수행합니다.
    /// </summary>
    /// <param name="e">시작 이벤트 인자입니다.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow();

        _serviceProvider = CompositionRoot.ConfigureServices(mainWindow);

        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;

        LoggingService.LogInfo("애플리케이션이 시작되었습니다.");

        MainWindow = mainWindow;
        mainWindow.Show();
    }

    /// <summary>
    /// 애플리케이션 종료 시 로그를 플러시하고 서비스 공급자를 해제합니다.
    /// </summary>
    /// <param name="e">종료 이벤트 인자입니다.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        LoggingService.CloseAndFlush();

        base.OnExit(e);
    }
}
