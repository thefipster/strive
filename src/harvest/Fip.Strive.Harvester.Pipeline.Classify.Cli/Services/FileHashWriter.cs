using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.Classify.Cli.Services;

public class FileHashWriter(IConnectionMultiplexer redis) : ISetHashIndex<FileIndex>
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task SetHashAsync(FileIndex entry) =>
        await _db.SetAddAsync(IndexDeclarations.FileHashSetKey, entry.Hash);
}
