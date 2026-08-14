using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;

namespace Fip.Strive.Application.UnitTests.Jobs;

public class JobRegistryTests
{
    [Fact]
    public void Resolves_a_components_identity_by_its_kind()
    {
        var registry = new JobRegistry([new StubHandler("unpack", "unpack", 3)]);

        var component = registry.Resolve("unpack");

        component.ComponentId.Should().Be("unpack");
        component.Version.Should().Be(3);
    }

    [Fact]
    public void An_unknown_kind_is_an_error_rather_than_a_silent_no_op()
    {
        var registry = new JobRegistry([new StubHandler("unpack", "unpack", 1)]);

        var act = () => registry.Resolve("classify");

        act.Should().Throw<InvalidOperationException>().WithMessage("*classify*");
    }

    [Fact]
    public void Two_handlers_claiming_one_kind_fail_at_construction()
    {
        // Picking one arbitrarily would mean the handler that ran was decided by DI registration
        // order, which is invisible at the point anything goes wrong.
        var act = () =>
            new JobRegistry([new StubHandler("unpack", "a", 1), new StubHandler("unpack", "b", 1)]);

        act.Should().Throw<InvalidOperationException>().WithMessage("*unpack*");
    }

    [Fact]
    public void Every_registered_component_is_listed()
    {
        var registry = new JobRegistry([
            new StubHandler("unpack", "unpack", 1),
            new StubHandler("classify", "classify", 4),
        ]);

        registry
            .All.Select(component => (component.ComponentId, component.Version))
            .Should()
            .BeEquivalentTo([("unpack", 1), ("classify", 4)]);
    }

    private sealed class StubHandler(string kind, string componentId, int version) : IJobHandler
    {
        public string Kind => kind;

        public string ComponentId => componentId;

        public int Version => version;

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
