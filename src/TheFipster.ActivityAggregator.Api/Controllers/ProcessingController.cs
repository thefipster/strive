using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/processing")]
public class ProcessingController(
    IIndexer<ImporterIndex> importIndex,
    IScannerService scanner,
    IAssimilaterService assimilater,
    IBackgroundTaskQueue tasks
) : ControllerBase
{
    [HttpGet("scan/{importHash}")]
    public IActionResult ExecuteScanning(string importHash)
    {
        if (string.IsNullOrWhiteSpace(importHash))
            return BadRequest();

        var index = importIndex.GetById(importHash);
        if (index == null)
            return NotFound();

        tasks.QueueBackgroundWorkItem(async ct => await scanner.CheckImportAsync(index, ct));

        return Ok();
    }

    [HttpGet("assimilate/{importHash}")]
    public IActionResult ExecuteAssimilating(string importHash)
    {
        if (string.IsNullOrWhiteSpace(importHash))
            return BadRequest();

        var index = importIndex.GetById(importHash);
        if (index == null)
            return NotFound();

        tasks.QueueBackgroundWorkItem(async ct => await assimilater.ConvergeImportAsync(index, ct));

        return Ok();
    }
}
