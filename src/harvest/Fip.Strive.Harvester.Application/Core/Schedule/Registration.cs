using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Fip.Strive.Harvester.Application.Core.Schedule;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddScheduleFeature(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.AddRegularIntervalJobsFromAssembly(typeof(Registration).Assembly);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
    }
}
