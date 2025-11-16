using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Storage.Contracts;

public interface IInventory
{
    public Task UpsertAsync(DateEntry entry);

    public Task<IEnumerable<int>> GetYearsAsync();

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(int year);

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(int year, int month);

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(int year, int month, int day);

    public Task<IEnumerable<DateEntry>> GetEntriesAsync(
        DateTime inclusiveStart,
        DateTime exclusiveEnd
    );
}
