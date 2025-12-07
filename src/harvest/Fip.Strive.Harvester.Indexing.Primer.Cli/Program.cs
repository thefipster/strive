using Fip.Strive.Harvester.Application.Infrastructure;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing;
using Fip.Strive.Harvester.Indexing.Primer.Cli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Indexing - Primer init");

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddPostgres(configuration).WithIndexingContext();
builder.Services.AddRedis(configuration);

builder.Services.AddScoped<Worker>();

var app = builder.Build();

Console.WriteLine("Harvester Indexing - Primer start.");

using (var scope = app.Services.CreateScope())
{
    var worker = scope.ServiceProvider.GetRequiredService<Worker>();
    await worker.ExecuteAsync();
}

Console.WriteLine("Harvester Indexing - Primer exit.");
