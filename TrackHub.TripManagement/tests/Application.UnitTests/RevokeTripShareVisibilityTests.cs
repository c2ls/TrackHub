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
using TrackHub.TripManagement.Application.TripShares.Commands.Revoke;

namespace TrackHub.TripManagement.Application.UnitTests;

/// <summary>
/// Group-visibility tests for share revocation.
/// <para>
/// Revoking is a destructive, customer-visible action: the link a dispatcher kills is the one an
/// end customer is watching a delivery on. The handler used to resolve the share by id under
/// ACCOUNT scope only, with no trip lookup at all, so a <c>User</c>-role dispatcher holding a
/// borrowed or guessed <c>TripShareId</c> could take down another group's tracking link — a denial
/// of service against data they were never entitled to see.
/// </para>
/// </summary>
[TestFixture]
public sealed class RevokeTripShareVisibilityTests
{
    private static readonly Guid TripShareId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid PublicLinkGrantId = Guid.Parse("88888888-8888-8888-8888-888888888888");

    private Mock<ITripShareWriter> shareWriter = null!;
    private Mock<ITripShareReader> shareReader = null!;
    private Mock<ITripReader> tripReader = null!;
    private Mock<ITripEventWriter> tripEventWriter = null!;
    private Mock<IPublicLinkGrantClient> grantClient = null!;

    [SetUp]
    public void SetUp()
    {
        shareWriter = new Mock<ITripShareWriter>();
        shareReader = new Mock<ITripShareReader>();
        tripReader = new Mock<ITripReader>();
        tripEventWriter = new Mock<ITripEventWriter>();
        grantClient = new Mock<IPublicLinkGrantClient>();

        shareWriter
            .Setup(w => w.RevokeShareAsync(TripShareId, TestFactory.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PublicLinkGrantId);

        // The share exists in the account - the ONLY thing the old handler ever checked.
        shareReader
            .Setup(r => r.FindTripIdByShareAsync(TripShareId, TestFactory.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFactory.TripId);
    }

    private RevokeTripShareCommandHandler Handler(Mock<IUser> user)
        => new(
            shareWriter.Object,
            shareReader.Object,
            tripReader.Object,
            tripEventWriter.Object,
            grantClient.Object,
            TestFactory.UserReader().Object,
            user.Object);

    /// <summary>
    /// The defect, asserted directly: a dispatcher outside the trip's groups gets
    /// <c>NotFoundException</c> — never <c>ForbiddenAccessException</c>, which would confirm the
    /// share is real — and NOTHING is revoked, locally or in Manager.
    /// </summary>
    [Test]
    public void Revoke_ByDispatcherOutsideTheTripsGroups_ThrowsNotFoundAndRevokesNothing()
    {
        var user = TestFactory.User(Roles.User);

        // The one visibility source: a trip outside the caller's groups is indistinguishable from
        // one that does not exist.
        tripReader
            .Setup(r => r.GetTripAsync(TestFactory.TripId, TestFactory.AccountId, TestFactory.UserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Ardalis.GuardClauses.NotFoundException($"{TestFactory.TripId}", "Trip"));

        var handler = Handler(user);

        Assert.ThrowsAsync<Ardalis.GuardClauses.NotFoundException>(async () =>
            await handler.Handle(new RevokeTripShareCommand(TestFactory.TripId, TripShareId), CancellationToken.None));

        shareWriter.Verify(
            w => w.RevokeShareAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "the share must not be revoked locally");

        grantClient.Verify(
            c => c.RevokeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "the Manager grant must not be revoked - the customer's link stays live");

        tripEventWriter.Verify(
            w => w.AppendAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(),
                It.IsAny<DateTimeOffset>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>An unknown share id is the same answer as an invisible one (non-disclosure).</summary>
    [Test]
    public void Revoke_OfAShareThatIsNotInTheAccount_ThrowsNotFound()
    {
        shareReader
            .Setup(r => r.FindTripIdByShareAsync(TripShareId, TestFactory.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var handler = Handler(TestFactory.User(Roles.User));

        Assert.ThrowsAsync<Ardalis.GuardClauses.NotFoundException>(async () =>
            await handler.Handle(new RevokeTripShareCommand(TestFactory.TripId, TripShareId), CancellationToken.None));

        grantClient.Verify(
            c => c.RevokeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// The positive path still works for a dispatcher inside the groups, and the audit event is
    /// addressed by the RESOLVED trip id rather than the caller-supplied one.
    /// </summary>
    [Test]
    public async Task Revoke_ByDispatcherInsideTheTripsGroups_RevokesLocallyAndInManager()
    {
        var user = TestFactory.User(Roles.User);

        tripReader
            .Setup(r => r.GetTripAsync(TestFactory.TripId, TestFactory.AccountId, TestFactory.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFactory.Trip());

        var handler = Handler(user);

        // A deliberately WRONG caller-supplied TripId: it must not reach the event log.
        var wrongTripId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var result = await handler.Handle(
            new RevokeTripShareCommand(wrongTripId, TripShareId), CancellationToken.None);

        Assert.That(result, Is.EqualTo(TripShareId));

        shareWriter.Verify(
            w => w.RevokeShareAsync(TripShareId, TestFactory.AccountId, It.IsAny<CancellationToken>()),
            Times.Once);

        grantClient.Verify(
            c => c.RevokeAsync(PublicLinkGrantId, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        tripEventWriter.Verify(
            w => w.AppendAsync(
                TestFactory.AccountId, TestFactory.TripId, null, TripEventTypes.TripShareRevoked,
                It.IsAny<DateTimeOffset>(), TripEventSources.Portal, null,
                It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "the audit event is addressed by the resolved trip id, not the caller-supplied one");
    }

    /// <summary>
    /// An Administrator is account-wide, so the scope user id passed to the visibility check is
    /// null — the group predicate is skipped, the ACCOUNT boundary is not.
    /// </summary>
    [Test]
    public async Task Revoke_ByAdministrator_ResolvesWithAccountWideScope()
    {
        tripReader
            .Setup(r => r.GetTripAsync(TestFactory.TripId, TestFactory.AccountId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFactory.Trip());

        var handler = Handler(TestFactory.User(Roles.Administrator));

        await handler.Handle(new RevokeTripShareCommand(TestFactory.TripId, TripShareId), CancellationToken.None);

        tripReader.Verify(
            r => r.GetTripAsync(TestFactory.TripId, TestFactory.AccountId, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
