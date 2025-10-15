---
agent: Agent_FeatureServices
task_ref: Task 4.5 - AutomationService 구현 (FlaUI 기반)
status: Completed
ad_hoc_delegation: false
compatibility_issues: false
important_findings: false
---

# Task Log: AutomationService 구현 (FlaUI 기반)

## Summary
- FlaUI.UIA3 v4.0.0 기반 Lightroom Classic UI 자동화 서비스를 구현했습니다.
- FeatureSelectorRegistry에서 스크립트를 로드하여 순차적으로 실행합니다.
- 타임아웃 30초, 최대 2회 재시도 (총 3회 시도) 정책을 구현했습니다.
- CancellationToken 지원으로 사용자 취소 가능합니다.

## Implementation Notes

### 패키지 설치
- **FlaUI.UIA3 v4.0.0** 설치 완료
  - FlaUI.Core v4.0.0 (종속성)
  - Interop.UIAutomationClient v10.19041.0 (종속성)

### 파일 생성
- **Services/Automation/AutomationResult.cs**:
  - 자동화 실행 결과 클래스
  - `Success`, `Message`, `ElapsedMs`, `Exception` 속성
  - `CreateSuccess()`, `CreateFailure()` 팩토리 메서드

- **Services/Automation/IAutomationService.cs**:
  - `Task<AutomationResult> RunAsync(featureId, inputValues, ct)` 메서드

- **Services/Automation/AutomationService.cs**:
  - FlaUI.UIA3.UIA3Automation 사용
  - **RunAsync()**:
    - FeatureSelectorRegistry.Get(featureId)로 스크립트 로드
    - 스크립트가 없으면 실패 반환 (현재 빈 레지스트리)
    - 재시도 로직: 초기 시도 1회 + 재시도 2회 = 총 3회
    - 각 시도 간 1초 대기
    - CancellationToken 지원
  - **ExecuteScriptAsync()**:
    - Lightroom/Adobe Lightroom Classic 프로세스 찾기
    - Application.Attach()로 앱 연결
    - GetMainWindow()로 메인 창 획득
    - 각 AutomationStep 순차 실행
    - 스텝 간 500ms 대기 (UI 안정화)
  - **FindElement()**:
    - UiaSelector (AutomationId, Name, ClassName, ControlType)로 UI 요소 찾기
    - ConditionFactory로 조건 생성
    - And() 조합으로 복합 조건 지원
    - FindFirstDescendant()로 요소 탐색
  - **PerformActionAsync()**:
    - "click": element.Click()
    - "settext": element.AsTextBox().Text 설정, inputValues 플레이스홀더 치환
    - "wait": Task.Delay(ms)
  - **ResolveText()**:
    - ActionData의 `{key}` 플레이스홀더를 inputValues[key]로 치환
  - IDisposable 구현으로 UIA3Automation 리소스 해제

### DI 통합
- **Bootstrap/CompositionRoot.cs**:
  - `IAutomationService` 싱글턴 등록

## Technical Details

### FlaUI v4 API 사용
- ControlType 매핑: 문자열 → FlaUI.Core.Definitions.ControlType
  - "button" → ControlType.Button
  - "edit" → ControlType.Edit
  - "text" → ControlType.Text
  - "window" → ControlType.Window
  - "pane" → ControlType.Pane
  - "menuitem" → ControlType.MenuItem
- 조건 조합: `conditions.Aggregate((a, b) => a.And(b))`

### 재시도 정책
- 최대 3회 시도 (초기 1회 + 재시도 2회)
- 재시도 간격: 1초
- 각 시도마다 로그 기록 (시도 X/3)

### 로깅
- INFO: 초기화, 시작, 성공, 스텝 실행, 클릭/입력 완료
- WARN: 재시도, 실패, 취소, UI 요소 탐색 오류

## Build / Verification
- Command: `dotnet build -p:EnableWindowsTargeting=true`
- Result: ✅ 성공 (경고 0개, 오류 0개)

## Output Files
- Services/Automation/AutomationResult.cs (신규)
- Services/Automation/IAutomationService.cs (신규)
- Services/Automation/AutomationService.cs (신규)
- Bootstrap/CompositionRoot.cs (수정 - AutomationService 등록)

## Follow-ups / Risks
- Task 4.6에서 TopMostManager에 AutomationService 통합 예정 (HandOffToLightroomAsync)
- Task 4.7에서 MainViewModel의 ButtonACommand/ButtonBCommand에서 AutomationService 사용 예정
- **Ad-Hoc Delegation 필요**: 실제 Lightroom Classic UI 요소 식별 및 스크립트 작성
  - FeatureSelectorRegistry에 "Lightroom.StartPhotoSession", "Lightroom.ExportPhotos" 스크립트 등록
  - 각 UI 요소의 AutomationId, Name, ClassName 확인
  - 클릭/입력 순서 정의
- 현재 빈 레지스트리이므로 RunAsync() 호출 시 "스크립트를 찾을 수 없음" 에러 반환

## Notes
- FlaUI는 Lightroom Classic 같은 복잡한 앱에서 UI 요소 탐색이 느릴 수 있음
- Lightroom Classic 버전별로 UI 구조가 다를 수 있으므로 유연한 선택자 설계 권장
- inputValues 플레이스홀더 치환 예: `{customerInput}` → `홍길동1234`
- IDisposable 구현으로 AutomationService 해제 시 UIA3Automation 리소스 정리
