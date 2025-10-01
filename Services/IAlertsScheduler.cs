using System;
using System.Collections.Generic;
using Lc_auto.Models;

namespace Lc_auto.Services;

/// <summary>
/// 구성 파일의 알림 정의를 기반으로 HH:mm 스케줄링을 수행하는 알림 스케줄러 인터페이스입니다.
/// </summary>
public interface IAlertsScheduler
{
    /// <summary>
    /// 알림 트리거 발생 시 알림 메시지를 전달하는 이벤트입니다.
    /// </summary>
    event EventHandler<string>? AlertTriggered;

    /// <summary>
    /// 새로운 알림 목록을 적용합니다. 기존 스케줄은 정리한 뒤 Start 호출 시점부터 반영됩니다.
    /// </summary>
    /// <param name="alerts">적용할 알림 구성 목록</param>
    void Apply(IEnumerable<AlertConfig> alerts);

    /// <summary>
    /// 알림 스케줄러를 시작합니다. 이미 실행 중이면 요청을 무시합니다.
    /// </summary>
    void Start();

    /// <summary>
    /// 알림 스케줄러를 중지하고 모든 타이머를 정리합니다.
    /// </summary>
    void Stop();
}
