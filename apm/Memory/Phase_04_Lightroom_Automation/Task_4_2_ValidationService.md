---
agent: Agent_UI_Features
task_ref: Task 4.2 - 입력값 검증 로직 구현
status: Completed
ad_hoc_delegation: false
compatibility_issues: false
important_findings: false
---

# Task Log: 입력값 검증 로직 구현

## Summary
- ValidationService를 구현하여 PRD §FR-02/FR-03 검증 규칙을 준수하도록 했습니다.
- InputFormDialog의 OnConfirmClick 메서드에 검증 로직을 통합했습니다.
- 검증 실패 시 DialogService를 사용해 사용자에게 에러 메시지를 표시하고 해당 입력 필드로 포커스를 이동합니다.

## Implementation Notes
- **Services/ValidationService.cs**:
  - 정적 클래스로 구현하여 전역에서 사용 가능
  - `ValidateName(string? name)`: `!string.IsNullOrWhiteSpace(name)` 체크
    - 비어 있지 않은 문자열 검증 (한글, 영문, 공백 허용)
  - `ValidatePhone(string? phone)`: `Regex.IsMatch(phone, @"^\d{4}$")` 체크
    - 정확히 4자리 숫자 검증
  - .NET 7+ `GeneratedRegex` 특성 사용으로 성능 최적화

- **UI/InputFormDialog.xaml.cs 수정**:
  - `OnConfirmClick()` 메서드에 검증 로직 추가
  - 검증 실패 시 `DialogService.ShowDialog()`로 에러 다이얼로그 표시
  - 검증 실패한 입력 필드로 포커스 이동 (UX 개선)
  - 검증 성공 시에만 `DialogResult = true` 설정

## Validation Rules (PRD §FR-02)
1. **예약자 성함**:
   - 비어 있지 않은 문자열
   - 한글, 영문, 공백 허용
   - 에러 메시지: "예약자 성함을 입력해주세요."

2. **휴대폰 뒤4자리**:
   - 정확히 4자리 숫자 (0-9)
   - 정규식: `^\d{4}$`
   - 에러 메시지: "휴대폰 뒤4자리를 정확히 입력해주세요. (4자리 숫자)"

## Build / Verification
- Command: `dotnet build -p:EnableWindowsTargeting=true`
- Result: ✅ 성공 (경고 0개, 오류 0개)

## Output Files
- Services/ValidationService.cs (신규)
- UI/InputFormDialog.xaml.cs (수정)

## Follow-ups / Risks
- Task 4.7에서 MainViewModel의 ButtonACommand/ButtonBCommand 구현 시 검증된 입력값 사용 예정
- 입력값 조합 포맷: `{성함}{뒤4자리}` (공백/구분자 없음, PRD §1.2 참조)

## Notes
- DialogService를 사용하여 일관된 에러 표시 방식 유지
- 검증 실패 시 입력 필드로 포커스 이동하여 사용자 편의성 향상
- GeneratedRegex 특성으로 정규식 컴파일 최적화 (.NET 8 지원)
