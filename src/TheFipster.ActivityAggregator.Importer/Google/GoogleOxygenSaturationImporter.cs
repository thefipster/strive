using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleOxygenSaturationImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutOxygenSaturation,
        DateRanges.Month,
        "timestamp,oxygen saturation percentage"
    ),
        IFileClassifier;
