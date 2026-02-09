using UPI.Core;

namespace UPI.Adapters;

public class PythonAdapter : IEngineAdapter
{
    public string Name => "Python (pip)";
    public string[] ProjectFiles => ["requirements.txt", "pyproject.toml"];
    public string DownloadUrl => "https://www.python.org/downloads/";

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
        // pip is often pip3 on Linux/macOS
        var globalPip = PathHelper.FindExecutable("pip") ?? PathHelper.FindExecutable("pip3");
        if (globalPip != null)
            return globalPip;

        var localPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".upi",
            "bin",
            OperatingSystem.IsWindows() ? "pip.exe" : "pip"
        );

        return File.Exists(localPath) ? localPath : null;
    }

    public void Execute(string[] args)
    {
        var exePath = GetExecutablePath();
        if (exePath == null)
        {
            Console.WriteLine("❌ pip not found.");
            return;
        }

        if (args.Length >= 2 && args[0] == "add")
        {
            var pkg = args[1];
            Console.WriteLine($"🚀 UPI Forwarding: pip install {pkg}");
            ProcessRunner.Run(exePath, $"install {pkg}");
            return;
        }

        Console.WriteLine("❌ Unknown Python command.");
    }
}