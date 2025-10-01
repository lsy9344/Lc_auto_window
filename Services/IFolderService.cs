using System.Threading;
using System.Threading.Tasks;

namespace Lc_auto.Services;

/// <summary>
/// PRD §1.2 버튼 d "사진 저장 폴더 열기" 플로우를 담당하는 폴더 열기 서비스 인터페이스입니다.
/// </summary>
public interface IFolderService
{
    /// <summary>
    /// 지정된 폴더 경로를 검증한 뒤 Windows 탐색기에서 엽니다.
    /// 경로가 잘못되었거나 탐색기 실행에 실패하면 false를 반환합니다.
    /// </summary>
    /// <param name="folderPath">열어야 할 대상 폴더의 절대 경로</param>
    /// <param name="ct">작업 취소 토큰</param>
    /// <returns>탐색기가 정상적으로 시작되면 true, 실패하면 false</returns>
    Task<bool> OpenAsync(string folderPath, CancellationToken ct);
}
