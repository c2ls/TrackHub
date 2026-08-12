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

using TrackHub.Router.Infrastructure.Navixy.Mappers;
using TrackHub.Router.Infrastructure.Navixy.Models;

namespace TrackHub.Router.Infrastructure.Navixy.Tests;

// Value-correctness tests for the Navixy mapper (TT-01). Navixy timestamps are naive strings that
// the mapper must read as UTC - a regression there shifts every position by the server offset.
[TestFixture]
public class PositionMapperTests
{
    private static DeviceTransporterVm Device { get; } = new()
    {
        TransporterId = Guid.Parse("4e8c1f60-b2d7-4903-8a5e-6c19d3f27b08"),
        Identifier = 7788,
        Serial = "SER-7788",
        Name = "Pickup-4",
        TransporterType = "Truck",
        TransporterTypeId = (short)TransporterType.Truck
    };

    private static readonly DateTimeOffset ExpectedTime = new(2026, 7, 17, 9, 20, 0, TimeSpan.Zero);

    [Test]
    public void MapToPositionVm_FromTracker_MapsLastUpdateAndParsesTimeAsUtc()
    {
        var tracker = new Tracker(
            Tracker_id: 7788,
            Imei: "868324028712345",
            Label: "Pickup-4",
            Last_update: new TrackerLastUpdate(
                Time: "2026-07-17 09:20:00",
                Lat: 4.710989,
                Lng: -74.072092,
                Speed: 63,
                Heading: 210));

        var result = tracker.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.DeviceName, Is.EqualTo("Pickup-4"));
            Assert.That(result.TransporterType, Is.EqualTo("Truck"));
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.DeviceDateTime, Is.EqualTo(ExpectedTime));
            Assert.That(result.DeviceDateTime.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(result.ServerDateTime, Is.Null);
            Assert.That(result.Speed, Is.EqualTo(63));
            Assert.That(result.Course, Is.EqualTo(210));
            // last_update carries no altitude or address.
            Assert.That(result.Altitude, Is.Null);
            Assert.That(result.Address, Is.Null);
        }
    }

    [Test]
    public void MapToPositionVm_FromTrackerWithoutLastUpdate_YieldsZeroCoordinatesAndMinDate()
    {
        var tracker = new Tracker(7788, "868324028712345", "Pickup-4", null);

        var result = tracker.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Latitude, Is.Zero);
            Assert.That(result.Longitude, Is.Zero);
            Assert.That(result.Speed, Is.Zero);
            Assert.That(result.Course, Is.Null);
            Assert.That(result.DeviceDateTime, Is.EqualTo(DateTimeOffset.MinValue));
        }
    }

    [Test]
    public void MapToPositionVm_FromTrackPoint_MapsAltitudeAndAddress()
    {
        var point = new TrackPoint(
            Lat: 4.710989,
            Lng: -74.072092,
            Alt: 2640,
            Get_time: "2026-07-17 09:20:00",
            Speed: 63,
            Heading: 210,
            Address: "Calle 100 #15-20",
            Precision: 5,
            Gsm_lbs: false,
            Parking: false);

        var result = point.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.Altitude, Is.EqualTo(2640));
            Assert.That(result.DeviceDateTime, Is.EqualTo(ExpectedTime));
            Assert.That(result.Speed, Is.EqualTo(63));
            Assert.That(result.Course, Is.EqualTo(210));
            Assert.That(result.Address, Is.EqualTo("Calle 100 #15-20"));
        }
    }

    [Test]
    public void MapToPositionVm_WithUnparseableTime_FallsBackToMinValue()
    {
        var point = new TrackPoint(4.710989, -74.072092, 2640, "17/07/2026 09:20", 63, 210, null, null, null, null);

        var result = point.MapToPositionVm(Device);

        Assert.That(result.DeviceDateTime, Is.EqualTo(DateTimeOffset.MinValue));
    }

    [Test]
    public void MapToPositionVm_ForTrackerCollection_SkipsTrackersWithoutUpdateOrDeviceMatch()
    {
        var lastUpdate = new TrackerLastUpdate("2026-07-17 09:20:00", 4.710989, -74.072092, 63, 210);
        var matched = new Tracker(7788, "868324028712345", "Pickup-4", lastUpdate);
        var noUpdate = new Tracker(7788, "868324028712345", "Pickup-4", null);
        var unknownDevice = new Tracker(9999, "000000000000000", "Ghost", lastUpdate);
        var dictionary = new Dictionary<int, DeviceTransporterVm> { [7788] = Device };

        var result = new[] { matched, noUpdate, unknownDevice }.MapToPositionVm(dictionary).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result[0].Latitude, Is.EqualTo(4.710989));
        }
    }
}
