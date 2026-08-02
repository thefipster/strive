using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Spo2
{
    public class GoogleFitbitOxygenSaturationClassifier()
        : CsvClassifier(
            DataSources.GoogleFitbitOxygenSaturation,
            DateRanges.Multi,
            "timestamp,oxygen saturation percentage"
        );
}
