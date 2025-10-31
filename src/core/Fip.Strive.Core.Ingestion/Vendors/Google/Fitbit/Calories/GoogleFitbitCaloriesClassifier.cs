using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Calories;

public class GoogleFitbitCaloriesClassifier()
    : CsvClassifier(DataSources.GoogleFitbitCalories, DateRanges.Multi, "timestamp,calories");
