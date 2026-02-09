using UPI.Adapters;
using UPI.Core;

var forwardArgs = ParseArgs(args);

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

var policy = EnginePolicyResolver.GetPolicy();
Console.WriteLine($"🔧 Engine policy: {policy}");

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

var comparison = OperatingSystem.IsWindows()
    ? StringComparison.OrdinalIgnoreCase
    : StringComparison.Ordinal;

var isLocal = exePath.StartsWith(UpiPaths.NodeEngine, comparison)
    || exePath.StartsWith(UpiPaths.PythonEngine, comparison);

var source = isLocal ? "local" : "system";
Console.WriteLine($"🔍 Engine source: {source} ({exePath})");

// Now Node/Python is installed, forward the command
adapter.Execute(forwardArgs);

static string[] ParseArgs(string[] args)
{
    var forwarded = new List<string>();

    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];

        if (TryConsumeValue(args, ref i, "--engine-policy", out var policy))
        {
            if (!string.IsNullOrWhiteSpace(policy))
                Environment.SetEnvironmentVariable("UPI_ENGINE_POLICY", policy);
            continue;
        }

        if (TryConsumeValue(args, ref i, "--engines-dir", out var enginesDir))
        {
            if (!string.IsNullOrWhiteSpace(enginesDir))
                Environment.SetEnvironmentVariable("UPI_ENGINES_DIR", enginesDir);
            continue;
        }

        forwarded.Add(arg);
    }

    return forwarded.ToArray();
}

static bool TryConsumeValue(string[] args, ref int index, string name, out string value)
{
    var arg = args[index];

    if (arg.StartsWith(name + "=", StringComparison.Ordinal))
    {
        value = arg.Substring(name.Length + 1);
        return true;
    }

    if (arg == name)
    {
        if (index + 1 >= args.Length)
        {
            Console.WriteLine($"❌ {name} requires a value.");
            Environment.Exit(1);
        }

        value = args[++index];
        return true;
    }

    value = string.Empty;
    return false;
}
