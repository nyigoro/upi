using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;

namespace UPI.Core;

public static class TarExtractor
{
    public static void ExtractTarGz(string archivePath, string extractTo)
    {
        Console.WriteLine($"📦 Extracting tar.gz: {archivePath}");

        if (Directory.Exists(extractTo))
            Directory.Delete(extractTo, true);

        Directory.CreateDirectory(extractTo);

        using var fileStream = File.OpenRead(archivePath);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

        TarFile.ExtractToDirectory(gzipStream, extractTo, overwriteFiles: true);

        Console.WriteLine("✅ tar.gz extraction complete.");
    }

    public static void ExtractTarXz(string archivePath, string extractTo)
    {
        if (!(OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()))
        {
            Console.WriteLine("❌ .tar.xz extraction is only supported on Linux/macOS.");
            return;
        }

        Console.WriteLine("📦 Linux/macOS detected. Using system 'tar' for .tar.xz extraction...");

        if (Directory.Exists(extractTo))
            Directory.Delete(extractTo, true);

        Directory.CreateDirectory(extractTo);

        var startInfo = new ProcessStartInfo
        {
            FileName = "tar",
            Arguments = $"-xf \"{archivePath}\" -C \"{extractTo}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            Console.WriteLine("❌ Failed to start tar process.");
            return;
        }

        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            Console.WriteLine("✅ System tar extraction complete.");
        }
        else
        {
            var error = process.StandardError.ReadToEnd();
            Console.WriteLine("❌ System tar failed.");
            Console.WriteLine(error);
            Console.WriteLine("⚠️ Ensure 'tar' and 'xz-utils' are installed.");
        }
    }
}