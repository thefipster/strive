using AwesomeAssertions;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;
using TheFipster.ActivityAggregator.Importer.UnitTests.Fixtures;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Tests.Extraction;

public class GpxImporterTests(TestFileFixture fixture) : IClassFixture<TestFileFixture>
{
    [Theory]
    [InlineData(Vendors.MyCollection, "2009.02.09 - 1717\\GpsX.gpx")]
    public void ExtractGpsPoints_FromGpxFile_ReturnsGpsSeries(string vendor, string filepath)
    {
        // Arrange
        var expectedTimestamp = new DateTime(2009, 02, 09, 17, 17, 0);

        var probe = fixture.GetFileProbe(vendor, filepath);
        var importer = Activator.CreateInstance(typeof(GenericGpxExtractor)) as IFileExtractor;
        importer.Should().NotBeNull();

        var index = new ExtractionRequest
        {
            Date = expectedTimestamp,
            Filepath = probe.Filepath,
            Range = DateRanges.Time,
            Source = DataSources.Gpx,
        };

        // Act
        var result = importer.Extract(index);

        // Assert
        importer.Source.Should().Be(DataSources.Gpx);
        result.Should().NotBeNull().And.NotBeEmpty();
        result.First().Series.Should().NotBeNull().And.NotBeEmpty();
        result.First().Timestamp.Should().Be(expectedTimestamp);
    }
}
