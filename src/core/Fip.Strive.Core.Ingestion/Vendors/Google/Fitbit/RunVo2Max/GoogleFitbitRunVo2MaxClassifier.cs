using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.RunVo2Max;

public class GoogleFitbitRunVo2MaxClassifier()
    : CsvClassifier(DataSources.GoogleFitbitRunVo2Max, DateRanges.Multi, "timestamp,run VO2 max");
