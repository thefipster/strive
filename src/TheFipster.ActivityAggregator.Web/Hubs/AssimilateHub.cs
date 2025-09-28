using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TheFipster.ActivityAggregator.Web.Hubs;

public class AssimilateHub : Hub
{
    public async Task Progress(string hash, int procCount, int outCount)
    {
        await Clients.All.SendAsync("OnProgress", hash, procCount, outCount);
    }

    public async Task Finished(string hash)
    {
        await Clients.All.SendAsync("OnFinished", hash);
    }
}
