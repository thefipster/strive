using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit
{
    public class GoogleFitbitOxygenSaturationClassifier()
        : CsvClassifier(
            DataSources.GoogleFitbitOxygenSaturation,
            DateRanges.Month,
            "timestamp,oxygen saturation percentage"
        );
}
