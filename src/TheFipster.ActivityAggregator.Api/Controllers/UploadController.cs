using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Api.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController(IZipsAction zipsAction, IChunkAction chunkAction) : ControllerBase
{
    [HttpGet("zips")]
    public IActionResult GetZipFilePage([FromQuery] UploadFilePageRequest request) =>
        Ok(zipsAction.TryGetZipFilePage(request));

    [HttpPost("chunk")]
    [DisableRequestSizeLimit]
    public async Task UploadChunk([FromForm] UploadChunkRequest request) =>
        await chunkAction.TryUploadChunkAsync(request);
}
