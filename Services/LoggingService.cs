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
        try
        {
            // log 디렉토리가 없으면 생성
            if (!System.IO.Directory.Exists("log"))
            {
                System.IO.Directory.CreateDirectory("log");
            }

            // 오늘 날짜의 로그 파일 경로를 생성하고, 존재하면 삭제합니다.
            // 이렇게 하면 프로그램 시작 시 항상 새로운 로그 파일에 기록됩니다.
            var todayLogFileName = $"Lc_auto-{DateTime.Now:yyyyMMdd}.log";
            var todayLogFilePath = System.IO.Path.Combine("log", todayLogFileName);

            if (System.IO.File.Exists(todayLogFilePath))
            {
                System.IO.File.Delete(todayLogFilePath);
            }
        }
        catch
        {
            // 파일 조작 실패 시 무시 (이어서 로깅 시작)
        }

        // Serilog 설정
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: "log/Lc_auto-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 50 * 1024 * 1024, // 50MB
                rollOnFileSizeLimit: true,
                shared: true // 여러 프로세스에서 접근 가능하도록 설정
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
