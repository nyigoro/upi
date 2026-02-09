namespace UPI.Core;

public static class NodeReleaseResolver
{
    public static string Version = "v22.14.0";

    public static string GetDownloadUrl()
    {
        var platform = PlatformHelper.GetNodePlatform();
        var arch = PlatformHelper.GetNodeArch();

        string fileName = platform switch
        {
            "win"    => $"node-{Version}-win-{arch}.zip",
            "linux"  => $"node-{Version}-linux-{arch}.tar.gz",
            "darwin" => $"node-{Version}-darwin-{arch}.tar.gz",
            _ => throw new NotSupportedException($"Unsupported platform: {platform}")
        };

        return $"https://nodejs.org/dist/{Version}/{fileName}";
    }
}
