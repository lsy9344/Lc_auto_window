using System.Threading;
using System.Threading.Tasks;

namespace Lc_auto.Services;

/// <summary>
/// PRD §1.2 버튼 c "배경지 설치 동영상 보기" 기능의 재생 흐름을 담당하는 미디어 서비스 인터페이스
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// 현재 동영상 재생 프로세스가 활성 상태인지 여부
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    /// 구성된 동영상 경로 및 선택적 플레이어를 사용해 재생을 수행합니다.
    /// </summary>
    /// <param name="videoPath">재생할 동영상 파일의 절대 경로</param>
    /// <param name="playerPath">사용자 지정 플레이어 실행 파일 경로 (없으면 기본 플레이어 사용)</param>
    /// <param name="ct">재생 요청 취소 토큰</param>
    /// <returns>재생 프로세스가 정상적으로 시작되면 true, 실패하면 false</returns>
    Task<bool> PlayAsync(string videoPath, string? playerPath, CancellationToken ct);
}
