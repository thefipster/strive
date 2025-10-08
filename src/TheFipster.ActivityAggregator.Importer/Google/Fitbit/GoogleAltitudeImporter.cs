using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleAltitudeImporter()
    : GoogleCsvParser(DataSources.FitbitTakeoutAltitude, DateRanges.Month, "timestamp,gain"),
        IFileClassifier;
