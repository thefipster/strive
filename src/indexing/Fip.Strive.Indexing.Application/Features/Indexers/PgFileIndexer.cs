using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class PgFileIndexer(IndexPgContext context) : IIndexer<FileIndex, string>
{
    public async Task<FileIndex?> FindAsync(string hash) =>
        await context.Files.AsNoTracking().FirstOrDefaultAsync(file => file.Hash == hash);

    public async Task UpsertAsync(FileIndex index)
    {
        var existing = context
            .Files.Include(i => i.Files)
            .FirstOrDefault(file => file.Hash == index.Hash);

        if (existing == null)
            await InsertAsync(index);
        else
            await UpdateAsync(index, existing);

        await context.SaveChangesAsync();
    }

    private async Task InsertAsync(FileIndex index) => await context.Files.AddAsync(index);

    private Task UpdateAsync(FileIndex index, FileIndex existing)
    {
        context.Entry(existing).CurrentValues.SetValues(index);
        return Task.CompletedTask;
    }
}
