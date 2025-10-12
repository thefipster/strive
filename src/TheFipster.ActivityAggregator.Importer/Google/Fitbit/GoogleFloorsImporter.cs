using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleFloorsImporter()
    : GoogleCsvParser(DataSources.GoogleFitbitFloors, DateRanges.Month, "timestamp,floors"),
        IFileClassifier;
