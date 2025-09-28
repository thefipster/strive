using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Services.Components;
using TheFipster.ActivityAggregator.Services.Worker;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Services.UnitTests;

public class ScannerTests
{
    [Theory]
    [InlineData(
        "E:\\polar\\test\\scanner\\7b4492de053fd7e84ff11f32066fa403-PolarTakeoutGenericPeriod-polar_takeout_generic_period.json"
    )]
    [InlineData(
        "E:\\polar\\test\\scanner\\95fcab8975dcfaa9ddd63f01bc2c0ff5-PolarTakeoutGenericPeriod-polar_takeout_generic_period.json"
    )]
    [InlineData(
        "E:\\polar\\test\\scanner\\ab512965debd4573e54e9cd3d6a2332f-PolarTakeoutActivity-polar_takeout_activity.json"
    )]
    public async Task ScanValidFiles_ThatAreNotIndexed_ResultsInClassification(string filepath)
    {
        // Arrange
        var scanner = CreateScanner();

        // Act
        var result = await scanner.CheckAsync(filepath, "", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Classification.Should().NotBeNull();
        result.Files.Should().HaveCount(1);
        result.Conflicts.Should().BeEmpty();
        result.Actions.Should().HaveCount(1);
        result.Actions.Last().Value.Should().Be(ScannerActions.Classified);
    }

    [Theory]
    [InlineData(
        "E:\\polar\\test\\scanner\\7b4492de053fd7e84ff11f32066fa403-PolarTakeoutGenericPeriod-polar_takeout_generic_period.json"
    )]
    [InlineData(
        "E:\\polar\\test\\scanner\\95fcab8975dcfaa9ddd63f01bc2c0ff5-PolarTakeoutGenericPeriod-polar_takeout_generic_period.json"
    )]
    [InlineData(
        "E:\\polar\\test\\scanner\\ab512965debd4573e54e9cd3d6a2332f-PolarTakeoutActivity-polar_takeout_activity.json"
    )]
    public async Task ScanFilesExcludedByFileExtension_ThatAreNotIndexed_ResultsInRejection(
        string filepath
    )
    {
        // Arrange
        var options = Options.Create(new ScannerConfig(".json"));
        var scanner = CreateScanner(options);

        // Act
        var result = await scanner.CheckAsync(filepath, "", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Classification.Should().BeNull();
        result.Files.Should().HaveCount(1);
        result.Conflicts.Should().BeEmpty();
        result.Actions.Should().HaveCount(1);
        result.Actions.Last().Value.Should().Be(ScannerActions.ExcludedByFileExtension);
    }

    [Theory]
    [InlineData("E:\\polar\\test\\scanner\\gps.kml.dcm")]
    public async Task ScanUnknownFiles_WithoutClassifier_ResultsInNoClassification(string filepath)
    {
        // Arrange
        var scanner = CreateScanner();

        // Act
        var result = await scanner.CheckAsync(filepath, "", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Classification.Should().BeNull();
        result.Files.Should().HaveCount(1);
        result.Conflicts.Should().BeEmpty();
        result.Actions.Should().HaveCount(1);
        result.Actions.Last().Value.Should().Be(ScannerActions.NotClassified);
    }

    private IScanner CreateScanner(IOptions<ScannerConfig>? options = null)
    {
        if (options == null)
            options = Options.Create(new ScannerConfig());

        var indexer = Substitute.For<IIndexer<ScannerIndex>>();
        indexer.GetById(Arg.Any<string>()).Returns(callInfo => null);

        var registry = new Registry();
        var logger = NullLogger<Classifier>.Instance as ILogger<Classifier>;
        var classifier = new Classifier(registry, logger) as IClassifier;

        return new Scanner(options, indexer, classifier);
    }
}
