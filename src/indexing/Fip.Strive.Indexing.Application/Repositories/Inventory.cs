using Fip.Strive.Indexing.Application.Contexts;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;

namespace Fip.Strive.Indexing.Application.Repositories;

public class Inventory(InventoryContext context) : IInventory
{
    public Task UpsertAsync(DateEntry entry)
    {
        var existing = context.Entries.FirstOrDefault(x => x.Date == entry.Date);

        if (existing == null)
            context.Entries.Add(entry);
        else
        {
            existing.Timestamp = entry.Timestamp;
            existing.Kind = entry.Kind;
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<int>> GetYearsAsync()
    {
        var years = context.Entries.Select(x => x.Year).Distinct().OrderBy(x => x);
        return Task.FromResult<IEnumerable<int>>(years);
    }

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(int year)
    {
        var entries = context.Entries.Where(x => x.Year == year).OrderBy(x => x.Timestamp);
        return Task.FromResult<IEnumerable<DateEntry>>(entries);
    }

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(int year, int month)
    {
        var entries = context
            .Entries.Where(x => x.Year == year && x.Month == month)
            .OrderBy(x => x.Timestamp);

        return Task.FromResult<IEnumerable<DateEntry>>(entries);
    }

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(int year, int month, int day)
    {
        var entries = context
            .Entries.Where(x => x.Year == year && x.Month == month && x.Day == day)
            .OrderBy(x => x.Timestamp);

        return Task.FromResult<IEnumerable<DateEntry>>(entries);
    }

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(
        DateTime inclusiveStart,
        DateTime exclusiveEnd
    )
    {
        var entries = context
            .Entries.Where(x => x.Timestamp >= inclusiveStart && x.Timestamp < exclusiveEnd)
            .OrderBy(x => x.Timestamp);

        return Task.FromResult<IEnumerable<DateEntry>>(entries);
    }
}
