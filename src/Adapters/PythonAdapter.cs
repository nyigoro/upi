using UPI.Core;

namespace UPI.Adapters;

public class PythonAdapter : IEngineAdapter
{
    public string Name => "Python (pip)";
    public string[] ProjectFiles => ["requirements.txt", "pyproject.toml"];
    public string DownloadUrl => "https://github.com/astral-sh/python-build-standalone/releases";

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
        if (OperatingSystem.IsWindows())
        {
            var python = Path.Combine(UpiPaths.PythonEngine, "python.exe");
            return File.Exists(python) ? python : null;
        }

        var python3 = Path.Combine(UpiPaths.PythonEngine, "bin", "python3");
        if (File.Exists(python3))
            return python3;

        var python = Path.Combine(UpiPaths.PythonEngine, "bin", "python");
        return File.Exists(python) ? python : null;
    }

    public void Execute(string[] args)
    {
        var python = GetExecutablePath();

        if (python == null)
        {
            Console.WriteLine("Python not found locally. Installing Python...");

            // Synchronously wait for download/install
            Bootstrapper.DownloadAsync(this).GetAwaiter().GetResult();

            // Re-check after installation
            python = GetExecutablePath();
            if (python == null)
            {
                Console.WriteLine("Failed to install Python.");
                return;
            }
        }

        if (args.Length >= 2 && args[0] == "add")
        {
            var pkg = args[1];
            Console.WriteLine($"🚀 UPI Forwarding: pip install {pkg}");
            ProcessRunner.Run(python, $"-m pip install {pkg}");
            return;
        }

        Console.WriteLine("❌ Unknown Python command.");
    }
}
