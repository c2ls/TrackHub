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

namespace TrackHub.Manager.Application.PublicLinks.Commands;

/// <summary>
/// Resolves an anonymous public link on behalf of a downstream service (spec 11 §7.8).
/// <para>
/// ServiceClient-only, modelled on <c>GetActiveGeocodingProviderQuery</c>: a portal user has no
/// business exchanging a raw public-link token through this surface, and a PublicLink principal
/// cannot reach any <c>[Authorize]</c> operation at all. Manager's own anonymous REST endpoint does
/// NOT go through this command — it calls <see cref="IPublicLinkGrantResolver"/> directly, because it
/// has no principal for the authorization pipeline to evaluate.
/// </para>
/// <para>
/// Deliberately NOT feature-gated on <c>public-links</c>: an account downgraded after issuing a link
/// must still have its already-issued links resolve or fail on their own terms (revocation, expiry),
/// mirroring the ungated revoke path (spec 02 §7.4 precedent).
/// </para>
/// </summary>
[Authorize(Resource = Resources.PublicLinks, Action = Actions.Read, PrincipalTypes = "ServiceClient")]
[AllowCrossAccount("Anonymous public-link resolution: TripManagement's public tracking endpoint resolves a link under its own global trip_client service identity, and the account is a property OF the link being resolved, not of any caller. There is no principal account to compare against.")]
public readonly record struct ResolvePublicLinkGrantCommand(
    Guid PublicLinkGrantId,
    Guid AccountId,
    string ResourceType,
    string ResourceId,
    string Scope,
    string Token) : IRequest<PublicLinkResolutionVm>;

public class ResolvePublicLinkGrantCommandHandler(IPublicLinkGrantResolver resolver) : IRequestHandler<ResolvePublicLinkGrantCommand, PublicLinkResolutionVm>
{
    public async Task<PublicLinkResolutionVm> Handle(ResolvePublicLinkGrantCommand request, CancellationToken cancellationToken)
    {
        var result = await resolver.ResolvePublicLinkGrantAsync(
            request.PublicLinkGrantId,
            request.AccountId,
            request.ResourceType,
            request.ResourceId,
            request.Scope,
            request.Token,
            cancellationToken);

        // Narrow the internal result to the service-facing echo: identifiers only, and only on
        // success. NotFound/Expired carry nulls so a rejected caller learns nothing about the grant.
        return result.Grant is { } grant
            ? new PublicLinkResolutionVm(result.Resolution, grant.PublicLinkGrantId, grant.ResourceId)
            : new PublicLinkResolutionVm(result.Resolution, null, null);
    }
}

public class ResolvePublicLinkGrantCommandValidator : AbstractValidator<ResolvePublicLinkGrantCommand>
{
    public ResolvePublicLinkGrantCommandValidator()
    {
        RuleFor(x => x.PublicLinkGrantId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.ResourceType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ResourceId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Token).NotEmpty().MaximumLength(500);
    }
}
