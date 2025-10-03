using AwesomeAssertions;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Polar;

namespace TheFipster.ActivityAggregator.Importer.UnitTests;

public class PolarTakeoutGenericPeriodEcgImporterTests
{
    [Fact]
    public void ClassifyFile_OfCorrectType_ReturnsClassificastion()
    {
        var file = "E:\\polar\\test\\polar_takeout\\generic_periods\\ecg.json";
        var importer = new PolarTakeoutGenericPeriodEcgImporter();
        var probe = new FileProbe(file);

        var result = importer.Classify(probe);

        result.Source.Should().Be(DataSources.PolarTakeoutGenericPeriodEcg);
    }
}
