using System.Reflection;
using Quartz;

namespace Fip.Strive.Harvester.Application.Core.Schedule;

public static class QuartzServiceCollectionExtensions
{
    public static IServiceCollectionQuartzConfigurator AddRegularIntervalJobsFromAssembly(
        this IServiceCollectionQuartzConfigurator quartz,
        Assembly assembly
    )
    {
        var jobTypes = assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IJob).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<RegularIntervalJobAttribute>() != null);

        foreach (var jobType in jobTypes)
        {
            var attribute = jobType.GetCustomAttribute<RegularIntervalJobAttribute>()!;

            var jobName = jobType.Name;
            var jobKey = new JobKey(jobName);

            quartz.AddJob(
                jobType,
                jobKey,
                jobConfig =>
                {
                    jobConfig.WithIdentity(jobKey);
                }
            );

            quartz.AddTrigger(trigger =>
                trigger
                    .ForJob(jobKey)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithInterval(attribute.Interval).RepeatForever()
                    )
            );
        }

        return quartz;
    }
}
