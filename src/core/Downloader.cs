using System.Net;

namespace UPI.Core;

public static class Downloader
{
    public static async Task DownloadFileAsync(string url, string outputPath)
    {
        Console.WriteLine($"📥 Downloading: {url}");
        Console.WriteLine($"📍 Saving to: {outputPath}");

        using var client = new HttpClient();
        var data = await client.GetByteArrayAsync(url);

        await File.WriteAllBytesAsync(outputPath, data);

        Console.WriteLine("✅ Download complete.");
    }
}