using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleHeartrateVariabilityImporter()
    : GoogleCsvParser(
        DataSources.GoogleFitbitHeartrateVariability,
        DateRanges.Month,
        "timestamp,root mean square of successive differences milliseconds,standard deviation milliseconds"
    ),
        IFileClassifier;
