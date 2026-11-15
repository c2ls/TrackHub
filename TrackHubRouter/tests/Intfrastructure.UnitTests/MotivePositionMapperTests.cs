// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using Common.Domain.Extensions;
using TrackHub.Router.Infrastructure.Motive.Mappers;
using TrackHub.Router.Infrastructure.Motive.Models;

namespace TrackHub.Router.Infrastructure.Motive.Tests;

[TestFixture]
public class PositionMapperTests
{
    private static readonly DeviceTransporterVm Device = new()
    {
        TransporterId = Guid.NewGuid(),
        Name = "Truck-1",
        TransporterType = "Truck",
    };

    [Test]
    public void MapToPositionVm_ConvertsImperialUnitsAndMapsCoreFields()
    {
        var time = new DateTimeOffset(2026, 7, 17, 10, 0, 0, TimeSpan.Zero);
        var location = new VehicleLocation(time, 47.565647, -122.276261, 90, 100, "Seattle, WA", 10, 123.56);

        var result = location.MapToPositionVm(Device);

        Assert.Multiple(() =>
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.DeviceName, Is.EqualTo("Truck-1"));
            Assert.That(result.Latitude, Is.EqualTo(47.565647));
            Assert.That(result.Longitude, Is.EqualTo(-122.276261));
            Assert.That(result.Altitude, Is.Null);
            Assert.That(result.DeviceDateTime, Is.EqualTo(time));
            Assert.That(result.ServerDateTime, Is.Null);
            Assert.That(result.Course, Is.EqualTo(90));
            Assert.That(result.Speed, Is.EqualTo(160.9344).Within(0.0001));
            Assert.That(result.Address, Is.EqualTo("Seattle, WA"));
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(16093.44).Within(0.01));
            Assert.That(result.Attributes!.Value.Hourmeter, Is.EqualTo(123.56));
            Assert.That(result.Attributes!.Value.Ignition, Is.Null);
        });
    }

    [Test]
    public void MapToPositionVm_NullSpeedAndOdometer_MapsToZeroSpeedAndNullMileage()
    {
        var location = new VehicleLocation(DateTimeOffset.UnixEpoch, 1, 2, null, null, null, null, null);

        var result = location.MapToPositionVm(Device);

        Assert.Multiple(() =>
        {
            Assert.That(result.Speed, Is.Zero);
            Assert.That(result.Course, Is.Null);
            Assert.That(result.Attributes!.Value.Mileage, Is.Null);
        });
    }

    [Test]
    public void MapToPositionVm_FromVehicles_SkipsUnrequestedAndLocationlessVehicles()
    {
        var location = new VehicleLocation(DateTimeOffset.UnixEpoch, 1, 2, 0, 0, null, null, null);
        var vehicles = new[]
        {
            new Vehicle(23, "V-23", location),
            new Vehicle(24, "V-24", null),
            new Vehicle(25, "V-25", location),
        };
        var lookup = new Dictionary<string, DeviceTransporterVm> { ["23"] = Device, ["24"] = Device };

        var result = vehicles.MapToPositionVm(lookup).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Deserialize_DocumentedVehicleLocationsPayload_ReadsSnakeCaseFieldsAsUtc()
    {
        const string json = """
            {"vehicles":[{"vehicle":{"id":23,"number":"V-1000","current_location":{"id":"af5b","lat":47.565647,"lon":-122.276261,
            "description":"Seattle, WA","located_at":"2016-03-17T12:12:07Z","bearing":90,"speed":65.1,"engine_hours":123.56,"odometer":456.22}}}],
            "pagination":{"per_page":25,"page_no":1,"total":1}}
            """;

        var response = json.Deserialize<VehiclesResponse>()!;
        var vehicle = response.Vehicles!.Single().Vehicle;

        Assert.Multiple(() =>
        {
            Assert.That(vehicle.Id, Is.EqualTo(23));
            Assert.That(vehicle.Number, Is.EqualTo("V-1000"));
            Assert.That(vehicle.CurrentLocation!.Value.LocatedAt, Is.EqualTo(new DateTimeOffset(2016, 3, 17, 12, 12, 7, TimeSpan.Zero)));
            Assert.That(vehicle.CurrentLocation!.Value.LocatedAt.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(vehicle.CurrentLocation!.Value.EngineHours, Is.EqualTo(123.56));
            Assert.That(response.Pagination!.Value.PerPage, Is.EqualTo(25));
            Assert.That(response.Pagination!.Value.Total, Is.EqualTo(1));
        });
    }

    [Test]
    public void Deserialize_DocumentedHistoryPayload_ReadsNestedLocations()
    {
        const string json = """
            {"vehicle_locations":[{"vehicle_location":{"id":"d167","located_at":"2016-05-24T09:01:11Z","lat":12.5,"lon":45.6,
            "bearing":2.24,"engine_hours":244.22,"type":"breadcrumb","speed":40.4,"odometer":5.58623553}}]}
            """;

        var location = json.Deserialize<VehicleLocationsResponse>()!.VehicleLocations!.Single().VehicleLocation;

        Assert.Multiple(() =>
        {
            Assert.That(location.LocatedAt, Is.EqualTo(new DateTimeOffset(2016, 5, 24, 9, 1, 11, TimeSpan.Zero)));
            Assert.That(location.Speed, Is.EqualTo(40.4));
            Assert.That(location.Bearing, Is.EqualTo(2.24));
        });
    }

    [Test]
    public void MapToDeviceVm_UsesVehicleIdAsSerialAndIdentifier()
    {
        var transporterId = Guid.NewGuid();

        var result = new Vehicle(6620, "Unit 6620", null).MapToDeviceVm(transporterId);

        Assert.Multiple(() =>
        {
            Assert.That(result.DeviceId, Is.EqualTo(transporterId));
            Assert.That(result.Identifier, Is.EqualTo(6620));
            Assert.That(result.Serial, Is.EqualTo("6620"));
            Assert.That(result.Name, Is.EqualTo("Unit 6620"));
        });
    }

    [Test]
    public void MapToDeviceVm_IdBeyondInt_KeepsSerialAndZeroIdentifier()
    {
        var result = new Vehicle(5_000_000_000, null, null).MapToDeviceVm();

        Assert.Multiple(() =>
        {
            Assert.That(result.Identifier, Is.Zero);
            Assert.That(result.Serial, Is.EqualTo("5000000000"));
            Assert.That(result.Name, Is.EqualTo("5000000000"));
            Assert.That(result.DeviceId, Is.Null);
        });
    }
}
