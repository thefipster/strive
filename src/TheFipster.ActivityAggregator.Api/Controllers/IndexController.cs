using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/index")]
public class IndexController(
    IIndexer<ZipIndex> zipIndex,
    IPagedIndexer<FileIndex> fileIndex,
    IPagedIndexer<ExtractorIndex> extractIndex,
    IInventoryIndexer inventory
) : ControllerBase
{
    [HttpGet("inventory/yearly")]
    public Dictionary<int, int[]> GetYearlyInventory() => inventory.GetYearly();

    [HttpGet("inventory/year/{year}")]
    public IEnumerable<InventoryIndex> GetInventoryByYear(int year) => inventory.GetByYear(year);

    [HttpGet("zip/all")]
    public IEnumerable<ZipIndex> GetZipInventory() => zipIndex.GetAll();

    [HttpGet("files/paged")]
    public PagedResult<FileIndex> GetFilePage(int page = 0, int size = 10) =>
        fileIndex.GetPaged(page, size);

    [HttpGet("extracts/paged")]
    public PagedResult<ExtractorIndex> GetExtractPage(int page = 0, int size = 10) =>
        extractIndex.GetPaged(page, size);
}
