using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleLivePaceImporter()
    : GoogleCsvParser(
        DataSources.GoogleFitbitLivePace,
        DateRanges.Month,
        "timestamp,steps,distance millimeters,altitude gain millimeters"
    ),
        IFileClassifier;
