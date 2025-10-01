using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lc_auto.UI.Components;

namespace Lc_auto.ViewModels;

/// <summary>
/// MainWindow의 ViewModel - MVVM 패턴 구현
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// 속성 변경 알림 이벤트
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 버튼 A (촬영 시작) 명령
    /// </summary>
    public ICommand ButtonACommand { get; }

    /// <summary>
    /// 버튼 B (내보내기) 명령
    /// </summary>
    public ICommand ButtonBCommand { get; }

    /// <summary>
    /// 버튼 C (배경지 설치 동영상 보기) 명령
    /// </summary>
    public ICommand ButtonCCommand { get; }

    /// <summary>
    /// 버튼 D (사진 저장 폴더 열기) 명령
    /// </summary>
    public ICommand ButtonDCommand { get; }

    public MainViewModel()
    {
        // DialogService 사용 예제:
        // DialogService.ShowDialog("오류", "파일을 찾을 수 없습니다.");

        // ToastService 사용 예제:
        // ToastService.Show("설정을 성공적으로 로드했습니다.", 3000);

        // Command 초기화
        ButtonACommand = new RelayCommand(() => LogInfo("버튼 A 클릭됨"));
        ButtonBCommand = new RelayCommand(() => LogInfo("버튼 B 클릭됨"));
        ButtonCCommand = new RelayCommand(() => LogInfo("버튼 C 클릭됨"));
        ButtonDCommand = new RelayCommand(() => LogInfo("버튼 D 클릭됨"));
    }

    /// <summary>
    /// 속성 변경 알림 헬퍼 메서드
    /// </summary>
    /// <param name="propertyName">변경된 속성 이름 (자동 지정)</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 로그 출력 플레이스홀더 메서드
    /// </summary>
    /// <param name="message">로그 메시지</param>
    private void LogInfo(string message)
    {
        // TODO: Replace with LoggingService in Task 1.11
    }
}
