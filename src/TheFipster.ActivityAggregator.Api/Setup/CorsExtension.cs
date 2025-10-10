using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Setup;

public static class CorsExtension
{
    public static void AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                Const.Cors.AllowAll,
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        services.AddCors(options =>
        {
            options.AddPolicy(
                Const.Cors.AllowOne,
                policy =>
                {
                    policy.WithOrigins("https://localhost:7260").AllowAnyMethod().AllowAnyHeader();
                }
            );
        });
    }
}
