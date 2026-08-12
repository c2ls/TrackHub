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
using TrackHub.Router.Application.Positions.Mappers;
using TrackHub.Router.Domain.Enumerators;

namespace Application.UnitTests;
[TestFixture]
public class TripMapperTests
{
    private readonly bool _ignitionBased = false;
    private readonly double _stoppedGap = 5;
    private readonly double _maxDistance = 10;
    private readonly TimeSpan _maxTimeGap = TimeSpan.FromMinutes(120);

    [Test]
    public void GroupPositionsIntoTrips_NoPositions_ReturnsEmptyList()
    {
        // Arrange
        var positions = new List<PositionVm>();

        // Act
        var result = positions.GroupPositionsIntoTrips(_ignitionBased, _stoppedGap, _maxDistance, _maxTimeGap);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GroupPositionsIntoTrips_SinglePosition_ReturnsSingleTrip()
    {
        // Arrange
        var positions = new List<PositionVm>
            {
                new() { DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 0, Longitude = 0, Speed = 0 }
            };

        // Act
        var result = positions.GroupPositionsIntoTrips(_ignitionBased, _stoppedGap, _maxDistance, _maxTimeGap);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Points, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void GroupPositionsIntoTrips_MultiplePositionsWithinThresholds_ReturnsSingleTrip()
    {
        // Arrange
        var positions = new List<PositionVm>
            {
                new() { DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 0, Longitude = 0, Speed = 0 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(1), Latitude = 0.001, Longitude = 0.001, Speed = 1 }
            };

        // Act
        var result = positions.GroupPositionsIntoTrips(_ignitionBased, _stoppedGap, _maxDistance, _maxTimeGap);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Points, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void GroupPositionsIntoTrips_MultiplePositionsExceedingThresholds_ReturnsMultipleTrips()
    {
        // Arrange
        var positions = new List<PositionVm>
            {
                new() { DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 0.001, Longitude = 0.001, Speed = 10 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(1), Latitude = 0.002, Longitude = 0.002, Speed = 9 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(2), Latitude = 0.011, Longitude = 0.011, Speed = 10 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(3), Latitude = 0.012, Longitude = 0.012, Speed = 9 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(4), Latitude = 0.023, Longitude = 0.023, Speed = 20 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(10), Latitude = 0.023, Longitude = 0.023, Speed = 0 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(11), Latitude = 0.023, Longitude = 0.023, Speed = 0 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(12), Latitude = 0.023, Longitude = 0.023, Speed = 0 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(13), Latitude = 0.024, Longitude = 0.024, Speed = 20 }
            };

        // Act
        var result = positions.GroupPositionsIntoTrips(_ignitionBased, _stoppedGap, _maxDistance, _maxTimeGap);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(3));
    }

    [Test]
    public void GroupPositionsIntoTrips_StopsAndStarts_ReturnsCorrectTrips()
    {
        // Arrange
        var positions = new List<PositionVm>
            {
                // first trip - moving
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(1), Latitude = 0.001, Longitude = 0.001, Speed = 10 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(2), Latitude = 0.002, Longitude = 0.002, Speed = 11 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(3), Latitude = 0.003, Longitude = 0.003, Speed = 12 },
                // second trip - stop
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(60), Latitude = 0.003, Longitude = 0.003, Speed = 0 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(61), Latitude = 0.003, Longitude = 0.003, Speed = 0 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(62), Latitude = 0.003, Longitude = 0.003, Speed = 0 },
                // third trip - moving
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(120), Latitude = 0.004, Longitude = 0.004, Speed = 10 },
                new() { DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(121), Latitude = 0.005, Longitude = 0.005, Speed = 11 },
            };

        // Act
        var result = positions.GroupPositionsIntoTrips(_ignitionBased, _stoppedGap, _maxDistance, _maxTimeGap);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.Count(x => x.Type == (short)TripTypeEnum.MOVING), Is.EqualTo(2));
        }
    }

    [Test]
    public void GroupPositionsIntoTrips_LargeDatasetWithMultipleGroups_ReturnsCorrectTrips()
    {
        // Arrange
        var positions = new List<PositionVm>();
        var startTime = DateTimeOffset.UtcNow;

        // First trip
        for (int i = 1; i < 100; i++)
        {
            positions.Add(new PositionVm
            {
                DeviceDateTime = startTime.AddMinutes(i),
                Latitude = i * 0.001,
                Longitude = i * 0.001,
                Speed = i % 10
            });
        }

        // Adding a gap to create a new trip
        startTime = startTime.AddHours(2);

        // Second trip
        for (int i = 1; i < 100; i++)
        {
            positions.Add(new PositionVm
            {
                DeviceDateTime = startTime.AddMinutes(i),
                Latitude = i * 0.001,
                Longitude = i * 0.001,
                Speed = i % 10
            });
        }

        // Adding another gap to create a third trip
        startTime = startTime.AddHours(2);

        // Third trip
        for (int i = 1; i < 100; i++)
        {
            positions.Add(new PositionVm
            {
                DeviceDateTime = startTime.AddMinutes(i),
                Latitude = i * 0.001,
                Longitude = i * 0.001,
                Speed = i % 10
            });
        }

        // Act
        var result = positions.GroupPositionsIntoTrips(_ignitionBased, _stoppedGap, _maxDistance, _maxTimeGap);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.Count(x => x.Type == (short)TripTypeEnum.MOVING), Is.EqualTo(3));
        }
    }
}
