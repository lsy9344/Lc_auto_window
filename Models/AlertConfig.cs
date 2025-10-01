using System.Text.Json.Serialization;

namespace Lc_auto.Models;

/// <summary>
/// 예약 알림 팝업 설정
/// </summary>
public class AlertConfig
{
    /// <summary>
    /// 알림 트리거 시간 (HH:mm 형식, 로컬 시간, ±1초 허용)
    /// </summary>
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// 알림 팝업에 표시할 메시지
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 반복 주기 ("daily": 매일, "once": 한 번만, 기본값: "daily")
    /// </summary>
    [JsonPropertyName("repeat")]
    public string? Repeat { get; set; }
}
