using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Features.Batch.Services.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/batch")]
public class BatchController(
    IOptions<ApiConfig> config,
    IPagedIndexer<BatchIndex> batchIndex,
    IBatchService batcher,
    IBackgroundTaskQueue tasks
) : ControllerBase
{
    [HttpGet]
    public IActionResult Batch()
    {
        try
        {
            var convergencePath = config.Value.ConvergeDirectoryPath;
            if (!string.IsNullOrWhiteSpace(convergencePath))
            {
                tasks.QueueBackgroundWorkItem(async ct =>
                    await batcher.CombineFilesAsync(convergencePath, ct)
                );
            }
            return Ok();
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("exists/{year}")]
    public IEnumerable<DateTime> GetExists(int year)
    {
        return batchIndex
            .GetFiltered(x => x.Timestamp.Year == year)
            .Select(x => x.Timestamp.Date)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    [HttpGet("merge")]
    public PagedResult<BatchIndex> GetPage([FromQuery] PagedRequest request)
    {
        return batchIndex.GetPaged(request);
    }

    [HttpGet("merge/{date}")]
    public IActionResult GetBatchByDate(string date)
    {
        if (!DateTime.TryParse(date, out DateTime day))
        {
            throw new HttpResponseException(
                400,
                "Invalid date format, use yyyy-MM-dd or ISO-8601."
            );
        }

        var batch = batchIndex.GetFiltered(x => x.Timestamp.Date == day.Date);
        if (!batch.Any())
            throw new HttpResponseException(404, "Batch not found.");

        return Ok(batch);
    }

    [HttpGet("merge/{date}/file")]
    [DisableRequestSizeLimit]
    public IActionResult GetMergedFileByDate(string date)
    {
        if (!DateTime.TryParse(date, out DateTime timestamp))
        {
            throw new HttpResponseException(
                400,
                "Invalid date format, use yyyy-MM-ddTHH:mm:ss.fff or ISO-8601."
            );
        }

        var batch = batchIndex.GetById(timestamp);
        if (batch == null)
            throw new HttpResponseException(404, "Batch not found.");

        var file = MergedFile.FromFile(batch.Filepath);
        return Ok(file);
    }
}
