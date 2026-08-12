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
using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.TripShares.Commands.Revoke;

/// <summary>
/// Revokes a public tracking link.
/// <para>
/// <b>Deliberately NOT feature-gated.</b> Every other trip surface carries
/// <c>[RequireFeature(FeatureKeys.TripManagement)]</c>; this one must not. An account that has been
/// downgraded or had the feature switched off still has live links out in the world, and taking
/// away the ability to pull them back would turn a billing change into a data-exposure incident.
/// Revocation is always available (spec 11 §7.3, following the spec-02 §7.4 precedent).
/// </para>
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Custom)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct RevokeTripShareCommand(Guid TripId, Guid TripShareId) : IRequest<Guid>;

public sealed class RevokeTripShareCommandHandler(
    ITripShareWriter shareWriter,
    ITripShareReader shareReader,
    ITripReader reader,
    ITripEventWriter tripEventWriter,
    IPublicLinkGrantClient publicLinkGrantClient,
    IUserReader userReader,
    IUser user) : IRequestHandler<RevokeTripShareCommand, Guid>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<Guid> Handle(RevokeTripShareCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);

        // Account scope alone was NOT enough here: this handler previously revoked by TripShareId
        // with no trip lookup at all, so a dispatcher holding a borrowed or guessed share id could
        // kill another group's customer-facing tracking link. Resolving through the shared helper
        // applies the same Visible() predicate as every other path; an unseen share is
        // NotFoundException, never Forbidden (non-disclosure).
        //
        // The RESOLVED trip id is used below, not request.TripId: the caller-supplied id is not
        // trusted to address the audit event.
        var tripId = await TripVisibility.ResolveVisibleTripByShareAsync(
            shareReader, reader, request.TripShareId, caller.AccountId, scopeUserId, cancellationToken);

        // Stamps RevokedAt locally and yields the Manager grant id the link is backed by.
        var publicLinkGrantId = await shareWriter.RevokeShareAsync(request.TripShareId, caller.AccountId, cancellationToken);

        await publicLinkGrantClient.RevokeAsync(publicLinkGrantId, user.Id ?? UserId.ToString(), cancellationToken);

        await tripEventWriter.AppendAsync(
            caller.AccountId, tripId, null, TripEventTypes.TripShareRevoked,
            DateTimeOffset.UtcNow, TripEventSources.Portal, null,
            $"trip-share-revoke:{request.TripShareId:N}", cancellationToken);

        return request.TripShareId;
    }
}

public sealed class RevokeTripShareValidator : AbstractValidator<RevokeTripShareCommand>
{
    public RevokeTripShareValidator()
    {
        RuleFor(v => v.TripId).NotEmpty();
        RuleFor(v => v.TripShareId).NotEmpty();
    }
}
