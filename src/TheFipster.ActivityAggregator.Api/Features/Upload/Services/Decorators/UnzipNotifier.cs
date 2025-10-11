using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Services.Decorators;

public class UnzipNotifier(IUnzipService component, INotifier notifier) : IUnzipService
{
    public async Task<ZipIndex> ExtractAsync(
        string zipFilepath,
        string outputDirectory,
        CancellationToken ct
    )
    {
        await notifier.ReportActionAsync(
            Const.Hubs.Importer.Actions.Unzip,
            $"Extraction of {new FileInfo(zipFilepath).Name} started."
        );

        var index = await component.ExtractAsync(zipFilepath, outputDirectory, ct);

        await notifier.ReportActionAsync(
            Const.Hubs.Importer.Actions.Unzip,
            $"Finished {new FileInfo(zipFilepath).Name}",
            true
        );

        return index;
    }
}
