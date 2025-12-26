using AwesomeAssertions;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.UnitTests.Extensions;
using Fip.Strive.Ingestion.Application.UnitTests.Fixtures;
using Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Activity;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.UnitTests.Vendors.Polar.Flow.Activity;

public class PolarFlowActivityTests(
    ExtractionServiceFixture extractionFixture,
    ClassificationServiceFixture classificationFixture
) : IClassFixture<ClassificationServiceFixture>, IClassFixture<ExtractionServiceFixture>
{
    [Theory]
    [InlineData("Variant 1", PolarFlowActivityTestData.FileVariant1, "2014-10-27")]
    [InlineData("Variant 2", PolarFlowActivityTestData.FileVariant2, "2016-05-09")]
    public void Classify_WithInstance_Classifies(
        string _,
        string fileContent,
        string expectedDateString
    )
    {
        // Arrange
        var expectedDate = DateHelper.ParseDateAsUtc(expectedDateString);
        var classifier = new PolarFlowActivityClassifier();

        using var file = TempFile.Create(fileContent, PolarFlowActivityTestData.Extension);
        var probe = file.GetProbe();

        // Act
        var result = classifier.Classify(probe);

        // Assert
        result.Should().NotBeNull();
        result.Filepath.Should().Be(file.Filepath);
        result.Source.Should().Be(DataSources.PolarFlowActivity);
        result.Datetype.Should().Be(DateRanges.Day);
        result.Datetime.Should().Be(expectedDate);
    }

    [Theory]
    [InlineData("Variant 1", PolarFlowActivityTestData.FileVariant1)]
    [InlineData("Variant 2", PolarFlowActivityTestData.FileVariant2)]
    public void Classify_WithService_UniquelyClassifies(string _, string fileContent)
    {
        // Arrange
        using var file = TempFile.Create(fileContent, PolarFlowActivityTestData.Extension);

        // Act
        var results = classificationFixture.Service.Classify(file.Filepath);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.Where(x => x.IsMatch).Should().HaveCount(1);
        results.First(x => x.IsMatch).Source.Should().Be(DataSources.PolarFlowActivity);
    }

    [Theory]
    [InlineData("Variant 1", PolarFlowActivityTestData.FileVariant1, "2014-10-27", 3)]
    [InlineData("Variant 2", PolarFlowActivityTestData.FileVariant2, "2016-05-09", 2)]
    public async Task Extract_WithService_HasTimestamp(
        string _,
        string fileContent,
        string dateString,
        int extractionCount
    )
    {
        // Arrange
        var expectedDate = DateHelper.ParseDateAsUtc(dateString);
        using var file = TempFile.Create(fileContent, PolarFlowActivityTestData.Extension);

        // Act
        var results = await extractionFixture.Service.ExtractAsync(
            file.Filepath,
            DataSources.PolarFlowActivity
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
        var expectedDate = DateHelper.ParseDateAsUtc("2014-10-27");
        var expectedStepsTime = "2014-10-27 08:11:30Z";
        var expectedMetsTime = "2014-10-27 08:10:30Z";

        var fileContent = PolarFlowActivityTestData.FileVariant1;
        using var file = TempFile.Create(fileContent, PolarFlowActivityTestData.Extension);

        // Act
        var results = await extractionFixture.Service.ExtractAsync(
            file.Filepath,
            DataSources.PolarFlowActivity
        );

        // Assert
        results.Should().NotBeNull();

        results.Extractions.Should().NotBeNull();
        results.Extractions.Should().HaveCount(3);

        results.Extractions.Select(x => x.Kind).Should().OnlyContain(x => x == DataKind.Day);
        results.Extractions.Select(x => x.Timestamp).Should().OnlyContain(x => x == expectedDate);

        results
            .Extractions.First(x => x.Series.Keys.Contains(Parameters.Steps))
            .Series[Parameters.Timestamp]
            .First()
            .Should()
            .Be(expectedStepsTime);

        results
            .Extractions.First(x => x.Series.Keys.Contains(Parameters.MetabolicRate))
            .Series[Parameters.Timestamp]
            .First()
            .Should()
            .Be(expectedMetsTime);
    }

    [Fact]
    public async Task ExtractVariant2_WithNoTimeInfo_HasTimestamp()
    {
        // Arrange
        var expectedDate = DateHelper.ParseDateAsUtc("2016-05-09");
        var expectedStepsTime = "2016-05-09 00:01:00Z";
        var expectedMetsTime = "2016-05-09 00:00:30Z";

        var fileContent = PolarFlowActivityTestData.FileVariant2;
        using var file = TempFile.Create(fileContent, PolarFlowActivityTestData.Extension);

        // Act
        var results = await extractionFixture.Service.ExtractAsync(
            file.Filepath,
            DataSources.PolarFlowActivity
        );

        // Assert
        results.Should().NotBeNull();

        results.Extractions.Should().NotBeNull();
        results.Extractions.Should().HaveCount(2);

        results.Extractions.Select(x => x.Kind).Should().OnlyContain(x => x == DataKind.Day);
        results.Extractions.Select(x => x.Timestamp).Should().OnlyContain(x => x == expectedDate);

        results
            .Extractions.First(x => x.Series.Keys.Contains(Parameters.Steps))
            .Series[Parameters.Timestamp]
            .Last()
            .Should()
            .Be(expectedStepsTime);

        results
            .Extractions.First(x => x.Series.Keys.Contains(Parameters.MetabolicRate))
            .Series[Parameters.Timestamp]
            .Last()
            .Should()
            .Be(expectedMetsTime);
    }
}
