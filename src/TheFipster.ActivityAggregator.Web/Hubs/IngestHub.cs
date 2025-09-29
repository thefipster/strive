using Microsoft.AspNetCore.SignalR;
using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Web.Hubs;

public class IngestHub : Hub
{
    public async Task OnUnzipFinished(string file, string result)
    {
        await Clients.All.SendAsync(Const.Hubs.Ingester.UnzipFinishedMethod, file, result);
    }

    public async Task OnFileScanFinished(string result)
    {
        await Clients.All.SendAsync(Const.Hubs.Ingester.FileScanFinished, result);
    }

    public async Task OnWorkerStart(string message)
    {
        await Clients.All.SendAsync(Const.Hubs.Ingester.WorkerInfoMethod, message);
    }
}
