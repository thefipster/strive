using Scalar.AspNetCore;

namespace TheFipster.ActivityAggregator.Api.Setup.Infrastructure;

public static class OpenApiExtensions
{
    public static void AddOpenApiEndpoint(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
    }

    public static void UseScalar(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }
}
