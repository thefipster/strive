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
    IIndexer<AssimilaterIndex> assimilateIndex
) : ControllerBase
{
    [HttpGet("importer/all")]
    public IEnumerable<ImporterIndex> GetImporterIndexes() => importIndexer.GetAll();

    [HttpGet("scanner/count/{hash}")]
    public int[] GetScannerIndexCount(string hash)
    {
        var indexes = GetScannerIndexes(hash).ToArray();
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

    [HttpGet("assimilator/all/{hash}")]
    public IEnumerable<AssimilaterIndex> GetAssimilatorIndexes(string hash) =>
        assimilateIndex.GetFiltered(x => x.OriginHash == hash);

    [HttpGet("assimilater/count/{hash}")]
    public int[] GetAssimilatorIndexCount(string hash)
    {
        var indexes = GetAssimilatorIndexes(hash).ToArray();
        return [indexes.Length, indexes.Sum(x => x.Count)];
    }
}
