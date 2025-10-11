using Microsoft.AspNetCore.SignalR;
using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Hubs;
using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Components;

public class Notifier(IHubContext<ImportHub> hubContext) : INotifier
{
    public async Task ReportActionAsync(string action, string message, bool update = false)
    {
        await hubContext
            .Clients.Group(action)
            .SendAsync(Const.Hubs.Importer.ReportAction, message, update);
    }

    public async Task ReportProgressAsync(string action, string message, double progress)
    {
        await hubContext
            .Clients.Group(action)
            .SendAsync(Const.Hubs.Importer.ReportProgress, message, progress);
    }
}
