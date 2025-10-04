using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/assimilate")]
public class AssimilateController(
    IOptions<ApiConfig> config,
    IPagedIndexer<ExtractorIndex> extractIndex,
    IImporterRegistry registry,
    IAssimilaterService assimilater,
    IBackgroundTaskQueue tasks
) : ControllerBase
{
    [HttpGet("extracts")]
    public PagedResult<ExtractorIndex> GetFilePage(int page = 0, int size = 10) =>
        extractIndex.GetPaged(page, size);

    [HttpGet("extractors")]
    public Dictionary<DataSources, int> GetExtractors() =>
        registry.LoadExtractors().ToDictionary(x => x.Source, y => y.ExtractorVersion);

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
