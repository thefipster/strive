using AwesomeAssertions;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;
using TheFipster.ActivityAggregator.Importer.UnitTests.Fixtures;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Tests.Extraction;

public class TheFipsterAppWeightImporterTests(TestFileFixture fixture)
    : IClassFixture<TestFileFixture>
{
    [Theory]
    [InlineData(Vendors.TheFipsterApp, "weights.json")]
    public void ExtractBodyweights_FromWeightsFile_ReturnsExtraction(string vendor, string filepath)
    {
        // Arrange
        var probe = fixture.GetFileProbe(vendor, filepath);
        var importer =
            Activator.CreateInstance(typeof(TheFipsterAppWeightExtractor)) as IFileExtractor;

        importer.Should().NotBeNull();

        var index = new ExtractionRequest
        {
            Date = DateTime.MinValue,
            Filepath = probe.Filepath,
            Range = DateRanges.Time,
            Source = DataSources.TheFipsterAppWeight,
        };

        // Act
        var result = importer.Extract(index);

        // Assert
        importer.Source.Should().Be(DataSources.TheFipsterAppWeight);
        result.Should().NotBeNull().And.NotBeEmpty();

        foreach (var item in result)
        {
            item.Series.Should().NotBeNull().And.NotBeEmpty();
            item.Attributes.Should().NotBeNull().And.NotBeEmpty();
            item.Events.Should().NotBeNull().And.NotBeEmpty();
        }
    }
}
