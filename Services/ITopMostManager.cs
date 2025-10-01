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
}
