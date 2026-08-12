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

using TrackHub.Router.Infrastructure.Flespi.Mappers;
using TrackHub.Router.Infrastructure.Flespi.Models;

namespace TrackHub.Router.Infrastructure.Flespi.Tests;

// Value-correctness tests for the Flespi mapper (TT-01).
[TestFixture]
public class PositionMapperTests
{
    private static DeviceTransporterVm Device { get; } = new()
    {
        TransporterId = Guid.Parse("cb27d905-4e1a-4d63-9f80-7a3e2c6b8d14"),
        Identifier = 1201,
        Serial = "SER-1201",
        Name = "Trailer-7",
        TransporterType = "Truck",
        TransporterTypeId = (short)TransporterType.Truck
    };

    // 2026-07-17T09:20:00Z
    private const double UnixTimestamp = 1784280000d;
    private static readonly DateTimeOffset ExpectedTime = DateTimeOffset.FromUnixTimeSeconds(1784280000);

    private static Message CreateMessage()
        => new(
            Ident: 1201,
            Device_id: 1201,
            Channel_id: 55,
            Timestamp: UnixTimestamp,
            Position_latitude: 4.710989,
            Position_longitude: -74.072092,
            Position_altitude: 2641.0,
            Position_speed: 48.3,
            Position_direction: 310,
            Position_satellites: 9);

    [Test]
    public void MapToPositionVm_MapsCoreFieldsAndConvertsUnixTimestamp()
    {
        var result = CreateMessage().MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.DeviceName, Is.EqualTo("Trailer-7"));
            Assert.That(result.TransporterType, Is.EqualTo("Truck"));
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.Altitude, Is.EqualTo(2641.0));
            Assert.That(result.DeviceDateTime, Is.EqualTo(ExpectedTime));
            Assert.That(result.ServerDateTime, Is.Null);
            Assert.That(result.Speed, Is.EqualTo(48.3));
            Assert.That(result.Course, Is.EqualTo(310));
            Assert.That(result.Attributes!.Value.Satellites, Is.EqualTo(9));
        }
    }

    [Test]
    public void MapToPositionVm_WithMissingCoordinates_DefaultsToZero()
    {
        var message = CreateMessage() with
        {
            Position_latitude = null,
            Position_longitude = null,
            Position_speed = null
        };

        var result = message.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Latitude, Is.Zero);
            Assert.That(result.Longitude, Is.Zero);
            Assert.That(result.Speed, Is.Zero);
        }
    }

    [Test]
    public void MapToPositionVm_ForCollection_SkipsMessagesWithoutCoordinates()
    {
        var withPosition = CreateMessage();
        var missingLatitude = CreateMessage() with { Position_latitude = null };
        var missingLongitude = CreateMessage() with { Position_longitude = null };

        var result = new[] { withPosition, missingLatitude, missingLongitude }
            .MapToPositionVm(Device)
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Latitude, Is.EqualTo(4.710989));
            Assert.That(result[0].Longitude, Is.EqualTo(-74.072092));
        }
    }
}
