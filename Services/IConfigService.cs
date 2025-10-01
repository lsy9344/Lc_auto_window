using Lc_auto.Models;

namespace Lc_auto.Services;

/// <summary>
/// 설정 관리 서비스 인터페이스 (config.json 로드, 검증, Hot Reload)
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// 현재 로드된 애플리케이션 설정
    /// </summary>
    AppConfig Current { get; }

    /// <summary>
    /// 설정 파일 변경 시 발생하는 이벤트 (Hot Reload, Phase 2에서 구현 예정)
    /// </summary>
    event EventHandler<AppConfig>? ConfigChanged;
}
