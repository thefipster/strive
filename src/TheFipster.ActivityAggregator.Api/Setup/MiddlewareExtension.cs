using TheFipster.ActivityAggregator.Api.Middleware;

namespace TheFipster.ActivityAggregator.Api.Setup;

public static class MiddlewareExtension
{
    public static void AddMiddleware(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
    }
}
