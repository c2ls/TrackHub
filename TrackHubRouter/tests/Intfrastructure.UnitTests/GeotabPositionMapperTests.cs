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

using Geotab.Checkmate.ObjectModel;
using TrackHub.Router.Infrastructure.Geotab.Mappers;

namespace TrackHub.Router.Infrastructure.Geotab.Tests;

// Value-correctness tests for the Geotab mapper (TT-01). The Geotab SDK hands back DateTime values
// with Kind=Unspecified; the mapper must stamp them UTC rather than let the local zone leak in.
[TestFixture]
public class PositionMapperTests
{
    private static DeviceTransporterVm Device { get; } = new()
    {
        TransporterId = Guid.Parse("d90a5c34-8b17-42fe-95c6-3a7e1b8d0426"),
        Identifier = 5150,
        Serial = "SER-5150",
        Name = "Rig-6",
        TransporterType = "Truck",
        TransporterTypeId = (short)TransporterType.Truck
    };

    private static readonly DateTime SdkDateTime =
        DateTime.SpecifyKind(new DateTime(2026, 7, 17, 9, 20, 0), DateTimeKind.Unspecified);

    private static readonly DateTimeOffset ExpectedTime = new(2026, 7, 17, 9, 20, 0, TimeSpan.Zero);

    [Test]
    public void MapToPositionVm_FromDeviceStatusInfo_MapsCoreFieldsAndBearing()
    {
        var status = new DeviceStatusInfo
        {
            Latitude = 4.710989,
            Longitude = -74.072092,
            Speed = 87.5f,
            Bearing = 210,
            DateTime = SdkDateTime
        };

        var result = status.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.DeviceName, Is.EqualTo("Rig-6"));
            Assert.That(result.TransporterType, Is.EqualTo("Truck"));
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.Speed, Is.EqualTo(87.5));
            Assert.That(result.Course, Is.EqualTo(210));
            Assert.That(result.DeviceDateTime, Is.EqualTo(ExpectedTime));
            // A Kind=Unspecified SDK timestamp must be read as UTC, not as local time.
            Assert.That(result.DeviceDateTime.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(result.ServerDateTime, Is.Null);
            Assert.That(result.Altitude, Is.Null);
            Assert.That(result.Attributes, Is.Null);
        }
    }

    [Test]
    public void MapToPositionVm_FromDeviceStatusInfoWithMissingValues_DefaultsToZero()
    {
        var status = new DeviceStatusInfo
        {
            Latitude = null,
            Longitude = null,
            Speed = null,
            Bearing = null,
            DateTime = null
        };

        var before = DateTimeOffset.UtcNow;
        var result = status.MapToPositionVm(Device);
        var after = DateTimeOffset.UtcNow;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Latitude, Is.Zero);
            Assert.That(result.Longitude, Is.Zero);
            Assert.That(result.Speed, Is.Zero);
            Assert.That(result.Course, Is.Null);
            // A missing SDK timestamp falls back to "now".
            Assert.That(result.DeviceDateTime, Is.InRange(before, after));
        }
    }

    [Test]
    public void MapToPositionVm_FromLogRecord_MapsCoreFields()
    {
        var record = new LogRecord
        {
            Latitude = 4.710989,
            Longitude = -74.072092,
            Speed = 87.5f,
            DateTime = SdkDateTime
        };

        var result = record.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            Assert.That(result.Speed, Is.EqualTo(87.5));
            Assert.That(result.DeviceDateTime, Is.EqualTo(ExpectedTime));
            Assert.That(result.DeviceDateTime.Offset, Is.EqualTo(TimeSpan.Zero));
            // LogRecord carries no heading, so the mapper reports a constant 0 course.
            Assert.That(result.Course, Is.Zero);
        }
    }

    [Test]
    public void MapToPositionVm_ForLogRecordCollection_MapsEveryRecord()
    {
        var records = new[]
        {
            new LogRecord { Latitude = 4.710989, Longitude = -74.072092, Speed = 10f, DateTime = SdkDateTime },
            new LogRecord { Latitude = 4.712000, Longitude = -74.073000, Speed = 20f, DateTime = SdkDateTime.AddMinutes(1) }
        };

        var result = records.MapToPositionVm(Device).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Speed, Is.EqualTo(10));
            Assert.That(result[1].Speed, Is.EqualTo(20));
            Assert.That(result[1].DeviceDateTime, Is.EqualTo(ExpectedTime.AddMinutes(1)));
        }
    }
}
