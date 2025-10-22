using AwesomeAssertions;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.UnitTests.Fixtures;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Tests;

public class PolarTakeout247OhrImporterTests(TestFileFixture fixture)
    : IClassFixture<TestFileFixture>
{
    [Theory]
    [InlineData("swim_lengths_data_readme.txt")]
    public void ClassifyFile_NotBeingJson_ThrowsClassificationException(string filename)
    {
        // Arrange
        var probe = fixture.GetFileProbe(Vendors.Trash, filename);
        var importer = new PolarFlow247OhrClassifier();

        // Act
        Action act = () => importer.Classify(probe);

        // Assert
        act.Should().Throw<ClassificationException>();
    }

    [Theory]
    [InlineData("random.json")]
    public void ClassifyFile_BeingAnUnknownJson_ThrowsClassificationException(string filename)
    {
        // Arrange
        var probe = fixture.GetFileProbe(Vendors.Trash, filename);
        var importer = new PolarFlow247OhrClassifier();

        // Act
        Action act = () => importer.Classify(probe);

        // Assert
        act.Should().Throw<ClassificationException>();
    }
}
