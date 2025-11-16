using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.Assimilate.Cli.Services;

public class ExtractIndexWriter(IConnectionMultiplexer redis) : ISetHashIndex<ExtractIndex>
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task SetHashAsync(ExtractIndex entry) =>
        await _db.ListLeftPushAsync(IndexDeclarations.ExtractDirtyListKey, entry.ToJson());
}
