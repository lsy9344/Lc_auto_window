using System;
using System.Windows;
using Lc_auto.Services;
using Lc_auto.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Lc_auto.Bootstrap;

/// <summary>
/// 애플리케이션 전역 의존성을 등록하는 구성 루트 클래스입니다.
/// </summary>
public static class CompositionRoot
{
    /// <summary>
    /// MainWindow 인스턴스를 기반으로 DI 컨테이너를 구성합니다.
    /// </summary>
    /// <param name="mainWindow">TopMostManager가 참조할 MainWindow 인스턴스입니다.</param>
    /// <returns>구성된 <see cref="IServiceProvider"/> 인스턴스입니다.</returns>
    public static IServiceProvider ConfigureServices(Window mainWindow)
    {
        ArgumentNullException.ThrowIfNull(mainWindow);

        var services = new ServiceCollection();

        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<ITopMostManager>(_ => new TopMostManager(mainWindow));
        services.AddSingleton<MainViewModel>();

        var serviceProvider = services.BuildServiceProvider();
        LoggingService.LogInfo("의존성 주입 구성이 완료되었습니다.");

        return serviceProvider;
    }
}
