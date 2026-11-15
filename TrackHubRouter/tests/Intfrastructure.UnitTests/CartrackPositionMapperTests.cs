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

using Common.Domain.Extensions;
using TrackHub.Router.Infrastructure.Cartrack.Mappers;
using TrackHub.Router.Infrastructure.Cartrack.Models;

namespace TrackHub.Router.Infrastructure.Cartrack.Tests;

[TestFixture]
public class PositionMapperTests
{
    private static readonly DeviceTransporterVm Device = new()
    {
        TransporterId = Guid.NewGuid(),
        Name = "Truck-1",
        TransporterType = "Truck",
    };

    private static readonly DateTimeOffset Time = new(2022, 1, 1, 8, 12, 2, TimeSpan.Zero);

    private static VehicleEvent Event(double? latitude = 1.320227, double? longitude = 103.889653, int? gpsFixType = 3, double? temp1 = 0)
        => new(Time, Time.AddSeconds(5), latitude, longitude, gpsFixType, 100, 60, true, 1000, 150, 152, temp1, "Tanjong Pagar, Singapore");

    [Test]
    public void MapToPositionVm_Event_MapsCoreFieldsAndConvertsClockToHours()
    {
        var result = Event().MapToPositionVm(Device);

        Assert.Multiple(() =>
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.Latitude, Is.EqualTo(1.320227));
            Assert.That(result.Longitude, Is.EqualTo(103.889653));
            Assert.That(result.Altitude, Is.EqualTo(152));
            Assert.That(result.DeviceDateTime, Is.EqualTo(Time));
            Assert.That(result.ServerDateTime, Is.EqualTo(Time.AddSeconds(5)));
            Assert.That(result.Speed, Is.EqualTo(60));
            Assert.That(result.Course, Is.EqualTo(100));
            Assert.That(result.Address, Is.EqualTo("Tanjong Pagar, Singapore"));
            Assert.That(result.Attributes!.Value.Ignition, Is.True);
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(1000));
            Assert.That(result.Attributes!.Value.Hourmeter, Is.EqualTo(2.5));
            Assert.That(result.Attributes!.Value.Temperature, Is.Null);
        });
    }

    [Test]
    public void MapToPositionVm_Event_KeepsNonZeroTemperature()
    {
        Assert.That(Event(temp1: -18.5).MapToPositionVm(Device).Attributes!.Value.Temperature, Is.EqualTo(-18.5));
    }

    [TestCase(1.0, 2.0, 3, true)]
    [TestCase(1.0, 2.0, null, true)]
    [TestCase(1.0, 2.0, 0, false)]
    [TestCase(null, 2.0, 3, false)]
    [TestCase(1.0, null, 3, false)]
    public void HasFix_RequiresCoordinatesAndASatelliteFix(double? latitude, double? longitude, int? gpsFixType, bool expected)
    {
        Assert.That(Event(latitude, longitude, gpsFixType).HasFix(), Is.EqualTo(expected));
    }

    [Test]
    public void MapToPositionVm_Status_PrefersLocationTimeAndSkipsVehiclesWithoutCoordinates()
    {
        var located = new VehicleStatus(1, "ABX123", Time, 90, 40, false, 5000, 30, 10, 0,
            new StatusLocation(Time.AddMinutes(-1), -26.2041, 28.0473, ""));
        var untimed = located with { Registration = "NOLOC", Location = new StatusLocation(null, null, null, null) };
        var unrequested = located with { Registration = "OTHER" };
        var lookup = new Dictionary<string, DeviceTransporterVm> { ["ABX123"] = Device, ["NOLOC"] = Device };

        var result = new[] { located, untimed, unrequested }.MapToPositionVm(lookup).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.DeviceDateTime, Is.EqualTo(Time.AddMinutes(-1)));
            Assert.That(result.Latitude, Is.EqualTo(-26.2041));
            Assert.That(result.Address, Is.Null);
            Assert.That(result.Attributes!.Value.Hourmeter, Is.EqualTo(0.5));
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(5000));
            Assert.That(result.Attributes!.Value.Temperature, Is.Null);
            Assert.That(result.Attributes!.Value.Ignition, Is.False);
        });
    }

    [Test]
    public void MapToPositionVm_StatusWithoutAnyTimestamp_IsSkipped()
    {
        var status = new VehicleStatus(1, "ABX123", null, null, null, null, null, null, null, null,
            new StatusLocation(null, 1, 2, null));

        Assert.That(new[] { status }.MapToPositionVm(new Dictionary<string, DeviceTransporterVm> { ["ABX123"] = Device }), Is.Empty);
    }

    [Test]
    public void MapToPositionVm_StatusWithoutLocationTime_FallsBackToEventTime()
    {
        var status = new VehicleStatus(1, "ABX123", Time, null, null, null, null, null, null, null,
            new StatusLocation(null, 1, 2, null));

        var result = new[] { status }.MapToPositionVm(new Dictionary<string, DeviceTransporterVm> { ["ABX123"] = Device }).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.DeviceDateTime, Is.EqualTo(Time));
            Assert.That(result.Speed, Is.Zero);
            Assert.That(result.Attributes!.Value.Hourmeter, Is.Null);
        });
    }

    [Test]
    public void Deserialize_DocumentedEventPayload_NormalizesOffsetsToUtc()
    {
        const string json = """
            {"data":[{"event_id":123456,"registration":"ABX123","terminal_event_type_id":2,"event_description":"PERIODIC_EVENT",
            "latitude":1.320227,"longitude":103.889653,"bearing":100,"speed":60,"clock":100500,"odometer":1000,"altitude":152,
            "gps_fix_type":3,"temp1":0,"event_ts":"2022-01-28 16:48:03+02","received_ts":"2022-01-01 08:12:02+00:00","position_description":"X"}],
            "meta":{"from":1,"to":1,"current_page":1,"per_page":1000,"last_page":1,"total":1}}
            """;

        var page = json.Deserialize<Page<VehicleEvent>>()!;
        var vehicleEvent = page.Data!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(vehicleEvent.EventTs, Is.EqualTo(new DateTimeOffset(2022, 1, 28, 14, 48, 3, TimeSpan.Zero)));
            Assert.That(vehicleEvent.EventTs.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(vehicleEvent.ReceivedTs, Is.EqualTo(new DateTimeOffset(2022, 1, 1, 8, 12, 2, TimeSpan.Zero)));
            Assert.That(vehicleEvent.GpsFixType, Is.EqualTo(3));
            Assert.That(vehicleEvent.Clock, Is.EqualTo(100500));
            Assert.That(page.Meta!.Value.LastPage, Is.EqualTo(1));
        });
    }

    [Test]
    public void Deserialize_OffsetlessTimestamp_IsTreatedAsUtc()
    {
        const string json = """{"data":[{"vehicle_id":7,"registration":"R1","event_ts":"2022-01-01 08:12:02","location":{"updated":"2022-01-01 08:12:00","latitude":1,"longitude":2}}]}""";

        var status = json.Deserialize<Page<VehicleStatus>>()!.Data!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(status.EventTs, Is.EqualTo(new DateTimeOffset(2022, 1, 1, 8, 12, 2, TimeSpan.Zero)));
            Assert.That(status.Location!.Value.Updated, Is.EqualTo(new DateTimeOffset(2022, 1, 1, 8, 12, 0, TimeSpan.Zero)));
            Assert.That(status.VehicleId, Is.EqualTo(7));
        });
    }

    [Test]
    public void MapToDeviceVm_UsesRegistrationAsSerialAndVehicleIdAsIdentifier()
    {
        var transporterId = Guid.NewGuid();

        var named = new Vehicle(654321, "ABX123", "Big car").MapToDeviceVm(transporterId);
        var unnamed = new Vehicle(5_000_000_000, "ABX124", null).MapToDeviceVm();

        Assert.Multiple(() =>
        {
            Assert.That(named.DeviceId, Is.EqualTo(transporterId));
            Assert.That(named.Identifier, Is.EqualTo(654321));
            Assert.That(named.Serial, Is.EqualTo("ABX123"));
            Assert.That(named.Name, Is.EqualTo("Big car"));
            Assert.That(unnamed.Identifier, Is.Zero);
            Assert.That(unnamed.Name, Is.EqualTo("ABX124"));
            Assert.That(unnamed.DeviceId, Is.Null);
        });
    }
}
