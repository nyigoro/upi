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

// Try to get the executable
var exePath = adapter.GetExecutablePath();

if (string.IsNullOrEmpty(exePath))
{
    Console.WriteLine($"⚠️ {adapter.Name} not found.");
    Console.Write("Install portable version? (y/n): ");

    if (Console.ReadKey().Key == ConsoleKey.Y)
    {
        Console.WriteLine();
        // ✅ Bootstrap Node or Python
        await Bootstrapper.DownloadAsync(adapter);

        // Retry getting executable path
        exePath = adapter.GetExecutablePath();
        if (string.IsNullOrEmpty(exePath))
        {
            Console.WriteLine($"❌ Failed to install {adapter.Name}.");
            return;
        }
    }
    else
    {
        return;
    }
}

// Now Node/Python is installed, forward the command
adapter.Execute(args);
