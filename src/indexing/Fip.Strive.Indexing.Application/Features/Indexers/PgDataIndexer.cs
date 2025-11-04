using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Domain;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class PgDataIndexer(IndexPgContext context) : IIndexer<DataIndex, string>
{
    public DataIndex? Find(string hash) => context.Data.Find(hash);

    public void Upsert(DataIndex index)
    {
        var existing = context.Data.FirstOrDefault(d => d.Hash == index.Hash);

        if (existing == null)
            Insert(index);
        else
            Update(index, existing);

        context.SaveChanges();
    }

    private void Insert(DataIndex index) => context.Data.Add(index);

    private void Update(DataIndex index, DataIndex existing)
    {
        context.Entry(existing).CurrentValues.SetValues(index);
    }
}
