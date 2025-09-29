using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TheFipster.ActivityAggregator.Web.Hubs;

public class IngestHub : Hub
{
    public async Task UnzipFinished(string file, string result)
    {
        await Clients.All.SendAsync("OnUnzipFinished", file, result);
    }

    public async Task WorkerStart(string message)
    {
        await Clients.All.SendAsync("OnWorkerStart", message);
    }
}
