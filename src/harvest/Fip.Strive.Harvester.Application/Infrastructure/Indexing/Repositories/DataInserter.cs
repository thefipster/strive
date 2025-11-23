using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories;

public class DataInserter(IDbContextFactory<IndexContext> dbContextFactory)
{
    public async Task<int> BulkInsert(CancellationToken ct, List<DataIndex> items)
    {
        if (items.Count == 0)
            return 0;

        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var incomingKeys = items.Select(index => index.Filepath).Distinct().ToArray();

        var existingKeys = await ctx
            .Data.AsNoTracking()
            .Where(z => incomingKeys.Contains(z.Filepath))
            .Select(z => z.Filepath)
            .ToListAsync(ct);

        var existing = new HashSet<string>(existingKeys, StringComparer.Ordinal);

        var toInsert = items.Where(i => !existing.Contains(i.Filepath)).ToList();
        if (toInsert.Count == 0)
            return 0;

        ctx.Data.AddRange(toInsert);
        await ctx.SaveChangesAsync(ct);

        return toInsert.Count;
    }
}
