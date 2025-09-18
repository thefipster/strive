using LiteDB;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components;

public class ScanIndexer(IndexerContext context) : IScanIndexer
{
    private readonly ILiteCollection<ScanIndex> collection = context.GetScanCollection();

    public void Set(ScanIndex index) => collection.Upsert(index);

    public ScanIndex? GetById(string id) => collection.FindById(new BsonValue(id));

    public IEnumerable<ScanIndex> GetFiltered(string filter) =>
        collection.Find(x => x.Directory == filter);

    public IEnumerable<ScanIndex> GetAll() => collection.FindAll();
}
