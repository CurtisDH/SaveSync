using Soulash2_SaveSync.Integrations;

namespace Soulash2_SaveSync;

internal static class Program
{
    public static void Main(string[] args)
    {
        var im = new IntegrationManager();
        im.ShowMenu();
        var saveSync = new SaveSync(im.SelectedIntegration);
        saveSync.Start();
    }
}

