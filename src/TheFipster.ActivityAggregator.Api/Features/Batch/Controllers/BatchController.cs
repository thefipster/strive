using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Controllers;

[ApiController]
[Route("api/batch")]
public class BatchController(
    IBatchAction batchAction,
    IBatchExistsPerYearAction batchPerYearAction,
    IBatchPageAction batchPageAction,
    IBatchByDateAction batchByDateAction,
    IMergeFileAction mergeFileAction
) : ControllerBase
{
    [HttpGet]
    public void Batch() => batchAction.Batch();

    [HttpGet("exists/{year}")]
    public IEnumerable<DateTime> GetExists(int year) => batchPerYearAction.GetExists(year);

    [HttpGet("merge")]
    public PagedResponse<BatchIndex> GetPage([FromQuery] PagedRequest request) =>
        batchPageAction.GetPage(request);

    [HttpGet("merge/{date}")]
    public IEnumerable<BatchIndex> GetBatchByDate(string date) =>
        batchByDateAction.GetBatchByDate(date);

    [HttpGet("merge/{date}/file")]
    [DisableRequestSizeLimit]
    public MergedFile GetMergedFileByDate(string date) => mergeFileAction.GetMergedFileByDate(date);
}
