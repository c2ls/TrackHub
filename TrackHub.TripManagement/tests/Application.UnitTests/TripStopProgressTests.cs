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
using TrackHub.TripManagement.Application.TripStops.Commands.Progress;

namespace TrackHub.TripManagement.Application.UnitTests;

/// <summary>
/// Acceptance 15: every command carrying a <c>clientEventId</c> is idempotent server-side.
/// This is verified by test because spec 10's offline outbox depends on it — an outbox that
/// retries a queued arrival must not produce two arrivals, and the server must not rely on the
/// client being careful.
/// </summary>
[TestFixture]
public class TripStopProgressTests
{
    private static readonly Guid ClientEventId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    [Test]
    public async Task RecordStopArrival_BuildsTheSpecifiedIdempotencyKey()
    {
        var harness = new ProgressHarness();
        var handler = harness.ArrivalHandler();

        await handler.Handle(Arrival(), CancellationToken.None);

        harness.StopWriter.Verify(w => w.RecordStopProgressAsync(
            TestFactory.TripId, TestFactory.StopId, TestFactory.AccountId, TripStopStatuses.Arrived,
            It.IsAny<DateTimeOffset>(), It.IsAny<double?>(), It.IsAny<double?>(), TripEventSources.Portal,
            $"trip-arrive:{TestFactory.StopId:N}:{ClientEventId:N}", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// A duplicate answers TRUE — success — and emits no alert.
    /// <para>
    /// This previously asserted <c>false</c> ("nothing written"), which is the WRITER's contract,
    /// not this mutation's. Over a <c>Boolean!</c> field a caller cannot tell that apart from a
    /// failure, and spec 10's offline outbox — the named consumer of this idempotency — would keep
    /// the event queued and retry it forever. Acceptance 15 says a duplicate submission returns
    /// success, so the device can drop it. The portal ignores the value entirely (its mutation
    /// succeeds on the absence of a GraphQL error), so nothing else depends on the old shape.
    /// </para>
    /// </summary>
    [Test]
    public async Task RecordStopArrival_DuplicateSubmission_SucceedsAndEmitsNoAlert()
    {
        var harness = new ProgressHarness(recorded: false);
        var handler = harness.ArrivalHandler();

        var result = await handler.Handle(Arrival(), CancellationToken.None);

        Assert.That(result, Is.True, "a duplicate is a SUCCESS the caller can retire, not a failure to retry");
        harness.AlertEmitter.Verify(e => e.EmitAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TripAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RecordStopDeparture_BuildsTheSpecifiedIdempotencyKey()
    {
        var harness = new ProgressHarness();
        var handler = new RecordStopDepartureCommandHandler(
            harness.StopWriter.Object, harness.Reader.Object, harness.AlertEmitter.Object,
            harness.UserReader.Object, harness.User.Object, TestFactory.Logger<RecordStopDepartureCommandHandler>());

        await handler.Handle(new RecordStopDepartureCommand(
            TestFactory.TripId, TestFactory.StopId, DateTimeOffset.UtcNow, null, null, ClientEventId), CancellationToken.None);

        harness.StopWriter.Verify(w => w.RecordStopProgressAsync(
            TestFactory.TripId, TestFactory.StopId, TestFactory.AccountId, TripStopStatuses.Departed,
            It.IsAny<DateTimeOffset>(), null, null, TripEventSources.Portal,
            $"trip-depart:{TestFactory.StopId:N}:{ClientEventId:N}", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SkipStop_BuildsTheSpecifiedIdempotencyKeyAndCarriesTheReason()
    {
        var harness = new ProgressHarness();
        var handler = new SkipStopCommandHandler(
            harness.StopWriter.Object, harness.Reader.Object, harness.AlertEmitter.Object,
            harness.UserReader.Object, harness.User.Object, TestFactory.Logger<SkipStopCommandHandler>());

        await handler.Handle(new SkipStopCommand(
            TestFactory.TripId, TestFactory.StopId, DateTimeOffset.UtcNow, "Customer closed", ClientEventId), CancellationToken.None);

        harness.StopWriter.Verify(w => w.RecordStopProgressAsync(
            TestFactory.TripId, TestFactory.StopId, TestFactory.AccountId, TripStopStatuses.Skipped,
            It.IsAny<DateTimeOffset>(), null, null, TripEventSources.Portal,
            $"trip-skip:{TestFactory.StopId:N}:{ClientEventId:N}", "Customer closed", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void RecordStopArrival_OnATripThatIsNotInProgress_IsRejectedWithTripNotActive()
    {
        var harness = new ProgressHarness(tripStatus: TripStatuses.Created);
        var handler = harness.ArrivalHandler();

        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
            await handler.Handle(Arrival(), CancellationToken.None));

        Assert.That(ex!.Errors.Values.SelectMany(v => v), Does.Contain(TripErrorCodes.TripNotActive));
        harness.StopWriter.Verify(w => w.RecordStopProgressAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<double?>(),
            It.IsAny<double?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static RecordStopArrivalCommand Arrival()
        => new(TestFactory.TripId, TestFactory.StopId, DateTimeOffset.UtcNow, 4.7, -74.0, ClientEventId);

    private sealed class ProgressHarness
    {
        public ProgressHarness(bool recorded = true, string tripStatus = TripStatuses.InProgress)
        {
            Reader.Setup(r => r.GetTripAsync(TestFactory.TripId, TestFactory.AccountId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestFactory.Trip(tripStatus));
            StopWriter.Setup(w => w.RecordStopProgressAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<double?>(),
                    It.IsAny<double?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(recorded);
        }

        public Mock<ITripStopWriter> StopWriter { get; } = new();

        public Mock<ITripReader> Reader { get; } = new();

        public Mock<IAlertEmitter> AlertEmitter { get; } = new();

        public Mock<IUser> User { get; } = TestFactory.User();

        public Mock<IUserReader> UserReader { get; } = TestFactory.UserReader();

        public RecordStopArrivalCommandHandler ArrivalHandler()
            => new(StopWriter.Object, Reader.Object, AlertEmitter.Object, UserReader.Object, User.Object,
                TestFactory.Logger<RecordStopArrivalCommandHandler>());
    }
}
