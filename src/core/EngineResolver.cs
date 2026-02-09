using UPI.Adapters;

namespace UPI.Core;

public static class EngineResolver
{
    public static IEngineAdapter? ResolveAdapter(string currentDir, List<IEngineAdapter> adapters)
    {
        foreach (var adapter in adapters)
        {
            if (adapter.DetectProject(currentDir))
                return adapter;
        }

        return null;
    }
}