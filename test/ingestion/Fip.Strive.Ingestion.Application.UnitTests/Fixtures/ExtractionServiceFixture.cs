using Fip.Strive.Ingestion.Application.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fip.Strive.Ingestion.Application.UnitTests.Fixtures;

public class ExtractionServiceFixture : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    public IExtractionService Service { get; private set; }

    public ExtractionServiceFixture()
    {
        var services = new ServiceCollection();

        services.AddLogging(c => c.AddProvider(NullLoggerProvider.Instance));
        services.AddExtractionFeature();

        _serviceProvider = services.BuildServiceProvider();

        Service = _serviceProvider.GetRequiredService<IExtractionService>();
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
