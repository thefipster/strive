using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleGpsLocationImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutGpsLocation,
        DateRanges.Day,
        "timestamp,latitude,longitude,altitude"
    ),
        IFileClassifier;
