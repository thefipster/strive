using AwesomeAssertions;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Polar;

namespace TheFipster.ActivityAggregator.Importer.UnitTests;

public class PolarTakeout247OhrImporterTests
{
    [Fact]
    public void ClassifyFile_OfCorrectType_ReturnsClassificastion()
    {
        var expectedDate = DateTime.Parse("2017-10-07");

        var file =
            "E:\\polar\\import\\polar_takeout\\247ohr_2017_10-788de140-3a96-4f44-a833-d19bfbed5fcc.json";
        var importer = new PolarTakeout247OhrImporter();
        var probe = new FileProbe(file);

        var result = importer.Classify(probe);

        result.Source.Should().Be(DataSources.PolarTakeout247Ohr);
        result.Datetime.Should().Be(expectedDate);
    }
}
