using System.Collections.Generic;

namespace Lc_auto.Services.Automation;

/// <summary>
/// Lightroom Classic UI 자동화 스크립트를 정의하는 클래스입니다.
/// FlaUI를 사용한 UI 요소 탐색 및 상호작용 로직을 담습니다.
/// </summary>
public class FeatureScript
{
    /// <summary>
    /// 기능 식별자 (예: "Lightroom.StartPhotoSession")
    /// </summary>
    public string FeatureId { get; init; } = string.Empty;

    /// <summary>
    /// 기능 설명 (예: "촬영 시작 자동화")
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// UI 자동화 스텝 목록 (순서대로 실행)
    /// 각 스텝은 UIA 선택자와 액션을 정의합니다.
    /// </summary>
    public List<AutomationStep> Steps { get; init; } = [];
}

/// <summary>
/// UI 자동화 개별 스텝을 정의하는 클래스입니다.
/// </summary>
public class AutomationStep
{
    /// <summary>
    /// 스텝 설명 (로그용)
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// UI 요소를 찾기 위한 선택자 (AutomationId, Name, ClassName 등)
    /// </summary>
    public UiaSelector Selector { get; init; } = new();

    /// <summary>
    /// 수행할 액션 (Click, SetText, Wait 등)
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// 액션에 필요한 추가 데이터 (예: SetText의 경우 입력할 텍스트)
    /// </summary>
    public string? ActionData { get; init; }
}

/// <summary>
/// UIA 선택자 (AutomationId, Name, ClassName 등)
/// </summary>
public class UiaSelector
{
    /// <summary>
    /// AutomationId 속성
    /// </summary>
    public string? AutomationId { get; init; }

    /// <summary>
    /// Name 속성
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// ClassName 속성
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// ControlType (예: "Button", "Edit", "Window")
    /// </summary>
    public string? ControlType { get; init; }
}
