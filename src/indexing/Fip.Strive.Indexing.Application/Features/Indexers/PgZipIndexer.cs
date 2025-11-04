using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class PgZipIndexer(IndexPgContext context) : IIndexer<ZipIndex, string>
{
    public ZipIndex? Find(string hash) => context.Zips.Find(hash);

    public void Upsert(ZipIndex index)
    {
        var existing = context.Zips.Include(z => z.Files).FirstOrDefault(z => z.Hash == index.Hash);

        if (existing == null)
            Insert(index);
        else
            Update(index, existing);

        context.SaveChanges();
    }

    private void Insert(ZipIndex index) => context.Zips.Add(index);

    private void Update(ZipIndex index, ZipIndex existing)
    {
        context.Entry(existing).CurrentValues.SetValues(index);

        existing.Files.Clear();
        foreach (var file in index.Files)
            existing.Files.Add(file);
    }
}
