using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleRespiratoryRateImporter()
    : GoogleCsvParser(
        DataSources.GoogleFitbitRespiratoryRate,
        DateRanges.AllTime,
        "timestamp,breaths per minute"
    ),
        IFileClassifier;
