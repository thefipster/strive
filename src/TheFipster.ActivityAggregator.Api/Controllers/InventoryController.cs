using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController(IInventoryIndexer inventory) : ControllerBase
{
    [HttpGet("yearly")]
    public Dictionary<int, int[]> GetYearlyInventory() => inventory.GetYearly();

    [HttpGet("year/{year}")]
    public IEnumerable<InventoryIndex> GetInventoryByYear(int year) => inventory.GetByYear(year);
}
