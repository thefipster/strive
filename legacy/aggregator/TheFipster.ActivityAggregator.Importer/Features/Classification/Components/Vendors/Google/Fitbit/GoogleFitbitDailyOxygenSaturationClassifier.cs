using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitDailyOxygenSaturationClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitDailyOxygenSaturation,
        DateRanges.Month,
        "timestamp,average percentage,lower bound percentage,upper bound percentage,baseline percentage,standard deviation percentage"
    );
