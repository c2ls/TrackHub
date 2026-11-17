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
using TrackHub.Router.Infrastructure.GpsTrackit.Mappers;
using TrackHub.Router.Infrastructure.GpsTrackit.Models;

namespace TrackHub.Router.Infrastructure.GpsTrackit.Tests;

[TestFixture]
public class PositionMapperTests
{
    private static readonly DeviceTransporterVm Device = new() { TransporterId = Guid.NewGuid(), Serial = "200154400", Name = "ABC123", TransporterType = "Truck" };

    [Test]
    public void Reading_BindsTheDocumentedPayload_AndConvertsImperialUnits()
    {
        const string json = """
            {"rowId":173880540646,"deviceId":"200154400","latitude":3.554231,"longitude":-73.704123,"heading":259.600006,
            "eventName":"Travel Start","ignitionStatus":"on","engineOdometer":8985.84,"satelliteCount":null,"speed":5,
            "unitTime":"2025-02-06T00:36:29.500Z","formattedAddress":"Transversal 2ª, Granada, Meta, Colombia"}
            """;
        var reading = JsonSerializer.Deserialize<Reading>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var position = reading.MapToPositionVm(Device)!.Value;

        using (Assert.EnterMultipleScope())
        {
            Assert.That((position.Latitude, position.Longitude), Is.EqualTo((3.554231, -73.704123)));
            Assert.That(position.Speed, Is.EqualTo(8.04672).Within(1e-9));
            Assert.That(position.Course, Is.EqualTo(259.600006));
            Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2025, 2, 6, 0, 36, 29, 500, TimeSpan.Zero)));
            Assert.That(position.Address, Is.EqualTo("Transversal 2ª, Granada, Meta, Colombia"));
            Assert.That(position.Attributes!.Value.Ignition, Is.True);
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(8985.84 * 1609.344).Within(1e-6));
        }
    }

    [TestCase(1738802189d)]
    [TestCase(1738802189000d)]
    public void MapToPositionVm_LastReading_AcceptsEpochSecondsOrMilliseconds(double unitTime)
    {
        var reading = new LastReading(unitTime, 1, 2, 0, 0, null, "Ignition Off", new ReadingAddress("Calle 1, Bogotá", "Bogotá", "Cundinamarca", "Colombia"));

        var position = reading.MapToPositionVm(Device)!.Value;

        Assert.That(position.DeviceDateTime, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1738802189)));
        Assert.That((position.Address, position.City, position.Country), Is.EqualTo(("Calle 1, Bogotá", "Bogotá", "Colombia")));
    }

    [Test]
    public void MapToPositionVm_UpdatedUnitWithoutReading_ReturnsNull()
        => Assert.That(new LastReading(null, 0, 0, null, null, null, null, null).MapToPositionVm(Device), Is.Null);

    [Test]
    public void Map_UsesUnitIdAsSerial_AndNameFallsBackToPlate()
    {
        var devices = DeviceMapper.Instance.Map(new[]
        {
            new Unit(1, "Unit 1", null, null),
            new Unit(2, null, new Asset("XYZ987"), null),
            new Unit(null, "Ghost", null, null)
        }).ToList();

        Assert.That(devices.Select(d => (d.Serial, d.Name)), Is.EqualTo(new[] { ("1", "Unit 1"), ("2", "XYZ987") }));
    }
}
