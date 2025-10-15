using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lc_auto.ViewModels;

/// <summary>
/// 사용자 입력 폼 다이얼로그(예약자 성함, 휴대폰 뒤4자리)의 데이터를 관리하는 ViewModel입니다.
/// </summary>
public class InputFormViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _phone = string.Empty;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 예약자 성함 (한글, 영문, 공백 허용)
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }

            _name = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 휴대폰 뒤4자리 (4자리 숫자만 허용)
    /// </summary>
    public string Phone
    {
        get => _phone;
        set
        {
            if (_phone == value)
            {
                return;
            }

            _phone = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
