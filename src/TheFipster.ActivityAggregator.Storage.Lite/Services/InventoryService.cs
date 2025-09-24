using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Activity;

namespace TheFipster.ActivityAggregator.Storage.Lite.Services;

public class InventoryService(ILiteDbWriter<Inventory> writer, ILiteDbReader<Inventory, int> reader)
    : IInventoryService
{
    public void Update(MergedRecord record)
    {
        var timestamp = record.Timestamp;
        var entries = Get(timestamp.Year, timestamp.Month, timestamp.Day);
        var entry = entries.FirstOrDefault() ?? new Inventory(timestamp);

        var metrics = record.Metrics.Keys;
        var series = record.Samples.Select(x => x.Type);
        var events = record.Events.Select(x => x.Type);

        entry.Metrics = entry.Metrics.Concat(metrics).ToHashSet();
        entry.Events = entry.Events.Concat(events).ToHashSet();
        entry.Series = entry.Series.Concat(series).ToHashSet();

        writer.Set(entry);
    }

    public IEnumerable<Inventory> Get(int year, int? month, int? day)
    {
        if (month.HasValue && day.HasValue)
        {
            var entry = reader.GetById(year * 10000 + month.Value * 100 + day.Value);
            if (entry != null)
                return [entry];

            return [];
        }

        if (month.HasValue)
            return reader.GetFiltered(x => x.Year == year && x.Month == month.Value);

        return reader.GetFiltered(x => x.Year == year);
    }
}
