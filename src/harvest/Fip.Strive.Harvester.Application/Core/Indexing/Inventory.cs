using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Core.Indexing;

public class Inventory : IInventory
{
    private readonly ILiteCollection<DateEntry> _collection;

    public Inventory(IndexContext context)
    {
        _collection = context.GetCollection<DateEntry>();

        _collection.EnsureIndex(x => x.Year);
        _collection.EnsureIndex(x => x.Month);
        _collection.EnsureIndex(x => x.Day);
        _collection.EnsureIndex(x => x.Date);
    }

    public void Upsert(DateEntry entry) => _collection.Upsert(entry);

    public IEnumerable<DateEntry> Get(int year) => _collection.Find(x => x.Year == year);

    public IEnumerable<DateEntry> Get(int year, int month) =>
        _collection.Find(x => x.Year == year && x.Month == month);

    public IEnumerable<DateEntry> Get(int year, int month, int day) =>
        _collection.Find(x => x.Year == year && x.Month == month && x.Day == day);

    public IEnumerable<DateEntry> Get(DateTime inclusiveStart, DateTime exclusiveEnd) =>
        _collection.Find(x => x.Date >= inclusiveStart && x.Date < exclusiveEnd);
}
