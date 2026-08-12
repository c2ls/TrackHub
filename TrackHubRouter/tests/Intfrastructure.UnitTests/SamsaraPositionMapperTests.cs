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

using TrackHub.Router.Domain.Models;
using TrackHub.Router.Infrastructure.Samsara.Mappers;
using TrackHub.Router.Infrastructure.Samsara.Models;

namespace TrackHub.Router.Infrastructure.Samsara.Tests;

// Value-correctness test for a provider mapper (router-audit A-16): the conversions that feed the
// platform's reported speed/position must be asserted, not just that empty input yields empty.
[TestFixture]
public class PositionMapperTests
{
    [Test]
    public void MapToPositionVm_ConvertsMphToKmhAndMapsCoreFields()
    {
        var device = new DeviceTransporterVm
        {
            TransporterId = Guid.NewGuid(),
            Name = "Truck-1",
            TransporterType = "Truck",
        };
        var time = new DateTimeOffset(2026, 7, 17, 10, 0, 0, TimeSpan.Zero);
        var gps = new GpsData(time, 4.65, -74.05, 90.0, 100.0, false, null);

        var result = gps.MapToPositionVm(device);

        Assert.Multiple(() =>
        {
            Assert.That(result.TransporterId, Is.EqualTo(device.TransporterId));
            Assert.That(result.Latitude, Is.EqualTo(4.65));
            Assert.That(result.Longitude, Is.EqualTo(-74.05));
            Assert.That(result.DeviceDateTime, Is.EqualTo(time));
            Assert.That(result.Course, Is.EqualTo(90.0));
            // 100 mph → 160.934 km/h (Samsara reports mph; the platform stores km/h).
            Assert.That(result.Speed, Is.EqualTo(160.934).Within(0.0001));
        });
    }
}
