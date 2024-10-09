using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SaveSync.Integrations;
using SaveSync.Integrations.DropBox;

namespace SaveSync.Configs;

public class SettingsConfig
{
    private const string ConfigFilePath = "savesync_config.json";
    public string BackupDirectory { get; set; } = "SaveSyncBackups";
    public string SelectedIntegrationName { get; set; } = "";
    public bool ReplaceSaveWithoutAsking { get; init; }

    public int RetryUploadCount { get; set; } = 3;

    public string SaveLocation { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        @"WizardsOfTheCode\Soulash2\saves");

    public LaunchConfig LaunchConfig { get; set; } = new();

    public ConfigModel DropboxConfigModel { get; set; } = new();

    [JsonIgnore]
    public BaseIntegration? SelectedIntegration;

    public void Save()
    {
        bool prompt = !File.Exists(ConfigFilePath);
        var s = JsonSerializer.Serialize<SettingsConfig>(this, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        File.WriteAllText(ConfigFilePath, s);
        if (prompt)
        {
            Console.WriteLine("##############################################################");
            Console.WriteLine($"Config was not found. A new one has been created at {Path.GetFullPath(ConfigFilePath)}");
            
            Console.WriteLine($"\nIf you want to want to customise SaveSync for a game that isn't 'Soulash 2'," +
                              $" please exit this now and edit the json.\n" +
                              $"Otherwise ensure this exe is in the same directory as Soulash 2");
            
            Console.WriteLine($"\n this will attempt to launch: \n {Path.GetFullPath(LaunchConfig.ExePath)}");
            
            Console.WriteLine("##############################################################");
            
            Console.WriteLine("Once confirmed press any key to continue...");
            Console.ReadKey();
        }
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
        // Enable reflection-based serialization
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
        return !File.Exists(ConfigFilePath) ? null : JsonSerializer.Deserialize<SettingsConfig>(File.ReadAllText(ConfigFilePath),options);
    }
}