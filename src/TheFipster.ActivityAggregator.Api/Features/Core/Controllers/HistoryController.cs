using TheFipster.ActivityAggregator.Api.Features.Core.Mediators.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Controllers;

[ApiController]
[Route("api/history")]
public class HistoryController(IImportHistoryAction importHistoryAction) : ControllerBase
{
    [HttpGet("import/{date}")]
    public HistoryIndex GetImportHistory(string date) => importHistoryAction.GetImportHistory(date);
}
