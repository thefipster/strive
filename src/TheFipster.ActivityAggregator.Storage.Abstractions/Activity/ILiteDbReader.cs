using System.Linq.Expressions;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Activity;

public interface ILiteDbReader<TItem, TKey>
{
    IEnumerable<TItem> GetFiltered(Expression<Func<TItem, bool>> filter);

    TItem? GetById(TKey id);
}
