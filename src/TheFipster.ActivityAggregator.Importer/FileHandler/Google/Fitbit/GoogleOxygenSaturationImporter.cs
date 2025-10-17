using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleOxygenSaturationImporter()
    : GoogleCsvParser(
        DataSources.GoogleFitbitOxygenSaturation,
        DateRanges.Month,
        "timestamp,oxygen saturation percentage"
    ),
        IFileClassifier;
