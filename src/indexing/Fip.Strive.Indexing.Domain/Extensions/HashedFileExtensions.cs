using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Domain.Extensions;

public static class HashedFileExtensions
{
    public static void Add(this ICollection<FileHashed> files, string path, string hash)
    {
        var file = new FileHashed
        {
            FileName = path,
            IndexedAt = DateTime.UtcNow,
            Hash = hash,
        };

        files.Add(file);
    }

    public static void Add(this ICollection<ZipHashed> files, string path, string hash)
    {
        var file = new ZipHashed
        {
            FileName = path,
            IndexedAt = DateTime.UtcNow,
            Hash = hash,
        };

        files.Add(file);
    }
}
