using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.RunVo2Max;

public class GoogleFitbitRunVo2MaxClassifier()
    : CsvClassifier(DataSources.GoogleFitbitRunVo2Max, DateRanges.Multi, "timestamp,run VO2 max");
