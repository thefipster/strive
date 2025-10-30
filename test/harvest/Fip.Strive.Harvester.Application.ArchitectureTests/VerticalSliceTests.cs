using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Fip.Strive.Core.Tests.Extensions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Fip.Strive.Harvester.Application.ArchitectureTests;

public class VerticalSliceTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(typeof(Registration).Assembly)
        .Build();

    private const string ApplicationNamespace = "Fip.Strive.Harvester.Application";
    private const string FeaturesNamespace = "Fip.Strive.Harvester.Application.Features";
    private const string CoreNamespace = "Fip.Strive.Harvester.Application.Core";
    private const string InfrastructureNamespace =
        "Fip.Strive.Harvester.Application.Infrastructure";

    [Fact]
    public void Features_Should_Not_Depend_On_Each_Other()
    {
        // Dynamically discover all feature namespaces (top-level only)
        var allFeatureNamespaces = Architecture
            .Namespaces.Where(ns =>
                ns.FullName.StartsWith(FeaturesNamespace)
                && ns.FullName.Split('.').Length == FeaturesNamespace.Split('.').Length + 1
            )
            .Select(ns => ns.FullName)
            .Distinct()
            .ToList();

        // Each feature should not depend on any other feature
        foreach (var featureNamespace in allFeatureNamespaces)
        {
            var otherFeatures = allFeatureNamespaces.Where(ns => ns != featureNamespace).ToList();

            if (otherFeatures.Any())
            {
                var rule = Types()
                    .That()
                    .ResideInNamespaceMatching(featureNamespace)
                    .Should()
                    .NotDependOnAny(
                        otherFeatures
                            .Aggregate(
                                Types().That().ResideInNamespaceMatching(otherFeatures.First()),
                                (current, ns) => current.Or().ResideInNamespaceMatching(ns)
                            )
                            .As($"Other features (excluding {featureNamespace})")
                    )
                    .Because(
                        $"feature {featureNamespace} should be independent and not reference other features"
                    );

                rule.Check(Architecture);
            }
        }

        // Ensure we actually found features to test
        Assert.True(
            allFeatureNamespaces.Count > 0,
            $"No features found to check in namespace {FeaturesNamespace}."
        );
    }

    // [Fact]
    // public void Core_Should_Not_Depend_On_Features()
    // {
    //     // Dynamically discover all feature namespaces
    //     var allFeatureNamespaces = Architecture
    //         .Namespaces.Where(ns =>
    //             ns.FullName.StartsWith(FeaturesNamespace)
    //             && ns.FullName.Split('.').Length == FeaturesNamespace.Split('.').Length + 1
    //         )
    //         .Select(ns => ns.FullName)
    //         .Distinct()
    //         .ToList();
    //
    //     var allCoreNamespaces = Architecture
    //         .Namespaces.Where(ns =>
    //             ns.FullName.StartsWith(CoreNamespace) && ns.FullName.Length > CoreNamespace.Length
    //         )
    //         .Select(ns => ns.GetNamespaceOfFeature(CoreNamespace))
    //         .Distinct()
    //         .ToList();
    //
    //     foreach (var feature in allFeatureNamespaces)
    //     {
    //         foreach (var core in allCoreNamespaces)
    //         {
    //             var rule = Types()
    //                 .That()
    //                 .ResideInNamespace(core)
    //                 .Should()
    //                 .NotDependOnAny(
    //                     Types().That().ResideInNamespace(feature).As($"Feature {feature}")
    //                 )
    //                 .Because($"{core} should not depend on Feature {feature}");
    //
    //             rule.Check(Architecture);
    //         }
    //     }
    // }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Features()
    {
        Types()
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespace(FeaturesNamespace).As("Features"))
            .Because("Infrastructure should not depend on Features")
            .Check(Architecture);
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Core()
    {
        Types()
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespace(CoreNamespace).As("Features"))
            .Because("Infrastructure should not depend on Core")
            .Check(Architecture);
    }
}
