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

using TrackHub.Router.Infrastructure.Plaspy.Mappers;
using TrackHub.Router.Infrastructure.Plaspy.Models;

namespace TrackHub.Router.Infrastructure.Plaspy.Tests;

[TestFixture]
public class PositionMapperTests
{
    [Test]
    public void MapToPositionVm_MapsCoordinatesSpeedCourseAndAttributes()
    {
        var device = new DeviceTransporterVm { TransporterId = Guid.NewGuid(), Serial = "abc", Name = "Van", TransporterType = "Truck" };
        var location = new Location("2026-03-01T10:15:30", 4.6097, -74.0817, 42.5, 180, 3.5, 12345.6);

        var position = location.MapToPositionVm(device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.TransporterId, Is.EqualTo(device.TransporterId));
            Assert.That(position.Latitude, Is.EqualTo(4.6097));
            Assert.That(position.Longitude, Is.EqualTo(-74.0817));
            Assert.That(position.Speed, Is.EqualTo(42.5));
            Assert.That(position.Course, Is.EqualTo(180));
            Assert.That(position.Altitude, Is.Null);
            Assert.That(position.ServerDateTime, Is.Null);
            Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2026, 3, 1, 10, 15, 30, TimeSpan.Zero)));
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(12345600).Within(1e-6));
            Assert.That(position.Attributes!.Value.Temperature, Is.EqualTo(3.5));
        }
    }

    [Test]
    public void MapToPositionVm_MissingSpeedAndMileage_AreZeroAndNull()
    {
        var position = new Location("2026-03-01T10:15:30Z", 1, 2, null, null, null, null).MapToPositionVm(new DeviceTransporterVm());

        Assert.That(position.Speed, Is.Zero);
        Assert.That(position.Attributes!.Value.Mileage, Is.Null);
    }

    [TestCase("2026-03-01T10:15:30")]
    [TestCase("2026-03-01T10:15:30Z")]
    [TestCase("2026-03-01T05:15:30-05:00")]
    public void ParseUtc_ReadsUnzonedValuesAsUtc(string value)
        => Assert.That(PositionMapper.ParseUtc(value), Is.EqualTo(new DateTimeOffset(2026, 3, 1, 10, 15, 30, TimeSpan.Zero)));

    [Test]
    public void FormatUtc_WritesIsoUtc()
        => Assert.That(PositionMapper.FormatUtc(new DateTimeOffset(2026, 3, 1, 5, 15, 30, TimeSpan.FromHours(-5))), Is.EqualTo("2026-03-01T10:15:30Z"));

    [Test]
    public void Map_UsesIdAsSerial_IdentifierZero_AndSkipsDevicesWithoutId()
    {
        var devices = DeviceMapper.Instance.Map(new[] { new Device("dev-1", "Van"), new Device(null, "Ghost"), new Device("dev-2", null) }).ToList();

        Assert.That(devices.Select(d => (d.Identifier, d.Serial, d.Name)), Is.EqualTo(new[] { (0, "dev-1", "Van"), (0, "dev-2", "dev-2") }));
    }
}
