using TheFipster.ActivityAggregator.Api.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Services.Decoration;

public class UnzipNotifier(IUnzipService component, INotifier notifier) : IUnzipService
{
    public async Task<ZipIndex> ExtractAsync(
        string zipFilepath,
        string outputDirectory,
        CancellationToken ct
    )
    {
        await notifier.ReportAsync($"Extraction of {new FileInfo(zipFilepath).Name} started.");

        var index = await component.ExtractAsync(zipFilepath, outputDirectory, ct);

        await notifier.ReportAsync($"Finished {new FileInfo(zipFilepath).Name}", true);

        return index;
    }
}
