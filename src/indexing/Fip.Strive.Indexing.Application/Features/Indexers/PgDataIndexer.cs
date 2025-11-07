using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class PgDataIndexer(IndexPgContext context) : IIndexer<DataIndex, string>
{
    public async Task<DataIndex?> FindAsync(string hash) =>
        await context.Data.AsNoTracking().FirstOrDefaultAsync(data => data.Hash == hash);

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
