지침 1) “프로세스 한정 금지” + “모던/고전 창 동시 탐색”

의도: 특정 PID에서만 찾지 말고 데스크톱 전체에서 “폴더 선택” 창을 찾는다. 최신 윈도우는 #32770가 아닌 파일탐색기 기반(CabinetWClass/ExplorerFrame/XamlWindow) 으로 뜰 수 있으므로 둘 다 지원한다. 제목(Name) 고정 매칭 금지.

수정 요구사항(수용기준)

Desktop 루트에서 창 검색 (PID 필터 제거).

ClassName 후보: CabinetWClass, ExplorerFrame, XamlWindow, #32770.

최소 8초 간 Retry(200ms 간격)로 폴링.

찾은 창을 Focus() 할 수 있어야 함.

코드 스케치 (C# / FlaUI)

AutomationElement FindFolderPicker(AutomationBase a, TimeSpan? to = null) {
    var timeout = to ?? TimeSpan.FromSeconds(8);
    var desk = a.GetDesktop();
    var cf = a.ConditionFactory;

    var cond = cf.ByControlType(ControlType.Window)
        .And(cf.ByClassName("CabinetWClass")
          .Or(cf.ByClassName("ExplorerFrame"))
          .Or(cf.ByClassName("XamlWindow"))
          .Or(cf.ByClassName("#32770")));

    var r = Retry.WhileNull(() => desk.FindAllChildren(cond).FirstOrDefault(),
                            timeout, TimeSpan.FromMilliseconds(200));
    var win = r.Result ?? throw new TimeoutException("Folder picker not found");
    win.Focus();
    return win;
}

지침 2) 요소찾기 대신 Ctrl+L 주소창 단축키로 경로 입력

의도: 내부 UI 구조(AutomationId, 가상화)가 들쭉날쭉하므로 주소창 단축키 Ctrl+L → 경로 입력 → Enter가 가장 견고하다.

수정 요구사항(수용기준)

창을 찾은 직후 Ctrl+L 전송으로 주소 입력 모드 진입.

지정 경로(예: C:\)를 입력 후 Enter.

필요 시 ‘확인/선택/Open/Select’ 버튼 눌러 닫기(있을 때만).

코드 스케치

void TypePathIntoPicker(AutomationBase a, string path) {
    var dlg = FindFolderPicker(a);
    dlg.Focus();
    Keyboard.Press(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_L);
    Keyboard.Type(path);
    Keyboard.Press(VirtualKeyShort.RETURN);

    var cf = a.ConditionFactory;
    var ok = dlg.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button)
        .And(cf.ByName("확인").Or(cf.ByName("선택"))
                          .Or(cf.ByName("Open")).Or(cf.ByName("Select"))));
    ok?.AsButton()?.Invoke();
}

지침 3) 권한(Elevation) 정합 + 타이밍 보강

의도: 대상 창이 관리자 권한(또는 부모 프로세스가 관리자)이면 자동화도 관리자 권한이어야 UIA가 보인다. 또한 버튼 클릭 직후 대화상자 생성까지 대기/재시도가 필요하다.

수정 요구사항(수용기준)

테스트 실행 프로세스를 관리자 권한으로도 실행해보는 옵션 제공(문서화).

“폴더 선택” 버튼 클릭 후 최소 300~500ms 지연 → 위의 전역 탐색 수행.

Retry.WhileNull로 8초 이상 대기, 실패 시 창 리스트를 로그로 덤프(클래스/제목 기록).

코드 스케치

// 버튼 클릭 직후 약간 대기 후 탐색
button.AsButton().Invoke();
WaitHelpers.Sleep(400); // or Task.Delay
var dlg = FindFolderPicker(automation, TimeSpan.FromSeconds(8));

// 디버깅용 전체 윈도우 덤프(실패 시)
void DumpTopWindows(AutomationBase a) {
    var desk = a.GetDesktop();
    foreach (var w in desk.FindAllChildren(
        a.ConditionFactory.ByControlType(ControlType.Window))) {
        Console.WriteLine($"Window: Name='{w.Name}', Class='{w.Properties.ClassName.Value}', CT='{w.ControlType}'");
    }
}