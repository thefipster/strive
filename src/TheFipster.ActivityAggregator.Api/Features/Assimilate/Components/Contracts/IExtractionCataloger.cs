using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Components.Contracts;

public interface IExtractionCataloger
{
    Task<ExtractionMeta> HandleExtractAsync(
        FileIndex index,
        FileExtraction extract,
        CancellationToken ct
    );
}
