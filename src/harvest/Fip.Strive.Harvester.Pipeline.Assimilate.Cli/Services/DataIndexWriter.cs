using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.Assimilate.Cli.Services;

public class DataIndexWriter(IConnectionMultiplexer redis) : ISetNameIndex<DataIndex>
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task SetFileAsync(DataIndex entry) =>
        await _db.ListLeftPushAsync(IndexDeclarations.DataDirtyListKey, entry.ToJson());
}
