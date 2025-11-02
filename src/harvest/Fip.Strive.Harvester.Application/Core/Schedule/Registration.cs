using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Harvester.Application.Core.Queue.Jobs;
using Fip.Strive.Harvester.Application.Features.HelloWorld.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Fip.Strive.Harvester.Application.Core.Schedule;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddScheduleFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var schedulerConfig = GetSchedulerConfig(configuration);
        services.AddQuartzScheduler(schedulerConfig);
    }

    public static void AddQuartzScheduler(this IServiceCollection services, ScheduleConfig config)
    {
        services.AddQuartz(q =>
        {
            q.AddRegularIntervalJob<HelloWorldJob>(config.HelloWorldIntervalSeconds);
            q.AddRegularIntervalJob<QueueCleanupJob>(config.QueueCleanupIntervalSeconds);
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
        if (intervalSeconds <= 0)
            return;

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

    private static ScheduleConfig GetSchedulerConfig(IConfiguration configuration)
    {
        var schedulerConfig = new ScheduleConfig();
        var schedulerSection = configuration.GetSection(schedulerConfig.ConfigSectionName);
        schedulerSection.Bind(schedulerConfig);
        return schedulerConfig;
    }
}
