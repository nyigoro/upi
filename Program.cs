using UPI.Adapters;
using UPI.Core;

var adapters = new List<IEngineAdapter>
{
    new NodeAdapter(),
    new PythonAdapter()
};

var currentDir = Directory.GetCurrentDirectory();
var adapter = EngineResolver.ResolveAdapter(currentDir, adapters);

if (adapter == null)
{
    Console.WriteLine("❌ No supported project detected.");
    return;
}

// Attempt to get executable path
var exePath = adapter.GetExecutablePath();

// If executable not found
if (string.IsNullOrEmpty(exePath))
{
    Console.WriteLine($"⚠️ {adapter.Name} not found locally. Installing portable version...");
    
    // Automatically download & install
    await Bootstrapper.DownloadAsync(adapter);

    // Re-check after installation
    exePath = adapter.GetExecutablePath();
    if (string.IsNullOrEmpty(exePath))
    {
        Console.WriteLine($"❌ Failed to install {adapter.Name}.");
        return;
    }
}

// Forward arguments to adapter
adapter.Execute(args);
