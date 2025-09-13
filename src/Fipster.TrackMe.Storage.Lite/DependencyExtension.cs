using Fipster.TrackMe.Storage.Lite.Abstraction;
using Fipster.TrackMe.Storage.Lite.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fipster.TrackMe.Storage.Lite;

public static class DependencyExtension
{
    public static IServiceCollection AddLiteDbStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<LiteDbConfig>(configuration.GetSection(LiteDbConfig.ConfigSectionName));

        services.AddScoped<ILiteDbService, LiteDbIndexContext>();

        return services;
    }
}
