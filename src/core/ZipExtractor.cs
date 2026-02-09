using System.IO.Compression;

namespace UPI.Core;

public static class ZipExtractor
{
    public static void Extract(string zipPath, string extractTo)
    {
        Console.WriteLine($"📦 Extracting {zipPath}...");
        Console.WriteLine($"📂 Into: {extractTo}");

        if (Directory.Exists(extractTo))
            Directory.Delete(extractTo, true);

        Directory.CreateDirectory(extractTo);

        ZipFile.ExtractToDirectory(zipPath, extractTo);

        Console.WriteLine("✅ Extraction complete.");
    }
}