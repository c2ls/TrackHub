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

using TrackHub.Router.Infrastructure.Pegasus.Mappers;
using TrackHub.Router.Infrastructure.Pegasus.Models;

namespace TrackHub.Router.Infrastructure.Pegasus.Tests;

[TestFixture]
public class PositionMapperTests
{
    private static DeviceTransporterVm Device(string serial = "867162029806270")
        => new() { TransporterId = Guid.NewGuid(), Identifier = 2839, Serial = serial, Name = "Truck 1", TransporterType = "Truck" };

    private static VehicleLatest Latest(long imei, LatestLocation? location)
        => new(2839, "Truck 1", new LatestDevice(imei, new DeviceLatest(location, new LatestIos(true), new LatestCounters(39741620, 7991180))));

    [Test]
    public void MapToPositionVm_Latest_ConvertsMphEpochSecondsAndCounters()
    {
        var device = Device();
        var location = new LatestLocation(-37.04291, -72.39416, 25, 10, 1659975548.5, 1659977251.342421);
        var latest = Latest(867162029806270, location).Device!.Latest!;

        var position = latest.MapToPositionVm(location, device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.TransporterId, Is.EqualTo(device.TransporterId));
            Assert.That(position.Latitude, Is.EqualTo(-37.04291));
            Assert.That(position.Longitude, Is.EqualTo(-72.39416));
            Assert.That(position.Speed, Is.EqualTo(16.09344).Within(1e-9));
            Assert.That(position.Course, Is.EqualTo(25));
            Assert.That(position.DeviceDateTime, Is.EqualTo(DateTimeOffset.FromUnixTimeMilliseconds(1659975548500)));
            Assert.That(position.DeviceDateTime.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(position.ServerDateTime, Is.EqualTo(DateTimeOffset.FromUnixTimeMilliseconds(1659977251342)));
            Assert.That(position.Attributes!.Value.Ignition, Is.True);
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(39741620));
            Assert.That(position.Attributes!.Value.Hourmeter, Is.EqualTo(7991180 / 3600d).Within(1e-9));
        }
    }

    [Test]
    public void MapToPositionVm_LatestCollection_KeepsOnlyRequestedDevicesWithALocation()
    {
        var device = Device();
        var location = new LatestLocation(1, 2, 0, 0, 1659975548, null);
        var vehicles = new[]
        {
            Latest(867162029806270, location),
            Latest(111111111111111, location),
            Latest(867162029806270, null),
            new VehicleLatest(5, "No device", null)
        };

        var positions = vehicles.MapToPositionVm(new Dictionary<string, DeviceTransporterVm> { [device.Serial] = device }).ToList();

        Assert.That(positions, Has.Count.EqualTo(1));
        Assert.That(positions[0].TransporterId, Is.EqualTo(device.TransporterId));
    }

    [TestCase("2018-10-16T22:12:20")]
    [TestCase("2018-10-16T22:12:20Z")]
    [TestCase("2018-10-16T22:12:20.000Z")]
    public void ParseUtc_WithOrWithoutZone_IsReadAsUtc(string value)
        => Assert.That(PositionMapper.ParseUtc(value), Is.EqualTo(new DateTimeOffset(2018, 10, 16, 22, 12, 20, TimeSpan.Zero)));

    [Test]
    public void FormatUtc_ConvertsToUtcWallClock()
        => Assert.That(PositionMapper.FormatUtc(new DateTimeOffset(2026, 1, 2, 8, 30, 15, TimeSpan.FromHours(-5))), Is.EqualTo("2026-01-02T13:30:15"));

    [Test]
    public void MapToPositionVm_RawEvent_MapsEveryFieldAndUnit()
    {
        var device = Device();
        var rawEvent = new RawEvent("2018-10-06T15:50:58", "2018-10-06T15:51:02.014862", 25.69714, -80.16553, 12.5, 7, 191, 9, false, 72542, 40823, 4.5);

        var position = rawEvent.MapToPositionVm(device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.Latitude, Is.EqualTo(25.69714));
            Assert.That(position.Longitude, Is.EqualTo(-80.16553));
            Assert.That(position.Altitude, Is.EqualTo(12.5));
            Assert.That(position.Speed, Is.EqualTo(7 * 1.609344).Within(1e-9));
            Assert.That(position.Course, Is.EqualTo(191));
            Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2018, 10, 6, 15, 50, 58, TimeSpan.Zero)));
            Assert.That(position.ServerDateTime, Is.EqualTo(new DateTimeOffset(2018, 10, 6, 15, 51, 2, TimeSpan.Zero).AddTicks(148620)));
            Assert.That(position.Attributes!.Value.Ignition, Is.False);
            Assert.That(position.Attributes!.Value.Satellites, Is.EqualTo(9));
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(72542));
            Assert.That(position.Attributes!.Value.Hourmeter, Is.EqualTo(40823 / 3600d).Within(1e-9));
            Assert.That(position.Attributes!.Value.Temperature, Is.EqualTo(4.5));
        }
    }

    [Test]
    public void MapToPositionVm_RawEvents_SkipsEventsWithoutCoordinates()
    {
        var events = new[]
        {
            new RawEvent("2018-10-06T15:50:58", null, 1, 2, null, 0, null, null, null, null, null, null),
            new RawEvent("2018-10-06T15:51:58", null, null, null, null, 0, null, null, null, null, null, null)
        };

        Assert.That(events.MapToPositionVm(Device()).Count(), Is.EqualTo(1));
    }

    [Test]
    public void Map_SkipsVehiclesWithoutADevice_AndMapsIdAndImei()
    {
        var vehicles = new[]
        {
            new Vehicle(3301, "Real Kamaleon 6", 867162027102748),
            new Vehicle(3302, "Unassigned", null),
            new Vehicle(3303, "Zero", 0)
        };

        var devices = DeviceMapper.Instance.Map(vehicles).ToList();

        Assert.That(devices, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(devices[0].Identifier, Is.EqualTo(3301));
            Assert.That(devices[0].Serial, Is.EqualTo("867162027102748"));
            Assert.That(devices[0].Name, Is.EqualTo("Real Kamaleon 6"));
            Assert.That(devices[0].DeviceId, Is.Null);
        }
    }

    [Test]
    public void ToIdentifier_IdBeyondInt_IsZero()
        => Assert.That(DeviceMapper.ToIdentifier((long)int.MaxValue + 1), Is.Zero);
}
