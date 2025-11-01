using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface IInventory
{
    public void Upsert(DateEntry entry);

    public IEnumerable<int> GetYears();

    public IEnumerable<DateEntry> GetEntries(int year);

    public IEnumerable<DateEntry> GetEntries(int year, int month);

    public IEnumerable<DateEntry> GetEntries(int year, int month, int day);

    public IEnumerable<DateEntry> GetEntries(DateTime inclusiveStart, DateTime exclusiveEnd);
}
