using System.Windows;
using Lc_auto.Services;
using Lc_auto.UI.Components;
using Lc_auto.ViewModels;

namespace Lc_auto.UI;

/// <summary>
/// 사용자 입력 폼 다이얼로그 (예약자 성함, 휴대폰 뒤4자리)
/// </summary>
public partial class InputFormDialog : Window
{
    /// <summary>
    /// 사용자 입력 데이터를 담는 ViewModel
    /// </summary>
    public InputFormViewModel ViewModel { get; }

    /// <summary>
    /// InputFormDialog 생성자
    /// </summary>
    /// <param name="confirmButtonText">확인 버튼에 표시할 텍스트 (예: "촬영 시작", "내보내기 시작")</param>
    public InputFormDialog(string confirmButtonText = "확인")
    {
        InitializeComponent();

        ViewModel = new InputFormViewModel();
        DataContext = ViewModel;

        // 확인 버튼 텍스트 설정
        ConfirmButton.Content = confirmButtonText;

        // 첫 번째 입력 필드에 포커스
        Loaded += (_, _) => NameTextBox.Focus();
    }

    /// <summary>
    /// 확인 버튼 클릭 핸들러
    /// </summary>
    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        // 입력값 검증 (PRD §FR-02 검증 규칙)
        if (!ValidationService.ValidateName(ViewModel.Name))
        {
            DialogService.ShowDialog("입력 오류", "예약자 성함을 입력해주세요.");
            NameTextBox.Focus();
            return;
        }

        if (!ValidationService.ValidatePhone(ViewModel.Phone))
        {
            DialogService.ShowDialog("입력 오류", "휴대폰 뒤4자리를 정확히 입력해주세요. (4자리 숫자)");
            PhoneTextBox.Focus();
            return;
        }

        // 검증 성공 - 다이얼로그 종료
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// 취소 버튼 클릭 핸들러
    /// </summary>
    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
