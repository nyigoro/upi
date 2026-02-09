namespace UPI.Core;

public static class UpiPaths
{
    public static string Root =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".upi");

    public static string Engines =>
        Path.Combine(Root, "engines");

    public static string Cache =>
        Path.Combine(Root, "cache");

    public static string NodeEngine =>
        Path.Combine(Engines, "node");

    public static string PythonEngine =>
        Path.Combine(Engines, "python");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(Engines);
        Directory.CreateDirectory(Cache);
        Directory.CreateDirectory(NodeEngine);
        Directory.CreateDirectory(PythonEngine);
    }
}
