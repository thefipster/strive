using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleDistanceImporter()
    : GoogleCsvParser(DataSources.FitbitTakeoutDistance, DateRanges.Month, "timestamp,distance"),
        IFileClassifier;
