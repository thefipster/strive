using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories;

public class ExtractInserter(IDbContextFactory<IndexContext> dbContextFactory)
{
    public async Task<int> BulkInsert(CancellationToken ct, List<ExtractIndex> items)
    {
        if (items.Count == 0)
            return 0;

        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var incomingKeys = items.Select(index => index.Hash).Distinct().ToArray();

        var existingKeys = await ctx
            .Extracts.AsNoTracking()
            .Where(z => incomingKeys.Contains(z.Hash))
            .Select(z => z.Hash)
            .ToListAsync(ct);

        var existing = new HashSet<string>(existingKeys, StringComparer.Ordinal);

        var toInsert = items.Where(i => !existing.Contains(i.Hash)).ToList();
        if (toInsert.Count == 0)
            return 0;

        ctx.Extracts.AddRange(toInsert);
        await ctx.SaveChangesAsync(ct);

        return toInsert.Count;
    }
}
