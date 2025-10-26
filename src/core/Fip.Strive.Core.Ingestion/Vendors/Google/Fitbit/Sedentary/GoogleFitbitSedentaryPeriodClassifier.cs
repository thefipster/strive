using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Sedentary;

public class GoogleFitbitSedentaryPeriodClassifier()
    : CsvClassifier(DataSources.GoogleFitbitSedentaryPeriod, DateRanges.Day, "start time,end time");
