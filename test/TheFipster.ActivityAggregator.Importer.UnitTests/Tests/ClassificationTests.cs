using AwesomeAssertions;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;
using TheFipster.ActivityAggregator.Importer.UnitTests.Fixtures;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Tests;

public class ClassificationTests(TestFileFixture fixture) : IClassFixture<TestFileFixture>
{
    [Theory]
    [MemberData(nameof(GetPolarFlowCases))]
    [MemberData(nameof(GetGoogleFitbitCases))]
    public void ClassifyFile_OfCorrectType_ReturnsClassification(FileClassificationCase test)
    {
        // Arrange
        var expectedDate = DateTime.Parse(test.Date);
        var probe = fixture.GetFileProbe(test.Vendor, test.Filename);
        var importer = (IFileClassifier)Activator.CreateInstance(test.ImporterType)!;

        // Act
        var result = importer.Classify(probe);

        // Assert
        importer.Source.Should().Be(test.ExpectedSource);
        result.Source.Should().Be(test.ExpectedSource);
        result.Datetime.Should().Be(expectedDate);
        result.Datetype.Should().Be(test.ExpectedRange);
    }

    public static IEnumerable<object[]> GetPolarFlowCases() =>
        PolarTakeoutClassificationDefinitions.GetClassificationCases();

    public static IEnumerable<object[]> GetGoogleFitbitCases() =>
        FitbitTakeoutClassificationDefinitions.GetClassificationCases();
}
