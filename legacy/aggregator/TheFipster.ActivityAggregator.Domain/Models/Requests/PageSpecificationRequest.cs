using System.Linq.Expressions;

namespace TheFipster.ActivityAggregator.Domain.Models.Requests;

public class PageSpecificationRequest<TItem>
{
    public PageSpecificationRequest() { }

    public PageSpecificationRequest(PagedRequest paging)
    {
        Page = paging.Page;
        Size = paging.Size;
    }

    public int Page { get; set; }
    public int Size { get; set; } = 10;

    public Expression<Func<TItem, object>>? Sort { get; set; }

    public bool IsDescending { get; set; }

    public List<Expression<Func<TItem, bool>>> Filters { get; set; } = [];

    public void AddFilter(Expression<Func<TItem, bool>> filter) => Filters.Add(filter);
}
