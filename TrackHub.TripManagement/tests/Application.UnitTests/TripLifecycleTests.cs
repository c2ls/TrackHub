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

using Common.Application.Interfaces;
using TrackHub.TripManagement.Application.Trips.Commands.Lifecycle;

namespace TrackHub.TripManagement.Application.UnitTests;

/// <summary>
/// Acceptance 11: the transition matrix is enforced and an illegal transition changes NOTHING.
/// The negative cases matter more than the positive ones — a lifecycle that silently accepts
/// <c>Completed → InProgress</c> would let a closed trip be reopened and re-billed.
/// </summary>
[TestFixture]
public class TripLifecycleTests
{
    [TestCase(TripStatuses.Created, TripStatuses.InProgress)]
    [TestCase(TripStatuses.Created, TripStatuses.Cancelled)]
    [TestCase(TripStatuses.InProgress, TripStatuses.Paused)]
    [TestCase(TripStatuses.InProgress, TripStatuses.Completed)]
    [TestCase(TripStatuses.InProgress, TripStatuses.Aborted)]
    [TestCase(TripStatuses.InProgress, TripStatuses.Cancelled)]
    [TestCase(TripStatuses.Paused, TripStatuses.InProgress)]
    [TestCase(TripStatuses.Paused, TripStatuses.Cancelled)]
    [TestCase(TripStatuses.Paused, TripStatuses.Aborted)]
    public void CanTransition_AllowsTheMatrix(string from, string to)
        => Assert.That(TripStatuses.CanTransition(from, to), Is.True);

    [TestCase(TripStatuses.Created, TripStatuses.Paused)]
    [TestCase(TripStatuses.Created, TripStatuses.Completed)]
    [TestCase(TripStatuses.Created, TripStatuses.Aborted)]
    [TestCase(TripStatuses.Completed, TripStatuses.InProgress)]
    [TestCase(TripStatuses.Cancelled, TripStatuses.InProgress)]
    [TestCase(TripStatuses.Aborted, TripStatuses.InProgress)]
    [TestCase(TripStatuses.Completed, TripStatuses.Cancelled)]
    public void CanTransition_RejectsEverythingElse(string from, string to)
        => Assert.That(TripStatuses.CanTransition(from, to), Is.False);

    [TestCase(TripStatuses.Completed)]
    [TestCase(TripStatuses.Cancelled)]
    [TestCase(TripStatuses.Aborted)]
    public void Terminal_StatusesAreTerminal(string status)
        => Assert.That(TripStatuses.IsTerminal(status), Is.True);

    [Test]
    public async Task StartTrip_TransitionsAndAppendsExactlyOneEvent()
    {
        var harness = new LifecycleHarness(TripStatuses.Created);
        var handler = new StartTripCommandHandler(
            harness.Writer.Object, harness.Reader.Object, harness.EventWriter.Object,
            harness.AlertEmitter.Object, harness.UserReader.Object, harness.User.Object,
            TestFactory.Logger<StartTripCommandHandler>());

        await handler.Handle(new StartTripCommand(TestFactory.TripId), CancellationToken.None);

        harness.Writer.Verify(w => w.TransitionTripAsync(
            TestFactory.TripId, TestFactory.AccountId, TripStatuses.InProgress, null, false, It.IsAny<CancellationToken>()), Times.Once);
        harness.EventWriter.Verify(w => w.AppendAsync(
            TestFactory.AccountId, TestFactory.TripId, null, TripEventTypes.TripStarted,
            It.IsAny<DateTimeOffset>(), TripEventSources.Portal, null,
            $"trip-start:{TestFactory.TripId:N}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void StartTrip_OnATerminalTrip_ThrowsAndWritesNothing()
    {
        var harness = new LifecycleHarness(TripStatuses.Completed);
        var handler = new StartTripCommandHandler(
            harness.Writer.Object, harness.Reader.Object, harness.EventWriter.Object,
            harness.AlertEmitter.Object, harness.UserReader.Object, harness.User.Object,
            TestFactory.Logger<StartTripCommandHandler>());

        Assert.ThrowsAsync<ValidationException>(async () =>
            await handler.Handle(new StartTripCommand(TestFactory.TripId), CancellationToken.None));

        harness.Writer.VerifyNoOtherCalls();
        harness.EventWriter.VerifyNoOtherCalls();
    }

    [Test]
    public void PauseTrip_FromCreated_IsRejected()
    {
        var harness = new LifecycleHarness(TripStatuses.Created);
        var handler = new PauseTripCommandHandler(
            harness.Writer.Object, harness.Reader.Object, harness.EventWriter.Object,
            harness.AlertEmitter.Object, harness.UserReader.Object, harness.User.Object,
            TestFactory.Logger<PauseTripCommandHandler>());

        Assert.ThrowsAsync<ValidationException>(async () =>
            await handler.Handle(new PauseTripCommand(TestFactory.TripId), CancellationToken.None));

        harness.Writer.VerifyNoOtherCalls();
    }

    [Test]
    public void CompleteTrip_WithAnOpenStop_IsRejectedWithStopsNotComplete()
    {
        var harness = new LifecycleHarness(TripStatuses.InProgress);
        harness.Reader
            .Setup(r => r.GetTripDetailAsync(TestFactory.TripId, TestFactory.AccountId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TripDetailVm(TestFactory.Trip(), [TestFactory.Stop(TripStopStatuses.Arrived)], null, null, [], []));

        var handler = new CompleteTripCommandHandler(
            harness.Writer.Object, harness.Reader.Object, harness.EventWriter.Object,
            harness.AlertEmitter.Object, harness.UserReader.Object, harness.User.Object,
            TestFactory.Logger<CompleteTripCommandHandler>());

        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
            await handler.Handle(new CompleteTripCommand(TestFactory.TripId, false), CancellationToken.None));

        Assert.That(ex!.Errors.Values.SelectMany(v => v), Does.Contain(TripErrorCodes.StopsNotComplete));
        harness.Writer.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CompleteTrip_WithForce_SkipsTheStopCheckAndRecordsTheOverride()
    {
        var harness = new LifecycleHarness(TripStatuses.InProgress);
        var handler = new CompleteTripCommandHandler(
            harness.Writer.Object, harness.Reader.Object, harness.EventWriter.Object,
            harness.AlertEmitter.Object, harness.UserReader.Object, harness.User.Object,
            TestFactory.Logger<CompleteTripCommandHandler>());

        await handler.Handle(new CompleteTripCommand(TestFactory.TripId, true), CancellationToken.None);

        harness.Writer.Verify(w => w.TransitionTripAsync(
            TestFactory.TripId, TestFactory.AccountId, TripStatuses.Completed, "forced", true, It.IsAny<CancellationToken>()), Times.Once);
        harness.Reader.Verify(r => r.GetTripDetailAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CompleteTrip_WithEveryStopClosed_IsAccepted()
    {
        var harness = new LifecycleHarness(TripStatuses.InProgress);
        harness.Reader
            .Setup(r => r.GetTripDetailAsync(TestFactory.TripId, TestFactory.AccountId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TripDetailVm(
                TestFactory.Trip(),
                [TestFactory.Stop(TripStopStatuses.Departed), TestFactory.Stop(TripStopStatuses.Skipped, 2, Guid.NewGuid())],
                null, null, [], []));

        var handler = new CompleteTripCommandHandler(
            harness.Writer.Object, harness.Reader.Object, harness.EventWriter.Object,
            harness.AlertEmitter.Object, harness.UserReader.Object, harness.User.Object,
            TestFactory.Logger<CompleteTripCommandHandler>());

        await handler.Handle(new CompleteTripCommand(TestFactory.TripId, false), CancellationToken.None);

        harness.Writer.Verify(w => w.TransitionTripAsync(
            TestFactory.TripId, TestFactory.AccountId, TripStatuses.Completed, null, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class LifecycleHarness
    {
        public LifecycleHarness(string status)
        {
            Reader.Setup(r => r.GetTripAsync(TestFactory.TripId, TestFactory.AccountId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestFactory.Trip(status));
            Writer.Setup(w => w.TransitionTripAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            EventWriter.Setup(w => w.AppendAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        public Mock<ITripWriter> Writer { get; } = new(MockBehavior.Strict);

        public Mock<ITripReader> Reader { get; } = new();

        public Mock<ITripEventWriter> EventWriter { get; } = new(MockBehavior.Strict);

        public Mock<IAlertEmitter> AlertEmitter { get; } = new();

        public Mock<IUser> User { get; } = TestFactory.User();

        public Mock<IUserReader> UserReader { get; } = TestFactory.UserReader();
    }
}
