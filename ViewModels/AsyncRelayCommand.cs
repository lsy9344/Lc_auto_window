using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Lc_auto.Services;

namespace Lc_auto.ViewModels;

/// <summary>
/// 비동기 작업을 실행하면서 중복 실행을 방지하는 ICommand 구현체
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;

    /// <summary>
    /// AsyncRelayCommand 생성자
    /// </summary>
    /// <param name="execute">비동기 명령 본문</param>
    /// <param name="canExecute">추가 실행 조건 (선택)</param>
    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        if (_isExecuting)
        {
            return false;
        }

        return _canExecute?.Invoke() ?? true;
    }

    /// <inheritdoc />
    public async void Execute(object? parameter)
    {
        if (_isExecuting || (_canExecute != null && !_canExecute()))
        {
            return;
        }

        try
        {
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            await _execute();
        }
        catch (Exception ex)
        {
            LoggingService.LogError("AsyncRelayCommand execution failed.", ex);
        }
        finally
        {
            _isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
