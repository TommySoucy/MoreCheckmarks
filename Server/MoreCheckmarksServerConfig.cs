using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoreCheckmarks;

/// <summary>
/// User-editable server configuration, stored as config.json next to the mod DLL.
/// </summary>
public class MoreCheckmarksServerConfig
{
    [JsonPropertyName("hideInactiveEventQuests")]
    public bool HideInactiveEventQuests { get; set; } = true;

    [JsonPropertyName("excludedQuestIds")]
    public List<string> ExcludedQuestIds { get; set; } = new();

    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    /// <summary>
    /// Loads config.json from the given folder. Creates it with defaults if missing or unreadable.
    /// Never throws; returns defaults on any error.
    /// </summary>
    public static MoreCheckmarksServerConfig LoadOrCreate(string modFolder, Action<string>? logError = null)
    {
        var path = Path.Combine(modFolder, "config.json");
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var parsed = JsonSerializer.Deserialize<MoreCheckmarksServerConfig>(json);
                if (parsed != null)
                {
                    parsed.ExcludedQuestIds ??= new();
                    return parsed;
                }
            }
        }
        catch (Exception ex)
        {
            logError?.Invoke($"Failed to read config.json, using defaults: {ex.Message}");
        }

        var fresh = new MoreCheckmarksServerConfig();
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(fresh, WriteOptions));
        }
        catch (Exception ex)
        {
            logError?.Invoke($"Failed to write default config.json: {ex.Message}");
        }
        return fresh;
    }
}
