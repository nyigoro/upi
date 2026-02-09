namespace UPI.Core;

public static class PythonInstaller
{
    private static readonly string PythonDir = UpiPaths.PythonEngine;

    public static async Task EnsureInstalledAsync()
    {
        if (Directory.Exists(PythonDir) && File.Exists(GetPythonExecutable()))
        {
            Console.WriteLine($"✅ Python already installed at {PythonDir}");
            return;
        }

        Console.WriteLine("🌍 Downloading Python...");

        UpiPaths.EnsureDirectories();

        var url = await PythonReleaseResolver.GetDownloadUrlAsync();
        var downloadPath = Path.Combine(UpiPaths.Cache, Path.GetFileName(url));
        var tempExtract = Path.Combine(UpiPaths.Cache, "python_extract");

        await Downloader.DownloadFileAsync(url, downloadPath);

        if (url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            ZipExtractor.Extract(downloadPath, tempExtract);
        }
        else if (url.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
        {
            TarExtractor.ExtractTarGz(downloadPath, tempExtract);
        }
        else if (url.EndsWith(".tar.xz", StringComparison.OrdinalIgnoreCase))
        {
            TarExtractor.ExtractTarXz(downloadPath, tempExtract);
        }
        else
        {
            Console.WriteLine("❌ Unknown archive format.");
            return;
        }

        var pythonRoot = Path.Combine(tempExtract, "python");
        if (!Directory.Exists(pythonRoot))
        {
            var extractedFolder = Directory.GetDirectories(tempExtract).FirstOrDefault();
            pythonRoot = extractedFolder ?? tempExtract;
        }

        if (Directory.Exists(PythonDir))
            Directory.Delete(PythonDir, true);

        DirectoryCopy(pythonRoot, PythonDir);

        SetExecutables();
        EnsurePip();

        Console.WriteLine($"✅ Python installed successfully at: {PythonDir}");
    }

    private static void SetExecutables()
    {
        if (OperatingSystem.IsWindows())
            return;

        var python3 = Path.Combine(PythonDir, "bin", "python3");
        var python = Path.Combine(PythonDir, "bin", "python");
        var pip3 = Path.Combine(PythonDir, "bin", "pip3");
        var pip = Path.Combine(PythonDir, "bin", "pip");

        if (File.Exists(python3))
            PlatformHelper.SetExecutablePermission(python3);
        if (File.Exists(python))
            PlatformHelper.SetExecutablePermission(python);
        if (File.Exists(pip3))
            PlatformHelper.SetExecutablePermission(pip3);
        if (File.Exists(pip))
            PlatformHelper.SetExecutablePermission(pip);
    }

    private static void EnsurePip()
    {
        var python = GetPythonExecutable();
        if (!File.Exists(python))
            return;

        var code = ProcessRunner.Run(python, "-m pip --version");
        if (code == 0)
            return;

        ProcessRunner.Run(python, "-m ensurepip");
    }

    private static string GetPythonExecutable()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(PythonDir, "python.exe");

        var python3 = Path.Combine(PythonDir, "bin", "python3");
        if (File.Exists(python3))
            return python3;

        return Path.Combine(PythonDir, "bin", "python");
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
