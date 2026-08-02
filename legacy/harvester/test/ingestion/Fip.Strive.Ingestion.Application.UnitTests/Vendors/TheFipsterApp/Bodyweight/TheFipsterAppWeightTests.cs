using AwesomeAssertions;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.UnitTests.Extensions;
using Fip.Strive.Ingestion.Application.UnitTests.Fixtures;
using Fip.Strive.Ingestion.Application.Vendors.TheFipsterApp.Bodyweight;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using DateHelper = Fip.Strive.Ingestion.Application.UnitTests.Extensions.DateHelper;

namespace Fip.Strive.Ingestion.Application.UnitTests.Vendors.TheFipsterApp.Bodyweight;

public class TheFipsterAppWeightTests(
    ExtractionServiceFixture extractionFixture,
    ClassificationServiceFixture classificationFixture
) : IClassFixture<ClassificationServiceFixture>, IClassFixture<ExtractionServiceFixture>
{
    [Fact]
    public void Classify_WithInstance_Classifies()
    {
        // Arrange
        var classifier = new TheFipsterAppWeightClassifier();
        using var file = TempFile.Create(
            TheFipsterAppWeightTestData.WeightJsonWithOneEntry,
            TheFipsterAppWeightTestData.Extension
        );
        var probe = new FileProbe(file.Filepath);

        // Act
        var result = classifier.Classify(probe);

        // Assert
        result.Should().NotBeNull();
        result.Filepath.Should().Be(file.Filepath);
        result.Source.Should().Be(DataSources.TheFipsterAppWeight);
        result.Datetype.Should().Be(DateRanges.Multi);
        result.Datetime.Should().BeNull();
    }

    [Fact]
    public void Classify_WithService_UniquelyClassifies()
    {
        // Arrange
        using var file = TempFile.Create(
            TheFipsterAppWeightTestData.WeightJsonWithOneEntry,
            TheFipsterAppWeightTestData.Extension
        );

        // Act
        var results = classificationFixture.Service.Classify(file.Filepath);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.Where(x => x.IsMatch).Should().HaveCount(1);
        results.First(x => x.IsMatch).Source.Should().Be(DataSources.TheFipsterAppWeight);
    }

    [Fact]
    public async Task Extract_WithService_HasTimestamp()
    {
        // Arrange
        var expectedDate = DateHelper.ParseDate("2025-10-23");
        var expectedEvent = new DateTime(2025, 10, 23, 10, 34, 56, DateTimeKind.Utc);

        using var file = TempFile.Create(
            TheFipsterAppWeightTestData.WeightJsonWithOneEntry,
            TheFipsterAppWeightTestData.Extension
        );

        // Act
        var results = await extractionFixture.Service.ExtractAsync(
            file.Filepath,
            DataSources.TheFipsterAppWeight
        );

        // Assert
        results.Should().NotBeNull();

        results.Extractions.Should().NotBeNull();
        results.Extractions.Should().HaveCount(1);
        results.Extractions.First().Kind.Should().Be(DataKind.Day);
        results.Extractions.First().Timestamp.Should().Be(expectedDate);

        results.Extractions.First().Events.Should().NotBeNull();
        results.Extractions.First().Events.Should().HaveCount(1);
        results.Extractions.First().Events.First().Timestamp.Should().Be(expectedEvent);
    }
}
