---
agent_type: Implementation
agent_id: Agent_CoreServices_1
handover_number: 1
last_completed_task: Task 1.13 - Win32 Interop 기본 구조 (SetForegroundWindow 등)
---

# Implementation Agent Handover File - CoreServices

## Active Memory Context

**User Preferences:**
- 모든 클래스와 메서드에 한국어 XML 주석 작성 선호
- Single-step task execution 방식으로 모든 작업을 한 응답에 완료
- Dependency context가 있는 경우 통합하여 즉시 작업 진행
- 빌드 검증을 통한 컴파일 확인 필수

**Working Insights:**
- .NET 8 WPF 프로젝트 구조 (Lc_auto.csproj)
- System.Text.Json이 .NET 8 런타임에 포함되어 별도 패키지 불필요
- Serilog 3.1.1, Serilog.Sinks.File 5.0.0 패키지 이미 설치됨 (Task 1.2)
- ConfigService.ConfigChanged 이벤트 미사용 경고(CS0067)는 Phase 2에서 Hot Reload 구현 시 해결 예정으로 정상
- 모든 빌드 검증에서 0 errors, 2 warnings (ConfigService 관련 경고만 발생)

## Task Execution Context

**Working Environment:**
- **Models/ 디렉토리**: 6개 config 모델 클래스
  - AppConfig.cs (루트 컨테이너)
  - MediaConfig.cs (videoPath, playerPath)
  - AutomationConfig.cs (p1, p2, timeoutSec + nested AutomationItem)
  - PathsConfig.cs (targetFolder)
  - AlertConfig.cs (time, message, repeat)
  - UiConfig.cs (alwaysOnTop, theme)

- **Services/ 디렉토리**: 5개 서비스 클래스
  - IConfigService.cs + ConfigService.cs (config.json 로드 및 검증)
  - LoggingService.cs (static class, Serilog 기반)
  - ITopMostManager.cs + TopMostManager.cs (Window TopMost 관리)

- **Interop/ 디렉토리**: Win32 API P/Invoke 선언
  - Win32.cs (SetForegroundWindow, ShowWindow, FindWindow, GetForegroundWindow)

**Issues Identified:**
- ConfigService.ConfigChanged 이벤트는 현재 구현되지 않았으며 Phase 2에서 FileSystemWatcher 추가 시 구현 예정
- ConfigService는 현재 Console.WriteLine() 사용 중 (LoggingService로 교체는 future task)
- config.json 파일은 Task 1.15에서 생성 예정
- TopMostManager의 Lightroom 포커스 핸드오프 및 재시도 로직은 Phase 4에서 구현 예정 (TODO 주석으로 명시)

## Current Context

**Recent User Directives:**
- Task Assignment Prompts에서 명시된 dependency_context 통합을 즉시 수행
- Memory Log는 Dynamic-MD 형식으로 작성 (YAML frontmatter + Markdown)
- 모든 작업 완료 후 Memory Log 작성 필수

**Working State:**
- 현재 빌드 상태: 성공 (0 errors, 2 warnings)
- Phase 01 - Project Foundation & Core Infrastructure 진행 중
- Agent_CoreServices 담당 작업들 진행 완료 (Task 1.9 ~ 1.13)
- 다음 작업 대기 중

**Task Execution Insights:**
- Dependency context가 있는 경우, 이전 작업의 Memory Log와 파일 참조를 활용하여 통합 진행
- Task 1.10에서 Task 1.9의 모델 클래스를 dependency context로 사용
- Task 1.11에서 Task 1.2의 Serilog 패키지 설치를 dependency context로 사용
- 각 작업마다 한국어 XML 주석과 빌드 검증이 필수

## Working Notes

**Development Patterns:**
- **DI Pattern**: 생성자로 의존성 주입 (예: TopMostManager는 Window 참조를 생성자로 받음)
- **Service Pattern**: 인터페이스로 추상화 (IConfigService, ITopMostManager)
- **Static Service Pattern**: LoggingService는 static class로 구현 (앱 전역 사용)
- **Defensive Programming**: ArgumentNullException으로 null 검증
- **JSON Serialization**: System.Text.Json 사용, JsonPropertyName 속성으로 매핑
- **Logging**: Serilog 사용, 롤링 파일 정책 (일별, 7일 보관, 50MB per file)
- **P/Invoke**: Win32 API 호출을 위한 DllImport 속성 사용

**Environment Setup:**
- Namespace 규칙: `Lc_auto.Models`, `Lc_auto.Services`, `Lc_auto.Interop`
- Config 경로: `config/config.json` (실행 파일 기준 상대 경로)
- Log 경로: `log/Lc_auto-.log` (Serilog가 런타임에 자동 생성)
- Memory Log 경로: `apm/Memory/Phase_01_Project_Foundation_Core_Infrastructure/Task_X_Y_*.md`

**User Interaction:**
- Task Assignment Prompt의 모든 항목을 한 응답에 완료 (single-step)
- 빌드 검증 후 Memory Log 작성
- Memory Log 작성 후 사용자에게 완료 보고
- 간결한 완료 메시지 선호 (파일 경로, 주요 기능, 빌드 결과, Memory Log 경로)
