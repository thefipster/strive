using Fip.Strive.Ingestion.Application.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fip.Strive.Ingestion.Application.UnitTests.Fixtures;

public class ClassificationServiceFixture : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    public IClassificationService ClassificationService { get; private set; }

    public ClassificationServiceFixture()
    {
        var services = new ServiceCollection();

        services.AddLogging(c => c.AddProvider(NullLoggerProvider.Instance));
        services.AddClassificationFeature();

        _serviceProvider = services.BuildServiceProvider();

        ClassificationService = _serviceProvider.GetRequiredService<IClassificationService>();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _serviceProvider.Dispose();
        return Task.CompletedTask;
    }
}
