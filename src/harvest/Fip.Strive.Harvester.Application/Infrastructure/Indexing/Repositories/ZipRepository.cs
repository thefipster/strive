using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories;

public class ZipRepository(IndexContext context) : IZipRepository
{
    public async Task<int> BulkInsert(CancellationToken ct, List<ZipIndex> items)
    {
        if (items.Count == 0)
            return 0;

        var incomingKeys = items.Select(index => index.Filepath).Distinct().ToArray();

        var existingKeys = await context
            .Zips.AsNoTracking()
            .Where(z => incomingKeys.Contains(z.Filepath))
            .Select(z => z.Filepath)
            .ToListAsync(ct);

        var existing = new HashSet<string>(existingKeys, StringComparer.Ordinal);

        var toInsert = items.Where(i => !existing.Contains(i.Filepath)).ToList();
        if (toInsert.Count == 0)
            return 0;

        context.Zips.AddRange(toInsert);
        await context.SaveChangesAsync(ct);

        return toInsert.Count;
    }

    public async Task<ZipIndex?> GetByHash(string hash) =>
        await context.Zips.AsNoTracking().FirstOrDefaultAsync(z => z.Hash == hash);

    public async Task<ZipIndex?> GetByFile(string filepath) =>
        await context.Zips.AsNoTracking().FirstOrDefaultAsync(z => z.Filepath == filepath);

    public async Task<PagedResponse<ZipIndex>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct
    )
    {
        if (pageNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        var query = context.Zips.AsNoTracking().AsQueryable();

        var count = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(z => z.CreatedAt)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<ZipIndex>(items, count);
    }
}
