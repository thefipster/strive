using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleDistanceImporter()
    : GoogleCsvParser(DataSources.GoogleFitbitDistance, DateRanges.Month, "timestamp,distance"),
        IFileClassifier;
