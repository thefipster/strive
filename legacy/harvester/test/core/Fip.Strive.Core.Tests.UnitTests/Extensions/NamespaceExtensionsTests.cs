using ArchUnitNET.Domain;
using AwesomeAssertions;
using Fip.Strive.Core.Tests.Extensions;

namespace Fip.Strive.Core.Tests.UnitTests.Extensions
{
    public class NamespaceExtensionsTests
    {
        [Fact]
        public void GetNamespaceOfFeature_returns_root_dot_feature_when_full_contains_feature()
        {
            var root = "Company.Project";
            var full = "Company.Project.FeatureX.Subsystem";
            var ns = new Namespace(full, []);

            var result = ns.GetNamespaceOfFeature(root);

            result.Should().Be("Company.Project.FeatureX");
        }

        [Fact]
        public void GetNamespaceOfFeature_throws_argument_exception_when_full_does_not_start_with_root()
        {
            var root = "Company.Project";
            var full = "Other.Company.Project.Feature";
            var ns = new Namespace(full, []);

            Action act = () => ns.GetNamespaceOfFeature(root);

            act.Should().Throw<ArgumentException>()
                .WithMessage($"{root} is not part of {full}");
        }

        [Fact]
        public void GetNamespaceOfFeature_throws_index_out_of_range_when_full_equals_root()
        {
            var root = "Company.Project";
            var full = "Company.Project";
            var ns = new Namespace(full, []);

            Action act = () => ns.GetNamespaceOfFeature(root);

            act.Should().Throw<ArgumentException>().WithMessage($"{root} is not a feature");
        }
    }
}