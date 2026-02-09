using UPI.Adapters;
using UPI.Core;

namespace UPI.Core;

public static class NodeInstaller
{
    private static readonly string NodeDir = UpiPaths.NodeEngine;

    public static async Task EnsureInstalledAsync()
    {
        if (Directory.Exists(NodeDir) && File.Exists(GetNodeExecutable()))
        {
            Console.WriteLine($"✅ Node already installed at {NodeDir}");
            return;
        }

        Console.WriteLine("🌍 Downloading Node.js...");

        UpiPaths.EnsureDirectories();

        var url = NodeReleaseResolver.GetDownloadUrl();
        var downloadPath = Path.Combine(UpiPaths.Cache, Path.GetFileName(url));
        var tempExtract = Path.Combine(UpiPaths.Cache, "node_extract");

        // 1️⃣ Download Node archive
        await Downloader.DownloadFileAsync(url, downloadPath);

        // 2️⃣ Extract archive
        if (url.EndsWith(".zip"))
        {
            ZipExtractor.Extract(downloadPath, tempExtract);
        }
        else if (url.EndsWith(".tar.gz"))
        {
            TarExtractor.ExtractTarGz(downloadPath, tempExtract);
        }
        else if (url.EndsWith(".tar.xz"))
        {
            TarExtractor.ExtractTarXz(downloadPath, tempExtract);
        }
        else
        {
            Console.WriteLine("❌ Unknown archive format.");
            return;
        }

        // 3️⃣ Copy extracted files
        var extractedFolder = Directory.GetDirectories(tempExtract).FirstOrDefault();
        if (extractedFolder == null)
        {
            Console.WriteLine("❌ Extraction failed: no folder found.");
            return;
        }

        if (Directory.Exists(NodeDir))
            Directory.Delete(NodeDir, true);

        DirectoryCopy(extractedFolder, NodeDir);

        // 4️⃣ Set executable permissions for Linux/macOS
        SetExecutables();

        // 5️⃣ Inject Node folder into PATH
        InjectPath();

        Console.WriteLine($"✅ Node installed successfully at: {NodeDir}");
    }

    private static void SetExecutables()
    {
        if (!OperatingSystem.IsWindows())
        {
            var nodeBin = Path.Combine(NodeDir, "bin", "node");
            var npmBin = Path.Combine(NodeDir, "bin", "npm");

            if (File.Exists(nodeBin))
                PlatformHelper.SetExecutablePermission(nodeBin);
            if (File.Exists(npmBin))
                PlatformHelper.SetExecutablePermission(npmBin);
        }
    }

    private static void InjectPath()
    {
        var nodePath = OperatingSystem.IsWindows()
            ? NodeDir
            : Path.Combine(NodeDir, "bin");

        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        Environment.SetEnvironmentVariable("PATH", $"{nodePath}{Path.PathSeparator}{currentPath}");
    }

    private static string GetNodeExecutable()
        => OperatingSystem.IsWindows()
            ? Path.Combine(NodeDir, "node.exe")
            : Path.Combine(NodeDir, "bin", "node");

    private static void DirectoryCopy(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            DirectoryCopy(dir, destSubDir);
        }
    }
}
