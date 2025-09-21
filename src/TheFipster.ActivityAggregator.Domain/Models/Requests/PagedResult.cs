namespace TheFipster.ActivityAggregator.Domain.Models;

public class PagedResult<TItem>
{
    public PagedResult() { }

    public PagedResult(IEnumerable<TItem> items, int page, int size)
    {
        Items = items;
        Paging = new PagedRequest(page, size);
    }

    public PagedRequest Paging { get; set; } = new();
    public IEnumerable<TItem> Items { get; set; } = [];
}
