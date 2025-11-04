namespace Fip.Strive.Core.Domain.Schemas.Requests.Paging;

public class PagedResponse<TResult>(IEnumerable<TResult> items, int total)
{
    public int Total { get; set; } = total;
    public IEnumerable<TResult> Items { get; set; } = items;
}
