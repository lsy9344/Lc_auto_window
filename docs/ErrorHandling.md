# 에러 처리 정책 및 가이드

## 1) 개요

본 문서는 Lc_auto 앱의 에러 처리 정책을 정의합니다. 실제 에러 메시지는 `resources/errors.json`에서 단일 소스로 관리되며, 코드와 문서 모두 이 파일을 참조합니다.

---

## 2) 에러 처리 원칙

### 2.1 기본 원칙
- **사용자 친화적 메시지**: 기술 용어를 최소화하고 해결 방법을 포함
- **상세 로그**: 모든 에러는 로그에 타임스탬프, 스택 트레이스, 컨텍스트 포함
- **복구 가능성 우선**: 가능한 경우 자동 재시도 또는 대체 동작 제공

### 2.2 UI 표시 기준 (Dialog vs Toast)

**다이얼로그 (Dialog)**: 사용자의 명시적 응답이나 의사결정이 필요한 경우
- 작업 차단 필요 (예: 필수 조건 미충족, 치명적 오류)
- 사용자 입력 필요 (예: 입력값 검증 실패, 권한 요청)
- 예시: Lightroom 미실행, 입력값 오류, 파일/폴더 없음

**토스트 (Toast)**: 정보성 알림으로 작업 흐름을 방해하지 않는 경우
- 복구 가능하거나 영향이 제한적 (예: 설정 로드 실패하지만 이전 설정 유지)
- 백그라운드 작업 결과 (예: 설정 핫리로드 성공, TopMost 복원 실패)
- 예시: 설정 업데이트 성공, 설정 파싱 실패 (이전 설정 유지)

**로그 전용 (None)**: UI 표시 없이 로그에만 기록
- 내부 동작 추적 (예: 재시도 시도, 사용자 취소)
- 디버깅 정보
- 예시: Lightroom 포커스 재시도, 사용자 폼 취소

---

## 3) 에러 메시지 카탈로그

**모든 에러 메시지는 `resources/errors.json`에서 관리됩니다.**

이 파일은 다음 정보를 포함합니다:
- `type`: UI 표시 방식 (`dialog`, `toast`, `none`)
- `title`: 다이얼로그 제목 (type=dialog일 때)
- `message`: 사용자 메시지 (플레이스홀더 `{varName}` 지원)
- `logLevel`: Serilog 로그 레벨 (`INFO`, `WARN`, `ERROR`)
- `logMessage`: 로그 메시지 템플릿

**스키마 정의**: `resources/errors.schema.json` (JSON Schema 표준)

### 3.1 에러 카테고리

에러는 다음 카테고리로 분류됩니다:

| 카테고리 | JSON 키 | 설명 |
|---------|---------|------|
| 설정 파일 | `config` | config.json 로드/검증 관련 |
| 자동화 | `automation` | FlaUI 자동화 및 입력값 검증 |
| 동영상 재생 | `media` | 동영상 파일/플레이어 관련 |
| 폴더 열기 | `folder` | 폴더 경로 및 탐색기 실행 |
| TopMost 관리 | `topmost` | 창 z-order 및 포커스 제어 |
| 알림 스케줄러 | `alerts` | 알림 시간 파싱 및 스케줄링 |
| 권한 | `permissions` | UAC 승격 요청 및 거부 |
| 시스템 | `system` | 크래시 복구 등 |

### 3.2 주요 에러 시나리오 및 동작

| 카테고리 | 에러 ID | UI 타입 | 동작 |
|---------|---------|---------|------|
| config | `file_not_found` | dialog | 제한 모드로 시작, 버튼 비활성화 |
| config | `parse_failed` | toast | 이전 설정 유지 |
| config | `required_fields_missing` | toast | 해당 기능 비활성화, 계속 실행 |
| automation | `lightroom_not_running` | dialog | 자동화 중단 |
| automation | `ui_element_not_found` | dialog | 30초 타임아웃 후 재시도 (총 3회) |
| automation | `timeout_exceeded` | dialog | 자동화 중단 |
| automation | `name_empty` | dialog | 폼 유지, 자동화 실행 안 함 |
| automation | `phone_invalid` | dialog | 폼 유지, 자동화 실행 안 함 |
| automation | `user_cancelled` | none | 자동화 취소 (로그만) |
| media | `player_not_found` | dialog | 재생 중단 (OS 기본 플레이어로 fallback 안 함) |
| media | `already_playing` | dialog | 새 재생 차단 |
| folder | `not_found` | dialog | 열기 중단 |
| topmost | `restore_failed` | toast | 1초 간격 3회 재시도 후 토스트 |
| topmost | `lightroom_focus_failed` | none | 재시도 (로그만) |
| permissions | `uac_required` | dialog | UAC 프롬프트 표시 |
| permissions | `uac_denied` | dialog | 자동화 기능 비활성화 |
| system | `crash_recovery` | toast | 크래시 플래그 기록 |

**전체 메시지 및 템플릿**: `resources/errors.json` 참조

---

## 4) 구현 가이드

### 4.1 에러 메시지 로드
```csharp
// ErrorService.cs 예시
public class ErrorService
{
    private readonly Dictionary<string, ErrorDefinition> _errors;

    public ErrorService()
    {
        var json = File.ReadAllText("resources/errors.json");
        var catalog = JsonSerializer.Deserialize<ErrorCatalog>(json);
        _errors = catalog.Errors;
    }

    public void ShowError(string category, string errorId, Dictionary<string, string> placeholders = null)
    {
        var error = _errors[$"{category}.{errorId}"];
        var message = ReplacePlaceholders(error.Message, placeholders);

        switch (error.Type)
        {
            case "dialog":
                ShowDialog(error.Title, message);
                break;
            case "toast":
                ShowToast(message);
                break;
        }

        Log(error.LogLevel, ReplacePlaceholders(error.LogMessage, placeholders));
    }
}
```

### 4.2 메시지 형식

**다이얼로그**: `resources/errors.json`의 `title` + `message` 사용
**토스트**: 3초 표시, 우측 하단, ModernWpf Notification 스타일
**로그**: Serilog 템플릿 (`{Timestamp} [{Level}] {Message}`)

---

## 5) 테스트 시나리오

각 에러 시나리오는 다음 방법으로 테스트:

1. **설정 파일 에러**: `config.json` 삭제/손상/잘못된 값 입력 → `resources/errors.json`의 메시지 표시 확인
2. **자동화 에러**: Lightroom 미실행 상태에서 자동화 실행, 잘못된 `featureId` 사용
3. **입력값 검증**: 빈 성함, 3자리/5자리 숫자, 문자 포함 입력
4. **동영상 에러**: 잘못된 경로 지정, 존재하지 않는 플레이어 지정
5. **폴더 에러**: 존재하지 않는 경로 지정
6. **권한 에러**: 일반 사용자 권한으로 실행 후 UIA 접근 시도

---

## 6) 사용자 지원 정보

에러 다이얼로그 하단에 포함할 지원 정보:

```
문제가 지속되면 다음을 확인해 주세요:
1. 로그 파일: log/Lc_auto-{date}.log
2. 설정 파일: config/config.json
3. 에러 메시지: resources/errors.json
4. GitHub Issues: https://github.com/your-repo/issues
```

---

## 7) 문서 동기화 정책

**중요**: 에러 메시지를 변경할 때는 다음 절차를 따릅니다:

1. `resources/errors.json` 수정 (단일 소스)
2. `resources/errors.schema.json` 검증 실행
3. 코드는 `errors.json`을 직접 참조 (하드코딩 금지)
4. 이 문서(`ErrorHandling.md`)는 정책과 카테고리만 기술, 개별 메시지는 `errors.json` 참조로 유지

이 방식으로 코드-문서 동기화 부담을 제거합니다.