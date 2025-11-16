using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Indexing.Sync.Cli.Services;

public class RedisListAccess(ILogger<RedisListAccess> logger)
{
    public async Task<List<TItem>> RightPopAll<TItem>(IDatabase db, string list)
    {
        var length = await db.ListLengthAsync(list);
        if (length == 0)
            return [];

        var items = new List<TItem>();
        for (long i = 0; i < length; i++)
        {
            var value = await db.ListRightPopAsync(list);
            if (!value.HasValue)
                break;

            var message = value.ToString();
            var index = JsonSerializer.Deserialize<TItem>(message);

            if (index is null)
            {
                logger.LogError("Could not deserialize {Message} from {List}", message, list);
                continue;
            }

            items.Add(index);
        }

        return items;
    }
}
