namespace Fip.Strive.AppHost.Extensions;

public static class ResourceBuilderExtensions
{
    public static IResourceBuilder<ProjectResource> WithReferenceAndWait(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<IResourceWithConnectionString> resource
    ) => builder.WithReference(resource).WaitFor(resource);
}
