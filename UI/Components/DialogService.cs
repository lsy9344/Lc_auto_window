using System.Windows;

namespace Lc_auto.UI.Components;

/// <summary>
/// 에러, 경고, 정보 메시지를 표시하는 재사용 가능한 다이얼로그 서비스
/// </summary>
public static class DialogService
{
    /// <summary>
    /// 지정된 제목과 메시지로 모달 다이얼로그를 표시합니다.
    /// </summary>
    /// <param name="title">다이얼로그 제목</param>
    /// <param name="message">표시할 메시지 내용</param>
    public static void ShowDialog(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK);
    }
}
