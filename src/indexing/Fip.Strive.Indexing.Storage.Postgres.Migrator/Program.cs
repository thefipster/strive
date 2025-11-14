using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Indexing.Storage.Postgres.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Indexing - Migrator starting...");

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

var connection = configuration.GetConnectionString("strive-harvester");

if (string.IsNullOrWhiteSpace(connection))
    throw new ConfigurationException("RabbitMQ connection string is missing.");

builder.Services.AddDbContext<IndexPgContext>(options => options.UseNpgsql(connection));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IndexPgContext>();
    db.Database.Migrate();
}

Console.WriteLine("Harvester Indexing - Migrator exited.");
