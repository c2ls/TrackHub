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
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Infrastructure.Mettax.Mappers;
using TrackHub.Router.Infrastructure.Mettax.Models;

namespace TrackHub.Router.Infrastructure.Mettax.Tests;

[TestFixture]
public class PositionMapperTests
{
    private static readonly DeviceTransporterVm Device = new() { TransporterId = Guid.NewGuid(), Serial = "711062605855", Name = "ABC123", TransporterType = "Truck" };

    private static Fix CreateFix(double? lat = 4.6097, double? lon = -74.0817, string? reportTime = null, string? deviceTime = "2026-03-01 10:15:40")
        => new("711062605855", reportTime, deviceTime, lat, lon, 42.5, 180, true, 9, 80, 1, "Calle 80", 1234.5);

    [Test]
    public void MapToPositionVm_MapsCoordinatesSpeedCourseAndAttributes()
    {
        var position = CreateFix().MapToPositionVm("2026-03-01 10:15:30", Device)!.Value;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(position.Latitude, Is.EqualTo(4.6097));
            Assert.That(position.Longitude, Is.EqualTo(-74.0817));
            Assert.That(position.Speed, Is.EqualTo(42.5));
            Assert.That(position.Course, Is.EqualTo(180));
            Assert.That(position.Address, Is.EqualTo("Calle 80"));
            Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2026, 3, 1, 10, 15, 30, TimeSpan.Zero)));
            Assert.That(position.ServerDateTime, Is.EqualTo(new DateTimeOffset(2026, 3, 1, 10, 15, 40, TimeSpan.Zero)));
            Assert.That(position.Attributes!.Value.Ignition, Is.True);
            Assert.That(position.Attributes!.Value.Satellites, Is.EqualTo(9));
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(1234500).Within(1e-6));
            Assert.That(AttributesExtra.ToDictionary(position.Attributes!.Value.Extra),
                Is.EquivalentTo(new Dictionary<string, string?> { ["battery"] = "80", ["gpsFix"] = "gps" }));
        }
    }

    [Test]
    public void MapToPositionVm_WithoutReportTime_FallsBackToTheSaveTime()
        => Assert.That(CreateFix().MapToPositionVm(null, Device)!.Value.DeviceDateTime,
            Is.EqualTo(new DateTimeOffset(2026, 3, 1, 10, 15, 40, TimeSpan.Zero)));

    [TestCase(null, -74.0)]
    [TestCase(0d, 0d)]
    public void MapToPositionVm_WithoutAFix_ReturnsNull(double? lat, double? lon)
        => Assert.That(CreateFix(lat, lon).MapToPositionVm("2026-03-01 10:15:30", Device), Is.Null);

    [Test]
    public void MapToPositionVm_UnparsableTime_ReturnsNull()
        => Assert.That(CreateFix(deviceTime: null).MapToPositionVm("01/03/2026", Device), Is.Null);

    [Test]
    public void Map_UsesIdAsSerial_AndNameFallsBackToId()
    {
        var devices = DeviceMapper.Instance.Map(new[] { new Device("1", "ABC123"), new Device(null, "Ghost"), new Device("2", null) }).ToList();

        Assert.That(devices.Select(d => (d.Identifier, d.Serial, d.Name)), Is.EqualTo(new[] { (0, "1", "ABC123"), (0, "2", "2") }));
    }

    [Test]
    public void ShadowResponse_BindsTheOpenApiPayload()
    {
        const string json = """
            {"code":0,"msg":"success","data":[{"deviceData":{"deviceName":"ABC123","deviceId":"711062605855","deviceTime":"2026-03-01 10:15:40",
            "address":"Calle 80","speed":42.5,"course":180,"acc":1,"lat":4.6097,"lon":-74.0817,"num":9,"quantity":80,"type":1,"odometer":1234.5},
            "expand":{"statusValue":"1","reportTime":"2026-03-01 10:15:30"}}]}
            """;
        var shadow = JsonSerializer.Deserialize<MettaxResponse<IReadOnlyList<DeviceShadow>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Data!.Single();

        var position = shadow.DeviceData!.MapToPositionVm(shadow.Expand!.ReportTime, Device)!.Value;

        Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2026, 3, 1, 10, 15, 30, TimeSpan.Zero)));
        Assert.That((position.Attributes!.Value.Ignition, position.Attributes!.Value.Satellites), Is.EqualTo(((bool?)true, (int?)9)));
    }
}
