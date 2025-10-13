using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Setup.Infrastructure;

public static class CorsExtension
{
    public static void AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                Defaults.Cors.AllowAll,
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        services.AddCors(options =>
        {
            options.AddPolicy(
                Defaults.Cors.AllowOne,
                policy =>
                {
                    policy.WithOrigins("https://localhost:7260").AllowAnyMethod().AllowAnyHeader();
                }
            );
        });
    }

    public static void UseCorsPolicy(this WebApplication app)
    {
        app.UseCors(
            app.Environment.IsDevelopment() ? Defaults.Cors.AllowAll : Defaults.Cors.AllowOne
        );
    }
}
