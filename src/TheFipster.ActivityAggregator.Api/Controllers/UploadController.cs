using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController(
    IUploader uploader,
    IOptions<ApiConfig> config,
    IIndexer<ZipIndex> zipIndex,
    IBackgroundTaskQueue tasks,
    IUnzipService unzipper
) : ControllerBase
{
    [HttpGet("zips")]
    public IEnumerable<ZipIndex> GetZipInventory() => zipIndex.GetAll();

    [HttpPost("chunk")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadChunk([FromForm] UploadChunkRequest request)
    {
        try
        {
            var uploadFilepath = await uploader.EnsureChunk(request, config.Value);
            var destinationDirectory = config.Value.UnzipDirectoryPath;

            if (
                !string.IsNullOrWhiteSpace(uploadFilepath)
                && !string.IsNullOrWhiteSpace(destinationDirectory)
            )
            {
                tasks.QueueBackgroundWorkItem(async ct =>
                    await unzipper.ExtractAsync(uploadFilepath, destinationDirectory, ct)
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
