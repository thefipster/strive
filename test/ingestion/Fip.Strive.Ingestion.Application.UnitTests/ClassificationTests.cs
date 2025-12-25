using AwesomeAssertions;
using Fip.Strive.Ingestion.Application.UnitTests.Data.TheFipsterApp;
using Fip.Strive.Ingestion.Application.UnitTests.Extensions;
using Fip.Strive.Ingestion.Application.UnitTests.Fixtures;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.UnitTests;

public class ClassificationTests(ClassificationServiceFixture fixture)
    : IClassFixture<ClassificationServiceFixture>
{
    [Theory]
    [InlineData(
        TheFipsterAppWeightTestData.WeightJsonWithOneEntry,
        TheFipsterAppWeightTestData.Extension,
        DataSources.TheFipsterAppWeight
    )]
    [InlineData(
        TheFipsterAppPlogTestData.PlogCsvWithOneLine,
        TheFipsterAppPlogTestData.Extension,
        DataSources.TheFipsterAppPlog
    )]
    public void Classify_WithAllClassifiers_ResultsInSingleClassification(
        string content,
        string extension,
        DataSources expectedSource
    )
    {
        // Arrange
        using var file = TempFile.Create(content, extension);

        // Act
        var results = fixture.ClassificationService.Classify(file.Filepath);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.Where(x => x.IsMatch).Should().HaveCount(1);
        results.First(x => x.IsMatch).Source.Should().Be(expectedSource);
    }
}
