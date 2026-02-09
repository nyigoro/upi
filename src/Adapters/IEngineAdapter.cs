namespace UPI.Adapters;

public interface IEngineAdapter
{
    string Name { get; }
    string[] ProjectFiles { get; }
    string DownloadUrl { get; }

    bool DetectProject(string directory);
    string? GetExecutablePath();
    void Execute(string[] args);
}