using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Spo2
{
    public class GoogleFitbitOxygenSaturationClassifier()
        : CsvClassifier(
            DataSources.GoogleFitbitOxygenSaturation,
            DateRanges.Multi,
            "timestamp,oxygen saturation percentage"
        );
}
