namespace Fip.Strive.Ingestion.Application.UnitTests.Vendors.Polar.Flow.SleepResult;

public class PolarFlowSleepResultTestData
{
    public const string Extension = ".json";

    public const string FileVariant1 =
        @"[
            {
            ""night"": ""2017-10-08"",
            ""evaluation"": {
              ""sleepType"": ""SLEEP_PLUS"",
              ""sleepSpan"": ""PT9H25M30S"",
              ""asleepDuration"": ""PT8H53M30S"",
              ""age"": 32.24657534246575,
              ""analysis"": {
                ""efficiencyPercent"": 94.34129089301503,
                ""continuityIndex"": 3.0,
                ""continuityClass"": 3,
                ""feedback"": 10111
              },
              ""interruptions"": {
                ""totalDuration"": ""PT32M"",
                ""totalCount"": 34,
                ""shortDuration"": ""PT23M"",
                ""shortCount"": 30,
                ""longDuration"": ""PT9M"",
                ""longCount"": 4
              }
            },
            ""sleepResult"": {
              ""hypnogram"": {
                ""sleepStart"": ""2017-10-08T00:55:57+02:00"",
                ""sleepEnd"": ""2017-10-08T10:21:27+02:00"",
                ""alarmSnoozeTimes"": [],
                ""sleepStateChanges"": [
                  {
                    ""offsetFromStart"": ""PT25M30S"",
                    ""state"": ""WAKE""
                  }
                ],
                ""sleepStartOffset"": 0,
                ""sleepEndOffset"": 0,
                ""rating"": ""SLEPT_QUITE_WELL"",
                ""birthday"": ""1985-07-10"",
                ""sleepGoal"": ""PT7H"",
                ""deviceId"": ""0A2E7029"",
                ""batteryRanOut"": false
              }
            }
          }
        ]";

    public const string FileVariant2 =
        @"[
            {
            ""night"": ""2025-12-10"",
            ""evaluation"": {
              ""sleepType"": ""SLEEP_PLUS_STAGES"",
              ""sleepSpan"": ""PT8H27M"",
              ""asleepDuration"": ""PT8H2M"",
              ""age"": 40.41917808219178,
              ""analysis"": {
                ""efficiencyPercent"": 95.06903353057199,
                ""continuityIndex"": 3.8,
                ""continuityClass"": 3,
                ""feedback"": 11121
              },
              ""interruptions"": {
                ""totalDuration"": ""PT25M"",
                ""totalCount"": 30,
                ""shortDuration"": ""PT18M"",
                ""shortCount"": 27,
                ""longDuration"": ""PT7M"",
                ""longCount"": 3
              },
              ""phaseDurations"": {
                ""wake"": ""PT25M"",
                ""rem"": ""PT2H9M30S"",
                ""light"": ""PT3H56M30S"",
                ""deep"": ""PT1H56M"",
                ""unknown"": ""PT0S"",
                ""remPercentage"": 25.542406311637084,
                ""deepPercentage"": 22.879684418145956
              }
            },
            ""sleepResult"": {
              ""hypnogram"": {
                ""sleepStart"": ""2025-12-10T01:27:00.715+01:00"",
                ""sleepEnd"": ""2025-12-10T09:54:00.715+01:00"",
                ""alarmSnoozeTimes"": [],
                ""sleepStateChanges"": [
                  {
                    ""offsetFromStart"": ""PT1H6M30S"",
                    ""state"": ""NONREM2""
                  }
                ],
                ""sleepStartOffset"": 0,
                ""sleepEndOffset"": 0,
                ""rating"": ""UNKNOWN"",
                ""birthday"": ""1985-07-10"",
                ""sleepGoal"": ""PT7H"",
                ""deviceId"": ""DC425127"",
                ""batteryRanOut"": false
              },
              ""sleepCycles"": {
                ""cycles"": {
                  ""sleepCycleModels"": [
                    {
                      ""secondsFromSleepStart"": 77775,
                      ""sleepDepthStart"": 0.9715438
                    }
                  ]
                }
              }
            }
          }
        ]";
}
