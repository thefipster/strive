using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleHeartrateVariabilityImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutHeartrateVariability,
        DateRanges.Month,
        "timestamp,root mean square of successive differences milliseconds,standard deviation milliseconds"
    ),
        IFileClassifier;
