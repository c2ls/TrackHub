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

using TrackHub.Router.Infrastructure.Webfleet.Mappers;
using TrackHub.Router.Infrastructure.Webfleet.Models;

namespace TrackHub.Router.Infrastructure.Webfleet.Tests;

[TestFixture]
public class PositionMapperTests
{
    private static DeviceTransporterVm Device { get; } = new()
    {
        TransporterId = Guid.Parse("0c4d6f1e-93a2-4b57-8e61-2f7a9d3c5b10"),
        Identifier = 0,
        Serial = "042",
        Name = "Van-42",
        TransporterType = "Truck",
        TransporterTypeId = (short)TransporterType.Truck
    };

    private static ObjectReport Report(
        long? odometer = 1234,
        long? odometerLong = null,
        int? ignition = 1,
        long? engineSeconds = 5400)
        => new(
            Objectno: "042",
            Objectname: "Van 42",
            Objectuid: "1-abc",
            Deleted: false,
            Pos_time: new DateTimeOffset(2026, 7, 17, 11, 20, 0, TimeSpan.FromHours(2)),
            Latitude_mdeg: 51339695,
            Longitude_mdeg: -12373074,
            Speed: 63,
            Course: 210,
            Postext: "Leipzig, Germany",
            Ignition: ignition,
            Odometer: odometer,
            Odometer_long: odometerLong,
            Engine_operation_time: engineSeconds);

    [Test]
    public void MapToPositionVm_FromObjectReport_ConvertsUnitsAndTime()
    {
        var result = Report().MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.DeviceName, Is.EqualTo("Van-42"));
            Assert.That(result.Latitude, Is.EqualTo(51.339695).Within(1e-9));
            Assert.That(result.Longitude, Is.EqualTo(-12.373074).Within(1e-9));
            Assert.That(result.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2026, 7, 17, 9, 20, 0, TimeSpan.Zero)));
            Assert.That(result.DeviceDateTime.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(result.Speed, Is.EqualTo(63));
            Assert.That(result.Course, Is.EqualTo(210));
            Assert.That(result.Address, Is.EqualTo("Leipzig, Germany"));
            Assert.That(result.Altitude, Is.Null);
            Assert.That(result.Attributes!.Value.Ignition, Is.True);
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(123400));
            Assert.That(result.Attributes!.Value.Hourmeter, Is.EqualTo(1.5));
            Assert.That(result.Attributes!.Value.Satellites, Is.Null);
            Assert.That(result.Attributes!.Value.Temperature, Is.Null);
        }
    }

    [Test]
    public void MapToPositionVm_PrefersOdometerInMetersOverHundredMeterOdometer()
    {
        var result = Report(odometer: 1234, odometerLong: 123456).MapToPositionVm(Device);

        Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(123456));
    }

    [TestCase(0, false)]
    [TestCase(null, null)]
    public void MapToPositionVm_MapsIgnitionState(int? ignition, bool? expected)
    {
        var result = Report(ignition: ignition, odometer: null, engineSeconds: null).MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Attributes!.Value.Ignition, Is.EqualTo(expected));
            Assert.That(result.Attributes!.Value.Mileage, Is.Null);
            Assert.That(result.Attributes!.Value.Hourmeter, Is.Null);
        }
    }

    [Test]
    public void MapToPositionVm_ForReportCollection_SkipsUnmatchedAndPositionlessObjects()
    {
        var matched = Report();
        var positionless = matched with { Pos_time = null };
        var unknown = matched with { Objectno = "999" };
        var dictionary = new Dictionary<string, DeviceTransporterVm> { ["042"] = Device };

        var result = new[] { matched, positionless, unknown }.MapToPositionVm(dictionary).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void MapToPositionVm_FromTrackPoint_ConvertsUnitsAndTimes()
    {
        var point = new TrackPoint(
            Pos_time: new DateTimeOffset(2026, 7, 17, 9, 20, 0, TimeSpan.Zero),
            Receivetime: new DateTimeOffset(2026, 7, 17, 11, 20, 5, TimeSpan.FromHours(2)),
            Latitude: -4710989,
            Longitude: -74072092,
            Speed: 48,
            Course: 90,
            Odometer: 50,
            Country: "CO",
            State: "DC");

        var result = point.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Latitude, Is.EqualTo(-4.710989).Within(1e-9));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092).Within(1e-9));
            Assert.That(result.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2026, 7, 17, 9, 20, 0, TimeSpan.Zero)));
            Assert.That(result.ServerDateTime, Is.EqualTo(new DateTimeOffset(2026, 7, 17, 9, 20, 5, TimeSpan.Zero)));
            Assert.That(result.ServerDateTime!.Value.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(result.Speed, Is.EqualTo(48));
            Assert.That(result.Course, Is.EqualTo(90));
            Assert.That(result.Country, Is.EqualTo("CO"));
            Assert.That(result.State, Is.EqualTo("DC"));
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(5000));
        }
    }

    [Test]
    public void MapToPositionVm_FromTrackPointWithoutOdometer_HasNoAttributes()
    {
        var point = new TrackPoint(DateTimeOffset.UtcNow, null, 1, 1, null, null, null, null, null);

        var result = point.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Attributes, Is.Null);
            Assert.That(result.Speed, Is.Zero);
            Assert.That(result.ServerDateTime, Is.Null);
        }
    }
}
