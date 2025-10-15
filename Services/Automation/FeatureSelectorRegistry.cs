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

                new AutomationStep
                {
                    Description = "세션 이름 입력",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "세션 이름:",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "{customerInput}"
                },

                // 5. "숏별로 사진 나누기" 체크박스 해제 (정확한 정보 기반)
                new AutomationStep
                {
                    Description = "숏별로 사진 나누기 체크박스 해제",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "숏별로 사진 나누기",
                        ControlType = "CheckBox"
                    },
                    Action = "ToggleCheckBox",
                    ActionData = "Off"
                },

                // 6. 템플릿 콤보박스 클릭
                new AutomationStep
                {
                    Description = "템플릿 콤보박스 클릭",
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
                    Description = "콤보박스 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 7. 템플릿 항목 선택
                new AutomationStep
                {
                    Description = "사용자 정의 이름 - 원본 파일 번호 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "3",
                        Name = "사용자 정의 이름 - 원본 파일 번호",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                // 8. 사용자 정의 텍스트 입력
                new AutomationStep
                {
                    Description = "사용자 정의 텍스트 입력 (원본)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "2136",
                        Name = "사용자 정의 텍스트:",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "원본"
                },

                // 9. 대상 - 선택 버튼 클릭
                new AutomationStep
                {
                    Description = "대상 - 선택 버튼 클릭",
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
                    Description = "폴더 선택 대화상자 대기",
                    Selector = new UiaSelector { },
                    Action = "WaitForFolderDialog"
                },

                // 10. 주소창 클릭 후 경로 입력
                new AutomationStep
                {
                    Description = "주소창 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1001",
                        Name = "폴더 선택",
                        ControlType = "Button"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "폴더 경로 입력",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1001",
                        ClassName = "ToolbarWindow32",
                        ControlType = "ToolBar"
                    },
                    Action = "ClearAndType",
                    ActionData = "{shootFolder}"
                },

                new AutomationStep
                {
                    Description = "경로 입력 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 11. 폴더 선택 확인 버튼
                new AutomationStep
                {
                    Description = "폴더 선택 확인",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "폴더 선택",
                        ControlType = "Button"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "폴더 선택 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 12. 최종 확인 버튼
                new AutomationStep
                {
                    Description = "설정 완료 - 확인 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "확인",
                        ControlType = "Button"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "촬영 시작 완료 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "2000"
                },

                // === 카메라 설정 자동화 (FlaUIInspectData.md 기반) ===

                // 13. 셔터 설정
                new AutomationStep
                {
                    Description = "셔터 텍스트 라벨 찾기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1412051328",
                        Name = "셔터:",
                        ControlType = "Text"
                    },
                    Action = "SetCameraParameter",
                    ActionData = "shutter:ShutterSpeed"
                },

                new AutomationStep
                {
                    Description = "셔터 설정 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 14. 조리개 설정
                new AutomationStep
                {
                    Description = "조리개 텍스트 라벨 찾기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1412053888",
                        Name = "조리개:",
                        ControlType = "Text"
                    },
                    Action = "SetCameraParameter",
                    ActionData = "aperture:Aperture"
                },

                new AutomationStep
                {
                    Description = "조리개 설정 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 15. ISO 설정
                new AutomationStep
                {
                    Description = "ISO 텍스트 라벨 찾기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1412049280",
                        Name = "ISO:",
                        ControlType = "Text"
                    },
                    Action = "SetCameraParameter",
                    ActionData = "iso:ISO"
                },

                new AutomationStep
                {
                    Description = "ISO 설정 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 16. WB (화이트 밸런스) 설정
                new AutomationStep
                {
                    Description = "WB 텍스트 라벨 찾기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1412048768",
                        Name = "WB:",
                        ControlType = "Text"
                    },
                    Action = "SetCameraParameter",
                    ActionData = "wb:WhiteBalance"
                },

                new AutomationStep
                {
                    Description = "WB 설정 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 17. 카메라 오류 확인 (2초 동안)
                new AutomationStep
                {
                    Description = "카메라 오류 확인",
                    Selector = new UiaSelector { },
                    Action = "CheckCameraErrors"
                }
            ]
        });
    }

    /// <summary>
    /// 내보내기 자동화 스크립트 등록
    /// FlaUIInspectData.md의 내보내기 매니저 섹션 기반으로 완전히 재작성
    /// </summary>
    private static void RegisterExportPhotosScript()
    {
        RegisterScript("Lightroom.ExportPhotos", new FeatureScript
        {
            FeatureId = "Lightroom.ExportPhotos",
            Description = "내보내기 자동화",
            Steps =
            [
                // 1. 라이브러리 모듈 전환 준비
                new AutomationStep
                {
                    Description = "라이브러리 모듈 창 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "(D2D Bridge View)",
                        ClassName = "AfxWnd140u",
                        ControlType = "Pane"
                    },
                    Action = "Click"
                },

                // 2. 전체 선택 (Ctrl+A)
                new AutomationStep
                {
                    Description = "전체 선택 (Ctrl+A)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "(D2D Bridge View)",
                        ClassName = "AfxWnd140u",
                        ControlType = "Pane"
                    },
                    Action = "SendKeys",
                    ActionData = "Ctrl+A"
                },

                new AutomationStep
                {
                    Description = "전체 선택 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 3. 파일 메뉴 클릭
                new AutomationStep
                {
                    Description = "파일 메뉴 열기",
                    Selector = new UiaSelector
                    {
                        Name = "파일(F)",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "메뉴 로드 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 4. 내보내기 메뉴 클릭
                new AutomationStep
                {
                    Description = "내보내기 메뉴 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "40513",
                        Name = "내보내기(E)...",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "내보내기 대화상자 로드 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "2000"
                },

                // === 내보내기 위치 설정 ===

                // 5. 내보내기 위치 섹션 상태 확인
                new AutomationStep
                {
                    Description = "내보낼 위치 텍스트 확인",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1817901184",
                        Name = "내보낼 위치:",
                        ControlType = "Text"
                    },
                    Action = "Wait",
                    ActionData = "100"
                },

                // 6. 내보내기 위치 섹션 클릭 (필요시 확장)
                new AutomationStep
                {
                    Description = "내보내기 위치 섹션 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "-1574358304",
                        Name = "내보내기 위치",
                        ControlType = "Text"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "섹션 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                // 7. 내보낼 위치 콤보박스 클릭
                new AutomationStep
                {
                    Description = "내보낼 위치 콤보박스 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "내보낼 위치:",
                        ControlType = "ComboBox"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "콤보박스 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "특정 폴더 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "특정 폴더",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "선택 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                // 8. 폴더 선택 버튼 클릭
                new AutomationStep
                {
                    Description = "폴더 선택 버튼 클릭",
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
                    Description = "폴더 선택 대화상자 대기",
                    Selector = new UiaSelector { },
                    Action = "WaitForFolderDialog"
                },

                new AutomationStep
                {
                    Description = "주소창 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1001",
                        Name = "주소:",
                        ControlType = "ToolBar"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "내보내기 폴더 경로 입력",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1001",
                        ClassName = "ToolbarWindow32",
                        ControlType = "ToolBar"
                    },
                    Action = "ClearAndType",
                    ActionData = "{exportFolder}"
                },

                new AutomationStep
                {
                    Description = "경로 입력 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                new AutomationStep
                {
                    Description = "폴더 선택 확인",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "폴더 선택",
                        ControlType = "Button"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "폴더 선택 후 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "500"
                },

                // 9. 하위 폴더에 넣기 체크박스 켜기
                new AutomationStep
                {
                    Description = "하위 폴더에 넣기 체크박스 켜기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "하위 폴더에 넣기:",
                        ControlType = "CheckBox"
                    },
                    Action = "ToggleCheckBox",
                    ActionData = "On"
                },

                // 10. 하위 폴더 이름 입력 (상대 위치 기반 탐색)
                new AutomationStep
                {
                    Description = "하위 폴더 이름 입력창 찾기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "하위 폴더에 넣기:",
                        ControlType = "CheckBox"
                    },
                    Action = "FindRelativeElement",
                    ActionData = "65535::Edit"
                },

                new AutomationStep
                {
                    Description = "하위 폴더 이름 입력 (고객명)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        ClassName = "Edit",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "{customerInput}"
                },

                // === 파일 이름 지정 설정 ===

                new AutomationStep
                {
                    Description = "파일 이름 지정 섹션 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "-1744491968",
                        Name = "파일 이름 지정",
                        ControlType = "Text"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "섹션 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "바꿀 이름 체크박스 켜기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "바꿀 이름:",
                        ControlType = "CheckBox"
                    },
                    Action = "ToggleCheckBox",
                    ActionData = "On"
                },

                new AutomationStep
                {
                    Description = "이름 템플릿 콤보박스 클릭",
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
                    Description = "사용자 정의 이름 - 원본 파일 번호 선택",
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
                    Description = "사용자 정의 텍스트 입력 (필터)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "2136",
                        Name = "사용자 정의 텍스트:",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "필터"
                },

                // === 파일 설정 ===

                new AutomationStep
                {
                    Description = "파일 설정 섹션 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "-1799002016",
                        Name = "파일 설정",
                        ControlType = "Text"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "섹션 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "이미지 형식 콤보박스 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "이미지 형식:",
                        ControlType = "ComboBox"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "콤보박스 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "JPEG 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "JPEG",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                // 품질 Slider 옆 Edit 찾기 (상대 위치 기반 탐색)
                new AutomationStep
                {
                    Description = "품질 Slider 찾기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "품질:",
                        ControlType = "Slider"
                    },
                    Action = "FindRelativeElement",
                    ActionData = "65535::Edit"
                },

                new AutomationStep
                {
                    Description = "품질 값 입력 (90)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        ClassName = "Edit",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "90"
                },

                new AutomationStep
                {
                    Description = "파일 크기 제한 체크박스 끄기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "다음으로 파일 크기 제한:",
                        ControlType = "CheckBox"
                    },
                    Action = "ToggleCheckBox",
                    ActionData = "Off"
                },

                // === 이미지 크기 조정 ===

                new AutomationStep
                {
                    Description = "이미지 크기 조정 섹션 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "-1853753216",
                        Name = "이미지 크기 조정",
                        ControlType = "Text"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "섹션 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "크기 조정하여 맞추기 체크박스 끄기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "크기 조정하여 맞추기:",
                        ControlType = "CheckBox"
                    },
                    Action = "ToggleCheckBox",
                    ActionData = "Off"
                },

                new AutomationStep
                {
                    Description = "크기 조정 방법 콤보박스 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        ControlType = "ComboBox"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "콤보박스 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "너비 및 높이 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "너비 및 높이",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "확대 안 함 체크박스 끄기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "확대 안 함",
                        ControlType = "CheckBox"
                    },
                    Action = "ToggleCheckBox",
                    ActionData = "Off"
                },

                // 너비 입력 (2456)
                new AutomationStep
                {
                    Description = "너비 입력 (2456)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "%",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "2456"
                },

                // 높이 입력 (4000)
                new AutomationStep
                {
                    Description = "높이 입력 (4000)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "높이:",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "4000"
                },

                // 단위 선택 (픽셀)
                new AutomationStep
                {
                    Description = "단위 콤보박스 클릭 (메가픽셀 → 픽셀)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "메가픽셀",
                        ControlType = "ComboBox"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "콤보박스 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "픽셀 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "픽셀",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                // 해상도 입력 (160)
                new AutomationStep
                {
                    Description = "해상도 콤보박스 클릭 (단위 선택)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        ControlType = "ComboBox"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "콤보박스 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "인치당 픽셀 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "인치당 픽셀",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "해상도 값 입력 (160)",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "해상도:",
                        ControlType = "Edit"
                    },
                    Action = "ClearAndType",
                    ActionData = "160"
                },

                // === 출력 선명하게 하기 ===

                new AutomationStep
                {
                    Description = "출력 선명하게 하기 섹션 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "-1853757824",
                        Name = "출력 선명하게 하기",
                        ControlType = "Text"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "섹션 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "선명하게 하기 체크박스 켜기",
                    Selector = new UiaSelector
                    {
                        AutomationId = "100",
                        Name = "선명하게 하기:",
                        ControlType = "CheckBox"
                    },
                    Action = "ToggleCheckBox",
                    ActionData = "On"
                },

                new AutomationStep
                {
                    Description = "선명하게 하기 대상 콤보박스 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        ControlType = "ComboBox"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "콤보박스 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "화면 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "화면",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "양 콤보박스 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "65535",
                        Name = "양:",
                        ControlType = "ComboBox"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "콤보박스 확장 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "300"
                },

                new AutomationStep
                {
                    Description = "고 선택",
                    Selector = new UiaSelector
                    {
                        AutomationId = "3",
                        Name = "고",
                        ControlType = "MenuItem"
                    },
                    Action = "Click"
                },

                // === 내보내기 실행 ===

                new AutomationStep
                {
                    Description = "내보내기 버튼 클릭",
                    Selector = new UiaSelector
                    {
                        AutomationId = "1",
                        Name = "내보내기",
                        ControlType = "Button"
                    },
                    Action = "Click"
                },

                new AutomationStep
                {
                    Description = "내보내기 처리 대기",
                    Selector = new UiaSelector { },
                    Action = "Wait",
                    ActionData = "2000"
                },

                // 덮어쓰기 경고 처리
                new AutomationStep
                {
                    Description = "덮어쓰기 경고 확인 및 처리",
                    Selector = new UiaSelector { },
                    Action = "HandleOverwriteWarning"
                }
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