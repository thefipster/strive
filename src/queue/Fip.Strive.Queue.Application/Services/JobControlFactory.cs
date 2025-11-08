using Fip.Strive.Queue.Storage.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Queue.Application.Services;

public class JobControlFactory(IServiceScopeFactory scopeFactory)
{
    public IJobControl GetScoped() =>
        scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IJobControl>();
}
