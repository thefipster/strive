namespace TheFipster.ActivityAggregator.Api.Features.Upload.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController(IZipsAction zipsAction, IChunkAction chunkAction) : ControllerBase
{
    [HttpGet("zips")]
    public IActionResult GetZipFilePage([FromQuery] ZipsPageRequest request) =>
        Ok(zipsAction.GetZipFilePage(request));

    [HttpPost("chunk")]
    [DisableRequestSizeLimit]
    public async Task UploadChunk([FromForm] UploadChunkRequest request) =>
        await chunkAction.UploadChunkAsync(request);
}
