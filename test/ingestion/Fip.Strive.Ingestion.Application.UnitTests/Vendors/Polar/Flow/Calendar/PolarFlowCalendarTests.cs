using AwesomeAssertions;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.UnitTests.Extensions;
using Fip.Strive.Ingestion.Application.UnitTests.Fixtures;
using Fip.Strive.Ingestion.Application.UnitTests.Vendors.Polar.Flow.Activity;
using Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Activity;
using Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Calendar;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.UnitTests.Vendors.Polar.Flow.Calendar;

public class PolarFlowCalendarTests(
    ExtractionServiceFixture extractionFixture,
    ClassificationServiceFixture classificationFixture
) : IClassFixture<ClassificationServiceFixture>, IClassFixture<ExtractionServiceFixture>
{
    [Fact]
    public void Classify_WithInstance_Classifies()
    {
        // Arrange
        var classifier = new PolarFlowCalendarItemsClassifier();
        using var file = TempFile.Create(
            PolarFlowCalendarTestData.File,
            PolarFlowCalendarTestData.Extension
        );
        var probe = file.GetProbe();

        // Act
        var result = classifier.Classify(probe);

        // Assert
        result.Should().NotBeNull();
        result.Filepath.Should().Be(file.Filepath);
        result.Source.Should().Be(DataSources.PolarFlowCalendarItems);
        result.Datetype.Should().Be(DateRanges.Multi);
        result.Datetime.Should().BeNull();
    }

    [Fact]
    public void Classify_WithService_UniquelyClassifies()
    {
        // Arrange
        using var file = TempFile.Create(
            PolarFlowCalendarTestData.File,
            PolarFlowCalendarTestData.Extension
        );

        // Act
        var results = classificationFixture.Service.Classify(file.Filepath);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.Where(x => x.IsMatch).Should().HaveCount(1);
        results.First(x => x.IsMatch).Source.Should().Be(DataSources.PolarFlowCalendarItems);
    }

    [Fact]
    public async Task Extract_WithService_HasTimestamp()
    {
        // Arrange
        var expectedDate = DateHelper.ParseDateAsUtc("2016-07-09");
        using var file = TempFile.Create(
            PolarFlowCalendarTestData.File,
            PolarFlowCalendarTestData.Extension
        );

        // Act
        var results = await extractionFixture.Service.ExtractAsync(
            file.Filepath,
            DataSources.PolarFlowCalendarItems
        );

        // Assert
        results.Should().NotBeNull();

        results.Extractions.Should().NotBeNull();
        results.Extractions.Should().HaveCount(1);
        results.Extractions.First().Timestamp.Should().Be(expectedDate);

        results.Extractions.First().Attributes.Should().NotBeNull();
        results.Extractions.First().Attributes.Should().HaveCount(1);
    }
}
