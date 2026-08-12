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

namespace TrackHub.TripManagement.Application.TripShares.Commands.Share;

/// <summary>
/// Issues a public tracking link for a trip. The grant itself is created in Manager — a parallel
/// link mechanism is forbidden, so token hashing, access counting and the audit event have exactly
/// one implementation (spec 11 §18.10). Manager's <c>[RequireFeature(public-links)]</c> still
/// applies on top of this module's own gate.
/// <para>
/// The plaintext token is returned <b>once</b>, here, and is never re-readable afterwards
/// (acceptance 23).
/// </para>
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Custom)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct ShareTripCommand(
    Guid TripId,
    DateTimeOffset ExpiresAt,
    string Purpose,
    TripShareFieldFlagsDto FieldFlags) : IRequest<TripShareVm>;

public sealed class ShareTripCommandHandler(
    ITripShareWriter shareWriter,
    ITripReader reader,
    ITripEventWriter tripEventWriter,
    IPublicLinkGrantClient publicLinkGrantClient,
    IUserReader userReader,
    IUser user) : IRequestHandler<ShareTripCommand, TripShareVm>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TripShareVm> Handle(ShareTripCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);

        // Validates the trip is inside the caller's account AND inside the caller's group scope
        // before any grant is minted — minting a public link for a trip you cannot see would turn
        // a visibility gap into a permanent, anonymous data leak.
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);
        await reader.GetTripAsync(request.TripId, caller.AccountId, scopeUserId, cancellationToken);

        var principalId = user.Id ?? UserId.ToString();
        var grant = await publicLinkGrantClient.CreateAsync(
            caller.AccountId,
            TripSharing.ResourceType,
            request.TripId.ToString(),
            TripSharing.TrackScope,
            request.Purpose,
            request.ExpiresAt,
            principalId,
            cancellationToken);

        var share = await shareWriter.CreateShareAsync(
            request.TripId,
            caller.AccountId,
            grant.PublicLinkGrantId,
            request.FieldFlags,
            principalId,
            grant.ExpiresAt,
            cancellationToken);

        await tripEventWriter.AppendAsync(
            caller.AccountId, request.TripId, null, TripEventTypes.TripShared,
            DateTimeOffset.UtcNow, TripEventSources.Portal, null,
            $"trip-share:{share.TripShareId:N}", cancellationToken);

        // The only moment the plaintext token exists outside Manager.
        return share with { Token = grant.Token };
    }
}

public sealed class ShareTripValidator : AbstractValidator<ShareTripCommand>
{
    public ShareTripValidator()
    {
        RuleFor(v => v.TripId).NotEmpty();
        RuleFor(v => v.Purpose).NotEmpty().MaximumLength(200);
        RuleFor(v => v.ExpiresAt).GreaterThan(_ => DateTimeOffset.UtcNow);
    }
}
