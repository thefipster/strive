using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/processing")]
public class ProcessingController : ControllerBase
{
    private readonly HubConnection connection;
    private readonly IIndexer<ImporterIndex> importIndex;
    private readonly IScannerService scanner;
    private readonly IBackgroundTaskQueue tasks;

    public ProcessingController(
        IIndexer<ImporterIndex> importIndex,
        IScannerService scanner,
        IBackgroundTaskQueue tasks
    )
    {
        this.importIndex = importIndex;
        this.scanner = scanner;
        this.tasks = tasks;

        connection = new HubConnectionBuilder().WithUrl("https://localhost:7260/scanhub").Build();
        connection.StartAsync().Wait();
    }

    [HttpGet("scan/{import}")]
    public IActionResult SendEvent(string import)
    {
        if (string.IsNullOrWhiteSpace(import))
            return BadRequest();

        var index = importIndex.GetById(import);
        if (index == null)
            return NotFound();

        tasks.QueueBackgroundWorkItem(async ct => await scanner.CheckImportAsync(index, ct));

        return Ok();
    }
}
