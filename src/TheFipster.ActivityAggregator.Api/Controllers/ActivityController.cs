using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Storage.Abstractions.Activity;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ActivityController(ILiteDbReader<MergedRecord, DateTime> reader) : ControllerBase
{
    [HttpGet("day")]
    public IEnumerable<MergedRecord> GetDay(DateTime day) =>
        reader.GetFiltered(x => x.Timestamp.Date == day.Date);
}
