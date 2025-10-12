using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Polar;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public static class PolarFlowClassificationDefinitions
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
                DataSources.PolarFlow247Ohr
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
                DataSources.PolarFlowGenericPeriodEcg
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
                DataSources.PolarFlowGenericPeriodSkinContact
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
                DataSources.PolarFlowGenericPeriodSpo2
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
                DataSources.PolarFlowGenericPeriodTemperature
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
                DataSources.PolarFlowActivity
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
                DataSources.PolarFlowTraining
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
                DataSources.PolarFlowPpi
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
                DataSources.PolarFlowSleepResult
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
                DataSources.PolarFlowSleepScore
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarFlowOrthostaticTestImporter),
                Vendors.PolarFlow,
                "orthostatic-test-result-4476608-21635975-dec88666-b962-422c-a697-4e75bf3d31bb.json",
                "2025-09-20 07:09:01",
                DateRanges.Day,
                DataSources.PolarFlowOrthostaticTest
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarFlowSleepWakeImporter),
                Vendors.PolarFlow,
                "sleep_wake_4476608-1545688c-38cb-4028-ad2f-a58a3c9db94e.json",
                "0001-01-01",
                DateRanges.AllTime,
                DataSources.PolarFlowSleepWake
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarFlowNightlyRecoveryImporter),
                Vendors.PolarFlow,
                "nightly_recovery_4476608-ffde1ddc-5ce2-456c-ae49-5e878d7902d1.json",
                "0001-01-01",
                DateRanges.AllTime,
                DataSources.PolarFlowNightlyRecovery
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(PolarFlowNightlyRecoveryBlobImporter),
                Vendors.PolarFlow,
                "nightly_recovery_blob_4476608-152fe6a7-0805-4fba-b51a-188a5ee6a35a.json",
                "0001-01-01",
                DateRanges.AllTime,
                DataSources.PolarFlowNightlyRecoveryBlob
            ),
        ];
    }
}
