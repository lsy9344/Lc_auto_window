using System;
using System.IO;
using System.Text.Json;
using Lc_auto.Models;

namespace Lc_auto.Services;

/// <summary>
/// 설정 파일(config.json)을 로드하고 필수 필드를 검증하며 AlertsScheduler를 초기화하는 서비스입니다.
/// </summary>
public class ConfigService : IConfigService
{
    private readonly IAlertsScheduler _alertsScheduler;
    private AppConfig _current;

    /// <summary>
    /// 현재 메모리에 로드된 애플리케이션 설정입니다.
    /// </summary>
    public AppConfig Current => _current;

    /// <summary>
    /// 설정 파일 변경 시 발생하는 이벤트 (Hot Reload, Phase 2에서 구현 예정).
    /// </summary>
#pragma warning disable CS0067 // Reserved for Hot Reload implementation in later phase
    public event EventHandler<AppConfig>? ConfigChanged;
#pragma warning restore CS0067

    /// <summary>
    /// ConfigService를 생성하고 즉시 config.json을 로드합니다.
    /// </summary>
    /// <param name="alertsScheduler">알림 스케줄을 적용할 AlertsScheduler 인스턴스</param>
    public ConfigService(IAlertsScheduler alertsScheduler)
    {
        _alertsScheduler = alertsScheduler ?? throw new ArgumentNullException(nameof(alertsScheduler));
        _current = new AppConfig();
        LoadConfig();
    }

    /// <summary>
    /// config.json 파일을 역직렬화하고 필수 필드를 검증한 뒤 AlertsScheduler에 적용합니다.
    /// </summary>
    private void LoadConfig()
    {
        const string configPath = "config/config.json";

        try
        {
            var json = File.ReadAllText(configPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<AppConfig>(json, options);

            if (config == null)
            {
                LoggingService.LogWarn("config.json 역직렬화 결과가 null이어서 기본 설정을 유지합니다.");
                _alertsScheduler.Stop();
                return;
            }

            _current = config;

            var alerts = _current.Alerts ?? Array.Empty<AlertConfig>();
            _alertsScheduler.Apply(alerts);
            _alertsScheduler.Start();

            if (!ValidateConfig())
            {
                LoggingService.LogWarn("config.json 필수 필드 검증에 실패했지만 기본 설정 값으로 계속 실행합니다.");
            }
            else
            {
                LoggingService.LogInfo("config.json 로드 및 검증을 완료했습니다.");
            }

            // TODO: Phase 2에서 Hot Reload 구현 시 ConfigChanged 이벤트를 발생시킵니다.
        }
        catch (FileNotFoundException ex)
        {
            LoggingService.LogError($"config.json 파일을 찾을 수 없어 기본 설정으로 실행합니다. 경로: {configPath}", ex);
            _alertsScheduler.Stop();
        }
        catch (JsonException ex)
        {
            LoggingService.LogError($"config.json JSON 파싱에 실패하여 기본 설정으로 실행합니다. 경로: {configPath}", ex);
            _alertsScheduler.Stop();
        }
        catch (Exception ex)
        {
            LoggingService.LogError("설정 로드 중 알 수 없는 오류가 발생하여 기본 설정으로 실행합니다.", ex);
            _alertsScheduler.Stop();
        }
    }

    /// <summary>
    /// 필수 설정 필드(videoPath, targetFolder, automation.p1/p2.featureId)를 검증합니다.
    /// </summary>
    /// <returns>모든 필수 필드가 유효하면 true, 하나라도 부적절하면 false</returns>
    private bool ValidateConfig()
    {
        var isValid = true;

        if (string.IsNullOrWhiteSpace(_current.Media?.VideoPath))
        {
            LoggingService.LogWarn("config.json 검증 실패: media.videoPath 값이 비어 있습니다.");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(_current.Paths?.TargetFolder))
        {
            LoggingService.LogWarn("config.json 검증 실패: paths.targetFolder 값이 비어 있습니다.");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(_current.Automation?.P1?.FeatureId))
        {
            LoggingService.LogWarn("config.json 검증 실패: automation.p1.featureId 값이 비어 있습니다.");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(_current.Automation?.P2?.FeatureId))
        {
            LoggingService.LogWarn("config.json 검증 실패: automation.p2.featureId 값이 비어 있습니다.");
            isValid = false;
        }

        return isValid;
    }
}
