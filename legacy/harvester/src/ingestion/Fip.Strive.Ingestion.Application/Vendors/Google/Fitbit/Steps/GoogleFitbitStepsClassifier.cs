using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Steps;

public class GoogleFitbitStepsClassifier()
    : CsvClassifier(DataSources.GoogleFitbitSteps, DateRanges.Multi, "timestamp,steps");
