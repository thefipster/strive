using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Web.Extensions;
using Fip.Strive.Harvester.Application.Core.Hubs;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories.Contracts;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Harvester.Web;

public static class ApplicationServices
{
    private const string ConnectionStringName = "strive-harvester-database";

    public static void AddIndexing(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddContext(configuration, ConnectionStringName);
        services.AddScoped<IZipRepository, ZipRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
    }

    private static void AddContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName
    )
    {
        var connection = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connection))
            throw new ConfigurationException("Postgres connection string is missing.");

        services.AddDbContext<IndexContext>(options => options.UseNpgsql(connection));
    }
}
