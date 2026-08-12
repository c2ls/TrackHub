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

using TrackHub.Router.Infrastructure.CommandTrack.Mappers;
using TrackHub.Router.Infrastructure.CommandTrack.Models;

namespace TrackHub.Router.Infrastructure.CommandTrack.Tests;

// Value-correctness tests for the CommandTrack mapper (TT-01): a realistic populated payload is
// asserted field-by-field so a reordered or dropped constructor argument fails the build's suite.
[TestFixture]
public class PositionMapperTests
{
    private static DeviceTransporterVm Device { get; } = new()
    {
        TransporterId = Guid.Parse("6f1d2a7c-2b0e-4a3f-9c11-0d8a7e5b4c31"),
        Identifier = 42,
        Serial = "SER-42",
        Name = "Truck-1",
        TransporterType = "Truck",
        TransporterTypeId = (short)TransporterType.Truck
    };

    private static readonly DateTimeOffset DeviceTime = new(2026, 7, 17, 10, 15, 30, TimeSpan.Zero);
    private static readonly DateTimeOffset ServerTime = new(2026, 7, 17, 10, 15, 35, TimeSpan.Zero);

    private static Position CreatePosition()
        => new(
            PositionId: 9001,
            Serial: "SER-42",
            Plate: "ABC123",
            Latitude: 4.710989,
            Longitude: -74.072092,
            Altitude: 2640.5,
            DeviceDateTime: DeviceTime,
            ServerDateTime: ServerTime,
            Speed: 62.5,
            Course: 187.25,
            EventId: 7,
            Address: "Calle 100",
            DistanceToAddress: 0.35,
            City: "Bogota",
            State: "Cundinamarca",
            Country: "Colombia",
            Ignition: true,
            Satellites: 11,
            Mileage: 154320.75,
            Temperature: -4.5);

    private static DevicePosition CreateDevicePosition()
        => new(
            Id: 9002,
            Serial: "SER-42",
            Plate: "ABC123",
            Latitude: 4.710989,
            Longitude: -74.072092,
            Altitude: 2640.5,
            DeviceDateTime: DeviceTime,
            Speed: 62.5,
            Course: 187.25,
            Address: "Calle 100",
            DistanceToAddress: 0,
            City: "Bogota",
            State: "Cundinamarca",
            Country: "Colombia",
            Ignition: true,
            Satellites: 11,
            Mileage: 154320.75,
            Hourmeter: 8123.25,
            Temperature: -4.5);

    [Test]
    public void MapToPositionVm_FromPosition_MapsEveryField()
    {
        var result = CreatePosition().MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.DeviceName, Is.EqualTo("Truck-1"));
            Assert.That(result.TransporterType, Is.EqualTo("Truck"));
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.Altitude, Is.EqualTo(2640.5));
            Assert.That(result.DeviceDateTime, Is.EqualTo(DeviceTime));
            Assert.That(result.ServerDateTime, Is.EqualTo(ServerTime));
            Assert.That(result.Speed, Is.EqualTo(62.5));
            Assert.That(result.Course, Is.EqualTo(187.25));
            Assert.That(result.EventId, Is.EqualTo(7));
            // A non-zero DistanceToAddress is appended to the address by GetAddress.
            Assert.That(result.Address, Is.EqualTo("Calle 100 (0.35 km)"));
            Assert.That(result.City, Is.EqualTo("Bogota"));
            Assert.That(result.State, Is.EqualTo("Cundinamarca"));
            Assert.That(result.Country, Is.EqualTo("Colombia"));
            Assert.That(result.Attributes!.Value.Ignition, Is.True);
            Assert.That(result.Attributes!.Value.Satellites, Is.EqualTo(11));
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(154320.75));
            Assert.That(result.Attributes!.Value.Temperature, Is.EqualTo(-4.5));
        }
    }

    [Test]
    public void MapToPositionVm_FromDevicePosition_MapsEveryFieldAndLeavesServerTimeUnset()
    {
        var result = CreateDevicePosition().MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.Altitude, Is.EqualTo(2640.5));
            Assert.That(result.DeviceDateTime, Is.EqualTo(DeviceTime));
            Assert.That(result.ServerDateTime, Is.Null);
            Assert.That(result.Speed, Is.EqualTo(62.5));
            Assert.That(result.Course, Is.EqualTo(187.25));
            Assert.That(result.EventId, Is.Null);
            // DistanceToAddress of 0 leaves the address untouched.
            Assert.That(result.Address, Is.EqualTo("Calle 100"));
            Assert.That(result.Attributes!.Value.Hourmeter, Is.EqualTo(8123.25));
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(154320.75));
        }
    }

    [Test]
    public void MapToPositionVm_WithDeviceDictionary_MatchesOnPlateAndSkipsUnknownVehicles()
    {
        var known = CreatePosition();
        var unknown = CreatePosition() with { Plate = "ZZZ999" };
        var dictionary = new Dictionary<string, DeviceTransporterVm> { ["ABC123"] = Device };

        var result = new[] { known, unknown }.MapToPositionVm(dictionary).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result[0].Latitude, Is.EqualTo(4.710989));
        }
    }
}
