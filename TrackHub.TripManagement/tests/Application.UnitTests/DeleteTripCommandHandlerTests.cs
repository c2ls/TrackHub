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
using TrackHub.TripManagement.Application.Trips.Commands.Delete;

namespace TrackHub.TripManagement.Application.UnitTests;

/// <summary>
/// Acceptance 16: a trip with recorded events cannot be deleted (409). Trip history is permanent —
/// the answer to "this trip should go away" is cancellation, which preserves stops, events, POD and
/// documents rather than orphaning them.
/// </summary>
[TestFixture]
public class DeleteTripCommandHandlerTests
{
    [Test]
    public void ATripWithEvents_IsRejectedWithConflict()
    {
        var harness = new DeleteHarness(TripStatuses.Created, hasEvents: true);

        var ex = Assert.ThrowsAsync<ConflictException>(async () =>
            await harness.Handler().Handle(new DeleteTripCommand(TestFactory.TripId), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo(TripErrorCodes.TripHasHistory));
        harness.Writer.Verify(w => w.DeleteTripAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase(TripStatuses.InProgress)]
    [TestCase(TripStatuses.Paused)]
    [TestCase(TripStatuses.Completed)]
    [TestCase(TripStatuses.Cancelled)]
    [TestCase(TripStatuses.Aborted)]
    public void ATripPastCreated_IsRejectedWithConflict(string status)
    {
        var harness = new DeleteHarness(status, hasEvents: false);

        Assert.ThrowsAsync<ConflictException>(async () =>
            await harness.Handler().Handle(new DeleteTripCommand(TestFactory.TripId), CancellationToken.None));

        harness.Writer.Verify(w => w.DeleteTripAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AnUntouchedCreatedTrip_IsDeleted()
    {
        var harness = new DeleteHarness(TripStatuses.Created, hasEvents: false);

        await harness.Handler().Handle(new DeleteTripCommand(TestFactory.TripId), CancellationToken.None);

        harness.Writer.Verify(w => w.DeleteTripAsync(TestFactory.TripId, TestFactory.AccountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class DeleteHarness
    {
        public DeleteHarness(string status, bool hasEvents)
        {
            Reader.Setup(r => r.GetTripAsync(TestFactory.TripId, TestFactory.AccountId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestFactory.Trip(status));
            EventWriter.Setup(w => w.HasEventsAsync(TestFactory.TripId, TestFactory.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(hasEvents);
        }

        public Mock<ITripWriter> Writer { get; } = new();

        public Mock<ITripReader> Reader { get; } = new();

        public Mock<ITripEventWriter> EventWriter { get; } = new();

        public Mock<IUser> User { get; } = TestFactory.User();

        public Mock<IUserReader> UserReader { get; } = TestFactory.UserReader();

        public DeleteTripCommandHandler Handler()
            => new(Writer.Object, Reader.Object, EventWriter.Object, UserReader.Object, User.Object);
    }
}
