using Microsoft.AspNetCore.SignalR;
using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Hubs;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Components;

public class Notifier(IHubContext<ImportHub> hubContext) : INotifier
{
    public async Task ReportAsync(string message, bool update = false)
    {
        await hubContext.Clients.All.SendAsync("ReportProcess", message, update);
    }
}
