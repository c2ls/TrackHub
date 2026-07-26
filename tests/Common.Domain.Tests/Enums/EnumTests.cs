using Common.Domain.Enums;
using FluentAssertions;

namespace Common.Domain.Tests.Enums;

// Each member's numeric value is pinned individually: the ints are a persistence and wire
// contract, so a renumbering must fail here. Membership is deliberately not asserted by
// set equality or count.
public class EnumTests
{
    [Theory]
    [InlineData(AccountType.Personal, 1)]
    [InlineData(AccountType.Business, 2)]
    [InlineData(AccountType.Associate, 3)]
    public void AccountType_HasExpectedValues(AccountType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Theory]
    [InlineData(CategoryType.Product, 1)]
    [InlineData(CategoryType.Service, 2)]
    public void CategoryType_HasExpectedValues(CategoryType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Theory]
    [InlineData(DeviceType.Aviation, 1)]
    [InlineData(DeviceType.Camera, 2)]
    [InlineData(DeviceType.Cycling, 3)]
    [InlineData(DeviceType.Cellular, 4)]
    [InlineData(DeviceType.Drones, 5)]
    [InlineData(DeviceType.EmergencyLocator, 6)]
    [InlineData(DeviceType.Fitness, 7)]
    [InlineData(DeviceType.Handheld, 8)]
    [InlineData(DeviceType.Marine, 9)]
    [InlineData(DeviceType.OBDScanner, 10)]
    [InlineData(DeviceType.PetTracking, 11)]
    [InlineData(DeviceType.Phone, 12)]
    [InlineData(DeviceType.Satellite, 13)]
    [InlineData(DeviceType.Smartwatch, 14)]
    [InlineData(DeviceType.Wearable, 15)]
    public void DeviceType_HasExpectedValues(DeviceType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Theory]
    [InlineData(ProtocolType.CommandTrack, 1)]
    [InlineData(ProtocolType.Traccar, 2)]
    [InlineData(ProtocolType.Flespi, 3)]
    [InlineData(ProtocolType.GeoTab, 4)]
    [InlineData(ProtocolType.GpsGate, 5)]
    [InlineData(ProtocolType.Navixy, 6)]
    [InlineData(ProtocolType.Samsara, 7)]
    [InlineData(ProtocolType.Wialon, 8)]
    [InlineData(ProtocolType.Protrack, 9)]
    [InlineData(ProtocolType.Mettax, 10)]
    public void ProtocolType_HasExpectedValues(ProtocolType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Theory]
    [InlineData(ReportType.Basic, 1)]
    [InlineData(ReportType.Custom, 2)]
    [InlineData(ReportType.External, 3)]
    public void ReportType_HasExpectedValues(ReportType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Theory]
    [InlineData(TransporterType.Aircraft, 1)]
    [InlineData(TransporterType.Asset, 2)]
    [InlineData(TransporterType.Bicycle, 3)]
    [InlineData(TransporterType.Boat, 4)]
    [InlineData(TransporterType.Car, 5)]
    [InlineData(TransporterType.CargoContainer, 6)]
    [InlineData(TransporterType.ConstructionVehicle, 7)]
    [InlineData(TransporterType.Child, 8)]
    [InlineData(TransporterType.DeliveryVan, 9)]
    [InlineData(TransporterType.Drone, 10)]
    [InlineData(TransporterType.ElderlyPerson, 11)]
    [InlineData(TransporterType.FleetVehicle, 12)]
    [InlineData(TransporterType.HeavyEquipment, 13)]
    [InlineData(TransporterType.Livestock, 14)]
    [InlineData(TransporterType.Motorcycle, 15)]
    [InlineData(TransporterType.Package, 16)]
    [InlineData(TransporterType.Person, 17)]
    [InlineData(TransporterType.Pet, 18)]
    [InlineData(TransporterType.SchoolBus, 19)]
    [InlineData(TransporterType.Scooter, 20)]
    [InlineData(TransporterType.Taxi, 21)]
    [InlineData(TransporterType.Tool, 22)]
    [InlineData(TransporterType.Truck, 23)]
    [InlineData(TransporterType.Tractor, 24)]
    public void TransporterType_HasExpectedValues(TransporterType type, int expected) =>
        ((int)type).Should().Be(expected);
}
