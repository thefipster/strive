using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abtraction;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Services.Abstractions;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController(
    IUploader uploader,
    IOptions<ApiConfig> config,
    IBackgroundTaskQueue tasks,
    IExtractor importer
) : ControllerBase
{
    [HttpPost("chunk")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadChunk([FromForm] UploadChunkRequest request)
    {
        try
        {
            var uploadFilepath = await uploader.EnsureChunk(request, config.Value);
            if (!string.IsNullOrWhiteSpace(uploadFilepath))
                tasks.QueueBackgroundWorkItem(async ct =>
                    await importer.ReadAsync(uploadFilepath, config.Value.UnzipDirectoryPath, ct)
                );
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
