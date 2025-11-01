using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Distance;

public class GoogleFitbitDistanceClassifier()
    : CsvClassifier(DataSources.GoogleFitbitDistance, DateRanges.Multi, "timestamp,distance");
