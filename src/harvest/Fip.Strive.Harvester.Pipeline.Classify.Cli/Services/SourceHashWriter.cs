using System.Text.Json;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Defaults;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
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
