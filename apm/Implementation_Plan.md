# Lc_auto – Implementation Plan

**Memory Strategy:** dynamic-md
**Last Modification:** Initial plan creation by Setup Agent
**Project Overview:** Windows WPF (.NET 8) launcher application for Lightroom Classic automation workflows. Provides four main button actions (video playback, two Lightroom automations, folder opening) with always-on-top window management, scheduled alert popups, and configuration-driven behavior. Target platform: Windows 10/11 x64, Korean UI.

## Phase 1: Project Foundation & Core Infrastructure

### Task 1.1 – .NET WPF 프로젝트 초기화 및 솔루션 구조 생성 │ Agent_Infrastructure
- **Objective:** .NET 8 WPF 프로젝트와 솔루션 파일을 생성하고 MVVM 아키텍처를 위한 기본 폴더 구조를 설정하여 후속 개발 작업의 기반을 마련한다.
- **Output:** Lc_auto.csproj 및 Lc_auto.sln 파일, Services/, ViewModels/, Models/, UI/, Interop/, Bootstrap/ 폴더 구조, .NET 8 Windows 타겟 설정이 완료된 프로젝트.
- **Guidance:** C# 표준 네이밍 컨벤션 사용, .csproj에서 TargetFramework=net8.0-windows 및 OutputType=WinExe 필수 확인, 폴더 구조는 MVVM + Service Layer 패턴에 따라 구성.

1. **프로젝트 생성:** `dotnet new wpf -n Lc_auto -o .` 명령으로 현재 디렉토리에 WPF 프로젝트 생성.
2. **솔루션 구성:** `dotnet new sln -n Lc_auto` 및 `dotnet sln add Lc_auto.csproj`로 솔루션 파일 생성 및 프로젝트 추가.
3. **폴더 생성:** Services/, ViewModels/, Models/, UI/, Interop/, Bootstrap/ 폴더 생성하여 레이어 분리 준비.
4. **프로젝트 설정 확인:** .csproj 파일에서 TargetFramework=net8.0-windows, OutputType=WinExe 설정 확인.

### Task 1.2 – NuGet 패키지 설치 및 종속성 구성 │ Agent_Infrastructure
- **Objective:** UI 테마, 로깅, JSON 직렬화를 위한 필수 NuGet 패키지를 설치하고 종속성 복원을 완료한다.
- **Output:** ModernWpf (v0.9.6), Serilog (v3.1.1), Serilog.Sinks.File (v5.0.0) 패키지 설치, 복원 완료된 NuGet 종속성.
- **Guidance:** Depends on: Task 1.1 Output by Agent_Infrastructure. System.Text.Json은 .NET 8 기본 포함이므로 별도 설치 불필요, dotnet restore로 복원 성공 확인.

- ModernWpf (v0.9.6), Serilog (v3.1.1), Serilog.Sinks.File (v5.0.0) 설치 (`dotnet add package <PackageName> --version <Version>`).
- System.Text.Json은 .NET 8에 기본 포함되므로 추가 설치 불필요.
- `dotnet restore` 명령으로 모든 종속성 복원 확인 및 오류 없음 검증.

### Task 1.3 – Git 저장소 초기화 및 .gitignore 설정 │ Agent_Infrastructure
- **Objective:** Git 버전 관리를 초기화하고 .NET 프로젝트에 적합한 .gitignore 파일을 생성하여 불필요한 파일 추적을 방지한다.
- **Output:** 초기화된 Git 저장소, .NET 표준 .gitignore 파일 (bin/, obj/, .vs/, *.user 등 포함), "Initial project setup" 커밋.
- **Guidance:** .gitignore에 bin/, obj/, .vs/, *.user, *.suo, log/, apm/Memory/ 포함, 한국어 주석으로 섹션 설명 추가.

1. **Git 초기화:** `git init` 명령으로 현재 디렉토리를 Git 저장소로 초기화.
2. **.gitignore 생성:** .NET 표준 템플릿 기반 .gitignore 파일 생성 (bin/, obj/, .vs/, *.user, *.suo, log/, apm/Memory/ 포함).
3. **초기 커밋:** `git add .` 및 `git commit -m "Initial project setup"` 실행하여 기본 프로젝트 구조 커밋.

### Task 1.4 – MainWindow XAML 기본 레이아웃 및 ModernWpf 테마 적용 │ Agent_UI_Foundation
- **Objective:** MainWindow의 기본 XAML 레이아웃을 구성하고 ModernWpf 테마를 적용하여 현대적인 UI 스타일을 설정한다.
- **Output:** ModernWpf 테마가 적용된 MainWindow.xaml 및 App.xaml, Window 속성 (Topmost=True, Title="Lc_auto", Width=400, Height=300) 설정 완료.
- **Guidance:** Depends on: Task 1.2 Output by Agent_Infrastructure. ModernWpf namespace는 `xmlns:ui="http://schemas.modernwpf.com/2019"`, 빌드 및 실행으로 테마 적용 검증 필수.

1. **App.xaml 테마 추가:** Application.Resources에 `<ui:ThemeResources />` 및 `<ui:XamlControlsResources />` 추가.
2. **MainWindow 레이아웃:** MainWindow.xaml에 Grid 레이아웃 추가, Margin 및 Padding 설정 (예: Padding="20").
3. **Window 속성 설정:** Title="Lc_auto", Topmost="True", Width="400", Height="300", ResizeMode="CanMinimize" 설정.
4. **빌드 및 실행 확인:** `dotnet build` 및 `dotnet run` 명령으로 ModernWpf 테마 적용 확인, 빌드 오류 없음 검증.

### Task 1.5 – 4개 버튼 UI 구현 (a/b/c/d 한국어 라벨) │ Agent_UI_Foundation
- **Objective:** PRD에 정의된 4개 기능 버튼을 한국어 라벨로 구현하여 사용자 인터페이스의 핵심 요소를 완성한다.
- **Output:** MainWindow.xaml에 정의된 4개 Button 컨트롤 ("촬영 시작", "내보내기", "배경지 설치 동영상 보기", "사진 저장 폴더 열기"), AutomationId 설정 완료.
- **Guidance:** Depends on: Task 1.4 Output. StackPanel 또는 Grid 사용, 버튼 간격 및 크기 통일, AutomationId는 FlaUI 테스트를 위한 필수 속성.

- StackPanel (Orientation="Vertical") 또는 Grid (RowDefinitions) 내에 4개 Button 정의.
- Button Content: "촬영 시작", "내보내기", "배경지 설치 동영상 보기", "사진 저장 폴더 열기" (PRD §1.2 참조).
- AutomationProperties.AutomationId 설정: ButtonA, ButtonB, ButtonC, ButtonD (UI 자동화 테스트용).
- 각 버튼 Margin, Padding, Height 통일하여 일관된 UI 제공.

### Task 1.6 – Dialog 컴포넌트 구현 │ Agent_UI_Foundation
- **Objective:** 재사용 가능한 Dialog 서비스를 구현하여 에러, 경고, 정보 메시지를 사용자에게 표시하는 표준 방법을 제공한다.
- **Output:** UI/Components/DialogService.cs 클래스, ShowDialog(title, message) 메서드, ModernWpf ContentDialog 또는 MessageBox 래퍼.
- **Guidance:** ModernWpf ContentDialog 또는 표준 MessageBox 사용, 한국어 주석으로 메서드 설명, MainViewModel에 주석 처리된 사용 예제 추가.

1. **DialogService 클래스:** UI/Components/DialogService.cs 생성, ShowDialog(string title, string message) 정적 메서드 정의.
2. **Dialog 구현:** ModernWpf ContentDialog 또는 System.Windows.MessageBox.Show() 사용하여 모달 다이얼로그 표시.
3. **사용 예제:** MainViewModel.cs에 주석 처리된 DialogService.ShowDialog() 호출 예제 추가.

### Task 1.7 – Toast 컴포넌트 구현 │ Agent_UI_Foundation
- **Objective:** 비차단 토스트 알림 컴포넌트를 구현하여 설정 로드 성공/실패 등 정보성 메시지를 표시한다.
- **Output:** UI/Components/ToastService.cs 클래스, Show(message, duration) 메서드, 우측 하단 배치 Toast UserControl/Popup.
- **Guidance:** 우측 하단 배치, 3초 기본 표시 시간, FadeIn/Out 애니메이션 구현, 한국어 주석 추가.

1. **ToastService 클래스:** UI/Components/ToastService.cs 생성, Show(string message, int durationMs = 3000) 정적 메서드 정의.
2. **Toast UI:** UserControl 또는 Popup 정의, 우측 하단 배치 (HorizontalAlignment="Right", VerticalAlignment="Bottom").
3. **애니메이션:** DoubleAnimation을 사용한 FadeIn (Opacity 0→1) 및 FadeOut (Opacity 1→0) 애니메이션 추가.
4. **사용 예제:** MainViewModel.cs에 주석 처리된 ToastService.Show() 호출 예제 추가.

### Task 1.8 – MainViewModel 기본 구조 및 명령 바인딩 │ Agent_UI_Foundation
- **Objective:** MVVM 패턴의 MainViewModel을 구현하고 4개 버튼에 대한 ICommand 바인딩을 설정하여 UI-ViewModel 연결을 완성한다.
- **Output:** ViewModels/MainViewModel.cs 클래스, INotifyPropertyChanged 구현, ButtonACommand ~ ButtonDCommand ICommand 속성, MainWindow DataContext 바인딩.
- **Guidance:** Depends on: Task 1.5 Output. RelayCommand 헬퍼 또는 CommunityToolkit.Mvvm 사용, 각 Command는 현재 로그 출력만 수행, 한국어 주석 추가.

1. **MainViewModel 클래스:** ViewModels/MainViewModel.cs 생성, INotifyPropertyChanged 구현 (PropertyChanged 이벤트 포함).
2. **RelayCommand 헬퍼:** RelayCommand 클래스 생성 또는 CommunityToolkit.Mvvm.Input.RelayCommand 사용.
3. **Command 정의:** ButtonACommand, ButtonBCommand, ButtonCCommand, ButtonDCommand ICommand 속성 정의, 각각 LogInfo("버튼 X 클릭됨") 호출.
4. **DataContext 설정:** MainWindow.xaml.cs 생성자에서 `DataContext = new MainViewModel()` 설정, 또는 XAML에서 바인딩.

### Task 1.9 – Config 모델 클래스 정의 (AppConfig, MediaConfig 등) │ Agent_CoreServices
- **Objective:** config.json 구조를 반영하는 C# 모델 클래스들을 정의하여 타입 안전한 설정 관리를 가능하게 한다.
- **Output:** Models/ 폴더 내 AppConfig.cs, MediaConfig.cs, AutomationConfig.cs, PathsConfig.cs, AlertConfig.cs, UiConfig.cs 클래스 파일.
- **Guidance:** PRD §4 config 스키마에 따라 각 속성 정의, System.Text.Json.Serialization.JsonPropertyName 특성 사용, nullable 속성은 ? 타입 지정.

- Models/AppConfig.cs, MediaConfig.cs, AutomationConfig.cs, PathsConfig.cs, AlertConfig.cs, UiConfig.cs 클래스 생성.
- 각 클래스 속성은 PRD §4의 config.json 스키마에 맞게 정의 (예: MediaConfig.VideoPath, MediaConfig.PlayerPath).
- System.Text.Json.Serialization.JsonPropertyName 특성 사용 (예: `[JsonPropertyName("videoPath")]`).
- nullable 속성은 string? 또는 해당 타입? 사용, 한국어 주석으로 각 속성 설명 추가.

### Task 1.10 – ConfigService 구현 (JSON 로드 및 기본 검증) │ Agent_CoreServices
- **Objective:** config.json 파일을 로드하고 필수 필드를 검증하는 ConfigService를 구현하여 앱 전역 설정 관리를 제공한다.
- **Output:** Services/IConfigService.cs 인터페이스, Services/ConfigService.cs 구현 클래스, LoadConfig() 및 ValidateConfig() 메서드, ConfigChanged 이벤트.
- **Guidance:** Depends on: Task 1.9 Output. config/config.json 파일 경로 하드코딩, JsonSerializer.Deserialize<AppConfig>() 사용, Hot Reload는 Phase 2 이후 구현.

1. **IConfigService 인터페이스:** Services/IConfigService.cs 정의, AppConfig Current { get; } 속성, event EventHandler<AppConfig> ConfigChanged.
2. **ConfigService 클래스:** Services/ConfigService.cs 생성, IConfigService 구현, 생성자에서 LoadConfig() 호출.
3. **LoadConfig() 메서드:** config/config.json 파일 읽기, JsonSerializer.Deserialize<AppConfig>(json, options) 수행, 예외 처리 포함.
4. **ValidateConfig() 메서드:** videoPath, targetFolder, automation.start_photo, automation.export_files 필드 null/empty 체크, 실패 시 로그 기록.
5. **ConfigChanged 이벤트:** EventHandler<AppConfig> 타입 정의, Hot Reload 구현은 Phase 2 이후로 연기.

### Task 1.11 – LoggingService 구현 (Serilog 설정) │ Agent_CoreServices
- **Objective:** Serilog를 사용한 로깅 서비스를 구현하여 앱 전역 로깅 인프라를 제공하고 롤링 파일 정책을 설정한다.
- **Output:** Services/LoggingService.cs 클래스, Serilog Log.Logger 초기화, LogInfo(), LogWarn(), LogError() 정적 메서드.
- **Guidance:** Depends on: Task 1.2 Output by Agent_Infrastructure. log/ 폴더는 자동 생성, 롤링 정책: 파일당 50MB, 7일 보관, 한국어 주석 추가.

1. **LoggingService 클래스:** Services/LoggingService.cs 생성, 정적 생성자에서 Serilog 초기화.
2. **Serilog 설정:** `Log.Logger = new LoggerConfiguration().WriteTo.File("log/Lc_auto-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, fileSizeLimitBytes: 50_000_000).MinimumLevel.Information().CreateLogger()`.
3. **로그 레벨:** MinimumLevel.Information() 설정, INFO/WARN/ERROR 레벨 사용.
4. **정적 메서드:** LogInfo(string message), LogWarn(string message, Exception? ex = null), LogError(string message, Exception ex) 메서드 제공.

### Task 1.12 – TopMostManager 서비스 구현 │ Agent_CoreServices
- **Objective:** 앱 창의 TopMost 상태를 관리하는 서비스를 구현하여 항상 최상위 표시 기능을 제공한다.
- **Output:** Services/ITopMostManager.cs 인터페이스, Services/TopMostManager.cs 구현 클래스, SetAppTopMost(bool) 메서드, 재시도 로직 기본 구조.
- **Guidance:** Window 참조는 생성자 주입, Phase 4에서 HandOffToLightroomAsync() 추가 예정, 한국어 주석 추가.

1. **ITopMostManager 인터페이스:** Services/ITopMostManager.cs 정의, SetAppTopMost(bool isTopMost) 메서드.
2. **TopMostManager 클래스:** Services/TopMostManager.cs 생성, Window 참조를 생성자로 주입받아 저장.
3. **SetAppTopMost() 메서드:** Window.Topmost 속성 설정, true/false 전환 지원.
4. **재시도 로직 기본 구조:** 실패 시 1초 간격 최대 3회 재시도 로직 주석으로 명시 (현재는 기본 구조만, Phase 4에서 HandOffToLightroomAsync() 추가 시 완성).

### Task 1.13 – Win32 Interop 기본 구조 (SetForegroundWindow 등) │ Agent_CoreServices
- **Objective:** Windows API 호출을 위한 P/Invoke 선언을 정의하여 창 포커스 제어 기능의 기반을 마련한다.
- **Output:** Interop/Win32.cs 클래스, SetForegroundWindow, ShowWindow, FindWindow, GetForegroundWindow 함수 DllImport 정의.
- **Guidance:** user32.dll 함수 사용, IntPtr 및 HWND 타입 처리, 한국어 주석으로 각 함수 용도 설명.

- Interop/Win32.cs 정적 클래스 생성.
- DllImport("user32.dll") 특성 사용하여 SetForegroundWindow(IntPtr hWnd), ShowWindow(IntPtr hWnd, int nCmdShow), FindWindow(string lpClassName, string lpWindowName), GetForegroundWindow() 함수 선언.
- 각 함수에 한국어 주석으로 용도 설명 추가 (예: "창을 포어그라운드로 전환").

### Task 1.14 – Dependency Injection 설정 (CompositionRoot) │ Agent_CoreServices
- **Objective:** DI 컨테이너를 설정하고 서비스들을 등록하여 MVVM 및 Service Layer의 의존성 주입을 활성화한다.
- **Output:** Bootstrap/CompositionRoot.cs 클래스, ConfigureServices() 메서드, App.xaml.cs OnStartup() 초기화, ServiceProvider를 통한 서비스 해결.
- **Guidance:** Depends on: Task 1.10, 1.11, 1.12 Output. Microsoft.Extensions.DependencyInjection (.NET 8 기본 포함) 사용, AddSingleton<I, T>() 패턴.

1. **ServiceCollection 사용:** Microsoft.Extensions.DependencyInjection.ServiceCollection 사용 (별도 패키지 불필요, .NET 8 기본 포함).
2. **CompositionRoot 클래스:** Bootstrap/CompositionRoot.cs 생성, static IServiceProvider ConfigureServices() 메서드 정의.
3. **서비스 등록:** AddSingleton<IConfigService, ConfigService>(), AddSingleton<ITopMostManager, TopMostManager>(), AddSingleton<ILoggingService, LoggingService>() 등 등록.
4. **App.xaml.cs 초기화:** OnStartup() 오버라이드, CompositionRoot.ConfigureServices() 호출 후 ServiceProvider 저장, MainWindow 생성 시 ServiceProvider 전달.

### Task 1.15 – config.json 템플릿 파일 생성 │ Agent_Infrastructure
- **Objective:** 앱 설정을 위한 config.json 템플릿 파일을 생성하여 사용자가 설정을 편집할 수 있는 기본 구조를 제공한다.
- **Output:** config/ 폴더, config/config.json 파일 (PRD §4 예제 기반, media, automation, paths, alerts, ui 섹션 포함).
- **Guidance:** PRD §4의 JSON 예제 사용, 경로는 플레이스홀더 값 (예: "C:\\Media\\video.mp4"), 한국어 주석은 JSON 지원 안 함.

- config/ 폴더 생성 (`mkdir config`).
- PRD §4의 예제 config.json 생성: media (videoPath, playerPath), automation (start_photo, export_files, timeoutSec), paths (targetFolder), alerts (time, message, repeat), ui (alwaysOnTop, theme) 섹션 포함.
- 경로는 플레이스홀더 값 사용 (예: "C:\\Media\\background_installation.mp4").

### Task 1.16 – config.schema.json 생성 │ Agent_Infrastructure
- **Objective:** config.json의 JSON Schema를 정의하여 설정 파일 검증 및 IDE 자동 완성을 지원한다.
- **Output:** config/config.schema.json 파일 (JSON Schema Draft-07 기준, media, automation, paths, alerts, ui 섹션 스키마 정의).
- **Guidance:** JSON Schema Draft-07 표준 사용, required 필드 지정 (videoPath, targetFolder 등), type 및 format 제약 추가.

- config/config.schema.json 생성.
- JSON Schema Draft-07 형식으로 media, automation, paths, alerts, ui 섹션 스키마 정의.
- required 필드 지정: videoPath, targetFolder, automation.start_photo, automation.export_files.
- 각 속성에 type (string, number, object, array) 및 description 추가.

## Phase 2: Media & Folder Features

### Task 2.1 – MediaService 구현 (동영상 재생, 단일 인스턴스 정책) │ Agent_FeatureServices
- **Objective:** 동영상 재생 기능을 구현하고 단일 인스턴스 정책을 적용하여 중복 재생을 방지한다.
- **Output:** Services/IMediaService.cs 및 MediaService.cs, PlayAsync() 메서드, IsPlaying 플래그, 프로세스 핸들 추적 기능.
- **Guidance:** Process.Start()로 외부 플레이어 실행, playerPath 유효성 검증, 프로세스 종료 시 IsPlaying = false 설정, 한국어 주석 추가.

1. **IMediaService 인터페이스:** Services/IMediaService.cs 정의, bool IsPlaying { get; } 속성, Task PlayAsync(string videoPath, string? playerPath, CancellationToken ct) 메서드.
2. **MediaService 클래스:** Services/MediaService.cs 생성, IMediaService 구현, private Process? _currentProcess 필드.
3. **PlayAsync() 메서드:** playerPath 유효성 검증 (File.Exists), Process.Start() 실행, 빈 playerPath는 OS 기본 플레이어 사용.
4. **단일 실행 정책:** IsPlaying 플래그 체크, 이미 재생 중이면 false 반환, 프로세스 Exited 이벤트에서 IsPlaying = false 설정.

### Task 2.2 – FolderService 구현 (폴더 열기, 경로 검증) │ Agent_FeatureServices
- **Objective:** Windows 탐색기로 지정된 폴더를 여는 기능을 구현하고 경로 검증을 수행한다.
- **Output:** Services/IFolderService.cs 및 FolderService.cs, OpenAsync() 메서드, Validate() 메서드.
- **Guidance:** explorer.exe 실행, Directory.Exists() 체크, 미존재 경로는 false 반환, 한국어 주석 추가.

1. **IFolderService 인터페이스:** Services/IFolderService.cs 정의, Task<bool> OpenAsync(string folderPath, CancellationToken ct) 메서드.
2. **FolderService 클래스:** Services/FolderService.cs 생성, IFolderService 구현.
3. **Validate() 메서드:** private bool Validate(string path) 메서드, Directory.Exists(path) 체크.
4. **OpenAsync() 메서드:** Validate() 호출, 성공 시 Process.Start("explorer.exe", folderPath) 실행, 실패 시 false 반환.

### Task 2.3 – MainViewModel에 버튼 c/d 명령 구현 │ Agent_UI_Features
- **Objective:** ButtonCCommand 및 ButtonDCommand를 MediaService 및 FolderService와 연결하여 동영상 재생 및 폴더 열기 기능을 완성한다.
- **Output:** MainViewModel의 ButtonCCommand 및 ButtonDCommand 구현, MediaService 및 FolderService 호출 로직.
- **Guidance:** Depends on: Task 2.1 Output by Agent_FeatureServices, Task 2.2 Output by Agent_FeatureServices. DI로 서비스 주입, async Command 사용, 에러 처리 포함.

- ButtonCCommand: MediaService.PlayAsync(config.Media.VideoPath, config.Media.PlayerPath, ct) 호출, 에러 시 LoggingService.LogError() 및 DialogService.ShowDialog().
- ButtonDCommand: FolderService.OpenAsync(config.Paths.TargetFolder, ct) 호출, false 반환 시 경고 로그 및 다이얼로그 표시.

## Phase 3: Alert Scheduling

### Task 3.1 – AlertConfig 및 AlertsScheduler 구현 │ Agent_FeatureServices
- **Objective:** 시간 기반 알림 스케줄러를 구현하여 HH:mm 형식의 시간에 알림을 트리거한다.
- **Output:** Models/AlertConfig.cs, Services/IAlertsScheduler.cs, Services/AlertsScheduler.cs, HH:mm 파싱 로직, System.Timers.Timer 기반 스케줄링.
- **Guidance:** System.Timers.Timer 사용, 로컬 시간 기준, ±1초 오차 허용, AlertTriggered 이벤트 정의, 한국어 주석 추가.

1. **AlertConfig 모델:** Models/AlertConfig.cs 정의, string Time { get; set; } (HH:mm 형식), string Message { get; set; }, string Repeat { get; set; } (daily/once) 속성.
2. **IAlertsScheduler 인터페이스:** Services/IAlertsScheduler.cs 정의, Apply(IEnumerable<AlertConfig> alerts), Start(), Stop() 메서드, event EventHandler<string> AlertTriggered.
3. **AlertsScheduler 클래스:** Services/AlertsScheduler.cs 생성, System.Timers.Timer 사용, 내부 List<Timer> 관리.
4. **HH:mm 파싱:** TimeSpan.ParseExact(alert.Time, "hh\\:mm", CultureInfo.InvariantCulture) 사용, 다음 트리거 시간 계산.
5. **이벤트 발생:** 트리거 시 AlertTriggered?.Invoke(this, alert.Message) 호출, repeat = daily면 다음날 재스케줄, once면 타이머 제거.

### Task 3.2 – AlertsViewModel 및 모달 UI 구현 │ Agent_UI_Features
- **Objective:** 알림 모달 UI를 구현하고 중복 알림 시 메시지 업데이트 로직을 적용한다.
- **Output:** ViewModels/AlertsViewModel.cs, UI/AlertDialog.xaml, CurrentMessage 속성, AlertsScheduler 이벤트 구독.
- **Guidance:** Depends on: Task 3.1 Output by Agent_FeatureServices. 단일 모달 인스턴스 유지, 중복 시 메시지만 업데이트, 한국어 주석 추가.

1. **AlertsViewModel 클래스:** ViewModels/AlertsViewModel.cs 생성, INotifyPropertyChanged 구현, string CurrentMessage 속성.
2. **AlertDialog.xaml:** UI/AlertDialog.xaml 모달 Window 생성, TextBlock (Message 바인딩), OK 버튼.
3. **중복 업데이트 로직:** AlertsScheduler.AlertTriggered 이벤트 구독, 기존 모달이 열려 있으면 CurrentMessage만 업데이트, 없으면 새 모달 표시.
4. **이벤트 구독:** App.xaml.cs 또는 MainViewModel에서 AlertsScheduler.AlertTriggered += AlertsViewModel.OnAlertTriggered 연결.

### Task 3.3 – ConfigService에서 alerts 로드 및 스케줄러 연결 │ Agent_FeatureServices
- **Objective:** ConfigService가 config.json의 alerts 섹션을 로드하고 AlertsScheduler에 적용한다.
- **Output:** ConfigService.LoadConfig() 내 alerts 배열 파싱 로직, AlertsScheduler.Apply() 호출.
- **Guidance:** Depends on: Task 3.1 Output. LoadConfig() 완료 후 AlertsScheduler.Apply(config.Alerts) 호출, 한국어 주석 추가.

- LoadConfig() 메서드에서 AppConfig.Alerts 속성 파싱 (List<AlertConfig>).
- ConfigService 생성자 또는 LoadConfig() 완료 후 AlertsScheduler.Apply(Current.Alerts) 호출, Start() 메서드로 타이머 시작.

## Phase 4: Lightroom Automation

### Task 4.1 – 입력 폼 Dialog 구현 (성함, 휴대폰 뒤4자리) │ Agent_UI_Features
- **Objective:** 사용자 입력을 받는 폼 다이얼로그를 구현하여 자동화 실행 전 필요한 데이터를 수집한다.
- **Output:** UI/InputFormDialog.xaml, ViewModels/InputFormViewModel.cs, Name 및 Phone TextBox, 확인/취소 버튼, DialogResult 반환.
- **Guidance:** PRD §1.2 참조, 한국어 라벨 ("예약자 성함", "휴대폰 뒤4자리"), DialogResult = true/false 반환, 한국어 주석 추가.

1. **InputFormDialog.xaml:** UI/InputFormDialog.xaml Window 생성, 두 개 TextBox (Name, Phone), 확인 및 취소 Button.
2. **InputFormViewModel:** ViewModels/InputFormViewModel.cs 생성, string Name { get; set; }, string Phone { get; set; } 속성.
3. **TextBox 배치:** StackPanel 또는 Grid 사용, Label "예약자 성함", "휴대폰 뒤4자리" 한국어 라벨.
4. **DialogResult 반환:** 확인 버튼 클릭 시 DialogResult = true, 취소 시 DialogResult = false, InputFormViewModel의 Name 및 Phone 값 반환.

### Task 4.2 – 입력값 검증 로직 구현 │ Agent_UI_Features
- **Objective:** 입력값을 검증하여 PRD에 정의된 규칙을 준수하는지 확인한다.
- **Output:** Services/ValidationService.cs, ValidateName() 및 ValidatePhone() 메서드, InputFormViewModel에 검증 로직 통합.
- **Guidance:** Depends on: Task 4.1 Output. PRD §FR-02 검증 규칙 참조 (성함: 비어있지 않음, 휴대폰: 정확히 4자리 숫자), 한국어 주석 추가.

1. **ValidationService 클래스:** Services/ValidationService.cs 생성, static bool ValidateName(string name), static bool ValidatePhone(string phone) 메서드.
2. **ValidateName():** !string.IsNullOrWhiteSpace(name) 체크, 비어 있지 않은 문자열 검증.
3. **ValidatePhone():** Regex.IsMatch(phone, @"^\d{4}$") 체크, 정확히 4자리 숫자 검증.
4. **InputFormViewModel 통합:** InputFormViewModel에서 확인 버튼 클릭 시 ValidationService.ValidateName() 및 ValidatePhone() 호출, 실패 시 DialogService.ShowDialog() 에러 표시.

### Task 4.3 – WindowFocusService 구현 (Lightroom 포커스 전환) │ Agent_CoreServices
- **Objective:** Lightroom Classic 창을 찾아 포어그라운드로 전환하는 서비스를 구현한다.
- **Output:** Services/IWindowFocusService.cs, Services/WindowFocusService.cs, TryActivateLightroomAsync() 및 RestoreAppWindow() 메서드.
- **Guidance:** Win32.FindWindow() 및 SetForegroundWindow() 사용, Lightroom 창 제목 또는 클래스명 검색, 한국어 주석 추가.

1. **IWindowFocusService 인터페이스:** Services/IWindowFocusService.cs 정의, Task<bool> TryActivateLightroomAsync(CancellationToken ct), void RestoreAppWindow() 메서드.
2. **WindowFocusService 클래스:** Services/WindowFocusService.cs 생성, private IntPtr _appWindowHandle 필드.
3. **TryActivateLightroomAsync():** Win32.FindWindow(null, "Lightroom Classic") 호출, 찾으면 Win32.SetForegroundWindow(hWnd), 실패 시 false 반환, 최대 3회 재시도.
4. **RestoreAppWindow():** Win32.SetForegroundWindow(_appWindowHandle) 호출하여 앱 창 포커스 복원.

### Task 4.4 – FeatureSelectorRegistry 기본 구조 │ Agent_FeatureServices
- **Objective:** featureId를 UIA 선택자 스크립트에 매핑하는 레지스트리 기본 구조를 생성한다.
- **Output:** Services/Automation/FeatureSelectorRegistry.cs, Dictionary<string, FeatureScript> 구조, Get(featureId) 메서드.
- **Guidance:** 현재는 빈 Dictionary, Phase 4 실행 중 사용자와 협업하여 선택자 추가, 한국어 주석으로 확장 가능성 명시.

- Services/Automation/FeatureSelectorRegistry.cs 정적 클래스 생성.
- private static readonly Dictionary<string, FeatureScript> _registry = new() 필드 정의 (현재 빈 Dictionary).
- public static FeatureScript? Get(string featureId) 메서드, _registry.TryGetValue(featureId, out var script) ? script : null 반환.
- 한국어 주석: "Lightroom Classic UI 요소 실행 중 사용자와 협업하여 추가 예정".

### Task 4.5 – AutomationService 구현 (FlaUI 기반) │ Agent_FeatureServices
- **Objective:** FlaUI를 사용하여 Lightroom Classic UI 자동화를 실행하는 서비스를 구현한다.
- **Output:** Services/Automation/IAutomationService.cs, Services/Automation/AutomationService.cs, RunAsync() 메서드, FlaUI UIA3 통합, 재시도 로직.
- **Guidance:** Depends on: Task 4.3 Output by Agent_CoreServices, Task 4.4 Output. FlaUI.UIA3 패키지 사용, 타임아웃 30초, 최대 2회 재시도, Ad-Hoc Delegation으로 사용자와 UI 요소 식별.

**Ad-Hoc Delegation – Lightroom UI 요소 식별 (참고: Agent-project-manager/prompts/ad-hoc/Research_Delegation_Guide.md)**

1. **IAutomationService 인터페이스:** Services/Automation/IAutomationService.cs 정의, Task<AutomationResult> RunAsync(string featureId, Dictionary<string, string> inputValues, CancellationToken ct) 메서드.
2. **AutomationService 클래스:** Services/Automation/AutomationService.cs 생성, FlaUI.UIA3.UIA3Automation _automation 필드.
3. **RunAsync() 메서드:** FeatureSelectorRegistry.Get(featureId)로 선택자 로드, FlaUI로 Lightroom 창 찾기 (Application.Attach), UI 요소 탐색 및 상호작용 실행.
4. **타임아웃 및 재시도:** 시도당 30초 타임아웃, 실패 시 최대 2회 재시도 (총 3회 시도), CancellationToken 지원.
5. **FeatureSelectorRegistry 사용:** Get(featureId)로 스크립트 로드, null이면 KeyNotFoundException 발생, 로그 기록.

### Task 4.6 – TopMostManager HandOffToLightroom 메서드 추가 │ Agent_CoreServices
- **Objective:** TopMostManager에 Lightroom으로 포커스를 넘기고 자동화 후 복원하는 메서드를 추가한다.
- **Output:** ITopMostManager.HandOffToLightroomAsync() 메서드, WindowFocusService 통합, 재시도 로직.
- **Guidance:** Depends on: Task 4.3 Output. WindowFocusService.TryActivateLightroomAsync() 호출, 자동화 실행 후 RestoreAppWindow(), 1초 간격 3회 재시도.

1. **ITopMostManager 확장:** HandOffToLightroomAsync(Func<Task> automationTask, CancellationToken ct) 메서드 추가.
2. **WindowFocusService 주입:** TopMostManager 생성자에 IWindowFocusService 주입, 필드 저장.
3. **포커스 전환:** WindowFocusService.TryActivateLightroomAsync(ct) 호출, 성공 시 automationTask() 실행.
4. **복원 및 재시도:** automationTask() 완료 후 WindowFocusService.RestoreAppWindow() 호출, SetAppTopMost(true) 설정, 실패 시 1초 간격 최대 3회 재시도.

### Task 4.7 – MainViewModel에 버튼 a/b 명령 구현 │ Agent_UI_Features
- **Objective:** ButtonACommand 및 ButtonBCommand를 구현하여 입력 폼 → 검증 → 자동화 전체 워크플로우를 완성한다.
- **Output:** MainViewModel의 ButtonACommand 및 ButtonBCommand 구현, InputFormDialog 표시, ValidationService 호출, AutomationService 실행.
- **Guidance:** Depends on: Task 4.1, 4.2, Task 4.5 Output by Agent_FeatureServices, Task 4.6 Output by Agent_CoreServices. 입력값 조합 `{성함}{뒤4자리}` 형식 (PRD §FR-02 참조).

- ButtonACommand: InputFormDialog 표시 → ValidateName() 및 ValidatePhone() 호출 → 성공 시 입력값 조합 (`name + phone`) → TopMostManager.HandOffToLightroomAsync(() => AutomationService.RunAsync("start_photo", inputValues, ct)) 호출.
- ButtonBCommand: InputFormDialog 표시 → 검증 → 입력값 조합 → TopMostManager.HandOffToLightroomAsync(() => AutomationService.RunAsync("export_files", inputValues, ct)) 호출.
- 입력값 조합 형식: `Dictionary<string, string> { ["customerInput"] = name + phone }` (공백/구분자 없음, PRD §1.2 참조).

