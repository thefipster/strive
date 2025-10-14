using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Services.Decorators;

public class BatchNotifier(IBatchService component, INotifier notifier) : IBatchService
{
    public async Task CombineFilesAsync(string convergancePath, CancellationToken ct)
    {
        await notifier.ReportActionAsync(
            Const.Hubs.Importer.Actions.Batch,
            "Batch grouping started."
        );

        await component.CombineFilesAsync(convergancePath, ct);
    }
}
