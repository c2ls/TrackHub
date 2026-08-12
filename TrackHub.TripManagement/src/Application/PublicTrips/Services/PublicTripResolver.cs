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

using TrackHub.TripManagement.Application.PublicTrips.Services.Interfaces;

namespace TrackHub.TripManagement.Application.PublicTrips.Services;

/// <summary>
/// Resolves a public tracking link through Manager's shared grant resolution, then projects the
/// <c>TripShare</c>-configured snapshot locally.
/// <para>
/// The feature re-check is <b>non-disclosure, not authorization</b>: an account that lost
/// <c>trip-management</c> returns 404 rather than <c>FEATURE_DISABLED</c>, because telling an
/// anonymous caller "this link is real but the account is downgraded" leaks the existence of the
/// trip and a fact about the customer's billing (acceptance 9).
/// </para>
/// </summary>
public sealed class PublicTripResolver(
    IPublicLinkGrantClient publicLinkGrantClient,
    ITripShareReader tripShareReader,
    IAccountFeatureReader accountFeatureReader) : IPublicTripResolver
{
    public async Task<PublicTripResultVm> ResolveAsync(
        Guid publicLinkGrantId,
        Guid accountId,
        string resourceId,
        string token,
        CancellationToken cancellationToken)
    {
        // Manager owns hashing, access counting and the PublicLinkAccessed audit event, in one
        // implementation shared with its own anonymous REST endpoint (spec 11 §18.10).
        var resolution = await publicLinkGrantClient.ResolveAsync(
            publicLinkGrantId, accountId, TripSharing.ResourceType, resourceId, TripSharing.TrackScope, token, cancellationToken);

        if (resolution.Resolution != PublicTripResolution.Found)
            return new PublicTripResultVm(resolution.Resolution, null);

        var enabled = await accountFeatureReader.IsFeatureEnabledAsync(accountId, FeatureKeys.TripManagement, cancellationToken);
        if (!enabled)
            return new PublicTripResultVm(PublicTripResolution.NotFound, null);

        var snapshot = await tripShareReader.GetPublicSnapshotAsync(publicLinkGrantId, accountId, cancellationToken);
        return snapshot is null
            ? new PublicTripResultVm(PublicTripResolution.NotFound, null)
            : new PublicTripResultVm(PublicTripResolution.Found, snapshot);
    }
}
