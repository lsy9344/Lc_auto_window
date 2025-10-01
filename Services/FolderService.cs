using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lc_auto.Services;

/// <summary>
/// Windows 탐색기를 통해 대상 폴더를 여는 FolderService 구현체입니다.
/// </summary>
public class FolderService : IFolderService
{
    /// <inheritdoc />
    public Task<bool> OpenAsync(string folderPath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!Validate(folderPath))
        {
            LoggingService.LogWarn($"폴더 경로 검증에 실패하여 열기를 중단했습니다. path={folderPath}");
            return Task.FromResult(false);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{folderPath}\"",
            UseShellExecute = true
        };

        try
        {
            ct.ThrowIfCancellationRequested();

            var process = Process.Start(startInfo);
            if (process == null)
            {
                LoggingService.LogError("탐색기 프로세스가 null을 반환했습니다.", new InvalidOperationException("Process.Start returned null"));
                return Task.FromResult(false);
            }

            LoggingService.LogInfo($"폴더를 Windows 탐색기로 열었습니다. path={folderPath}");
            return Task.FromResult(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LoggingService.LogError("폴더 열기 중 예외가 발생했습니다.", ex);
            return Task.FromResult(false);
        }
    }

    private bool Validate(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            LoggingService.LogWarn("폴더 경로가 비어 있습니다.");
            return false;
        }

        if (!Directory.Exists(path))
        {
            LoggingService.LogWarn($"폴더 경로가 존재하지 않습니다. path={path}");
            return false;
        }

        return true;
    }
}
