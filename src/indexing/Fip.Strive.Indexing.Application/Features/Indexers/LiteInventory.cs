using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;
using LiteDB;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class LiteInventory : IInventory
{
    private readonly ILiteCollection<DateEntry> _collection;

    public LiteInventory(IndexLiteContext context)
    {
        _collection = context.GetCollection<DateEntry>();

        _collection.EnsureIndex(x => x.Year);
        _collection.EnsureIndex(x => x.Month);
        _collection.EnsureIndex(x => x.Day);
        _collection.EnsureIndex(x => x.Date);
    }

    public Task UpsertAsync(DateEntry entry)
    {
        _collection.Upsert(entry);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<int>> GetYearsAsync()
    {
        var result = _collection
            .Query()
            .Select(entry => entry.Year)
            .ToEnumerable()
            .Distinct()
            .OrderBy(year => year);

        return Task.FromResult<IEnumerable<int>>(result);
    }

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(int year)
    {
        var result = _collection
            .Find(entry => entry.Year == year)
            .OrderBy(entry => entry.Timestamp);

        return Task.FromResult<IEnumerable<DateEntry>>(result);
    }

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(int year, int month)
    {
        var result = _collection
            .Find(entry => entry.Year == year && entry.Month == month)
            .OrderBy(entry => entry.Timestamp);

        return Task.FromResult<IEnumerable<DateEntry>>(result);
    }

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(int year, int month, int day)
    {
        var result = _collection
            .Find(entry => entry.Year == year && entry.Month == month && entry.Day == day)
            .OrderBy(entry => entry.Timestamp);

        return Task.FromResult<IEnumerable<DateEntry>>(result);
    }

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(
        DateTime inclusiveStart,
        DateTime exclusiveEnd
    )
    {
        var result = _collection
            .Find(entry => entry.Date >= inclusiveStart && entry.Date < exclusiveEnd)
            .OrderBy(entry => entry.Timestamp);

        return Task.FromResult<IEnumerable<DateEntry>>(result);
    }
}
