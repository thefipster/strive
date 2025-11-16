using Fip.Strive.Harvester.Indexing.Sync.Cli.Application;
using Fip.Strive.Harvester.Indexing.Sync.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fip.Strive.Harvester.Indexing.Sync.Cli.Setup;

public static class Dependencies
{
    public static void AddServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<RedisListAccess>();
        builder.Services.AddSingleton<ZipInserter>();
        builder.Services.AddSingleton<FileInserter>();
        builder.Services.AddSingleton<SourceInserter>();
        builder.Services.AddSingleton<ExtractInserter>();
        builder.Services.AddSingleton<DataInserter>();
        builder.Services.AddSingleton<Service>();
        builder.Services.AddHostedService<Service>();
    }
}
