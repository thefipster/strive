using AwesomeAssertions;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;
using TheFipster.ActivityAggregator.Importer.UnitTests.Fixtures;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Tests.Extraction;

public class RunGpsCsv2ImporterTests(TestFileFixture fixture) : IClassFixture<TestFileFixture>
{
    [Theory]
    [InlineData(Vendors.MyCollection, "2011.04.03 - 1248\\unbenannt.csv")]
    public void ExtractCsvData_FromRunGpsV2File_ReturnsSampleSeries(string vendor, string filepath)
    {
        // Arrange
        var expectedTimestamp = new DateTime(2011, 04, 03, 12, 48, 47).AddMilliseconds(401);
        var expectedDataSource = DataSources.RunGpsCsvV2;

        var probe = fixture.GetFileProbe(vendor, filepath);
        var importer = Activator.CreateInstance(typeof(RunGpsCsv2Extractor)) as IFileExtractor;
        importer.Should().NotBeNull();

        var index = new ExtractionRequest
        {
            Date = expectedTimestamp,
            Filepath = probe.Filepath,
            Range = DateRanges.Time,
            Source = expectedDataSource,
        };

        // Act
        var result = importer.Extract(index);

        // Assert
        importer.Source.Should().Be(expectedDataSource);
        result.Should().NotBeNull().And.NotBeEmpty();
        result.First().Series.Should().NotBeNull().And.NotBeEmpty();
        result.First().Timestamp.Should().Be(expectedTimestamp);
    }
}
