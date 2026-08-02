using Fip.Strive.Harvester.Application.Infrastructure;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Indexing - Migrator init");

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddPostgres(configuration).WithIndexingContext();

var app = builder.Build();

Console.WriteLine("Harvester Indexing - Migrator start.");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IndexContext>();
    db.Database.Migrate();
}

Console.WriteLine("Harvester Indexing - Migrator exit.");
