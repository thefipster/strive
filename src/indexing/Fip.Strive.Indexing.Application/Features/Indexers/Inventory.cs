using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;
using LiteDB;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

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

    public IEnumerable<int> GetYears() =>
        _collection
            .Query()
            .Select(entry => entry.Year)
            .ToEnumerable()
            .Distinct()
            .OrderBy(year => year);

    public IEnumerable<DateEntry> GetEntries(int year) =>
        _collection.Find(entry => entry.Year == year).OrderBy(entry => entry.Timestamp);

    public IEnumerable<DateEntry> GetEntries(int year, int month) =>
        _collection
            .Find(entry => entry.Year == year && entry.Month == month)
            .OrderBy(entry => entry.Timestamp);

    public IEnumerable<DateEntry> GetEntries(int year, int month, int day) =>
        _collection
            .Find(entry => entry.Year == year && entry.Month == month && entry.Day == day)
            .OrderBy(entry => entry.Timestamp);

    public IEnumerable<DateEntry> GetEntries(DateTime inclusiveStart, DateTime exclusiveEnd) =>
        _collection
            .Find(entry => entry.Date >= inclusiveStart && entry.Date < exclusiveEnd)
            .OrderBy(entry => entry.Timestamp);
}
