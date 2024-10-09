using System.Diagnostics;
using SaveSync.Integrations;

namespace SaveSync;

public class SaveSync(BaseIntegration? imSelectedIntegration)
{
    public void Start()
    {
        var launchCfg = IntegrationManager.SettingsConfig.LaunchConfig;
        Console.WriteLine($"Launch config exe path:{launchCfg.ExePath}");
        var startInfo = new ProcessStartInfo
        {
            FileName = launchCfg.ExePath,
            WorkingDirectory = Path.GetDirectoryName(launchCfg.ExePath),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        imSelectedIntegration.DownloadAndPromptReplace();
        var process = Process.Start(startInfo);
        process?.WaitForExit();
        Console.WriteLine($"Uploading save file to {imSelectedIntegration.GetType().Name}");
        int count = 0;
        bool status = false;
        while (status == false)
        {
            count++;
            status = imSelectedIntegration.ZipAndUpload();
            if (count > IntegrationManager.SettingsConfig.RetryUploadCount)
            {
                Console.WriteLine($"Failed to upload attempting retry:{count}, max attempts:{IntegrationManager.SettingsConfig.RetryUploadCount}");
                break;
            }
        }
        Thread.Sleep(1000);
    }
}

public class LaunchConfig()
{
#if DEBUG
    public string ExePath { get; set; } = @"C:\Program Files (x86)\Steam\steamapps\common\Soulash 2\Soulash 2.exe";
#else
    public string ExePath { get; set; } = "Soulash 2.exe";
#endif
}