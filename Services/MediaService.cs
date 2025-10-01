using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lc_auto.Services;

/// <summary>
/// 동영상 재생과 단일 인스턴스 정책을 관리하는 서비스 구현체입니다.
/// </summary>
public class MediaService : IMediaService
{
    private readonly object _sync = new();
    private Process? _currentProcess;

    /// <inheritdoc />
    public bool IsPlaying
    {
        get
        {
            lock (_sync)
            {
                return _currentProcess is { HasExited: false };
            }
        }
    }

    /// <inheritdoc />
    public Task<bool> PlayAsync(string videoPath, string? playerPath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(videoPath))
        {
            throw new ArgumentException("동영상 경로는 비어 있을 수 없습니다.", nameof(videoPath));
        }

        lock (_sync)
        {
            if (_currentProcess is { HasExited: false })
            {
                LoggingService.LogWarn("동영상 재생이 이미 진행 중이므로 새 요청을 무시합니다.");
                return Task.FromResult(false);
            }
        }

        if (!File.Exists(videoPath))
        {
            LoggingService.LogWarn($"동영상 파일을 찾을 수 없어 재생을 중단했습니다. 경로: {videoPath}");
            return Task.FromResult(false);
        }

        ProcessStartInfo startInfo;
        if (string.IsNullOrWhiteSpace(playerPath))
        {
            startInfo = new ProcessStartInfo
            {
                FileName = videoPath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(videoPath) ?? Environment.CurrentDirectory
            };
        }
        else
        {
            if (!File.Exists(playerPath))
            {
                LoggingService.LogWarn($"사용자 지정 플레이어를 찾을 수 없어 재생을 중단했습니다. 경로: {playerPath}");
                return Task.FromResult(false);
            }

            startInfo = new ProcessStartInfo
            {
                FileName = playerPath,
                Arguments = $"\"{videoPath}\"",
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(playerPath) ?? Environment.CurrentDirectory
            };
        }

        Process? startedProcess = null;

        try
        {
            ct.ThrowIfCancellationRequested();

            startedProcess = Process.Start(startInfo);
            if (startedProcess == null)
            {
                LoggingService.LogError("동영상 재생 프로세스 시작에 실패했습니다.", new InvalidOperationException("Process.Start returned null"));
                return Task.FromResult(false);
            }

            startedProcess.EnableRaisingEvents = true;
            startedProcess.Exited += HandleProcessExited;

            lock (_sync)
            {
                _currentProcess = startedProcess;
            }

            var playerDescriptor = string.IsNullOrWhiteSpace(playerPath) ? "기본" : playerPath;
            LoggingService.LogInfo($"동영상 재생을 시작했습니다. videoPath={videoPath}, player={playerDescriptor}");
            return Task.FromResult(true);
        }
        catch (OperationCanceledException)
        {
            if (startedProcess != null)
            {
                CleanupProcess(startedProcess);
            }

            lock (_sync)
            {
                if (_currentProcess == startedProcess)
                {
                    _currentProcess = null;
                }
            }

            throw;
        }
        catch (Exception ex)
        {
            LoggingService.LogError("동영상 재생 중 예외가 발생했습니다.", ex);

            if (startedProcess != null)
            {
                CleanupProcess(startedProcess);
            }

            lock (_sync)
            {
                if (_currentProcess == startedProcess)
                {
                    _currentProcess = null;
                }
            }

            return Task.FromResult(false);
        }
    }

    private void HandleProcessExited(object? sender, EventArgs e)
    {
        if (sender is not Process exitedProcess)
        {
            LoggingService.LogWarn("동영상 재생 종료 이벤트에서 프로세스를 식별하지 못했습니다.");
            return;
        }

        CleanupProcess(exitedProcess);

        lock (_sync)
        {
            if (_currentProcess == exitedProcess)
            {
                _currentProcess = null;
            }
        }

        LoggingService.LogInfo("동영상 재생이 종료되었습니다.");
    }

    private void CleanupProcess(Process process)
    {
        try
        {
            process.EnableRaisingEvents = false;
            process.Exited -= HandleProcessExited;
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn("동영상 재생 프로세스 이벤트 해제 중 경고가 발생했습니다.", ex);
        }

        try
        {
            process.Dispose();
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn("동영상 재생 프로세스 리소스 해제 중 경고가 발생했습니다.", ex);
        }
    }
}
