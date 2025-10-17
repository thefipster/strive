using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleAltitudeImporter()
    : GoogleCsvParser(DataSources.GoogleFitbitAltitude, DateRanges.Month, "timestamp,gain"),
        IFileClassifier;
