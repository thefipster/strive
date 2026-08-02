using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Services;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController(IInventoryIndexer inventory) : ControllerBase
{
    [HttpGet("yearly")]
    public Dictionary<int, int[]> GetYearlyInventory() => inventory.GetYearly();

    [HttpGet("year/{year}")]
    public IEnumerable<InventoryIndex> GetInventoryByYear(int year) => inventory.GetByYear(year);
}
