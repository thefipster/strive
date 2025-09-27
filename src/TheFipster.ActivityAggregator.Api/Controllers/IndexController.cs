using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/index")]
public class IndexController(IIndexer<ImporterIndex> importIndexer) : ControllerBase
{
    [HttpGet("importer")]
    public IEnumerable<ImporterIndex> GetImporterIndexes() => importIndexer.GetAll();
}
