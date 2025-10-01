using System.Text.Json.Serialization;

namespace Lc_auto.Models;

/// <summary>
/// UI 동작 설정
/// </summary>
public class UiConfig
{
    /// <summary>
    /// 항상 위(AlwaysOnTop) 윈도우 동작 활성화 (기본값: true, 의도된 사용 시 true여야 함)
    /// </summary>
    [JsonPropertyName("alwaysOnTop")]
    public bool AlwaysOnTop { get; set; } = true;

    /// <summary>
    /// ModernWpf 테마 모드 ("auto": 자동, "light": 밝게, "dark": 어둡게, 기본값: "auto")
    /// </summary>
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "auto";
}
