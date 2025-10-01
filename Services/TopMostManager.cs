using System;
using System.Windows;

namespace Lc_auto.Services;

/// <summary>
/// 앱 창 TopMost 상태 관리 구현 (Phase 4에서 Lightroom 포커스 핸드오프 추가 예정)
/// </summary>
public class TopMostManager : ITopMostManager
{
    private readonly Window _window;

    /// <summary>
    /// TopMostManager 생성자 - Window 참조 주입
    /// </summary>
    /// <param name="window">관리할 Window 객체</param>
    /// <exception cref="ArgumentNullException">window가 null인 경우</exception>
    public TopMostManager(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    /// <summary>
    /// 앱 창의 TopMost 상태 설정
    /// </summary>
    /// <param name="isTopMost">true: 항상 위, false: 일반 창</param>
    public void SetAppTopMost(bool isTopMost)
    {
        _window.Topmost = isTopMost;
        LoggingService.LogInfo($"TopMost 상태 변경: {isTopMost}");

        // TODO: Phase 4 - HandOffToLightroomAsync() 추가 시 재시도 로직 구현
        // - 실패 시 1초 간격 최대 3회 재시도
        // - WindowFocusService.TryActivateLightroomAsync() 호출
        // - Automation 실행 후 RestoreAppWindow() 및 SetAppTopMost(true) 복원
    }
}
