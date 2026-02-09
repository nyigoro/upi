using UPI.Adapters;

namespace UPI.Core;

public static class Bootstrapper
{
    public static async Task DownloadAsync(IEngineAdapter adapter)
    {
        if (adapter is NodeAdapter)
        {
            Console.WriteLine("🌍 Bootstrapping Node.js...");
            await NodeInstaller.EnsureInstalledAsync();
            return;
        }

        if (adapter is PythonAdapter)
        {
            Console.WriteLine("🌍 Bootstrapping Python...");
            await PythonInstaller.EnsureInstalledAsync();
            return;
        }

        Console.WriteLine("⚠️ Bootstrapper not implemented for this adapter yet.");
    }
}
