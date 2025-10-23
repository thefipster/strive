using Fip.Strive.Harvester.Application.Features.Schedule.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Fip.Strive.Harvester.Application.Features.Schedule;

public static class Registration
{
    public static void AddScheduleFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var schedulerSection = configuration.GetSection(ScheduleConfig.ConfigSectionName);

        var schedulerConfig = new ScheduleConfig();
        schedulerSection.Bind(schedulerConfig);

        services.Configure<ScheduleConfig>(schedulerSection);

        services.AddQuartzScheduler(schedulerConfig);
    }

    public static void AddQuartzScheduler(this IServiceCollection services, ScheduleConfig config)
    {
        services.AddQuartz(q =>
        {
            q.AddRegularIntervalJob<HelloWorldJob>(config.HelloWorldIntervalSeconds);
            q.AddRegularIntervalJob<QueueCleanupJob>(60);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
    }

    private static void AddRegularIntervalJob<TJob>(
        this IServiceCollectionQuartzConfigurator quartz,
        int intervalSeconds
    )
        where TJob : IJob
    {
        var jobName = typeof(TJob).Name;
        var jobKey = new JobKey(jobName);
        quartz.AddJob<TJob>(opts => opts.WithIdentity(jobKey));

        var triggerName = $"{jobName}Trigger";
        var interval = TimeSpan.FromSeconds(intervalSeconds);
        quartz.AddTrigger(opts =>
            opts.ForJob(jobKey)
                .WithIdentity(triggerName)
                .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
        );
    }
}
