using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Api.Mediators.Scan.Contracts;
using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/scan")]
public class ScanController(
    IFilesAction fileAction,
    IClassifiersAction classifiersAction,
    IScanAction scanAction
) : ControllerBase
{
    [HttpGet]
    public void Scan() => scanAction.TryScan();

    [HttpGet("files")]
    public PagedResult<FileIndex> GetFilePage([FromQuery] ScanFilePageRequest request) =>
        fileAction.GetFilePage(request);

    [HttpGet("classifiers")]
    public Dictionary<DataSources, int> GetClassifiers() => classifiersAction.GetClassifiers();
}
