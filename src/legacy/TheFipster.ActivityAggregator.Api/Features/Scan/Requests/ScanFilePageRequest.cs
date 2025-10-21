using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Requests;

public class ScanFilePageRequest
{
    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;

    public string? Classified { get; set; }
    public string? Range { get; set; }
    public string? Search { get; set; }

    public PageSpecificationRequest<FileIndex> ToSpecification()
    {
        var specifications = new PageSpecificationRequest<FileIndex>
        {
            Page = Page,
            Size = Size,
            Sort = s => s.IndexedAt,
            IsDescending = true,
        };

        if (!string.IsNullOrWhiteSpace(Search) && Search.Length >= 3)
            specifications.AddFilter(f => f.Path.Contains(Search));

        if (!string.IsNullOrWhiteSpace(Range))
            switch (Range)
            {
                case "All":
                    // no filter needed
                    break;
                default:
                    var dateRange = (DateRanges)Enum.Parse(typeof(DateRanges), Range);
                    specifications.AddFilter(f => f.Range == dateRange);
                    break;
            }

        if (!string.IsNullOrWhiteSpace(Classified))
            switch (Classified)
            {
                case "All":
                    // no filter needed
                    break;
                case "Classified":
                    specifications.AddFilter(f => f.Source.HasValue);
                    break;
                case "Unclassified":
                    specifications.AddFilter(f => f.Source == null);
                    break;
                default:
                    var source = (DataSources)Enum.Parse(typeof(DataSources), Classified);
                    specifications.AddFilter(f => f.Source == source);
                    break;
            }

        return specifications;
    }
}
