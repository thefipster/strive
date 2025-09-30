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
    IIndexer<ImporterIndex> importIndexer,
    IIndexer<ScannerIndex> scanIndex,
    IIndexer<AssimilaterIndex> assimilateIndex,
    IIndexer<ZipIndex> zipIndex,
    IPagedIndexer<FileIndex> fileIndex,
    IInventoryIndexer inventory
) : ControllerBase
{
    [HttpGet("importer/all")]
    public IEnumerable<ImporterIndex> GetImporterIndexes() => importIndexer.GetAll();

    [HttpGet("scanner/count/{hash}")]
    public int[] GetScannerIndexCount(string hash)
    {
        var indexes = scanIndex.GetFiltered(x => x.OriginHash == hash).ToArray();
        return
        [
            indexes.Length,
            indexes.Sum(x => x.Files.Count),
            indexes.Count(x => x.Classification != null),
            indexes.Count(x => x.Classification == null),
        ];
    }

    [HttpGet("scanner/all/{hash}")]
    public IEnumerable<ScannerIndex> GetScannerIndexes(string hash) =>
        scanIndex.GetFiltered(x => x.OriginHash == hash);

    [HttpGet("assimilater/all/{hash}")]
    public IEnumerable<AssimilaterIndex> GetAssimilatorIndexes(string hash) =>
        assimilateIndex.GetFiltered(x => x.OriginHash == hash);

    [HttpGet("assimilater/count/{hash}")]
    public int[] GetAssimilatorIndexCount(string hash)
    {
        var indexes = GetAssimilatorIndexes(hash).ToArray();
        return [indexes.Length, indexes.Sum(x => x.Count)];
    }

    [HttpGet("inventory/yearly")]
    public Dictionary<int, int[]> GetYearlyInventory() => inventory.GetYearly();

    [HttpGet("inventory/year/{year}")]
    public IEnumerable<InventoryIndex> GetInventoryByYear(int year) => inventory.GetByYear(year);

    [HttpGet("zip/all")]
    public IEnumerable<ZipIndex> GetZipInventory() => zipIndex.GetAll();

    [HttpGet("files/paged")]
    public PagedResult<FileIndex> GetFilePage(int page = 0, int size = 10) =>
        fileIndex.GetPaged(page, size);
}
