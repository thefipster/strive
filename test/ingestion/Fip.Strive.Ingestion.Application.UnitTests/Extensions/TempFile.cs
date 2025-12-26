using Fip.Strive.Ingestion.Domain.Components;

namespace Fip.Strive.Ingestion.Application.UnitTests.Extensions;

public sealed class TempFile : IDisposable
{
    public string Filepath { get; }

    public TempFile(string path)
    {
        Filepath = path;
    }

    public FileProbe GetProbe() => new(Filepath);

    public void Dispose()
    {
        try
        {
            if (File.Exists(Filepath))
                File.Delete(Filepath);
        }
        catch
        {
            // Intentionally ignore cleanup errors in tests
        }
    }

    public static TempFile Create(string content, string extension = ".tmp")
    {
        var tempPath = Path.GetTempPath();
        var filename = $"{Guid.NewGuid()}{extension}";
        var path = Path.Combine(tempPath, filename);
        File.WriteAllText(path, content);
        return new TempFile(path);
    }
}
