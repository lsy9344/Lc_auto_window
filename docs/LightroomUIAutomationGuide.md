# Lightroom Classic UI 자동화 스크립트 작성 가이드

## 개요
이 문서는 Lc_auto 애플리케이션에서 Lightroom Classic UI 자동화를 설정하는 방법을 설명합니다.

현재 `Services/Automation/FeatureSelectorRegistry.cs`에 등록된 스크립트는 **플레이스홀더(템플릿)**입니다. 실제 Lightroom Classic 환경에서 UI 요소를 식별하여 수정해야 합니다.

---

## 필수 도구

### FlaUI Inspect
- **다운로드**: https://github.com/FlaUI/FlaUI/releases
- **파일**: `FlaUIInspect.exe` (최신 릴리스에서 다운로드)
- **용도**: Windows UI Automation 요소 탐색 및 속성 확인

---

## 단계별 가이드

### Step 1: FlaUI Inspect 다운로드 및 실행

1. **FlaUI Inspect 다운로드**
   ```
   https://github.com/FlaUI/FlaUI/releases
   → Assets → FlaUIInspect.zip 다운로드
   → 압축 해제
   ```

2. **Lightroom Classic 실행**
   - Adobe Lightroom Classic을 실행합니다.
   - 자동화하려는 기능(예: 촬영 시작, 내보내기)이 포함된 화면으로 이동합니다.

3. **FlaUIInspect.exe 실행**
   - 관리자 권한으로 실행 권장
   - Lightroom Classic 창이 활성 상태여야 합니다.

---

### Step 2: UI 요소 식별

#### 2.1 요소 선택
- FlaUIInspect에서 **"Hover Mode"** 버튼 클릭
- Lightroom Classic 창의 원하는 UI 요소 위에 마우스를 올립니다.
- Ctrl 키를 누르면 해당 요소가 선택됩니다.

#### 2.2 속성 확인
FlaUIInspect 오른쪽 패널에서 다음 속성을 확인합니다:

| 속성 | 설명 | 예시 |
|------|------|------|
| **AutomationId** | 고유 식별자 (가장 안정적) | `"FileMenu"`, `"ExportButton"` |
| **Name** | 표시되는 텍스트 | `"File"`, `"Export"`, `"확인"` |
| **ClassName** | 윈도우 클래스 이름 | `"Button"`, `"MenuItem"` |
| **ControlType** | 컨트롤 유형 | `"Button"`, `"Edit"`, `"MenuItem"` |

**우선순위**:
1. **AutomationId** (가장 안정적 - 언어/버전 독립적)
2. **Name** (텍스트 기반 - 언어에 따라 변경될 수 있음)
3. **ClassName** (보조 식별자)

---

### Step 3: 스크립트 수정

#### 3.1 파일 열기
```
Services/Automation/FeatureSelectorRegistry.cs
```

#### 3.2 촬영 시작 스크립트 예제
```csharp
RegisterScript("Lightroom.StartPhotoSession", new FeatureScript
{
    FeatureId = "Lightroom.StartPhotoSession",
    Description = "촬영 시작 자동화",
    Steps =
    [
        // 예제: 파일 메뉴 클릭
        new AutomationStep
        {
            Description = "파일 메뉴 열기",
            Selector = new UiaSelector
            {
                Name = "File",  // FlaUI Inspect에서 확인한 Name 값
                ControlType = "MenuItem"
            },
            Action = "Click"
        },

        // 대기 (UI 로드 시간 확보)
        new AutomationStep
        {
            Description = "대화상자 로드 대기",
            Selector = new UiaSelector { },
            Action = "Wait",
            ActionData = "1000"  // 밀리초 (1초)
        },

        // 텍스트 입력
        new AutomationStep
        {
            Description = "고객명 입력",
            Selector = new UiaSelector
            {
                AutomationId = "txtCustomerName",  // FlaUI Inspect에서 확인한 ID
                ControlType = "Edit"
            },
            Action = "SetText",
            ActionData = "{customerInput}"  // "홍길동1234"로 치환됨
        },

        // 버튼 클릭
        new AutomationStep
        {
            Description = "확인 버튼 클릭",
            Selector = new UiaSelector
            {
                Name = "OK",  // 또는 "확인" (Lightroom 언어 설정에 따라)
                ControlType = "Button"
            },
            Action = "Click"
        }
    ]
});
```

---

## UiaSelector 사용 가이드

### Selector 조합 규칙
여러 속성을 조합하면 요소 식별이 더 정확해집니다:

```csharp
new UiaSelector
{
    AutomationId = "btnExport",  // 우선 순위 1
    Name = "Export",             // 우선 순위 2
    ControlType = "Button"       // 우선 순위 3
}
```

FlaUI는 **AND 조건**으로 모든 지정된 속성이 일치하는 요소를 찾습니다.

### 언어별 대응
Lightroom Classic의 언어 설정에 따라 Name이 달라질 수 있습니다:

```csharp
// 영어 버전
Name = "File"

// 한국어 버전
Name = "파일"
```

**권장**: AutomationId를 우선 사용하여 언어 독립성을 확보하세요.

---

## Action 유형

### 1. Click
UI 요소 클릭

```csharp
Action = "Click"
```

### 2. SetText
텍스트 입력 필드에 값 설정

```csharp
Action = "SetText",
ActionData = "{customerInput}"  // 플레이스홀더
```

**플레이스홀더 치환**:
- `{customerInput}` → 사용자가 입력한 `{성함}{뒤4자리}` (예: "홍길동1234")

### 3. Wait
지정된 시간(밀리초) 대기

```csharp
Action = "Wait",
ActionData = "1000"  // 1초
```

---

## 디버깅 팁

### 1. 로그 확인
실행 중 로그는 `log/Lc_auto-YYYYMMDD.log`에 기록됩니다:

```
[INF] FeatureSelectorRegistry 초기화 완료 (등록된 스크립트: 2개)
[WRN] 현재 스크립트는 플레이스홀더입니다. FlaUI Inspect로 실제 Lightroom UI 요소를 식별하여 수정하세요.
[INF] 자동화 시작: Lightroom.StartPhotoSession
[INF] 스텝 실행: 파일 메뉴 열기
[ERR] UI 요소를 찾을 수 없습니다: 파일 메뉴 열기
```

### 2. 자주 발생하는 오류

#### "UI 요소를 찾을 수 없습니다"
**원인**:
- AutomationId/Name이 잘못 지정됨
- 대화상자가 아직 로드되지 않음

**해결**:
1. FlaUI Inspect로 정확한 속성 재확인
2. Wait 스텝 추가 (대기 시간 증가)

#### "자동화 실패 (3회 시도)"
**원인**:
- Lightroom Classic이 실행 중이지 않음
- UI 요소 변경 (Lightroom 업데이트)

**해결**:
1. Lightroom Classic 실행 확인
2. 최신 Lightroom 버전에 맞게 스크립트 재작성

---

## 테스트 절차

### 1. 스크립트 수정 후 빌드
```bash
dotnet build -p:EnableWindowsTargeting=true
```

### 2. 애플리케이션 실행
```bash
dotnet run
```

### 3. 자동화 테스트
1. Lightroom Classic 실행
2. Lc_auto 앱에서 "촬영 시작" 버튼 클릭
3. 입력 폼에 정보 입력 (예: 성함 "홍길동", 휴대폰 "1234")
4. "촬영 시작" 버튼 클릭
5. 자동화 실행 관찰

### 4. 로그 확인
```
log/Lc_auto-YYYYMMDD.log
```

성공 시:
```
[INF] 자동화 시작: Lightroom.StartPhotoSession
[INF] 스텝 실행: 파일 메뉴 열기
[INF] 클릭 완료: 파일 메뉴 열기
[INF] 스텝 실행: 고객명 입력
[INF] 텍스트 입력 완료: 고객명 입력 = "홍길동1234"
[INF] 자동화 성공: Lightroom.StartPhotoSession (소요 시간: 3452ms)
```

---

## 고급 사용법

### 조건부 스텝
현재는 지원하지 않습니다. 모든 스텝은 순차적으로 실행됩니다.

### 동적 대기
특정 UI 요소가 나타날 때까지 대기하는 기능은 현재 미구현입니다. 고정 대기 시간(Wait)을 사용하세요.

### 스크립트 버전 관리
Lightroom Classic 버전별로 다른 스크립트가 필요한 경우:

```csharp
// 버전별 featureId 사용 (예: Lightroom 13.x)
RegisterScript("Lightroom.StartPhotoSession.v13", new FeatureScript { ... });
RegisterScript("Lightroom.StartPhotoSession.v14", new FeatureScript { ... });
```

---

## 추가 리소스

- **FlaUI GitHub**: https://github.com/FlaUI/FlaUI
- **FlaUI 문서**: https://github.com/FlaUI/FlaUI/wiki
- **Lc_auto 프로젝트 문서**: `docs/PRD.md`, `docs/Architecture.md`

---

## 지원

스크립트 작성 중 문제가 발생하면:
1. 로그 파일 확인 (`log/Lc_auto-*.log`)
2. FlaUI Inspect로 UI 요소 재확인
3. 대기 시간(Wait) 증가 시도
4. Lightroom Classic 버전 및 언어 설정 확인

**참고**: 현재 템플릿 스크립트는 실제 Lightroom Classic UI 구조를 반영하지 않습니다. 반드시 FlaUI Inspect를 사용하여 실제 값으로 수정해야 정상 작동합니다.
