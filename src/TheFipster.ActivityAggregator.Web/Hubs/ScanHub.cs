using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TheFipster.ActivityAggregator.Web.Hubs;

public class ScanHub : Hub
{
    public async Task Progress(string hash, int procCount)
    {
        await Clients.All.SendAsync("OnProgress", hash, procCount);
    }

    public async Task Finished(string hash)
    {
        await Clients.All.SendAsync("OnFinished", hash);
    }
}
