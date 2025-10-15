using System;
using System.Threading;
using System.Threading.Tasks;
using Lc_auto.Interop;

namespace Lc_auto.Services;

/// <summary>
/// 창 포커스 제어 서비스 구현
/// Lightroom Classic 창과 앱 창 간 포커스 전환을 담당합니다.
/// </summary>
public class WindowFocusService : IWindowFocusService
{
    private IntPtr _appWindowHandle = IntPtr.Zero;

    // Lightroom Classic 창 제목 후보 목록 (버전에 따라 다를 수 있음)
    private static readonly string[] LightroomWindowTitles =
    [
        "Lightroom Classic",
        "Adobe Lightroom Classic",
        "Lightroom"
    ];

    /// <inheritdoc />
    public void SetAppWindowHandle(nint handle)
    {
        _appWindowHandle = handle;
        LoggingService.LogInfo($"앱 창 핸들 설정: 0x{handle:X}");
    }

    /// <inheritdoc />
    public async Task<bool> TryActivateLightroomAsync(CancellationToken ct = default)
    {
        const int maxRetries = 3;
        const int retryDelayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            if (ct.IsCancellationRequested)
            {
                LoggingService.LogWarn("Lightroom 활성화 작업 취소됨");
                return false;
            }

            // Lightroom Classic 창 찾기 (여러 제목 시도)
            IntPtr lightroomHandle = IntPtr.Zero;
            string? foundTitle = null;

            foreach (var title in LightroomWindowTitles)
            {
                lightroomHandle = Win32.FindWindow(null, title);
                if (lightroomHandle != IntPtr.Zero)
                {
                    foundTitle = title;
                    break;
                }
            }

            if (lightroomHandle == IntPtr.Zero)
            {
                LoggingService.LogWarn($"Lightroom Classic 창을 찾을 수 없음 (시도 {attempt}/{maxRetries})");

                if (attempt < maxRetries)
                {
                    await Task.Delay(retryDelayMs, ct);
                    continue;
                }

                LoggingService.LogWarn("Lightroom Classic 창을 찾지 못했습니다. Lightroom이 실행 중인지 확인하세요.");
                return false;
            }

            // Lightroom 창을 포어그라운드로 전환
            bool success = Win32.SetForegroundWindow(lightroomHandle);

            if (success)
            {
                LoggingService.LogInfo($"Lightroom Classic 창 활성화 성공 (제목: \"{foundTitle}\", 핸들: 0x{lightroomHandle:X})");
                return true;
            }

            LoggingService.LogWarn($"Lightroom Classic 창 활성화 실패 (시도 {attempt}/{maxRetries})");

            if (attempt < maxRetries)
            {
                await Task.Delay(retryDelayMs, ct);
            }
        }

        LoggingService.LogWarn($"Lightroom Classic 창 활성화를 {maxRetries}회 시도했으나 실패했습니다.");
        return false;
    }

    /// <inheritdoc />
    public void RestoreAppWindow()
    {
        if (_appWindowHandle == IntPtr.Zero)
        {
            LoggingService.LogWarn("앱 창 핸들이 설정되지 않아 포커스 복원을 건너뜁니다.");
            return;
        }

        bool success = Win32.SetForegroundWindow(_appWindowHandle);

        if (success)
        {
            LoggingService.LogInfo("앱 창 포커스 복원 성공");
        }
        else
        {
            LoggingService.LogWarn("앱 창 포커스 복원 실패 - 재시도 필요할 수 있음");
        }
    }
}
