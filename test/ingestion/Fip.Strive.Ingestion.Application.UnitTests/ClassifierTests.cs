using AwesomeAssertions;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Application.UnitTests.Data.TheFipsterApp;
using Fip.Strive.Ingestion.Application.UnitTests.Extensions;
using Fip.Strive.Ingestion.Application.Vendors.TheFipsterApp.Bodyweight;
using Fip.Strive.Ingestion.Application.Vendors.TheFipsterApp.Plog;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.UnitTests;

public class ClassifierTests
{
    [Theory]
    [InlineData(
        typeof(TheFipsterAppPlogClassifier),
        TheFipsterAppPlogTestData.PlogCsvWithOneLine,
        TheFipsterAppPlogTestData.Extension,
        DataSources.TheFipsterAppPlog,
        DateRanges.Multi
    )]
    [InlineData(
        typeof(TheFipsterAppWeightClassifier),
        TheFipsterAppWeightTestData.WeightJsonWithOneEntry,
        TheFipsterAppWeightTestData.Extension,
        DataSources.TheFipsterAppWeight,
        DateRanges.Multi
    )]
    public void TestDirectInstance_WithGoodData_ClassifiesCorrectly(
        Type classifierType,
        string content,
        string extension,
        DataSources expectedSource,
        DateRanges expectedRange,
        DateTime? expectedTimestamp = null
    )
    {
        // Arrange
        var classifier = EnsureClassifier(classifierType);
        using var file = TempFile.Create(content, extension);
        var probe = new FileProbe(file.Filepath);

        // Act
        var result = classifier.Classify(probe);

        // Assert
        result.Should().NotBeNull();
        result.Filepath.Should().Be(file.Filepath);
        result.Source.Should().Be(expectedSource);
        result.Datetype.Should().Be(expectedRange);
        result.Datetime.Should().Be(expectedTimestamp);
    }

    private static IFileClassifier EnsureClassifier(Type classifierType)
    {
        var classifier = Activator.CreateInstance(classifierType) as IFileClassifier;

        if (classifier == null)
            throw new Exception($"Could not create classifier of type {classifierType}");

        return classifier;
    }
}
