using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Options;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Models;

public class AssimilateExtractPageRequest
{
    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;

    public string? Classified { get; set; }
    public string? Parameter { get; set; }
    public string? Date { get; set; }

    public PageSpecificationRequest<ExtractorIndex> ToSpecification()
    {
        var specifications = new PageSpecificationRequest<ExtractorIndex>
        {
            Page = Page,
            Size = Size,
            Sort = s => s.IndexedAt,
            IsDescending = true,
        };

        if (!string.IsNullOrWhiteSpace(Parameter))
            switch (Parameter)
            {
                case ParametersOptions.Any:
                    // no filter needed
                    break;
                default:
                    specifications.AddFilter(f => f.Metrics.Contains(Parameter));
                    break;
            }

        if (!string.IsNullOrWhiteSpace(Classified))
            switch (Classified)
            {
                case ClassificationOptions.All:
                    // no filter needed
                    break;
                case ClassificationOptions.Classified:
                    specifications.AddFilter(f => f.Source.HasValue);
                    break;
                case ClassificationOptions.Unclassified:
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
