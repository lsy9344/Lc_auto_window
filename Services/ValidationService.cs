using System.Text.RegularExpressions;

namespace Lc_auto.Services;

/// <summary>
/// 사용자 입력값 검증을 담당하는 서비스입니다.
/// PRD §FR-02/FR-03 검증 규칙을 구현합니다.
/// </summary>
public static partial class ValidationService
{
    /// <summary>
    /// 예약자 성함 검증 규칙: 비어 있지 않은 문자열 (한글, 영문, 공백 허용)
    /// </summary>
    /// <param name="name">검증할 성함</param>
    /// <returns>검증 성공 시 true</returns>
    public static bool ValidateName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <summary>
    /// 휴대폰 뒤4자리 검증 규칙: 정확히 4자리 숫자
    /// </summary>
    /// <param name="phone">검증할 휴대폰 뒤4자리</param>
    /// <returns>검증 성공 시 true</returns>
    public static bool ValidatePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return false;
        }

        return PhoneRegex().IsMatch(phone);
    }

    /// <summary>
    /// 휴대폰 뒤4자리 정규식: 정확히 4자리 숫자 (^\d{4}$)
    /// </summary>
    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex PhoneRegex();
}
