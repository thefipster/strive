using TheFipster.ActivityAggregator.Domain.Models;

namespace TheFipster.ActivityAggregator.Storage.Abstractions;

public interface IInventoryService
{
    void Update(MergedRecord record);

    IEnumerable<Inventory> Get(int year, int? month, int? day);
}
