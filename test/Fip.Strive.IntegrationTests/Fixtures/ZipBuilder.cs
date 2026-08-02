using System.IO.Compression;
using System.Text;

namespace Fip.Strive.IntegrationTests.Fixtures;

public static class ZipBuilder
{
    /// <summary>Writes an archive with the given <c>path -&gt; content</c> entries and returns its path.</summary>
    public static string Create(
        string directory,
        string fileName,
        params (string Path, string Content)[] entries
    )
    {
        Directory.CreateDirectory(directory);
        var archivePath = Path.Combine(directory, fileName);

        using var file = new FileStream(archivePath, FileMode.Create, FileAccess.Write);
        using var archive = new ZipArchive(file, ZipArchiveMode.Create);

        foreach (var (path, content) in entries)
        {
            var entry = archive.CreateEntry(path, CompressionLevel.NoCompression);
            using var stream = entry.Open();
            stream.Write(Encoding.UTF8.GetBytes(content));
        }

        return archivePath;
    }

    /// <summary>Adds an explicit directory entry, which carries no content and must be skipped.</summary>
    public static string CreateWithDirectoryEntry(
        string directory,
        string fileName,
        string directoryEntry,
        params (string Path, string Content)[] entries
    )
    {
        Directory.CreateDirectory(directory);
        var archivePath = Path.Combine(directory, fileName);

        using var file = new FileStream(archivePath, FileMode.Create, FileAccess.Write);
        using var archive = new ZipArchive(file, ZipArchiveMode.Create);

        archive.CreateEntry(directoryEntry);

        foreach (var (path, content) in entries)
        {
            var entry = archive.CreateEntry(path, CompressionLevel.NoCompression);
            using var stream = entry.Open();
            stream.Write(Encoding.UTF8.GetBytes(content));
        }

        return archivePath;
    }
}
