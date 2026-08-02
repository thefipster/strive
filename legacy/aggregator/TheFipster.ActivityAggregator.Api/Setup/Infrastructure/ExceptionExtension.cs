using TheFipster.ActivityAggregator.Api.Middleware;

namespace TheFipster.ActivityAggregator.Api.Setup.Infrastructure;

public static class ExceptionExtension
{
    public static void AddExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}
