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

using TrackHub.TripManagement.Application.Common;
using TrackHub.TripManagement.Application.Trips.Services;
using TrackHub.TripManagement.Domain.Exceptions;

namespace TrackHub.TripManagement.Application.UnitTests;

/// <summary>
/// Acceptance 17's second half: ETA carries <c>EtaSource = Ors</c> only while positions are fresh,
/// and falls back to <c>Planned</c>/<c>Unavailable</c> otherwise.
/// <para>
/// The gap these tests close is the dishonest one (spec 11 §18.11): before the fix, a trip whose
/// tracker went dark was filtered out of the candidate set entirely, so its last ORS-derived
/// <c>EtaAt</c> and <c>EtaSource = Ors</c> simply stayed on the stop. The UI had no way to know it
/// was showing a two-hour-old guess as a live estimate. A stale candidate must therefore be
/// asserted to MOVE to a fallback source, not merely to be left alone.
/// </para>
/// </summary>
[TestFixture]
public class TripEtaServiceTests
{
    private static readonly Guid StopId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    [Test]
    public async Task FreshPosition_WritesAnOrsSourcedEta()
    {
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(fresh: true));

        var refreshed = await harness.Service().RefreshEtasAsync(CancellationToken.None);

        Assert.That(refreshed, Is.EqualTo(1));
        harness.StopWriter.Verify(w => w.UpdateStopEtaAsync(
            StopId, TestFactory.AccountId, It.IsAny<DateTimeOffset?>(), EtaSources.Ors, It.IsAny<CancellationToken>()), Times.Once);
    }

    // The acceptance-17 gap. A stale position must never leave an Ors-labelled ETA standing.
    [Test]
    public async Task StalePosition_DowngradesTheEtaToThePlannedSchedule()
    {
        var plannedTo = DateTimeOffset.UtcNow.AddHours(3);
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(
            fresh: false,
            plannedArrivalTo: plannedTo,
            currentEtaAt: DateTimeOffset.UtcNow.AddMinutes(-100),
            currentEtaSource: EtaSources.Ors));

        var refreshed = await harness.Service().RefreshEtasAsync(CancellationToken.None);

        Assert.That(refreshed, Is.EqualTo(1));
        harness.StopWriter.Verify(w => w.UpdateStopEtaAsync(
            StopId, TestFactory.AccountId, plannedTo, EtaSources.Planned, It.IsAny<CancellationToken>()), Times.Once);

        // The stale branch must not spend provider quota on a position it does not trust.
        harness.RoutingProvider.Verify(p => p.GetSummaryAsync(
            It.IsAny<CoordinateVm>(), It.IsAny<CoordinateVm>(), It.IsAny<CancellationToken>()), Times.Never);
        harness.StopWriter.Verify(w => w.UpdateStopEtaAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset?>(), EtaSources.Ors, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task StalePositionWithNoPlannedWindow_MarksTheEtaUnavailable()
    {
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(
            fresh: false,
            plannedArrivalTo: null,
            currentEtaAt: DateTimeOffset.UtcNow.AddMinutes(-100),
            currentEtaSource: EtaSources.Ors));

        var refreshed = await harness.Service().RefreshEtasAsync(CancellationToken.None);

        Assert.That(refreshed, Is.EqualTo(1));
        harness.StopWriter.Verify(w => w.UpdateStopEtaAsync(
            StopId, TestFactory.AccountId, null, EtaSources.Unavailable, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task TripThatNeverReportedAPosition_FallsBackRatherThanBeingSkipped()
    {
        var plannedTo = DateTimeOffset.UtcNow.AddHours(2);
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(fresh: false, plannedArrivalTo: plannedTo) with
        {
            LastLatitude = null,
            LastLongitude = null,
            LastPositionAt = null,
        });

        var refreshed = await harness.Service().RefreshEtasAsync(CancellationToken.None);

        Assert.That(refreshed, Is.EqualTo(1));
        harness.StopWriter.Verify(w => w.UpdateStopEtaAsync(
            StopId, TestFactory.AccountId, plannedTo, EtaSources.Planned, It.IsAny<CancellationToken>()), Times.Once);
    }

    // SVD-11: a stale trip stays stale for hours. Once downgraded, every later cycle must be a
    // genuine no-op, or the on-work-only recorder writes a BackgroundJobRun row every 5 minutes.
    [Test]
    public async Task AlreadyDowngradedStop_IsANoOpAndCountsNoWork()
    {
        var plannedTo = DateTimeOffset.UtcNow.AddHours(3);
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(
            fresh: false,
            plannedArrivalTo: plannedTo,
            currentEtaAt: plannedTo,
            currentEtaSource: EtaSources.Planned,
            delayAlertedAt: DateTimeOffset.UtcNow));

        var refreshed = await harness.Service().RefreshEtasAsync(CancellationToken.None);

        Assert.That(refreshed, Is.EqualTo(0));
        harness.StopWriter.Verify(w => w.UpdateStopEtaAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RoutingOutageOnAFreshPosition_StillFallsBackToPlanned()
    {
        var plannedTo = DateTimeOffset.UtcNow.AddHours(3);
        var harness = new EtaHarness();
        harness.RoutingProvider
            .Setup(p => p.GetSummaryAsync(It.IsAny<CoordinateVm>(), It.IsAny<CoordinateVm>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RoutingUnavailableException(TripErrorCodes.RoutingUnavailable, "502 from ORS"));
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: plannedTo));

        var refreshed = await harness.Service().RefreshEtasAsync(CancellationToken.None);

        Assert.That(refreshed, Is.EqualTo(1));
        harness.StopWriter.Verify(w => w.UpdateStopEtaAsync(
            StopId, TestFactory.AccountId, plannedTo, EtaSources.Planned, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Defect 5: the travel time is spent starting NOW, not starting when the last fix arrived.
    // Anchoring to a 14-minute-old position made every ETA up to 14 minutes optimistic.
    [Test]
    public async Task Eta_IsAnchoredToNowNotToThePositionTimestamp()
    {
        var positionAt = DateTimeOffset.UtcNow.AddMinutes(-14);
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(fresh: true) with { LastPositionAt = positionAt });

        DateTimeOffset? written = null;
        harness.StopWriter
            .Setup(w => w.UpdateStopEtaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid, DateTimeOffset?, string, CancellationToken>((_, _, eta, _, _) => written = eta)
            .Returns(Task.CompletedTask);

        var before = DateTimeOffset.UtcNow;
        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        // 600s of travel from now, NOT from a 14-minute-old fix.
        Assert.That(written, Is.Not.Null);
        Assert.That(written!.Value, Is.GreaterThanOrEqualTo(before.AddSeconds(600)));
        Assert.That(written!.Value, Is.GreaterThan(positionAt.AddSeconds(600).AddMinutes(10)));
    }

    // Defect 4: without a lower bound, the first cycle after a deployment reminds about every
    // historical trip that was created and never started.
    [Test]
    public async Task ScheduleReminder_QueriesABoundedWindowAroundNow()
    {
        var harness = new EtaHarness();
        DateTimeOffset dueAfter = default;
        DateTimeOffset dueBefore = default;
        harness.DetectionReader
            .Setup(r => r.GetTripsDueToStartAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, DateTimeOffset, DateTimeOffset, CancellationToken>((_, after, before, _) => (dueAfter, dueBefore) = (after, before))
            .ReturnsAsync([]);

        await harness.Service().RaiseStartRemindersAsync(CancellationToken.None);

        var now = DateTimeOffset.UtcNow;
        Assert.That(dueAfter, Is.GreaterThan(now.AddHours(-2)), "the window must not reach back into history");
        Assert.That(dueAfter, Is.LessThan(now), "a start that slipped between cycles must still be reminded");
        Assert.That(dueBefore, Is.GreaterThan(now));
        Assert.That(dueBefore - dueAfter, Is.LessThan(TimeSpan.FromHours(3)));
    }

    // ----- TripDelayed: once per stop, stamped only after a successful emission -----------------

    [Test]
    public async Task Delay_IsRaisedAndStampedWhenTheEtaPassesTheThreshold()
    {
        // The plan said arrive an hour ago and the live ETA is ten minutes out: that is a delay
        // well past the account's threshold.
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: DateTimeOffset.UtcNow.AddHours(-1)));

        var refreshed = await harness.Service().RefreshEtasAsync(CancellationToken.None);

        Assert.That(refreshed, Is.EqualTo(1));
        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripDelayed, TripAlertSeverities.Warning, $"trip-delayed:{StopId:N}",
            It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Once);
        harness.StopWriter.Verify(w => w.MarkStopDelayAlertedAsync(
            StopId, TestFactory.AccountId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delay_IsNotRaisedASecondTimeOnceTheStopIsStamped()
    {
        // DelayAlertedAt is the once-per-stop marker. Without it the job re-alerts every 5 minutes
        // for as long as the vehicle stays late — an alert storm on exactly the trips an operator
        // is already watching, which trains them to ignore the channel.
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(
            fresh: true,
            plannedArrivalTo: DateTimeOffset.UtcNow.AddHours(-1),
            delayAlertedAt: DateTimeOffset.UtcNow.AddMinutes(-20)));

        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripDelayed, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
        harness.StopWriter.Verify(w => w.MarkStopDelayAlertedAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Delay_WhenTheEmitterFails_IsNotStampedSoTheNextCycleRetries()
    {
        // The geofence-dwell precedent, and the counterpart of the start-reminder ordering below:
        // stamping before the emission is known to have succeeded burns the one-shot marker and
        // the account is told about the delay ZERO times instead of once.
        var harness = new EtaHarness();
        harness.AlertEmitter
            .Setup(e => e.EmitAsync(TripEventTypes.TripDelayed, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Manager is down"));
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: DateTimeOffset.UtcNow.AddHours(-1)));

        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        harness.StopWriter.Verify(w => w.MarkStopDelayAlertedAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Delay_IsNotRaisedForAnEtaInsideTheThreshold()
    {
        // Arriving a minute or two after the planned window is not a delay worth an alert; the
        // account's DelayThresholdMinutes is what separates the two.
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: DateTimeOffset.UtcNow.AddHours(4)));

        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripDelayed, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ----- DelayThresholdMinutes: both sides of the boundary -------------------------------------
    //
    // The harness mocks a 600s ORS duration, so the computed ETA is always "now + 10 minutes".
    // With a threshold of T minutes the alert fires exactly when planned + T < now + 10, which lets
    // each case sit one minute either side of the line instead of hours away from it. Far-from-the-
    // boundary cases pass with the comparison deleted entirely; these do not.

    [Test]
    public async Task Delay_IsNotRaisedOneMinuteInsideTheDefaultThreshold()
    {
        // ETA = now + 10m, threshold = (now - 4m) + 15m = now + 11m. Inside by one minute.
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: DateTimeOffset.UtcNow.AddMinutes(-4)));

        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripDelayed, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Delay_IsRaisedOneMinuteOutsideTheDefaultThreshold()
    {
        // ETA = now + 10m, threshold = (now - 6m) + 15m = now + 9m. Outside by one minute.
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: DateTimeOffset.UtcNow.AddMinutes(-6)));

        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripDelayed, TripAlertSeverities.Warning, It.IsAny<string>(),
            It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Once);
        harness.StopWriter.Verify(w => w.MarkStopDelayAlertedAsync(
            StopId, TestFactory.AccountId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delay_UsesTheConfiguredThresholdRatherThanTheDefault_WhenItIsWider()
    {
        // An hour late against a two-hour tolerance is not an alert. Under the DEFAULT 15 minutes
        // this same candidate WOULD alert, so a service that ignores config.DelayThresholdMinutes
        // fails here.
        var harness = new EtaHarness();
        harness.WithConfig(TripAccountConfigVm.Default with { DelayThresholdMinutes = 120 });
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: DateTimeOffset.UtcNow.AddMinutes(-60)));

        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripDelayed, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Delay_UsesTheConfiguredThresholdRatherThanTheDefault_WhenItIsTighter()
    {
        // ETA = now + 10m, threshold = (now - 3m) + 2m = now - 1m. Alert. Under the DEFAULT 15
        // minutes the threshold would be now + 12m and nothing would fire, so this pins the
        // configured value being read in the other direction too.
        var harness = new EtaHarness();
        harness.WithConfig(TripAccountConfigVm.Default with { DelayThresholdMinutes = 2 });
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: DateTimeOffset.UtcNow.AddMinutes(-3)));

        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripDelayed, TripAlertSeverities.Warning, It.IsAny<string>(),
            It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delay_ReportsTheLatenessAgainstThePlannedWindow_NotAgainstTheThreshold()
    {
        // DelayMinutes is (eta - plannedTo): the threshold decides WHETHER to alert, never the
        // number the account is shown. ETA = now + 10m against a window that closed 30m ago.
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: DateTimeOffset.UtcNow.AddMinutes(-30)));

        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripDelayed, It.IsAny<string>(), It.IsAny<string>(),
            It.Is<TripAlertDto>(a => a.DelayMinutes == 40), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delay_IsNotRaisedWithoutAPlannedWindowToBeLateAgainst()
    {
        // "Late" is meaningless with nothing to be late for. Treating a null planned window as
        // zero would make every unscheduled trip permanently delayed.
        var harness = new EtaHarness();
        harness.WithCandidate(Candidate(fresh: true, plannedArrivalTo: null));

        await harness.Service().RefreshEtasAsync(CancellationToken.None);

        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripDelayed, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ----- TripStartDue: check the key, emit, THEN append ---------------------------------------

    [Test]
    public async Task StartReminder_IsRaisedOnceAndRecordedInTheEventLog()
    {
        var harness = new EtaHarness();
        harness.WithTripDueToStart(TestFactory.Trip(TripStatuses.Created));

        var raised = await harness.Service().RaiseStartRemindersAsync(CancellationToken.None);

        Assert.That(raised, Is.EqualTo(1));
        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripStartDue, TripAlertSeverities.Info, $"trip-startdue:{TestFactory.TripId:N}",
            It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Once);
        harness.EventWriter.Verify(w => w.AppendAsync(
            TestFactory.AccountId, TestFactory.TripId, null, TripEventTypes.TripStartDue, It.IsAny<DateTimeOffset>(),
            TripEventSources.Job, null, $"trip-start-due:{TestFactory.TripId:N}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task StartReminder_NeverChangesTheTripStatus()
    {
        // Spec 11 §10, spelled out because it is the tempting shortcut: auto-starting a trip nobody
        // began would stamp an ActualStartAt and a distance baseline for a vehicle that has not
        // moved, and every report downstream inherits the fiction. A reminder is a nudge to a
        // human. The service touches ONLY the alert channel and the event log.
        var harness = new EtaHarness();
        harness.WithTripDueToStart(TestFactory.Trip(TripStatuses.Created));

        await harness.Service().RaiseStartRemindersAsync(CancellationToken.None);

        harness.StopWriter.VerifyNoOtherCalls();
        harness.EventWriter.Verify(w => w.AppendAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(),
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task StartReminder_IsNotRaisedAgainOnceTheEventKeyExists()
    {
        // The event log IS the once-only guard: the job runs every 15 minutes and the lead window
        // spans hours, so the same trip is a candidate on many consecutive cycles.
        var harness = new EtaHarness();
        harness.WithTripDueToStart(TestFactory.Trip(TripStatuses.Created));
        harness.EventWriter
            .Setup(w => w.HasEventAsync(TestFactory.AccountId, $"trip-start-due:{TestFactory.TripId:N}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var raised = await harness.Service().RaiseStartRemindersAsync(CancellationToken.None);

        Assert.That(raised, Is.Zero);
        harness.AlertEmitter.Verify(e => e.EmitAsync(
            TripEventTypes.TripStartDue, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task StartReminder_WhenTheEmitterFails_TheKeyIsNotBurnedSoTheNextCycleRetries()
    {
        // The ordering fix, verbatim: CHECK the key, EMIT, then APPEND. Appending first burned the
        // key before the emission was known to have succeeded, so a transient Manager failure meant
        // the reminder was never retried and the account got it ZERO times rather than once.
        var harness = new EtaHarness();
        harness.WithTripDueToStart(TestFactory.Trip(TripStatuses.Created));
        harness.AlertEmitter
            .Setup(e => e.EmitAsync(TripEventTypes.TripStartDue, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Manager is down"));

        var raised = await harness.Service().RaiseStartRemindersAsync(CancellationToken.None);

        Assert.That(raised, Is.Zero);
        harness.EventWriter.Verify(w => w.AppendAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(),
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never,
            "the key must not be burned by a failed emission — the next cycle has to be able to retry");
    }

    [Test]
    public async Task StartReminder_AFailedEmissionOnOneTripDoesNotStopTheRest()
    {
        // Per-trip isolation: one account's Manager problem must not silently drop every later
        // trip in the same cycle.
        var secondTripId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var harness = new EtaHarness();
        harness.WithTripsDueToStart(
            TestFactory.Trip(TripStatuses.Created),
            TestFactory.Trip(TripStatuses.Created, secondTripId));
        harness.AlertEmitter
            .Setup(e => e.EmitAsync(TripEventTypes.TripStartDue, It.IsAny<string>(), $"trip-startdue:{TestFactory.TripId:N}", It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Manager is down"));

        var raised = await harness.Service().RaiseStartRemindersAsync(CancellationToken.None);

        Assert.That(raised, Is.EqualTo(1), "the second trip's reminder still had to go out");
    }

    [Test]
    public async Task StartReminder_LosingTheAppendRaceCountsAsNoWork()
    {
        // Two cycles (or two replicas) both pass the HasEventAsync check; the unique index is still
        // the authority and the loser's append returns false. Counting it would make the
        // on-work-only recorder write a BackgroundJobRun row for work it did not do.
        var harness = new EtaHarness();
        harness.WithTripDueToStart(TestFactory.Trip(TripStatuses.Created));
        harness.EventWriter
            .Setup(w => w.AppendAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var raised = await harness.Service().RaiseStartRemindersAsync(CancellationToken.None);

        Assert.That(raised, Is.Zero);
    }

    private static EtaCandidateVm Candidate(
        bool fresh,
        DateTimeOffset? plannedArrivalTo = null,
        DateTimeOffset? currentEtaAt = null,
        string? currentEtaSource = null,
        DateTimeOffset? delayAlertedAt = null)
        => new(
            TestFactory.TripId,
            TestFactory.AccountId,
            "TRIP-001",
            TestFactory.TransporterId,
            null,
            4.65,
            -74.05,
            fresh ? DateTimeOffset.UtcNow.AddMinutes(-2) : DateTimeOffset.UtcNow.AddHours(-2),
            fresh,
            StopId,
            "Customer site",
            4.7,
            -74.0,
            plannedArrivalTo,
            delayAlertedAt,
            currentEtaAt,
            currentEtaSource ?? EtaSources.Unavailable);

    private sealed class EtaHarness
    {
        public EtaHarness()
        {
            AccountFeatureReader
                .Setup(r => r.GetEnabledAccountIdsAsync(FeatureKeys.TripManagement, It.IsAny<CancellationToken>()))
                .ReturnsAsync([TestFactory.AccountId]);
            AccountFeatureReader
                .Setup(r => r.GetAccountConfigAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TripAccountConfigVm.Default);

            RoutingProvider.SetupGet(p => p.IsConfigured).Returns(true);
            RoutingProvider.SetupGet(p => p.Name).Returns(RoutePlanProviders.OpenRouteService);
            RoutingProvider
                .Setup(p => p.GetSummaryAsync(It.IsAny<CoordinateVm>(), It.IsAny<CoordinateVm>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RouteSummaryVm(12000d, 600));

            DetectionReader
                .Setup(r => r.GetEtaCandidatesAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            DetectionReader
                .Setup(r => r.GetTripsDueToStartAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            StopWriter
                .Setup(w => w.UpdateStopEtaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            StopWriter
                .Setup(w => w.MarkStopDelayAlertedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // The once-only guard's default answer: the key is not there yet, and the append wins.
            EventWriter
                .Setup(w => w.HasEventAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            EventWriter
                .Setup(w => w.AppendAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        public Mock<IAccountFeatureReader> AccountFeatureReader { get; } = new();

        public Mock<ITripDetectionReader> DetectionReader { get; } = new();

        public Mock<ITripStopWriter> StopWriter { get; } = new();

        public Mock<ITripEventWriter> EventWriter { get; } = new();

        public Mock<IRoutingProvider> RoutingProvider { get; } = new();

        public Mock<IAlertEmitter> AlertEmitter { get; } = new();

        public void WithCandidate(EtaCandidateVm candidate)
            => DetectionReader
                .Setup(r => r.GetEtaCandidatesAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([candidate]);

        /// <summary>Replaces the account configuration so a non-default threshold can be exercised.</summary>
        public void WithConfig(TripAccountConfigVm config)
            => AccountFeatureReader
                .Setup(r => r.GetAccountConfigAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(config);

        public void WithTripDueToStart(TripVm trip) => WithTripsDueToStart(trip);

        public void WithTripsDueToStart(params TripVm[] trips)
            => DetectionReader
                .Setup(r => r.GetTripsDueToStartAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(trips);

        public TripEtaService Service()
            => new(AccountFeatureReader.Object, DetectionReader.Object, StopWriter.Object, EventWriter.Object,
                RoutingProvider.Object, AlertEmitter.Object, TestFactory.Logger<TripEtaService>());
    }
}
