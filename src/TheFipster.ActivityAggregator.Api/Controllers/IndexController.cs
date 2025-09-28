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
    IIndexer<ScannerIndex> scanIndex
) : ControllerBase
{
    [HttpGet("importer/all")]
    public IEnumerable<ImporterIndex> GetImporterIndexes() => importIndexer.GetAll();

    [HttpGet("scanner/count/{hash}")]
    public int[] GetScannerIndexCount(string hash)
    {
        var indexes = GetScannerIndexes(hash).ToArray();
        return [indexes.Length, indexes.Sum(x => x.Files.Count)];
    }

    [HttpGet("scanner/all/{hash}")]
    public IEnumerable<ScannerIndex> GetScannerIndexes(string hash) =>
        scanIndex.GetFiltered(x => x.OriginHash == hash);
}
