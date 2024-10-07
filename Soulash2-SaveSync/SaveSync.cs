using System.Diagnostics;
using System.Text.Json;
using Soulash2_SaveSync.Integrations;

namespace Soulash2_SaveSync;

public class SaveSync(BaseIntegration? imSelectedIntegration)
{
    private const string CfgName = "launch_cfg.json";
    public void Start()
    {
        var launchCfg = new LaunchConfig();
        if (File.Exists(CfgName))
        {
            launchCfg = JsonSerializer.Deserialize<LaunchConfig>(File.ReadAllText(CfgName));
        }
        else
        {
            var s = JsonSerializer.Serialize(launchCfg);
            File.WriteAllText(CfgName,s);
        }
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
        var status = imSelectedIntegration.ZipAndUpload();
        // todo setup retry 
    }
}

public class LaunchConfig()
{
#if DEBUG
    // Adjust this as required
    public string ExePath { get; set; } = @"C:\Program Files (x86)\Steam\steamapps\common\Soulash 2\Soulash 2.exe";
#else
    public string ExePath { get; set; } = "Soulash 2.exe";
#endif
}