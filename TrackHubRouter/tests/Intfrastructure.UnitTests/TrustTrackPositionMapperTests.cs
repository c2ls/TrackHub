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
using TrackHub.Router.Infrastructure.TrustTrack.Mappers;
using TrackHub.Router.Infrastructure.TrustTrack.Models;

namespace TrackHub.Router.Infrastructure.TrustTrack.Tests;

[TestFixture]
public class PositionMapperTests
{
    private static readonly DeviceTransporterVm Device = new() { TransporterId = Guid.NewGuid(), Serial = "obj-1", Name = "ABC123", TransporterType = "Truck" };
    private static readonly DateTimeOffset FixTime = new(2026, 3, 1, 10, 15, 30, TimeSpan.Zero);

    [Test]
    public void MapToPositionVm_LastCoordinate_MapsCoordinatesSpeedDirectionAltitudeAndSatellites()
    {
        var trackedObject = new TrackedObject("obj-1", "Van", null,
            new Position(4.6097, -74.0817, FixTime.ToOffset(TimeSpan.FromHours(-5)), 42, 180, 2600, 9));

        var position = trackedObject.MapToPositionVm(Device)!.Value;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That((position.Latitude, position.Longitude), Is.EqualTo((4.6097, -74.0817)));
            Assert.That(position.Speed, Is.EqualTo(42));
            Assert.That(position.Course, Is.EqualTo(180));
            Assert.That(position.Altitude, Is.EqualTo(2600));
            Assert.That(position.DeviceDateTime, Is.EqualTo(FixTime));
            Assert.That(position.DeviceDateTime.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(position.Attributes!.Value.Satellites, Is.EqualTo(9));
            Assert.That(position.Attributes!.Value.Ignition, Is.Null);
        }
    }

    [Test]
    public void MapToPositionVm_HistoryCoordinate_MapsIgnitionMileageInMetersAndAddress()
    {
        var coordinate = new Coordinate(FixTime, true, new Position(4.6, -74.1, null, 10, 90, null, 7),
            new Address("Colombia", "Bogotá", "Calle 80", "12-34"), new Inputs(new CalculatedInputs(1234.5)));

        var position = coordinate.MapToPositionVm(Device)!.Value;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.Attributes!.Value.Ignition, Is.True);
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(1234500).Within(1e-6));
            Assert.That(position.Address, Is.EqualTo("Calle 80 12-34"));
            Assert.That(position.City, Is.EqualTo("Bogotá"));
            Assert.That(position.Country, Is.EqualTo("Colombia"));
        }
    }

    [Test]
    public void MapToPositionVm_WithoutCoordinateOrFix_ReturnsNull()
    {
        Assert.That(new TrackedObject("obj-1", null, null, null).MapToPositionVm(Device), Is.Null);
        Assert.That(new Coordinate(FixTime, null, new Position(0, 0, null, 0, 0, null, null), null, null).MapToPositionVm(Device), Is.Null);
    }

    [Test]
    public void FormatUtc_WritesEscapedIsoWithMilliseconds()
        => Assert.That(PositionMapper.FormatUtc(new DateTimeOffset(2026, 3, 1, 5, 15, 30, TimeSpan.FromHours(-5))),
            Is.EqualTo("2026-03-01T10%3A15%3A30.000Z"));

    [Test]
    public void Map_NameFallsBackToPlateThenId()
    {
        var devices = DeviceMapper.Instance.Map(new[]
        {
            new TrackedObject("1", "Van", null, null),
            new TrackedObject("2", null, new VehicleParams("XYZ987"), null),
            new TrackedObject("3", null, null, null),
            new TrackedObject(null, "Ghost", null, null)
        }).ToList();

        Assert.That(devices.Select(d => (d.Serial, d.Name)), Is.EqualTo(new[] { ("1", "Van"), ("2", "XYZ987"), ("3", "3") }));
    }

    [Test]
    public void LastCoordinatePage_BindsTheSwaggerPayload()
    {
        const string json = """
            {"continuation_token":"2026-03-01T10:15:30.000Z","results":[{"id":"obj-1","imei":356938035643809,"name":"Van",
            "vehicle_params":{"plate_number":"ABC123"},"last_coordinate":{"latitude":4.6097,"longitude":-74.0817,
            "datetime":"2026-03-01T10:15:30Z","speed":42,"direction":180,"altitude":2600,"satellites_count":9}}]}
            """;
        var trackedObject = JsonSerializer.Deserialize<ContinuationPage<TrackedObject>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Entries.Single();

        var position = trackedObject.MapToPositionVm(Device)!.Value;

        Assert.That(trackedObject.VehicleParams!.PlateNumber, Is.EqualTo("ABC123"));
        Assert.That((position.Speed, position.Course, position.Altitude), Is.EqualTo((42d, (double?)180, (double?)2600)));
        Assert.That(position.DeviceDateTime, Is.EqualTo(FixTime));
    }

    [TestCase("ON", true)]
    [TestCase("OFF", false)]
    [TestCase("UNKNOWN", null)]
    public void Coordinate_BindsIgnitionStatus(string status, bool? expected)
    {
        var json = $$$"""{"datetime":"2026-03-01T10:15:30Z","ignition_status":"{{{status}}}","position":{"latitude":1,"longitude":2,"speed":10.5}}""";

        var coordinate = JsonSerializer.Deserialize<Coordinate>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.That(coordinate.MapToPositionVm(Device)!.Value.Attributes!.Value.Ignition, Is.EqualTo(expected));
    }
}
