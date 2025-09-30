namespace TheFipster.ActivityAggregator.Domain.Models;

public class PagedRequest
{
    public PagedRequest() { }

    public PagedRequest(int page, int size)
    {
        Page = page;
        Size = size;
    }

    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;

    public PagedResult<TItem> ToResult<TItem>(IEnumerable<TItem> items, int total) =>
        new PagedResult<TItem>(items, Page, Size, total);
}
