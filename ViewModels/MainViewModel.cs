using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Lc_auto.Services;
using Lc_auto.UI.Components;

namespace Lc_auto.ViewModels;

/// <summary>
/// MainWindow의 ViewModel - MVVM 패턴 구현
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly IMediaService _mediaService;
    private readonly IFolderService _folderService;
    private readonly IConfigService _configService;

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

    public MainViewModel(
        IMediaService mediaService,
        IFolderService folderService,
        IConfigService configService)
    {
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        _folderService = folderService ?? throw new ArgumentNullException(nameof(folderService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));

        ButtonACommand = new AsyncRelayCommand(ExecuteButtonAAsync);
        ButtonBCommand = new AsyncRelayCommand(ExecuteButtonBAsync);
        ButtonCCommand = new AsyncRelayCommand(ExecuteButtonCAsync);
        ButtonDCommand = new AsyncRelayCommand(ExecuteButtonDAsync);
    }

    /// <summary>
    /// 속성 변경 알림 헬퍼 메서드
    /// </summary>
    /// <param name="propertyName">변경된 속성 이름 (자동 지정)</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private Task ExecuteButtonAAsync()
    {
        LoggingService.LogInfo("버튼 A 명령이 실행되었습니다.");
        return Task.CompletedTask;
    }

    private Task ExecuteButtonBAsync()
    {
        LoggingService.LogInfo("버튼 B 명령이 실행되었습니다.");
        return Task.CompletedTask;
    }

    private async Task ExecuteButtonCAsync()
    {
        try
        {
            var mediaConfig = _configService.Current.Media;

            if (_mediaService.IsPlaying)
            {
                ToastService.Show("동영상이 이미 재생 중입니다.", 2000);
                LoggingService.LogWarn("사용자가 동영상 재생을 요청했지만 이미 재생 중입니다.");
                return;
            }

            var started = await _mediaService.PlayAsync(mediaConfig.VideoPath, mediaConfig.PlayerPath, CancellationToken.None);

            if (started)
            {
                ToastService.Show("동영상 재생을 시작했습니다.", 3000);
                LoggingService.LogInfo($"동영상 재생 명령이 성공적으로 실행되었습니다. videoPath={mediaConfig.VideoPath}");
            }
            else
            {
                DialogService.ShowDialog("동영상 재생 실패", "동영상 파일 또는 플레이어를 확인해주세요.");
                LoggingService.LogWarn("동영상 재생 서비스가 실패 결과를 반환했습니다.");
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("동영상 재생 명령 실행 중 예외가 발생했습니다.", ex);
            DialogService.ShowDialog("동영상 재생 실패", "동영상 재생 중 오류가 발생했습니다.");
        }
    }

    private async Task ExecuteButtonDAsync()
    {
        try
        {
            var targetFolder = _configService.Current.Paths.TargetFolder;
            var opened = await _folderService.OpenAsync(targetFolder, CancellationToken.None);

            if (opened)
            {
                ToastService.Show("사진 저장 폴더를 열었습니다.", 3000);
                LoggingService.LogInfo($"사진 저장 폴더 열기 명령이 성공적으로 실행되었습니다. targetFolder={targetFolder}");
            }
            else
            {
                DialogService.ShowDialog("폴더 열기 실패", "폴더를 열 수 없습니다. 경로를 확인해주세요.");
                LoggingService.LogWarn("사진 저장 폴더 열기 서비스가 실패 결과를 반환했습니다.");
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("사진 저장 폴더 열기 명령 실행 중 예외가 발생했습니다.", ex);
            DialogService.ShowDialog("폴더 열기 실패", "폴더를 여는 중 오류가 발생했습니다.");
        }
    }
}
