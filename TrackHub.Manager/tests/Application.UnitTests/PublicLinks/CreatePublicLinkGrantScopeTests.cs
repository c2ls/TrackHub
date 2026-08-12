using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Application.PublicLinks.Commands;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Application.UnitTests.PublicLinks;

/// <summary>
/// Pins the compensation on the user-reachable <c>[AllowCrossAccount]</c> mint surface: the marker
/// switches the pipeline tenant guard off for EVERY caller, so the handler's own re-binding of
/// USER principals to their own account is the only thing preventing a permanent, anonymous
/// cross-tenant document leak. Removing that check fails these tests.
/// </summary>
[TestFixture]
public class CreatePublicLinkGrantScopeTests
{
    private static PublicLinkGrantDto GrantFor(Guid accountId)
        => new(accountId, "Trip", Guid.NewGuid().ToString(), "trip:read", "trip-share", null,
            DateTimeOffset.UtcNow.AddDays(1), Guid.NewGuid().ToString());

    private static ICurrentPrincipal Principal(PrincipalType type, Guid? accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(type);
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        return principal.Object;
    }

    [Test]
    public void User_MintingAGrantForAForeignAccount_IsForbidden_AndNothingIsWritten()
    {
        var writer = new Mock<IPublicLinkGrantWriter>();
        var handler = new CreatePublicLinkGrantCommandHandler(
            writer.Object, Principal(PrincipalType.User, Guid.NewGuid()));

        Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(
            new CreatePublicLinkGrantCommand(GrantFor(Guid.NewGuid())), CancellationToken.None));
        writer.Verify(
            w => w.CreatePublicLinkGrantAsync(It.IsAny<PublicLinkGrantDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task User_MintingAGrantForTheirOwnAccount_Passes()
    {
        var accountId = Guid.NewGuid();
        var writer = new Mock<IPublicLinkGrantWriter>();
        var handler = new CreatePublicLinkGrantCommandHandler(
            writer.Object, Principal(PrincipalType.User, accountId));

        await handler.Handle(new CreatePublicLinkGrantCommand(GrantFor(accountId)), CancellationToken.None);

        writer.Verify(
            w => w.CreatePublicLinkGrantAsync(It.IsAny<PublicLinkGrantDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GlobalServiceIdentity_MintingOnBehalfOfATenant_Passes()
    {
        // The trip_client hop the [AllowCrossAccount] marker exists for: the account it sends is
        // the sharing user's own, carried on the DTO.
        var writer = new Mock<IPublicLinkGrantWriter>();
        var handler = new CreatePublicLinkGrantCommandHandler(
            writer.Object, Principal(PrincipalType.ServiceClient, null));

        await handler.Handle(new CreatePublicLinkGrantCommand(GrantFor(Guid.NewGuid())), CancellationToken.None);

        writer.Verify(
            w => w.CreatePublicLinkGrantAsync(It.IsAny<PublicLinkGrantDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
