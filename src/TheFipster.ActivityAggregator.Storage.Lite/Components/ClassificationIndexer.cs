using LiteDB;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components;

public class ClassificationIndexer(IndexerContext context) : IClassificationIndexer
{
    private ILiteCollection<ClassificationIndex> collection = context.GetClassificationCollection();

    public void Set(ClassificationIndex index) => collection.Upsert(index);

    public ClassificationIndex? GetById(string id) => collection.FindById(new BsonValue(id));

    public IEnumerable<ClassificationIndex> GetFiltered(string filter) =>
        collection.Find(x => x.Filter == filter);

    public IEnumerable<ClassificationIndex> GetAll() => collection.FindAll();
}
