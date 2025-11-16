using Fip.Strive.Core.Application.Features.FileSystem.Services;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.PubSub;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Pipeline.Scanner.Cli.Application;
using Fip.Strive.Harvester.Pipeline.Scanner.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fip.Strive.Harvester.Pipeline.Scanner.Cli;

public static class Inject
{
    public static void AddServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPubSubClient, PubSubClient>();
        builder.Services.AddSingleton<IDirectoryService, DirectoryService>();
        builder.Services.AddSingleton<IFileHasher, FileHasher>();
        builder.Services.AddSingleton<IScanIndexer, FileChecker>();
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
