using Microsoft.AspNetCore.SignalR;
using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Web.Hubs;

public class IngestHub : Hub
{
    public async Task OnWorkerStart(string message)
    {
        await Clients.All.SendAsync(Const.Hubs.Ingester.WorkerInfo, message);
    }

    public async Task OnUnzipFinished(string file, string result)
    {
        await Clients.All.SendAsync(Const.Hubs.Ingester.UnzipFinished, file, result);
    }

    public async Task OnFileScanFinished(string result)
    {
        await Clients.All.SendAsync(Const.Hubs.Ingester.FileScanFinished, result);
    }

    public async Task OnFileScanProgress(int count)
    {
        await Clients.All.SendAsync(Const.Hubs.Ingester.FileScanProgress, count);
    }

    public async Task OnAssimilationFinished(string result)
    {
        await Clients.All.SendAsync(Const.Hubs.Ingester.AssimilationFinished, result);
    }

    public async Task OnAssimilationProgress(int count)
    {
        await Clients.All.SendAsync(Const.Hubs.Ingester.AssimilationProgress, count);
    }
}
