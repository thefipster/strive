using System.Linq.Expressions;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

public interface IIndexer<TIndex>
{
    void Set(TIndex index);

    TIndex? GetById(object id);

    IEnumerable<TIndex> GetFiltered(Expression<Func<TIndex, bool>> filter);

    IEnumerable<TIndex> GetAll();

    bool Exists(Expression<Func<TIndex, bool>> filter);
}
