using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Controllers;

[ApiController]
[Route("api/scan")]
public class ScanController(
    IFilePageAction pageAction,
    IClassifiersAction classifiersAction,
    IScanAction scanAction
) : ControllerBase
{
    [HttpGet]
    public void Scan() => scanAction.Scan();

    [HttpGet("classifiers")]
    public Dictionary<DataSources, int> GetClassifiers() => classifiersAction.GetClassifiers();

    [HttpGet("files")]
    public PagedResponse<FileIndex> GetFilePage([FromQuery] ScanFilePageRequest request) =>
        pageAction.GetFilePage(request);
}
