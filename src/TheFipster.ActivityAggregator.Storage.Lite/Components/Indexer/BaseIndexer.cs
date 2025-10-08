using System.Linq.Expressions;
using LiteDB;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

public class BaseIndexer<TIndex>(IndexerContext context) : IIndexer<TIndex>
{
    protected readonly ILiteCollection<TIndex> Collection = context.GetCollection<TIndex>();

    public IEnumerable<TIndex> GetFiltered(Expression<Func<TIndex, bool>> filter) =>
        Collection.Find(filter);

    public void Set(TIndex index) => Collection.Upsert(index);

    public TIndex? GetById(object id) => Collection.FindById(new BsonValue(id));

    public IEnumerable<TIndex> GetAll() => Collection.FindAll();

    public bool Exists(Expression<Func<TIndex, bool>> filter) => Collection.Exists(filter);
}
