using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Indexing.Application.Features.Contracts;

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
