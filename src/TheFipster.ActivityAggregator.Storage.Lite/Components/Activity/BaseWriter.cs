using LiteDB;
using TheFipster.ActivityAggregator.Storage.Abstractions.Activity;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Activity;

public class BaseWriter<TItem>(ActivityContext context) : ILiteDbWriter<TItem>
{
    private readonly ILiteCollection<TItem> collection = context.GetCollection<TItem>();

    public void Set(TItem index) => collection.Upsert(index);
}
