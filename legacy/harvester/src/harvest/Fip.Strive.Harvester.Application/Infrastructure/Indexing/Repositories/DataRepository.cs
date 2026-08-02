using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories;

public class DataRepository(IndexContext context) : IDataRepository
{
    public async Task<int> BulkInsert(CancellationToken ct, List<DataIndex> items)
    {
        if (items.Count == 0)
            return 0;

        var incomingKeys = items.Select(index => index.Filepath).Distinct().ToArray();

        var existingKeys = await context
            .Data.AsNoTracking()
            .Where(z => incomingKeys.Contains(z.Filepath))
            .Select(z => z.Filepath)
            .ToListAsync(ct);

        var existing = new HashSet<string>(existingKeys, StringComparer.Ordinal);

        var toInsert = items.Where(i => !existing.Contains(i.Filepath)).ToList();
        if (toInsert.Count == 0)
            return 0;

        context.Data.AddRange(toInsert);
        await context.SaveChangesAsync(ct);

        return toInsert.Count;
    }

    public async Task<PagedResponse<DataIndex>> GetPageAsync(
        string parentHash,
        int pageNumber,
        int pageSize,
        CancellationToken ct
    )
    {
        if (pageNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        var query = context.Data.AsNoTracking().Where(x => x.ParentHash == parentHash);

        var count = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(z => z.CreatedAt)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<DataIndex>(items, count);
    }
}
