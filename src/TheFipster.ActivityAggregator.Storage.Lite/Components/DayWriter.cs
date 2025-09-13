using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using LiteDB;
using TheFipster.ActivityAggregator.Storage.Lite.Abstraction;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components;

public class DayWriter : IDayWriter
{
    private readonly ILiteCollection<UnifiedRecord> _records;

    public DayWriter(ILiteDbService liteDbService)
    {
        _records = liteDbService.GetCollection<UnifiedRecord>("DayRecords");
    }

    public void SetExtraction(FileExtraction extraction)
    {
        var date = extraction.Timestamp.Date;
        var day = _records.FindById(new BsonValue(date)) ?? new UnifiedRecord(date);

        foreach (var metric in extraction.Attributes)
        {
            day.Metrics[metric.Key] = metric.Value;
        }

        foreach (var series in extraction.Series)
        {
            day.Series[series.Key] = series.Value;
        }

        _records.Upsert(day);
    }
}
