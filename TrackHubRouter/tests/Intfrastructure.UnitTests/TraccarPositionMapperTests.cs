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

using TrackHub.Router.Infrastructure.Traccar.Mappers;
using TrackHub.Router.Infrastructure.Traccar.Models;

namespace TrackHub.Router.Infrastructure.Traccar.Tests;

// Value-correctness tests for the Traccar mapper (TT-01).
[TestFixture]
public class PositionMapperTests
{
    private static DeviceTransporterVm Device { get; } = new()
    {
        TransporterId = Guid.Parse("2c5a9b41-7d3e-4f18-8a26-91b0c4d7e5f2"),
        Identifier = 17,
        Serial = "SER-17",
        Name = "Van-3",
        TransporterType = "Truck",
        TransporterTypeId = (short)TransporterType.Truck
    };

    private static readonly DateTimeOffset DeviceTime = new(2026, 7, 17, 8, 5, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ServerTime = new(2026, 7, 17, 8, 5, 9, TimeSpan.Zero);

    private static Position CreatePosition()
        => new(
            Id: 555,
            DeviceId: 17,
            Protocol: "teltonika",
            DeviceTime: DeviceTime,
            FixTime: DeviceTime,
            ServerTime: ServerTime,
            Outdated: false,
            Valid: true,
            Latitude: 4.710989,
            Longitude: -74.072092,
            Altitude: 2640.4,
            Speed: 55.6,
            Course: 275.0,
            Address: "Autopista Norte",
            Accuracy: 5.0,
            Attributes: new PositionAttribute(Ignition: true, TotalDistance: 900_000, Odometer: 154_320));

    [Test]
    public void MapToPositionVm_MapsCoreFieldsAndRoundsAltitudeAndSpeed()
    {
        var result = CreatePosition().MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result.DeviceName, Is.EqualTo("Van-3"));
            Assert.That(result.TransporterType, Is.EqualTo("Truck"));
            Assert.That(result.Latitude, Is.EqualTo(4.710989));
            Assert.That(result.Longitude, Is.EqualTo(-74.072092));
            // The mapper rounds altitude and speed to whole units.
            Assert.That(result.Altitude, Is.EqualTo(2640));
            Assert.That(result.DeviceDateTime, Is.EqualTo(DeviceTime));
            Assert.That(result.ServerDateTime, Is.EqualTo(ServerTime));
            // RA-04 (open, needs live verification): the mapper treats Traccar's `speed` as km/h and
            // only rounds it. Traccar's API documents knots. This pins the CURRENT behaviour - if the
            // conversion is corrected, this expectation must change to 55.6 kn -> 103 km/h.
            Assert.That(result.Speed, Is.EqualTo(56));
            Assert.That(result.Course, Is.EqualTo(275.0));
            Assert.That(result.EventId, Is.Null);
            Assert.That(result.Address, Is.EqualTo("Autopista Norte"));
            Assert.That(result.City, Is.Null);
            Assert.That(result.State, Is.Null);
            Assert.That(result.Country, Is.Null);
            Assert.That(result.Attributes!.Value.Ignition, Is.True);
            // Odometer wins over TotalDistance when both are present.
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(154_320));
        }
    }

    [Test]
    public void MapToPositionVm_WithoutOdometer_FallsBackToTotalDistance()
    {
        var position = CreatePosition() with
        {
            Attributes = new PositionAttribute(Ignition: false, TotalDistance: 900_000, Odometer: null)
        };

        var result = position.MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Attributes!.Value.Ignition, Is.False);
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(900_000));
        }
    }

    [Test]
    public void MapToPositionVm_WithDeviceDictionary_MatchesOnDeviceIdAndSkipsUnknownDevices()
    {
        var known = CreatePosition();
        var unknown = CreatePosition() with { DeviceId = 99 };
        var dictionary = new Dictionary<int, DeviceTransporterVm> { [17] = Device };

        var result = new[] { known, unknown }.MapToPositionVm(dictionary).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TransporterId, Is.EqualTo(Device.TransporterId));
            Assert.That(result[0].Longitude, Is.EqualTo(-74.072092));
        }
    }
}
