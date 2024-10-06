using System.Text.Json;

namespace Soulash2_SaveSync.Integrations.DropBox;

public class DropboxSettingsConfig
{
    public DropboxSettingsConfig()
    {
        try
        {
            if (File.Exists("Dropbox_Config.json"))
            {
                string jsonString = File.ReadAllText("Dropbox_Config.json");
                var config = JsonSerializer.Deserialize<ConfigModel>(jsonString);

                AccessToken = config?.AccessToken ?? "";
                Uid = config?.Uid ?? "";
                RefreshToken = config?.RefreshToken ?? "";
            }
            else
            {
                Reset();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"error occurred. resetting dropbox config: {e.Message}");
            Reset();
        }
    }

    public string AccessToken { get; set; }
    public string Uid { get; set; }
    public string RefreshToken { get; set; }

    public void Save()
    {
        var config = new
        {
            AccessToken,
            Uid,
            RefreshToken
        };

        string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("Dropbox_Config.json", jsonString);
    }

    public void Reset()
    {
        AccessToken = "";
        RefreshToken = "";
        Uid = "";
    }

    private class ConfigModel
    {
        public string AccessToken { get; set; }
        public string Uid { get; set; }
        public string RefreshToken { get; set; }
    }
}