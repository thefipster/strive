using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.ActivityLevel;

public class GoogleFitbitActivityLevelClassifier()
    : CsvClassifier(DataSources.GoogleFitbitActivityLevel, DateRanges.Multi, "timestamp,level");
