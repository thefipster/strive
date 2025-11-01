using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface IInventory
{
    public void Upsert(DateEntry entry);

    public IEnumerable<DateEntry> Get(int year);

    public IEnumerable<DateEntry> Get(int year, int month);

    public IEnumerable<DateEntry> Get(int year, int month, int day);

    public IEnumerable<DateEntry> Get(DateTime inclusiveStart, DateTime exclusiveEnd);
}
