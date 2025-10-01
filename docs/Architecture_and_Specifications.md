# 아키텍처 및 기술 명세

## 1) Scope and Objectives

- **Purpose**: `PRD.md`에 명시된 요구사항을 구현하기 위한 아키텍처, 기술 선택, 제약 조건, 측정 가능한 동작을 정의합니다.
- **Primary Goals**: 4가지 주요 기능(동영상 재생, 2개의 Lightroom Classic 자동화, 폴더 열기)과 예약된 알림 팝업을 제공하는 Always-on-top 유틸리티.
- **Target OS**: Windows 10/11, x64 only.
- **Out of Scope (P1/Next)**: 기본 롤링 정책을 넘어서는 로깅 강화, 트레이 아이콘, 한국어 외 다국어 지원.

---

## 2) System Architecture

- **Pattern**: MVVM + Service Layer + Config-driven behavior.
- **Framework**: .NET 8, C# WPF.
- **레이어링**: UI(View, XAML) ↔ ViewModel ↔ Service(도메인/인프라) ↔ 외부(프로세스, OS, Lightroom)

### 2.1 모듈 구조

```
Lc_auto
  ├─ App.xaml / MainWindow.xaml (WPF View)
  ├─ ViewModels
  │   ├─ MainViewModel
  │   └─ AlertsViewModel
  ├─ Services
  │   ├─ TopMostManager
  │   ├─ WindowFocusService
  │   ├─ Automation
  │   │   ├─ AutomationService
  │   │   └─ FeatureSelectorRegistry
  │   ├─ MediaService
  │   ├─ FolderService
  │   ├─ AlertsScheduler
  │   ├─ ConfigService
  │   └─ LoggingService
  ├─ Interop
  │   └─ Win32 (SetForegroundWindow 등)
  ├─ Models
  │   ├─ AppConfig, MediaConfig, AutomationConfig, AlertConfig, PathsConfig, UiConfig
  │   └─ AutomationResult, ErrorInfo
  ├─ UI
  │   ├─ Components (Dialogs, Toasts)
  │   └─ Themes (ModernWpf)
  └─ Bootstrap
      └─ CompositionRoot (DI, Serilog init, Config bootstrap)
```

### 2.2 모듈 설명

- **MainViewModel**: 버튼 a/b/c/d 핸들링, 현재 상태 표시, 진행 중 액션 단일 인스턴스 보장.
- **AlertsViewModel**: 현재 표시 중인 알림 모달 관리 및 콘텐츠 갱신.
- **TopMostManager**: 앱 TopMost 제어, Lightroom 포커스 전후 핸드오프, 실패 재시도.
- **WindowFocusService**: Lightroom 창 찾기/포어그라운드 전환, 앱 포커스 복구.
- **AutomationService**: FlaUI 기반 자동화 실행. 재시도(30s × 최대 2회), UI 조건 기반 완료 판정, 중단/오류 처리.
- **FeatureSelectorRegistry**: `featureId` → UIA 선택자/행동 스크립트 매핑.
- **MediaService**: 동영상 재생. `playerPath` 유효성 검증, OS 기본 플레이어 fallback 금지, 단일 실행 보장.
- **FolderService**: 탐색기로 폴더 열기. 미존재 시 경고.
- **AlertsScheduler**: HH:mm 스케줄 타이머, 중복 발생 시 기존 모달 메시지 갱신, 절전 복귀 처리.
- **ConfigService**: `config/config.json` 로드/검증/핫리로드. 오류 시 제한 모드 유지 및 토스트 알림.
- **LoggingService**: Serilog 설정 및 사용 헬퍼.
- **Interop.Win32**: `SetForegroundWindow`, `ShowWindow` 등 포커스/포그라운드 제어.
- **CompositionRoot**: DI 등록, Serilog 초기화, Config 초기 로드, ModernWpf 테마 구성.

### 2.3 모듈 상호작용 흐름

- **자동화 실행 (FR-02/FR-03)**:
  1. `MainViewModel`은 `ConfigService`에서 현재 자동화 설정을 읽고 사용자 입력값을 검증한 후 `TopMostManager`에 작업 시작을 요청합니다.
  2. `TopMostManager`는 `WindowFocusService`를 호출해 Lightroom 창 포그라운드 전환 토큰을 획득하고 앱의 `TopMost` 속성을 일시적으로 해제합니다.
  3. 포그라운드 전환이 완료되면 `AutomationService.RunAsync(featureId, inputValues, ct)`를 실행하고, 이때 `FeatureSelectorRegistry`에서 단계별 UIA 선택자를 로드합니다.
  4. `AutomationService`는 실행 상태와 오류를 `LoggingService`에 기록하며, 실패 시 재시도 정책에 따라 동일 호출을 반복합니다.
  5. 자동화가 완료되면 `TopMostManager`가 앱 포커스를 복원하고 `WindowFocusService`를 통해 메인 창 포커스를 재설정합니다.
  6. 결과(`AutomationResult`)는 `MainViewModel`로 전달되어 사용자에게 성공/실패 상태를 표시합니다.

- **동영상 재생 (FR-04)**:
  1. `MainViewModel`이 사용자 요청을 수신하면 `ConfigService`에서 미디어 관련 경로를 확인합니다.
  2. `MediaService`는 경로 유효성을 검증하고 외부 플레이어 프로세스를 시작하며, 실행 핸들을 내부 상태로 보관합니다.
  3. 프로세스 시작에 실패할 경우 `LoggingService`를 통해 오류를 기록하고 `MainViewModel`에 에러 상태를 반환합니다.

- **폴더 열기 (FR-05)**:
  1. `MainViewModel`이 `FolderService.OpenAsync(paths.targetFolder, ct)`를 호출합니다.
  2. `FolderService`는 경로 존재 여부를 확인하고 `explorer.exe`를 실행합니다.
  3. 미존재 경로일 경우 예외 대신 결과 플래그로 실패를 반환하고, `MainViewModel`이 사용자 경고 다이얼로그를 표시합니다.

- **예약 알림 (FR-06)**:
  1. `ConfigService` 변경 이벤트가 발생하면 `AlertsScheduler.Apply(alerts)`로 최신 스케줄이 전달됩니다.
  2. 내부 타이머가 트리거되면 `AlertsScheduler`는 `AlertsViewModel`에 현재 메시지를 푸시하고, 중복 알림 시 콘텐츠만 갱신합니다.
  3. 알림 종료 시 `AlertsViewModel`은 상태를 초기화하고 필요한 경우 `LoggingService`에 통계를 기록합니다.

- **설정 핫리로드 및 로깅**:
  - `ConfigService`는 파일 변경을 감지하면 새 설정을 유효성 검사하고, 성공 시 `ConfigChanged` 이벤트로 모든 의존 모듈(예: `AlertsScheduler`, `AutomationService`)에 브로드캐스트합니다.
  - 각 서비스는 필요 시 변경된 섹션만 반영하고, 실패 시 `LoggingService`에 경고를 남긴 뒤 기존 값을 유지합니다.

### 2.4 Concurrency

- UI 작업은 Dispatcher 스레드에서, 오래 실행되는 작업은 백그라운드 스레드에서 처리.
- 자동화 및 알림 타이머는 CancellationToken을 지원하는 async/await 사용.
- Busy polling을 피하고 이벤트/타이머 트리거 및 FlaUI 타임아웃에 의존.

---

## 3) Technology Stack and Libraries

- **WPF**: .NET 8
- **UI Theme**: ModernWpf (Auto theme)
- **Automation**: FlaUI (UIA3)
- **JSON**: System.Text.Json
- **Logging**: Serilog + Serilog.Sinks.File
- **Process/Window Interop**: P/Invoke (`SetForegroundWindow` 등)

---

## 4) Detailed Functional Specifications

### FR-01 Always On Top
- **Behavior**: 앱이 TopMost 상태를 자동으로 관리하며, 수동 토글은 제공하지 않음.
- **Initialization**: 시작 시 메인 윈도우 `TopMost=true`.
- **During Automations (FR-02/03)**:
  - 자동화 시작 전, Lightroom Classic을 포그라운드로 전환.
  - 자동화 완료 후 500ms 내에 앱을 다시 포그라운드로 전환하고 `TopMost=true`로 복원.
- **Failure Handling**: TopMost 복원 실패 시 1초 간격으로 최대 3회 재시도 후 에러 로그 기록.

### FR-02/FR-03 Automations (FlaUI)
- **User Workflow**: 전체 사용자 워크플로우는 **`PRD.md` §1.2** 참조.
- **Inputs**:
  - `config.automation.p1.featureId` / `config.automation.p2.featureId`
  - 사용자 입력 폼: "예약자 성함", "휴대폰 뒤4자리"
- **Input Validation & Format**: 입력값 검증 규칙 및 조합 포맷은 **`PRD.md` §FR-02** 참조.
- **Execution**:
  1. 사용자에게 입력 폼 표시.
  2. 폼 제출 시 입력값 검증.
  3. Lightroom Classic 창을 포그라운드로 전환.
  4. FlaUI (UIA3)를 통해 UI 요소를 찾고, 클릭/텍스트 입력 수행.
  5. 자동화 시퀀스의 지정된 단계에서 조합된 입력값 붙여넣기.
  6. 시도당 타임아웃: 30초.
  7. 재시도 정책: 최초 시도 1회 + 최대 2회 재시도 (총 3회, 최대 약 90초).
  8. 완료 판정: 예상되는 UI 상태/조건(예: 다이얼로그 닫힘)에 따라 완료 판정.
- **Input Lifecycle**: UI 폼으로 수집된 값은 자동화 실행 중에 한 번만 사용되며, 실행 직후 폐기됨.
- **Minimize/Exit Handling**: 자동화 중 Lightroom이 최소화되거나 종료되면 즉시 자동화를 실패 처리하고 앱 포커스 복원.
- **Errors**: UI 요소 탐색 실패 또는 상호작용 실패 시, 재시도 정책을 따른 후 에러 다이얼로그 표시 및 로그 기록.

### FR-04 Video Playback
- **Inputs**: `media.videoPath`, optional `media.playerPath`.
- **Player Resolution**:
  - `playerPath`가 비어있지 않지만 실행 불가능한 경우: 에러 다이얼로그 표시 (OS 기본 플레이어로 fallback 안 함).
  - `playerPath`가 비어있는 경우: OS 기본 플레이어로 실행.
- **Single-Instance Policy**: 한 번에 하나의 동영상만 재생 가능. 이미 재생 중일 경우 경고 다이얼로그 표시.

### FR-05 Open Folder
- **Input**: `paths.targetFolder`.
- **Behavior**: `explorer.exe`를 통해 해당 경로에서 Windows 탐색기 실행.
- **Nonexistent Path**: 에러 다이얼로그 표시 (폴더를 생성하지 않음).

### FR-06 Scheduled Alerts
- **Config**: `alerts[]` (`time`: HH:mm, `message`, `repeat`: daily|once).
- **Scheduling**: 로컬 시간 기준 타이머, 트리거 허용 오차 ±1초.
- **Duplicate Rule**: 알림 다이얼로그가 이미 표시된 상태에서 다른 알림이 트리거될 경우, 기존 다이얼로그의 메시지를 최신 내용으로 업데이트.
- **Sleep/Wake**: 절전 모드에서 복귀 시, 놓친 알림은 건너뛰고 다음 스케줄부터 재개.

---

## 5) API/Interface Sketches

```text
// Services
interface IAutomationService {
  Task<AutomationResult> RunAsync(string featureId, IReadOnlyDictionary<string, string> inputValues, CancellationToken ct);
}

interface IWindowFocusService {
  void CaptureAppWindow();
  Task<bool> TryActivateLightroomAsync(CancellationToken ct);
  void RestoreAppWindow();
}

interface ITopMostManager {
  void SetAppTopMost(bool isTopMost);
  Task HandOffToLightroomAsync(Func<CancellationToken, Task> automation, CancellationToken ct);
}

interface IMediaService {
  bool IsPlaying { get; }
  Task PlayAsync(string mediaPath, string? playerPath, CancellationToken ct);
  void Stop();
}

interface IFolderService {
  Task<bool> OpenAsync(string folderPath, CancellationToken ct);
  bool Validate(string folderPath);
}

interface IAlertsScheduler {
  void Apply(IEnumerable<AlertConfig> alerts);
  void Start();
  void Stop();
}

interface IConfigService {
  AppConfig Current { get; }
  event EventHandler<AppConfig> ConfigChanged;
}
```

---

## 6) Configuration Management

- **Location**: 앱 실행 파일(.exe)과 동일한 디렉토리의 `config/config.json`.
- **Hot Reload**: `FileSystemWatcher`를 통해 활성화. 변경 감지 시 유효성 검사 후 적용. 유효하지 않으면 이전 설정 유지 및 토스트 알림.
- **Validation**: 시작 및 리로드 시 `config/config.schema.json`에 대해 유효성 검사.
- **Schema**: 전체 스키마 및 검증 규칙은 **`config/config.schema.json`** 참조.

---

## 7) Non-Functional Requirements

- **Performance**: 유휴 상태에서 CPU < 3%, RSS < 120MB.
- **Reliability**: 자동화 실패는 명확히 사용자에게 알리고 로그에 원인 저장. 재시도 정책 준수.
- **Security**: 기본적으로 관리자 권한 없이 실행. 특정 자동화에 권한 상승이 필요할 경우 UAC를 통해 요청.
- **Privacy**: 개인 식별 정보(PII)를 처리하지 않음. 로그 및 설정 파일에 민감한 데이터를 저장하지 않음.

---

## 8) Logging and Diagnostics

- **Library**: Serilog 및 Serilog.Sinks.File 사용.
- **Directory**: 실행 파일 옆 `log/` 디렉토리.
- **Policy**: 파일당 50MB, 7일 보관 롤링 정책.
- **Levels**: 정상 작동은 INFO, 복구 가능한 문제는 WARN, 실패는 ERROR.

---

## 9) Packaging and Deployment

- **Publish**: Single-file, self-contained, `win-x64`.
- **Code Signing**: 서명되지 않은 상태로 배포.
- **Distribution**: .exe, `config/` 폴더, `log/` 폴더를 포함한 Zip 파일.

---

## 10) Open Issues and Risks

| Risk | Impact | Mitigation | Trigger/Contingency | Owner |
|------|--------|------------|---------------------|-------|
| Foreground Control Limits | Windows 포그라운드 정책 거부로 자동화가 시작되지 않음 | `WindowFocusService`에 3단계 재시도와 사용자 승인 팝업 추가, 포커스 실패 시 TopMost 재설정 지연 | `TryActivateLightroomAsync`가 연속 실패하면 사용자가 직접 Lightroom을 활성화할 수 있도록 인스트럭션 표시 | Runtime Owner |
| External Player Behavior | 외부 플레이어가 헬퍼 프로세스를 생성하거나 종료 이벤트를 보내지 않아 앱 상태가 꼬일 수 있음 | `MediaService`에서 프로세스 트리 모니터링과 timeout(5분) 강제 종료, 플레이어별 호환성 목록 유지 | 타임아웃 발생 시 강제 종료 후 사용자에게 재생 실패 안내, 로그에 플레이어 경로 기록 | Media Owner |
| Config Hot Reload Failures | 잘못된 설정으로 전체 서비스가 중단될 위험 | 적용 전 스키마 검증 + 실패 시 이전 설정 롤백, 경고 토스트와 로그 남김 | 동일 오류가 2회 연속 발생하면 핫리로드 자동 비활성화 및 수동 재설정 안내 | Config Owner |
