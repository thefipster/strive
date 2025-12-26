using AwesomeAssertions;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.UnitTests.Extensions;
using Fip.Strive.Ingestion.Application.UnitTests.Fixtures;
using Fip.Strive.Ingestion.Application.UnitTests.Vendors.Polar.Flow.Activity;
using Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.SleepResult;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.UnitTests.Vendors.Polar.Flow.SleepResult;

public class PolarFlowSleepResultTests(
    ExtractionServiceFixture extractionFixture,
    ClassificationServiceFixture classificationFixture
) : IClassFixture<ClassificationServiceFixture>, IClassFixture<ExtractionServiceFixture>
{
    [Theory]
    [InlineData("Variant 1", PolarFlowSleepResultTestData.FileVariant1, "2017-10-08")]
    [InlineData("Variant 2", PolarFlowSleepResultTestData.FileVariant2, "2025-12-10")]
    public void Classify_WithInstance_Classifies(
        string _,
        string fileContent,
        string expectedDateString
    )
    {
        // Arrange
        var expectedDate = DateHelper.ParseDateAsUtc(expectedDateString);
        var classifier = new PolarFlowSleepResultClassifier();

        using var file = TempFile.Create(fileContent, PolarFlowSleepResultTestData.Extension);
        var probe = file.GetProbe();

        // Act
        var result = classifier.Classify(probe);

        // Assert
        result.Should().NotBeNull();
        result.Filepath.Should().Be(file.Filepath);
        result.Source.Should().Be(DataSources.PolarFlowSleepResult);
        result.Datetype.Should().Be(DateRanges.Multi);
        result.Datetime.Should().BeNull();
    }

    [Theory]
    [InlineData("Variant 1", PolarFlowSleepResultTestData.FileVariant1)]
    [InlineData("Variant 2", PolarFlowSleepResultTestData.FileVariant2)]
    public void Classify_WithService_UniquelyClassifies(string _, string fileContent)
    {
        // Arrange
        using var file = TempFile.Create(fileContent, PolarFlowSleepResultTestData.Extension);

        // Act
        var results = classificationFixture.Service.Classify(file.Filepath);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.Where(x => x.IsMatch).Should().HaveCount(1);
        results.First(x => x.IsMatch).Source.Should().Be(DataSources.PolarFlowSleepResult);
    }

    [Theory]
    [InlineData("Variant 1", PolarFlowSleepResultTestData.FileVariant1, "2017-10-08", 1)]
    [InlineData("Variant 2", PolarFlowSleepResultTestData.FileVariant2, "2025-12-10", 1)]
    public async Task Extract_WithService_HasTimestamp(
        string _,
        string fileContent,
        string dateString,
        int extractionCount
    )
    {
        // Arrange
        var expectedDate = DateHelper.ParseDateAsUtc(dateString);
        using var file = TempFile.Create(fileContent, PolarFlowSleepResultTestData.Extension);

        // Act
        var results = await extractionFixture.Service.ExtractAsync(
            file.Filepath,
            DataSources.PolarFlowSleepResult
        );

        // Assert
        results.Should().NotBeNull();

        results.Extractions.Should().NotBeNull();
        results.Extractions.Should().HaveCount(extractionCount);

        results.Extractions.Select(x => x.Kind).Should().OnlyContain(x => x == DataKind.Day);
        results.Extractions.Select(x => x.Timestamp).Should().OnlyContain(x => x == expectedDate);
    }

    [Fact]
    public async Task ExtractVariant1_WithTimeInfo_HasTimestamp()
    {
        // Arrange
        var expectedDate = DateHelper.ParseDateAsUtc("2017-10-08");
        var expectedStartTime = "2017-10-07 22:55:57Z";
        var expectedEndTime = "2017-10-08 08:21:27Z";

        var fileContent = PolarFlowSleepResultTestData.FileVariant1;
        using var file = TempFile.Create(fileContent, PolarFlowSleepResultTestData.Extension);

        // Act
        var results = await extractionFixture.Service.ExtractAsync(
            file.Filepath,
            DataSources.PolarFlowSleepResult
        );

        // Assert
        results.Should().NotBeNull();

        results.Extractions.Should().NotBeNull();
        results.Extractions.Should().HaveCount(1);

        results.Extractions.Select(x => x.Kind).Should().OnlyContain(x => x == DataKind.Day);
        results.Extractions.Select(x => x.Timestamp).Should().OnlyContain(x => x == expectedDate);

        results
            .Extractions.First()
            .Attributes[Parameters.SleepStart]
            .Should()
            .Be(expectedStartTime);
        results.Extractions.First().Attributes[Parameters.SleepEnd].Should().Be(expectedEndTime);
    }

    [Fact]
    public async Task ExtractVariant2_WithNoTimeInfo_HasTimestamp()
    {
        // Arrange
        var expectedDate = DateHelper.ParseDateAsUtc("2025-12-10");
        var expectedStartTime = "2025-12-10 00:27:00Z";
        var expectedEndTime = "2025-12-10 08:54:00Z";

        var fileContent = PolarFlowSleepResultTestData.FileVariant2;
        using var file = TempFile.Create(fileContent, PolarFlowSleepResultTestData.Extension);

        // Act
        var results = await extractionFixture.Service.ExtractAsync(
            file.Filepath,
            DataSources.PolarFlowSleepResult
        );

        // Assert
        results.Should().NotBeNull();

        results.Extractions.Should().NotBeNull();
        results.Extractions.Should().HaveCount(1);

        results.Extractions.Select(x => x.Kind).Should().OnlyContain(x => x == DataKind.Day);
        results.Extractions.Select(x => x.Timestamp).Should().OnlyContain(x => x == expectedDate);

        results
            .Extractions.First()
            .Attributes[Parameters.SleepStart]
            .Should()
            .Be(expectedStartTime);
        results.Extractions.First().Attributes[Parameters.SleepEnd].Should().Be(expectedEndTime);
    }
}
