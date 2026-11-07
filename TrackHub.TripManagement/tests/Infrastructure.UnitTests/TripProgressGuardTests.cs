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

using Common.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TrackHub.TripManagement.Domain.Constants;
using TrackHub.TripManagement.Domain.Records;
using TrackHub.TripManagement.Infrastructure.TripDB.Entities;
using TrackHub.TripManagement.Infrastructure.TripDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class TripProgressGuardTests
{
    private static readonly Guid TripId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly Guid StopId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Guid DeliveryId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003");

    private static async Task<WriterTestContext> SeededAsync(string stopStatus, bool requiresPod, string tripStatus = TripStatuses.InProgress, string deliveryStatus = DeliveryStatuses.Pending)
    {
        var context = WriterTestContext.Create();
        var trip = WriterTestData.Trip(TripId, "TRIP-G");
        trip.Status = tripStatus;
        trip.ActualStartAt = DateTimeOffset.UtcNow.AddHours(-2);
        context.Trips.Add(trip);
        var stop = WriterTestData.Stop(StopId, TripId, stopStatus);
        stop.RequiresPod = requiresPod;
        stop.ActualArrivalAt = stopStatus == TripStopStatuses.Pending ? null : DateTimeOffset.UtcNow.AddHours(-1);
        context.TripStops.Add(stop);
        context.Deliveries.Add(new Delivery
        {
            DeliveryId = DeliveryId,
            AccountId = WriterTestData.AccountId,
            TripStopId = StopId,
            ClientName = "Acme",
            Status = deliveryStatus,
        });
        await context.SaveChangesAsync(CancellationToken.None);
        context.ChangeTracker.Clear();
        return context;
    }

    private static Task<bool> DepartAsync(WriterTestContext context, DateTimeOffset occurredAt, string source = TripEventSources.Staff)
        => new TripStopWriter(context).RecordStopProgressAsync(
            TripId, StopId, WriterTestData.AccountId, TripStopStatuses.Departed, occurredAt,
            null, null, source, $"trip-depart:{Guid.NewGuid():N}", null, CancellationToken.None);

    [Test]
    public async Task ManualDeparture_OfAStopThatRequiresPod_IsRefusedUntilThePodExists()
    {
        using var context = await SeededAsync(TripStopStatuses.Arrived, requiresPod: true);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => DepartAsync(context, DateTimeOffset.UtcNow));

        Assert.That(ex!.Code, Is.EqualTo(TripErrorCodes.PodRequired));
    }

    [Test]
    public async Task DetectedDeparture_OfAStopThatRequiresPod_IsRecorded()
    {
        using var context = await SeededAsync(TripStopStatuses.Arrived, requiresPod: true);

        Assert.That(await DepartAsync(context, DateTimeOffset.UtcNow, TripEventSources.Detection), Is.True);
    }

    [Test]
    public async Task ManualDeparture_BeforeTheStopsArrival_IsRefused()
    {
        using var context = await SeededAsync(TripStopStatuses.Arrived, requiresPod: false);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => DepartAsync(context, DateTimeOffset.UtcNow.AddHours(-1.5)));

        Assert.That(ex!.Code, Is.EqualTo(TripErrorCodes.EventTimeOutOfRange));
    }

    [Test]
    public async Task ManualDeparture_InTheFuture_IsRefused()
    {
        using var context = await SeededAsync(TripStopStatuses.Arrived, requiresPod: false);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => DepartAsync(context, DateTimeOffset.UtcNow.AddHours(1)));

        Assert.That(ex!.Code, Is.EqualTo(TripErrorCodes.EventTimeOutOfRange));
    }

    [Test]
    public async Task Completion_WithADepartedStopMissingItsPod_IsRefusedUnlessADispatcherForcesIt()
    {
        using var context = await SeededAsync(TripStopStatuses.Departed, requiresPod: true);
        var writer = new TripWriter(context, Mock.Of<Common.Application.Interfaces.IUser>());

        var ex = await Assert.ThrowsAsync<ConflictException>(() => writer.TransitionTripAsync(
            TripId, WriterTestData.AccountId, TripStatuses.Completed, TripEventTypes.TripCompleted,
            TripEventSources.Detection, "trip-complete:auto", null, null, false, DateTimeOffset.UtcNow, CancellationToken.None));
        var forced = await writer.TransitionTripAsync(
            TripId, WriterTestData.AccountId, TripStatuses.Completed, TripEventTypes.TripCompleted,
            TripEventSources.Portal, "trip-complete:forced", null, null, true, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Code, Is.EqualTo(TripErrorCodes.PodRequired));
            Assert.That(forced, Is.True);
        });
    }

    [Test]
    public async Task DeliveryEdits_OnATerminalTrip_AreRefused()
    {
        using var context = await SeededAsync(TripStopStatuses.Departed, requiresPod: false, tripStatus: TripStatuses.Completed);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => new DeliveryWriter(context).UpdateDeliveryAsync(
            DeliveryId, WriterTestData.AccountId, new DeliveryDto("R", "Acme", null, null, null, 0), CancellationToken.None));

        Assert.That(ex!.Code, Is.EqualTo(TripErrorCodes.TripAlreadyTerminal));
    }

    [Test]
    public async Task ADeliveryWithAnOutcome_CannotBeEditedOrDeleted()
    {
        using var context = await SeededAsync(TripStopStatuses.Arrived, requiresPod: false, deliveryStatus: DeliveryStatuses.Delivered);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => new DeliveryWriter(context).DeleteDeliveryAsync(
            DeliveryId, WriterTestData.AccountId, CancellationToken.None));

        var remaining = await context.Deliveries.AsNoTracking().CountAsync();
        Assert.Multiple(() =>
        {
            Assert.That(ex!.Code, Is.EqualTo(TripErrorCodes.DeliveryOutcomeRecorded));
            Assert.That(remaining, Is.EqualTo(1));
        });
    }
}
