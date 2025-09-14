using LiteDB;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components;

public class ScanIndexer(IndexerContext context) : IScanIndexer
{
    private ILiteCollection<ClassificationIndex> collection = context.GetScanCollection();

    public void Set(ClassificationIndex index) => collection.Upsert(index);

    public ClassificationIndex Get(string id) => collection.FindById(new BsonValue(id));

    public IEnumerable<ClassificationIndex> Filter(DataSources filter) =>
        collection.Find(x => x.Classifications.Any(x => x.Source == filter));
}
