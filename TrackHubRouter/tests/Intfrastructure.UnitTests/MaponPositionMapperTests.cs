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

using System.Text.Json;
using TrackHub.Router.Infrastructure.Mapon.Mappers;
using TrackHub.Router.Infrastructure.Mapon.Models;

namespace TrackHub.Router.Infrastructure.Mapon.Tests;

[TestFixture]
public class PositionMapperTests
{
    private static readonly DeviceTransporterVm Device = new() { TransporterId = Guid.NewGuid(), Serial = "14700", Name = "XX1234", TransporterType = "Truck" };

    [Test]
    public void UnitList_BindsTheDocumentedPayload_AndMapsItsCurrentState()
    {
        const string json = """
            {"data":{"units":[{"unit_id":14700,"number":"XX1234","label":"VOLVO FH","lat":56.9529,"lng":24.0867,
            "last_update":"2017-05-22T12:23:46Z","speed":72,"direction":245,"mileage":113746523,
            "ignition":{"gmt":"2017-05-22T12:23:46Z","value":"on"},"altitude":{"gmt":"2017-05-22T12:23:46Z","value":"15"}}]}}
            """;
        var unit = JsonSerializer.Deserialize<MaponResponse<UnitList>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Data!.Units!.Single();

        var position = unit.MapToPositionVm(Device)!.Value;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.Serial, Is.EqualTo("14700"));
            Assert.That((position.Latitude, position.Longitude), Is.EqualTo((56.9529, 24.0867)));
            Assert.That(position.Speed, Is.EqualTo(72));
            Assert.That(position.Course, Is.EqualTo(245));
            Assert.That(position.Altitude, Is.EqualTo(15));
            Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2017, 5, 22, 12, 23, 46, TimeSpan.Zero)));
            Assert.That(position.Attributes!.Value.Ignition, Is.True);
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(113746523));
        }
    }

    [Test]
    public void MapToPositionVm_RoutePoint_MapsTimeAndSpeed_WithoutHeading()
    {
        var position = new RoutePoint(new DateTimeOffset(2017, 4, 3, 14, 44, 16, TimeSpan.Zero), 28.45721, -43.2719, 31).MapToPositionVm(Device)!.Value;

        Assert.That((position.Speed, position.Course), Is.EqualTo((31d, (double?)null)));
        Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2017, 4, 3, 14, 44, 16, TimeSpan.Zero)));
    }

    [Test]
    public void MapToPositionVm_WithoutFixOrTime_ReturnsNull()
    {
        Assert.That(new RoutePoint(null, 1, 2, 0).MapToPositionVm(Device), Is.Null);
        Assert.That(new RoutePoint(DateTimeOffset.UtcNow, 0, 0, 0).MapToPositionVm(Device), Is.Null);
    }

    [Test]
    public void Map_UsesUnitIdAsSerial_AndPlateAsName()
    {
        var devices = DeviceMapper.Instance.Map(new[]
        {
            new Unit(1, "XX1234", "VOLVO", null, null, null, null, null, null, null, null),
            new Unit(2, null, "MAN", null, null, null, null, null, null, null, null),
            new Unit(null, "GHOST", null, null, null, null, null, null, null, null, null)
        }).ToList();

        Assert.That(devices.Select(d => (d.Serial, d.Name)), Is.EqualTo(new[] { ("1", "XX1234"), ("2", "MAN") }));
    }
}
