using System.Text.Json;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.Scanner.Cli.Services;

public class FileChecker(IConnectionMultiplexer redis) : IScanIndexer
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<bool> HashExistsAsync(string hash) =>
        await _db.SetContainsAsync(IndexDeclarations.FileHashSetKey, hash);

    public async Task SetFileAsync(FileInstance entry)
    {
        var json = JsonSerializer.Serialize(entry);
        await _db.ListLeftPushAsync(IndexDeclarations.FileDirtyListKey, json);
    }
}
