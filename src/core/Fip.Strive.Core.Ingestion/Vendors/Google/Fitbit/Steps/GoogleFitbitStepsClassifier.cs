using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Steps;

public class GoogleFitbitStepsClassifier()
    : CsvClassifier(DataSources.GoogleFitbitSteps, DateRanges.Multi, "timestamp,steps");
