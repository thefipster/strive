using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleDailyHeartrateVariabilityImporter()
    : GoogleCsvParser(
        DataSources.GoogleFitbitDailyHeartrateVariability,
        DateRanges.Month,
        "timestamp,average heart rate variability milliseconds,non rem heart rate beats per minute,entropy,deep sleep root mean square of successive differences milliseconds"
    ),
        IFileClassifier;
