using System.Text.Json;
using System.Text.Json.Serialization;
using Soulash2_SaveSync.Integrations;
using Soulash2_SaveSync.Integrations.DropBox;

namespace Soulash2_SaveSync.Configs;

public class SettingsConfig
{
    private const string ConfigFilePath = "savesync_config.json";
    public string SelectedIntegrationName { get; set; } = "";
    public bool ReplaceSaveWithoutAsking { get; init; }
    
    public string SaveLocation { get; set; } =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        @"WizardsOfTheCode\Soulash2\saves");

    public LaunchConfig LaunchConfig { get; set; } = new();
    
    public ConfigModel ConfigModel { get; set; } = new();

    [JsonIgnore]
    public BaseIntegration? SelectedIntegration;

    public void Save()
    {
        var s = JsonSerializer.Serialize<SettingsConfig>(this);
        File.WriteAllText(ConfigFilePath, s);
    }

    public bool TryLoadExisting(List<BaseIntegration?> integrations)
    {
        if (!File.Exists(ConfigFilePath)) return false;
        
        try
        {
            var integrationSettings = JsonSerializer.Deserialize<SettingsConfig>(File.ReadAllText(ConfigFilePath));
            if (integrationSettings == null) return false;

            SelectedIntegration =
                integrations.FirstOrDefault(i => i?.GetType().Name == integrationSettings.SelectedIntegrationName);
            if (SelectedIntegration != null)
            {
                Console.WriteLine($"Loaded existing selected integration: {SelectedIntegration.GetType().Name}");
                integrationSettings.SelectedIntegrationName = SelectedIntegration.GetType().Name;
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
}