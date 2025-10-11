using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Upload.Models;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController(IZipsAction zipsAction, IChunkAction chunkAction) : ControllerBase
{
    [HttpGet("zips")]
    public IActionResult GetZipFilePage([FromQuery] UploadFilePageRequest request) =>
        Ok(zipsAction.GetZipFilePage(request));

    [HttpPost("chunk")]
    [DisableRequestSizeLimit]
    public async Task UploadChunk([FromForm] UploadChunkRequest request) =>
        await chunkAction.UploadChunkAsync(request);
}
