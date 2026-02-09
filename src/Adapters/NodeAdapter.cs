using UPI.Core;

namespace UPI.Adapters;

public class NodeAdapter : IEngineAdapter
{
    public string Name => "Node.js (npm)";
    public string[] ProjectFiles => new[] { "package.json" };
    public string DownloadUrl => "https://nodejs.org/dist/";

    public bool DetectProject(string directory)
    {
        foreach (var file in ProjectFiles)
        {
            if (File.Exists(Path.Combine(directory, file)))
                return true;
        }

        return false;
    }

    public string? GetExecutablePath()
    {
        // 1️⃣ Check system PATH first
        var globalNpm = PathHelper.FindExecutable("npm");
        if (globalNpm != null)
            return globalNpm;

        // 2️⃣ Check local UPI cache
        string relativePath = OperatingSystem.IsWindows()
            ? "npm.cmd"
            : "bin/npm";

        var localNpm = Path.Combine(UpiPaths.NodeEngine, relativePath);
        return File.Exists(localNpm) ? localNpm : null;
    }

    public void Execute(string[] args)
    {
        var exePath = GetExecutablePath();

        if (exePath == null)
        {
            Console.WriteLine("🌐 Node/npm not found locally. Installing Node...");

            // Synchronously wait for download/install
            Bootstrapper.DownloadAsync(this).GetAwaiter().GetResult();

            // Re-check after installation
            exePath = GetExecutablePath();
            if (exePath == null)
            {
                Console.WriteLine("❌ Failed to install Node/npm.");
                return;
            }
        }

        if (args.Length >= 2 && args[0] == "add")
        {
            var pkg = args[1];
            Console.WriteLine($"🚀 UPI Forwarding: npm install {pkg}");
            ProcessRunner.Run(exePath, $"install {pkg}");
            return;
        }

        Console.WriteLine("❌ Unknown Node command.");
    }
}
