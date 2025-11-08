using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Queue.Domain;

public sealed class QueueFeatureBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
