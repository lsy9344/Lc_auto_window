using System;
using Serilog;
using Serilog.Events;

namespace Lc_auto.Services;

/// <summary>
/// 앱 전역 로깅 서비스 (Serilog 기반)
/// </summary>
public static class LoggingService
{
    /// <summary>
    /// Serilog 정적 로거 초기화
    /// </summary>
    static LoggingService()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: "log/Lc_auto-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 50_000_000,
                rollOnFileSizeLimit: true
            )
            .CreateLogger();

        LogInfo("LoggingService 초기화 완료");
    }

    /// <summary>
    /// 정보 메시지 로그 기록
    /// </summary>
    /// <param name="message">로그 메시지</param>
    public static void LogInfo(string message)
    {
        Log.Information(message);
    }

    /// <summary>
    /// 경고 메시지 로그 기록 (선택적 예외 포함)
    /// </summary>
    /// <param name="message">로그 메시지</param>
    /// <param name="ex">선택적 예외 객체</param>
    public static void LogWarn(string message, Exception? ex = null)
    {
        if (ex != null)
        {
            Log.Warning(ex, message);
        }
        else
        {
            Log.Warning(message);
        }
    }

    /// <summary>
    /// 오류 메시지 로그 기록 (예외 필수)
    /// </summary>
    /// <param name="message">로그 메시지</param>
    /// <param name="ex">예외 객체</param>
    public static void LogError(string message, Exception ex)
    {
        Log.Error(ex, message);
    }

    /// <summary>
    /// 로그 버퍼 플러시 및 리소스 해제 (App.OnExit에서 호출)
    /// </summary>
    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }
}
