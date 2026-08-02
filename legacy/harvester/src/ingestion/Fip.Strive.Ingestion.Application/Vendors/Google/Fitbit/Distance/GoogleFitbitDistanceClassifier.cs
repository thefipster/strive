using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Distance;

public class GoogleFitbitDistanceClassifier()
    : CsvClassifier(DataSources.GoogleFitbitDistance, DateRanges.Multi, "timestamp,distance");
