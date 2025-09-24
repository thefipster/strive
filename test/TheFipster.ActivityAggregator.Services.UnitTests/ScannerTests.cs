using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer;
using TheFipster.ActivityAggregator.Services.Abstractions;
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
    public async Task Test1(string filepath)
    {
        // Arrange
        var options = Options.Create(new ScannerConfig());
        var registry = new Registry();
        var indexer = Substitute.For<IIndexer<ScannerIndex>>();
        indexer.GetById(Arg.Any<string>()).Returns(callInfo => null);
        var logger = NullLogger<Scanner>.Instance as ILogger<Scanner>;

        var scanner = new Scanner(options, registry, indexer, logger) as IScanner;

        // Act
        var result = await scanner.CheckAsync(filepath, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
