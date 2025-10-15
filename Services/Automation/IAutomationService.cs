using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lc_auto.Services.Automation;

/// <summary>
/// Lightroom Classic UI 자동화 서비스 인터페이스
/// </summary>
public interface IAutomationService
{
    /// <summary>
    /// 지정된 featureId에 대응하는 자동화를 실행합니다.
    /// </summary>
    /// <param name="featureId">실행할 기능 식별자 (예: "Lightroom.StartPhotoSession")</param>
    /// <param name="inputValues">자동화에 필요한 입력값 (키: 파라미터명, 값: 입력값)</param>
    /// <param name="ct">취소 토큰</param>
    /// <returns>자동화 실행 결과</returns>
    Task<AutomationResult> RunAsync(string featureId, Dictionary<string, string> inputValues, CancellationToken ct = default);
}
