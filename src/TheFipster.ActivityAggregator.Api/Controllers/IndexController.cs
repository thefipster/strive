using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class IndexController(IMasterIndexer masterIndex, IIndexer<UnifyIndex> unifyIndex)
    : ControllerBase
{
    [HttpGet("year")]
    public Dictionary<DateTime, IEnumerable<DataKind>> GetYear(int yearNumber)
    {
        var year = new DateTime(yearNumber, 1, 1);
        return unifyIndex
            .GetFiltered(x => x.Year == year)
            .GroupBy(x => x.Timestamp.Date)
            .OrderBy(x => x.Key)
            .ToDictionary(x => x.Key, y => y.Select(x => x.Kind));
    }

    [HttpGet("month")]
    public Dictionary<DateTime, IEnumerable<DataKind>> GetMonth(DateTime date)
    {
        var minDate = new DateTime(date.Year, date.Month, 1).AddDays(-7);
        var maxDate = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(6);

        return unifyIndex
            .GetFiltered(x => x.Timestamp >= minDate && x.Timestamp <= maxDate)
            .GroupBy(x => x.Timestamp.Date)
            .OrderBy(x => x.Key)
            .ToDictionary(x => x.Key, y => y.Select(x => x.Kind));
    }

    [HttpGet("day")]
    public IEnumerable<MasterIndex> GetDay(DateTime day) => masterIndex.GetByDay(day.Date);

    [HttpGet("conflicts")]
    public PagedResult<UnifyIndex> GetConflicts([FromQuery] PagedRequest paging) =>
        unifyIndex.GetFiltered(x => x.HasConflicts).OrderBy(x => x.Timestamp).ToPagedResult(paging);
}
