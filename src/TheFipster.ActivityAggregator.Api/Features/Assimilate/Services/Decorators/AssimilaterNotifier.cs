using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Services.Decorators;

public class AssimilaterNotifier(IAssimilaterService component, INotifier notifier)
    : IAssimilaterService
{
    public async Task ExtractFilesAsync(string destinationDirectory, CancellationToken ct)
    {
        await notifier.ReportActionAsync(
            Const.Hubs.Importer.Actions.Assimilate,
            "Assimilation started."
        );

        await component.ExtractFilesAsync(destinationDirectory, ct);

        await notifier.ReportActionAsync(
            Const.Hubs.Importer.Actions.Assimilate,
            "Finished assimilation.",
            true
        );
    }
}
