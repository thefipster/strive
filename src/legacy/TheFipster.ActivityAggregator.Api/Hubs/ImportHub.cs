using Microsoft.AspNetCore.SignalR;

namespace TheFipster.ActivityAggregator.Api.Hubs;

public class ImportHub(ILogger<ImportHub> logger) : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        logger.LogInformation(
            "Client {ConnectionId} joined group {GroupName}",
            Context.ConnectionId,
            groupName
        );
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        logger.LogInformation(
            "Client {ConnectionId} left group {GroupName}",
            Context.ConnectionId,
            groupName
        );
    }
}
