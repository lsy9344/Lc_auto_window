namespace Lc_auto.Services.Automation;

/// <summary>
/// UI 자동화 실행 결과를 담는 클래스입니다.
/// </summary>
public class AutomationResult
{
    /// <summary>
    /// 실행 성공 여부
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 결과 메시지 (성공/에러 메시지)
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 소요 시간 (밀리초)
    /// </summary>
    public long ElapsedMs { get; init; }

    /// <summary>
    /// 발생한 예외 (선택)
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// 성공 결과 생성
    /// </summary>
    public static AutomationResult CreateSuccess(string message, long elapsedMs)
    {
        return new AutomationResult
        {
            Success = true,
            Message = message,
            ElapsedMs = elapsedMs
        };
    }

    /// <summary>
    /// 실패 결과 생성
    /// </summary>
    public static AutomationResult CreateFailure(string message, long elapsedMs, Exception? exception = null)
    {
        return new AutomationResult
        {
            Success = false,
            Message = message,
            ElapsedMs = elapsedMs,
            Exception = exception
        };
    }
}
