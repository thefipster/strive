using AwesomeAssertions;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts;
using TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;
using TheFipster.ActivityAggregator.Importer.UnitTests.Fixtures;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Tests;

public class ClassificationTests(TestFileFixture fixture) : IClassFixture<TestFileFixture>
{
    [Theory]
    [MemberData(nameof(GetPolarFlowCases))]
    [MemberData(nameof(GetGoogleFitbitCases))]
    [MemberData(nameof(GetGoogleTimelineCases))]
    [MemberData(nameof(GetWithingsCases))]
    [MemberData(nameof(GetGarminCases))]
    [MemberData(nameof(GetStravaCases))]
    [MemberData(nameof(GetTheFipsterAppCases))]
    [MemberData(nameof(GetMyCollectionCases))]
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
        PolarFlowClassificationDefinitions.GetClassificationCases();

    public static IEnumerable<object[]> GetGoogleFitbitCases() =>
        GoogleFitbitClassificationDefinitions.GetClassificationCases();

    public static IEnumerable<object[]> GetGoogleTimelineCases() =>
        GoogleTimelineClassificationDefinitions.GetClassificationCases();

    public static IEnumerable<object[]> GetWithingsCases() =>
        WithingsClassificationDefinitions.GetClassificationCases();

    public static IEnumerable<object[]> GetGarminCases() =>
        GarminClassificationDefinition.GetClassificationCases();

    public static IEnumerable<object[]> GetStravaCases() =>
        StravaClassificationDefinition.GetClassificationCases();

    public static IEnumerable<object[]> GetTheFipsterAppCases() =>
        TheFipsterAppClassificationDefinition.GetClassificationCases();

    public static IEnumerable<object[]> GetMyCollectionCases() =>
        MyCollectionClassificationDefinitions.GetClassificationCases();
}
