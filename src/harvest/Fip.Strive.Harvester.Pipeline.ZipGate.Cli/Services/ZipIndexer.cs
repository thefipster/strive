using System.Text.Json;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Domain.Indexes;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Services;

public class ZipIndexer(IConnectionMultiplexer redis) : IFullIndexer<ZipIndex>
{
    private readonly IDatabase _db = redis.GetDatabase();

    private const string HashSetKey = "strive:harvest:index:zip";
    private const string DirtyListKey = "strive:harvest:index_dirty:zip";

    public async Task<bool> HashExistsAsync(string hash) =>
        await _db.SetContainsAsync(HashSetKey, hash);

    public async Task SetHashAsync(ZipIndex entry) => await _db.SetAddAsync(HashSetKey, entry.Hash);

    public async Task SetFileAsync(ZipIndex entry)
    {
        var json = JsonSerializer.Serialize(entry);
        await _db.ListLeftPushAsync(DirtyListKey, json);
    }
}
