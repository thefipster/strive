using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleSwimLengthImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutSwimLength,
        DateRanges.Month,
        "timestamp,lap time,stroke count,stroke type"
    ),
        IFileClassifier;
