using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TheFipster.ActivityAggregator.Web.Hubs;

public class EventHub : Hub
{
    public async Task ReportProgress(string message)
    {
        // Send event to all connected clients
        await Clients.All.SendAsync("ReceiveEvent", message);
    }
}
