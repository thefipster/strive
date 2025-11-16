using Fip.Strive.Indexing.Application.Contexts;
using Fip.Strive.Indexing.Application.Repositories.Contracts;
using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Application.Repositories;

public class FileRepository(FileContext context) : IFileRepository
{
    public Task<bool> ExistsAsync(string hash)
    {
        var result = context.Files.Any(x => x.Hash == hash);
        return Task.FromResult(result);
    }

    public Task<FileMeta?> GetInfoAsync(string hash)
    {
        var result = context.Files.Where(x => x.Hash == hash).ToArray();
        if (result.Length == 0)
            return Task.FromResult<FileMeta?>(null);

        var meta = new FileMeta
        {
            Hash = hash,
            ClassificationHashes = result.Select(x => x.ClassificationHash).ToList(),
            Files = result.Select(x => x.Filepath).ToList(),
        };

        return Task.FromResult<FileMeta?>(meta);
    }

    public Task SetAsync(FileIndexV2 index)
    {
        var existing = context.Files.FirstOrDefault(x => x.Filepath == index.Filepath);

        if (existing == null)
            context.Files.Add(index);
        else
            existing.ClassificationHash = index.ClassificationHash;

        return Task.CompletedTask;
    }
}
