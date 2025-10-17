namespace TheFipster.ActivityAggregator.Domain.Models.Requests;

public class PagedResponse<TItem>
{
    public PagedResponse() { }

    public PagedResponse(IEnumerable<TItem> items, int page, int size, int total)
    {
        Items = items;
        Total = total;
        Paging = new PagedRequest(page, size);
    }

    public int Total { get; set; }

    public PagedRequest Paging { get; set; } = new();
    public IEnumerable<TItem> Items { get; set; } = [];
}
