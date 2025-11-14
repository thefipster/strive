using System.Text.Json;
using Fip.Strive.Harvester.Domain.Indexes;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli;

public class PathIndexer(IConnectionMultiplexer redis) : IPathIndexer
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task UpsertAsync(ZipIndexV2 entry)
    {
        var json = JsonSerializer.Serialize(entry);
        await _db.StringSetAsync($"strive:harvest:index:zip:path:{entry.Filepath}", json);
        await _db.ListRightPushAsync("strive:harvest:index_dirty:zip:path", entry.Filepath);
    }
}
