namespace Fip.Strive.Ingestion.Application.UnitTests.Vendors.TheFipsterApp.Bodyweight;

public class TheFipsterAppWeightTestData
{
    public const string Extension = ".json";

    public const string WeightJsonWithOneEntry =
        @"[
          {
            ""Weight"": 77.8,
            ""Reason"": ""some reason"",
            ""Date"": ""2025-10-23T12:34:56+02:00""
          }
        ]";
}
