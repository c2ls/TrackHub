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

using Microsoft.EntityFrameworkCore;
using TrackHub.TripManagement.Domain.Constants;
using TrackHub.TripManagement.Domain.Records;
using TrackHub.TripManagement.Infrastructure.TripDB.Entities;
using TrackHub.TripManagement.Infrastructure.TripDB.Writers;

namespace Infrastructure.UnitTests;

/// <summary>
/// Exercises the writers' duplicate-handling branches for REAL, against a context that answers
/// with a genuine PostgreSQL 23505 <see cref="DbUpdateException"/>.
/// <para>
/// The Application suite stubs these paths by having a mocked writer return <c>false</c>, which
/// proves the caller honours a duplicate but proves nothing about the writer. That gap hid two
/// defects: catching the violation WITHOUT detaching the failed <c>Added</c> entries leaves them in
/// a request-scoped context's change tracker, so the next <c>SaveChangesAsync</c> replays the dead
/// insert — a retried POD returned 500 instead of the idempotent success spec 10's offline outbox
/// depends on (acceptance 15), and a genuine event later in the same request was silently lost.
/// Only a test that keeps saving on the SAME context after the violation can see either.
/// </para>
/// </summary>
[TestFixture]
public class WriterDuplicateHandlingTests
{
    private static readonly Guid TripId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid StopId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid SecondStopId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly DateTimeOffset OccurredAt = new(2026, 7, 21, 13, 0, 0, TimeSpan.Zero);

    private static async Task<WriterTestContext> SeededAsync(params TrackHub.TripManagement.Infrastructure.TripDB.Entities.TripStop[] stops)
    {
        var context = WriterTestContext.Create();
        context.Trips.Add(WriterTestData.Trip(TripId, "TRIP-001"));
        foreach (var stop in stops)
        {
            context.TripStops.Add(stop);
        }
        await context.SaveChangesAsync(CancellationToken.None);
        return context;
    }

    [Test]
    public async Task StopProgress_OnADuplicate_ReturnsFalseWithoutThrowing()
    {
        using var context = await SeededAsync(WriterTestData.Stop(StopId, TripId, TripStopStatuses.Pending));
        var writer = new TripStopWriter(context);
        context.FailNextSave();

        var recorded = await writer.RecordStopProgressAsync(
            TripId, StopId, WriterTestData.AccountId, TripStopStatuses.Arrived, OccurredAt,
            4.7, -74.0, TripEventSources.Detection, "trip-arrive:dup", null, CancellationToken.None);

        Assert.That(recorded, Is.False, "a duplicate must be reported as 'already recorded', not thrown");
    }

    /// <summary>
    /// A replayed event must short-circuit BEFORE the stop-transition guard.
    /// <para>
    /// Found by a smoke test, not by this suite: every duplicate test here replayed against a stop
    /// that had not moved on, so the guard was never reached. The real sequence is spec 10's
    /// offline outbox — a driver captures arrival and departure with no signal, both eventually
    /// sync, and the queued arrival is retried once the stop is already <c>Departed</c>. With the
    /// guard running first that retry threw <c>STOP_ALREADY_DEPARTED</c> permanently, so the device
    /// could only ever surface it as a hard failure. Acceptance 15 says a duplicate is a success.
    /// </para>
    /// </summary>
    [Test]
    public async Task StopProgress_ReplayedAfterTheStopHasDeparted_IsADuplicateNotATransitionError()
    {
        using var context = await SeededAsync(WriterTestData.Stop(StopId, TripId, TripStopStatuses.Departed));
        var writer = new TripStopWriter(context);

        // The arrival this stop already recorded, re-sent after the departure landed.
        context.TripEvents.Add(new TripEvent
        {
            AccountId = WriterTestData.AccountId,
            TripId = TripId,
            TripStopId = StopId,
            EventType = TripEventTypes.TripStopArrived,
            OccurredAt = OccurredAt,
            Source = TripEventSources.Driver,
            IdempotencyKey = "trip-arrive:replayed",
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var replayed = await writer.RecordStopProgressAsync(
            TripId, StopId, WriterTestData.AccountId, TripStopStatuses.Arrived, OccurredAt,
            4.7, -74.0, TripEventSources.Driver, "trip-arrive:replayed", null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(replayed, Is.False, "the replay is a duplicate — it must not throw STOP_ALREADY_DEPARTED");
            Assert.That(context.TripStops.Single().Status, Is.EqualTo(TripStopStatuses.Departed),
                "a replayed arrival must not drag a departed stop backwards");
        });

        var events = await context.TripEvents.CountAsync(CancellationToken.None);
        Assert.That(events, Is.EqualTo(1), "no second event row");
    }

    // The regression this fixture exists for. After a duplicate, the SAME scoped context must still
    // be usable: the failed insert has to have left the change tracker.
    [Test]
    public async Task StopProgress_AfterADuplicate_StillRecordsAGenuineLaterEventOnTheSameContext()
    {
        using var context = await SeededAsync(
            WriterTestData.Stop(StopId, TripId, TripStopStatuses.Pending),
            WriterTestData.Stop(SecondStopId, TripId, TripStopStatuses.Pending, sequence: 2));
        var writer = new TripStopWriter(context);

        context.FailNextSave();
        var duplicate = await writer.RecordStopProgressAsync(
            TripId, StopId, WriterTestData.AccountId, TripStopStatuses.Arrived, OccurredAt,
            4.7, -74.0, TripEventSources.Detection, "trip-arrive:dup", null, CancellationToken.None);

        // A real second arrival, on a different stop, in the same request.
        var genuine = await writer.RecordStopProgressAsync(
            TripId, SecondStopId, WriterTestData.AccountId, TripStopStatuses.Arrived, OccurredAt.AddMinutes(1),
            4.71, -74.01, TripEventSources.Detection, "trip-arrive:genuine", null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(duplicate, Is.False);
            Assert.That(genuine, Is.True, "the genuine later event was dropped — the failed insert was never detached");
        });

        var events = await context.TripEvents.CountAsync(CancellationToken.None);
        Assert.That(events, Is.EqualTo(1), "exactly the genuine event should be persisted");
    }

    [Test]
    public async Task StopProgress_ADuplicateLeavesNoTrackedMutationsBehind()
    {
        using var context = await SeededAsync(WriterTestData.Stop(StopId, TripId, TripStopStatuses.Pending));
        var writer = new TripStopWriter(context);
        context.FailNextSave();

        await writer.RecordStopProgressAsync(
            TripId, StopId, WriterTestData.AccountId, TripStopStatuses.Arrived, OccurredAt,
            4.7, -74.0, TripEventSources.Detection, "trip-arrive:dup", null, CancellationToken.None);

        // The duplicate's status/timestamp mutations must not leak into a later successful save.
        await context.SaveChangesAsync(CancellationToken.None);

        var stop = await context.TripStops.AsNoTracking()
            .FirstAsync(s => s.TripStopId == StopId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(stop.Status, Is.EqualTo(TripStopStatuses.Pending));
            Assert.That(stop.ActualArrivalAt, Is.Null);
        });
    }

    [Test]
    public async Task Pod_OnADuplicate_ReturnsTheExistingRecordInsteadOfThrowing()
    {
        using var context = await SeededAsync(WriterTestData.Stop(StopId, TripId, TripStopStatuses.Arrived));
        var writer = new ProofOfDeliveryWriter(context);
        var clientEventId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        var first = await writer.RecordAsync(
            WriterTestData.AccountId, TripId,
            new ProofOfDeliveryDto(StopId, null, "Receiver", null, null, OccurredAt, 4.7, -74.0, [], clientEventId),
            CancellationToken.None);

        // The retry: the unique (TripStopId, ClientEventId) index rejects the second insert.
        context.FailNextSave();
        var retry = await writer.RecordAsync(
            WriterTestData.AccountId, TripId,
            new ProofOfDeliveryDto(StopId, null, "Receiver", null, null, OccurredAt, 4.7, -74.0, [], clientEventId),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(retry.ProofOfDelivery.ProofOfDeliveryId, Is.EqualTo(first.ProofOfDelivery.ProofOfDeliveryId),
                "the retry must resolve to the SAME POD, not a second one");
            Assert.That(retry.ProofOfDelivery.ReceiverName, Is.EqualTo("Receiver"));
            Assert.That(retry.Created, Is.False,
                "the retry must report that it did NOT insert, so the handler skips the side effects");
            Assert.That(first.Created, Is.True);
        });

        var pods = await context.ProofsOfDelivery.CountAsync(CancellationToken.None);
        Assert.That(pods, Is.EqualTo(1), "a retried offline submission must produce exactly one row");
    }

    // Acceptance 15 verbatim: "a duplicate submission returns success and produces exactly one row".
    // Before the detach fix this threw DbUpdateException out to the caller as a 500, and spec 10's
    // outbox would have retried it forever.
    [Test]
    public async Task Pod_AfterADuplicate_TheSameContextIsStillUsable()
    {
        using var context = await SeededAsync(WriterTestData.Stop(StopId, TripId, TripStopStatuses.Arrived));
        var writer = new ProofOfDeliveryWriter(context);
        var clientEventId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        await writer.RecordAsync(
            WriterTestData.AccountId, TripId,
            new ProofOfDeliveryDto(StopId, null, "Receiver", null, null, OccurredAt, 4.7, -74.0, [], clientEventId),
            CancellationToken.None);

        context.FailNextSave();
        await writer.RecordAsync(
            WriterTestData.AccountId, TripId,
            new ProofOfDeliveryDto(StopId, null, "Receiver", null, null, OccurredAt, 4.7, -74.0, [], clientEventId),
            CancellationToken.None);

        // A stop-progress event raised later in the same request must still land.
        var stopWriter = new TripStopWriter(context);
        var recorded = await stopWriter.RecordStopProgressAsync(
            TripId, StopId, WriterTestData.AccountId, TripStopStatuses.Departed, OccurredAt.AddMinutes(5),
            4.7, -74.0, TripEventSources.Detection, "trip-depart:after-pod", null, CancellationToken.None);

        Assert.That(recorded, Is.True, "the context was poisoned by the duplicate POD insert");
    }
}
