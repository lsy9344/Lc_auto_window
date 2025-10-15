using System.Text.Json.Serialization;

namespace Lc_auto.Models;

/// <summary>
/// Lightroom Classic 자동화 설정
/// </summary>
public class AutomationConfig
{
    /// <summary>
    /// Lightroom Classic 실행 파일 절대 경로
    /// </summary>
    [JsonPropertyName("lightroomPath")]
    public string LightroomPath { get; set; } = string.Empty;

    /// <summary>
    /// Lightroom 실행 후 대기 시간 (초 단위)
    /// </summary>
    [JsonPropertyName("startupWaitSec")]
    public int StartupWaitSec { get; set; } = 5;

    /// <summary>
    /// 버튼 a (촬영 시작) 자동화 설정
    /// </summary>
    [JsonPropertyName("p1")]
    public AutomationItem P1 { get; set; } = new();

    /// <summary>
    /// 버튼 b (내보내기) 자동화 설정
    /// </summary>
    [JsonPropertyName("p2")]
    public AutomationItem P2 { get; set; } = new();

    /// <summary>
    /// 자동화 시도당 타임아웃 (초 단위, 초기 시도 1회 + 재시도 2회 = 최대 3회)
    /// </summary>
    [JsonPropertyName("timeoutSec")]
    public int TimeoutSec { get; set; } = 30;

    /// <summary>
    /// 개별 자동화 항목 (p1, p2에 사용)
    /// </summary>
    public class AutomationItem
    {
        /// <summary>
        /// Feature Selector ID (FeatureSelectorRegistry에 매핑됨)
        /// </summary>
        [JsonPropertyName("featureId")]
        public string FeatureId { get; set; } = string.Empty;

        /// <summary>
        /// 자동화 기능 설명 (사람이 읽을 수 있는 형태)
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
