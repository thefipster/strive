using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
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
    [HttpGet("merge")]
    public PagedResult<BatchIndex> GetFilePage(int page = 0, int size = 10) =>
        batchIndex.GetPaged(page, size);

    [HttpGet]
    public IActionResult Batch()
    {
        try
        {
            var convergancePath = config.Value.ConvergeDirectoryPath;
            if (!string.IsNullOrWhiteSpace(convergancePath))
            {
                tasks.QueueBackgroundWorkItem(async ct =>
                    await batcher.CombineFilesAsync(convergancePath, ct)
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
