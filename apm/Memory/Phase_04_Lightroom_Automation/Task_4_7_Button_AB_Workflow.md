---
agent: Agent_UI_Features
task_ref: Task 4.7 - MainViewModel 버튼 a/b 명령 구현
status: Completed
ad_hoc_delegation: false
compatibility_issues: false
important_findings: false
---

# Task Log: MainViewModel 버튼 a/b 명령 구현

## Summary
- MainViewModel의 ButtonACommand와 ButtonBCommand를 구현하여 입력 폼 → 검증 → 자동화 → 결과 표시의 전체 워크플로우를 완성했습니다.
- InputFormDialog, ValidationService, AutomationService, TopMostManager를 통합했습니다.
- 사용자 입력값을 `{성함}{뒤4자리}` 형식으로 조합하여 자동화에 전달합니다.

## Implementation Notes

### MainViewModel 수정
- **ViewModels/MainViewModel.cs**:
  - **새 의존성 주입**:
    - `IAutomationService _automationService`
    - `ITopMostManager _topMostManager`
    - 생성자에서 ArgumentNullException 검증

  - **ExecuteButtonAAsync() (촬영 시작)**:
    1. **입력 폼 표시**: `new InputFormDialog("촬영 시작")` 생성 및 ShowDialog() 호출
    2. **취소 처리**: DialogResult != true → 로그 기록 후 종료
    3. **입력값 조합**: `{name}{phone}` 형식 (공백/구분자 없음)
       - 예: "홍길동" + "1234" → `홍길동1234`
    4. **자동화 실행**:
       - `Dictionary<string, string> { ["customerInput"] = customerInput }` 생성
       - `TopMostManager.HandOffToLightroomAsync()` 호출
         - 람다 내부에서 `AutomationService.RunAsync("Lightroom.StartPhotoSession", inputValues, ct)` 실행
         - 실패 시 InvalidOperationException throw
    5. **결과 표시**:
       - 성공: ToastService.Show("촬영 시작 자동화가 완료되었습니다.", 3000)
       - 실패: DialogService.ShowDialog("자동화 실패", ...)
    6. **예외 처리**: 모든 예외를 catch하여 사용자에게 오류 다이얼로그 표시

  - **ExecuteButtonBAsync() (내보내기)**:
    - ButtonA와 동일한 흐름
    - 확인 버튼 텍스트: "내보내기 시작"
    - featureId: "Lightroom.ExportPhotos"
    - 성공 메시지: "내보내기 자동화가 완료되었습니다."

## Workflow Details

### 전체 흐름 (PRD §1.2 구현)
1. **사용자가 버튼 클릭**
2. **InputFormDialog 표시** (Task 4.1)
   - "예약자 성함" 입력
   - "휴대폰 뒤4자리" 입력
   - "확인" 버튼 클릭
3. **ValidationService 검증** (Task 4.2)
   - 성함: 비어있지 않음
   - 휴대폰: 정확히 4자리 숫자
   - 실패 시 에러 다이얼로그, 자동화 중단
4. **입력값 조합**
   - `{성함}{뒤4자리}` 형식 (공백/구분자 없음)
5. **TopMostManager.HandOffToLightroomAsync()** (Task 4.6)
   - Lightroom Classic 창 활성화 (Task 4.3)
   - 앱 TopMost 해제
   - 자동화 실행
   - 앱 창 복원 및 TopMost 설정
6. **AutomationService.RunAsync()** (Task 4.5)
   - FeatureSelectorRegistry에서 스크립트 조회 (Task 4.4)
   - FlaUI로 Lightroom UI 자동화 실행
   - 최대 3회 재시도
7. **결과 표시**
   - 성공: Toast 알림
   - 실패: Dialog 경고

### 입력값 포맷 (PRD §FR-02)
- **조합 형식**: `{성함}{뒤4자리}` (공백/구분자 없음)
- **예시**:
  - "홍길동" + "1234" → `홍길동1234`
  - "Kim Jane" + "5678" → `Kim Jane5678`
- **전달**: `Dictionary<string, string> { ["customerInput"] = "홍길동1234" }`
- **사용**: AutomationService의 SetText 액션에서 `{customerInput}` 플레이스홀더 치환

## Build / Verification
- Command: `dotnet build -p:EnableWindowsTargeting=true`
- Result: ✅ 성공 (경고 0개, 오류 0개)

## Output Files
- ViewModels/MainViewModel.cs (수정 - AutomationService, TopMostManager 주입 및 ButtonA/B 구현)

## Follow-ups / Risks
- **스크립트 등록 필요**: FeatureSelectorRegistry에 "Lightroom.StartPhotoSession", "Lightroom.ExportPhotos" 스크립트 등록 필요
  - 현재 빈 레지스트리이므로 AutomationService.RunAsync()는 "스크립트를 찾을 수 없음" 에러 반환
  - 사용자와 협업하여 실제 Lightroom Classic UI 요소 식별 및 스크립트 작성 필요 (Ad-Hoc Delegation)
- **Lightroom 실행 필요**: 자동화 실행 시 Lightroom Classic이 실행 중이어야 함
- **입력값 폐기**: 자동화 완료 후 inputValues는 메모리에서 해제됨 (다음 실행 시 재입력)

## Integration Points
- **Task 4.1** (InputFormDialog): 입력 폼 UI 및 ViewModel
- **Task 4.2** (ValidationService): 입력값 검증 (InputFormDialog에 통합)
- **Task 4.3** (WindowFocusService): Lightroom 창 활성화 및 앱 창 복원
- **Task 4.4** (FeatureSelectorRegistry): featureId → 스크립트 매핑
- **Task 4.5** (AutomationService): FlaUI 기반 Lightroom UI 자동화
- **Task 4.6** (TopMostManager): 포커스 핸드오프 및 TopMost 관리

## Notes
- 현재 구현은 모든 Phase 4 구성 요소를 통합하여 엔드투엔드 워크플로우를 완성했습니다.
- FeatureSelectorRegistry에 스크립트를 등록하면 실제 Lightroom Classic 자동화를 실행할 수 있습니다.
- 입력값 검증은 InputFormDialog에서 수행되므로 MainViewModel은 검증된 값만 받습니다.
- CancellationTokenSource는 각 자동화마다 새로 생성하여 독립적인 취소 제어를 제공합니다.
- ToastService와 DialogService를 적절히 사용하여 UX를 개선했습니다.
