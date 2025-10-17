using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitDemographicVo2MaxClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitDemographicVo2Max,
        DateRanges.AllTime,
        "timestamp,demographic vo2max"
    );
