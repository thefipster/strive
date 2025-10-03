namespace TheFipster.ActivityAggregator.Domain.Models.Requests;

public class PagedResult<TItem>
{
    public PagedResult() { }

    public PagedResult(IEnumerable<TItem> items, int page, int size, int total)
    {
        Items = items;
        Total = total;
        Paging = new PagedRequest(page, size);
    }

    public int Total { get; set; }

    public PagedRequest Paging { get; set; } = new();
    public IEnumerable<TItem> Items { get; set; } = [];
}
