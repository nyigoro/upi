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
        var policy = EnginePolicyResolver.GetPolicy();

        var local = GetLocalPythonPath();
        var global = GetGlobalPythonPath();

        return policy switch
        {
            EnginePolicy.LocalOnly => local,
            EnginePolicy.PreferSystem => global ?? local,
            _ => local ?? global
        };
    }

    private static string? GetLocalPythonPath()
    {
        if (OperatingSystem.IsWindows())
        {
            var pythonExe = Path.Combine(UpiPaths.PythonEngine, "python.exe");
            return File.Exists(pythonExe) ? pythonExe : null;
        }

        var python3 = Path.Combine(UpiPaths.PythonEngine, "bin", "python3");
        if (File.Exists(python3))
            return python3;

        var pythonBin = Path.Combine(UpiPaths.PythonEngine, "bin", "python");
        return File.Exists(pythonBin) ? pythonBin : null;
    }

    private static string? GetGlobalPythonPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return PathHelper.FindExecutable("python")
                ?? PathHelper.FindExecutable("python3")
                ?? PathHelper.FindExecutable("py");
        }

        return PathHelper.FindExecutable("python3")
            ?? PathHelper.FindExecutable("python");
    }

    public void Execute(string[] args)
    {
        var python = GetExecutablePath();
        if (python == null)
        {
            Console.WriteLine("❌ Python not found.");
            return;
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

