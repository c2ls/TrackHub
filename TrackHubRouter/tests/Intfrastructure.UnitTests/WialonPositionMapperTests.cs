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

using TrackHub.Router.Infrastructure.Wialon.Mappers;
using TrackHub.Router.Infrastructure.Wialon.Models;

namespace TrackHub.Router.Infrastructure.Wialon.Tests;

// Value-correctness tests for the Wialon mapper (TT-01). Wialon transposes the coordinate pair
// (X = longitude, Y = latitude), so a swap here would silently mirror every reported position.
[TestFixture]
public class PositionMapperTests
{
    private static DeviceTransporterVm Device { get; } = new()
    {
        TransporterId = Guid.Parse("7d3b6e21-95c4-4a08-b17f-2e6d0a4c8b39"),
        Identifier = 3301,
        Serial = "SER-3301",
        Name = "Tanker-2",
        TransporterType = "Truck",
        TransporterTypeId = (short)TransporterType.Truck
    };

    // 2026-07-17T09:20:00Z
    private const long UnixTimestamp = 1784280000L;
    private static readonly DateTimeOffset ExpectedTime = DateTimeOffset.FromUnixTimeSeconds(UnixTimestamp);

    private static Position CreatePosition()
        => new(
            T: UnixTimestamp,
            X: -74.072092,  // longitude
            Y: 4.710989,    // latitude
            Z: 2640,        // altitude
            S: 74,          // speed
            C: 145,         // course
            Sc: 12);        // satellites

    [Test]
    public void MapToPositionVm_FromUnit_MapsYToLatitudeAndXToLongitude()
    {
        var unit = new Unit(Id: 3301, Nm: "Tanker-2", Cls: 2, Uid: "358899051234567", Pos: CreatePosition());

        var result = unit.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.DeviceName, Is.EqualTo("Tanker-2"));
            Assert.That(result.TransporterType, Is.EqualTo("Truck"));
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.Altitude, Is.EqualTo(2640));
            Assert.That(result.DeviceDateTime, Is.EqualTo(ExpectedTime));
            Assert.That(result.ServerDateTime, Is.Null);
            Assert.That(result.Speed, Is.EqualTo(74));
            Assert.That(result.Course, Is.EqualTo(145));
            Assert.That(result.Attributes!.Value.Satellites, Is.EqualTo(12));
        }
    }

    [Test]
    public void MapToPositionVm_FromUnitWithoutPosition_YieldsZeroCoordinatesAndMinDate()
    {
        var unit = new Unit(Id: 3301, Nm: "Tanker-2", Cls: 2, Uid: "358899051234567", Pos: null);

        var result = unit.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Latitude, Is.Zero);
            Assert.That(result.Longitude, Is.Zero);
            Assert.That(result.Altitude, Is.Null);
            Assert.That(result.Speed, Is.Zero);
            Assert.That(result.Course, Is.Null);
            Assert.That(result.DeviceDateTime, Is.EqualTo(DateTimeOffset.MinValue));
        }
    }

    [Test]
    public void MapToPositionVm_FromMessage_UsesMessageTimestampAndOdometer()
    {
        var message = new Message(
            T: UnixTimestamp,
            Tp: "ud",
            Pos: CreatePosition(),
            P: new MessageParams(Odo: 154_320.5, Pwr_ext: 27.4, Hdop: 0.8));

        var result = message.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.DeviceDateTime, Is.EqualTo(ExpectedTime));
            Assert.That(result.Speed, Is.EqualTo(74));
            Assert.That(result.Course, Is.EqualTo(145));
            Assert.That(result.Attributes!.Value.Satellites, Is.EqualTo(12));
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(154_320.5));
        }
    }

    [Test]
    public void MapToPositionVm_ForUnitCollection_SkipsUnitsWithoutPositionOrDeviceMatch()
    {
        var matched = new Unit(3301, "Tanker-2", 2, null, CreatePosition());
        var noPosition = new Unit(3301, "Tanker-2", 2, null, null);
        var unknownDevice = new Unit(9999, "Ghost", 2, null, CreatePosition());
        var dictionary = new Dictionary<int, DeviceTransporterVm> { [3301] = Device };

        var result = new[] { matched, noPosition, unknownDevice }.MapToPositionVm(dictionary).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result[0].Latitude, Is.EqualTo(4.710989));
        }
    }

    [Test]
    public void MapToPositionVm_ForMessageCollection_SkipsMessagesWithoutPosition()
    {
        var withPosition = new Message(UnixTimestamp, "ud", CreatePosition(), null);
        var withoutPosition = new Message(UnixTimestamp, "ud", null, null);

        var result = new[] { withPosition, withoutPosition }.MapToPositionVm(Device).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
    }
}
