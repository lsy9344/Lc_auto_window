using System.Collections.Generic;

namespace Lc_auto.Services.Automation;

/// <summary>
/// featureId를 UIA 선택자 스크립트에 매핑하는 레지스트리 클래스입니다.
/// Lightroom Classic UI 요소와의 상호작용을 정의합니다.
/// FlaUIInspectData.md 기반으로 완전히 재작성됨
/// </summary>
public static class FeatureSelectorRegistry
{
    private static readonly Dictionary<string, FeatureScript> _registry = new();

    /// <summary>
    /// 정적 생성자 - 자동화 스크립트 등록
    /// </summary>
    static FeatureSelectorRegistry()
    {
        RegisterStartPhotoSessionScript();
        RegisterExportPhotosScript();

        var registeredCount = GetRegisteredFeatureIds().Count();
        LoggingService.LogInfo($"FeatureSelectorRegistry 초기화 완료 (등록된 스크립트: {registeredCount}개)");
    }

    /// <summary>
    /// 촬영 시작 자동화 스크립트 등록
    /// FlaUIInspectData.md의 촬영 매니저 섹션 기반으로 완전히 재작성
    /// </summary>
    private static void RegisterStartPhotoSessionScript()
    {
        RegisterScript("Lightroom.StartPhotoSession", new FeatureScript
        {
            FeatureId = "Lightroom.StartPhotoSession",
            Description = "촬영 시작 자동화",
            Steps =
            [

                new AutomationStep
                {
                    Description = "1. 왼쪽 상단 파일 메뉴 클릭",
                    Selector = new UiaSelector
                    {
                        Name = "파일(F)",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "2. 메뉴 드롭다운 확인",
                    Selector = new UiaSelector
                    {
                        Name = "파일(F)",
                        ClassName = "#32768",
                        ControlType = "Menu"
                    },
                    Action = "WaitForElement"
                },

                new AutomationStep
                {
                    Description = "3. 연결전송된 촬영 메뉴 클릭",
                    Selector = new UiaSelector
                    {
                        Name = "연결전송된 촬영",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "4. 연결전송된 촬영 드롭다운 확인",
                    Selector = new UiaSelector
                    {
                        Name = "파일(F)",
                        ClassName = "#32768",
                        ControlType = "Menu"
                    },
                    Action = "WaitForElement"
                },

                new AutomationStep
                {
                    Description = "5. 연결전송된 촬영 시작 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "40506",
                        Name = "연결전송된 촬영 시작...",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "6. '연결전송된 촬영 설정' 윈도우 팝업 확인",
                    Selector = new UiaSelector
                    {
                        Name = "연결전송된 촬영 설정",
                        ClassName = "Afx:0000000140000000:0",
                        ControlType = "Window"
                     },
                    Action = "WaitForElement"

                },

            ]
        });
    }

    /// <summary>
    /// featureId에 해당하는 자동화 스크립트를 가져옵니다.
    /// </summary>
    /// <param name="featureId">기능 식별자 (예: "Lightroom.StartPhotoSession")</param>
    /// <returns>스크립트가 존재하면 반환, 없으면 null</returns>
    public static FeatureScript? Get(string featureId)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            LoggingService.LogWarn("빈 featureId로 Get() 호출됨");
            return null;
        }

        if (_registry.TryGetValue(featureId, out var script))
        {
            LoggingService.LogInfo($"FeatureScript 조회 성공: {featureId}");
            return script;
        }

        LoggingService.LogWarn($"FeatureScript를 찾을 수 없음: {featureId}");
        return null;
    }

    /// <summary>
    /// featureId와 스크립트를 등록합니다. (향후 확장용)
    /// </summary>
    /// <param name="featureId">기능 식별자</param>
    /// <param name="script">자동화 스크립트</param>
    public static void RegisterScript(string featureId, FeatureScript script)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            LoggingService.LogWarn("빈 featureId로 RegisterScript() 호출됨");
            return;
        }

        if (script == null)
        {
            LoggingService.LogWarn($"null script로 RegisterScript() 호출됨: {featureId}");
            return;
        }

        _registry[featureId] = script;
        LoggingService.LogInfo($"FeatureScript 등록 완료: {featureId} - {script.Description}");
    }

    /// <summary>
    /// 등록된 모든 featureId 목록을 가져옵니다. (디버깅용)
    /// </summary>
    /// <returns>등록된 featureId 목록</returns>
    public static IEnumerable<string> GetRegisteredFeatureIds()
    {
        return _registry.Keys;
    }
}