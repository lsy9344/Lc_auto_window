---
agent: Agent_FeatureServices
task_ref: Task 4.4 - FeatureSelectorRegistry 기본 구조
status: Completed
ad_hoc_delegation: false
compatibility_issues: false
important_findings: false
---

# Task Log: FeatureSelectorRegistry 기본 구조

## Summary
- featureId를 UIA 선택자 스크립트에 매핑하는 FeatureSelectorRegistry 기본 구조를 구현했습니다.
- 확장 가능한 설계로, 향후 사용자와 협업하여 실제 Lightroom Classic UI 요소를 추가할 수 있습니다.
- FeatureScript, AutomationStep, UiaSelector 데이터 클래스를 정의하여 자동화 스크립트 구조를 표준화했습니다.

## Implementation Notes
- **Services/Automation/FeatureScript.cs**:
  - `FeatureScript`: 자동화 스크립트 정의 클래스
    - `FeatureId`: 기능 식별자 (예: "Lightroom.StartPhotoSession")
    - `Description`: 기능 설명
    - `Steps`: 자동화 스텝 목록
  - `AutomationStep`: 개별 스텝 정의
    - `Description`: 스텝 설명 (로그용)
    - `Selector`: UIA 선택자 (AutomationId, Name, ClassName, ControlType)
    - `Action`: 수행할 액션 (Click, SetText, Wait 등)
    - `ActionData`: 액션 데이터 (예: SetText의 입력 텍스트)
  - `UiaSelector`: UI 요소 선택자
    - `AutomationId`, `Name`, `ClassName`, `ControlType` 속성

- **Services/Automation/FeatureSelectorRegistry.cs**:
  - 정적 클래스로 전역 접근 가능
  - `_registry`: Dictionary<string, FeatureScript> (현재 빈 Dictionary)
  - `Get(featureId)`: 스크립트 조회, 없으면 null 반환
  - `RegisterScript(featureId, script)`: 스크립트 등록 (향후 확장용)
  - `GetRegisteredFeatureIds()`: 등록된 featureId 목록 (디버깅용)
  - 정적 생성자에서 초기화 로그 기록

## Design Rationale
- **확장 가능한 구조**:
  - 현재는 빈 레지스트리로 시작
  - RegisterScript()로 동적 추가 가능
  - 향후 JSON 파일이나 코드로 스크립트 정의 가능

- **표준화된 스크립트 형식**:
  - FeatureScript로 모든 자동화를 일관되게 정의
  - AutomationStep으로 순차적 실행 가능
  - UiaSelector로 FlaUI 선택자 표준화

- **로깅 통합**:
  - 초기화, 조회, 등록 시 LoggingService 사용
  - 디버깅 및 문제 해결 용이

## Build / Verification
- Command: `dotnet build -p:EnableWindowsTargeting=true`
- Result: ✅ 성공 (경고 0개, 오류 0개)

## Output Files
- Services/Automation/FeatureScript.cs (신규)
- Services/Automation/FeatureSelectorRegistry.cs (신규)

## Follow-ups / Risks
- Task 4.5 AutomationService에서 FeatureSelectorRegistry.Get()을 사용하여 스크립트 로드 예정
- **Ad-Hoc Delegation 필요**: 실제 Lightroom Classic UI 요소 식별 및 스크립트 작성은 사용자와 협업 필요
  - featureId: "Lightroom.StartPhotoSession", "Lightroom.ExportPhotos"
  - 각 UI 요소의 AutomationId, Name, ClassName 확인
  - 클릭/입력 순서 정의
- 현재는 빈 레지스트리이므로 AutomationService 실행 시 null 반환 → 에러 핸들링 필요

## Notes
- FlaUI (v4.x) UIA3 Automation을 사용하여 Lightroom Classic UI 제어 예정
- Lightroom Classic은 Adobe 제품으로 UI 구조가 복잡할 수 있음
- 버전별로 UI 요소가 달라질 수 있으므로 유연한 선택자 설계 권장
- 향후 스크립트는 `config/automation/` 폴더에 JSON으로 외부화 가능
