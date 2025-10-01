using System.Text.Json.Serialization;

namespace Lc_auto.Models;

/// <summary>
/// 파일 시스템 경로 설정
/// </summary>
public class PathsConfig
{
    /// <summary>
    /// 사진 저장 폴더 경로 (버튼 d로 열림, 경로가 없으면 에러 표시)
    /// </summary>
    [JsonPropertyName("targetFolder")]
    public string TargetFolder { get; set; } = string.Empty;
}
