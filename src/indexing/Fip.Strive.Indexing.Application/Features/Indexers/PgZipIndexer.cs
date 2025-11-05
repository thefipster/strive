using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class PgZipIndexer(IndexPgContext context) : IIndexer<ZipIndex, string>
{
    public async Task<ZipIndex?> FindAsync(string hash) => await context.Zips.FindAsync(hash);

    public async Task UpsertAsync(ZipIndex index)
    {
        var existing = context.Zips.Include(z => z.Files).FirstOrDefault(z => z.Hash == index.Hash);

        if (existing == null)
            await InsertAsync(index);
        else
            await UpdateAsync(index, existing);
    }

    private async Task InsertAsync(ZipIndex index)
    {
        var files = index.Files.ToArray();
        index.Files.Clear();

        context.Zips.Add(index);
        await context.SaveChangesAsync();

        foreach (var file in files)
            index.Files.Add(file);

        await context.SaveChangesAsync();
    }

    private async Task UpdateAsync(ZipIndex index, ZipIndex existing)
    {
        context.Entry(existing).CurrentValues.SetValues(index);

        existing.Files.Clear();
        foreach (var file in index.Files)
            existing.Files.Add(file);

        await context.SaveChangesAsync();
    }
}
