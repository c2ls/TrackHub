using Common.Application.Interfaces;

namespace TrackHub.Manager.Application.PublicLinks.Commands;

// MIXED SURFACE — read the handler before touching these attributes.
//
// Two callers mint grants here: the portal (a USER, always for their own account) and
// TripManagement's ShareTripCommand, which relays a user's share action to Manager over the global
// trip_client service identity. Only the second one crosses tenants, and only as an artifact of the
// service hop — the account it sends is the calling user's own.
//
// The account is nested in PublicLinkGrantDto, so before TrackHubCommon 1.0.7 neither caller was
// policed. [AllowCrossAccount] is the minimum needed to keep the trip_client hop working, but it
// switches the pipeline guard off for EVERY caller, portal included — and minting a grant for
// another tenant's document would be a permanent, anonymous data leak. The handler therefore
// re-binds user principals to their own account explicitly, so the opt-out buys the service
// identity exactly what it needs and nothing more.
[Authorize(Resource = Resources.PublicLinks, Action = Actions.Write)]
[RequireFeature(FeatureKeys.PublicLinks)]
[AllowCrossAccount("TripManagement relays a user's trip-share action to Manager under the global trip_client identity (spec 11 §18.10 forbids a parallel link mechanism), and that token carries no account claim. User principals are NOT unguarded by this: the handler binds them to their own account below.")]
public readonly record struct CreatePublicLinkGrantCommand(PublicLinkGrantDto PublicLinkGrant) : IRequest<PublicLinkGrantVm>;
public class CreatePublicLinkGrantCommandHandler(IPublicLinkGrantWriter writer, ICurrentPrincipal principal) : IRequestHandler<CreatePublicLinkGrantCommand, PublicLinkGrantVm>
{
    public async Task<PublicLinkGrantVm> Handle(CreatePublicLinkGrantCommand request, CancellationToken cancellationToken)
    {
        // Restores, for user principals only, the tenant binding that [AllowCrossAccount] removes.
        if (principal.PrincipalType == PrincipalType.User
            && principal.AccountId != request.PublicLinkGrant.AccountId)
        {
            throw new ForbiddenAccessException(
                "Insufficient permissions. A public link may only be issued for the caller's own account.");
        }

        return await writer.CreatePublicLinkGrantAsync(request.PublicLinkGrant, cancellationToken);
    }
}

[Authorize(Resource = Resources.PublicLinks, Action = Actions.Delete)]
[AllowCrossAccount("TripManagement relays a user's trip-share revocation here under the global trip_client identity with no account claim (same hop as CreatePublicLinkGrantCommand above). Tenant callers are NOT unguarded by this: PublicLinkGrantWriter.RevokePublicLinkGrantAsync loads the grant and RequireAccountWriteAccess checks its owning account.")]
public readonly record struct RevokePublicLinkGrantCommand(Guid PublicLinkGrantId, string RevokedBy) : IRequest;
public class RevokePublicLinkGrantCommandHandler(IPublicLinkGrantWriter writer) : IRequestHandler<RevokePublicLinkGrantCommand>
{
    public async Task Handle(RevokePublicLinkGrantCommand request, CancellationToken cancellationToken) => await writer.RevokePublicLinkGrantAsync(request.PublicLinkGrantId, request.RevokedBy, cancellationToken);
}

// RecordPublicLinkAccessCommand was REMOVED (spec 11 §7.8/§18.10, closing spec 02 §7.4). It was a
// second, divergent access counter: it wrote a `RecordPublicLinkAccess` audit action while the
// anonymous REST endpoint independently counted access and wrote `PublicLinkAccessed`. Access
// counting and the `PublicLinkAccessed` audit event now live in exactly one place —
// `ResolvePublicLinkGrantCommand` / `IPublicLinkGrantResolver`. Do not reintroduce a standalone
// "record access" surface: an access that was not a successful resolution is not an access.
