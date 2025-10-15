using System.Text.Json.Serialization;

namespace Lc_auto.Models;

/// <summary>
/// Lightroom Classic 카메라 설정 (촬영 시작 자동화에 사용)
/// </summary>
public class CameraConfig
{
    /// <summary>
    /// 셔터 속도 설정값
    /// </summary>
    [JsonPropertyName("shutter")]
    public string Shutter { get; set; } = "10";

    /// <summary>
    /// 조리개 설정값
    /// </summary>
    [JsonPropertyName("aperture")]
    public string Aperture { get; set; } = "10";

    /// <summary>
    /// ISO 설정값
    /// </summary>
    [JsonPropertyName("iso")]
    public string Iso { get; set; } = "400";

    /// <summary>
    /// 화이트 밸런스 설정값
    /// </summary>
    [JsonPropertyName("whiteBalance")]
    public string WhiteBalance { get; set; } = "자동";
}
