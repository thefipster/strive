using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.LivePace;

public class GoogleFitbitLivePaceClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitLivePace,
        DateRanges.Multi,
        "timestamp,steps,distance millimeters,altitude gain millimeters"
    );
