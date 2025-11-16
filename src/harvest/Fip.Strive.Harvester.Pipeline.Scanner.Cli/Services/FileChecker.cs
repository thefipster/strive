using System.Text.Json;
using Fip.Strive.Harvester.Domain.Indexes;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.Scanner.Cli.Services;

public class FileChecker(IConnectionMultiplexer redis) : IScanIndexer
{
    private readonly IDatabase _db = redis.GetDatabase();

    private const string HashSetKey = "strive:harvest:index:file";
    private const string DirtyListKey = "strive:harvest:index_dirty:file";

    public async Task<bool> HashExistsAsync(string hash) =>
        await _db.SetContainsAsync(HashSetKey, hash);

    public async Task SetFileAsync(FileInstance entry)
    {
        var json = JsonSerializer.Serialize(entry);
        await _db.ListLeftPushAsync(DirtyListKey, json);
    }
}
