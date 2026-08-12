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

using TrackHub.Router.Infrastructure.GpsGate.Mappers;
using TrackHub.Router.Infrastructure.GpsGate.Models;

namespace TrackHub.Router.Infrastructure.GpsGate.Tests;

// Value-correctness tests for the GpsGate mapper (TT-01).
[TestFixture]
public class PositionMapperTests
{
    private static DeviceTransporterVm Device { get; } = new()
    {
        TransporterId = Guid.Parse("a1f4c8d2-3e56-4b79-8c0d-5f2a6b1e9d47"),
        Identifier = 8,
        Serial = "SER-8",
        Name = "Bus-9",
        TransporterType = "Truck",
        TransporterTypeId = (short)TransporterType.Truck
    };

    private static readonly DateTimeOffset Timestamp = new(2026, 7, 17, 12, 30, 0, TimeSpan.Zero);

    [Test]
    public void MapToPositionVm_MapsCoordinatesAndTimestamp()
    {
        var device = new Device(
            Id: 8,
            Name: "Bus-9",
            IMEI: "356938035643809",
            Latitude: 4.710989,
            Longitude: -74.072092,
            Altitude: 2640,
            TimeStamp: Timestamp);

        var before = DateTimeOffset.UtcNow;
        var result = device.MapToPositionVm(Device);
        var after = DateTimeOffset.UtcNow;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.DeviceName, Is.EqualTo("Bus-9"));
            Assert.That(result.TransporterType, Is.EqualTo("Truck"));
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.DeviceDateTime, Is.EqualTo(Timestamp));
            Assert.That(result.ServerDateTime, Is.InRange(before, after));
            // GpsGate's device list endpoint carries no altitude, speed, course or attributes.
            Assert.That(result.Altitude, Is.Null);
            Assert.That(result.Speed, Is.Zero);
            Assert.That(result.Course, Is.Null);
            Assert.That(result.Attributes, Is.Null);
        }
    }

    [Test]
    public void MapToPositionVm_WithoutTimestamp_FallsBackToUtcNow()
    {
        var device = new Device(
            Id: 8,
            Name: "Bus-9",
            IMEI: "356938035643809",
            Latitude: 4.710989,
            Longitude: -74.072092,
            Altitude: null,
            TimeStamp: null);

        var before = DateTimeOffset.UtcNow;
        var result = device.MapToPositionVm(Device);
        var after = DateTimeOffset.UtcNow;

        Assert.That(result.DeviceDateTime, Is.InRange(before, after));
    }
}
