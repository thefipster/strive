using System.Text.Json;
using Fip.Strive.Harvester.Domain.Indexes;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli;

public class HashIndexer(IConnectionMultiplexer redis) : IHashIndexer
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task UpsertAsync(ZipIndexV2 entry)
    {
        var json = JsonSerializer.Serialize(entry);
        await _db.StringSetAsync($"strive:harvest:index:zip:hash:{entry.Hash}", json);
        await _db.ListRightPushAsync("strive:harvest:index_dirty:zip:hash", entry.Hash);
    }

    public async Task<ZipIndexV2?> GetAsync(string hash)
    {
        var val = await _db.StringGetAsync($"strive:harvest:index:zip:hash:{hash}");
        return val.IsNullOrEmpty ? null : JsonSerializer.Deserialize<ZipIndexV2>(val.ToString());
    }
}
