---
agent: Agent_UI_Features
task_ref: Task 4.1 - 입력 폼 Dialog 구현
status: Completed
ad_hoc_delegation: false
compatibility_issues: false
important_findings: false
---

# Task Log: 입력 폼 Dialog 구현 (성함, 휴대폰 뒤4자리)

## Summary
- 사용자 입력을 받는 폼 다이얼로그(InputFormDialog) 및 ViewModel(InputFormViewModel)을 구현했습니다.
- PRD §1.2 워크플로우에 정의된 대로 "예약자 성함", "휴대폰 뒤4자리" 입력 필드를 포함합니다.
- 확인 버튼 텍스트를 동적으로 설정 가능하여 "촬영 시작" 및 "내보내기 시작" 시나리오에 모두 사용 가능합니다.

## Implementation Notes
- **ViewModels/InputFormViewModel.cs**:
  - INotifyPropertyChanged 구현
  - `string Name` 속성: 예약자 성함 (한글, 영문, 공백 허용)
  - `string Phone` 속성: 휴대폰 뒤4자리 (4자리 숫자)
  - PropertyChanged 이벤트로 UI 바인딩 지원

- **UI/InputFormDialog.xaml**:
  - ModernWpf 스타일 적용 (AlertDialog 패턴 참조)
  - StackPanel 레이아웃으로 두 TextBox 배치
  - "예약자 성함", "휴대폰 뒤4자리" 한국어 라벨
  - 확인/취소 버튼 (HorizontalAlignment="Right")
  - MaxLength="4" 설정으로 Phone 입력 제한
  - UpdateSourceTrigger=PropertyChanged로 즉시 바인딩

- **UI/InputFormDialog.xaml.cs**:
  - 생성자 파라미터 `confirmButtonText`로 확인 버튼 텍스트 동적 설정
  - `OnConfirmClick()`: DialogResult = true 반환
  - `OnCancelClick()`: DialogResult = false 반환
  - Loaded 이벤트에서 NameTextBox에 자동 포커스
  - `ViewModel` 프로퍼티로 입력 데이터 노출

## Build / Verification
- Command: `dotnet build -p:EnableWindowsTargeting=true`
- Result: ✅ 성공 (경고 0개, 오류 0개)

## Output Files
- ViewModels/InputFormViewModel.cs
- UI/InputFormDialog.xaml
- UI/InputFormDialog.xaml.cs

## Follow-ups / Risks
- Task 4.2에서 ValidationService를 통합하여 입력값 검증 로직 추가 예정 (PRD §FR-02 검증 규칙)
- Task 4.7에서 MainViewModel의 ButtonACommand/ButtonBCommand에 InputFormDialog 통합 예정

## Notes
- DialogResult 반환 패턴으로 ShowDialog() 사용 가능
- 확인 버튼 텍스트: "촬영 시작" (start_photo), "내보내기 시작" (export_files)로 구분 가능
- 취소 시 DialogResult = false, 사용자 입력 폐기
