---
agent: Agent_CoreServices
task_ref: Task 4.6 - TopMostManager HandOffToLightroom 메서드 추가
status: Completed
ad_hoc_delegation: false
compatibility_issues: false
important_findings: false
---

# Task Log: TopMostManager HandOffToLightroom 메서드 추가

## Summary
- TopMostManager에 HandOffToLightroomAsync() 메서드를 추가하여 Lightroom으로 포커스를 넘기고 자동화 후 앱 창으로 복원하는 기능을 구현했습니다.
- WindowFocusService를 주입받아 포커스 제어 및 재시도 로직을 구현했습니다.
- 자동화 실행 중 예외 발생 시에도 앱 창 복원을 보장합니다.

## Implementation Notes

### ITopMostManager 인터페이스 확장
- **Services/ITopMostManager.cs**:
  - `Task<bool> HandOffToLightroomAsync(Func<Task> automationTask, CancellationToken ct)` 메서드 추가
  - 반환값: 성공 시 true, 실패 시 false

### TopMostManager 구현 수정
- **Services/TopMostManager.cs**:
  - **생성자 변경**:
    - `IWindowFocusService windowFocusService` 파라미터 추가
    - 필드 `_windowFocusService` 저장
    - ArgumentNullException 검증

  - **HandOffToLightroomAsync()**:
    1. **Lightroom 활성화**: `_windowFocusService.TryActivateLightroomAsync(ct)` 호출
       - 실패 시 false 반환, 자동화 중단
    2. **앱 TopMost 해제**: `SetAppTopMost(false)` 호출 (Lightroom이 최상위 유지)
    3. **안정화 대기**: 500ms (포커스 전환 안정화)
    4. **자동화 실행**: `await automationTask()` 호출
    5. **앱 창 복원**: `RestoreAppFocusWithRetryAsync(ct)` 호출 (재시도 포함)

  - **예외 처리**:
    - `OperationCanceledException`: 취소 시에도 앱 창 복원 시도
    - `Exception`: 모든 예외 발생 시 앱 창 복원 시도, 로그 기록

  - **RestoreAppFocusWithRetryAsync()**:
    - 최대 3회 재시도 (1초 간격)
    - 각 시도마다:
      1. `_windowFocusService.RestoreAppWindow()` 호출
      2. 500ms 대기 (안정화)
      3. `SetAppTopMost(true)` 호출
    - 성공 시 true 반환
    - 모든 시도 실패 시 false 반환, 경고 로그

### DI 통합
- **Bootstrap/CompositionRoot.cs**:
  - TopMostManager 생성자에 WindowFocusService 주입
  - `sp.GetRequiredService<IWindowFocusService>()` 사용

## Technical Details

### 포커스 핸드오프 흐름
1. **앱 TopMost** → **Lightroom 활성화** → **앱 TopMost 해제**
2. **자동화 실행**
3. **앱 창 복원** → **앱 TopMost 설정**

### 재시도 정책
- **Lightroom 활성화**: WindowFocusService에서 3회 재시도
- **앱 창 복원**: TopMostManager에서 3회 재시도 (1초 간격)

### 안정화 대기
- Lightroom 활성화 후: 500ms
- 앱 창 복원 후: 500ms (각 재시도마다)
- 목적: 포커스 전환 안정화, UI 렌더링 완료 대기

### 로깅
- INFO: 핸드오프 시작, 자동화 실행 중/완료, 앱 창 복원 성공
- WARN: Lightroom 활성화 실패, 취소, 오류, 복원 재시도, 복원 실패

## Build / Verification
- Command: `dotnet build -p:EnableWindowsTargeting=true`
- Result: ✅ 성공 (경고 0개, 오류 0개)

## Output Files
- Services/ITopMostManager.cs (수정 - HandOffToLightroomAsync 추가)
- Services/TopMostManager.cs (수정 - WindowFocusService 주입 및 HandOffToLightroomAsync 구현)
- Bootstrap/CompositionRoot.cs (수정 - TopMostManager 생성 시 WindowFocusService 주입)

## Follow-ups / Risks
- Task 4.7에서 MainViewModel의 ButtonACommand/ButtonBCommand에서 TopMostManager.HandOffToLightroomAsync() 사용 예정
- 자동화 실행 중 Lightroom 최소화/종료 시나리오는 AutomationService에서 처리 (프로세스 검증)
- Windows 보안 정책에 따라 포커스 전환이 차단될 수 있음 → 재시도 로직으로 대응

## Notes
- HandOffToLightroomAsync()는 자동화 태스크를 파라미터로 받아 실행 (Func<Task>)
- 예외 발생 시에도 반드시 앱 창 복원 시도 (finally 없이 catch에서 처리)
- CancellationToken.None으로 복원 시도 (취소되어도 앱 창은 복원 필요)
- TopMost 상태 관리와 포커스 제어를 통합하여 일관된 UX 제공
