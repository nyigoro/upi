using UPI.Adapters;
using UPI.Core;

namespace UPI.Core;

public static class Bootstrapper
{
    public static async Task DownloadAsync(IEngineAdapter adapter)
    {
        UpiPaths.EnsureDirectories();

        if (adapter is not NodeAdapter)
        {
            Console.WriteLine("⚠️ Bootstrapper not implemented for this adapter yet.");
            return;
        }

        var url = NodeReleaseResolver.GetDownloadUrl();
        var platform = OperatingSystem.IsWindows() ? "win" :
                       OperatingSystem.IsLinux() ? "linux" : "darwin";
        var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower();

        Console.WriteLine($"🌍 Detected Platform: {platform}");
        Console.WriteLine($"🧠 Detected Arch: {arch}");

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

        // 3️⃣ Get extracted folder
        var extractedFolder = Directory.GetDirectories(tempExtract).FirstOrDefault();
        if (extractedFolder == null)
        {
            Console.WriteLine("❌ Extraction failed: no folder found.");
            return;
        }

        // 4️⃣ Remove previous Node engine if exists
        if (Directory.Exists(UpiPaths.NodeEngine))
            Directory.Delete(UpiPaths.NodeEngine, true);

        // 5️⃣ Copy extracted Node to engines folder
        DirectoryCopy(extractedFolder, UpiPaths.NodeEngine);

        // 6️⃣ Set executable permissions for Linux/macOS
        if (!OperatingSystem.IsWindows())
        {
            var nodeBin = Path.Combine(UpiPaths.NodeEngine, "bin", "node");
            if (File.Exists(nodeBin))
                PlatformHelper.SetExecutablePermission(nodeBin);

            var npmBin = Path.Combine(UpiPaths.NodeEngine, "bin", "npm");
            if (File.Exists(npmBin))
                PlatformHelper.SetExecutablePermission(npmBin);
        }

        // 7️⃣ Inject Node folder into current PATH (Windows + Linux/macOS)
        var nodeDir = OperatingSystem.IsWindows()
            ? UpiPaths.NodeEngine               // node.exe lives here on Windows
            : Path.Combine(UpiPaths.NodeEngine, "bin"); // bin/ for Linux/macOS

        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        Environment.SetEnvironmentVariable("PATH", $"{nodeDir}{Path.PathSeparator}{currentPath}");

        Console.WriteLine($"✅ Node installed successfully at: {UpiPaths.NodeEngine}");
    }

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
