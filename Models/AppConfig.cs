using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lc_auto.Models;

/// <summary>
/// Lc_auto 애플리케이션 전체 설정 (config.json의 루트 컨테이너)
/// </summary>
public class AppConfig
{
    /// <summary>
    /// 미디어 설정 (동영상 재생 관련)
    /// </summary>
    [JsonPropertyName("media")]
    public MediaConfig Media { get; set; } = new();

    /// <summary>
    /// Lightroom Classic 자동화 설정
    /// </summary>
    [JsonPropertyName("automation")]
    public AutomationConfig Automation { get; set; } = new();

    /// <summary>
    /// 파일 시스템 경로 설정
    /// </summary>
    [JsonPropertyName("paths")]
    public PathsConfig Paths { get; set; } = new();

    /// <summary>
    /// 예약 알림 팝업 목록
    /// </summary>
    [JsonPropertyName("alerts")]
    public List<AlertConfig> Alerts { get; set; } = new();

    /// <summary>
    /// UI 동작 설정
    /// </summary>
    [JsonPropertyName("ui")]
    public UiConfig Ui { get; set; } = new();
}
