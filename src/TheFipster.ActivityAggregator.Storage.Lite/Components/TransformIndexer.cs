using LiteDB;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components;

public class TransformIndexer(IndexerContext context) : ITransformIndexer
{
    private ILiteCollection<TransformIndex> collection = context.GetTransformCollection();

    public void Set(TransformIndex index) => collection.Upsert(index);

    public TransformIndex Get(string id) => collection.FindById(new BsonValue(id));

    public IEnumerable<TransformIndex> Filter(DateTime filter) =>
        collection.Find(x => x.Date == filter.ToDateString());
}
