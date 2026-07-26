using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class ResourcesTests
{
    [Theory]
    [InlineData(Resources.Accounts, "Accounts")]
    [InlineData(Resources.AccountsMaster, "AccountsMaster")]
    [InlineData(Resources.AccountFeatures, "AccountFeatures")]
    [InlineData(Resources.AccountFeaturesMaster, "AccountFeaturesMaster")]
    [InlineData(Resources.Administrative, "Administrative")]
    [InlineData(Resources.Credentials, "Credentials")]
    [InlineData(Resources.Devices, "Devices")]
    [InlineData(Resources.DevicesMaster, "DevicesMaster")]
    [InlineData(Resources.Geofences, "Geofences")]
    [InlineData(Resources.Geofencing, "Geofencing")]
    [InlineData(Resources.Groups, "Groups")]
    [InlineData(Resources.ManageDevices, "ManageDevices")]
    [InlineData(Resources.Operators, "Operators")]
    [InlineData(Resources.OperatorsMaster, "OperatorsMaster")]
    [InlineData(Resources.Permissions, "Permissions")]
    [InlineData(Resources.Positions, "Positions")]
    [InlineData(Resources.Profile, "Profile")]
    [InlineData(Resources.Reports, "Reports")]
    [InlineData(Resources.SettingsScreen, "SettingsScreen")]
    [InlineData(Resources.TollCatalog, "TollCatalog")]
    [InlineData(Resources.Transporters, "Transporters")]
    [InlineData(Resources.TransporterType, "TransporterType")]
    [InlineData(Resources.Trips, "Trips")]
    [InlineData(Resources.TripTracking, "TripTracking")]
    [InlineData(Resources.Users, "Users")]
    public void Resources_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);
}
