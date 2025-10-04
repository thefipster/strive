using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/scan")]
public class ScanController(
    IScannerService scanner,
    IPagedIndexer<FileIndex> fileIndex,
    IOptions<ApiConfig> config,
    IBackgroundTaskQueue tasks
) : ControllerBase
{
    [HttpGet("files")]
    public PagedResult<FileIndex> GetFilePage(
        int page = 0,
        int size = 10,
        bool? classified = null,
        string? search = null
    )
    {
        if (classified == null && string.IsNullOrWhiteSpace(search))
            return fileIndex.GetPaged(page, size, null, sort => sort.Timestamp!);

        if (classified.HasValue && !string.IsNullOrWhiteSpace(search))
            return fileIndex.GetPaged(
                page,
                size,
                filter => filter.Source.HasValue == classified.Value,
                sort => sort.Timestamp!,
                descending: true,
                s => s.Path.Contains(search)
            );

        if (classified.HasValue)
            return fileIndex.GetPaged(
                page,
                size,
                filter => filter.Source.HasValue == classified.Value
            );

        return fileIndex.GetPaged(
            page,
            size,
            null,
            sort => sort.Timestamp!,
            true,
            s => s.Path.Contains(search!)
        );
    }

    [HttpGet]
    public IActionResult Scan()
    {
        try
        {
            var destinationDirectory = config.Value.UnzipDirectoryPath;
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                tasks.QueueBackgroundWorkItem(async ct =>
                    await scanner.CheckDirectoryAsync(destinationDirectory, ct)
                );
            }
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (DataMisalignedException)
        {
            return StatusCode(500);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }

        return Ok();
    }
}
