using System.Text.Json.Serialization;

namespace Lc_auto.Models;

/// <summary>
/// 미디어 설정 (동영상 재생 관련)
/// </summary>
public class MediaConfig
{
    /// <summary>
    /// 배경지 설치 동영상 파일 경로 (필수, 절대 경로 권장)
    /// </summary>
    [JsonPropertyName("videoPath")]
    public string VideoPath { get; set; } = string.Empty;

    /// <summary>
    /// 커스텀 비디오 플레이어 실행 파일 경로 (선택, 빈 문자열이면 OS 기본 플레이어 사용)
    /// </summary>
    [JsonPropertyName("playerPath")]
    public string? PlayerPath { get; set; }
}
