using AwesomeAssertions;
using Fip.Strive.Ingestion.Application.UnitTests.Data.TheFipsterApp;
using Fip.Strive.Ingestion.Application.UnitTests.Extensions;
using Fip.Strive.Ingestion.Application.UnitTests.Fixtures;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.UnitTests;

public class ExtractionTests(ExtractionServiceFixture fixture)
    : IClassFixture<ExtractionServiceFixture>
{
    [Theory]
    [InlineData(
        TheFipsterAppPlogTestData.PlogCsvWithOneLine,
        TheFipsterAppPlogTestData.Extension,
        DataSources.TheFipsterAppPlog,
        2025,
        11,
        03,
        7,
        50
    )]
    [InlineData(
        TheFipsterAppWeightTestData.WeightJsonWithOneEntry,
        TheFipsterAppWeightTestData.Extension,
        DataSources.TheFipsterAppWeight,
        2025,
        10,
        23,
        10,
        34,
        56
    )]
    public async Task Classify_WithAllClassifiers_ResultsInSingleClassification(
        string content,
        string extension,
        DataSources expectedSource,
        int year,
        int month,
        int day,
        int hour = 0,
        int minute = 0,
        int second = 0
    )
    {
        // Arrange
        using var file = TempFile.Create(content, extension);

        // Act
        var results = await fixture.ExtractionService.ExtractAsync(file.Filepath, expectedSource);

        // Assert
        results.Should().NotBeNull();
        results.Extractions.Should().NotBeNull();
        results.Extractions.Should().HaveCount(1);
        results.Extractions.First().Events.Should().NotBeNull();
        results.Extractions.First().Events.Should().HaveCount(1);
        results.Extractions.First().Events.First().Timestamp.Kind.Should().Be(DateTimeKind.Utc);
        results.Extractions.First().Events.First().Timestamp.Year.Should().Be(year);
        results.Extractions.First().Events.First().Timestamp.Month.Should().Be(month);
        results.Extractions.First().Events.First().Timestamp.Day.Should().Be(day);
        results.Extractions.First().Events.First().Timestamp.Hour.Should().Be(hour);
        results.Extractions.First().Events.First().Timestamp.Minute.Should().Be(minute);
        results.Extractions.First().Events.First().Timestamp.Second.Should().Be(second);
    }
}
