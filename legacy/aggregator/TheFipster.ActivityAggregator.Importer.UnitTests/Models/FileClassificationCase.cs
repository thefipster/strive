using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Models;

public record FileClassificationCase(
    Type ImporterType,
    string Vendor,
    string Filename,
    string Date,
    DateRanges ExpectedRange,
    DataSources ExpectedSource
);
