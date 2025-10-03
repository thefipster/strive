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
    public PagedResult<FileIndex> GetFilePage(int page = 0, int size = 10) =>
        fileIndex.GetPaged(page, size);

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
