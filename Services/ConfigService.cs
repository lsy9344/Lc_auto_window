using System;
using System.IO;
using System.Text.Json;
using Lc_auto.Models;

namespace Lc_auto.Services;

/// <summary>
/// 설정 관리 서비스 구현 (config.json 로드, 검증, Hot Reload)
/// </summary>
public class ConfigService : IConfigService
{
    private AppConfig _current;

    /// <summary>
    /// 현재 로드된 애플리케이션 설정
    /// </summary>
    public AppConfig Current => _current;

    /// <summary>
    /// 설정 파일 변경 시 발생하는 이벤트 (Hot Reload, Phase 2에서 구현 예정)
    /// </summary>
    public event EventHandler<AppConfig>? ConfigChanged;

    /// <summary>
    /// ConfigService 생성자 - config.json 파일을 로드하여 초기화
    /// </summary>
    public ConfigService()
    {
        _current = new AppConfig(); // 기본값으로 초기화
        LoadConfig();
    }

    /// <summary>
    /// config.json 파일을 읽어서 AppConfig 객체로 역직렬화하고 검증
    /// </summary>
    private void LoadConfig()
    {
        const string configPath = "config/config.json";

        try
        {
            // JSON 파일 읽기
            string json = File.ReadAllText(configPath);

            // JSON 역직렬화 옵션 설정
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // AppConfig 객체로 역직렬화
            var config = JsonSerializer.Deserialize<AppConfig>(json, options);

            if (config != null)
            {
                _current = config;

                // 필수 필드 검증
                if (!ValidateConfig())
                {
                    Console.WriteLine("[ConfigService] 경고: 일부 필수 설정 필드가 누락되었습니다. 기본값으로 계속 진행합니다.");
                }
                else
                {
                    Console.WriteLine("[ConfigService] config.json 로드 및 검증 완료.");
                }

                // TODO: Phase 2에서 Hot Reload 구현 시 이벤트 발생
            }
            else
            {
                Console.WriteLine($"[ConfigService] 오류: {configPath} 역직렬화 실패 (null 반환). 기본값 사용.");
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"[ConfigService] 오류: {configPath} 파일을 찾을 수 없습니다. 기본값 사용.");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[ConfigService] 오류: {configPath} JSON 파싱 실패 - {ex.Message}. 기본값 사용.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConfigService] 오류: 설정 로드 중 예외 발생 - {ex.Message}. 기본값 사용.");
        }
    }

    /// <summary>
    /// 필수 설정 필드 검증 (videoPath, targetFolder, p1.featureId, p2.featureId)
    /// </summary>
    /// <returns>모든 필수 필드가 유효하면 true, 하나라도 누락되면 false</returns>
    private bool ValidateConfig()
    {
        bool isValid = true;

        // Media.VideoPath 검증
        if (string.IsNullOrWhiteSpace(_current.Media?.VideoPath))
        {
            Console.WriteLine("[ConfigService] 검증 실패: media.videoPath 필드가 비어있습니다.");
            isValid = false;
        }

        // Paths.TargetFolder 검증
        if (string.IsNullOrWhiteSpace(_current.Paths?.TargetFolder))
        {
            Console.WriteLine("[ConfigService] 검증 실패: paths.targetFolder 필드가 비어있습니다.");
            isValid = false;
        }

        // Automation.P1.FeatureId 검증 (start_photo)
        if (string.IsNullOrWhiteSpace(_current.Automation?.P1?.FeatureId))
        {
            Console.WriteLine("[ConfigService] 검증 실패: automation.p1.featureId 필드가 비어있습니다.");
            isValid = false;
        }

        // Automation.P2.FeatureId 검증 (export_files)
        if (string.IsNullOrWhiteSpace(_current.Automation?.P2?.FeatureId))
        {
            Console.WriteLine("[ConfigService] 검증 실패: automation.p2.featureId 필드가 비어있습니다.");
            isValid = false;
        }

        return isValid;
    }
}
