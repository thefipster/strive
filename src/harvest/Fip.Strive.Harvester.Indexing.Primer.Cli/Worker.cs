using Fip.Strive.Harvester.Application.Defaults;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Indexing.Primer.Cli;

public class Worker(IndexContext postgres, IConnectionMultiplexer redisConnection)
{
    private const int BatchSize = 1000;
    private readonly IDatabase _redis = redisConnection.GetDatabase();

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        await PrimeZips(ct);
        await PrimeFiles(ct);
    }

    private async Task PrimeZips(CancellationToken ct = default)
    {
        await PrimeAsync(
            query: created =>
                postgres
                    .Zips.AsNoTracking()
                    .Where(x => x.CreatedAt > created)
                    .OrderBy(z => z.CreatedAt),
            hash: z => (RedisValue)z.Hash,
            createdSelector: z => z.CreatedAt,
            redisKey: IndexDeclarations.ZipHashSetKey,
            ct: ct
        );
    }

    private async Task PrimeFiles(CancellationToken ct = default)
    {
        await PrimeAsync(
            query: created =>
                postgres
                    .Files.AsNoTracking()
                    .Where(f => f.CreatedAt > created)
                    .OrderBy(f => f.CreatedAt),
            hash: f => (RedisValue)f.Hash,
            createdSelector: f => f.CreatedAt,
            redisKey: IndexDeclarations.FileHashSetKey,
            ct: ct
        );
    }

    private async Task PrimeAsync<T>(
        Func<DateTime, IQueryable<T>> query,
        Func<T, RedisValue> hash,
        Func<T, DateTime> createdSelector,
        RedisKey redisKey,
        CancellationToken ct = default
    )
    {
        var lastCreated = DateTime.MinValue;

        while (!ct.IsCancellationRequested)
        {
            var page = await query(lastCreated).Take(BatchSize).ToListAsync(ct);
            if (page.Count == 0)
                break;

            var batch = page.Select(hash).ToArray();
            if (batch.Length > 0)
                await _redis.SetAddAsync(redisKey, batch);

            lastCreated = createdSelector(page[^1]);
        }
    }
}
