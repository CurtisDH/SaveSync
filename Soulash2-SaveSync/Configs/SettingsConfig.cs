namespace Soulash2_SaveSync.Configs;

public abstract class SettingsConfig
{
    public string SelectedIntegrationName { get; set; } = "";
    public string JsonConfig { get; set; } = "";
}