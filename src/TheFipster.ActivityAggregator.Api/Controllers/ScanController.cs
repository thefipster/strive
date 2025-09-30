using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/scan")]
public class ScanController(
    IScannerService scanner,
    IOptions<ApiConfig> config,
    IBackgroundTaskQueue tasks
) : ControllerBase
{
    [HttpGet]
    [DisableRequestSizeLimit]
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
