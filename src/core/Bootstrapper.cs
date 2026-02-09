using UPI.Adapters;

namespace UPI.Core;

public static class Bootstrapper
{
    public static async Task DownloadAsync(IEngineAdapter adapter)
    {
        // Only NodeAdapter is currently supported
        if (adapter is NodeAdapter)
        {
            Console.WriteLine("🌍 Bootstrapping Node.js...");
            await NodeInstaller.EnsureInstalledAsync();
            return;
        }

        Console.WriteLine("⚠️ Bootstrapper not implemented for this adapter yet.");
    }
}
