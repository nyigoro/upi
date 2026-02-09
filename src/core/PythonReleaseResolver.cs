using System.Net.Http;
using System.Text.Json;

namespace UPI.Core;

public static class PythonReleaseResolver
{
    public static string DefaultVersion = "3.12";

    private const string LatestReleaseMetadataUrl =
        "https://raw.githubusercontent.com/astral-sh/python-build-standalone/latest-release/latest-release.json";

    private const string GitHubReleaseApiBase =
        "https://api.github.com/repos/astral-sh/python-build-standalone/releases/tags/";

    public static async Task<string> GetDownloadUrlAsync()
    {
        var overrideUrl = Environment.GetEnvironmentVariable("UPI_PYTHON_URL");
        if (!string.IsNullOrWhiteSpace(overrideUrl))
            return overrideUrl;

        var requested = Environment.GetEnvironmentVariable("UPI_PYTHON_VERSION");
        if (string.IsNullOrWhiteSpace(requested))
            requested = DefaultVersion;

        var tag = await GetLatestReleaseTagAsync();
        var assets = await GetReleaseAssetsAsync(tag);

        var targetCandidates = GetTargetCandidates();
        var filtered = assets
            .Where(a => IsSupportedAsset(a.Name, targetCandidates))
            .ToList();

        if (filtered.Count == 0)
            throw new InvalidOperationException("No matching portable Python build found for this platform.");

        var selected = SelectVersion(filtered, requested);
        if (selected == null)
            throw new InvalidOperationException($"No portable Python build found for version '{requested}'.");

        return selected.DownloadUrl;
    }

    private static bool IsSupportedAsset(string name, List<string> targetCandidates)
    {
        if (!name.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!name.Contains("-install_only", StringComparison.Ordinal))
            return false;

        foreach (var target in targetCandidates)
        {
            if (name.Contains(target, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static ReleaseAsset? SelectVersion(List<ReleaseAsset> assets, string requestedVersion)
    {
        var hasPatch = requestedVersion.Split('.', StringSplitOptions.RemoveEmptyEntries).Length >= 3;

        if (hasPatch && Version.TryParse(requestedVersion, out var exact))
        {
            return assets
                .Where(a => TryParseAssetVersion(a.Name, out var v) && v == exact)
                .OrderByDescending(a => a.Name.Contains("install_only_stripped", StringComparison.Ordinal))
                .FirstOrDefault();
        }

        if (!Version.TryParse(requestedVersion, out var requested))
            throw new InvalidOperationException($"Invalid Python version '{requestedVersion}'. Use '3.12' or '3.12.7'.");

        var candidates = assets
            .Select(a => new
            {
                Asset = a,
                Version = TryParseAssetVersion(a.Name, out var v) ? v : null,
                IsStripped = a.Name.Contains("install_only_stripped", StringComparison.Ordinal)
            })
            .Where(x => x.Version != null && x.Version.Major == requested.Major && x.Version.Minor == requested.Minor)
            .OrderByDescending(x => x.Version)
            .ThenByDescending(x => x.IsStripped)
            .ToList();

        return candidates.FirstOrDefault()?.Asset;
    }

    private static bool TryParseAssetVersion(string name, out Version version)
    {
        version = new Version();
        if (!name.StartsWith("cpython-", StringComparison.Ordinal))
            return false;

        var plusIndex = name.IndexOf('+');
        if (plusIndex < 0)
            return false;

        var versionText = name.Substring("cpython-".Length, plusIndex - "cpython-".Length);
        return Version.TryParse(versionText, out version);
    }

    private static async Task<string> GetLatestReleaseTagAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("UPI");

        var json = await client.GetStringAsync(LatestReleaseMetadataUrl);
        using var doc = JsonDocument.Parse(json);
        var tag = doc.RootElement.GetProperty("tag").GetString();

        if (string.IsNullOrWhiteSpace(tag))
            throw new InvalidOperationException("Could not resolve python-build-standalone release tag.");

        return tag;
    }

    private static async Task<List<ReleaseAsset>> GetReleaseAssetsAsync(string tag)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("UPI");

        var json = await client.GetStringAsync($"{GitHubReleaseApiBase}{tag}");
        using var doc = JsonDocument.Parse(json);

        var assets = new List<ReleaseAsset>();
        foreach (var asset in doc.RootElement.GetProperty("assets").EnumerateArray())
        {
            var name = asset.GetProperty("name").GetString();
            var url = asset.GetProperty("browser_download_url").GetString();

            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(url))
                assets.Add(new ReleaseAsset(name!, url!));
        }

        return assets;
    }

    private static List<string> GetTargetCandidates()
    {
        var candidates = new List<string>();

        if (OperatingSystem.IsWindows())
        {
            candidates.AddRange(System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture switch
            {
                System.Runtime.InteropServices.Architecture.Arm64 => new[] { "aarch64-pc-windows-msvc-shared", "aarch64-pc-windows-msvc" },
                _ => new[] { "x86_64-pc-windows-msvc-shared", "x86_64-pc-windows-msvc" }
            });
        }
        else if (OperatingSystem.IsLinux())
        {
            candidates.AddRange(System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture switch
            {
                System.Runtime.InteropServices.Architecture.Arm64 => new[] { "aarch64-unknown-linux-gnu", "aarch64-unknown-linux-musl" },
                _ => new[] { "x86_64-unknown-linux-gnu", "x86_64-unknown-linux-musl" }
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            candidates.AddRange(System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture switch
            {
                System.Runtime.InteropServices.Architecture.Arm64 => new[] { "aarch64-apple-darwin", "arm64-apple-darwin" },
                _ => new[] { "x86_64-apple-darwin" }
            });
        }
        else
        {
            throw new NotSupportedException("Unsupported OS platform.");
        }

        return candidates;
    }

    private record ReleaseAsset(string Name, string DownloadUrl);
}



