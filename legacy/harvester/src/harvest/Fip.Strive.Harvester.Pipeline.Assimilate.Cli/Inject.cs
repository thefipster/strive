using Fip.Strive.Core.Application.Features.FileSystem.Services;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Core.PubSub;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Fip.Strive.Harvester.Pipeline.Assimilate.Cli.Application;
using Fip.Strive.Harvester.Pipeline.Assimilate.Cli.Services;
using Fip.Strive.Ingestion.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fip.Strive.Harvester.Pipeline.Assimilate.Cli;

public static class Inject
{
    public static void AddServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddIngestionFeature();

        builder.Services.AddSingleton<IPubSubClient, PubSubClient>();
        builder.Services.AddSingleton<IFileHasher, FileHasher>();
        builder.Services.AddSingleton<ISetNameIndex<DataIndex>, DataIndexWriter>();
        builder.Services.AddSingleton<ISetHashIndex<ExtractIndex>, ExtractIndexWriter>();
        builder.Services.AddSingleton<IProcessor, Worker>();
        builder.Services.AddSingleton<Service>();
        builder.Services.AddHostedService<Service>();
    }

    public static void AddOptions(this HostApplicationBuilder builder)
    {
        builder
            .Services.AddOptions<Config>()
            .BindConfiguration(Config.ConfigSectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
