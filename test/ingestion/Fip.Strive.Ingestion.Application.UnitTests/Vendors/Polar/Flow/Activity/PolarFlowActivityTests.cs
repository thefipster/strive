using AwesomeAssertions;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.UnitTests.Extensions;
using Fip.Strive.Ingestion.Application.UnitTests.Fixtures;
using Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Activity;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using DateHelper = Fip.Strive.Ingestion.Application.UnitTests.Extensions.DateHelper;

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
        var expectedDate = DateHelper.ParseDate(expectedDateString);
        var classifier = new PolarFlowActivityClassifier();

        using var file = TempFile.Create(fileContent, PolarFlowActivityTestData.Extension);
        var probe = new FileProbe(file.Filepath);

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
        var expectedDate = DateHelper.ParseDate(dateString);
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

        results.Extractions.First().Kind.Should().Be(DataKind.Day);
        results.Extractions.First().Timestamp.Should().Be(expectedDate);
    }
}
