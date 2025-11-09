using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Indexing.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Indexing.Application;

internal sealed class IndexingFeature;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static IndexingFeatureBuilder AddIndexingFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return new IndexingFeatureBuilder(services, configuration);
    }
}
