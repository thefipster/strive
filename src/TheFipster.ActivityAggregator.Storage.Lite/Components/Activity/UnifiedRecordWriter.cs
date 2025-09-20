using LiteDB;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Storage.Abstractions.Activity;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Activity;

public class UnifiedRecordWriter(ActivityContext context) : IUnifiedRecordWriter
{
    private readonly ILiteCollection<UnifiedRecord> collection =
        context.GetCollection<UnifiedRecord>();

    public void Upsert(UnifiedRecord record) => collection.Upsert(record);
}
