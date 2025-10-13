using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Services.Decorators;

public class AssimilaterNotifier(IAssimilaterService component, INotifier notifier)
    : IAssimilaterService
{
    public async Task ExtractFiles(string destinationDirectory, CancellationToken ct)
    {
        await notifier.ReportActionAsync(
            Defaults.Hubs.Importer.Actions.Assimilate,
            "Assimilation started."
        );

        await component.ExtractFiles(destinationDirectory, ct);
    }
}
