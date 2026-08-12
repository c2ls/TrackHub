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

namespace TrackHub.TripManagement.Domain.Interfaces;

public interface ITripShareReader
{
    /// <summary>
    /// Projects the public snapshot for a resolved grant, honouring the share's field flags.
    /// Live position is included only when <c>IncludeLivePosition</c> is set AND the trip is
    /// still <c>InProgress</c> (acceptance 23).
    /// </summary>
    Task<PublicTripVm?> GetPublicSnapshotAsync(Guid publicLinkGrantId, Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    /// The trip a share belongs to, within the account, or <c>null</c> when no such share exists.
    /// <para>
    /// Deliberately account-scoped ONLY — it answers "which trip is this share on", not "may the
    /// caller see it". Group visibility is applied by the caller against the resolved trip
    /// (<c>TripVisibility.ResolveVisibleTripByShareAsync</c>), so this module keeps ONE
    /// implementation of the group predicate (acceptance 4) instead of growing a second one here.
    /// </para>
    /// </summary>
    Task<Guid?> FindTripIdByShareAsync(Guid tripShareId, Guid accountId, CancellationToken cancellationToken);
}
