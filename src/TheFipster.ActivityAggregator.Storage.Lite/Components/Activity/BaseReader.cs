using System.Linq.Expressions;
using LiteDB;
using TheFipster.ActivityAggregator.Storage.Abstractions.Activity;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Activity;

public class BaseReader<TItem, TKey>(ActivityContext context) : ILiteDbReader<TItem, TKey>
{
    private readonly ILiteCollection<TItem> collection = context.GetCollection<TItem>();

    public IEnumerable<TItem> GetFiltered(Expression<Func<TItem, bool>> filter) =>
        collection.Find(filter);

    public TItem? GetById(TKey id) => collection.FindById(new BsonValue(id));
}
