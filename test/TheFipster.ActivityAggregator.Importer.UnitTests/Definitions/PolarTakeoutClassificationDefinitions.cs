using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Polar;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public static class PolarTakeoutClassificationDefinitions
{
    public static IEnumerable<object[]> GetClassificationCases()
    {
        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeout247OhrImporter),
                Vendors.PolarFlow,
                "247ohr_2017_10-788de140-3a96-4f44-a833-d19bfbed5fcc.json",
                "2017-10-07",
                DateRanges.Month,
                DataSources.PolarTakeout247Ohr
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeoutGenericPeriodEcgImporter),
                Vendors.PolarFlow,
                "generic-period-ecg.json",
                "2024-08-08 20:05:05",
                DateRanges.Day,
                DataSources.PolarTakeoutGenericPeriodEcg
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeoutGenericPeriodSkinContactImporter),
                Vendors.PolarFlow,
                "generic-period-skin_contact.json",
                "2023-01-01",
                DateRanges.Day,
                DataSources.PolarTakeoutGenericPeriodSkinContact
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeoutGenericPeriodSpo2Importer),
                Vendors.PolarFlow,
                "generic-period-spo2.json",
                "2024-08-09 14:55:09",
                DateRanges.Day,
                DataSources.PolarTakeoutGenericPeriodSpo2
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeoutGenericPeriodTemperatureImporter),
                Vendors.PolarFlow,
                "generic-period-temperature.json",
                "2023-01-01",
                DateRanges.Day,
                DataSources.PolarTakeoutGenericPeriodTemperature
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeoutActivityImporter),
                Vendors.PolarFlow,
                "activity-2014-11-06-e0b29915-ae10-40c9-a34b-9e4f862cbea3.json",
                "2014-11-06",
                DateRanges.Day,
                DataSources.PolarTakeoutActivity
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeoutTrainingImporter),
                Vendors.PolarFlow,
                "training-session-2025-08-09-8166543922-f3906546-0ec8-4289-bee9-d00212b00e3b.json",
                "2025-08-09 13:09:02.595",
                DateRanges.Time,
                DataSources.PolarTakeoutTraining
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeoutPpiImporter),
                Vendors.PolarFlow,
                "ppi_samples_2024_08_3-826e9ca5-6985-4576-80d2-b782e352c2f1.json",
                "2024-08-18",
                DateRanges.Month,
                DataSources.PolarTakeoutPpi
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeoutSleepResultImporter),
                Vendors.PolarFlow,
                "sleep_result_4476608-84a1e546-63b8-461a-ab85-770cfc3651e9.json",
                "2017-10-08",
                DateRanges.AllTime,
                DataSources.PolarTakeoutSleepResult
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarTakeoutSleepScoreImporter),
                Vendors.PolarFlow,
                "sleep_score_4476608-0e38b6c4-ade9-405f-a1a5-8bb11bb68219.json",
                "2019-08-29",
                DateRanges.AllTime,
                DataSources.PolarTakeoutSleepScore
            ),
        ];
    }
}
