using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Models;
using Fipster.TrackMe.Storage.Lite.Abstraction;
using LiteDB;

namespace Fipster.TrackMe.Storage.Lite.Components;

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
