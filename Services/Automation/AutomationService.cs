using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA2;
using FlaUI.UIA3;
using Lc_auto.Interop;

namespace Lc_auto.Services.Automation;

/// <summary>
/// FlaUI 기반 Lightroom Classic UI 자동화 서비스 구현
/// FlaUIInspectData.md 기반으로 완전히 재작성됨
/// </summary>
public class AutomationService : IAutomationService, IDisposable
{
    private readonly AutomationBase _automation;
    private readonly AutomationBase? _automationFallback;
    private readonly IReadOnlyList<AutomationBase> _automationBackends;
    private readonly IConfigService _configService;
    private bool _disposed;

    // 자동화 설정
    private const int TimeoutPerAttemptMs = 30000; // 30초
    private const int MaxRetries = 2; // 초기 시도 1회 + 재시도 2회 = 총 3회

    // 카메라 설정 매핑 (FlaUIInspectData.md 기반)
    private static readonly Dictionary<string, int> ShutterMapping = new()
    {
        { "4", 23 }, { "5", 24 }, { "6", 25 }, { "8", 26 }, { "10", 27 }, { "13", 28 }, { "15", 29 }, { "20", 30 }
    };

    private static readonly Dictionary<string, int> ApertureMapping = new()
    {
        { "7.1", 6 }, { "8", 7 }, { "9", 8 }, { "10", 9 }, { "11", 10 }, { "13", 11 }, { "14", 12 }, { "26", 13 }
    };

    private static readonly Dictionary<string, int> ISOMapping = new()
    {
        { "100", 1 }, { "200", 2 }, { "400", 3 }, { "800", 4 }, { "1600", 5 }
    };

    private static readonly Dictionary<string, int> WhiteBalanceMapping = new()
    {
        { "자동", 1 }, { "일광", 2 }, { "그늘", 3 }, { "흐림", 4 }, { "텅스텐", 5 }, { "형광", 6 }, { "플래시", 7 }, { "수동", 8 }
    };
    private static readonly string[] FolderDialogClassNames =
    [
        "#32770",        // FlaUIInspectData.md: 폴더 선택 윈도우
        "CabinetWClass",
        "ExploreWClass",
        "ApplicationFrameWindow"
    ];

    private static readonly string[] FolderDialogWindowNames =
    [
        "폴더 선택",
        "폴더 선택(&F)",
        "Select Folder",
    ];

    private static readonly string[] FolderDialogTitleKeywords =
    [
        "폴더",
        "Folder",
        "찾아보기",
        "찾기",
        "Browse",
        "Select Folder",
        "폴더 선택"
    ];

    private static readonly string[] FolderDialogConfirmKeywords =
    [
        "폴더 선택",
        "폴더 선택(&F)",
        "선택",
        "Select",
        "Select Folder",
        "확인",
        "OK"
    ];

    public AutomationService(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _automation = new UIA3Automation();

        try
        {
            _automationFallback = new UIA2Automation();
        }
        catch (Exception ex)
        {
            _automationFallback = null;
            LoggingService.LogWarn($"UIA2 자동화 백엔드 초기화 실패 - UIA3만 사용합니다: {ex.Message}");
        }

        _automationBackends = _automationFallback != null
            ? new[] { _automation, _automationFallback }
            : new[] { _automation };

        LogAutomationEnvironment();
        LoggingService.LogInfo(_automationFallback != null
            ? "AutomationService 초기화 완료 (기본: UIA3, 폴백: UIA2)"
            : "AutomationService 초기화 완료 (UIA3 단일 백엔드)");
    }

    /// <inheritdoc />
    public async Task<AutomationResult> RunAsync(
        string featureId,
        Dictionary<string, string> inputValues,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            LoggingService.LogWarn("빈 featureId로 RunAsync() 호출됨");
            return AutomationResult.CreateFailure("featureId가 비어 있습니다.", 0);
        }

        var sw = Stopwatch.StartNew();
        LoggingService.LogInfo($"자동화 시작: {featureId}");

        // 스크립트 조회
        var script = FeatureSelectorRegistry.Get(featureId);
        if (script == null)
        {
            sw.Stop();
            var errorMsg = $"자동화 스크립트를 찾을 수 없습니다: {featureId}. FeatureSelectorRegistry에 스크립트를 등록하세요.";
            LoggingService.LogWarn(errorMsg);
            return AutomationResult.CreateFailure(errorMsg, sw.ElapsedMilliseconds);
        }

        // 재시도 로직
        Exception? lastException = null;
        for (int attempt = 1; attempt <= MaxRetries + 1; attempt++)
        {
            if (ct.IsCancellationRequested)
            {
                sw.Stop();
                LoggingService.LogWarn($"자동화 취소됨: {featureId}");
                return AutomationResult.CreateFailure("사용자가 자동화를 취소했습니다.", sw.ElapsedMilliseconds);
            }

            try
            {
                LoggingService.LogInfo($"자동화 시도 {attempt}/{MaxRetries + 1}: {featureId}");

                // 자동화 실행
                await ExecuteScriptAsync(script, inputValues, ct);

                sw.Stop();
                LoggingService.LogInfo($"자동화 성공: {featureId} (소요 시간: {sw.ElapsedMilliseconds}ms)");
                return AutomationResult.CreateSuccess($"자동화가 성공적으로 완료되었습니다: {script.Description}", sw.ElapsedMilliseconds);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                LoggingService.LogWarn($"자동화 시도 {attempt}/{MaxRetries + 1} 실패: {ex.Message}");

                if (attempt <= MaxRetries)
                {
                    // 재시도 전 대기 (1초)
                    await Task.Delay(1000, ct);
                }
            }
        }

        // 모든 재시도 실패
        sw.Stop();
        var failureMsg = $"자동화 실패 ({MaxRetries + 1}회 시도): {script.Description}";
        if (lastException != null)
        {
            LoggingService.LogWarn($"{failureMsg} - {lastException.Message}", lastException);
        }
        else
        {
            LoggingService.LogWarn(failureMsg);
        }

        return AutomationResult.CreateFailure(failureMsg, sw.ElapsedMilliseconds, lastException);
    }

    /// <summary>
    /// FeatureScript를 실행합니다.
    /// </summary>
    private async Task ExecuteScriptAsync(
        FeatureScript script,
        Dictionary<string, string> inputValues,
        CancellationToken ct)
    {
        // Lightroom Classic 프로세스 찾기
        var lightroomProcess = Process.GetProcessesByName("Lightroom").FirstOrDefault()
            ?? Process.GetProcessesByName("Adobe Lightroom Classic").FirstOrDefault();

        // Lightroom이 실행 중이지 않으면 자동 실행
        if (lightroomProcess == null)
        {
            LoggingService.LogInfo("Lightroom Classic 프로세스를 찾을 수 없습니다. 자동 실행을 시작합니다.");

            var lightroomPath = _configService.Current.Automation?.LightroomPath;
            var startupWaitSec = _configService.Current.Automation?.StartupWaitSec ?? 5;

            if (string.IsNullOrWhiteSpace(lightroomPath))
            {
                throw new InvalidOperationException("config.json의 automation.lightroomPath가 설정되지 않았습니다.");
            }

            if (!System.IO.File.Exists(lightroomPath))
            {
                throw new InvalidOperationException($"Lightroom 실행 파일을 찾을 수 없습니다: {lightroomPath}");
            }

            // Lightroom 실행
            LoggingService.LogInfo($"Lightroom Classic 실행: {lightroomPath}");
            var startInfo = new ProcessStartInfo
            {
                FileName = lightroomPath,
                UseShellExecute = true
            };

            lightroomProcess = Process.Start(startInfo);

            if (lightroomProcess == null)
            {
                throw new InvalidOperationException("Lightroom Classic 실행에 실패했습니다.");
            }

            // 시작 대기
            LoggingService.LogInfo($"Lightroom Classic 시작 대기 중 ({startupWaitSec}초)...");
            await Task.Delay(startupWaitSec * 1000, ct);

            // 프로세스 재탐색 (실제 Lightroom 메인 프로세스 찾기)
            lightroomProcess = Process.GetProcessesByName("Lightroom").FirstOrDefault()
                ?? Process.GetProcessesByName("Adobe Lightroom Classic").FirstOrDefault();

            if (lightroomProcess == null)
            {
                throw new InvalidOperationException($"Lightroom Classic 실행 후에도 프로세스를 찾을 수 없습니다 ({startupWaitSec}초 대기 후).");
            }

            LoggingService.LogInfo($"Lightroom Classic 프로세스 발견 (PID: {lightroomProcess.Id})");
        }
        else
        {
            LoggingService.LogInfo($"실행 중인 Lightroom Classic 프로세스 발견 (PID: {lightroomProcess.Id})");
        }

        using var app = Application.Attach(lightroomProcess.Id);
        var mainWindow = app.GetMainWindow(_automation);

        if (mainWindow == null)
        {
            throw new InvalidOperationException("Lightroom Classic 메인 창을 찾을 수 없습니다.");
        }

        LoggingService.LogInfo($"Lightroom Classic 메인 창 확인 완료");

        // 메인 창에 포커스 설정
        mainWindow.Focus();
        LoggingService.LogInfo("Lightroom Classic 메인 창에 포커스를 설정했습니다.");

        // 각 스텝 실행
        foreach (var step in script.Steps)
        {
            ct.ThrowIfCancellationRequested();

            LoggingService.LogInfo($"스텝 실행: {step.Description}");

            // Wait 액션은 UI 요소를 찾지 않고 바로 실행
            if (IsPassiveAction(step.Action))
            {
                await PerformActionAsync(app, null, step, inputValues, ct);
            }
            else
            {
                // 현재 활성 Window 가져오기 (대화상자가 열렸을 수 있음)
                var currentWindow = GetCurrentActiveWindow(app, step);

                LoggingService.LogInfo($"현재 활성 Window: {GetSafeProperty(currentWindow, e => e.Name)} (ClassName: {GetSafeProperty(currentWindow, e => e.ClassName)})");

                // UI 요소 찾기
                var element = FindElement(currentWindow, step.Selector);
                if (element == null)
                {
                    // 실패 시 상세한 윈도우 정보 기록
                    LoggingService.LogWarn($"UI 요소를 찾을 수 없어 실패: {step.Description}");
                    LoggingService.LogWarn($"찾으려던 UI 요소 정보:");
                    LoggingService.LogWarn($"  - AutomationId: '{step.Selector.AutomationId}'");
                    LoggingService.LogWarn($"  - Name: '{step.Selector.Name}'");
                    LoggingService.LogWarn($"  - ClassName: '{step.Selector.ClassName}'");
                    LoggingService.LogWarn($"  - ControlType: '{step.Selector.ControlType}'");

                    // 모든 윈도우 목록 기록
                    var allWindowsForDebug = CollectAllWindows(app);
                    LogWindowSnapshot(allWindowsForDebug, "실패 시점의 전체 윈도우 목록");

                    throw new InvalidOperationException($"UI 요소를 찾을 수 없습니다: {step.Description}");
                }

                // UI 요소 정보 로깅
                LogElementInfo(element, step.Description);

                // 액션 수행
                await PerformActionAsync(app, element, step, inputValues, ct);
            }

            // ComboBox 클릭 후에는 UI 안정화를 위해 짧게 대기
            if (step.Description.Contains("콤보박스 클릭"))
            {
                await Task.Delay(300, ct);
            }
            else
            {
                await Task.Delay(1000, ct);
            }
        }
    }

    private static bool IsPassiveAction(string action)
    {
        return action.Equals("Wait", StringComparison.OrdinalIgnoreCase) ||
               action.Equals("WaitForElement", StringComparison.OrdinalIgnoreCase) ||
               action.Equals("WaitForFolderDialog", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 현재 활성화된 Window를 가져옵니다.
    /// MenuItem을 찾을 때는 항상 메인 창을 사용하고,
    /// 다른 UI 요소를 찾을 때만 특정 대화상자를 전환합니다.
    /// </summary>
    private AutomationElement GetCurrentActiveWindow(Application app, AutomationStep? currentStep = null)
    {
        var mainWindow = app.GetMainWindow(_automation);
        var appWindows = app.GetAllTopLevelWindows(_automation);
        LoggingService.LogInfo($"앱의 Window 개수: {appWindows.Length}");

        var desktopWindows = _automation
            .GetDesktop()
            .FindAllChildren(cf =>
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Window)
                    .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu))
                    .Or(cf.ByClassName("#32768")));
        LoggingService.LogInfo($"데스크톱의 Window 개수: {desktopWindows.Length}");

        var allWindows = MergeWindows(appWindows, desktopWindows);
        LogSystemDialogs(allWindows);

        // MenuItem을 찾는 경우 - 템플릿 관련 스텝에서만 ComboBox 드롭다운 사용
        if (currentStep != null &&
            !string.IsNullOrWhiteSpace(currentStep.Selector.ControlType) &&
            currentStep.Selector.ControlType.Equals("MenuItem", StringComparison.OrdinalIgnoreCase))
        {
            // 템플릿 항목 선택 스텝인지 확인 (AutomationId: 3 또는 "원본 파일 번호" 포함)
            var isTemplateSelectionStep =
                (currentStep.Selector.AutomationId?.Equals("3", StringComparison.OrdinalIgnoreCase) == true) ||
                (!string.IsNullOrWhiteSpace(currentStep.Selector.Name) &&
                 currentStep.Selector.Name.Contains("원본 파일 번호", StringComparison.Ordinal));

            if (isTemplateSelectionStep)
            {
                LoggingService.LogInfo("템플릿 선택 스텝 감지 - ComboBox 드롭다운 검색 우선");

                // ComboBox 드롭다운 메뉴를 찾음
                var comboBoxDropdown = FindComboBoxDropdown(allWindows);
                if (comboBoxDropdown != null)
                {
                    LoggingService.LogInfo($"템플릿 선택 - ComboBox 드롭다운 사용 (Name: {GetSafeProperty(comboBoxDropdown, e => e.Name)})");

                    // 포커스 설정 시 팝업이 닫히는 문제를 방지하기 위해 포커스 설정을 생략합니다.
                    // TryBringWindowToFront(comboBoxDropdown);
                    return comboBoxDropdown;
                }
            }

            // 일반적인 컨텍스트 메뉴 찾기 시도
            AutomationElement? contextMenu = null;

            // 최대 3번 컨텍스트 메뉴 검색 시도
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                contextMenu = FindContextMenu(allWindows);
                if (contextMenu != null)
                {
                    LoggingService.LogInfo($"MenuItem 검색 - 컨텍스트 메뉴 사용 (시도 {attempt}, Name: {GetSafeProperty(contextMenu, e => e.Name)})");

                    // 포커스 설정 실패해도 계속 진행
                    TryBringWindowToFront(contextMenu);
                    return contextMenu;
                }

                if (attempt < 3)
                {
                    // 짧은 대기 후 재시도
                    System.Threading.Thread.Sleep(200);

                    // 윈도우 목록 새로고침
                    appWindows = app.GetAllTopLevelWindows(_automation);
                    desktopWindows = _automation
                        .GetDesktop()
                        .FindAllChildren(cf =>
                            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Window)
                                .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu))
                                .Or(cf.ByClassName("#32768")));
                    allWindows = MergeWindows(appWindows, desktopWindows);
                }
            }

            // 어떤 메뉴도 찾지 못했다면 로그를 남기고 메인 창 사용
            LoggingService.LogWarn("컨텍스트 메뉴를 찾지 못했습니다. 메인 창을 사용합니다.");

            if (mainWindow != null)
            {
                LoggingService.LogInfo($"MenuItem 검색 - 메인 창 사용 (Name: {GetSafeProperty(mainWindow, e => e.Name)})");
                TryBringWindowToFront(mainWindow);
                return mainWindow;
            }
            else
            {
                LoggingService.LogWarn("MenuItem 검색 중 메인 창을 찾을 수 없습니다.");
            }
        }

        // Window를 찾는 경우 - 연결전송된 촬영 설정 윈도우 우선 탐색
        if (currentStep != null &&
            !string.IsNullOrWhiteSpace(currentStep.Selector.ControlType) &&
            currentStep.Selector.ControlType.Equals("Window", StringComparison.OrdinalIgnoreCase))
        {
            var tetheredCaptureDialog = FindTetheredCaptureDialog(allWindows);
            if (tetheredCaptureDialog != null)
            {
                LoggingService.LogInfo($"연결전송된 촬영 설정 윈도우 발견 (Name: {GetSafeProperty(tetheredCaptureDialog, e => e.Name)}) - 이 Window 사용");
                TryBringWindowToFront(tetheredCaptureDialog);
                return tetheredCaptureDialog;
            }
        }

        // 다른 UI 요소를 찾는 경우 - 대화상자 우선 순위 적용
        // FlaUIInspectData.md 기반: #32770 클래스 이름의 '폴더 선택' 윈도우 우선 탐색
        var folderDialog = FindFolderDialogByProcess(app.ProcessId) ?? FindFolderDialogWindow(allWindows);
        if (folderDialog != null)
        {
            LoggingService.LogInfo($"폴더 선택 대화상자 발견 (Name: {GetSafeProperty(folderDialog, e => e.Name)}, ClassName: {GetSafeProperty(folderDialog, e => e.ClassName)}) - 이 Window 사용");
            TryBringWindowToFront(folderDialog);
            return folderDialog;
        }

        if (mainWindow != null)
        {
            LoggingService.LogInfo($"메인 창 사용 (Name: {GetSafeProperty(mainWindow, e => e.Name)})");
            return mainWindow;
        }

        if (appWindows.Length > 0)
        {
            LoggingService.LogWarn("메인 창을 찾을 수 없어 첫 번째 App Window 사용");
            return appWindows[0];
        }

        throw new InvalidOperationException("활성 Window를 찾을 수 없습니다.");
    }

    private AutomationElement[] MergeWindows(AutomationElement[] appWindows, AutomationElement[] desktopWindows)
    {
        return appWindows
            .Concat(desktopWindows)
            .Distinct()
            .ToArray();
    }

    private void LogSystemDialogs(IEnumerable<AutomationElement> windows)
    {
        var systemDialogs = windows
            .Where(w => string.Equals(GetSafeProperty(w, e => e.ClassName), "#32770", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (systemDialogs.Length == 0)
        {
            return;
        }

        LoggingService.LogInfo($"발견된 #32770 시스템 대화상자: {systemDialogs.Length}개");
        foreach (var dialog in systemDialogs)
        {
            var dialogInfo = $"  - Dialog: Name='{GetSafeProperty(dialog, e => e.Name)}', ClassName='#32770'";
            LoggingService.LogInfo(dialogInfo);
        }
    }

  
    /// <summary>
    /// '폴더 선택' 대화상자를 찾습니다.
    /// FlaUIInspectData.md 기반: Name='폴더 선택', ClassName='#32770'
    /// </summary>
    private AutomationElement? FindFolderDialogWindow(IEnumerable<AutomationElement> windows)
    {
        foreach (var window in windows)
        {
            if (IsFolderPickerWindow(_automation, window))
            {
                var name = GetSafeProperty(window, e => e.Name);
                var className = GetSafeProperty(window, e => e.ClassName);
                LoggingService.LogInfo($"폴더 선택 대화상자 후보 발견 (Name: {name}, ClassName: {className})");
                return window;
            }
        }

        LoggingService.LogInfo("폴더 선택 대화상자를 찾지 못했습니다 (표준 UIA 탐색)");
        return null;
    }

    /// <summary>
    /// '연결전송된 촬영 설정' 윈도우 팝업 확인
    /// FlaUIInspectData.md 기반: Name='연결전송된 촬영 설정', ClassName='Afx:0000000140000000:0'
    /// </summary>
    private AutomationElement? FindTetheredCaptureDialog(IEnumerable<AutomationElement> windows)
    {
        foreach (var window in windows)
        {
            var name = GetSafeProperty(window, e => e.Name);
            var className = GetSafeProperty(window, e => e.ClassName);

            // FlaUIInspectData.md 정보: Name='연결전송된 촬영 설정', ClassName='Afx:0000000140000000:0'
            if (name.Equals("연결전송된 촬영 설정", StringComparison.Ordinal) &&
                className.Equals("Afx:0000000140000000:0", StringComparison.Ordinal))
            {
                LoggingService.LogInfo($"'연결전송된 촬영 설정' 윈도우 발견 (Name: {name}, ClassName: {className})");
                return window;
            }
        }

        LoggingService.LogInfo("'연결전송된 촬영 설정' 윈도우를 찾지 못했습니다");
        return null;
    }

    private AutomationElement? FindFolderDialogByProcess(int processId)
    {
        if (processId <= 0)
        {
            return null;
        }

        var desktop = _automation.GetDesktop();
        var cf = _automation.ConditionFactory;

        var processWindows = desktop.FindAllChildren(
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Window)
              .And(cf.ByProcessId(processId)));

        foreach (var window in processWindows)
        {
            var title = NormalizeValue(GetSafeProperty(window, e => e.Name));
            if (!string.IsNullOrEmpty(title) &&
                FolderDialogWindowNames.Any(n => string.Equals(n, title, StringComparison.OrdinalIgnoreCase)))
            {
                LoggingService.LogInfo($"프로세스 ID {processId}에서 폴더 선택 창을 직접 감지했습니다 (Name: {title}, ClassName: {GetSafeProperty(window, e => e.ClassName)})");
                return window;
            }
        }

        foreach (var window in processWindows)
        {
            if (IsFolderDialogWindow(window))
            {
                LoggingService.LogInfo($"프로세스 ID {processId}의 창 중 폴더 대화상자를 감지했습니다 (Name: {GetSafeProperty(window, e => e.Name)}, ClassName: {GetSafeProperty(window, e => e.ClassName)})");
                return window;
            }
        }

        LoggingService.LogInfo($"프로세스 ID {processId}에서 폴더 선택 대화상자를 찾지 못했습니다 (총 {processWindows.Length}개의 프로세스 윈도우 검색됨)");
        return null;
    }

    private bool IsFolderDialogWindow(AutomationElement window)
    {
        var className = NormalizeValue(GetSafeProperty(window, e => e.ClassName));
        var title = NormalizeValue(GetSafeProperty(window, e => e.Name));

        // 사용자가 제공한 특정 케이스에 대한 명시적 확인
        if (className.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase) &&
            title.Equals("Adobe Lightroom Classic - 파일 탐색기", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!FolderDialogClassNames.Contains(className, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(title))
        {
            if (ContainsKeyword(title, FolderDialogTitleKeywords))
            {
                return true;
            }
        }

        var conditionFactory = _automation.ConditionFactory;

        try
        {
            // 확인/선택 버튼 검사 (AutomationId 1 또는 예상 Name)
            var confirmButton = window.FindFirstDescendant(conditionFactory.ByAutomationId("1")
                .And(conditionFactory.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

            if (confirmButton != null)
            {
                var buttonName = NormalizeValue(GetSafeProperty(confirmButton, e => e.Name));
                if (!string.IsNullOrEmpty(buttonName) && ContainsKeyword(buttonName, FolderDialogConfirmKeywords))
                {
                    return true;
                }
            }

            // 주소창 관련 컨트롤 존재 여부 (업데이트된 UI 정보 기반)
            // 새로운 UI: AutomationId: 1001, Name: 폴더 선택, ControlType: Button
            // 기존 UI: AutomationId: 1001, 41477, 1148 (ToolbarWindow32)
            var addressBarButton = window.FindFirstDescendant(conditionFactory.ByAutomationId("1001")
                .And(conditionFactory.ByControlType(FlaUI.Core.Definitions.ControlType.Button))
                .And(conditionFactory.ByName("폴더 선택")));

            if (addressBarButton != null)
            {
                LoggingService.LogInfo("폴더 대화상자 확인: '폴더 선택' 주소창 버튼 발견 (새로운 UI)");
                return true;
            }

            // 기존 주소창 컨트롤 (하위 호환성)
            var addressBar = window.FindFirstDescendant(conditionFactory.ByAutomationId("1001")) ??
                             window.FindFirstDescendant(conditionFactory.ByAutomationId("41477")) ??
                             window.FindFirstDescendant(conditionFactory.ByAutomationId("1148"));
            if (addressBar != null)
            {
                return true;
            }

            // '폴더 선택' 버튼 또는 '선택...' 버튼 존재 여부
            var selectButton = window.FindFirstDescendant(conditionFactory.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                .And(conditionFactory.ByName("폴더 선택"))) ??
                window.FindFirstDescendant(
                    conditionFactory.ByAutomationId("65535")
                        .And(conditionFactory.ByName("선택..."))
                        .And(conditionFactory.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

            if (selectButton != null)
            {
                var buttonName = GetSafeProperty(selectButton, e => e.Name);
                if (buttonName == "선택...")
                {
                    LoggingService.LogInfo("폴더 대화상자 확인: '선택...' 버튼 발견 (FlaUIInspectData.md 기반)");
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"폴더 대화상자 탐지 중 오류: {ex.Message}", ex);
        }

        return false;
    }

    /// <summary>
    /// ComboBox 드롭다운이 열려있는지 확인합니다.
    /// 사용자 제공 inspection 데이터 기반: ClassName:#32768, ControlType:Menu, Name:컨텍스트
    /// </summary>
    private bool IsComboBoxDropdownOpen()
    {
        try
        {
            var desktop = _automation.GetDesktop();
            var cf = _automation.ConditionFactory;

            // 방법 1: 특정 이름과 클래스를 가진 메뉴 찾기
            var comboBoxMenu = desktop.FindFirstDescendant(
                cf.ByClassName("#32768")
                    .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu))
                    .And(cf.ByName("컨텍스트")));

            if (comboBoxMenu != null)
            {
                LoggingService.LogInfo("ComboBox 드롭다운 발견 (방법 1: 특정 조건)");
                return true;
            }

            // 방법 2: 모든 #32768 클래스 윈도우에서 Menu 컨트롤 찾기
            var allWindows = desktop.FindAllChildren(cf.ByClassName("#32768"));
            foreach (var window in allWindows)
            {
                var menus = window.FindAllDescendants(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu));
                if (menus != null && menus.Length > 0)
                {
                    // 메뉴에 MenuItem 자식이 있는지 확인
                    var menuItems = window.FindAllDescendants(cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem));
                    if (menuItems != null && menuItems.Length > 0)
                    {
                        LoggingService.LogInfo($"ComboBox 드롭다운 발견 (방법 2: #32768 클래스, MenuItem {menuItems.Length}개)");
                        return true;
                    }
                }
            }

            // 방법 3: MenuItem을 직접 검색 (AutomationId: 3, Name: 사용자 정의 이름 - 원본 파일 번호)
            var specificMenuItem = desktop.FindFirstDescendant(
                cf.ByAutomationId("3")
                    .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem))
                    .And(cf.ByName("사용자 정의 이름 - 원본 파일 번호")));

            if (specificMenuItem != null)
            {
                LoggingService.LogInfo("ComboBox 드롭다운 발견 (방법 3: 특정 MenuItem)");
                return true;
            }

            // 방법 4: 모든 MenuItem 검색으로 광범위하게 확인
            var allMenuItems = desktop.FindAllDescendants(cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem));
            if (allMenuItems != null && allMenuItems.Length > 0)
            {
                // 원본 파일 번호 관련 MenuItem이 있는지 확인
                var fileNumberMenuItem = allMenuItems.FirstOrDefault(item =>
                    item.Name != null && item.Name.Contains("원본 파일 번호"));

                if (fileNumberMenuItem != null)
                {
                    LoggingService.LogInfo($"ComboBox 드롭다운 발견 (방법 4: 원본 파일 번호 MenuItem: {fileNumberMenuItem.Name})");
                    return true;
                }
            }

            LoggingService.LogInfo("ComboBox 드롭다운을 찾지 못함");
            return false;
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"ComboBox 드롭다운 확인 중 오류: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ComboBox 드롭다운 메뉴를 찾습니다.
    /// 사용자 제공 inspection 데이터 기반: ClassName:#32768, ControlType:Menu, Name:컨텍스트
    /// </summary>
    private AutomationElement? FindComboBoxDropdown(IEnumerable<AutomationElement> windows)
    {
        try
        {
            foreach (var window in windows)
            {
                var dropdown = TryResolveComboDropdownFromElement(window);
                if (dropdown != null)
                {
                    LoggingService.LogInfo($"ComboBox 드롭다운 발견 (Name: {GetSafeProperty(dropdown, e => e.Name)}, ClassName: {GetSafeProperty(dropdown, e => e.ClassName)})");
                    return dropdown;
                }
            }

            var desktopDropdown = FindComboBoxDropdownOnDesktop();
            if (desktopDropdown != null)
            {
                LoggingService.LogInfo($"데스크톱에서 ComboBox 드롭다운 발견 (Name: {GetSafeProperty(desktopDropdown, e => e.Name)}, ClassName: {GetSafeProperty(desktopDropdown, e => e.ClassName)})");
                return desktopDropdown;
            }

            return null;
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"ComboBox 드롭다운 검색 중 오류: {ex.Message}");
            return null;
        }
    }

    private AutomationElement? TryResolveComboDropdownFromElement(AutomationElement element)
    {
        if (IsComboBoxDropdownCandidate(element))
        {
            return element;
        }

        try
        {
            var menuDescendant = element.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu));
            if (menuDescendant != null)
            {
                return WalkUpToDropdownHost(menuDescendant);
            }
        }
        catch (Exception)
        {
            // 메뉴 탐색이 실패해도 무시
        }

        return null;
    }

    private AutomationElement? FindComboBoxDropdownOnDesktop()
    {
        try
        {
            var desktop = _automation.GetDesktop();
            var cf = _automation.ConditionFactory;

            var popupCandidates = desktop.FindAllChildren(cf => cf.ByClassName("#32768"));
            foreach (var candidate in popupCandidates ?? Array.Empty<AutomationElement>())
            {
                var dropdown = TryResolveComboDropdownFromElement(candidate);
                if (dropdown != null)
                {
                    return dropdown;
                }
            }

            var menuItems = desktop.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem));
            foreach (var item in menuItems ?? Array.Empty<AutomationElement>())
            {
                var automationId = NormalizeValue(GetSafeProperty(item, e => e.AutomationId));
                var name = NormalizeValue(GetSafeProperty(item, e => e.Name));

                if (automationId.Equals("3", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("사용자 정의 이름 - 원본 파일 번호", StringComparison.Ordinal) ||
                    name.Contains("원본 파일 번호", StringComparison.Ordinal))
                {
                    var dropdownHost = WalkUpToDropdownHost(item);
                    if (dropdownHost != null)
                    {
                        return dropdownHost;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"데스크톱 ComboBox 드롭다운 직접 검색 중 오류: {ex.Message}");
        }

        return null;
    }

    private AutomationElement? WalkUpToDropdownHost(AutomationElement element)
    {
        var current = element;
        while (current != null)
        {
            if (IsComboBoxDropdownCandidate(current))
            {
                return current;
            }

            try
            {
                current = current.Parent;
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
    }

    private bool IsComboBoxDropdownCandidate(AutomationElement element)
    {
        try
        {
            var className = NormalizeValue(GetSafeProperty(element, e => e.ClassName));
            var controlType = element.ControlType;

            var isMenuPopup = className.Equals("#32768", StringComparison.OrdinalIgnoreCase) ||
                              controlType == FlaUI.Core.Definitions.ControlType.Menu;

            if (!isMenuPopup)
            {
                return false;
            }

            return ContainsDropdownItems(element);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool ContainsDropdownItems(AutomationElement element)
    {
        try
        {
            var menuItems = element.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem));
            if (menuItems != null && menuItems.Length > 0)
            {
                return true;
            }

            var listItems = element.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem));
            return listItems != null && listItems.Length > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 열려 있는 컨텍스트 메뉴(팝업 메뉴)를 찾습니다.
    /// </summary>
    private AutomationElement? FindContextMenu(IEnumerable<AutomationElement> windows)
    {
        // 먼저 모든 윈도우에서 컨텍스트 메뉴 검색
        foreach (var window in windows)
        {
            var className = GetSafeProperty(window, e => e.ClassName);
            var name = GetSafeProperty(window, e => e.Name);

            // 컨텍스트 메뉴는 보통 #32768 클래스 이름을 가짐
            if (className.Equals("#32768", StringComparison.OrdinalIgnoreCase))
            {
                // 메뉴인지 확인 (MenuItem 자식이 있는지)
                var menuItems = window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem));
                if (menuItems != null && menuItems.Length > 0)
                {
                    LoggingService.LogInfo($"컨텍스트 메뉴 발견 (Name: {name}, ClassName: {className}, MenuItem 개수: {menuItems.Length})");
                    return window;
                }
            }
        }

        // 첫 번째 검색에서 찾지 못했다면, 데스크톱에서 직접 검색
        try
        {
            var desktop = _automation.GetDesktop();
            var allDesktopWindows = desktop.FindAllChildren(cf =>
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Window)
                    .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu))
                    .Or(cf.ByClassName("#32768")));

            foreach (var window in allDesktopWindows)
            {
                var className = GetSafeProperty(window, e => e.ClassName);
                var name = GetSafeProperty(window, e => e.Name);

                if (className.Equals("#32768", StringComparison.OrdinalIgnoreCase))
                {
                    var menuItems = window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem));
                    if (menuItems != null && menuItems.Length > 0)
                    {
                        LoggingService.LogInfo($"데스크톱에서 컨텍스트 메뉴 발견 (Name: {name}, ClassName: {className}, MenuItem 개수: {menuItems.Length})");
                        return window;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"데스크톱에서 컨텍스트 메뉴 검색 중 오류: {ex.Message}");
        }

        return null;
    }

    private static bool ContainsKeyword(string value, string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return keywords.Any(keyword =>
            value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Equals("null", StringComparison.OrdinalIgnoreCase) ? string.Empty : value;
    }

    private void TryBringWindowToFront(AutomationElement window)
    {
        try
        {
            if (window == null)
            {
                LoggingService.LogWarn("TryBringWindowToFront: window가 null입니다.");
                return;
            }

            window.Focus();

            var asWindow = window.AsWindow();
            asWindow?.SetForeground();
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"대화상자 포커스 설정 실패: {ex.Message}");
            // 포커스 설정 실패해도 계속 진행
        }
    }

    private async Task ClearAndTypeAsync(AutomationElement element, string text, CancellationToken ct)
    {
        var className = NormalizeValue(GetSafeProperty(element, e => e.ClassName));
        var controlType = element.ControlType;

        // ValuePattern 지원 시 먼저 시도
        if (element.Patterns.Value.IsSupported)
        {
            try
            {
                element.Patterns.Value.Pattern.SetValue(text);
                return;
            }
            catch (Exception ex)
            {
                LoggingService.LogWarn($"ValuePattern으로 텍스트 설정 실패: {ex.Message} - 키보드 입력으로 대체합니다.");
            }
        }

        // 일반적인 텍스트 입력 처리
        element.Focus();

        // 기존 텍스트 선택 삭제 (더블 클릭으로 전체 선택 후 삭제)
        try
        {
            element.DoubleClick();
            await Task.Delay(100, ct);
            Keyboard.Press(VirtualKeyShort.DELETE);
            await Task.Delay(100, ct);
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"텍스트 선택 삭제 실패: {ex.Message} - 직접 입력 시도");
        }

        Keyboard.Type(text);
    }

    private async Task WaitForFolderDialogAsync(Application app, CancellationToken ct)
    {
        const int timeoutMs = 8000;
        const int pollIntervalMs = 200;

        var sw = Stopwatch.StartNew();
        var foregroundAttempted = false;

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var folderDialog = FindFolderDialogByProcess(app.ProcessId);
            AutomationElement[]? snapshotWindows = null;

            if (folderDialog == null)
            {
                snapshotWindows = CollectAllWindows(app);
                folderDialog = FindFolderDialogWindow(snapshotWindows);
            }

            if (folderDialog == null && !foregroundAttempted)
            {
                var remainingMs = Math.Max(0, timeoutMs - sw.ElapsedMilliseconds);
                var waitBudget = TimeSpan.FromMilliseconds(Math.Min(5000, remainingMs));

                if (waitBudget > TimeSpan.Zero)
                {
                    folderDialog = await WaitForegroundFolderDialogAsync(waitBudget, ct);
                }

                foregroundAttempted = true;
            }

            if (folderDialog == null)
            {
                folderDialog = TryFindFolderDialogViaWin32(app);
            }

            if (folderDialog != null)
            {
                LoggingService.LogInfo($"폴더 선택 대화상자 감지 완료 (Name: {GetSafeProperty(folderDialog, e => e.Name)}, ClassName: {GetSafeProperty(folderDialog, e => e.ClassName)})");
                TryBringWindowToFront(folderDialog);
                return;
            }

            if (sw.ElapsedMilliseconds >= timeoutMs)
            {
                LoggingService.LogWarn("폴더 선택 대화상자를 찾을 수 없습니다 (타임아웃).");
                snapshotWindows ??= CollectAllWindows(app);
                LogWindowSnapshot(snapshotWindows, "폴더 대화상자 탐색 타임아웃 시점의 Window 목록");

                throw new InvalidOperationException("폴더 선택 대화상자를 찾을 수 없습니다.");
            }

            await Task.Delay(pollIntervalMs, ct);
        }
    }

    private AutomationElement[] CollectAllWindows(Application app)
    {
        var appWindows = app.GetAllTopLevelWindows(_automation);
        var desktopWindows = _automation
            .GetDesktop()
            .FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Window));

        return MergeWindows(appWindows, desktopWindows);
    }

    private async Task<AutomationElement?> WaitForegroundFolderDialogAsync(TimeSpan timeout, CancellationToken ct)
    {
        if (timeout <= TimeSpan.Zero)
        {
            return null;
        }

        var deadline = DateTime.UtcNow.Add(timeout);
        var currentProcessId = (uint)Process.GetCurrentProcess().Id;
        var lastHandle = IntPtr.Zero;

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();

            var foreground = Win32.GetForegroundWindow();
            if (foreground != IntPtr.Zero && foreground != lastHandle)
            {
                lastHandle = foreground;
                Win32.GetWindowThreadProcessId(foreground, out var foregroundPid);

                if (foregroundPid != currentProcessId)
                {
                    foreach (var automation in _automationBackends)
                    {
                        try
                        {
                            var element = automation.FromHandle(foreground);
                            if (element != null && IsFolderPickerWindow(automation, element))
                            {
                                LoggingService.LogInfo($"포어그라운드 창 기반 폴더 대화상자 감지 (백엔드: {automation.GetType().Name}, hwnd: 0x{foreground.ToInt64():X})");
                                return element;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingService.LogWarn($"포어그라운드 창을 AutomationElement로 변환 실패 (백엔드 {automation.GetType().Name}): {ex.Message}");
                        }
                    }
                }
            }

            await Task.Delay(100, ct);
        }

        return null;
    }

    private AutomationElement? TryFindFolderDialogViaWin32(Application app)
    {
        try
        {
            var mainWindowHandle = IntPtr.Zero;

            try
            {
                var mainWindow = app.GetMainWindow(_automation);
                mainWindowHandle = mainWindow?.FrameworkAutomationElement.NativeWindowHandle ?? IntPtr.Zero;
            }
            catch (Exception ex)
            {
                LoggingService.LogWarn($"메인 창 핸들을 가져오는 중 오류 발생: {ex.Message}");
            }

            var candidateHandle = FindFolderDialogHandleViaWin32(mainWindowHandle);
            if (candidateHandle == IntPtr.Zero)
            {
                return null;
            }

            foreach (var automation in _automationBackends)
            {
                try
                {
                    var element = automation.FromHandle(candidateHandle);
                    if (element != null && IsFolderPickerWindow(automation, element))
                    {
                        LoggingService.LogInfo($"Win32 열거 기반 폴더 대화상자 감지 성공 (백엔드: {automation.GetType().Name}, hwnd: 0x{candidateHandle.ToInt64():X})");
                        return element;
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.LogWarn($"Win32 대화상자 AutomationElement 변환 실패 (백엔드 {automation.GetType().Name}): {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"Win32 기반 폴더 대화상자 탐색 중 오류: {ex.Message}");
        }

        return null;
    }

    private IntPtr FindFolderDialogHandleViaWin32(IntPtr mainWindowHandle)
    {
        var candidates = new List<(IntPtr Hwnd, int Score)>();
        var currentProcessId = (uint)Process.GetCurrentProcess().Id;

        Win32.EnumWindows((hwnd, lParam) =>
        {
            try
            {
                if (!Win32.IsWindowVisible(hwnd))
                {
                    return true;
                }

                Win32.GetWindowThreadProcessId(hwnd, out var pid);
                if (pid == currentProcessId)
                {
                    return true;
                }

                var title = GetWindowTextSafe(hwnd);
                if (string.IsNullOrWhiteSpace(title))
                {
                    return true;
                }

                var className = GetClassNameSafe(hwnd);
                var score = 0;

                var owner = Win32.GetWindow(hwnd, Win32.GW_OWNER);
                if (mainWindowHandle != IntPtr.Zero && owner == mainWindowHandle)
                {
                    score += 3;
                }

                var exStyle = Win32.GetWindowLong(hwnd, Win32.GWL_EXSTYLE);
                if ((exStyle & Win32.WS_EX_DLGMODALFRAME) != 0)
                {
                    score += 2;
                }

                if (!string.IsNullOrEmpty(className) &&
                    (className.Contains("CabinetWClass", StringComparison.OrdinalIgnoreCase) ||
                     className.Contains("#32770", StringComparison.OrdinalIgnoreCase) ||
                     className.Contains("XamlWindow", StringComparison.OrdinalIgnoreCase) ||
                     className.Contains("ApplicationFrameWindow", StringComparison.OrdinalIgnoreCase)))
                {
                    score += 2;
                }

                if (ContainsKeyword(title, FolderDialogTitleKeywords))
                {
                    score += 2;
                }

                if (score >= 3)
                {
                    candidates.Add((hwnd, score));
                    LoggingService.LogInfo($"Win32 후보 창 발견 (hwnd: 0x{hwnd.ToInt64():X}, title: '{title}', class: '{className}', score: {score})");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogWarn($"Win32 창 평가 중 오류: {ex.Message}");
            }

            return true;
        }, IntPtr.Zero);

        if (candidates.Count == 0)
        {
            return IntPtr.Zero;
        }

        var best = candidates
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.Hwnd.ToInt64())
            .First();

        LoggingService.LogInfo($"Win32 후보 선정: hwnd=0x{best.Hwnd.ToInt64():X}, score={best.Score}");
        return best.Hwnd;
    }

    private static string GetWindowTextSafe(IntPtr hwnd)
    {
        var sb = new StringBuilder(256);
        _ = Win32.GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static string GetClassNameSafe(IntPtr hwnd)
    {
        var sb = new StringBuilder(256);
        _ = Win32.GetClassName(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private void LogWindowSnapshot(IEnumerable<AutomationElement> windows, string header)
    {
        LoggingService.LogInfo(header);

        var windowsList = windows.ToList();
        LoggingService.LogInfo($"총 윈도우 개수: {windowsList.Count}");

        int windowIndex = 0;
        foreach (var window in windowsList)
        {
            windowIndex++;
            var name = GetSafeProperty(window, e => e.Name);
            var className = GetSafeProperty(window, e => e.ClassName);
            var automationId = GetSafeProperty(window, e => e.AutomationId);
            var controlType = window.ControlType.ToString();
            var isEnabled = window.IsEnabled ? "활성화" : "비활성화";
            var isVisible = GetSafeIsOffscreen(window) ? "화면 밖" : "화면 내";

            var info = $"  [{windowIndex}] Window: Name='{name}', ClassName='{className}', AutomationId='{automationId}', ControlType='{controlType}', {isEnabled}, {isVisible}";
            LoggingService.LogInfo(info);

            // 특히 중요한 윈도우에 대한 추가 정보
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (name.Contains("폴더") || name.Contains("Folder"))
                {
                    LoggingService.LogInfo($"      → 폴더 관련 윈도우 발견!");
                }
                if (name.Contains("연결전송된 촬영 설정"))
                {
                    LoggingService.LogInfo($"      → 연결전송된 촬영 설정 윈도우 발견!");
                }
                if (name.Contains("내보내기"))
                {
                    LoggingService.LogInfo($"      → 내보내기 관련 윈도우 발견!");
                }
            }
        }
    }

    /// <summary>
    /// UiaSelector로 UI 요소를 찾습니다.
    /// MenuItem을 찾을 때는 먼저 컨텍스트 메뉴를 찾은 후 그 안에서 검색합니다.
    /// </summary>
    private AutomationElement? FindElement(AutomationElement parent, UiaSelector selector)
    {
        try
        {
            // MenuItem을 찾는 경우, 특별한 검색 로직 사용
            if (!string.IsNullOrWhiteSpace(selector.ControlType) &&
                selector.ControlType.Equals("MenuItem", StringComparison.OrdinalIgnoreCase))
            {
                LoggingService.LogInfo($"MenuItem 검색 시작 - AutomationId: {selector.AutomationId}, Name: {selector.Name}");

                // 1. 먼저 모든 Menu 요소를 찾아서 로깅
                var allMenus = parent.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu));
                if (allMenus != null && allMenus.Length > 0)
                {
                    LoggingService.LogInfo($"발견된 Menu 요소: {allMenus.Length}개");
                    foreach (var menu in allMenus)
                    {
                        var menuInfo = $"  - Menu: Name='{GetSafeProperty(menu, e => e.Name)}', ClassName='{GetSafeProperty(menu, e => e.ClassName)}', AutomationId='{GetSafeProperty(menu, e => e.AutomationId)}'";
                        LoggingService.LogInfo(menuInfo);
                    }
                }
                else
                {
                    LoggingService.LogWarn("Menu 요소를 찾을 수 없습니다.");
                }

                // 2. 모든 MenuItem을 찾아서 로깅
                var allMenuItems = parent.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem));
                if (allMenuItems != null && allMenuItems.Length > 0)
                {
                    LoggingService.LogInfo($"발견된 MenuItem 요소: {allMenuItems.Length}개");
                    foreach (var item in allMenuItems)
                    {
                        var itemInfo = $"  - MenuItem: Name='{GetSafeProperty(item, e => e.Name)}', AutomationId='{GetSafeProperty(item, e => e.AutomationId)}', ClassName='{GetSafeProperty(item, e => e.ClassName)}'";
                        LoggingService.LogInfo(itemInfo);
                    }

                    // 3. AutomationId로 먼저 검색 시도
                    if (!string.IsNullOrWhiteSpace(selector.AutomationId))
                    {
                        var matchByAutomationId = allMenuItems.FirstOrDefault(item =>
                        {
                            var itemAutomationId = GetSafeProperty(item, e => e.AutomationId);
                            return itemAutomationId == selector.AutomationId;
                        });

                        if (matchByAutomationId != null)
                        {
                            LoggingService.LogInfo($"AutomationId로 MenuItem 발견: {selector.AutomationId}");
                            return matchByAutomationId;
                        }
                    }

                    // 4. Name으로 검색 시도
                    if (!string.IsNullOrWhiteSpace(selector.Name))
                    {
                        var matchByName = allMenuItems.FirstOrDefault(item =>
                        {
                            var itemName = GetSafeProperty(item, e => e.Name);
                            return itemName != null && itemName.Equals(selector.Name, StringComparison.OrdinalIgnoreCase);
                        });

                        if (matchByName != null)
                        {
                            LoggingService.LogInfo($"Name으로 MenuItem 발견: {selector.Name}");
                            return matchByName;
                        }

                        // 부분 일치 시도
                        var matchByPartialName = allMenuItems.FirstOrDefault(item =>
                        {
                            var itemName = GetSafeProperty(item, e => e.Name);
                            return itemName != null && itemName.Contains(selector.Name, StringComparison.OrdinalIgnoreCase);
                        });

                        if (matchByPartialName != null)
                        {
                            LoggingService.LogInfo($"Name (부분 일치)으로 MenuItem 발견: {selector.Name}");
                            return matchByPartialName;
                        }
                    }

                    LoggingService.LogWarn($"조건에 맞는 MenuItem을 찾을 수 없음 (AutomationId: {selector.AutomationId}, Name: {selector.Name})");
                }
                else
                {
                    LoggingService.LogWarn("MenuItem 요소를 하나도 찾을 수 없습니다.");
                }

                // 5. 마지막으로 기존 방식으로 시도
                LoggingService.LogInfo("기존 검색 방식으로 재시도...");
                return FindElementWithConditions(parent, selector);
            }

            // 일반적인 검색 (MenuItem이 아닌 경우)
            return FindElementWithConditions(parent, selector);
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"UI 요소 탐색 중 오류: {ex.Message}", ex);
            return null;
        }
    }

    /// <summary>
    /// UI 요소의 속성을 안전하게 가져옵니다. 속성을 지원하지 않는 경우 "N/A"를 반환합니다.
    /// </summary>
    private string GetSafeProperty(AutomationElement element, Func<AutomationElement, string> propertyGetter)
    {
        try
        {
            return propertyGetter(element) ?? "null";
        }
        catch (FlaUI.Core.Exceptions.PropertyNotSupportedException)
        {
            return "N/A";
        }
        catch (Exception)
        {
            return "Error";
        }
    }

    /// <summary>
    /// UI 요소의 IsOffscreen 속성을 안전하게 가져옵니다. 속성을 지원하지 않는 경우 false를 반환합니다.
    /// </summary>
    private bool GetSafeIsOffscreen(AutomationElement element)
    {
        try
        {
            return element.IsOffscreen;
        }
        catch (FlaUI.Core.Exceptions.PropertyNotSupportedException)
        {
            return false; // 기본값으로 화면 내로 가정
        }
        catch (Exception)
        {
            return false; // 오류 시 기본값으로 화면 내로 가정
        }
    }

    /// <summary>
    /// UiaSelector의 조건을 기반으로 요소를 찾습니다.
    /// </summary>
    private AutomationElement? FindElementWithConditions(AutomationElement parent, UiaSelector selector)
    {
        var conditions = new List<FlaUI.Core.Conditions.ConditionBase>();

        if (!string.IsNullOrWhiteSpace(selector.AutomationId))
        {
            conditions.Add(_automation.ConditionFactory.ByAutomationId(selector.AutomationId));
        }

        if (!string.IsNullOrWhiteSpace(selector.Name))
        {
            conditions.Add(_automation.ConditionFactory.ByName(selector.Name));
        }

        if (!string.IsNullOrWhiteSpace(selector.ClassName))
        {
            conditions.Add(_automation.ConditionFactory.ByClassName(selector.ClassName));
        }

        if (!string.IsNullOrWhiteSpace(selector.ControlType))
        {
            // ControlType 문자열을 FlaUI ControlType으로 변환
            var controlType = selector.ControlType.ToLowerInvariant() switch
            {
                "button" => FlaUI.Core.Definitions.ControlType.Button,
                "edit" => FlaUI.Core.Definitions.ControlType.Edit,
                "text" => FlaUI.Core.Definitions.ControlType.Text,
                "static" => FlaUI.Core.Definitions.ControlType.Text,
                "window" => FlaUI.Core.Definitions.ControlType.Window,
                "pane" => FlaUI.Core.Definitions.ControlType.Pane,
                "menuitem" => FlaUI.Core.Definitions.ControlType.MenuItem,
                "menu" => FlaUI.Core.Definitions.ControlType.Menu,
                "combobox" => FlaUI.Core.Definitions.ControlType.ComboBox,
                "checkbox" => FlaUI.Core.Definitions.ControlType.CheckBox,
                "toolbar" => FlaUI.Core.Definitions.ControlType.ToolBar,
                "slider" => FlaUI.Core.Definitions.ControlType.Slider,
                _ => throw new NotSupportedException($"지원하지 않는 ControlType: {selector.ControlType}")
            };
            conditions.Add(_automation.ConditionFactory.ByControlType(controlType));
        }

        if (conditions.Count == 0)
        {
            throw new InvalidOperationException("선택자에 조건이 하나도 없습니다.");
        }

        var condition = conditions.Count == 1
            ? conditions[0]
            : conditions.Aggregate((a, b) => a.And(b));

        return parent.FindFirstDescendant(condition);
    }

    /// <summary>
    /// UI 요소에 액션을 수행합니다.
    /// </summary>
    private async Task PerformActionAsync(
        Application app,
        AutomationElement? element,
        AutomationStep step,
        Dictionary<string, string> inputValues,
        CancellationToken ct)
    {
        switch (step.Action.ToLowerInvariant())
        {
            case "click":
                if (element == null) throw new InvalidOperationException($"Click 액션에 UI 요소가 필요합니다: {step.Description}");

                // MenuItem에 대한 특별 처리 - 직접 클릭만 사용
                if (element.ControlType == FlaUI.Core.Definitions.ControlType.MenuItem)
                {
                    LoggingService.LogInfo($"MenuItem 클릭 시도: {step.Description}");

                    // 모든 MenuItem은 직접 클릭으로 처리
                    element.Click();
                }
                // ComboBox에 대한 특별 처리
                else if (element.ControlType == FlaUI.Core.Definitions.ControlType.ComboBox)
                {
                    LoggingService.LogInfo($"ComboBox 클릭 시도: {step.Description}");

                    // 1. 기본 클릭 시도
                    element.Click();

                    // 2. ComboBox 확장 확인 (다양한 방법으로 시도)
                    await Task.Delay(150, ct);  // 더 빠른 체크를 위해 대기 시간 단축
                    bool isExpanded = false;

                    // 방법 1: ExpandCollapsePattern 사용
                    var comboBoxElement = element.AsComboBox();
                    if (comboBoxElement?.Patterns.ExpandCollapse.IsSupported == true)
                    {
                        isExpanded = comboBoxElement.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value == FlaUI.Core.Definitions.ExpandCollapseState.Expanded;
                        LoggingService.LogInfo($"ExpandCollapsePattern으로 ComboBox 확장 상태 확인: {isExpanded}");
                    }

                    // 방법 2: 드롭다운 메뉴 직접 검색 (사용자 제공 inspection 데이터 기반)
                    if (!isExpanded)
                    {
                        isExpanded = IsComboBoxDropdownOpen();
                        LoggingService.LogInfo($"드롭다운 메뉴 검색으로 ComboBox 확장 상태 확인: {isExpanded}");
                    }

                    // 방법 3: ComboBox 항목 개수 확인
                    if (!isExpanded && comboBoxElement != null)
                    {
                        try
                        {
                            var items = comboBoxElement.Items;
                            if (items != null && items.Length > 0)
                            {
                                var firstItem = items[0];
                                try
                                {
                                    // 간단하게 항목 존재 여부만으로 확장 상태 확인
                                    isExpanded = true;
                                    LoggingService.LogInfo($"ComboBox 항목 존재로 확장 상태 확인: {isExpanded}");
                                }
                                catch (Exception ex)
                                {
                                    LoggingService.LogWarn($"ComboBox 항목 확인 중 오류: {ex.Message}");
                                    isExpanded = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingService.LogWarn($"ComboBox 항목 확인 중 오류: {ex.Message}");
                        }
                    }

                    // 3. 아직 확장되지 않았다면 추가 시도
                    if (!isExpanded)
                    {
                        LoggingService.LogInfo("ComboBox이 확장되지 않아 추가 시도합니다.");

                        // ComboBox에 다시 포커스 설정
                        element.Focus();
                        await Task.Delay(100, ct);

                        // 다양한 확장 방법 시도
                        try
                        {
                            // 시도 1: 다시 클릭
                            element.Click();
                            await Task.Delay(300, ct);

                            isExpanded = IsComboBoxDropdownOpen();
                            LoggingService.LogInfo($"재클릭 후 확장 상태 재확인: {isExpanded}");

                            if (!isExpanded)
                            {
                                // 시도 2: 포커스 설정 후 다시 클릭
                                element.Focus();
                                await Task.Delay(100, ct);
                                element.Click();
                                await Task.Delay(300, ct);

                                isExpanded = IsComboBoxDropdownOpen();
                                LoggingService.LogInfo($"포커스 후 재클릭 확장 상태 재확인: {isExpanded}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingService.LogWarn($"ComboBox 확장 시도 중 오류: {ex.Message}");
                        }
                    }

                    LoggingService.LogInfo($"ComboBox 최종 확장 상태: {(isExpanded ? "확장됨" : "확장되지 않음")}");
                }
                else
                {
                    element.Click();
                }

                LoggingService.LogInfo($"클릭 완료: {step.Description}");
                break;

            case "settext":
                if (element == null) throw new InvalidOperationException($"SetText 액션에 UI 요소가 필요합니다: {step.Description}");
                if (string.IsNullOrWhiteSpace(step.ActionData))
                {
                    throw new InvalidOperationException($"SetText 액션에 ActionData가 없습니다: {step.Description}");
                }

                // ActionData를 inputValues에서 치환
                var text = ResolveText(step.ActionData, inputValues);
                element.AsTextBox().Text = text;
                LoggingService.LogInfo($"텍스트 입력 완료: {step.Description} = \"{text}\"");
                break;

            case "sendkeys":
                if (element == null) throw new InvalidOperationException($"SendKeys 액션에 UI 요소가 필요합니다: {step.Description}");
                if (string.IsNullOrWhiteSpace(step.ActionData))
                {
                    throw new InvalidOperationException($"SendKeys 액션에 ActionData가 없습니다: {step.Description}");
                }

                element.Focus();
                var keys = ResolveText(step.ActionData, inputValues);

                // 특수 키 처리
                if (keys.Equals("Delete", StringComparison.OrdinalIgnoreCase))
                {
                    Keyboard.Press(VirtualKeyShort.DELETE);
                }
                else
                {
                    // 주소창에 경로 붙여넣기 처리 (15단계)
                    if (step.Description.Contains("주소창 클릭하여 경로 붙여넣기"))
                    {
                        await ClearAndTypeAsync(element, keys, ct);
                    }
                    else
                    {
                        Keyboard.Type(keys);
                    }
                }

                LoggingService.LogInfo($"키보드 입력 완료: {step.Description} = \"{keys}\"");
                break;

            case "clearandtype":
                if (element == null) throw new InvalidOperationException($"ClearAndType 액션에 UI 요소가 필요합니다: {step.Description}");
                if (string.IsNullOrWhiteSpace(step.ActionData))
                {
                    throw new InvalidOperationException($"ClearAndType 액션에 ActionData가 없습니다: {step.Description}");
                }

                var textToType = ResolveText(step.ActionData, inputValues);
                await ClearAndTypeAsync(element, textToType, ct);
                LoggingService.LogInfo($"텍스트 클리어 후 입력 완료: {step.Description} = \"{textToType}\"");
                break;

            case "togglecheckbox":
                if (element == null) throw new InvalidOperationException($"ToggleCheckBox 액션에 UI 요소가 필요합니다: {step.Description}");
                if (string.IsNullOrWhiteSpace(step.ActionData))
                {
                    throw new InvalidOperationException($"ToggleCheckBox 액션에 ActionData가 없습니다: {step.Description}");
                }

                var targetState = step.ActionData.ToLowerInvariant();
                var checkbox = element.AsCheckBox();
                var currentState = checkbox.IsChecked ?? false;

                bool shouldBeChecked = targetState == "on" || targetState == "true" || targetState == "checked";

                if ((shouldBeChecked && !currentState) || (!shouldBeChecked && currentState))
                {
                    checkbox.Toggle();
                    LoggingService.LogInfo($"체크박스 상태 변경: {step.Description} → {(shouldBeChecked ? "ON" : "OFF")}");
                }
                else
                {
                    LoggingService.LogInfo($"체크박스 상태 유지: {step.Description} = {(currentState ? "ON" : "OFF")}");
                }
                break;

            case "selectcomboboxitem":
                if (element == null) throw new InvalidOperationException($"SelectComboBoxItem 액션에 UI 요소가 필요합니다: {step.Description}");
                if (string.IsNullOrWhiteSpace(step.ActionData))
                {
                    throw new InvalidOperationException($"SelectComboBoxItem 액션에 ActionData가 없습니다: {step.Description}");
                }

                var comboBox = element.AsComboBox();
                var itemToSelect = ResolveText(step.ActionData, inputValues);

                // 콤보박스 확장
                comboBox.Expand();
                await Task.Delay(300, ct);

                // 항목 선택 (Name으로)
                var item = comboBox.Items.FirstOrDefault(i =>
                    i.Name != null && i.Name.Contains(itemToSelect));

                if (item != null)
                {
                    item.Select();
                    LoggingService.LogInfo($"콤보박스 항목 선택 완료: {step.Description} = \"{itemToSelect}\"");
                }
                else
                {
                    throw new InvalidOperationException($"콤보박스 항목을 찾을 수 없음: {itemToSelect}");
                }
                break;

            case "wait":
                var delayMs = int.TryParse(step.ActionData, out var ms) ? ms : 1000;
                await Task.Delay(delayMs, ct);
                LoggingService.LogInfo($"대기 완료: {delayMs}ms");
                break;

            case "waitforelement":
                if (element == null) throw new InvalidOperationException($"WaitForElement 액션에 UI 요소가 필요합니다: {step.Description}");

                var timeoutMs = int.TryParse(step.ActionData, out var waitMs) ? waitMs : 5000;
                var startWaitTime = Stopwatch.StartNew();

                while (startWaitTime.ElapsedMilliseconds < timeoutMs)
                {
                    ct.ThrowIfCancellationRequested();

                    // 현재 활성 윈도우에서 요소 다시 검색
                    var activeWindow = GetCurrentActiveWindow(app, step);
                    var foundElement = FindElement(activeWindow, step.Selector);

                    if (foundElement != null)
                    {
                        LoggingService.LogInfo($"요소 대기 완료: {step.Description} (소요 시간: {startWaitTime.ElapsedMilliseconds}ms)");
                        return;
                    }

                    await Task.Delay(200, ct);
                }

                throw new InvalidOperationException($"요소 대기 타임아웃: {step.Description} (타임아웃: {timeoutMs}ms)");

            case "waitforfolderdialog":
                await WaitForFolderDialogAsync(app, ct);
                break;

            case "checkcameraerrors":
                await CheckForCameraErrorsAsync(ct);
                break;

            case "handleoverwritewarning":
                await HandleOverwriteWarningAsync(ct);
                break;

            case "findfolderpickerandtypepath":
                if (string.IsNullOrWhiteSpace(step.ActionData))
                {
                    throw new InvalidOperationException($"FindFolderPickerAndTypePath 액션에 ActionData가 없습니다: {step.Description}");
                }

                var targetPath = ResolveText(step.ActionData, inputValues);
                await FindFolderPickerAndTypePathAsync(targetPath, ct);
                LoggingService.LogInfo($"폴더 선택창 경로 입력 완료: {step.Description} = \"{targetPath}\"");
                break;

            case "findrelativeelement":
                if (element == null) throw new InvalidOperationException($"FindRelativeElement 액션에 기준 요소가 필요합니다: {step.Description}");

                // ActionData에서 targetSelector 정보 파싱 (AutomationId:Name:ControlType 형식)
                var parts = (step.ActionData ?? "").Split(':');
                if (parts.Length < 3)
                {
                    throw new InvalidOperationException($"FindRelativeElement 액션의 ActionData 형식이 잘못되었습니다: {step.ActionData}");
                }

                var targetSelector = new UiaSelector
                {
                    AutomationId = parts[0],
                    Name = parts[1],
                    ControlType = parts[2]
                };

                var relativeElement = FindElementRelativeTo(element, targetSelector);
                if (relativeElement == null)
                {
                    throw new InvalidOperationException($"상대 위치 요소를 찾을 수 없습니다: {step.Description}");
                }

                LogElementInfo(relativeElement, step.Description);
                break;

            case "setcameraparameter":
                if (element == null) throw new InvalidOperationException($"SetCameraParameter 액션에 기준 요소가 필요합니다: {step.Description}");

                // ActionData 형식: "settingType:configKey" (예: "shutter:Shutter")
                var paramParts = (step.ActionData ?? "").Split(':');
                if (paramParts.Length != 2)
                {
                    throw new InvalidOperationException($"SetCameraParameter 액션의 ActionData 형식이 잘못되었습니다: {step.ActionData}");
                }

                var settingType = paramParts[0];
                var configKey = paramParts[1];

                // config에서 값 가져오기
                var config = _configService.Current.Camera;
                var configValue = config?.GetType().GetProperty(configKey)?.GetValue(config)?.ToString();

                if (string.IsNullOrEmpty(configValue))
                {
                    LoggingService.LogWarn($"카메라 설정값을 찾을 수 없습니다: {configKey}");
                    break;
                }

                var automationId = GetCameraSettingAutomationId(settingType, configValue);

                // 기준 텍스트 라벨 찾기 (셔터:, 조리개:, ISO:, WB:)
                var labelElement = element;

                // 상대 위치로 ComboBox 찾기
                var comboBoxSelector = new UiaSelector
                {
                    AutomationId = "65535",
                    ControlType = "ComboBox"
                };

                var settingComboBox = FindElementRelativeTo(labelElement, comboBoxSelector);
                if (settingComboBox == null)
                {
                    throw new InvalidOperationException($"{settingType} ComboBox를 찾을 수 없습니다.");
                }

                // ComboBox 클릭
                settingComboBox.Click();
                await Task.Delay(300, ct);

                // 메뉴에서 해당 항목 클릭
                var currentWindow = GetCurrentActiveWindow(app);
                var menuItemSelector = new UiaSelector
                {
                    AutomationId = automationId.ToString(),
                    ControlType = "MenuItem"
                };

                var menuItem = FindElement(currentWindow, menuItemSelector);
                if (menuItem != null)
                {
                    menuItem.Click();
                    LoggingService.LogInfo($"{settingType} 설정 완료: {configValue} (AutomationId: {automationId})");
                }
                else
                {
                    throw new InvalidOperationException($"{settingType} 메뉴 항목을 찾을 수 없습니다: {configValue} (AutomationId: {automationId})");
                }

                await Task.Delay(500, ct);
                break;

            default:
                throw new NotSupportedException($"지원하지 않는 액션: {step.Action}");
        }
    }

    /// <summary>
    /// UI 요소의 상세 정보를 로깅합니다.
    /// </summary>
    private void LogElementInfo(AutomationElement element, string actionDescription)
    {
        var automationId = GetSafeProperty(element, e => e.AutomationId);
        var name = GetSafeProperty(element, e => e.Name);
        var className = GetSafeProperty(element, e => e.ClassName);
        var controlType = element.ControlType.ToString();

        LoggingService.LogInfo($"UI 요소 정보 - {actionDescription}:");
        LoggingService.LogInfo($"  AutomationId: {automationId}");
        LoggingService.LogInfo($"  Name: {name}");
        LoggingService.LogInfo($"  ClassName: {className}");
        LoggingService.LogInfo($"  ControlType: {controlType}");
    }

    /// <summary>
    /// 다른 요소를 기준으로 상대 위치에 있는 요소를 찾습니다.
    /// FlaUIInspectData.md의 "위 객체와 상대 위치 기반 탐색 설정" 구현
    /// </summary>
    private AutomationElement? FindElementRelativeTo(AutomationElement referenceElement, UiaSelector targetSelector)
    {
        try
        {
            // 기준 요소의 부모를 찾음
            var parent = referenceElement.Parent;
            if (parent == null)
            {
                LoggingService.LogWarn("기준 요소의 부모를 찾을 수 없습니다.");
                return null;
            }

            // 부모 내에서 대상 요소 찾기
            return FindElementWithConditions(parent, targetSelector);
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"상대 위치 요소 탐색 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 카메라 관련 오류 케이스를 확인합니다.
    /// FlaUIInspectData.md의 오류 케이스 처리 구현
    /// </summary>
    private async Task CheckForCameraErrorsAsync(CancellationToken ct)
    {
        const int checkDurationMs = 2000; // 2초 동안 확인
        const int checkIntervalMs = 200;  // 200ms 간격으로 확인
        var startTime = Stopwatch.StartNew();

        while (startTime.ElapsedMilliseconds < checkDurationMs)
        {
            ct.ThrowIfCancellationRequested();

            var desktop = _automation.GetDesktop();
            var cf = _automation.ConditionFactory;

            // Case01: "카메라를 감지하는 중..." 확인
            var detectingText = desktop.FindFirstDescendant(
                cf.ByAutomationId("-1985744256")
                    .And(cf.ByName("카메라를 감지하는 중..."))
                    .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

            if (detectingText != null)
            {
                throw new InvalidOperationException("카메라 감지 중 오류 발생: '카메라를 감지하는 중...' 상태가 지속됩니다.");
            }

            // Case02: "카메라가 검색되지 않음" 확인
            var connectionPane = desktop.FindFirstDescendant(
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Pane)
                    .And(cf.ByName("연결전송된 촬영")));

            if (connectionPane != null)
            {
                var notFoundCombo = connectionPane.FindFirstDescendant(
                    cf.ByAutomationId("65535")
                        .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ComboBox)));

                if (notFoundCombo != null)
                {
                    var valuePattern = notFoundCombo.Patterns.Value?.Pattern;
                    var value = valuePattern?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(value) && value.Contains("카메라가 검색되지 않음"))
                    {
                        throw new InvalidOperationException("카메라 연결 오류: '카메라가 검색되지 않음'");
                    }
                }
            }

            await Task.Delay(checkIntervalMs, ct);
        }

        LoggingService.LogInfo("카메라 오류 감지 완료 - 문제 없음");
    }

    /// <summary>
    /// 최신 윈도우 폴더 선택창을 찾고 Ctrl+L 단축키로 경로를 입력합니다.
    /// 지침 1) 프로세스 한정 금지 + 모던/고전 창 동시 탐색
    /// 지침 2) Ctrl+L 주소창 단축키로 경로 입력
    /// 지침 3) 권한 정합 + 타이밍 보강
    /// </summary>
    private async Task FindFolderPickerAndTypePathAsync(string targetPath, CancellationToken ct)
    {
        LoggingService.LogInfo($"폴더 선택창 찾기 시작 (전역 탐색 + 최신 윈도우 지원): {targetPath}");

        // 지침 3: 버튼 클릭 후 최소 300~500ms 지연 후 탐색 시작
        await Task.Delay(400, ct);

        // 지침 1: 전역 탐색 - Desktop 루트에서 창 검색 (PID 필터 제거)
        var folderPicker = await FindFolderPickerGlobalAsync(ct);

        if (folderPicker == null)
        {
            // 디버깅용 전체 윈도우 덤프
            DumpTopWindows();
            throw new TimeoutException("폴더 선택창을 8초 동안 찾지 못했습니다 (CabinetWClass, ExplorerFrame, XamlWindow, #32770 모두 탐색)");
        }

        // 지침 2: Ctrl+L 단축키로 경로 입력
        await TypePathIntoFolderPickerAsync(folderPicker, targetPath, ct);

        LoggingService.LogInfo($"폴더 선택창 경로 입력 성공: {targetPath}");
    }

    /// <summary>
    /// 전역에서 폴더 선택창을 찾습니다 (PID 필터 없음).
    /// ClassName 후보: CabinetWClass, ExplorerFrame, XamlWindow, #32770
    /// 최소 8초 간 Retry(200ms 간격)로 폴링
    /// </summary>
    private async Task<AutomationElement?> FindFolderPickerGlobalAsync(CancellationToken ct)
    {
        var totalTimeout = TimeSpan.FromSeconds(8);
        var overallTimer = Stopwatch.StartNew();

        foreach (var automation in _automationBackends)
        {
            var remaining = totalTimeout - overallTimer.Elapsed;
            if (remaining <= TimeSpan.Zero)
            {
                LoggingService.LogWarn("폴더 선택창 탐색 타임아웃 - 잔여 시간 없음");
                break;
            }

            var result = await FindFolderPickerGlobalAsync(automation, remaining, ct);
            if (result != null)
            {
                if (!ReferenceEquals(automation, _automation))
                {
                    LoggingService.LogInfo($"UIA 폴백 백엔드({automation.GetType().Name})에서 폴더 선택창을 찾았습니다.");
                }

                return result;
            }
        }

        return null;
    }

    private async Task<AutomationElement?> FindFolderPickerGlobalAsync(AutomationBase automation, TimeSpan timeout, CancellationToken ct)
    {
        var pollInterval = TimeSpan.FromMilliseconds(200);
        var startTime = Stopwatch.StartNew();

        var desktop = automation.GetDesktop();
        var cf = automation.ConditionFactory;

        // 지침 1: 다양한 ClassName 지원 (모던/고전 창 동시 탐색)
        var folderPickerCondition = cf.ByControlType(FlaUI.Core.Definitions.ControlType.Window)
            .And(cf.ByClassName("CabinetWClass")
              .Or(cf.ByClassName("ExplorerFrame"))
              .Or(cf.ByClassName("XamlWindow"))
              .Or(cf.ByClassName("#32770")));

        LoggingService.LogInfo($"전역 폴더 선택창 탐색 시작 (백엔드: {automation.GetType().Name}, 타임아웃: {timeout.TotalMilliseconds}ms, 200ms 간격 폴링)");

        while (startTime.Elapsed < timeout)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var allWindows = desktop.FindAllChildren(folderPickerCondition);

                foreach (var window in allWindows)
                {
                    if (IsFolderPickerWindow(automation, window))
                    {
                        LoggingService.LogInfo($"폴더 선택창 발견 (전역 탐색, 백엔드 {automation.GetType().Name}): Name='{GetSafeProperty(window, e => e.Name)}', ClassName='{GetSafeProperty(window, e => e.ClassName)}'");

                        // 지침 1: 찾은 창을 Focus() 할 수 있어야 함
                        TryBringWindowToFront(window);
                        return window;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogWarn($"폴더 선택창 탐색 중 오류(백엔드 {automation.GetType().Name}): {ex.Message}");
            }

            await Task.Delay(pollInterval, ct);
        }

        LoggingService.LogWarn($"폴더 선택창 탐색 타임아웃 (백엔드 {automation.GetType().Name}, 타임아웃 {timeout.TotalMilliseconds}ms)");
        return null;
    }

    /// <summary>
    /// 창이 폴더 선택창인지 확인합니다.
    /// </summary>
    private bool IsFolderPickerWindow(AutomationBase automation, AutomationElement window)
    {
        try
        {
            var name = GetSafeProperty(window, e => e.Name);
            var className = GetSafeProperty(window, e => e.ClassName);

            // 기존 고전 폴더 선택창 (#32770)
            if (className.Equals("#32770", StringComparison.OrdinalIgnoreCase))
            {
                // Name에 "폴더" 또는 "Folder"가 포함되어 있거나,
                // 자식 요소에 확인/선택 버튼이 있는지 확인
                if (name.Contains("폴더") || name.Contains("Folder") || name.Contains("Browse"))
                {
                    return true;
                }

                // 확인 버튼 확인
                var cf = automation.ConditionFactory;
                var confirmButton = window.FindFirstDescendant(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                    .And(cf.ByName("확인").Or(cf.ByName("선택")).Or(cf.ByName("OK")).Or(cf.ByName("Select"))));

                if (confirmButton != null)
                {
                    return true;
                }
            }

            // 최신 파일 탐색기 기반 폴더 선택창 (CabinetWClass, ExplorerFrame, XamlWindow)
            if (className.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase) ||
                className.Equals("ExplorerFrame", StringComparison.OrdinalIgnoreCase) ||
                className.Equals("XamlWindow", StringComparison.OrdinalIgnoreCase))
            {
                // 주소창이나 탐색 컨트롤이 있는지 확인
                var cf = automation.ConditionFactory;

                // 주소창 확인 (다양한 AutomationId 지원)
                var addressBar = window.FindFirstDescendant(cf.ByAutomationId("1001"))
                    ?? window.FindFirstDescendant(cf.ByAutomationId("41477"))
                    ?? window.FindFirstDescendant(cf.ByAutomationId("1148"));

                if (addressBar != null)
                {
                    return true;
                }

                // 트리뷰나 목록 컨트롤 확인
                var treeView = window.FindFirstDescendant(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tree))
                    ?? window.FindFirstDescendant(cf.ByControlType(FlaUI.Core.Definitions.ControlType.List));

                if (treeView != null)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"폴더 선택창 확인 중 오류: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 폴더 선택창에 Ctrl+L로 경로를 입력합니다.
    /// 지침 2: 요소찾기 대신 Ctrl+L 주소창 단축키로 경로 입력
    /// </summary>
    private async Task TypePathIntoFolderPickerAsync(AutomationElement folderPicker, string path, CancellationToken ct)
    {
        LoggingService.LogInfo($"폴더 선택창에 경로 입력 시작 (Ctrl+L 방식): {path}");

        try
        {
            // 지침 2: 창을 찾은 직후 Ctrl+L 전송으로 주소 입력 모드 진입
            folderPicker.Focus();
            await Task.Delay(100, ct);

            // Ctrl+L 단축키로 주소창 활성화
            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_L);
            Keyboard.Release(VirtualKeyShort.KEY_L);
            Keyboard.Release(VirtualKeyShort.CONTROL);
            await Task.Delay(200, ct);

            // 지침 2: 지정 경로 입력 후 Enter
            Keyboard.Type(path);
            await Task.Delay(100, ct);
            Keyboard.Press(VirtualKeyShort.RETURN);
            Keyboard.Release(VirtualKeyShort.RETURN);
            await Task.Delay(300, ct);

            // 지침 2: 필요 시 '확인/선택/Open/Select' 버튼 눌러 닫기(있을 때만)
            var automation = folderPicker.Automation ?? _automation;
            var cf = automation.ConditionFactory;
            var okButton = folderPicker.FindFirstDescendant(
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                    .And(cf.ByName("확인")
                        .Or(cf.ByName("선택"))
                        .Or(cf.ByName("Open"))
                        .Or(cf.ByName("Select"))
                        .Or(cf.ByName("OK"))));

            if (okButton != null)
            {
                LoggingService.LogInfo("확인/선택 버튼 클릭");
                okButton.AsButton().Invoke();
                await Task.Delay(300, ct);
            }
            else
            {
                LoggingService.LogInfo("확인 버튼 없음 - Enter만으로 폴더 선택 완료");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"폴더 선택창 경로 입력 실패: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 디버깅용 전체 윈도우 덤프 (지침 3)
    /// </summary>
    private void DumpTopWindows()
    {
        try
        {
            var desktop = _automation.GetDesktop();
            var cf = _automation.ConditionFactory;

            var allWindows = desktop.FindAllChildren(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Window));

            LoggingService.LogInfo($"=== 디버깅: 전체 윈도우 덤프 (총 {allWindows.Length}개) ===");

            foreach (var window in allWindows)
            {
                var name = GetSafeProperty(window, e => e.Name);
                var className = GetSafeProperty(window, e => e.ClassName);
                var isEnabled = window.IsEnabled;
                var isVisible = !GetSafeIsOffscreen(window);

                var info = $"Window: Name='{name}', Class='{className}', Enabled={isEnabled}, Visible={isVisible}";
                LoggingService.LogInfo($"  {info}");

                // 폴더 선택창 후보들 강조 표시
                if (className.Equals("CabinetWClass") || className.Equals("ExplorerFrame") ||
                    className.Equals("XamlWindow") || className.Equals("#32770"))
                {
                    LoggingService.LogInfo($"    → 폴더 선택창 후보!");
                }
            }

            LoggingService.LogInfo("=== 윈도우 덤프 종료 ===");
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"윈도우 덤프 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 덮어쓰기 경고 대화상자를 확인하고 처리합니다.
    /// FlaUIInspectData.md의 덮어쓰기 경고 대응 구현
    /// </summary>
    private async Task HandleOverwriteWarningAsync(CancellationToken ct)
    {
        const int waitDurationMs = 2000; // 2초 동안 확인
        const int checkIntervalMs = 200;  // 200ms 간격으로 확인
        var startTime = Stopwatch.StartNew();

        while (startTime.ElapsedMilliseconds < waitDurationMs)
        {
            ct.ThrowIfCancellationRequested();

            var desktop = _automation.GetDesktop();
            var cf = _automation.ConditionFactory;

            // "다음 파일이 이미 존재합니다." 메시지 확인
            var warningText = desktop.FindFirstDescendant(
                cf.ByAutomationId("-2012510512")
                    .And(cf.ByName("다음 파일이 이미 존재합니다."))
                    .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

            if (warningText != null)
            {
                LoggingService.LogInfo("덮어쓰기 경고 대화상자 발견");

                // "덮어쓰기" 버튼 찾기
                var overwriteButton = desktop.FindFirstDescendant(
                    cf.ByAutomationId("65535")
                        .And(cf.ByName("덮어쓰기"))
                        .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

                if (overwriteButton != null)
                {
                    LoggingService.LogInfo("덮어쓰기 버튼 클릭");
                    overwriteButton.Click();
                    await Task.Delay(500, ct);
                    return;
                }
                else
                {
                    LoggingService.LogWarn("덮어쓰기 버튼을 찾을 수 없습니다.");
                }
            }

            await Task.Delay(checkIntervalMs, ct);
        }

        LoggingService.LogInfo("덮어쓰기 경고 없음 - 정상 진행");
    }

    /// <summary>
    /// config 값을 기반으로 카메라 설정 AutomationId를 가져옵니다.
    /// FlaUIInspectData.md의 순번자료 기반 매핑
    /// </summary>
    private int GetCameraSettingAutomationId(string settingType, string configValue)
    {
        return settingType.ToLowerInvariant() switch
        {
            "shutter" => ShutterMapping.TryGetValue(configValue, out var shutterId) ? shutterId : 23,
            "aperture" => ApertureMapping.TryGetValue(configValue, out var apertureId) ? apertureId : 6,
            "iso" => ISOMapping.TryGetValue(configValue, out var isoId) ? isoId : 1,
            "wb" or "whitebalance" => WhiteBalanceMapping.TryGetValue(configValue, out var wbId) ? wbId : 1,
            _ => throw new ArgumentException($"지원하지 않는 카메라 설정 타입: {settingType}")
        };
    }

    /// <summary>
    /// ActionData의 플레이스홀더를 inputValues로 치환합니다.
    /// 예: "{customerInput}" → inputValues["customerInput"]
    /// </summary>
    private string ResolveText(string template, Dictionary<string, string> inputValues)
    {
        var result = template;

        foreach (var kvp in inputValues)
        {
            var placeholder = $"{{{kvp.Key}}}";
            result = result.Replace(placeholder, kvp.Value);
        }

        return result;
    }

    private void LogAutomationEnvironment()
    {
        try
        {
            var backendOrder = string.Join(" → ", _automationBackends.Select(a => a.GetType().Name));
            var is64BitProcess = Environment.Is64BitProcess ? "x64" : "x86";
            var elevation = IsCurrentProcessElevated() ? "관리자" : "일반";
            var dpiAwareness = GetProcessDpiAwarenessDescription();

            LoggingService.LogInfo($"자동화 환경 확인 - 백엔드: {backendOrder}, 프로세스 비트수: {is64BitProcess}, DPI Awareness: {dpiAwareness}, 권한: {elevation}");
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn($"자동화 환경 정보를 기록하지 못했습니다: {ex.Message}");
        }
    }

    private static bool IsCurrentProcessElevated()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static string GetProcessDpiAwarenessDescription()
    {
        try
        {
            using var currentProcess = Process.GetCurrentProcess();
            var result = GetProcessDpiAwareness(currentProcess.Handle, out var awareness);

            if (result == 0)
            {
                return awareness switch
                {
                    ProcessDpiAwareness.ProcessDpiUnaware => "Unaware",
                    ProcessDpiAwareness.ProcessSystemDpiAware => "System",
                    ProcessDpiAwareness.ProcessPerMonitorDpiAware => "PerMonitor",
                    _ => awareness.ToString()
                };
            }

            return $"Unknown (HRESULT=0x{result:X8})";
        }
        catch (DllNotFoundException)
        {
            return "Unknown (Shcore.dll 없음)";
        }
        catch (Exception ex)
        {
            return $"Unknown ({ex.Message})";
        }
    }

    [DllImport("Shcore.dll")]
    private static extern int GetProcessDpiAwareness(IntPtr hprocess, out ProcessDpiAwareness awareness);

    private enum ProcessDpiAwareness
    {
        ProcessDpiUnaware = 0,
        ProcessSystemDpiAware = 1,
        ProcessPerMonitorDpiAware = 2
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var automation in _automationBackends)
        {
            automation?.Dispose();
        }

        _disposed = true;
        LoggingService.LogInfo("AutomationService 해제됨");
    }
}
