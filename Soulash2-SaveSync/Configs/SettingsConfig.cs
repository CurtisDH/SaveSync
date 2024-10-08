using System.Text.Json;
using System.Text.Json.Serialization;
using Soulash2_SaveSync.Integrations;
using Soulash2_SaveSync.Integrations.DropBox;

namespace Soulash2_SaveSync.Configs;

public class SettingsConfig
{
    private const string ConfigFilePath = "savesync_config.json";
    public string BackupDirectory { get; set; } = "SaveSyncBackups";
    public string SelectedIntegrationName { get; set; } = "";
    public bool ReplaceSaveWithoutAsking { get; init; }

    public string SaveLocation { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        @"WizardsOfTheCode\Soulash2\saves");

    public LaunchConfig LaunchConfig { get; set; } = new();

    public ConfigModel DropboxConfigModel { get; set; } = new();

    [JsonIgnore]
    public BaseIntegration? SelectedIntegration;

    public void Save()
    {
        var s = JsonSerializer.Serialize<SettingsConfig>(this, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        File.WriteAllText(ConfigFilePath, s);
    }

    public bool TryLoadExisting(List<BaseIntegration?> integrations)
    {
        if (!File.Exists(ConfigFilePath)) return false;
        try
        {
            SelectedIntegration =
                integrations.FirstOrDefault(i => i?.GetType().Name == SelectedIntegrationName);
            if (SelectedIntegration != null)
            {
                Console.WriteLine($"Loaded existing selected integration: {SelectedIntegration.GetType().Name}");
                SelectedIntegrationName = SelectedIntegration.GetType().Name;
                return SelectedIntegration.TestConnection();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return false;
        }

        return false;
    }

    public static SettingsConfig? LoadJson()
    {
        return !File.Exists(ConfigFilePath) ? null : JsonSerializer.Deserialize<SettingsConfig>(File.ReadAllText(ConfigFilePath));
    }
}