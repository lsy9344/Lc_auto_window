using System.Windows.Input;

namespace Lc_auto.ViewModels;

/// <summary>
/// ICommand 인터페이스를 구현하는 재사용 가능한 명령 헬퍼 클래스
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    /// <summary>
    /// RelayCommand 생성자
    /// </summary>
    /// <param name="execute">명령 실행 시 호출될 액션</param>
    /// <param name="canExecute">명령 실행 가능 여부를 반환하는 함수 (선택 사항)</param>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// 명령 실행 가능 여부가 변경되었을 때 발생하는 이벤트
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// 명령 실행 가능 여부를 확인합니다.
    /// </summary>
    /// <param name="parameter">명령 매개변수</param>
    /// <returns>명령 실행 가능 여부</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute();
    }

    /// <summary>
    /// 명령을 실행합니다.
    /// </summary>
    /// <param name="parameter">명령 매개변수</param>
    public void Execute(object? parameter)
    {
        _execute();
    }
}
