namespace TheFipster.ActivityAggregator.Domain.Models.Requests;

public class PagedRequest
{
    public PagedRequest() { }

    public PagedRequest(int page, int size)
    {
        Page = page;
        Size = size;
    }

    public int Page { get; set; }
    public int Size { get; set; } = 10;

    public PagedResult<TItem> ToResult<TItem>(IEnumerable<TItem> items, int total) =>
        new PagedResult<TItem>(items, Page, Size, total);

    public PageSpecificationRequest<TIndex> ToSpecification<TIndex>() =>
        new PageSpecificationRequest<TIndex> { Page = Page, Size = Size };
}
