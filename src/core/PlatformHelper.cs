using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UPI.Core;

public static class PlatformHelper
{
    public static string GetNodePlatform()
    {
        if (OperatingSystem.IsWindows()) return "win";
        if (OperatingSystem.IsLinux()) return "linux";
        if (OperatingSystem.IsMacOS()) return "darwin";

        throw new NotSupportedException("Unsupported OS platform.");
    }

    public static string GetNodeArch()
    {
    return RuntimeInformation.ProcessArchitecture switch
        {
        Architecture.X64 => "x64",
        Architecture.Arm64 => "arm64",
        Architecture.X86 => "x86",
        _ => throw new NotSupportedException("Unsupported architecture")
         };
    }


    // This is the missing piece!
    public static void SetExecutablePermission(string filePath)
    {
        if (!OperatingSystem.IsWindows())
        {
            try
            {
                // Run 'chmod +x' to allow the file to run on Linux/macOS
                Process.Start("chmod", $"+x \"{filePath}\"").WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Warning: Could not set permissions for {filePath}: {ex.Message}");
            }
        }
    }
}