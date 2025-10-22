using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public static class GoogleFitbitClassificationDefinitions
{
    public static IEnumerable<object[]> GetClassificationCases()
    {
        yield return
        [
            new FileClassificationCase(
                typeof(GoogleFitbitStepsClassifier),
                Vendors.GoogleFitbit,
                "steps_2023-09-01.csv",
                "2023-09-01 05:28:00",
                DateRanges.Month,
                DataSources.GoogleFitbitSteps
            ),
        ];
    }
}
