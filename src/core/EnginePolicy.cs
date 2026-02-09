namespace UPI.Core;

public enum EnginePolicy
{
    PreferLocal,
    PreferSystem,
    LocalOnly
}

public static class EnginePolicyResolver
{
    public static EnginePolicy GetPolicy()
    {
        var value = Environment.GetEnvironmentVariable("UPI_ENGINE_POLICY");
        if (string.IsNullOrWhiteSpace(value))
            return EnginePolicy.PreferLocal;

        switch (value.Trim().ToLowerInvariant())
        {
            case "prefer-local":
            case "preferlocal":
            case "local-prefer":
            case "local":
                return EnginePolicy.PreferLocal;
            case "prefer-system":
            case "prefersystem":
            case "system":
                return EnginePolicy.PreferSystem;
            case "local-only":
            case "localonly":
            case "portable-only":
            case "portable":
                return EnginePolicy.LocalOnly;
            default:
                return EnginePolicy.PreferLocal;
        }
    }
}
