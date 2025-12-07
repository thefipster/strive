using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories;

public class SourceInserter(IndexContext context)
{
    public async Task<int> BulkInsert(CancellationToken ct, List<SourceIndex> items)
    {
        if (items.Count == 0)
            return 0;

        var incomingKeys = items.Select(index => index.Hash).Distinct().ToArray();

        var existingKeys = await context
            .Sources.AsNoTracking()
            .Where(z => incomingKeys.Contains(z.Hash))
            .Select(z => z.Hash)
            .ToListAsync(ct);

        var existing = new HashSet<string>(existingKeys, StringComparer.Ordinal);

        var toInsert = items.Where(i => !existing.Contains(i.Hash)).ToList();
        if (toInsert.Count == 0)
            return 0;

        context.Sources.AddRange(toInsert);
        await context.SaveChangesAsync(ct);

        return toInsert.Count;
    }
}
