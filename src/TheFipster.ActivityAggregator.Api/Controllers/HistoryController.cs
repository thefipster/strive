using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("api/history")]
public class HistoryController(IHistoryIndexer history) : ControllerBase
{
    [HttpGet("import/{date}")]
    public IActionResult GetImportHistory(string date)
    {
        if (!DateTime.TryParse(date, out var timestamp))
        {
            throw new HttpResponseException(
                400,
                "Invalid date format, use yyyy-MM-ddTHH:mm:ss.fff or ISO-8601."
            );
        }

        var result = history.GetProcessingPath(timestamp);
        if (result == null)
            throw new HttpResponseException(404, "Batch not found.");

        return Ok(result);
    }
}
