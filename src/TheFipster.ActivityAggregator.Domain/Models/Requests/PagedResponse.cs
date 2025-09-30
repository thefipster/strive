namespace TheFipster.ActivityAggregator.Domain.Models.Requests;

public class PagedResponse<TItem>
{
    public IList<TItem> Items { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int Total { get; set; }
}
