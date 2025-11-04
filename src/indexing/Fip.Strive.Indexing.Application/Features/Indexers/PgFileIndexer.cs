using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Domain;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class PgFileIndexer(IndexPgContext context) : IIndexer<FileIndex, string>
{
    public FileIndex? Find(string hash) => context.Files.Find(hash);

    public void Upsert(FileIndex index)
    {
        var existing = context.Files.FirstOrDefault(f => f.Hash == index.Hash);

        if (existing == null)
            Insert(index);
        else
            Update(index, existing);

        context.SaveChanges();
    }

    private void Insert(FileIndex index) => context.Files.Add(index);

    private void Update(FileIndex index, FileIndex existing)
    {
        context.Entry(existing).CurrentValues.SetValues(index);
    }
}
