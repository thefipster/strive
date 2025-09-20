using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TheFipster.ActivityAggregator.Cli;
using TheFipster.ActivityAggregator.Importer;
using TheFipster.ActivityAggregator.Pipeline;
using TheFipster.ActivityAggregator.Storage.Lite;

Console.WriteLine("Pipeline App started.");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog(
        (context, _, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext();
        }
    )
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
