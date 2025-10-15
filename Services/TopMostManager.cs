using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lc_auto.Services;

/// <summary>
/// 앱 창 TopMost 상태 관리 구현 및 Lightroom 포커스 핸드오프
/// </summary>
public class TopMostManager : ITopMostManager
{
    private readonly Window _window;
    private readonly IWindowFocusService _windowFocusService;

    private WindowState _windowStateBeforeAutomation = WindowState.Normal;
    private bool _wasTopMostBeforeAutomation;
    private bool _shouldRestoreWindowState;

    // 복원 재시도 설정
    private const int MaxRestoreRetries = 3;
    private const int RetryDelayMs = 1000;

    /// <summary>
    /// TopMostManager 생성자 - Window 및 WindowFocusService 주입
    /// </summary>
    /// <param name="window">관리할 Window 객체</param>
    /// <param name="windowFocusService">창 포커스 서비스</param>
    /// <exception cref="ArgumentNullException">window 또는 windowFocusService가 null인 경우</exception>
    public TopMostManager(Window window, IWindowFocusService windowFocusService)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _windowFocusService = windowFocusService ?? throw new ArgumentNullException(nameof(windowFocusService));
    }

    /// <summary>
    /// 앱 창의 TopMost 상태 설정
    /// </summary>
    /// <param name="isTopMost">true: 항상 위, false: 일반 창</param>
    public void SetAppTopMost(bool isTopMost)
    {
        if (!_window.Dispatcher.CheckAccess())
        {
            _window.Dispatcher.Invoke(() => SetAppTopMost(isTopMost));
            return;
        }

        if (_window.Topmost == isTopMost)
        {
            return;
        }

        _window.Topmost = isTopMost;
        LoggingService.LogInfo($"TopMost 상태 변경: {isTopMost}");
    }

    /// <inheritdoc />
    public async Task<bool> HandOffToLightroomAsync(Func<Task> automationTask, CancellationToken ct = default)
    {
        if (automationTask == null)
        {
            throw new ArgumentNullException(nameof(automationTask));
        }

        LoggingService.LogInfo("Lightroom으로 포커스 핸드오프 시작");

        PrepareWindowForAutomation();

        try
        {
            // 1. Lightroom Classic 창 활성화 시도 (실패해도 계속 진행)
            bool lightroomActivated = await _windowFocusService.TryActivateLightroomAsync(ct);
            if (lightroomActivated)
            {
                LoggingService.LogInfo("Lightroom Classic 창 활성화 성공");
            }
            else
            {
                LoggingService.LogInfo("Lightroom Classic 창을 찾지 못했습니다. 자동화가 Lightroom을 실행할 것입니다.");
            }

            // 2. 앱 창 TopMost 해제 (Lightroom이 최상위로 유지되도록)
            SetAppTopMost(false);

            // 3. 자동화 실행 (AutomationService가 Lightroom이 없으면 자동 실행함)
            LoggingService.LogInfo("자동화 실행 중...");
            await automationTask();
            LoggingService.LogInfo("자동화 실행 완료");

            // 4. 앱 창으로 포커스 복원 및 TopMost 설정 (재시도 포함)
            return await RestoreAppFocusWithRetryAsync(ct);
        }
        catch (OperationCanceledException)
        {
            LoggingService.LogWarn("Lightroom 포커스 핸드오프가 취소되었습니다.");
            // 취소되어도 앱 창 복원 시도
            await RestoreAppFocusWithRetryAsync(CancellationToken.None);
            ResetWindowTracking();
            return false;
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"Lightroom 포커스 핸드오프 중 오류 발생: {ex.Message}", ex);
            // 오류 발생해도 앱 창 복원 시도
            await RestoreAppFocusWithRetryAsync(CancellationToken.None);
            ResetWindowTracking();
            return false;
        }
    }

    /// <summary>
    /// 앱 창으로 포커스를 복원하고 TopMost를 설정합니다. (재시도 포함)
    /// </summary>
    private async Task<bool> RestoreAppFocusWithRetryAsync(CancellationToken ct)
    {
        for (int attempt = 1; attempt <= MaxRestoreRetries; attempt++)
        {
            if (ct.IsCancellationRequested)
            {
                LoggingService.LogWarn("앱 창 복원 작업 취소됨");
                ResetWindowTracking();
                return false;
            }

            try
            {
                LoggingService.LogInfo($"앱 창 복원 시도 {attempt}/{MaxRestoreRetries}");

                await RestoreWindowStateAsync();

                // 앱 창으로 포커스 복원
                _windowFocusService.RestoreAppWindow();

                // 짧은 대기 (포커스 전환 안정화)
                await Task.Delay(500, ct);

                bool lightroomRunning = IsLightroomRunning();

                if (!lightroomRunning && _wasTopMostBeforeAutomation)
                {
                    SetAppTopMost(true);
                    LoggingService.LogInfo("앱 창 복원 및 TopMost 설정 성공");
                }
                else
                {
                    SetAppTopMost(false);
                    LoggingService.LogInfo("앱 창 복원 완료 (Lightroom 실행 중 - TopMost 미설정)");
                }
                ResetWindowTracking();
                return true;
            }
            catch (Exception ex)
            {
                LoggingService.LogWarn($"앱 창 복원 시도 {attempt}/{MaxRestoreRetries} 실패: {ex.Message}");

                if (attempt < MaxRestoreRetries)
                {
                    await Task.Delay(RetryDelayMs, ct);
                }
            }
        }

        LoggingService.LogWarn($"앱 창 복원을 {MaxRestoreRetries}회 시도했으나 실패했습니다.");
        ResetWindowTracking();
        return false;
    }

    private void PrepareWindowForAutomation()
    {
        if (!_window.Dispatcher.CheckAccess())
        {
            _window.Dispatcher.Invoke(PrepareWindowForAutomation);
            return;
        }

        _wasTopMostBeforeAutomation = _window.Topmost;
        _windowStateBeforeAutomation = _window.WindowState;
        _shouldRestoreWindowState = true;

        SetAppTopMost(false);

        if (_window.WindowState != WindowState.Minimized)
        {
            _window.WindowState = WindowState.Minimized;
            LoggingService.LogInfo("Lightroom 자동화 동안 메인 창을 최소화했습니다.");
        }
    }

    private async Task RestoreWindowStateAsync()
    {
        if (!_shouldRestoreWindowState)
        {
            return;
        }

        if (!_window.Dispatcher.CheckAccess())
        {
            await _window.Dispatcher.InvokeAsync(RestoreWindowStateCore);
            return;
        }

        RestoreWindowStateCore();
    }

    private void RestoreWindowStateCore()
    {
        if (!_shouldRestoreWindowState)
        {
            return;
        }

        if (_window.WindowState == WindowState.Minimized)
        {
            var targetState = _windowStateBeforeAutomation == WindowState.Minimized
                ? WindowState.Normal
                : _windowStateBeforeAutomation;

            _window.WindowState = targetState;
            LoggingService.LogInfo($"메인 창 상태를 {targetState}로 복원했습니다.");
        }

        if (!_window.IsVisible)
        {
            _window.Show();
        }
    }

    private static bool IsLightroomRunning()
    {
        try
        {
            return Process.GetProcessesByName("Lightroom").Length > 0
                   || Process.GetProcessesByName("Adobe Lightroom Classic").Length > 0;
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"Lightroom 프로세스 상태 확인 중 오류 발생: {ex.Message}");
            return false;
        }
    }

    private void ResetWindowTracking()
    {
        if (!_window.Dispatcher.CheckAccess())
        {
            _window.Dispatcher.Invoke(ResetWindowTracking);
            return;
        }

        _shouldRestoreWindowState = false;
    }
}
