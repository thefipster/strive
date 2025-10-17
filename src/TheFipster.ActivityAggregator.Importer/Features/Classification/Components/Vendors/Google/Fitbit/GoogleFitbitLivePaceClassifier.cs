using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitLivePaceClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitLivePace,
        DateRanges.Month,
        "timestamp,steps,distance millimeters,altitude gain millimeters"
    );
