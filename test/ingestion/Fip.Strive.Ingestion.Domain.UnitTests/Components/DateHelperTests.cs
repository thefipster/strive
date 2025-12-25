using AwesomeAssertions;
using Fip.Strive.Ingestion.Domain.Components;

namespace Fip.Strive.Ingestion.Domain.UnitTests.Components;

public class DateHelperTests
{
    [Fact]
    public void ConvertGermanDateToUtc_BeforeOctoberDst_ShiftsByTwoHours()
    {
        var date = DateHelper.GetUtcDateFromGermanLocalTime(2025, 10, 25, 12);

        date.Kind.Should().Be(DateTimeKind.Utc);
        date.Hour.Should().Be(10);
    }

    [Fact]
    public void ConvertGermanDateToUtc_AfterOctoberDst_ShiftsByOneHours()
    {
        var date = DateHelper.GetUtcDateFromGermanLocalTime(2025, 10, 26, 12);

        date.Kind.Should().Be(DateTimeKind.Utc);
        date.Hour.Should().Be(11);
    }

    [Fact]
    public void ConvertGermanDateToUtc_BeforeMarchDst_ShiftsByOneHours()
    {
        var date = DateHelper.GetUtcDateFromGermanLocalTime(2025, 03, 29, 12);

        date.Kind.Should().Be(DateTimeKind.Utc);
        date.Hour.Should().Be(11);
    }

    [Fact]
    public void ConvertGermanDateToUtc_AfterMarchDst_ShiftsByTwoHours()
    {
        var date = DateHelper.GetUtcDateFromGermanLocalTime(2025, 03, 30, 12);

        date.Kind.Should().Be(DateTimeKind.Utc);
        date.Hour.Should().Be(10);
    }
}
