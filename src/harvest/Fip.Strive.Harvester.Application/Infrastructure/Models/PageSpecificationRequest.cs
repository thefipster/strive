using System.Linq.Expressions;

namespace Fip.Strive.Harvester.Application.Infrastructure.Models;

public class PageSpecificationRequest<TItem>(
    Expression<Func<TItem, object>> sort,
    bool isDescending = false
)
{
    public PageSpecificationRequest(
        Expression<Func<TItem, object>> sort,
        bool isDescending,
        int page
    )
        : this(sort, isDescending) => Page = page;

    public PageSpecificationRequest(
        Expression<Func<TItem, object>> sort,
        bool isDescending,
        int page,
        int size
    )
        : this(sort, isDescending, page) => Size = size;

    public PageSpecificationRequest(
        Expression<Func<TItem, object>> sort,
        bool isDescending = false,
        params Expression<Func<TItem, bool>>[] filters
    )
        : this(sort, isDescending) => Filters = filters.ToList();

    public PageSpecificationRequest(
        Expression<Func<TItem, object>> sort,
        bool isDescending,
        int page,
        params Expression<Func<TItem, bool>>[] filters
    )
        : this(sort, isDescending, page) => Filters = filters.ToList();

    public PageSpecificationRequest(
        Expression<Func<TItem, object>> sort,
        bool isDescending,
        int page,
        int size,
        params Expression<Func<TItem, bool>>[] filters
    )
        : this(sort, isDescending, page, size) => Filters = filters.ToList();

    public int Page { get; set; }

    public int Size { get; set; } = 10;

    public Expression<Func<TItem, object>>? Sort { get; set; } = sort;

    public bool IsDescending { get; set; } = isDescending;

    public List<Expression<Func<TItem, bool>>> Filters { get; set; } = [];

    public void AddFilter(Expression<Func<TItem, bool>> filter) => Filters.Add(filter);
}
