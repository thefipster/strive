using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class IndexController(IMasterIndexer index) : ControllerBase
{
    [HttpGet("month", Name = "GetMonth")]
    public Dictionary<DateTime, int> GetMonth(DateTime date)
    {
        var minDate = new DateTime(date.Year, date.Month, 1).AddDays(-7);
        var maxDate = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(6);

        var runner = minDate;
        var month = new List<MasterIndex>();
        while (runner <= maxDate)
        {
            var report = index.GetByDay(runner);
            month.AddRange(report);
            runner = runner.AddDays(1);
        }

        var byDay = month.GroupBy(x => x.Timestamp.Date);

        return byDay.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Count());
    }

    [HttpGet("day", Name = "GetDay")]
    public IEnumerable<MasterIndex> GetDay(DateTime day) => index.GetByDay(day.Date);
}
