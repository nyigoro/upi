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
        // Local UPI cache only (portable)
        string relativePath = OperatingSystem.IsWindows()
            ? "npm.cmd"
            : "bin/npm";

        var localNpm = Path.Combine(UpiPaths.NodeEngine, relativePath);

        var localNode = OperatingSystem.IsWindows()
            ? Path.Combine(UpiPaths.NodeEngine, "node.exe")
            : Path.Combine(UpiPaths.NodeEngine, "bin", "node");

        if (!File.Exists(localNpm) || !File.Exists(localNode))
            return null;

        EnsureLocalNodeOnPath();
        return localNpm;
    }

    private static void EnsureLocalNodeOnPath()
    {
        var nodePath = OperatingSystem.IsWindows()
            ? UpiPaths.NodeEngine
            : Path.Combine(UpiPaths.NodeEngine, "bin");

        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        if (PathContains(currentPath, nodePath))
            return;

        Environment.SetEnvironmentVariable("PATH", $"{nodePath}{Path.PathSeparator}{currentPath}");
    }

    private static bool PathContains(string pathList, string path)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        foreach (var entry in pathList.Split(Path.PathSeparator))
        {
            if (string.Equals(entry.Trim(), path, comparison))
                return true;
        }

        return false;
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
