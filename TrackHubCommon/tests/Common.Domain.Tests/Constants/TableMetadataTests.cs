using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class TableMetadataTests
{
    [Theory]
    [InlineData(TableMetadata.Account, "accounts")]
    [InlineData(TableMetadata.AccountSettings, "account_settings")]
    [InlineData(TableMetadata.Action, "actions")]
    [InlineData(TableMetadata.Category, "categories")]
    [InlineData(TableMetadata.Client, "clients")]
    [InlineData(TableMetadata.Credential, "credentials")]
    [InlineData(TableMetadata.Device, "devices")]
    [InlineData(TableMetadata.TransporterGroup, "transporter_group")]
    [InlineData(TableMetadata.Geofence, "geofences")]
    [InlineData(TableMetadata.GeofenceEvent, "geofenceevents")]
    [InlineData(TableMetadata.Group, "groups")]
    [InlineData(TableMetadata.Operator, "operators")]
    [InlineData(TableMetadata.Policy, "policies")]
    [InlineData(TableMetadata.Position, "position")]
    [InlineData(TableMetadata.Report, "reports")]
    [InlineData(TableMetadata.Resource, "resources")]
    [InlineData(TableMetadata.ResourceAction, "resource_action")]
    [InlineData(TableMetadata.ResourceActionPolicy, "resource_action_policy")]
    [InlineData(TableMetadata.ResourceActionRole, "resource_action_role")]
    [InlineData(TableMetadata.Role, "roles")]
    [InlineData(TableMetadata.Transporter, "transporters")]
    [InlineData(TableMetadata.TransporterPosition, "transporter_position")]
    [InlineData(TableMetadata.TransporterType, "transporter_type")]
    [InlineData(TableMetadata.TollStation, "toll_stations")]
    [InlineData(TableMetadata.TollTariff, "toll_tariffs")]
    [InlineData(TableMetadata.TollVehicleClass, "toll_vehicle_classes")]
    [InlineData(TableMetadata.TransporterTollClass, "transporter_toll_classes")]
    [InlineData(TableMetadata.Trip, "trips")]
    [InlineData(TableMetadata.TripAssignment, "trip_assignments")]
    [InlineData(TableMetadata.TripDelivery, "trip_deliveries")]
    [InlineData(TableMetadata.TripDocument, "trip_documents")]
    [InlineData(TableMetadata.TripEvent, "trip_events")]
    [InlineData(TableMetadata.TripPod, "trip_pods")]
    [InlineData(TableMetadata.TripRoutePlan, "route_plans")]
    [InlineData(TableMetadata.TripShare, "trip_shares")]
    [InlineData(TableMetadata.TripStop, "trip_stops")]
    [InlineData(TableMetadata.User, "users")]
    [InlineData(TableMetadata.UserGroup, "user_group")]
    [InlineData(TableMetadata.UserSettings, "user_settings")]
    [InlineData(TableMetadata.UserRole, "user_role")]
    [InlineData(TableMetadata.UserPolicy, "user_policy")]
    public void TableMetadata_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);
}
