using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class PgZipIndexer(IndexPgContext context) : IIndexer<ZipIndex, string>
{
    public async Task<ZipIndex?> FindAsync(string hash) =>
        await context.Zips.AsNoTracking().FirstOrDefaultAsync(zip => zip.Hash == hash);

    public async Task UpsertAsync(ZipIndex index)
    {
        var existing = context
            .Zips.Include(zip => zip.Files)
            .FirstOrDefault(z => z.Hash == index.Hash);

        if (existing == null)
            await InsertAsync(index);
        else
            await UpdateAsync(index, existing);

        await context.SaveChangesAsync();
    }

    private async Task InsertAsync(ZipIndex index) => await context.Zips.AddAsync(index);

    private Task UpdateAsync(ZipIndex index, ZipIndex existing)
    {
        context.Entry(existing).CurrentValues.SetValues(index);
        return Task.CompletedTask;
    }
}
