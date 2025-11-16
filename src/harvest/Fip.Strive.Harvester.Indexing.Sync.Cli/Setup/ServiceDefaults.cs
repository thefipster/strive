using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Indexing.Sync.Cli.Setup;

public static class ServiceDefaults
{
    public static void AddDefaults(this HostApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        builder.Logging.AddConsole();
    }
}
