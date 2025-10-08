using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/scan")]
public class ScanController(
    IScannerService scanner,
    IPagedIndexer<FileIndex> fileIndex,
    IOptions<ApiConfig> config,
    IImporterRegistry registry,
    IBackgroundTaskQueue tasks,
    ILogger<ScanController> logger
) : ControllerBase
{
    [HttpGet]
    public IActionResult Scan() => TryScan();

    [HttpGet("files")]
    public IActionResult GetFilePage([FromQuery] ScanFilePageRequest request) =>
        TryGetFilePage(request);

    [HttpGet("classifiers")]
    public IActionResult GetClassifiers() => TryGetClassifiers();

    private IActionResult TryGetFilePage(ScanFilePageRequest request)
    {
        try
        {
            var specifications = CreateSpecifiedRequest(request);
            var page = fileIndex.GetPaged(specifications);
            return Ok(page);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error.");
            return StatusCode(500);
        }
    }

    private IActionResult TryGetClassifiers()
    {
        try
        {
            var classifiers = registry
                .LoadClassifiers()
                .ToDictionary(x => x.Source, y => y.ClassifierVersion);
            return Ok(classifiers);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error.");
            return StatusCode(500);
        }
    }

    private IActionResult TryScan()
    {
        try
        {
            EnqueueScanTask();
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (DataMisalignedException e)
        {
            logger.LogError(e, "Data mapping error.");
            return StatusCode(500);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error.");
            return StatusCode(500);
        }

        return Ok();
    }

    private void EnqueueScanTask()
    {
        var destinationDirectory = config.Value.UnzipDirectoryPath;
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            tasks.QueueBackgroundWorkItem(async ct =>
                await scanner.CheckDirectoryAsync(destinationDirectory, ct)
            );
        }
    }

    private static PageSpecificationRequest<FileIndex> CreateSpecifiedRequest(
        ScanFilePageRequest request
    )
    {
        var specifications = new PageSpecificationRequest<FileIndex>();
        specifications.Page = request.Page;
        specifications.Size = request.Size;
        specifications.Sort = s => s.Timestamp!;
        specifications.IsDescending = true;

        AddClassificationFilter(specifications, request.Classified);
        AddRangeFilter(specifications, request.Range);
        AddSearchFilter(specifications, request.Search);
        return specifications;
    }

    private static void AddSearchFilter(
        PageSpecificationRequest<FileIndex> specifications,
        string? search
    )
    {
        if (!string.IsNullOrWhiteSpace(search) && search.Length >= 3)
        {
            specifications.AddFilter(f => f.Path.Contains(search));
        }
    }

    private static void AddRangeFilter(
        PageSpecificationRequest<FileIndex> specifications,
        string? range
    )
    {
        if (!string.IsNullOrWhiteSpace(range))
        {
            switch (range)
            {
                case "All":
                    // no filter needed
                    break;
                default:
                    var dateRange = (DateRanges)Enum.Parse(typeof(DateRanges), range);
                    specifications.AddFilter(f => f.Range == dateRange);
                    break;
            }
        }
    }

    private static void AddClassificationFilter(
        PageSpecificationRequest<FileIndex> specifications,
        string? classified
    )
    {
        if (!string.IsNullOrWhiteSpace(classified))
        {
            switch (classified)
            {
                case "All":
                    // no filter needed
                    break;
                case "Classified":
                    specifications.AddFilter(f => f.Source.HasValue);
                    break;
                case "Unclassified":
                    specifications.AddFilter(f => f.Source == null);
                    break;
                default:
                    var source = (DataSources)Enum.Parse(typeof(DataSources), classified);
                    specifications.AddFilter(f => f.Source == source);
                    break;
            }
        }
    }
}
