using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.LivePace;

public class GoogleFitbitLivePaceClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitLivePace,
        DateRanges.Multi,
        "timestamp,steps,distance millimeters,altitude gain millimeters"
    );
