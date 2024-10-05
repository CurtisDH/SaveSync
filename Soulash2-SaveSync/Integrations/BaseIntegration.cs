using System.IO.Compression;

namespace Soulash2_SaveSync.Integrations;

public abstract class BaseIntegration
{
    protected const string DownloadFolder = "SaveSyncDownloads";

    protected string SaveLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        @"WizardsOfTheCode\Soulash2\saves");

    protected abstract byte[] Download();
    protected abstract bool Upload(byte[] zippedContents);
    public abstract void DisplayUiOptions();
    public abstract void Run();

    public void DownloadAndPromptReplace()
    {
        var downloadedFiles = Download();
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
            var saveFilePath = Path.Combine(SaveLocation, relativePath);

            if (File.Exists(saveFilePath))
            {
                var downloadedFileInfo = new FileInfo(downloadedFilePath);
                var saveFileInfo = new FileInfo(saveFilePath);

                Console.WriteLine($"File: {relativePath}");
                Console.WriteLine($"Downloaded file date: {downloadedFileInfo.LastWriteTime}");
                Console.WriteLine($"Existing file date: {saveFileInfo.LastWriteTime}");

                bool shouldReplace;

                if (downloadedFileInfo.LastWriteTime > saveFileInfo.LastWriteTime)
                {
                    Console.WriteLine("The downloaded file is newer. Do you want to replace the existing file? (y/n)");
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
                var saveDirectory = new DirectoryInfo(SaveLocation);
                foreach (var file in saveDirectory.GetFiles())
                {
                    var zipEntry = archive.CreateEntry(file.Name, CompressionLevel.Fastest);
                    using var zipStream = zipEntry.Open();
                    using var fileStream = file.OpenRead();
                    fileStream.CopyTo(zipStream);
                }
            }

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while zipping directory: {ex.Message}");
            return new byte[]{};
        }
    }
}