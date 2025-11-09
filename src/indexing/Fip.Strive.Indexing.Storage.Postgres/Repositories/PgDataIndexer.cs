using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Fip.Strive.Indexing.Storage.Postgres.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Storage.Postgres.Repositories;

public class PgDataIndexer(IndexPgContext context) : IIndexer<DataIndex, string>
{
    public async Task<DataIndex?> FindAsync(string key) =>
        await context.Data.AsNoTracking().FirstOrDefaultAsync(data => data.Hash == key);

    public async Task UpsertAsync(DataIndex index)
    {
        var existing = await context.Data.FirstOrDefaultAsync(data => data.Hash == index.Hash);

        if (existing == null)
            await InsertAsync(index);
        else
            await UpdateAsync(index, existing);

        await context.SaveChangesAsync();
    }

    private async Task InsertAsync(DataIndex index) => await context.Data.AddAsync(index);

    private Task UpdateAsync(DataIndex index, DataIndex existing)
    {
        context.Entry(existing).CurrentValues.SetValues(index);
        return Task.CompletedTask;
    }
}
