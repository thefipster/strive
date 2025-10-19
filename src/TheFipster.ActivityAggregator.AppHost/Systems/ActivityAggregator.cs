namespace TheFipster.ActivityAggregator.AppHost.Systems;

public static class ActivityAggregator
{
    public static void AddActivityAggregatorProject(this IDistributedApplicationBuilder builder)
    {
        var api = builder.AddProject<Projects.TheFipster_ActivityAggregator_Api>("api");
        builder.AddProject<Projects.TheFipster_ActivityAggregator_Web>("blazor").WithReference(api);
    }
}
