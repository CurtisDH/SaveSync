namespace SaveSync;
internal static class Program
{
    public static void Main(string[] args)
    {
        var im = new IntegrationManager();
        var saveSync = new SaveSync(IntegrationManager.SettingsConfig.SelectedIntegration);
        saveSync.Start();
    }
}