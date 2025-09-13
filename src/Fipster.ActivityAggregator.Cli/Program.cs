using Fipster.ActivityAggregator.Cli;
using Fipster.TrackMe.Importer;
using Fipster.TrackMe.Pipeline;
using Fipster.TrackMe.Storage.Lite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Executing Pipeline");
Console.WriteLine("==================");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(
        (context, config) =>
        {
            config
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true,
                    reloadOnChange: true
                );

            config.AddEnvironmentVariables();
            config.AddCommandLine(args);
        }
    )
    .ConfigureServices(
        (context, services) =>
        {
            services.AddImporters();
            services.AddLiteDbStorage(context.Configuration);
            services.AddPipeline(context.Configuration);

            services.AddSingleton<Runner>();
        }
    )
    .Build();

var runner = host.Services.GetRequiredService<Runner>();
var token = host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
await runner.ExecuteAsync(token);
await host.RunAsync();
