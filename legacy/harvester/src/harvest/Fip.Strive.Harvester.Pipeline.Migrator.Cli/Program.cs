using Fip.Strive.Harvester.Application.Infrastructure;
using Fip.Strive.Harvester.Application.Infrastructure.Pipeline;
using Fip.Strive.Harvester.Pipeline.Migrator.Cli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Pipeline - Migrator init");

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddRabbit(configuration);
builder.Services.AddPostgres(configuration).WithPipelineContext();
builder.Services.AddScoped<RabbitMigrator>();

var app = builder.Build();

Console.WriteLine("Harvester Pipeline - Migrator start");

using (var scope = app.Services.CreateScope())
{
    var postgres = scope.ServiceProvider.GetRequiredService<PipelineContext>();
    postgres.Database.Migrate();

    var rabbit = scope.ServiceProvider.GetRequiredService<RabbitMigrator>();
    await rabbit.ExecuteAsync();
}

Console.WriteLine("Harvester Pipeline - Migrator exit.");
