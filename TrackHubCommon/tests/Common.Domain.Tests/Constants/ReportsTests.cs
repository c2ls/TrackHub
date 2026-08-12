using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class ReportsTests
{
    [Theory]
    [InlineData(Reports.LiveReport, "LiveReport")]
    [InlineData(Reports.PositionRecord, "PositionRecord")]
    [InlineData(Reports.TransportersInGeofence, "TransportersInGeofence")]
    public void Reports_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);
}
