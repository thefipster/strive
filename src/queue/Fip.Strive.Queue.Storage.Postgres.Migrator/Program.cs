using Fip.Strive.Queue.Storage.Postgres.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Migrator starting for postgres queue schema.");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<PostgresQueueContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("strive-harvester"))
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PostgresQueueContext>();
    db.Database.Migrate();
}

Console.WriteLine("Migrations applied successfully for postgres queue schema.!");
