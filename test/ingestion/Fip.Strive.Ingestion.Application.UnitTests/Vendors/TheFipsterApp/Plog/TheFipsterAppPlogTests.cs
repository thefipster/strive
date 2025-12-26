using AwesomeAssertions;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.UnitTests.Extensions;
using Fip.Strive.Ingestion.Application.UnitTests.Fixtures;
using Fip.Strive.Ingestion.Application.Vendors.TheFipsterApp.Plog;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.UnitTests.Vendors.TheFipsterApp.Plog;

public class TheFipsterAppPlogTests(
    ExtractionServiceFixture extractionFixture,
    ClassificationServiceFixture classificationFixture
) : IClassFixture<ClassificationServiceFixture>, IClassFixture<ExtractionServiceFixture>
{
    [Fact]
    public void Classify_WithInstance_Classifies()
    {
        // Arrange
        var classifier = new TheFipsterAppPlogClassifier();
        using var file = TempFile.Create(
            TheFipsterAppPlogTestData.PlogCsvWithOneLine,
            TheFipsterAppPlogTestData.Extension
        );
        var probe = file.GetProbe();

        // Act
        var result = classifier.Classify(probe);

        // Assert
        result.Should().NotBeNull();
        result.Filepath.Should().Be(file.Filepath);
        result.Source.Should().Be(DataSources.TheFipsterAppPlog);
        result.Datetype.Should().Be(DateRanges.Multi);
        result.Datetime.Should().BeNull();
    }

    [Fact]
    public void Classify_WithService_UniquelyClassifies()
    {
        // Arrange
        using var file = TempFile.Create(
            TheFipsterAppPlogTestData.PlogCsvWithOneLine,
            TheFipsterAppPlogTestData.Extension
        );

        // Act
        var results = classificationFixture.Service.Classify(file.Filepath);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.Where(x => x.IsMatch).Should().HaveCount(1);
        results.First(x => x.IsMatch).Source.Should().Be(DataSources.TheFipsterAppPlog);
    }

    [Fact]
    public async Task Extract_WithService_HasTimestamp()
    {
        // Arrange
        var expectedDate = DateHelper.ParseDateAsUtc("2025-11-03");
        var expectedEvent = new DateTime(2025, 11, 3, 7, 50, 0, DateTimeKind.Utc);

        using var file = TempFile.Create(
            TheFipsterAppPlogTestData.PlogCsvWithOneLine,
            TheFipsterAppPlogTestData.Extension
        );

        // Act
        var results = await extractionFixture.Service.ExtractAsync(
            file.Filepath,
            DataSources.TheFipsterAppPlog
        );

        // Assert
        results.Should().NotBeNull();

        results.Extractions.Should().NotBeNull();
        results.Extractions.Should().HaveCount(1);
        results.Extractions.Select(x => x.Kind).Should().OnlyContain(x => x == DataKind.Day);
        results.Extractions.Select(x => x.Timestamp).Should().OnlyContain(x => x == expectedDate);

        results.Extractions.First().Events.Should().NotBeNull();
        results.Extractions.First().Events.Should().HaveCount(1);
        results.Extractions.First().Events.First().Timestamp.Should().Be(expectedEvent);
    }
}
