using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitSwimLengthClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitSwimLength,
        DateRanges.Month,
        "timestamp,lap time,stroke count,stroke type"
    );
