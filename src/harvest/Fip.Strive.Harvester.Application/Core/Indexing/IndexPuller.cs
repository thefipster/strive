using System.Text.Json;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Domain.Indexes;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Application.Core.Indexing;

public class IndexPuller(IConnectionMultiplexer redis) : IIndexPuller
{
    private readonly IDatabase _db = redis.GetDatabase();

    private const string DirtyListKey = "strive:harvest:index_dirty:zip";

    public async Task<ZipIndex?> GetFileAsync(ZipIndex entry)
    {
        var value = await _db.ListRightPopAsync(DirtyListKey);

        if (value.HasValue)
            return JsonSerializer.Deserialize<ZipIndex>(value.ToString());

        return null;
    }
}
