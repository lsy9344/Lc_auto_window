# 테스트 전략 및 방법론

## 1) 개요

본 문서는 Lc_auto 앱의 테스트 전략, 구체적인 테스트 시나리오, 환경 구성, 자동화 방법을 정의합니다.

---

## 2) 테스트 레벨

### 2.1 단위 테스트 (Unit Tests)
- **범위**: 비즈니스 로직, 설정 검증, 스케줄러 계산
- **도구**: xUnit + Moq
- **목표 커버리지**: 70% 이상
- **제외 항목**: WPF UI 바인딩, Win32 Interop (통합 테스트에서 커버)

### 2.2 통합 테스트 (Integration Tests)
- **범위**: Services 간 상호작용, Config 로드/핫리로드, 로깅
- **도구**: xUnit + TestHost
- **실행 환경**: CI/CD (GitHub Actions)

### 2.3 UI 자동화 테스트 (E2E Tests)
- **범위**: 버튼 클릭 → 자동화 실행 → 결과 검증
- **도구**: FlaUI (테스트 앱 자체를 FlaUI로 조작)
- **실행 환경**: 로컬 테스트 워크스테이션 (Lightroom Classic 설치 필수)

### 2.4 수동 테스트 (Manual Tests)
- **범위**: 실제 Lightroom Classic 워크플로우, 사용자 경험
- **빈도**: 주요 릴리스 전
- **체크리스트**: 수용 기준 (AC-01~AC-07) 기반

---

## 3) 테스트 환경

### 3.1 로컬 개발 환경
- **OS**: Windows 10 Pro (x64)
- **Lightroom Classic**: 최신 버전 + 이전 2개 버전
- **.NET SDK**: 8.0
- **IDE**: Visual Studio 2022 또는 VS Code + C# Extension

### 3.2 CI/CD 환경 (GitHub Actions)
- **OS**: `windows-latest`
- **실행**: 단위 테스트 + 통합 테스트
- **제외**: UI 자동화 테스트 (Lightroom 라이선스 문제)

### 3.3 테스트 워크스테이션
- **용도**: UI 자동화 테스트, 수동 테스트
- **스펙**: Windows 10/11, Lightroom Classic 설치, 테스트용 사진 라이브러리
- **접근**: 로컬 머신 또는 테스트 전용 VM

---

## 4) 테스트 데이터

### 4.1 설정 파일 (`config/config.json`)
테스트용 설정 파일 3종 준비:

**config.test.valid.json** (정상 케이스)
```json
{
  "media": {
    "videoPath": "C:\\TestMedia\\sample.mp4",
    "playerPath": ""
  },
  "automation": {
    "p1": { "featureId": "Test.Feature1", "description": "Test automation 1" },
    "p2": { "featureId": "Test.Feature2", "description": "Test automation 2" },
    "timeoutSec": 30
  },
  "paths": {
    "targetFolder": "C:\\TestOutput"
  },
  "alerts": [
    { "time": "10:00", "message": "Test alert", "repeat": "daily" }
  ],
  "ui": { "alwaysOnTop": true, "theme": "auto" }
}
```

**config.test.invalid.json** (JSON 파싱 에러)
```json
{
  "media": {
    "videoPath": "C:\\TestMedia\\sample.mp4",
    "playerPath": ""
  },
  "automation": {
    "p1": { "featureId": "Test.Feature1" // 쉼표 누락
    "timeoutSec": 30
  }
}
```

**config.test.missing.json** (필수 필드 누락)
```json
{
  "media": {
    "videoPath": ""
  },
  "automation": {},
  "paths": {},
  "alerts": [],
  "ui": { "alwaysOnTop": true }
}
```

### 4.2 미디어 파일
- **sample.mp4**: 5초 길이 테스트 동영상 (720p)
- **invalid.mp4**: 손상된 파일 (에러 시나리오 테스트)
- 경로: `TestAssets/media/`

### 4.3 Lightroom Classic 테스트 카탈로그
- **위치**: `C:\TestLightroom\TestCatalog.lrcat`
- **내용**: 10장의 샘플 사진, 테스트용 프리셋
- **백업**: 테스트 전 원본 카탈로그 복사 (각 테스트 독립성 보장)

---

## 5) 단위 테스트 시나리오

### 5.1 ConfigService
| 테스트 케이스 | 입력 | 예상 결과 |
|-------------|-----|----------|
| `LoadConfig_ValidFile_Success` | `config.test.valid.json` | `AppConfig` 객체 반환, 모든 필드 채워짐 |
| `LoadConfig_InvalidJson_ThrowsException` | `config.test.invalid.json` | `JsonException` 발생 |
| `LoadConfig_MissingFields_ReturnsPartialConfig` | `config.test.missing.json` | `AppConfig` 객체 반환, 누락 필드는 null/default |
| `ValidateConfig_MissingRequired_ReturnsFalse` | 빈 `AppConfig` | `IsValid = false`, `ValidationErrors` 포함 |
| `HotReload_FileChanged_RaisesEvent` | 파일 수정 감지 | `ConfigChanged` 이벤트 발생 |

### 5.2 AlertsScheduler
| 테스트 케이스 | 입력 | 예상 결과 |
|-------------|-----|----------|
| `CalculateNextTrigger_DailyAlert_CorrectTime` | `time: "10:00", repeat: "daily"`, 현재 시각 09:00 | 다음 트리거: 오늘 10:00 |
| `CalculateNextTrigger_DailyAlert_NextDay` | `time: "10:00", repeat: "daily"`, 현재 시각 11:00 | 다음 트리거: 내일 10:00 |
| `CalculateNextTrigger_OnceAlert_OnlyOnce` | `time: "10:00", repeat: "once"` | 최초 1회만 트리거, 이후 무시 |
| `ParseTime_InvalidFormat_ThrowsException` | `time: "25:00"` | `FormatException` 발생 |

### 5.3 FeatureSelectorRegistry
| 테스트 케이스 | 입력 | 예상 결과 |
|-------------|-----|----------|
| `Resolve_ValidFeatureId_ReturnsScript` | `"Lightroom.StartPhotoSession"` | `FeatureScript` 반환 |
| `Resolve_InvalidFeatureId_ThrowsException` | `"Invalid.FeatureId"` | `KeyNotFoundException` 발생 |
| `Resolve_LocalizedTitle_MatchesKorean` | Lightroom 한국어 버전 | 한글 컨트롤명 매칭 성공 |

---

## 6) 통합 테스트 시나리오

### 6.1 Config 로드 → Service 초기화
| 단계 | 동작 | 검증 |
|-----|-----|-----|
| 1 | `config.test.valid.json` 로드 | `ConfigService.Current` 객체 검증 |
| 2 | `AlertsScheduler.Apply()` 호출 | 알림 스케줄 설정됨 |
| 3 | `MediaService` 초기화 | `videoPath` 검증 완료 |

### 6.2 Hot Reload
| 단계 | 동작 | 검증 |
|-----|-----|-----|
| 1 | 앱 실행, 초기 config 로드 | 초기 설정 적용 |
| 2 | `config.json` 수정 (alerts 추가) | `FileSystemWatcher` 트리거 |
| 3 | 설정 재로드 | `ConfigChanged` 이벤트 발생, 새 설정 적용 |
| 4 | 잘못된 JSON으로 수정 | 에러 토스트 표시, 이전 설정 유지 |

### 6.3 로깅
| 단계 | 동작 | 검증 |
|-----|-----|-----|
| 1 | 앱 시작 | `log/Lc_auto-{date}.log` 생성 |
| 2 | 자동화 실행 (성공) | `INFO` 로그 기록 |
| 3 | 자동화 실행 (실패) | `ERROR` 로그 + 스택 트레이스 기록 |
| 4 | 로그 파일 50MB 초과 | 롤링 파일 생성 (`.001`, `.002`) |

---

## 7) UI 자동화 테스트 시나리오 (FlaUI)

### 7.1 앱 시작 및 TopMost
```csharp
[Fact]
public void AppStart_TopMost_IsTrue()
{
    using var app = Application.Launch("Lc_auto.exe");
    var mainWindow = app.GetMainWindow();

    Assert.True(mainWindow.Properties.TopMost);
}
```

### 7.2 버튼 a: 촬영 시작 자동화
```csharp
[Fact]
public void ButtonA_ShowsInputForm_AndRunsAutomation()
{
    using var app = Application.Launch("Lc_auto.exe");
    var mainWindow = app.GetMainWindow();

    // 버튼 a 클릭
    var buttonA = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ButtonA"));
    buttonA.Click();

    // 입력 폼 표시 확인
    var inputDialog = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("InputDialog"));
    Assert.NotNull(inputDialog);

    // 입력값 채우기
    var nameField = inputDialog.FindFirstDescendant(cf => cf.ByAutomationId("NameField"));
    var phoneField = inputDialog.FindFirstDescendant(cf => cf.ByAutomationId("PhoneField"));
    nameField.AsTextBox().Text = "홍길동";
    phoneField.AsTextBox().Text = "1234";

    // 확인 버튼 클릭
    var confirmButton = inputDialog.FindFirstDescendant(cf => cf.ByAutomationId("ConfirmButton"));
    confirmButton.Click();

    // 자동화 실행 대기 (모의 환경이므로 성공 다이얼로그 또는 에러 다이얼로그 확인)
    Wait.UntilResponsive(mainWindow, TimeSpan.FromSeconds(5));

    // 결과 검증 (실제 Lightroom 없으면 에러 다이얼로그)
    var errorDialog = mainWindow.FindFirstDescendant(cf => cf.ByName("오류"));
    Assert.NotNull(errorDialog); // Lightroom 미실행 시 에러 예상
}
```

### 7.3 버튼 c: 동영상 재생
```csharp
[Fact]
public void ButtonC_PlaysVideo_Success()
{
    using var app = Application.Launch("Lc_auto.exe");
    var mainWindow = app.GetMainWindow();

    var buttonC = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ButtonC"));
    buttonC.Click();

    // 동영상 플레이어 프로세스 시작 확인
    System.Threading.Thread.Sleep(2000);
    var videoProcess = System.Diagnostics.Process.GetProcessesByName("vlc"); // 예: VLC
    Assert.NotEmpty(videoProcess);
}
```

### 7.4 버튼 d: 폴더 열기
```csharp
[Fact]
public void ButtonD_OpensFolder_Success()
{
    using var app = Application.Launch("Lc_auto.exe");
    var mainWindow = app.GetMainWindow();

    var buttonD = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ButtonD"));
    buttonD.Click();

    // 탐색기 프로세스 확인
    System.Threading.Thread.Sleep(1000);
    var explorerWindows = new UIA3Automation().GetDesktop().FindAllDescendants(cf => cf.ByClassName("CabinetWClass"));
    Assert.NotEmpty(explorerWindows);
}
```

---

## 8) 수동 테스트 체크리스트

릴리스 전 수행할 전체 시나리오 검증:

### 8.1 AC-01: TopMost 동작
- [ ] 앱 시작 시 즉시 TopMost 상태 확인
- [ ] 버튼 a 클릭 → 자동화 실행 중 Lightroom 최상위 확인
- [ ] 자동화 완료 후 500ms 이내 앱 TopMost 복귀 확인
- [ ] 버튼 b도 동일하게 테스트

### 8.2 AC-02: 입력 폼 및 자동화
- [ ] 버튼 a 클릭 → 입력 폼 표시 확인
- [ ] "홍길동" + "1234" 입력 후 "촬영 시작" 클릭
- [ ] Lightroom에서 "홍길동1234" 붙여넣기 확인
- [ ] 자동화 완료 후 입력값 폐기 확인 (다음 클릭 시 빈 폼)
- [ ] 버튼 b도 동일하게 테스트

### 8.3 AC-03: 동영상 재생
- [ ] 버튼 c 클릭 → 동영상 재생 확인
- [ ] 재생 중 버튼 c 다시 클릭 → 중복 실행 차단 메시지 확인
- [ ] 잘못된 `playerPath` 설정 후 에러 메시지 확인

### 8.4 AC-04: 자동화 에러 처리
- [ ] Lightroom 미실행 상태에서 버튼 a 클릭 → 에러 다이얼로그 확인
- [ ] 잘못된 `featureId` 설정 후 자동화 실행 → 타임아웃 에러 확인
- [ ] 자동화 중 Lightroom 최소화 → 즉시 중단 및 에러 다이얼로그 확인

### 8.5 AC-05: 폴더 열기
- [ ] 버튼 d 클릭 → 탐색기에서 폴더 열림 확인
- [ ] 잘못된 경로 설정 후 에러 다이얼로그 확인

### 8.6 AC-06: 알림
- [ ] `alerts` 설정에 현재 시각 +1분 알림 추가
- [ ] 1분 후 모달 팝업 확인
- [ ] OK 클릭 → 모달 닫힘 확인
- [ ] 중복 알림 테스트: 첫 모달 표시 중 두 번째 알림 발생 → 메시지 업데이트 확인

### 8.7 AC-07: 관리자 권한
- [ ] 일반 사용자 권한으로 앱 실행 확인
- [ ] UIA 권한 필요 시 UAC 프롬프트 확인

### 8.8 Hot Reload
- [ ] 앱 실행 중 `config.json` 수정 → 토스트 알림 확인
- [ ] 알림 시간 변경 → 즉시 재스케줄 확인
- [ ] 잘못된 JSON 저장 → 에러 토스트 확인, 이전 설정 유지 확인

---

## 9) 성능 테스트

### 9.1 유휴 상태 모니터링
- **도구**: Task Manager 또는 Performance Monitor
- **측정 항목**: CPU 사용률, 메모리 사용량 (RSS)
- **기준**: CPU < 3%, RSS < 120 MB
- **방법**:
  1. 앱 시작 후 5분간 대기
  2. Task Manager에서 평균 CPU 및 메모리 확인
  3. 기준 초과 시 타이머/이벤트 핸들러 최적화

### 9.2 자동화 성능
- **측정 항목**: 자동화 완료 시간 (`elapsedMs`)
- **기준**: 평균 < 10초, 최대 60초 (재시도 포함)
- **방법**:
  1. 동일한 자동화를 10회 반복 실행
  2. 평균 및 최대 시간 로그에서 추출
  3. 비정상적인 지연 시 FlaUI 선택자 최적화

---

## 10) 회귀 테스트 전략

### 10.1 자동화 실행
- **빈도**: 매 PR, 매 릴리스 전
- **범위**: 단위 테스트 + 통합 테스트 (CI/CD)
- **실패 처리**: PR 블록, 실패 원인 로그 확인

### 10.2 UI 테스트 (로컬)
- **빈도**: 주 1회, 릴리스 전
- **범위**: FlaUI E2E 테스트 (테스트 워크스테이션)
- **실패 처리**: 로그 수집, Lightroom 버전 확인, 선택자 업데이트

### 10.3 수동 테스트
- **빈도**: 릴리스 직전
- **범위**: 전체 AC 체크리스트
- **결과 기록**: 테스트 결과를 GitHub Issue 또는 릴리스 노트에 첨부

---

## 11) 테스트 자동화 스크립트

### 11.1 Smoke Test (PowerShell)
```powershell
# smoke-test.ps1
Write-Host "Running Lc_auto smoke test..."

# 1. 앱 실행
Start-Process -FilePath ".\Lc_auto.exe" -PassThru | Out-Null
Start-Sleep -Seconds 3

# 2. 프로세스 확인
$process = Get-Process -Name "Lc_auto" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "✓ App started successfully"
} else {
    Write-Host "✗ App failed to start"
    exit 1
}

# 3. 로그 파일 생성 확인
$logFile = Get-ChildItem -Path ".\log\" -Filter "Lc_auto-*.log" | Select-Object -First 1
if ($logFile) {
    Write-Host "✓ Log file created: $($logFile.Name)"
} else {
    Write-Host "✗ Log file not found"
    exit 1
}

# 4. 종료
Stop-Process -Name "Lc_auto" -Force
Write-Host "Smoke test passed!"
```

### 11.2 CI/CD Workflow (GitHub Actions)
```yaml
# .github/workflows/test.yml
name: Test

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Run unit tests
        run: dotnet test --no-build --verbosity normal --filter "Category=Unit"

      - name: Run integration tests
        run: dotnet test --no-build --verbosity normal --filter "Category=Integration"

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: test-results
          path: TestResults/
```

---

## 12) 테스트 문서화

### 12.1 테스트 결과 기록
- **위치**: `TestResults/` 폴더 (Git 제외)
- **형식**: JUnit XML (xUnit 출력)
- **보관**: CI/CD 아티팩트로 업로드

### 12.2 버그 리포트 템플릿
```markdown
## 버그 설명
[간략한 설명]

## 재현 단계
1. [단계 1]
2. [단계 2]
3. [단계 3]

## 예상 동작
[정상 동작]

## 실제 동작
[비정상 동작]

## 환경
- OS: Windows 10/11
- Lightroom Classic 버전: [버전]
- Lc_auto 버전: [버전]
- 로그 파일: [첨부]

## 스크린샷
[이미지 첨부]
```

---

## 13) 참고 자료

- **FlaUI Documentation**: https://github.com/FlaUI/FlaUI
- **xUnit Documentation**: https://xunit.net/
- **Moq Documentation**: https://github.com/moq/moq4
- **GitHub Actions**: https://docs.github.com/en/actions