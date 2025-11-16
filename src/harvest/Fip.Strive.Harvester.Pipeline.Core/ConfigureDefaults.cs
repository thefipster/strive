using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Pipeline.Core;

public static class ConfigureDefaults
{
    public static void AddDefaults(this HostApplicationBuilder builder)
    {
        builder.Logging.AddConsole();
    }
}
