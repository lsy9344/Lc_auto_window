지침 4) 포그라운드 창 기반 폴백(클래스/제목 가정 불필요)

의도: 버튼 클릭 직후 실제 화면 최전면(Foreground)으로 올라온 창을 바로 잡아 Ctrl+L을 보내는 하드 폴백. 창을 못 찾아도 동작.

수용기준

“폴더 선택” 버튼 클릭 직후 ForegroundWindow가 바뀔 때까지 5초 폴링.

포그라운드 창의 프로세스가 테스트 프로세스와 다른지 확인(자기 자신에 오타 입력 방지).

잡은 핸들을 FlaUI 요소로 승격해 Focus() 후 Ctrl+L → 경로 입력 → Enter.

코드 스케치 (C# / FlaUI + P/Invoke)

static class Native {
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
}

AutomationElement WaitForegroundChanged(AutomationBase a, int ms = 5000) {
    var until = DateTime.UtcNow.AddMilliseconds(ms);
    uint myPid = (uint)Process.GetCurrentProcess().Id;
    IntPtr last = IntPtr.Zero;
    while (DateTime.UtcNow < until) {
        var h = Native.GetForegroundWindow();
        if (h != IntPtr.Zero && h != last) {
            last = h;
            Native.GetWindowThreadProcessId(h, out var pid);
            if (pid != myPid) { // 내 프로세스 아닌 진짜 떠오른 창
                var el = a.FromHandle(h);
                if (el != null) return el;
            }
        }
        Thread.Sleep(100);
    }
    throw new TimeoutException("Foreground window not detected.");
}

// 사용 예: 버튼 클릭 직후
button.AsButton().Invoke();
var dlg = WaitForegroundChanged(automation);
dlg.Focus();
Keyboard.Press(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_L);
Keyboard.Type(@"C:\");
Keyboard.Press(VirtualKeyShort.RETURN);

지침 5) Win32 HWND 크롤링 → FromHandle로 승격

의도: UIA 트리에 안 보이는 창을 Win32 수준에서 직접 열거(EnumWindows) 하여 후보를 고른 뒤 FlaUI로 승격. 제목·클래스 불문, 가시성/스타일/오너를 단서로 탐지.

수용기준

EnumWindows로 최상위 HWND 나열 → IsWindowVisible & GetWindowText 길이>0 필터.

오너(owner)가 내 메인 윈도우이거나(GW_OWNER), 스타일에 WS_EX_DLGMODALFRAME 있는 창 우선.

못 찾으면 ClassName 힌트(CabinetWClass, #32770, XamlWindow, ApplicationFrameWindow, DirectUIHWND 포함 자식) 가점.

최종 HWND를 automation.FromHandle(hwnd)로 요소화 → Ctrl+L 경로 입력.

코드 스케치

static class Native {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  [DllImport("user32.dll")] public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
  [DllImport("user32.dll")] public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd); // GW_OWNER=4
  [DllImport("user32.dll")] public static extern int GetWindowLong(IntPtr hWnd, int nIndex); // GWL_EXSTYLE=-20
  const int GW_OWNER = 4, GWL_EXSTYLE = -20, WS_EX_DLGMODALFRAME = 0x0001;
}

AutomationElement Win32CrawlPickFolderDialog(AutomationBase a, IntPtr mainHwnd) {
    var cands = new List<IntPtr>();
    Native.EnumWindows((h, l) => {
        if (!Native.IsWindowVisible(h)) return true;
        var sb = new StringBuilder(256);
        Native.GetWindowText(h, sb, sb.Capacity);
        var title = sb.ToString();
        if (string.IsNullOrWhiteSpace(title)) return true;

        // 힌트 점수 계산
        int score = 0;
        var clsSb = new StringBuilder(256);
        Native.GetClassName(h, clsSb, clsSb.Capacity);
        var cls = clsSb.ToString();

        var owner = Native.GetWindow(h, Native.GW_OWNER);
        var ex = Native.GetWindowLong(h, Native.GWL_EXSTYLE);

        if (owner == mainHwnd) score += 3;
        if ((ex & Native.WS_EX_DLGMODALFRAME) != 0) score += 2;

        if (cls.Contains("CabinetWClass") || cls.Contains("#32770") ||
            cls.Contains("XamlWindow") || cls.Contains("ApplicationFrameWindow"))
            score += 2;

        if (title.Contains("폴더") || title.Contains("찾아보기", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("Folder") || title.Contains("Browse"))
            score += 2;

        if (score >= 3) cands.Add(h);
        return true;
    }, IntPtr.Zero);

    var hwnd = cands.FirstOrDefault();
    if (hwnd == IntPtr.Zero) throw new Exception("No candidate dialog via Win32 crawl.");
    return a.FromHandle(hwnd) ?? throw new Exception("FromHandle failed.");
}

// 사용 예
button.AsButton().Invoke();
Thread.Sleep(400);
var main = app.GetMainWindow(automation);
var dlg = Win32CrawlPickFolderDialog(automation, main?.FrameworkAutomationElement.NativeWindowHandle ?? IntPtr.Zero);
dlg.Focus();
Keyboard.Press(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_L);
Keyboard.Type(@"C:\");
Keyboard.Press(VirtualKeyShort.RETURN);

지침 6) 백엔드/환경 전환(UA2↔UA3, x64, DPI, 샌드박스)

의도: UIA 백엔드/프로세스/환경 차이로 인해 트리가 숨는 케이스를 정면 돌파.

수용기준

UIA2Automation로 한 번, UIA3Automation로 한 번 모두 시도하는 팩토리 추가.

테스트 실행을 x64 빌드로 고정(Windows 64비트에서 권장).

시작 시 DPI 우회 적용: SetProcessDPIAware() 호출 또는 앱 매니페스트 DPI Aware 설정.

(중요) 앱과 동일/높은 권한으로 실행(관리자 권한 옵션). 샌드박스/가상 데스크톱(보안 데스크톱)에서는 불가.

코드 스케치

// 1) 백엔드 스위치 유틸
AutomationBase CreateAutomation(bool useUia2) =>
    useUia2 ? new UIA2Automation() : new UIA3Automation();

// 2) 시도 순서: UIA3 → 실패 시 UIA2
AutomationElement TryFindWithBothBackends(Func<AutomationBase, AutomationElement> finder) {
    using (var a3 = CreateAutomation(false)) {
        try { return finder(a3); } catch { /* fallthrough */ }
    }
    using (var a2 = CreateAutomation(true)) {
        return finder(a2);
    }
}

// 3) DPI 인지 (프로세스 초기에 1회)
[DllImport("user32.dll")] static extern bool SetProcessDPIAware();
static void EnsureDpiAware() { try { SetProcessDPIAware(); } catch {} }

// 사용 예
EnsureDpiAware();
var dlg = TryFindWithBothBackends(a => {
    // 앞서 만든 FindFolderPicker 또는 Win32CrawlPickFolderDialog 사용
    return FindFolderPicker(a, TimeSpan.FromSeconds(8));
});
dlg.Focus();
Keyboard.Press(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_L);
Keyboard.Type(@"C:\");
Keyboard.Press(VirtualKeyShort.RETURN);