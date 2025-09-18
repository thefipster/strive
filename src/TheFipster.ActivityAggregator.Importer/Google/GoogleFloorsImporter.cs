using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleFloorsImporter()
    : GoogleCsvParser(DataSources.FitbitTakeoutFloors, DateRanges.Month, "timestamp,floors"),
        IFileClassifier;
