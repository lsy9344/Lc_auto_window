using System.Threading;
using System.Threading.Tasks;

namespace Lc_auto.Services;

/// <summary>
/// 창 포커스 제어 서비스 인터페이스
/// </summary>
public interface IWindowFocusService
{
    /// <summary>
    /// Lightroom Classic 창을 찾아 포어그라운드로 전환합니다.
    /// </summary>
    /// <param name="ct">취소 토큰</param>
    /// <returns>성공 시 true, 실패 시 false</returns>
    Task<bool> TryActivateLightroomAsync(CancellationToken ct = default);

    /// <summary>
    /// 앱 창으로 포커스를 복원합니다.
    /// </summary>
    void RestoreAppWindow();

    /// <summary>
    /// 현재 앱 창 핸들을 저장합니다.
    /// </summary>
    /// <param name="handle">앱 창 핸들</param>
    void SetAppWindowHandle(nint handle);
}
