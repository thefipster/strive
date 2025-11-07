using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Sedentary;

public class GoogleFitbitSedentaryPeriodClassifier()
    : CsvClassifier(DataSources.GoogleFitbitSedentaryPeriod, DateRanges.Day, "start time,end time");
