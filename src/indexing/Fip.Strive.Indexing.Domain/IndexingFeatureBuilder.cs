using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Indexing.Domain;

public sealed class IndexingFeatureBuilder(
    IServiceCollection services,
    IConfiguration configuration
)
{
    public IServiceCollection Services { get; } = services;

    public IConfiguration Configuration { get; set; } = configuration;
}
