using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/assimilate")]
public class AssimilateController(
    IOptions<ApiConfig> config,
    IPagedIndexer<ExtractorIndex> extractIndex,
    IAssimilaterService assimilater,
    IBackgroundTaskQueue tasks
) : ControllerBase
{
    [HttpGet("extracts")]
    public PagedResult<ExtractorIndex> GetFilePage(int page = 0, int size = 10) =>
        extractIndex.GetPaged(page, size);

    [HttpGet]
    public IActionResult Assimilate()
    {
        try
        {
            var destinationDirectory = config.Value.UnzipDirectoryPath;
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                tasks.QueueBackgroundWorkItem(async ct =>
                    await assimilater.ExtractFilesAsync(destinationDirectory, ct)
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
