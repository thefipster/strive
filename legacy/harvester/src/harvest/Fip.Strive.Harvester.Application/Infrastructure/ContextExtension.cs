using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Infrastructure;

public class ContextExtension(IServiceCollection services, string connectionString)
{
    public IServiceCollection Services => services;

    public string ConnectionString => connectionString;
}
