using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleRespiratoryRateImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutRespiratoryRate,
        DateRanges.AllTime,
        "timestamp,breaths per minute"
    ),
        IFileClassifier;
