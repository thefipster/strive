using FluentAssertions;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Polar;

namespace TheFipster.ActivityAggregator.Importer.UnitTests;

public class PolarTakeoutGenericPeriodTemperatureImporterTests
{
    [Fact]
    public void ClassifyFile_OfCorrectType_ReturnsClassificastion()
    {
        var file = "E:\\polar\\test\\polar_takeout\\generic_periods\\temperature.json";
        var importer = new PolarTakeoutGenericPeriodTemperatureImporter();
        var probe = new FileProbe(file);

        var result = importer.Classify(probe);

        result.Source.Should().Be(DataSources.PolarTakeoutGenericPeriodTemperature);
    }
}
