using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Migrator starting for indexing.");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<IndexPgContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("strive-harvester"))
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IndexPgContext>();
    db.Database.Migrate();
}

Console.WriteLine("Migrations applied successfully to indexing!");
