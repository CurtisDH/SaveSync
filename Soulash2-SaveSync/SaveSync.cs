using System.Diagnostics;
using Soulash2_SaveSync.Integrations;

namespace Soulash2_SaveSync;

public class SaveSync(BaseIntegration? imSelectedIntegration)
{
#if DEBUG
    // Adjust this as required
    private string ExePath = @"C:\Program Files (x86)\Steam\steamapps\common\Soulash 2\Soulash 2.exe";
#else
    private const string ExePath = "Soulash 2.exe";
#endif
    public void Start()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ExePath,
            WorkingDirectory = Path.GetDirectoryName(ExePath),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        imSelectedIntegration.DownloadAndPromptReplace();
        var process = Process.Start(startInfo);
        process?.WaitForExit();
        var status = imSelectedIntegration.ZipAndUpload();
        // todo setup retry 
    }
}