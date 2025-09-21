using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Storage.Abstractions.Activity;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ActivityController(ILiteDbReader<UnifiedRecord, DateTime> reader) : ControllerBase
{
    [HttpGet("day")]
    public IEnumerable<UnifiedRecord> GetDay(DateTime day) =>
        reader.GetFiltered(x => x.Timestamp.Date == day.Date);
}
