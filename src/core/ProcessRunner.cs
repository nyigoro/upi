using System.Diagnostics;

namespace UPI.Core;

public static class ProcessRunner
{
    public static int Run(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo();

        if (OperatingSystem.IsWindows() && fileName.EndsWith(".cmd"))
        {
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/c \"\"{fileName}\" {arguments}\"";
        }
        else
        {
            startInfo.FileName = fileName;
            startInfo.Arguments = arguments;
        }

        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = false;

        var engineDir = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(engineDir))
        {
            var current = Environment.GetEnvironmentVariable("PATH") ?? "";
            startInfo.EnvironmentVariables["PATH"] =
                engineDir + Path.PathSeparator + current;
        }

        using var process = Process.Start(startInfo);
        process!.WaitForExit();
        return process.ExitCode;
    }
}
