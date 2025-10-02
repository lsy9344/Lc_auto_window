---
agent: Agent_CoreServices
task_ref: Task 1.13 - Win32 Interop 기본 구조 (SetForegroundWindow 등)
status: Completed
ad_hoc_delegation: false
compatibility_issues: false
important_findings: false
---

# Task Log: Win32 Interop 기본 구조 (SetForegroundWindow 등)

## Summary
Win32 P/Invoke 래퍼 클래스를 신규로 정의하여 Lightroom 자동화 시 필요한 창 포커스 제어 API를 노출했다. SetForegroundWindow, ShowWindow, FindWindow, GetForegroundWindow와 ShowWindow 관련 상수를 정리하고 한국어 XML 주석으로 동작과 인수를 설명했다.

## Details

### 1. Interop/Win32.cs 생성 및 네임스페이스 구성
- `Lc_auto.Interop` 네임스페이스 아래 `public static class Win32`를 정의하여 모든 P/Invoke 선언을 중앙화.
- `System`과 `System.Runtime.InteropServices` using 구문을 추가해 `IntPtr`, `DllImport`, `MarshalAs`를 참조.
- 클래스 요약 주석에 "창 포커스 제어용 Windows API P/Invoke" 목적을 명시.

### 2. 창 포커스 제어 API 선언
- `SetForegroundWindow(IntPtr hWnd)`를 Bool 반환형으로 선언하고 `[return: MarshalAs(UnmanagedType.Bool)]` 속성으로 실제 Win32 반환값을 .NET bool로 매핑.
- `ShowWindow(IntPtr hWnd, int nCmdShow)` 선언과 함께 창 표시 상태 제어를 담당하는 매개변수 설명을 XML 주석으로 작성.
- `FindWindow(string? lpClassName, string? lpWindowName)`는 `CharSet.Unicode` 옵션을 사용해 한글 창 제목도 정상 처리하도록 설정.
- `GetForegroundWindow()` 선언을 추가해 현재 포어그라운드 창 핸들을 조회 가능하도록 구성.

### 3. ShowWindow 명령 상수 정의
- `SW_HIDE`, `SW_SHOWNORMAL`, `SW_SHOWMINIMIZED`, `SW_SHOWMAXIMIZED`, `SW_RESTORE` 상수를 `public const int`로 노출.
- 각 상수마다 동작을 설명하는 한국어 XML 주석을 작성해 추후 호출부에서 의미를 즉시 파악 가능하도록 함.

## Validation
- 선언 시그니처 검토를 통해 P/Invoke 선언이 요구 사항과 일치함을 확인했고, `dotnet build` 실행을 시도했으나 로컬 환경에 .NET SDK가 없어(`dotnet: command not found`) 실제 빌드 검증은 수행하지 못함.
- 추가 테스트는 필요하지 않음 (순수 선언 파일).

## Follow-up
- Lightroom 창 클래스명/제목 확정 후 `FindWindow` 호출부에서 사용할 값은 ConfigService/AutomationService 연계 시점에 결정 예정.
