namespace Fip.Strive.Harvester.Application.Infrastructure.Models;

public class PagedResponse<TResult>(IEnumerable<TResult> items, int total)
{
    public int Total { get; set; } = total;
    public IEnumerable<TResult> Items { get; set; } = items;
}
