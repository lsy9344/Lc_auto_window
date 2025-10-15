using System.Collections.Generic;
using System.Linq;

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
                    Action = "Wait",
                    ActionData = "500"
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
                    Action = "Wait",
                    ActionData = "500"
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
                    Action = "Wait",
                    ActionData = "1000"

                },

                new AutomationStep
                {
                    Description = "7. 세션 이름 입력창에 붙여넣기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "세션 이름:",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "{customerInput}"
                },

                new AutomationStep
                {
                    Description = "8. '숏별로 사진 나누기' 체크박스 해제",
                    Selector = new UiaSelector
                    {
                        Name = "숏별로 사진 나누기",
                        ControlType = "CheckBox"
                    },
                    Action = "ToggleCheckBox",
                    ActionData = "off"
                },

                new AutomationStep
                {
                    Description = "9. 사용자 정의 이름 템플릿 콤보박스 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "2134",
                        Name = "템플릿:",
                        ControlType = "ComboBox"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "10. 사용자 정의 이름 템플릿 드롭다운 확인",
                    Selector = new UiaSelector
                    {
                        Name = "컨텍스트",
                        ClassName = "#32768",
                        ControlType = "Menu"
                    },
                    Action = "Wait",
                    ActionData = "500"
                },

                new AutomationStep
                {
                    Description = "11. 콤보박스에서 원본 파일 번호 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "3",
                        Name = "사용자 정의 이름 - 원본 파일 번호",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "12. '원본' 텍스트 입력",
                    Selector = new UiaSelector
                    {
                        AutomationId = "2136",
                        Name = "사용자 정의 텍스트:",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "원본"
                },

                new AutomationStep
                {
                    Description = "13. 경로 '선택...' 버튼 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "선택...",
                        ControlType = "Button"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "14. 폴더 선택 윈도우 확인",
                    Selector = new UiaSelector
                    {
                        Name = "폴더 선택",
                        ClassName = "#32770",
                        ControlType = "Window"
                    },
                    Action = "WaitForFolderDialog"
                },

                new AutomationStep
                {
                    Description = "15. 주소창 클릭하여 경로 붙여넣기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1001",
                        ClassName = "ToolbarWindow32",
                        ControlType = "ToolBar"
                    },
                    Action = "SendKeys",
                    ActionData = "{targetFolder}"
                },

                new AutomationStep
                {
                    Description = "16. 카메라 오류 확인 (2초간 체크)",
                    Action = "CheckCameraErrors"
                },

            ]
        });
    }

    /// <summary>
    /// 사진 내보내기 자동화 스크립트 등록
    /// FlaUIInspectData.md의 내보내기 매니저 섹션 기반으로 완전히 재작성
    /// </summary>
    private static void RegisterExportPhotosScript()
    {
        RegisterScript("Lightroom.ExportPhotos", new FeatureScript
        {
            FeatureId = "Lightroom.ExportPhotos",
            Description = "사진 내보내기 자동화",
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
                    Action = "Wait",
                    ActionData = "500"
                },
                new AutomationStep
                {
                    Description = "3. 내보내기 메뉴 클릭",
                    Selector = new UiaSelector
                    {
                        Name = "내보내기",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },
                new AutomationStep
                {
                    Description = "4. 내보내기 드롭다운 확인",
                    Selector = new UiaSelector
                    {
                        Name = "파일(F)",
                        ClassName = "#32768",
                        ControlType = "Menu"
                    },
                    Action = "Wait",
                    ActionData = "500"
                },
                new AutomationStep
                {
                    Description = "5. 다른 이름으로 내보내기 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "40487",
                        Name = "다른 이름으로 내보내기...",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },
                new AutomationStep
                {
                    Description = "6. '내보내기' 대화상자 확인",
                    Selector = new UiaSelector
                    {
                        Name = "다른 이름으로 내보내기",
                        ClassName = "#32770",
                        ControlType = "Window"
                    },
                    Action = "Wait",
                    ActionData = "1000"
                }
            ]
        });
    }

    /// <summary>
    /// 자동화 스크립트를 등록합니다.
    /// </summary>
    private static void RegisterScript(string featureId, FeatureScript script)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            throw new ArgumentException("featureId가 비어 있습니다.", nameof(featureId));
        }

        if (script == null)
        {
            throw new ArgumentNullException(nameof(script));
        }

        _registry[featureId] = script;
    }

    /// <summary>
    /// 등록된 모든 featureId 목록을 반환합니다.
    /// </summary>
    public static IEnumerable<string> GetRegisteredFeatureIds()
    {
        return _registry.Keys.ToList();
    }

    /// <summary>
    /// 지정된 featureId에 해당하는 자동화 스크립트를 가져옵니다.
    /// </summary>
    public static FeatureScript? Get(string featureId)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            return null;
        }

        _registry.TryGetValue(featureId, out var script);
        return script;
    }
}
