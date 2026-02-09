namespace UPI.Core;

public static class PathHelper
{
    public static string? FindExecutable(string command)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return null;

        string[] extensions = OperatingSystem.IsWindows()
            ? [".exe", ".cmd", ".bat"]
            : [""];

        foreach (var path in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(path))
                continue;

            foreach (var ext in extensions)
            {
                var fullPath = Path.Combine(path.Trim(), command + ext);
                if (File.Exists(fullPath))
                    return fullPath;
            }
        }

        return null;
    }
}