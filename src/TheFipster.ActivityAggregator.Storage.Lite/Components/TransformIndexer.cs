using LiteDB;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components;

public class TransformIndexer(IndexerContext context) : ITransformIndexer
{
    private ILiteCollection<TransformIndex> collection = context.GetTransformCollection();

    public void Set(TransformIndex index) => collection.Upsert(index);

    public TransformIndex? GetById(string id) => collection.FindById(new BsonValue(id));

    public IEnumerable<TransformIndex> GetFiltered(string filter) =>
        collection.Find(x => x.Filter == filter);

    public IEnumerable<TransformIndex> GetAll() => collection.FindAll();
}
