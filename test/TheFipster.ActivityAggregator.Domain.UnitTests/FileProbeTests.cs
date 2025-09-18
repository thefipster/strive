using AwesomeAssertions;
using TheFipster.ActivityAggregator.Domain.Tools;

namespace TheFipster.ActivityAggregator.Domain.UnitTests;

public class FileProbeTests
{
    [Fact]
    public void ProbeJsonFile_AllReadVariants_ReturnsTextLinesAndProps()
    {
        var file =
            "E:\\polar\\import\\polar_takeout\\247ohr_2017_10-788de140-3a96-4f44-a833-d19bfbed5fcc.json";
        var bufferSize = 3000;
        var probe = new FileProbe(file, bufferSize);

        var isText = probe.IsText();
        var text = probe.GetText();
        var lines = probe.GetLines();
        var props = probe.GetJsonPropertyNames();
        var values = probe.GetJsonPropertiesWithValues();

        isText.Should().BeTrue();
        text.Length.Should().Be(bufferSize);
        lines.Should().HaveCountGreaterThan(0);
        props.Should().HaveCountGreaterThan(0);
        values.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ProbeJsonFile_GetPropertyName_ReturnsMultipleProps()
    {
        var file =
            "E:\\polar\\import\\polar_takeout\\generic-period-0d442000-017f-2125-b368-14c0a1b78d76-94c4801f-bd9f-4e74-b680-a72724bab751.json";
        var bufferSize = 3000;
        var probe = new FileProbe(file, bufferSize);

        var isText = probe.IsText();
        var props = probe.GetJsonPropertyNames();

        isText.Should().BeTrue();
        props.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ProbeXmlFile_AllReadVariants_ReturnsTextLinesAndProps()
    {
        var file =
            "E:\\polar\\import\\my_collection\\2011\\2011.04.01 - 1029\\Rungps01042011110924.gpx";
        var bufferSize = 3000;
        var probe = new FileProbe(file, bufferSize);

        var isText = probe.IsText();
        var text = probe.GetText();
        var lines = probe.GetLines();
        var props = probe.GetXmlPropsAndAttributes();

        isText.Should().BeTrue();
        text.Length.Should().Be(bufferSize);
        lines.Should().HaveCountGreaterThan(0);
        props.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ProbeImageFile_CheckIsText_ShouldBeFalse()
    {
        var file =
            "E:\\polar\\import\\my_collection\\2008\\2008.06.15 - 1329\\Report-Dateien\\1.png";
        var bufferSize = 3000;
        var probe = new FileProbe(file, bufferSize);

        var isText = probe.IsText();

        isText.Should().BeFalse();
    }
}
