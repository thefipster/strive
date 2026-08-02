using Microsoft.AspNetCore.SignalR;

namespace Fip.Strive.Harvester.Application.Core.Hubs;

public class HelloWorldHub : Hub
{
    public const string HubName = "hello";
}
