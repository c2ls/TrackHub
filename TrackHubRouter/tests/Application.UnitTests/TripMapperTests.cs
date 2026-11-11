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
    private const double StoppedGap = 5;
    private const double MaxDistance = 10;
    private static readonly TimeSpan MaxTimeGap = TimeSpan.FromMinutes(120);
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);

    private static PositionVm At(double minute, double latitude, double speed, bool? ignition = null)
        => new()
        {
            DeviceDateTime = Start.AddMinutes(minute),
            Latitude = latitude,
            Longitude = 0,
            Speed = speed,
            Attributes = ignition is null ? null : new AttributesVm(ignition, null, null, null, null)
        };

    private static List<TripVm> Group(IEnumerable<PositionVm> positions, bool accBased = false)
        => [.. positions.GroupPositionsIntoTrips(accBased, StoppedGap, MaxDistance, MaxTimeGap)];

    private static short[] Types(IEnumerable<TripVm> trips) => [.. trips.Select(t => t.Type)];

    private static void AssertContinuous(List<TripVm> trips)
    {
        for (var i = 1; i < trips.Count; i++)
        {
            Assert.That(trips[i].Points[0], Is.EqualTo(trips[i - 1].Points[^1]), $"segment {i} does not start where segment {i - 1} ends");
            Assert.That(trips[i].From, Is.EqualTo(trips[i - 1].To));
        }
    }

    private const short Stop = (short)TripTypeEnum.STOP;
    private const short Moving = (short)TripTypeEnum.MOVING;
    private const short NoData = (short)TripTypeEnum.NO_DATA;

    [Test]
    public void NoPositions_ReturnsEmpty()
        => Assert.That(Group([]), Is.Empty);

    [Test]
    public void SingleStandingPosition_ReturnsOneStop()
    {
        var trips = Group([At(0, 0, 0)]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Stop }));
            Assert.That(trips[0].Points, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void BriefHalt_StaysInsideTheTrip()
    {
        var trips = Group(
        [
            At(0, 0.000, 30),
            At(1, 0.005, 30),
            At(2, 0.010, 0),
            At(3, 0.010, 0),
            At(4, 0.015, 30),
            At(5, 0.020, 30)
        ]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving }));
            Assert.That(trips[0].Points, Has.Count.EqualTo(6));
        }
    }

    [Test]
    public void LongHalt_BecomesAStopSharingItsBoundaryPoints()
    {
        var trips = Group(
        [
            At(0, 0.000, 30),
            At(1, 0.005, 30),
            At(2, 0.010, 0),
            At(10, 0.010, 0),
            At(20, 0.010, 0),
            At(21, 0.015, 30),
            At(22, 0.020, 30)
        ]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving, Stop, Moving }));
            AssertContinuous(trips);
            Assert.That(trips[0].To, Is.EqualTo(Start.AddMinutes(2)));
            Assert.That(trips[1].Duration, Is.EqualTo(TimeSpan.FromMinutes(19)));
            Assert.That(trips[1].TotalDistance, Is.Zero);
            Assert.That(trips[2].From, Is.EqualTo(Start.AddMinutes(21)));
        }
    }

    [Test]
    public void HaltCutByTheRangeEdge_IsKeptAsAStop()
    {
        var trips = Group(
        [
            At(0, 0.000, 0),
            At(2, 0.000, 0),
            At(3, 0.005, 30),
            At(4, 0.010, 30),
            At(5, 0.015, 0),
            At(6, 0.015, 0)
        ]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Stop, Moving, Stop }));
            AssertContinuous(trips);
        }
    }

    [Test]
    public void DwellKeepsTheApproachAndDepartureInTheTrips()
    {
        var trips = Group(
        [
            At(0, 0.0000, 30, ignition: true),
            At(1, 0.0030, 30, ignition: true),
            At(2, 0.0038, 20, ignition: true),
            At(3, 0.0045, 5, ignition: true),
            At(10, 0.0045, 0, ignition: true),
            At(20, 0.0045, 2, ignition: true),
            At(21, 0.0052, 20, ignition: true),
            At(22, 0.0080, 30, ignition: true),
            At(23, 0.0110, 30, ignition: true)
        ], accBased: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving, Stop, Moving }));
            Assert.That(trips[1].From, Is.EqualTo(Start.AddMinutes(3)));
            Assert.That(trips[1].To, Is.EqualTo(Start.AddMinutes(21)));
            AssertContinuous(trips);
        }
    }

    [Test]
    public void StopIsBackdatedToItsFirstStandingPoint()
    {
        var trips = Group(
        [
            At(0, 0.000, 30),
            At(1, 0.005, 0),
            At(2, 0.005, 0),
            At(3, 0.005, 0),
            At(30, 0.005, 0),
            At(31, 0.010, 30),
            At(32, 0.015, 30)
        ]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving, Stop, Moving }));
            Assert.That(trips[1].From, Is.EqualTo(Start.AddMinutes(1)));
        }
    }

    [Test]
    public void SingleStandingReport_FollowedByLateDeparture_IsAStop()
    {
        var trips = Group(
        [
            At(0, 0.000, 30),
            At(1, 0.005, 0),
            At(41, 0.005, 30),
            At(42, 0.010, 30)
        ]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving, Stop, Moving }));
            Assert.That(trips[1].Duration, Is.EqualTo(TimeSpan.FromMinutes(40)));
            AssertContinuous(trips);
        }
    }

    [Test]
    public void AccBased_IgnitionOffWhileRolling_IsAStop()
    {
        var trips = Group(
        [
            At(0, 0.000, 30, ignition: true),
            At(1, 0.005, 30, ignition: true),
            At(2, 0.010, 5, ignition: false),
            At(20, 0.010, 0, ignition: false),
            At(21, 0.010, 0, ignition: true),
            At(22, 0.015, 30, ignition: true)
        ], accBased: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving, Stop, Moving }));
            Assert.That(trips[1].From, Is.EqualTo(Start.AddMinutes(2)));
            Assert.That(trips[1].To, Is.EqualTo(Start.AddMinutes(21)));
            AssertContinuous(trips);
        }
    }

    [Test]
    public void AccBased_EngineRunningOnTheSpot_IsAStopUntilItRelocates()
    {
        var trips = Group(
        [
            At(0, 0.00000, 2, ignition: true),
            At(30, 0.00010, 0, ignition: true),
            At(60, 0.00005, 3, ignition: true),
            At(61, 0.00200, 5, ignition: true),
            At(62, 0.00400, 5, ignition: true),
            At(90, 0.00405, 1, ignition: true),
            At(120, 0.00400, 0, ignition: true)
        ], accBased: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Stop, Moving, Stop }));
            Assert.That(trips[0].To, Is.EqualTo(Start.AddMinutes(61)));
            Assert.That(trips[1].To, Is.EqualTo(Start.AddMinutes(62)));
            AssertContinuous(trips);
        }
    }

    [Test]
    public void DriftWhileIdle_DoesNotCreateATrip()
    {
        var trips = Group(
        [
            At(0, 0.00000, 0),
            At(5, 0.00003, 0),
            At(10, 0.00020, 2),
            At(15, 0.00001, 0),
            At(20, 0.00002, 0)
        ]);

        Assert.That(Types(trips), Is.EqualTo(new[] { Stop }));
    }

    [Test]
    public void AccBased_ParkedSilence_IsOneStopNotAGap()
    {
        var trips = Group(
        [
            At(0, 0.000, 30, ignition: true),
            At(1, 0.005, 0, ignition: false),
            At(600, 0.005, 0, ignition: true),
            At(601, 0.010, 30, ignition: true)
        ], accBased: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving, Stop, Moving }));
            Assert.That(trips[1].Duration, Is.EqualTo(TimeSpan.FromMinutes(599)));
            AssertContinuous(trips);
        }
    }

    [Test]
    public void AccBased_WithoutIgnitionReports_FallsBackToSpeed()
    {
        var trips = Group(
        [
            At(0, 0.000, 30),
            At(1, 0.005, 30),
            At(2, 0.010, 0),
            At(30, 0.010, 0),
            At(31, 0.015, 30),
            At(32, 0.020, 30)
        ], accBased: true);

        Assert.That(Types(trips), Is.EqualTo(new[] { Moving, Stop, Moving }));
    }

    [Test]
    public void SilenceWhileMoving_IsReportedAsNoData()
    {
        var trips = Group(
        [
            At(0, 0.000, 30),
            At(1, 0.005, 30),
            At(200, 0.050, 30),
            At(201, 0.055, 30)
        ]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving, NoData, Moving }));
            Assert.That(trips[1].Duration, Is.EqualTo(TimeSpan.FromMinutes(199)));
            Assert.That(trips[1].TotalDistance, Is.Zero);
            AssertContinuous(trips);
        }
    }

    [Test]
    public void PositionJump_IsReportedAsNoData()
    {
        var trips = Group(
        [
            At(0, 0.000, 30),
            At(1, 0.005, 30),
            At(2, 1.000, 30),
            At(3, 1.005, 30)
        ]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving, NoData, Moving }));
            AssertContinuous(trips);
        }
    }

    [Test]
    public void TripEndingOnAStandingPoint_AddsNoEmptyStop()
    {
        var trips = Group(
        [
            At(0, 0.000, 30, ignition: true),
            At(1, 0.005, 30, ignition: true),
            At(2, 0.010, 0, ignition: false)
        ], accBased: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving }));
            Assert.That(trips[0].To, Is.EqualTo(Start.AddMinutes(2)));
        }
    }

    [Test]
    public void UnorderedAndDuplicatedPositions_AreNormalized()
    {
        var trips = Group(
        [
            At(2, 0.010, 30),
            At(0, 0.000, 30),
            At(1, 0.005, 30),
            At(1, 0.005, 30)
        ]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Types(trips), Is.EqualTo(new[] { Moving }));
            Assert.That(trips[0].Points, Has.Count.EqualTo(3));
            Assert.That(trips[0].TotalDistance, Is.EqualTo(1.11).Within(0.01));
        }
    }
}
