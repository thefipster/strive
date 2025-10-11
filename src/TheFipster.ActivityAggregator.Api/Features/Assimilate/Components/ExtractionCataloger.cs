using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Components;

public class ExtractionCataloger(IOptions<ApiConfig> config) : IExtractionCataloger
{
    public async Task<ExtractionMeta> HandleExtractAsync(
        FileIndex index,
        FileExtraction extract,
        CancellationToken ct
    )
    {
        var filepath = extract.Write(config.Value.ConvergeDirectoryPath);
        var file = new FileInfo(filepath);
        var hash = await file.HashXx3Async(ct);
        var size = file.Length;

        return ExtractionMeta.New(extract, filepath, hash, size);
    }
}
