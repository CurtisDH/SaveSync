using System.IO.Compression;

namespace Soulash2_SaveSync.Integrations;

public abstract class BaseIntegration
{
    private const string DownloadFolder = "SaveSyncDownloads";
    
    protected abstract byte[] Download();
    protected abstract bool Upload(byte[] zippedContents);
    public abstract Task DisplayUiOptions();

    public void DownloadAndPromptReplace()
    {
        Console.WriteLine("Creating backup...");
        var current = ZipDirectory();
        var backupPath = Path.Combine(IntegrationManager.SettingsConfig.BackupDirectory,$"Backup-{DateTime.Now.DayOfWeek}.zip");
        if (!Directory.Exists(IntegrationManager.SettingsConfig.BackupDirectory))
            Directory.CreateDirectory(IntegrationManager.SettingsConfig.BackupDirectory);
        File.WriteAllBytes(backupPath,current);
        Console.WriteLine($"Backup created at {backupPath}. Size: {(float)current.Length/1024} KB");
        Console.WriteLine("Downloading...");
        var downloadedFiles = Download();
        if (downloadedFiles.Length == 0)
        {
            Console.WriteLine("No valid download file found.");
            return;
        }
        string downloadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), DownloadFolder);

        if (Directory.Exists(downloadFolderPath))
            Directory.Delete(downloadFolderPath, true);
        Directory.CreateDirectory(downloadFolderPath);

        using (var memoryStream = new MemoryStream(downloadedFiles))
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                archive.ExtractToDirectory(downloadFolderPath);
            }
        }

        var extractedFiles = Directory.GetFiles(downloadFolderPath, "*", SearchOption.AllDirectories);
        foreach (var downloadedFilePath in extractedFiles)
        {
            var relativePath = Path.GetRelativePath(downloadFolderPath, downloadedFilePath);
            var saveFilePath = Path.Combine(IntegrationManager.SettingsConfig.SaveLocation, relativePath);

            if (File.Exists(saveFilePath))
            {
                var downloadedFileInfo = new FileInfo(downloadedFilePath);
                var saveFileInfo = new FileInfo(saveFilePath);

                Console.WriteLine($"File: {relativePath}");
                Console.WriteLine($"Downloaded file date: {downloadedFileInfo.LastWriteTime}");
                Console.WriteLine($"Existing file date: {saveFileInfo.LastWriteTime}");

                bool shouldReplace = IntegrationManager.SettingsConfig.ReplaceSaveWithoutAsking;
                if (shouldReplace == false)
                {
                    if ((downloadedFileInfo.LastWriteTime > saveFileInfo.LastWriteTime))
                    {
                        Console.WriteLine(
                            "The downloaded file is newer. Do you want to replace the existing file? (y/n)");
                        var input = Console.ReadLine();
                        shouldReplace = input?.ToLower() == "y";
                    }
                    else
                    {
                        Console.WriteLine(
                            "The existing file is newer or has the same date. Do you want to replace it anyway? (y/n)");
                        var input = Console.ReadLine();
                        shouldReplace = input?.ToLower() == "y";
                    }
                }

                if (shouldReplace)
                {
                    File.Copy(downloadedFilePath, saveFilePath, overwrite: true);
                    Console.WriteLine($"Replaced {relativePath}.");
                }
                else
                {
                    Console.WriteLine($"Skipped {relativePath}.");
                }
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(saveFilePath) ?? string.Empty);
                File.Copy(downloadedFilePath, saveFilePath);
                Console.WriteLine($"Added new file: {relativePath}");
            }
        }
    }

    public bool ZipAndUpload()
    {
        var zippedData = ZipDirectory();
        var success = Upload(zippedData);
        Console.WriteLine(success ? "Upload completed successfully." : "Upload failed.");
        return success;
    }

    private byte[] ZipDirectory()
    {
        try
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var saveDirectory = new DirectoryInfo(IntegrationManager.SettingsConfig.SaveLocation);
                ZipDirectoryRecursively(saveDirectory, archive, string.Empty);
            }

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while zipping directory: {ex.Message}");
            return new byte[] { };
        }
    }

    private void ZipDirectoryRecursively(DirectoryInfo directory, ZipArchive archive, string basePath)
    {
        foreach (var file in directory.GetFiles())
        {
            var relativePath = Path.Combine(basePath, file.Name);
            var zipEntry = archive.CreateEntry(relativePath, CompressionLevel.Fastest);
            using var zipStream = zipEntry.Open();
            using var fileStream = file.OpenRead();
            fileStream.CopyTo(zipStream);
        }

        foreach (var subDirectory in directory.GetDirectories())
        {
            var subDirectoryPath = Path.Combine(basePath, subDirectory.Name);
            ZipDirectoryRecursively(subDirectory, archive, subDirectoryPath);
        }
    }

    public abstract bool TestConnection();
}