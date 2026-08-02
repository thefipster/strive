namespace Fip.Strive.AppHost.Extensions;

public static class ResourceBuilderExtensions
{
    public static IResourceBuilder<ProjectResource> WithReferenceAndWait(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<IResourceWithConnectionString> resource
    ) => builder.WithReference(resource).WaitFor(resource);

    public static IResourceBuilder<ProjectResource> AddDependentResource<TProject>(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<IResourceWithConnectionString>[] referenceAndWait
    )
        where TProject : IProjectMetadata, new()
    {
        var resBuilder = builder.AddProject<TProject>(name);

        foreach (var reference in referenceAndWait)
            resBuilder.WithReferenceAndWait(reference);

        return resBuilder;
    }

    public static IResourceBuilder<ProjectResource> AddDependentResource<TProject>(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<IResourceWithConnectionString>[] referenceAndWait,
        IResourceBuilder<ProjectResource>[] waitForCompletion
    )
        where TProject : IProjectMetadata, new()
    {
        var resBuilder = builder.AddProject<TProject>(name);

        foreach (var reference in referenceAndWait)
            resBuilder.WithReferenceAndWait(reference);

        foreach (var wait in waitForCompletion)
            resBuilder.WaitForCompletion(wait);

        return resBuilder;
    }

    public static IResourceBuilder<ProjectResource> AddDependentResourceWithSequence<TProject>(
        this IDistributedApplicationBuilder builder,
        string name,
        Dictionary<string, string> environmentVariables,
        IResourceBuilder<IResourceWithConnectionString>[] dependencies,
        IResourceBuilder<ProjectResource>[] waitForCompletion
    )
        where TProject : IProjectMetadata, new()
    {
        var resBuilder = builder.AddProject<TProject>(name);

        foreach (var dependency in dependencies)
            resBuilder.WithReferenceAndWait(dependency);

        foreach (var (key, value) in environmentVariables)
            resBuilder.WithEnvironment(key, value);

        foreach (var wait in waitForCompletion)
            resBuilder.WaitForCompletion(wait);

        return resBuilder;
    }

    public static IResourceBuilder<ProjectResource> AddDependentResourceWithSequence<TProject>(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<IResourceWithConnectionString>[] dependencies,
        IResourceBuilder<ProjectResource>[] waitForCompletion
    )
        where TProject : IProjectMetadata, new()
    {
        var resBuilder = builder.AddProject<TProject>(name);

        foreach (var dependency in dependencies)
            resBuilder.WithReferenceAndWait(dependency);

        foreach (var wait in waitForCompletion)
            resBuilder.WaitForCompletion(wait);

        return resBuilder;
    }
}
