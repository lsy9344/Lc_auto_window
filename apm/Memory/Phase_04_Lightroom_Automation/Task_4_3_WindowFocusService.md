---
agent: Agent_CoreServices
task_ref: Task 4.3 - WindowFocusService 구현
status: Completed
ad_hoc_delegation: false
compatibility_issues: false
important_findings: false
---

# Task Log: WindowFocusService 구현 (Lightroom 포커스 전환)

## Summary
- Lightroom Classic 창을 찾아 포어그라운드로 전환하는 WindowFocusService를 구현했습니다.
- 자동화 실행 전후로 앱 창과 Lightroom 창 간 포커스 전환을 담당합니다.
- Win32 API (FindWindow, SetForegroundWindow)를 사용하여 창 제어를 구현했습니다.

## Implementation Notes
- **Services/IWindowFocusService.cs**:
  - `Task<bool> TryActivateLightroomAsync(CancellationToken ct)`: Lightroom 창 활성화
  - `void RestoreAppWindow()`: 앱 창 포커스 복원
  - `void SetAppWindowHandle(nint handle)`: 앱 창 핸들 저장

- **Services/WindowFocusService.cs**:
  - `_appWindowHandle` 필드: 앱 창 핸들 저장
  - `LightroomWindowTitles` 배열: 여러 Lightroom 버전 대응
    - "Lightroom Classic"
    - "Adobe Lightroom Classic"
    - "Lightroom"
  - **TryActivateLightroomAsync()**:
    - 최대 3회 재시도 (1초 간격)
    - 여러 창 제목으로 FindWindow 시도
    - SetForegroundWindow로 포커스 전환
    - 성공/실패 로그 기록
  - **RestoreAppWindow()**:
    - 저장된 앱 창 핸들로 포커스 복원
    - 실패 시 경고 로그

- **Bootstrap/CompositionRoot.cs**:
  - `IWindowFocusService` 싱글턴 등록

- **App.xaml.cs**:
  - MainWindow.Show() 후 WindowInteropHelper로 핸들 획득
  - WindowFocusService.SetAppWindowHandle() 호출

## Technical Details
- **Win32 API 사용** (Interop/Win32.cs):
  - `FindWindow(null, windowTitle)`: 창 제목으로 창 찾기
  - `SetForegroundWindow(hWnd)`: 창을 포어그라운드로 전환
  - `GetForegroundWindow()`: 현재 포어그라운드 창 핸들 가져오기

- **재시도 정책**:
  - 최대 3회 시도
  - 재시도 간격: 1초
  - CancellationToken 지원

- **로깅**:
  - INFO: 성공적인 활성화, 핸들 설정
  - WARN: 재시도, 실패, 창을 찾지 못함

## Build / Verification
- Command: `dotnet build -p:EnableWindowsTargeting=true`
- Result: ✅ 성공 (경고 0개, 오류 0개)

## Output Files
- Services/IWindowFocusService.cs (신규)
- Services/WindowFocusService.cs (신규)
- Bootstrap/CompositionRoot.cs (수정 - WindowFocusService 등록)
- App.xaml.cs (수정 - SetAppWindowHandle 호출)

## Follow-ups / Risks
- Task 4.6에서 TopMostManager에 WindowFocusService 통합 예정 (HandOffToLightroomAsync 메서드)
- Lightroom Classic 창 제목이 버전이나 열린 파일에 따라 달라질 수 있음 → 배열로 여러 후보 지원
- Windows 보안 정책에 따라 SetForegroundWindow가 실패할 수 있음 → 재시도 로직으로 대응

## Notes
- WindowInteropHelper를 사용하여 WPF Window의 Win32 핸들 획득
- Lightroom이 실행 중이지 않거나 최소화된 경우에도 FindWindow로 찾을 수 있음
- 향후 필요 시 `ShowWindow(hWnd, SW_RESTORE)`를 추가하여 최소화된 창 복원 가능
