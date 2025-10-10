using Microsoft.AspNetCore.SignalR;
using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Hubs;

public class ImportHub : Hub
{
    public async Task OnProcessStart(string process)
    {
        await Clients.All.SendAsync(Const.Hubs.Importer.OnProcessStart, process);
    }

    public async Task OnProcessReport(string process, string message, double? progress = null)
    {
        await Clients.All.SendAsync(
            Const.Hubs.Importer.OnProcessReport,
            process,
            message,
            progress
        );
    }

    public async Task OnProcessDone(string process, string result)
    {
        await Clients.All.SendAsync(Const.Hubs.Importer.OnProcessDone, process, result);
    }
}
