using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Application.UnitTests;

public class RegistrationTests
{
    [Fact]
    public void AddApplication_builds_a_resolvable_container()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var act = () => services.BuildServiceProvider(validateScopes: true);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddApplication_returns_the_same_collection_for_chaining()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        result.Should().BeSameAs(services);
    }
}
