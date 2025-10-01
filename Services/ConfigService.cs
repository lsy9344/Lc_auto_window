using System;
using System.IO;
using System.Text.Json;
using Lc_auto.Models;

namespace Lc_auto.Services;

/// <summary>
/// ?¤ì • ê´€ë¦??œë¹„??êµ¬í˜„ (config.json ë¡œë“œ, ê²€ì¦? Hot Reload)
/// </summary>
public class ConfigService : IConfigService
{
    private AppConfig _current;

    /// <summary>
    /// ?„ì¬ ë¡œë“œ??? í”Œë¦¬ì??´ì…˜ ?¤ì •
    /// </summary>
    public AppConfig Current => _current;

    /// <summary>
    /// ?¤ì • ?Œì¼ ë³€ê²???ë°œìƒ?˜ëŠ” ?´ë²¤??(Hot Reload, Phase 2?ì„œ êµ¬í˜„ ?ˆì •)
    /// </summary>
    #pragma warning disable CS0067 // Reserved for Hot Reload implementation in later phase
    public event EventHandler<AppConfig>? ConfigChanged;
#pragma warning restore CS0067

    /// <summary>
    /// ConfigService ?ì„±??- config.json ?Œì¼??ë¡œë“œ?˜ì—¬ ì´ˆê¸°??
    /// </summary>
    public ConfigService()
    {
        _current = new AppConfig(); // ê¸°ë³¸ê°’ìœ¼ë¡?ì´ˆê¸°??
        LoadConfig();
    }

    /// <summary>
    /// config.json ?Œì¼???½ì–´??AppConfig ê°ì²´ë¡???§?¬í™”?˜ê³  ê²€ì¦?
    /// </summary>
    private void LoadConfig()
    {
        const string configPath = "config/config.json";

        try
        {
            // JSON ?Œì¼ ?½ê¸°
            string json = File.ReadAllText(configPath);

            // JSON ??§?¬í™” ?µì…˜ ?¤ì •
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // AppConfig ê°ì²´ë¡???§?¬í™”
            var config = JsonSerializer.Deserialize<AppConfig>(json, options);

            if (config != null)
            {
                _current = config;

                // ?„ìˆ˜ ?„ë“œ ê²€ì¦?
                if (!ValidateConfig())
                {
                    Console.WriteLine("[ConfigService] ê²½ê³ : ?¼ë? ?„ìˆ˜ ?¤ì • ?„ë“œê°€ ?„ë½?˜ì—ˆ?µë‹ˆ?? ê¸°ë³¸ê°’ìœ¼ë¡?ê³„ì† ì§„í–‰?©ë‹ˆ??");
                }
                else
                {
                    Console.WriteLine("[ConfigService] config.json ë¡œë“œ ë°?ê²€ì¦??„ë£Œ.");
                }

                // TODO: Phase 2?ì„œ Hot Reload êµ¬í˜„ ???´ë²¤??ë°œìƒ
            }
            else
            {
                Console.WriteLine($"[ConfigService] ?¤ë¥˜: {configPath} ??§?¬í™” ?¤íŒ¨ (null ë°˜í™˜). ê¸°ë³¸ê°??¬ìš©.");
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"[ConfigService] ?¤ë¥˜: {configPath} ?Œì¼??ì°¾ì„ ???†ìŠµ?ˆë‹¤. ê¸°ë³¸ê°??¬ìš©.");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[ConfigService] ?¤ë¥˜: {configPath} JSON ?Œì‹± ?¤íŒ¨ - {ex.Message}. ê¸°ë³¸ê°??¬ìš©.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConfigService] ?¤ë¥˜: ?¤ì • ë¡œë“œ ì¤??ˆì™¸ ë°œìƒ - {ex.Message}. ê¸°ë³¸ê°??¬ìš©.");
        }
    }

    /// <summary>
    /// ?„ìˆ˜ ?¤ì • ?„ë“œ ê²€ì¦?(videoPath, targetFolder, p1.featureId, p2.featureId)
    /// </summary>
    /// <returns>ëª¨ë“  ?„ìˆ˜ ?„ë“œê°€ ? íš¨?˜ë©´ true, ?˜ë‚˜?¼ë„ ?„ë½?˜ë©´ false</returns>
    private bool ValidateConfig()
    {
        bool isValid = true;

        // Media.VideoPath ê²€ì¦?
        if (string.IsNullOrWhiteSpace(_current.Media?.VideoPath))
        {
            Console.WriteLine("[ConfigService] ê²€ì¦??¤íŒ¨: media.videoPath ?„ë“œê°€ ë¹„ì–´?ˆìŠµ?ˆë‹¤.");
            isValid = false;
        }

        // Paths.TargetFolder ê²€ì¦?
        if (string.IsNullOrWhiteSpace(_current.Paths?.TargetFolder))
        {
            Console.WriteLine("[ConfigService] ê²€ì¦??¤íŒ¨: paths.targetFolder ?„ë“œê°€ ë¹„ì–´?ˆìŠµ?ˆë‹¤.");
            isValid = false;
        }

        // Automation.P1.FeatureId ê²€ì¦?(start_photo)
        if (string.IsNullOrWhiteSpace(_current.Automation?.P1?.FeatureId))
        {
            Console.WriteLine("[ConfigService] ê²€ì¦??¤íŒ¨: automation.p1.featureId ?„ë“œê°€ ë¹„ì–´?ˆìŠµ?ˆë‹¤.");
            isValid = false;
        }

        // Automation.P2.FeatureId ê²€ì¦?(export_files)
        if (string.IsNullOrWhiteSpace(_current.Automation?.P2?.FeatureId))
        {
            Console.WriteLine("[ConfigService] ê²€ì¦??¤íŒ¨: automation.p2.featureId ?„ë“œê°€ ë¹„ì–´?ˆìŠµ?ˆë‹¤.");
            isValid = false;
        }

        return isValid;
    }
}
