using System.Text.Json;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.Classify.Cli.Services;

public class SourceHashWriter(IConnectionMultiplexer redis) : ISetNameIndex<SourceIndex>
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task SetFileAsync(SourceIndex entry)
    {
        await _db.ListLeftPushAsync(IndexDeclarations.SourceDirtyListKey, entry.ToJson());
    }
}
