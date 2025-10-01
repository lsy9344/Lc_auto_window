using System;
using System.Runtime.InteropServices;

namespace Lc_auto.Interop;

/// <summary>
/// Windows API P/Invoke 선언 (창 포커스 제어용)
/// </summary>
public static class Win32
{
    /// <summary>
    /// 창을 포어그라운드로 전환
    /// </summary>
    /// <param name="hWnd">창 핸들</param>
    /// <returns>성공 시 true, 실패 시 false</returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// 창의 표시 상태 설정 (최소화, 최대화, 복원 등)
    /// </summary>
    /// <param name="hWnd">창 핸들</param>
    /// <param name="nCmdShow">표시 명령 (SW_* 상수 사용)</param>
    /// <returns>성공 시 true, 실패 시 false</returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// 클래스 이름 및/또는 창 제목으로 최상위 창 검색
    /// </summary>
    /// <param name="lpClassName">클래스 이름 (null 가능)</param>
    /// <param name="lpWindowName">창 제목 (null 가능)</param>
    /// <returns>창 핸들, 찾지 못하면 IntPtr.Zero</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    /// <summary>
    /// 현재 포어그라운드 창 핸들 가져오기
    /// </summary>
    /// <returns>현재 포어그라운드 창 핸들</returns>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    // ShowWindow 명령 상수

    /// <summary>
    /// 창 숨기기
    /// </summary>
    public const int SW_HIDE = 0;

    /// <summary>
    /// 창을 정상 크기로 표시하고 활성화
    /// </summary>
    public const int SW_SHOWNORMAL = 1;

    /// <summary>
    /// 창을 최소화하고 활성화
    /// </summary>
    public const int SW_SHOWMINIMIZED = 2;

    /// <summary>
    /// 창을 최대화하고 활성화
    /// </summary>
    public const int SW_SHOWMAXIMIZED = 3;

    /// <summary>
    /// 창을 원래 크기와 위치로 복원하고 활성화
    /// </summary>
    public const int SW_RESTORE = 9;
}
