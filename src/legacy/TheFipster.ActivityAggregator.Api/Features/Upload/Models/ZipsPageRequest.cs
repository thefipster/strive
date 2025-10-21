using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Models;

public class ZipsPageRequest
{
    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;

    public string? Search { get; set; }

    public PageSpecificationRequest<ZipIndex> ToSpecification()
    {
        var specifications = new PageSpecificationRequest<ZipIndex>
        {
            Page = Page,
            Size = Size,
            Sort = s => s.IndexedAt,
            IsDescending = true,
        };

        if (!string.IsNullOrWhiteSpace(Search))
            specifications.AddFilter(f => f.ZipPath.Contains(Search));

        return specifications;
    }
}
