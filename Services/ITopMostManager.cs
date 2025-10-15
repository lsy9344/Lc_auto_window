using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lc_auto.Services;

/// <summary>
/// 앱 창 TopMost 상태 관리 서비스
/// </summary>
public interface ITopMostManager
{
    /// <summary>
    /// 앱 창의 TopMost 상태 설정
    /// </summary>
    /// <param name="isTopMost">true: 항상 위, false: 일반 창</param>
    void SetAppTopMost(bool isTopMost);

    /// <summary>
    /// Lightroom Classic으로 포커스를 넘기고 자동화를 실행한 후 앱 창으로 복원합니다.
    /// </summary>
    /// <param name="automationTask">실행할 자동화 태스크</param>
    /// <param name="ct">취소 토큰</param>
    /// <returns>성공 시 true, 실패 시 false</returns>
    Task<bool> HandOffToLightroomAsync(Func<Task> automationTask, CancellationToken ct = default);
}
